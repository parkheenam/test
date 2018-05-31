using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class DispatcherControl
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Type GET_LOT_BATCH_TYPE0(ref bool handled, Type prevReturnValue)
        {
            try
            {
                return default(Type);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Type);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="aeqp"/>
        /// <param name="wips"/>
        /// <param name="dispatcher"/>
        /// <param name="handled"/>
        public void ON_DISPATCH0(DispatchingAgent da, AoEquipment aeqp, IList<IHandlingBatch> wips, IEntityDispatcher dispatcher, ref bool handled)
        {

        }

        /// <summary>
        /// </summary>
        /// <param name="db"/>
        /// <param name="wips"/>
        /// <param name="ctx"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<IHandlingBatch> EVALUATE0(DispatcherBase db, IList<IHandlingBatch> wips, IDispatchContext ctx, ref bool handled, IList<IHandlingBatch> prevReturnValue)
        {
            try
            {
                wips.QuickSort(new ComparerHelper.LotCompare());
                return wips;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IList<IHandlingBatch>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="aeqp"/>
        /// <param name="wips"/>
        /// <param name="handled"/>
        public void ON_DISPATCHED0(DispatchingAgent da, AoEquipment aeqp, IHandlingBatch[] wips, ref bool handled)
        {
            if (wips != null) 
            {
                DoubleDictionary<Step, object, StepPlan> stepPlans = StepPlanManager.Current.RunPlans;

                foreach (var wip in wips)
                {
                    foreach (var stepPlan in stepPlans)
                    {
                        if (stepPlan.Step.StepID != wip.CurrentStep.StepID)
                            continue;

                        Tuple<string, string, string> key = stepPlan.Key as Tuple<string, string, string>;
                        string lineID = key.Item1;
                        string stepID = key.Item2;
                        string prodID = key.Item3;

                        var lot = wip.Sample as MicronBEAssyBELot;

                        if (lineID != lot.LineID)
                            continue;

                        if (prodID != lot.Product.ProductID)
                            continue;

                        var target = stepPlan.StepTargetList.FirstOrDefault();

                        if (target == null)
                            continue;

                        target.Peg(lot, lot.UnitQty);
                    }
                }
            }
        }
    }
}
