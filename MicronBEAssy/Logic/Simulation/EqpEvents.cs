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
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class EqpEvents
    {
        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="state"/>
        /// <param name="handled"/>
        public void PROCESS_STATE_CHANGED0(AoEquipment aeqp, IHandlingBatch hb, ProcessStates state, ref bool handled)
        {
            try
            {
                string status = string.Empty;
                if (state == ProcessStates.LastLoading || state == ProcessStates.LastUnloading)
                    status = LoadingStates.BUSY.ToString();
                else if (state == ProcessStates.StartSetup || state == ProcessStates.EndSetup)
                    status = LoadingStates.SETUP.ToString();

                if (string.IsNullOrEmpty(status))
                    return;

                SimulationHelper.CollectEqpPlan(hb, aeqp, status);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="state"/>
        /// <param name="handled"/>
        public void LOADING_STATE_CHANGED0(AoEquipment aeqp, IHandlingBatch hb, LoadingStates state, ref bool handled)
        {

        }
    }
}
