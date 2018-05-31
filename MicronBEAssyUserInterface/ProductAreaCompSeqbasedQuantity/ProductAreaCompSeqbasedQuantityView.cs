using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.Projects;

using MicronBEAssy;
using MicronBEAssy.Inputs;
using DevExpress.XtraPivotGrid;
using MicronBEAssyUserInterface.Class;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using MicronBEAssy.Outputs;
using Mozart.Text;

namespace MicronBEAssyUserInterface.ProductAreaCompSeqbasedQuantity
{
    public partial class ProductAreaCompSeqbasedQuantityView : XtraUserControlView
    {
        IExperimentResultItem _result;
        ResultDataContext _resultDataContext = null;
        Dictionary<Tuple<string, string, bool, bool, int>, UIProduct> _prodInfos = new Dictionary<Tuple<string, string, bool, bool, int>, UIProduct>();
        Dictionary<Tuple<string, string>, UIProcess> _processList = new Dictionary<Tuple<string, string>, UIProcess>();
        Dictionary<Tuple<string, string>, UIProductDetail> _productDetailList = new Dictionary<Tuple<string, string>, UIProductDetail>();

        public ProductAreaCompSeqbasedQuantityView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                _result = this.Document.GetResultItem();
                _resultDataContext = this.Document.GetResultItem().GetCtx<ResultDataContext>();
            }

            InitializeControl();
        }

        private void InitializeControl()
        {
            if (this._result == null)
                return;

            LoadData();

            SetComboBox();

            //SetPivotGrid(dt, this.pivotGridControl);
        }

        private void LoadData()
        {
            _prodInfos = new Dictionary<Tuple<string,string,bool,bool,int>,UIProduct>(DataHelper.LoadProductRoute(this._resultDataContext));
            _processList = new Dictionary<Tuple<string, string>, UIProcess>(DataHelper.LoadProcessStep(this._resultDataContext));
            _productDetailList = new Dictionary<Tuple<string, string>, UIProductDetail>(DataHelper.LoadProductDetail(this._resultDataContext, _processList));
        }

        private void SetPivotGrid(DataTable dt, DevExpress.XtraPivotGrid.PivotGridControl pivotGrid)
        {
            XtraPivotGridHelper.DataViewTable dataTable = new XtraPivotGridHelper.DataViewTable();

            foreach (DataColumn info in dt.Columns)
                dataTable.AddColumn(info.ColumnName, info.ColumnName, info.DataType, PivotArea.RowArea, null, null);

            pivotGrid.BeginUpdate();
            pivotGrid.ClearPivotGridFields();
            pivotGrid.CreatePivotGridFields(dataTable);
            pivotGrid.DataSource = dt;
            pivotGrid.EndUpdate();

            pivotGrid.BestFit();
        }

        private DataTable CreateDataTable(Dictionary<Tuple<string, string>, ResultData> resultDatas)
        {
            DataTable dt = new DataTable("RESULT");

            DataColumn colProductID = new DataColumn("PRODUCT_ID", typeof(string));
            DataColumn colProductName = new DataColumn("PRODUCT_NAME", typeof(string));
            DataColumn colDemandQty = new DataColumn("DEMAND(P)", typeof(decimal));
            DataColumn colActQty = new DataColumn("ACT(P)", typeof(decimal));
            DataColumn colRemaninQty = new DataColumn("REMAIN(P)", typeof(decimal));
            DataColumn colProcess = new DataColumn("B/N_STEPS", typeof(string));
            DataColumn colSequence = new DataColumn("COMP_SEQ", typeof(int));
            DataColumn colDieAttach = new DataColumn("DIE_ATTACH", typeof(string));
            DataColumn colInProductName = new DataColumn("ASSY_IN_PROD_NAME", typeof(string));
            DataColumn colCompQty = new DataColumn("COMP_QTY", typeof(int));
            DataColumn colEPEWipQty = new DataColumn("EPE(P)", typeof(decimal));
            DataColumn colEPEInQty = new DataColumn("NEED_EPE(P)", typeof(decimal));
            DataColumn colCR2WipQty = new DataColumn("CR2(C)", typeof(decimal));
            DataColumn colCR1WipQty = new DataColumn("CR1(C)", typeof(decimal));
            DataColumn colStockWipQty = new DataColumn("STOCK(C)", typeof(decimal));
            DataColumn colInQty = new DataColumn("NEED_STOCK(C)", typeof(decimal));
            DataColumn colCutOff = new DataColumn("RELEASE_CUT_OFF", typeof(string));

            DataColumn colFabProductNames = new DataColumn("FAB_PRODUCT_NAMES", typeof(string));
            DataColumn colNoKitQty = new DataColumn("NO_KIT_INV(C)", typeof(decimal));
            DataColumn colNeedWaferQty = new DataColumn("NEED_WAFER(C)", typeof(decimal));

            dt.Columns.Add(colProductID);
            dt.Columns.Add(colProductName);
            dt.Columns.Add(colDemandQty);
            dt.Columns.Add(colActQty);
            dt.Columns.Add(colRemaninQty);
            dt.Columns.Add(colProcess);

            dt.Columns.Add(colSequence);
            dt.Columns.Add(colDieAttach);
            dt.Columns.Add(colInProductName);
            dt.Columns.Add(colCompQty);
            dt.Columns.Add(colEPEWipQty);
            dt.Columns.Add(colEPEInQty);
            dt.Columns.Add(colCR2WipQty);
            dt.Columns.Add(colCR1WipQty);
            dt.Columns.Add(colStockWipQty);
            dt.Columns.Add(colInQty);
            dt.Columns.Add(colCutOff);

            dt.Columns.Add(colFabProductNames);
            dt.Columns.Add(colNoKitQty);
            dt.Columns.Add(colNeedWaferQty);

            foreach (ResultData result in resultDatas.Values)
            {
#if DEBUG
                if(result.ProductID == "144868")
                    Console.WriteLine();
#endif

                List<ResultDataAssyPart> assyResultList = new List<ResultDataAssyPart>(result.AssyInResultData.Values);
                assyResultList.Sort((x, y) => x.Product.CompSeq.CompareTo(y.Product.CompSeq));

                if (assyResultList.Count > 0)
                {
                    foreach (ResultDataAssyPart rdap in assyResultList)
                    {
                        DataRow row = dt.NewRow();

                        row[colProductID] = result.ProductID;
                        row[colProductName] = result.ProductName;
                        row[colDemandQty] = result.DemandQty;
                        row[colActQty] = result.ActQty;
                        row[colRemaninQty] = result.RemainQty;
                        row[colProcess] = result.Process;

                        row[colSequence] = rdap.Product.CompSeq;
                        row[colDieAttach] = rdap.DAStep != null ? rdap.DAStep.StepID : string.Empty;

                        if(rdap.Product.ProductDetail != null)
                            row[colInProductName] = rdap.Product.ProductDetail.ProductName;

                        row[colCompQty] = rdap.Product.CompQty;
                        row[colEPEWipQty] = rdap.EPEWipQty;

                        row[colEPEInQty] = rdap.EPEInQty;
                        row[colCR2WipQty] = rdap.CR2WipQty;
                        row[colCR1WipQty] = rdap.CR1WipQty;
                        row[colStockWipQty] = rdap.StockWipQty;
                        row[colInQty] = rdap.InQty;
                        if (rdap.CutOffDate != DateTime.MinValue)
                            row[colCutOff] = DateUtility.DbToString(rdap.CutOffDate);

                        row[colFabProductNames] = rdap.WaferProductNames;
                        row[colNoKitQty] = rdap.NoKitQty;
                        row[colNeedWaferQty] = rdap.NeedWaferQty;

                        dt.Rows.Add(row);
                    }
                }
                else
                {
                    DataRow row = dt.NewRow();

                    row[colProductID] = result.ProductID;
                    row[colProductName] = result.ProductName;
                    row[colDemandQty] = result.DemandQty;
                    row[colActQty] = result.ActQty;
                    row[colRemaninQty] = result.RemainQty;
                    row[colProcess] = result.Process;

                    dt.Rows.Add(row);
                }
            }

            return dt;
        }

        private UIProduct FindProduct(string lineID, string productID, bool isMcpPart = false, bool isMidPart = false, int seq = 1)
        {
            Tuple<string, string, bool, bool, int> key = new Tuple<string, string, bool, bool, int>(lineID, productID, isMcpPart, isMidPart, seq);

            UIProduct product;
            _prodInfos.TryGetValue(key, out product);

            return product;
        }

        private UIProductDetail FindProductDetail(string lineID, string productID)
        {
            Tuple<string, string> key = new Tuple<string, string>(lineID, productID);

            UIProductDetail productDetail;
            this._productDetailList.TryGetValue(key, out productDetail);

            return productDetail;
        }

        private Dictionary<Tuple<string, string>, ResultData> SetData()
        {
            Dictionary<Tuple<string, string>, ResultData> resultDatas = new Dictionary<Tuple<string, string>, ResultData>();

            CollectionResultData(resultDatas);

            CollectActInfo(resultDatas);

            CollectPegWip(resultDatas);

            CollectUnpegWip(resultDatas);

            return resultDatas;
        }

        private void CollectUnpegWip(Dictionary<Tuple<string, string>, ResultData> resultDatas)
        {
            string excess = "EXCESS";
            string lotsReceived = "LOTS RECEIVED";
            string dieAttach = "DIE ATTACH";
            string run = "RUN";
            foreach (UnPegHistory info in _resultDataContext.UnPegHistory)
            {
                if (info.REASON != excess)
                    continue;

#if DEBUG
                if(info.PRODUCT_ID == "284231")
                    Console.WriteLine();
#endif

                UIProductDetail productDetail = FindProductDetail(info.LINE_ID, info.PRODUCT_ID);
                UIStep step = productDetail.Process.FindStep(info.STEP_ID);
                UIProduct product = FindProduct(info.LINE_ID, info.PRODUCT_ID);

                UIProduct wipProduct = null;

                int daThroughCount = step.DaThroughCount;

                if (step.StepGroup == dieAttach && info.LOT_STATE != run)
                    daThroughCount = daThroughCount - 1;

                if (product != null)
                {
                    if (product.StepID == lotsReceived)
                        wipProduct = product;
                    else if (product.MaxSequence == daThroughCount)
                        wipProduct = product;
                    else if (daThroughCount > 0)
                        wipProduct = FindProduct(info.LINE_ID, info.PRODUCT_ID, true, true, daThroughCount);
                    else
                        wipProduct = FindProduct(info.LINE_ID, info.PRODUCT_ID, true, false, 1);
                }
                else
                {
                    foreach (UIProduct prod in _prodInfos.Values)
                    {
                        if (prod.ProductID == info.PRODUCT_ID && prod.LineID == info.LINE_ID)
                        {
                            wipProduct = prod;
                            break;
                        }
                    }
                }

                if (wipProduct == null)
                    continue;

#if DEBUG
                if(wipProduct == null)
                    Console.WriteLine();
#endif

                if (wipProduct.IsWaferPart)
                {
                    foreach (ResultData result in resultDatas.Values)
                    {
                        foreach (ResultDataAssyPart rdap in result.AssyInResultData.Values)
                        {
                            if (wipProduct.Nexts.Contains(rdap.Product))
                            {
                                rdap.NoKitQty += info.UNPEG_QTY;
                            }
                        }
                    }
                }
                else if (wipProduct.IsMcpPart == false)
                {
                    Tuple<string, string> key = new Tuple<string, string>(wipProduct.LineID, wipProduct.ProductID);

                    ResultData result;
                    if (resultDatas.TryGetValue(key, out result))
                    {
                        foreach (UIProduct inProd in result.Product.AssyParts)
                        {
                            ResultDataAssyPart rdap;
                            if (result.AssyInResultData.TryGetValue(inProd.CompSeq, out rdap) == false)
                            {
                                rdap = new ResultDataAssyPart(result, inProd);
                                result.AssyInResultData.Add(inProd.CompSeq, rdap);
                            }

                            if (step.Categroy == UIStepCategory.EPE)
                            {
                                rdap.EPEWipQty += info.UNPEG_QTY;
                            }
                            else if (step.Categroy == UIStepCategory.CR2)
                            {
                                rdap.CR2WipQty += info.UNPEG_QTY * rdap.Product.CompQty;
                            }
                            else
                            {
                                //이런 재공 있으면 이상 Data
                                Console.WriteLine();
                            }
                        }
                    }
                }
                else if (wipProduct.IsMidPart)
                {
                    Tuple<string, string> key = new Tuple<string, string>(wipProduct.LineID, wipProduct.ProductID);

                    ResultData result;
                    if (resultDatas.TryGetValue(key, out result))
                    {
                        foreach (UIProduct inProd in result.Product.AssyParts)
                        {
                            ResultDataAssyPart rdap;
                            if (result.AssyInResultData.TryGetValue(inProd.CompSeq, out rdap) == false)
                            {
                                rdap = new ResultDataAssyPart(result, inProd);
                                result.AssyInResultData.Add(inProd.CompSeq, rdap);
                            }

                            if (inProd.CompSeq <= wipProduct.CompSeq)
                            {
                                if (step.Categroy == UIStepCategory.EPE)
                                {
                                    //MidPart인데 EPE재공으로 집계될 수 없음. 확인후 조치필요
                                    Console.WriteLine();
                                }
                                else if (step.Categroy == UIStepCategory.CR2)
                                {
                                    rdap.CR2WipQty += info.UNPEG_QTY * rdap.Product.CompQty;
                                }
                                else
                                {
                                    //이런 재공 있으면 이상 Data
                                    Console.WriteLine();
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (UIProduct prod in wipProduct.FinalProducts)
                    {
                        Tuple<string, string> key = new Tuple<string, string>(prod.LineID, prod.ProductID);
                        ResultData result;
                        if (resultDatas.TryGetValue(key, out result))
                        {
                            foreach (ResultDataAssyPart rdap in result.AssyInResultData.Values)
                            {
                                if (rdap.Product.ProductID != wipProduct.ProductID)
                                    continue;

                                if (step.Categroy == UIStepCategory.EPE)
                                {
                                    //이상 Data - 확인후 조치필요
                                    Console.WriteLine();
                                }
                                else if (step.Categroy == UIStepCategory.CR2)
                                {
                                    rdap.CR2WipQty += info.UNPEG_QTY;
                                }
                                else if (step.Categroy == UIStepCategory.CR1)
                                {
                                    rdap.CR1WipQty += info.UNPEG_QTY;
                                }
                                else if (step.Categroy == UIStepCategory.STOCK && wipProduct.IsWaferPart == false)
                                {
                                    rdap.StockWipQty += info.UNPEG_QTY;
                                }
                                else
                                {
                                    rdap.NoKitQty += info.UNPEG_QTY;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CollectionResultData(Dictionary<Tuple<string, string>, ResultData> resultDatas)
        {
            string all = " (ALL)";
            Dictionary<Tuple<string, string, string>, DateTime> cutOffInfos = new Dictionary<Tuple<string, string, string>, DateTime>();

            HashSet<string> lineList = new HashSet<string>();
            string planView = string.Empty;
            HashSet<string> designIDList = new HashSet<string>();
            HashSet<string> productList = new HashSet<string>();
            string queryProductID = this.textBoxProductID.Text;
            string queryProductName = this.textBoxProductName.Text;

            foreach (object line in this.checkedComboBoxLineID.CheckedItems)
                lineList.Add(line.ToString());

            planView = this.comboBoxPlanView.SelectedItem.ToString();
            
            foreach (object design in this.checkedComboBoxDesignID.CheckedItems)
                designIDList.Add(design.ToString());

            
            foreach (UIProduct product in _prodInfos.Values)
            {
                if (product.ProductDetail == null)
                    continue;

                if (designIDList.Contains(product.ProductDetail.DesignID) == false)
                    continue;

                if (string.IsNullOrEmpty(queryProductID) == false)
                {
                    if (LikeUtility.Like(product.ProductID, queryProductID) == false)
                        continue;
                }

                if (string.IsNullOrEmpty(queryProductName) == false)
                {
                    if (LikeUtility.Like(product.ProductDetail.ProductName, queryProductName) == false)
                        continue;
                }

                productList.Add(product.ProductID);
            }

            foreach (StepTarget target in _resultDataContext.StepTarget)
            {
                if (planView != all && planView.CompareTo(target.WEEK_NO) < 0)
                    continue;

                if (target.SEQUENCE > 1)
                    continue;

                Tuple<string, string, string> targetKey = new Tuple<string, string, string>(target.LINE_ID, target.MO_PRODUCT_ID, target.PRODUCT_ID);

                if (cutOffInfos.ContainsKey(targetKey) == false)
                    cutOffInfos.Add(targetKey, target.TARGET_DATE);
                else
                {
                    if (cutOffInfos[targetKey] < target.TARGET_DATE)
                        cutOffInfos[targetKey] = target.TARGET_DATE;
                }
            }

            foreach (Demand demand in _resultDataContext.ModelContext.Demand)
            {
#if DEBUG
                if(demand.PRODUCT_ID == "319346")
                    Console.WriteLine();
#endif
                if (lineList.Contains(demand.LINE_ID) == false)
                    continue;

                if (planView != all && planView.CompareTo(demand.WEEK_NO) < 0)
                    continue;

                if (productList.Contains(demand.PRODUCT_ID) == false)
                    continue;

                Tuple<string, string> key = new Tuple<string, string>(demand.LINE_ID, demand.PRODUCT_ID);

                UIProductDetail productDetail = FindProductDetail(demand.LINE_ID, demand.PRODUCT_ID);

                ResultData resultData;
                if (resultDatas.TryGetValue(key, out resultData) == false)
                {
                    resultData = new ResultData();
                    resultData.Product = FindProduct(demand.LINE_ID, demand.PRODUCT_ID);
                    resultData.ProductID = demand.PRODUCT_ID;
                    resultData.LineID = demand.LINE_ID;
                    if(productDetail != null)
                        resultData.ProductName = productDetail.ProductName;

                    if (resultData.Product != null)
                    {
                        resultData.Process = resultData.Product.Process != null ? resultData.Product.Process.DAWBProcess : string.Empty;

                        foreach (UIProduct inProd in resultData.Product.AssyParts)
                        {
                            if (resultData.AssyInResultData.ContainsKey(inProd.CompSeq) == false)
                            {
                                ResultDataAssyPart rdap = new ResultDataAssyPart(resultData, inProd);

                                Tuple<string, string, string> cutOffKey = new Tuple<string, string, string>(resultData.LineID, resultData.ProductID, rdap.Product.ProductID);

                                if (cutOffInfos.ContainsKey(cutOffKey))
                                    rdap.CutOffDate = cutOffInfos[cutOffKey];

                                resultData.AssyInResultData.Add(inProd.CompSeq, rdap);
                            }
                        }
                    }

                    resultDatas.Add(key, resultData);
                }

                resultData.DemandQty += demand.QTY;
            }
        }

        private void CollectPegWip(Dictionary<Tuple<string, string>, ResultData> resultDatas)
        {
            string lotsReceived = "LOTS RECEIVED";
            string run = "RUN";
            string dieAttach = "DIE ATTACH";

            foreach (PegHistory info in this._resultDataContext.PegHistory)
            {
                UIProductDetail productDetail = FindProductDetail(info.LINE_ID, info.PRODUCT_ID);
                UIStep step = productDetail.Process.FindStep(info.STEP_ID);
                UIProduct moProduct = FindProduct(info.LINE_ID, info.MO_PRODUCT_ID);
                UIProduct product = FindProduct(info.LINE_ID, info.PRODUCT_ID);

                UIProduct wipProduct = null;

                int daThroughCount = step.DaThroughCount;

                if (step.StepGroup == dieAttach && info.LOT_STATE != run)
                    daThroughCount = daThroughCount - 1;

                if (product != null)
                {
                    if (product.StepID == lotsReceived)
                        wipProduct = product;
                    else if (product.MaxSequence == daThroughCount)
                        wipProduct = product;
                    else if(daThroughCount > 0)
                        wipProduct = FindProduct(info.LINE_ID, info.PRODUCT_ID, true, true, daThroughCount);
                    else
                        wipProduct = FindProduct(info.LINE_ID, info.PRODUCT_ID, true, false, 1);
                }
                else
                {
                    wipProduct = FindProduct(info.LINE_ID, info.PRODUCT_ID, true, false, info.COMP_SEQ);
                }

#if DEBUG
                if(wipProduct == null)
                    Console.WriteLine();

                if (info.MO_PRODUCT_ID == "227128")
                    Console.WriteLine();
#endif

                Tuple<string, string> resultKey = new Tuple<string, string>(info.LINE_ID, info.MO_PRODUCT_ID);
                ResultData resultData;
                if (resultDatas.TryGetValue(resultKey, out resultData))
                {
                    if (moProduct == wipProduct)
                    {

                        foreach (UIProduct inProd in wipProduct.AssyParts)
                        {
                            ResultDataAssyPart rdap;
                            if (resultData.AssyInResultData.TryGetValue(inProd.CompSeq, out rdap) == false)
                            {
                                rdap = new ResultDataAssyPart(resultData, inProd);
                                resultData.AssyInResultData.Add(inProd.CompSeq, rdap);
                            }

                            if (step.Categroy == UIStepCategory.EPE)
                            {
                                rdap.EPEWipQty += info.PEG_QTY;
                            }
                            else if (step.Categroy == UIStepCategory.CR2)
                            {
                                rdap.CR2WipQty += info.PEG_QTY * rdap.Product.CompQty;
                            }
                            else
                            {
                                //이런 재공 있으면 이상 Data
                                Console.WriteLine();
                            }
                        }
                    }
                    else if (wipProduct.IsMidPart)
                    {
                        foreach (UIProduct inProd in moProduct.AssyParts)
                        {
                            ResultDataAssyPart rdap;
                            if (resultData.AssyInResultData.TryGetValue(inProd.CompSeq, out rdap) == false)
                            {
                                rdap = new ResultDataAssyPart(resultData, inProd);
                                resultData.AssyInResultData.Add(inProd.CompSeq, rdap);
                            }

                            if (inProd.CompSeq <= wipProduct.CompSeq)
                            {
                                if (step.Categroy == UIStepCategory.EPE)
                                {
                                    //MidPart인데 EPE재공으로 집계될 수 없음. 확인후 조치필요
                                    Console.WriteLine();
                                }
                                else if (step.Categroy == UIStepCategory.CR2)
                                {
                                    rdap.CR2WipQty += info.PEG_QTY * rdap.Product.CompQty;
                                }
                                else
                                {
                                    //이런 재공 있으면 이상 Data
                                    Console.WriteLine();
                                }
                            }
                        }
                    }
                    else
                    {
                        ResultDataAssyPart rdap;
                        if (resultData.AssyInResultData.TryGetValue(wipProduct.CompSeq, out rdap) == false)
                        {
                            rdap = new ResultDataAssyPart(resultData, wipProduct);
                            resultData.AssyInResultData.Add(wipProduct.CompSeq, rdap);
                        }

                        if (step.Categroy == UIStepCategory.EPE)
                        {
                            //이상 Data - 확인후 조치필요
                            Console.WriteLine();
                        }
                        else if (step.Categroy == UIStepCategory.CR2)
                        {
                            rdap.CR2WipQty += info.PEG_QTY;
                        }
                        else if (step.Categroy == UIStepCategory.CR1)
                        {
                            rdap.CR1WipQty += info.PEG_QTY;
                        }
                        else if (step.Categroy == UIStepCategory.STOCK && wipProduct.IsWaferPart == false)
                        {
                            rdap.StockWipQty += info.PEG_QTY;
                        }
                        else
                        {
                            rdap.NoKitQty += info.PEG_QTY;
                        }
                    }
                }
            }
        }

        private void CollectActInfo(Dictionary<Tuple<string, string>, ResultData> resultDatas)
        {
            Dictionary<Tuple<string, string>, decimal> actInfos = new Dictionary<Tuple<string, string>, decimal>();
            foreach (ActInfo actInfo in _resultDataContext.ModelContext.ActInfo)
            {
                Tuple<string, string> key = new Tuple<string, string>(actInfo.LINE_ID, actInfo.PRODUCT_ID);

                if (actInfos.ContainsKey(key) == false)
                    actInfos.Add(key, 0);

                actInfos[key] += actInfo.ACT_QTY;
            }

            foreach (KeyValuePair<Tuple<string, string>, ResultData> result in resultDatas)
            {
                if (actInfos.ContainsKey(result.Key))
                {
                    result.Value.ActQty = actInfos[result.Key];
                }
            }
        }

        private void SetComboBox()
        {
            HashSet<string> lineIDList = new HashSet<string>();
            HashSet<string> designIDList = new HashSet<string>();
            HashSet<string> planViews = new HashSet<string>();

            foreach (ProductMaster productMaster in _resultDataContext.ModelContext.ProductMaster)
            {
                lineIDList.Add(productMaster.LINE_ID);
                designIDList.Add(productMaster.DESIGN_ID);
            }

            foreach (Demand demand in _resultDataContext.ModelContext.Demand)
            {
                if (string.IsNullOrEmpty(demand.WEEK_NO))
                    continue;

                planViews.Add(demand.WEEK_NO);
            }

            foreach (string lineID in lineIDList)
                this.checkedComboBoxLineID.Items.Add(lineID, true);

            foreach (string designID in designIDList)
                this.checkedComboBoxDesignID.Items.Add(designID, true);

            List<string> planViewList = new List<string>();
            planViewList.AddRange(planViews);

            this.comboBoxPlanView.Items.Add(" (ALL)");
            planViewList.Sort((x, y) => x.CompareTo(y));
            foreach (string planView in planViewList)
                this.comboBoxPlanView.Items.Add(planView);

            this.checkedComboBoxLineID.Select();
            this.checkedComboBoxDesignID.Select();
            this.comboBoxPlanView.SelectedIndex = 0;
        }

        private void buttonQuery_Click(object sender, EventArgs e)
        {
            this.panel1.Controls.Clear();

            GridControl grid = new GridControl();
            grid.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(grid);

            Dictionary<Tuple<string, string>, ResultData> resultDatas = SetData();

            DataTable dt = CreateDataTable(resultDatas);

            grid.DataSource = dt;
            (grid.MainView as GridView).OptionsView.ColumnAutoWidth = false;
            (grid.MainView as GridView).BestFitColumns();
            (grid.MainView as GridView).OptionsView.ShowAutoFilterRow = true;
        }

        private void textBoxProductID_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxProductID.Text.Count() == 0)
                return;

            this.textBoxProductName.Clear();
        }

        private void textBoxProductName_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxProductName.Text.Count() == 0)
                return;

            this.textBoxProductID.Clear();
        }
    }

    public class ResultData
    {
        public UIProduct Product { get; set; }
        public string LineID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal DemandQty { get; set; }
        public decimal ActQty { get; set; }
        public string Process { get; set; }

        public Dictionary<int, ResultDataAssyPart> AssyInResultData { get; set; }

        public decimal RemainQty
        {
            get
            {
                return DemandQty - ActQty;
            }
        }

        public ResultData()
        {
            AssyInResultData = new Dictionary<int, ResultDataAssyPart>();
        }
    }

    public class ResultDataAssyPart
    {
        public ResultData ResultData { get; private set; }
        public UIProduct Product{get; private set;}
        public UIStep DAStep { get; set; }
        public DateTime CutOffDate { get; set; }
        public decimal EPEWipQty { get; set; }
        public decimal EPEInQty
        {
            get
            {
                return (ResultData.RemainQty - EPEWipQty) * Product.CompQty;
            }
        }

        public decimal CR2WipQty { get; set; }
        public decimal CR1WipQty { get; set; }
        public decimal StockWipQty { get; set; }
        public decimal InQty
        {
            get
            {
                return EPEInQty - CR2WipQty - CR1WipQty - StockWipQty;
            }
        }

        public decimal NoKitQty { get; set; }
        public decimal NeedWaferQty
        {
            get
            {
                return InQty - NoKitQty;
            }
        }

        public string WaferProductNames { get; private set; }

        public ResultDataAssyPart(ResultData resultData, UIProduct product)
        {
            this.ResultData = resultData;
            this.Product = product;

            if (resultData.Product != null && resultData.Product.Process != null)
                this.DAStep = resultData.Product.Process.FindDaStep(product.CompSeq);

            if (product.Prevs.Count > 0)
            {
                for(int i = 0; i < product.Prevs.Count; i++)
                {
                    UIProduct prev = product.Prevs[i];

                    WaferProductNames += prev.ProductDetail.ProductName;

                    if(i + 1 < product.Prevs.Count)
                        WaferProductNames += ", ";
                }
            }
        }
    }
}
