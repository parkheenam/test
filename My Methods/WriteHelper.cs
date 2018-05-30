using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using MicronBEAssy.Outputs;
using Mozart.SeePlan;
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.Simulation;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class WriteHelper
    {
        public static void WriteStepTarget(Mozart.SeePlan.Pegging.PegPart pegPart, bool isOut)
        {
            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;

                foreach (MicronBEAssyBEPegTarget target in pegPart.PegTargetList)
                {
                    MicronBEAssyBEMoPlan moPlan = target.Mo as MicronBEAssyBEMoPlan;
                    MicronBEAssyBEMoMaster moMaster = moPlan.MoMaster as MicronBEAssyBEMoMaster;

                    StepTarget info = new StepTarget();

                    info.LINE_ID = pp.Product.LineID;
                    info.PRODUCT_ID = pp.Product.ProductID;
                    info.PROCESS_ID = pp.CurrentStep.RouteID;
                    info.STEP_ID = pp.CurrentStep.StepID;

                    if (isOut)
                        info.OUT_QTY = Convert.ToDecimal(target.Qty);
                    else
                        info.IN_QTY = Convert.ToDecimal(target.Qty);

                    info.TARGET_DATE = target.DueDate;
                    info.MO_PRODUCT_ID = moPlan.ProductID;
                    info.DESIGN_ID = pp.Product.DesignID();
                    info.DEMAND_ID = moPlan.DemandID;
                    info.WEEK_NO = moPlan.WeekNo;
                    info.SEQUENCE = (pp.CurrentStep as MicronBEAssyBEStep).Sequence;
                    info.IS_BASE = UtilityHelper.IsYN(pp.Product.IsBase());

                    OutputMart.Instance.StepTarget.Add(info);
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        public static void WritePeg(Mozart.SeePlan.Pegging.IMaterial m, Mozart.SeePlan.Pegging.PegTarget target, double qty)
        {
            try
            {
                MicronBEAssyPlanWip wip = m as MicronBEAssyPlanWip;
                MicronBEAssyBEPegPart pp = target.PegPart as MicronBEAssyBEPegPart;
                MicronBEAssyBEMoMaster mo = pp.MoMaster as MicronBEAssyBEMoMaster;
                MicronBEAssyBEMoPlan moPlan = target.MoPlan as MicronBEAssyBEMoPlan;

                PegHistory info = new PegHistory();

                info.LOT_ID = wip.LotID;
                info.PRODUCT_ID = wip.Product.ProductID;
                info.STEP_ID = string.IsNullOrEmpty(wip.MapStep.StepID) ? StringUtility.IdentityNull : wip.MapStep.StepID;
                info.MAIN_QTY = Convert.ToDecimal(wip.Wip.UnitQty);
                info.PEG_QTY = Convert.ToDecimal(qty);
                info.LOT_STATE = wip.Wip.CurrentState.ToString();
                info.LINE_ID = wip.Product.LineID;
                info.MO_PRODUCT_ID = mo.Product.ProductID;
                info.DESIGN_ID = wip.Product.DesignID();
                info.DEMAND_ID = moPlan.DemandID;
                info.WEEK_NO = moPlan.WeekNo;
                info.IS_BASE = UtilityHelper.IsYN(pp.Product.IsBase());

                if (pp.Product is AssyMcpPart)
                {
                    info.COMP_SEQ = (pp.Product as AssyMcpPart).CompSeq;  
                }
                else
                {
                    info.COMP_SEQ = 1;
                }
                

                OutputMart.Instance.PegHistory.Add(info);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        public static void WriteErrorHistory(ErrorLevel errorLevel, string reason)
        {
            ErrorHistory info = new ErrorHistory();

            info.LEVEL = errorLevel.ToString();
            info.REASON = reason;

            OutputMart.Instance.ErrorHistory.Add(info);
        }

        public static void WriteActPeg(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, double qty)
        {
            try
            {
                MicronBEAssyPlanWip wip = m as MicronBEAssyPlanWip;
                MicronBEAssyBEPegPart pp = target.PegPart as MicronBEAssyBEPegPart;
                MicronBEAssyBEMoMaster mo = pp.MoMaster as MicronBEAssyBEMoMaster;

                PegHistory info = new PegHistory();

                info.LOT_ID = LotType.ACT.ToString();
                info.LINE_ID = wip.GetWipInfo().LineID;
                info.PRODUCT_ID = wip.GetWipInfo().WipProductID;
                info.MAIN_QTY = Convert.ToDecimal(wip.GetWipInfo().UnitQty);
                info.PEG_QTY = Convert.ToDecimal(qty);
                info.STEP_ID = StringUtility.IdentityNull;
                info.MO_PRODUCT_ID = mo.Product.ProductID;
                info.LOT_STATE = StringUtility.IdentityNull;

                OutputMart.Instance.PegHistory.Add(info);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        public static void WriteUnpeg(MicronBEAssyPlanWip planWip, UnpegReason unpegReason)
        {
            try
            {
                MicronBEAssyWipInfo wipInfo = planWip.GetWipInfo();

                if (planWip.Product != null)
                {
                    WriteUnpeg(wipInfo.LineID, wipInfo.LotID, planWip.Product.ProductID,
                        planWip.MapStep.StepID, (decimal)wipInfo.UnitQty, (decimal)planWip.Qty,
                        planWip.Product.DesignID(), planWip.Product.IsBase(), wipInfo.CurrentState.ToString(),
                        unpegReason.ToString(), string.Empty);
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        internal static void WriteUnpeg(Inputs.Wip entity, string designID, bool isBase, EntityState state,UnpegReason reason, string reasonDeatil)
        {
            try
            {
                WriteUnpeg(entity.LINE_ID, entity.LOT_ID, entity.PRODUCT_ID, entity.STEP_ID, entity.LOT_QTY, entity.LOT_QTY, designID, isBase, state.ToString(), reason.ToString(), reasonDeatil);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        private static void WriteUnpeg(string lineID, string lotID, string productID, string stepID, decimal mainQty, decimal unpegQty, string designID, bool isBase, string lotState, string reason, string reasonDetail)
        {
            try
            {
                UnPegHistory info = new UnPegHistory();

                info.LINE_ID = lineID;
                info.LOT_ID = lotID;
                info.PRODUCT_ID = productID;
                info.STEP_ID = stepID;
                info.MAIN_QTY = mainQty;
                info.UNPEG_QTY = unpegQty;
                info.DESIGN_ID = designID;
                info.IS_BASE = UtilityHelper.IsYN(isBase);
                info.REASON = reason;
                info.REASON_DETAIL = reasonDetail;
                info.LOT_STATE = lotState;

                OutputMart.Instance.UnPegHistory.Add(info);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        internal static void WriteMasterDataErrorLog(MasterDataErrorEventType type, string lineID, string stepID, string lotID, string productID,
            string designID, string eqpID, decimal lotQty, string reason, string reasonDetail, string description, string relatedInputSchema)
        {
            try
            {
                MasterDataErrorLog info = new MasterDataErrorLog();

                info.EVENT_TYPE = type.ToString();
                info.LINE_ID = lineID;
                info.STEP_ID = stepID;
                info.LOT_ID = lotID;
                info.PRODUCT_ID = productID;
                info.DESIGN_ID = designID;
                info.EQUIPMENT_ID = eqpID;
                info.LOT_QTY = lotQty;
                info.REASON = reason;
                info.REASON_DETAIL = reasonDetail;
                info.DESCRIPTION = description;
                info.RELATED_INPUT_SCHEMA = relatedInputSchema;

                OutputMart.Instance.MasterDataErrorLog.Add(info);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }
    }
}
