using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Simulation;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using Mozart.SeePlan.DataModel;
using Mozart.Text;
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.Simulation.Engine;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class Route
    {
        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<string> GET_LOADABLE_EQP_LIST0(Mozart.SeePlan.Simulation.DispatchingAgent da, IHandlingBatch hb, ref bool handled, IList<string> prevReturnValue)
        {
            try
            {
                List<string> list = new List<string>();

                return list;

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

#if DEBUG
                if(lot.LotID == "LOT10_L")
                    Console.WriteLine();
#endif

                MicronBEAssyBEStep step = hb.CurrentStep as MicronBEAssyBEStep;

                foreach (EqpArrange eqpArrange in InputMart.Instance.EqpArrange.DefaultView)
                {
                    if (step.StepID != eqpArrange.STEP_ID)
                        continue;

                    if (lot.Product.LineID != eqpArrange.LINE_ID)
                        continue;

                    if (LikeUtility.Like(lot.Product.ProductID, eqpArrange.PRODUCT_ID) == false)
                        continue;

                    //if (LikeUtility.Like(lot.Product.Process.ProcessID, eqpArrange.PROCESS_ID) == false)
                    //    continue;

                    foreach (MicronBEAssyEqp eqp in InputMart.Instance.MicronBEAssyEqp.Values)
                    {
                        //if (LikeUtility.Like(eqp.EqpModel, eqpArrange.EQP_MODEL) == false)
                        //    continue;

                        if (LikeUtility.Like(eqp.EqpID, eqpArrange.EQP_ID) == false)
                            continue;

                        list.Add(eqp.EqpID);
                    }
                }

                return list;
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IList<string>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="task"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public LoadInfo CREATE_LOAD_INFO0(ILot lot, Step task, ref bool handled, LoadInfo prevReturnValue)
        {
            try
            {
                MicronBEAssyPlanInfo plan = new MicronBEAssyPlanInfo();

                MicronBEAssyBELot beLot = lot as MicronBEAssyBELot;

                plan.Init(task);
                plan.LotID = lot.LotID;
                plan.UnitQty = lot.UnitQty;
                plan.ProductID = beLot.Product.ProductID;
                plan.ProcessID = beLot.Process.ProcessID;

                return plan;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(LoadInfo);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_DISPATCH_IN0(DispatchingAgent da, IHandlingBatch hb, ref bool handled)
        {
            try
            {
#if DEBUG
                if(hb.Sample.LotID == "LOT10_L")
                    Console.WriteLine();
#endif

                SimulationHelper.CollectEqpPlan(hb, null, LoadState.WAIT.ToString());
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="ao"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        public void ON_START_TASK0(IHandlingBatch hb, ActiveObject ao, DateTime now, ref bool handled)
        {
            try
            {
                AoEquipment equip = ao as AoEquipment;

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                if (equip == null)
                    SimulationHelper.CollectEqpPlan(hb, equip, LoadState.BUSY.ToString());
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="ao"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        public void ON_END_TASK0(IHandlingBatch hb, ActiveObject ao, DateTime now, ref bool handled)
        {
            try
            {
                AoEquipment equip = ao as AoEquipment;

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                if (equip == null)
                    SimulationHelper.CollectEqpPlan(hb, equip, LoadState.BUSY.ToString());

                MicronBEAssyBEStep step = lot.CurrentStep as MicronBEAssyBEStep;
                //if (step.StepAction != null && step.StepAction.FWTractIn == StepActionInfo.MCP)
                //    lot.ChildLots.Clear();

                if (lot.ReservationEqp == equip)
                    lot.ReservationEqp = null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }
    }
}
