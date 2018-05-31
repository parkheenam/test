using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicronBEAssy;
using MicronBEAssyUserInterface.Class;
using MicronBEAssy.Inputs;

namespace MicronBEAssyUserInterface
{
    public static class DataHelper
    {
        static ResultDataContext _resultDataContext = null;

        /// <summary>
        /// Key : LINE_ID, PRODUCT_ID, IsMcpPart, IsMidPart, McpSeq
        /// </summary>
        private static Dictionary<Tuple<string, string, bool, bool, int>, UIProduct> ProductInfos = new Dictionary<Tuple<string, string, bool, bool, int>, UIProduct>();
        private static Dictionary<Tuple<string, string>, UIProductDetail> ProductDetailList = new Dictionary<Tuple<string, string>, UIProductDetail>();

        internal static Dictionary<Tuple<string, string, bool, bool, int>, UIProduct> LoadProductRoute(MicronBEAssy.ResultDataContext resultDataContext)
        {
            _resultDataContext = resultDataContext;
            ProductInfos.Clear();

            ProductDetailList.Clear();

            Dictionary<Tuple<string, string>, UIProcess> processList = LoadProcessStep(resultDataContext);

            ProductDetailList = LoadProductDetail(resultDataContext, processList);

            CollectProductRouteData();

            CollectBinSplitInfoData();

            CollectAltProdInfoData();

            //
            return ProductInfos;
        }

        public static Dictionary<Tuple<string, string>, UIProductDetail> LoadProductDetail(ResultDataContext resultDataContext, Dictionary<Tuple<string, string>, UIProcess> processList, bool isProductName = false)
        {
            Dictionary<Tuple<string, string>, UIProductDetail> productDetailList = new Dictionary<Tuple<string,string>,UIProductDetail>();
            foreach(ProductMaster info in resultDataContext.ModelContext.ProductMaster)
            {
                Tuple<string, string> key = null;

                if (isProductName == false)
                    key = new Tuple<string, string>(info.LINE_ID, info.PRODUCT_ID);
                else
                    key = new Tuple<string, string>(info.LINE_ID, info.PRODUCT_NAME);

                UIProcess process = FindProcess(info.LINE_ID, info.PROCESS_ID, processList);

                UIProductDetail productDetail;
                if (productDetailList.TryGetValue(key, out productDetail) == false)
                {
                    productDetail = new UIProductDetail(info, process);
                    productDetailList.Add(key, productDetail);
                }
            }

            return productDetailList;
        }

        public static UIProcess FindProcess(string lineID, string processID, Dictionary<Tuple<string, string>, UIProcess> processList)
        {
            Tuple<string, string> key = new Tuple<string, string>(lineID, processID);
            UIProcess process;
            processList.TryGetValue(key, out process);

            return process;
        }

        public static UIProductDetail FindProductDetail(string lineID, string productID)
        {
            UIProductDetail productDetail;
            ProductDetailList.TryGetValue(new Tuple<string, string>(lineID, productID), out productDetail);

            return productDetail;
        }

        private static void CollectAltProdInfoData()
        {
            foreach (var info in _resultDataContext.ModelContext.AltProductInfo)
            {
                UIProduct orgProd = FindProduct(info.LINE_ID, info.ORG_PROD_ID);
                if (orgProd == null)
                    continue;

                UIProduct altProd = FindProduct(info.LINE_ID, info.ALT_PROD_ID);
                if (altProd == null)
                    continue;

                if (orgProd.AltProductInfos.ContainsKey(info.PRIORITY) == false)
                    orgProd.AltProductInfos.Add(info.PRIORITY, altProd);
            }
        }

        private static void CollectBinSplitInfoData()
        {
            foreach (var info in _resultDataContext.ModelContext.BinSplitInfo)
            {
                UIProduct fromProd = FindProduct(info.LINE_ID, info.FROM_PROD_ID);
                if (fromProd == null)
                    continue;

                UIProduct toProd = FindProduct(info.LINE_ID, info.TO_PROD_ID);
                if (toProd == null)
                    continue;

                UIBinSplitInfo binInfo = new UIBinSplitInfo(fromProd, toProd, info.PORTION, info.GRADE, info.BIN_RANK);

                if (fromProd.BinSplitInfos.ContainsKey(toProd) == false)
                    fromProd.BinSplitInfos.Add(toProd, binInfo);
            }
        }

        private static void CollectProductRouteData()
        {
            _resultDataContext.ModelContext.McpBom.OrderBy(x => x.COMP_SEQ);

            Dictionary<Tuple<string, string>, List<UIProduct>> assyInProdList = new Dictionary<Tuple<string, string>, List<UIProduct>>();

            foreach (var info in _resultDataContext.ModelContext.McpBom)
            {
                Tuple<string, string, bool, bool, int> prodKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.FINAL_PROD_ID, false, false, 1);

                UIProduct mcpProduct;
                if (ProductInfos.TryGetValue(prodKey, out mcpProduct) == false)
                {
                    mcpProduct = new UIProduct(info.LINE_ID, info.FINAL_PROD_ID, info.FINAL_PROD_NAME, false, false, 1);
                    
                    ProductInfos.Add(prodKey, mcpProduct);
                }

                if (mcpProduct.CompSeq < info.COMP_SEQ)
                    mcpProduct.CompSeq = info.COMP_SEQ;
            }

            List<UIProduct> assyList = new List<UIProduct>();

            foreach (var info in _resultDataContext.ModelContext.McpBom)
            {
                Tuple<string, string, bool, bool, int> mcpProductKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.FINAL_PROD_ID, false, false, 1);
                UIProduct mcpProduct;
                if (ProductInfos.TryGetValue(mcpProductKey, out mcpProduct))
                {
#if DEBUG
                    if (info.FINAL_PROD_ID == "318173")
                        Console.WriteLine();
#endif
                    Tuple<string, string, bool, bool, int> ainKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.ASSY_IN_PROD_ID, true, false, info.COMP_SEQ);

                    UIProduct ainProd;
                    if (ProductInfos.TryGetValue(ainKey, out ainProd) == false)
                    {
                        ainProd = new UIProduct(info.LINE_ID, info.ASSY_IN_PROD_ID, info.ASSY_IN_PROD_NAME, true, false, info.COMP_SEQ, info.COMP_QTY);
                        ainProd.StepID = info.STEP_ID;
                        
                        ProductInfos.Add(ainKey, ainProd);

                        Tuple<string, string> aKey = new Tuple<string, string>(info.LINE_ID, info.ASSY_IN_PROD_ID);
                        List<UIProduct> ainList;
                        if (assyInProdList.TryGetValue(aKey, out ainList) == false)
                            assyInProdList.Add(aKey, ainList = new List<UIProduct>());

                        ainList.Add(ainProd);
                    }

                    ainProd.FinalProducts.Add(mcpProduct);

                    if(mcpProduct.AssyParts.Contains(ainProd) == false)
                        mcpProduct.AssyParts.Add(ainProd);

                    UIProduct nextProd = null;
                    if (mcpProduct.CompSeq > info.COMP_SEQ)
                    {
                        Tuple<string, string, bool, bool, int> midKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.TO_PROD_ID, true, true, info.COMP_SEQ);
                        UIProduct midProd;
                        if (ProductInfos.TryGetValue(midKey, out midProd) == false)
                        {
                            midProd = new UIProduct(info.LINE_ID, info.TO_PROD_ID, info.TO_PROD_NAME, true, true, info.COMP_SEQ);
                            ProductInfos.Add(midKey, midProd);
                        }

                        midProd.Prevs.Add(ainProd);
                        ainProd.Nexts.Add(midProd);

                        nextProd = midProd;
                    }
                    else
                    {
                        mcpProduct.Prevs.Add(ainProd);
                        ainProd.Nexts.Add(mcpProduct);

                        nextProd = mcpProduct;
                    }

                    Tuple<string, string, bool, bool, int> prevMidKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.FROM_PROD_ID, true, true, info.COMP_SEQ - 1);

                    UIProduct prevMidProd = FindProduct(info.LINE_ID, info.FROM_PROD_ID, true, true, info.COMP_SEQ - 1);
                    if (prevMidProd != null)
                    {
                        prevMidProd.Nexts.Add(nextProd);
                        prevMidProd.StepID = info.STEP_ID;
                        nextProd.Prevs.Add(prevMidProd);
                    }
                }
            }

            foreach (var info in _resultDataContext.ModelContext.ProductRoute)
            {
                Tuple<string, string, bool, bool, int> fromKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.FROM_PROD_ID, false, false, 1);
                UIProduct fromProd;
                if (ProductInfos.TryGetValue(fromKey, out fromProd) == false)
                {
                    fromProd = new UIProduct(info.LINE_ID, info.FROM_PROD_ID, info.FROM_PROD_NAME, false, false, 1);
                    fromProd.IsWaferPart = true;

                    ProductInfos.Add(fromKey, fromProd);
                }

                fromProd.StepID = info.STEP_ID;

                List<UIProduct> ainList;
                if (assyInProdList.TryGetValue(new Tuple<string, string>(info.LINE_ID, info.TO_PROD_ID), out ainList))
                {
                    foreach (UIProduct ainProd in ainList)
                    {
                        fromProd.Nexts.Add(ainProd);
                        ainProd.Prevs.Add(fromProd);
                    }
                }
                else
                {
                    Tuple<string, string, bool, bool, int> toKey = new Tuple<string, string, bool, bool, int>(info.LINE_ID, info.TO_PROD_ID, false, false, 1);

                    UIProduct toProd;
                    if (ProductInfos.TryGetValue(toKey, out toProd) == false)
                    {
                        toProd = new UIProduct(info.LINE_ID, info.TO_PROD_ID, info.TO_PROD_NAME, false, false, 1);

                        ProductInfos.Add(toKey, toProd);
                    }

                    fromProd.Nexts.Add(toProd);
                    toProd.Prevs.Add(fromProd);
                }
            }
        }

        public static UIProduct FindProduct(string lineID, string productID, bool isMcp = false, bool isMid = false, int seq = 1)
        {
            Tuple<string, string, bool, bool, int> key = new Tuple<string, string, bool, bool, int>(lineID, productID, isMcp, isMid, seq);

            UIProduct prod;
            ProductInfos.TryGetValue(key, out prod);

            return prod;
        }

        public static Dictionary<Tuple<string, string>, UIProcess> LoadProcessStep(MicronBEAssy.ResultDataContext resultDataContext)
        {
            Dictionary<Tuple<string, string>, UIProcess> processList = new Dictionary<Tuple<string, string>, UIProcess>();
            foreach(ProcessStep info in resultDataContext.ModelContext.ProcessStep)
            {
                Tuple<string, string> key = new Tuple<string, string>(info.LINE_ID, info.PROCESS_ID);

                UIProcess process;
                if (processList.TryGetValue(key, out process) == false)
                {
                    process = new UIProcess(info.LINE_ID, info.PROCESS_ID);
                    processList.Add(key, process);
                }

                UIStep step = new UIStep(process, info.STEP_ID, info.SEQUENCE, info.STEP_GROUP);
                process.AddStep(step);
            }

            string dieBank = "DIE BANK";
            string dieAttach = "DIE ATTACH";
            string wireBond = "WIRE BOND";
            List<string> stockSteps = new List<string>
            {
                "LOTS RECEIVED",
                "WAFER STORAGE INV",
                "DIE BANK"
            };
            
            foreach (UIProcess process in processList.Values)
            {
#if DEBUG
                if (process.ProcessID == "(MSB ASSY) M60A-X8_VFBGA-63/120_2.0")
                    Console.WriteLine();
#endif

                List<UIStep> stepList = new List<UIStep>(process.Steps.Values);
                stepList.Sort((x, y) => x.Sequence.CompareTo(y.Sequence));

                UIStep dieBankStep = null;
                UIStep firstDAStep = null;
                UIStep lastCR2Step = null;
                int daCnt = 0;
                foreach (UIStep step in stepList)
                {
                    if (dieBankStep == null && step.StepID == dieBank)
                        dieBankStep = step;

                    if (firstDAStep == null && step.StepGroup == dieAttach)
                        firstDAStep = step;

                    if ((step.StepGroup == wireBond || step.StepGroup == dieAttach) && (lastCR2Step == null || lastCR2Step.Sequence < step.Sequence))
                        lastCR2Step = step;

                    if (step.StepID.Length > 0 && (step.StepGroup == dieAttach || step.StepGroup == wireBond))
                        process.DAWBProcess += step.StepID[0];

                    if (step.StepGroup == dieAttach)
                    {
                        daCnt++;
                        if(process.DaSteps.ContainsKey(daCnt) == false)
                            process.DaSteps.Add(daCnt, step);
                    }

                    step.DaThroughCount = daCnt;
                }

                foreach (UIStep step in stepList)
                {
                    if ((dieBankStep != null && step.Sequence <= dieBankStep.Sequence) || stockSteps.Contains(step.StepID))
                        step.Categroy = UIStepCategory.STOCK;
                    else if (firstDAStep == null || step.Sequence < firstDAStep.Sequence)
                        step.Categroy = UIStepCategory.CR1;
                    else if (lastCR2Step != null && step.Sequence <= lastCR2Step.Sequence)
                        step.Categroy = UIStepCategory.CR2;
                    else
                        step.Categroy = UIStepCategory.EPE;
                }
            }

            return processList;
        }
    }
}
