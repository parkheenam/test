using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Simulation;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using Mozart.Text;
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class ProcessControl
    {
        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public ProcTimeInfo GET_PROCESS_TIME0(Mozart.SeePlan.Simulation.AoEquipment aeqp, IHandlingBatch hb, ref bool handled, ProcTimeInfo prevReturnValue)
        {
            try
            {
                ProcTimeInfo info = new ProcTimeInfo();

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                MicronBEAssyBEStep step = hb.CurrentStep as MicronBEAssyBEStep;

                foreach (StepTime time in InputMart.Instance.StepTime.DefaultView)
                {
                    if (step.StepID != time.STEP_ID)
                        continue;

                    if (lot.Product.LineID != time.LINE_ID)
                        continue;

                    if (LikeUtility.Like(lot.Product.ProductID, time.PRODUCT_ID) == false)
                        continue;

                    //if (LikeUtility.Like(lot.Product.Process.ProcessID, time.PROCESS_ID) == false)
                    //    continue;

                    if (LikeUtility.Like(aeqp.EqpID, time.EQP_ID) == false)
                        continue;

                    double tactTimeBySec = (double)time.TACT_TIME;

                    info.TactTime = TimeSpan.FromSeconds(tactTimeBySec);

                    break;
                }

                return info;

            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(ProcTimeInfo);
            }                
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_NEED_SETUP0(AoEquipment aeqp, IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            try
            {
                MicronBEAssyEqp eqp = null;
                if (InputMart.Instance.MicronBEAssyEqp.ContainsKey(aeqp.EqpID))
                    eqp = InputMart.Instance.MicronBEAssyEqp[aeqp.EqpID];

                if (eqp == null)
                    return false;

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                BEStep step = lot.CurrentStep;
                StepMaster sm = InputMart.Instance.StepMasterView.FindRows(step.StepID).FirstOrDefault();

                if (sm == null)
                    return false;

                foreach (var info in InputMart.Instance.SetupInfo.DefaultView)
                {
                    if (info.LINE_ID != eqp.LineID)
                        continue;

                    //if (UtilityHelper.StringToEnum(info.STEP_GROUP, StepGroup.NONE) != eqp.StepGroup)
                    //    continue;

                    //if (LikeUtility.Like(eqp.EqpModel, info.EQP_MODEL) == false)
                    //    continue;

                    if (aeqp.LastPlan == null)
                        return false;

                    SetupType type = SimulationHelper.CheckSetup(aeqp, hb);

                    if (type != SetupType.NONE)
                        return true;
                    else
                        return false;
                }

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_CUSTOM_LOAD0(AoEquipment aeqp, IHandlingBatch hb, ref bool handled)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                string stepID = hb.CurrentStep.StepID;
                double stdLotSize = SimulationHelper.GetStdLotSize(stepID, lot.Product.LineID, lot.Product.ProductID);

                if (stdLotSize > 0 && stdLotSize < lot.UnitQtyDouble)
                {
                    List<MicronBEAssyBELot> splitLots = SimulationHelper.GetSplitLots(lot, stdLotSize);

                    foreach (MicronBEAssyBELot splitLot in splitLots)
                    {
                        splitLot.ReservationEqp = aeqp;
                        AoFactory.Current.In(splitLot);
                        aeqp.AddInBuffer(splitLot);
                        SimulationHelper.CollectEqpPlan(splitLot, aeqp, LoadState.WAIT.ToString());
                    }

                    string waitPlanKey = SimulationHelper.GetEqpPlanKey(lot, string.Empty, LoadState.WAIT.ToString());
                    if (InputMart.Instance.EqpPlans.ContainsKey(waitPlanKey))
                        InputMart.Instance.EqpPlans.Remove(waitPlanKey);
                    
                }
                else
                {
                    AoFactory.Current.In(lot);
                    aeqp.AddInBuffer(lot);
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }
    }
}
