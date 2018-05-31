using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using Mozart.Text;
using MicronBEAssy.Inputs;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.SemiBE.DataModel;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class FindHelper
    {
        public static MicronBEAssyProcess FindProcess(string lineID, string processID)
        {
            try
            {
                Tuple<string, string> key = Tuple.Create(lineID, processID);
                MicronBEAssyProcess process;
                if (InputMart.Instance.MicronBEAssyProcess.TryGetValue(key, out process))
                {
                    return process;
                }

                return null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        public static StepTat FindTAT(string productID, string stepID, string lineID)
        {
            try
            {
                foreach (StepTat tat in InputMart.Instance.StepTat.DefaultView)
                {
                    if (tat.PRODUCT_ID != productID)
                        continue;

                    if (tat.STEP_ID != stepID)
                        continue;

                    if (tat.LINE_ID != lineID)
                        continue;

                    return tat;
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }

            return null;
        }

        public static double FindYield(string productID, string stepID, string lineID)
        {
            try
            {
                foreach (Yield yield in InputMart.Instance.Yield.DefaultView)
                {
                    if (yield.PRODUCT_ID != productID)
                        continue;

                    if (yield.STEP_ID != stepID)
                        continue;

                    if (yield.LINE_ID != lineID)
                        continue;

                    return (double)yield.YIELD;
                }

                return 1;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return 1;
            }
        }

        public static Product FindProduct(string lineID, string productID, bool isMcpPart = false, bool isMidPart = false, int compSeq = 1)
        {
            try
            {
                Tuple<string, string, bool, bool, int> key = new Tuple<string, string, bool, bool, int>(lineID, productID, isMcpPart, isMidPart, compSeq);

                Product product;
                if (InputMart.Instance.MicronBEProducts.TryGetValue(key, out product))
                    return product;

                return null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        public static Mozart.SeePlan.Simulation.EntityState FindLotState(string lotState, string holdCode, string eqpID)
        {
            //LOT_STATE = "RUNNING" 이고 EQP_ID 가 NULL이 아니면 RUN
            //위 조건이 아닌데 HOLD_CODE = "YES"이면 HOLD
            //그 외 WAIT
            try
            {
                if (eqpID != Constants.NULL && string.IsNullOrEmpty(eqpID) == false && lotState == Constants.RUNNING)
                {
                    return EntityState.RUN;
                }
                else if (holdCode == Constants.YES)
                    return EntityState.HOLD;

                return EntityState.WAIT;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(EntityState);
            }
        }

        public static DateTime GetEngineStartTime()
        {
            try
            {
                var context = ServiceLocator.Resolve<ModelContext>();
                return context.StartTime;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(DateTime);
            }
        }

        public static DateTime GetNowDT()
        {
            try
            {
                var factory = ServiceLocator.Resolve<AoFactory>();

                if (factory == null)
                    return GetEngineStartTime();

                DateTime now = factory.NowDT;

                return now;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(DateTime);
            }

        }

        public static MicronBEAssyEqp FindEquipment(string eqpID)
        {
            MicronBEAssyEqp eqp = null;
            InputMart.Instance.MicronBEAssyEqp.TryGetValue(eqpID, out eqp);

            return eqp;
        }

        internal static ProductDetail FindProductDetail(string lineID, string productID)
        {
            Tuple<string, string> key = new Tuple<string, string>(lineID, productID);
            
            ProductDetail productDetail;
            if (InputMart.Instance.ProductDetail.TryGetValue(key, out productDetail))
                return productDetail;

            return null;
        }

        internal static Product FindWipProduct(Wip entity, MicronBEAssyProcess process, MicronBEAssyBEStep step, Mozart.SeePlan.Simulation.EntityState state, out string unpegReason)
        {
            Product product = null;
            unpegReason = string.Empty;

#if DEBUG
            if(entity.PRODUCT_ID == "264153")
                Console.WriteLine();

            if(entity.PRODUCT_ID == "264157")
                Console.WriteLine();
#endif
            int daThroughtCnt = step.DaThroughCount;

            if (step.StepGroup == Constants.DieAttach && state != EntityState.RUN)
            {
                daThroughtCnt = daThroughtCnt - 1;//DA 대기중인 재공은 DA를 진행하지 않은 상태로 판단
            }

            Product wipProduct = FindHelper.FindProduct(entity.LINE_ID, entity.PRODUCT_ID);

            if (wipProduct is MicronBEAssyProduct)
                product = wipProduct;
            else if (wipProduct != null && wipProduct is AssyMcpProduct && daThroughtCnt >= 1)
            {
                AssyMcpProduct mcpProduct = wipProduct as AssyMcpProduct;

                if (daThroughtCnt == mcpProduct.MaxSequence)
                    product = mcpProduct;
                else
                {
                    AssyMcpPart midPart = FindHelper.FindProduct(entity.LINE_ID, entity.PRODUCT_ID, true, true, daThroughtCnt) as AssyMcpPart;

                    if (midPart != null)
                        product = midPart;

#if DEBUG
                    if(midPart == null)
                        Console.WriteLine();
#endif
                }
            }
            else
            {
                //임시로직 - 로직이 정해지먼 재정의 해야함.
                //초기화시 CompSeq가 확정된 Part를 찾을 수 없기 때문에 대표로 Part를 하나 찾아서 정의를 해야함.
                ICollection<AssyMcpPart> products = FindHelper.FindAssyInParts(entity.LINE_ID, entity.PRODUCT_ID);

                if (products != null && products.Count > 0)
                    product = products.ElementAt(0);
            }

            if (product == null)
            {
                if (step.StepID == Constants.LOTSRECEIVED)
                {
                    unpegReason = "Cannot find McpBom or ProductRoute";
                }
                else
                {
                    unpegReason = "Cannot find McpBom";
                }
            }

            return product;
        }

        public static ICollection<AssyMcpPart> FindAssyInParts(string lineID, string productID)
        {
            Tuple<string, string> key = new Tuple<string,string>(lineID, productID);
            ICollection<AssyMcpPart> products;
            if (InputMart.Instance.FindAssyInPartCache.TryGetValue(key, out products) == false)
            {
                foreach (Product beProduct in InputMart.Instance.MicronBEProducts.Values)
                {
                    if (beProduct is AssyMcpProduct)
                        continue;

                    if (beProduct is AssyMcpPart)
                    {
                        AssyMcpPart mcpPart = beProduct as AssyMcpPart;

                        if (mcpPart.IsMidPart)
                            continue;

                        if(mcpPart.LineID == lineID && mcpPart.ProductID == productID)
                            InputMart.Instance.FindAssyInPartCache.Add(key, mcpPart);
                    }
                }

                return InputMart.Instance.FindAssyInPartCache[key];
            }

            return products;
        }
    }
}
