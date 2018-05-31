using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Simulation;
using MicronBEAssy.Outputs;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class FactoryEvents
    {
        /// <summary>
        /// </summary>
        /// <param name="aoFactory"/>
        /// <param name="handled"/>
        public void ON_DONE0(Mozart.SeePlan.Simulation.AoFactory aoFactory, ref bool handled)
        {
            try
            {
                foreach (EqpPlan plan in InputMart.Instance.EqpPlans.Values)
                {
                    OutputMart.Instance.EqpPlan.Add(plan);
                }
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            } 
        }
    }
}
