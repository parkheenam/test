using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.DataModel;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Persists;
using MicronBEAssy.Outputs;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class WRITE_TARGET
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="isOut"/>
        /// <param name="handled"/>
        public void WRITE_TARGET0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isOut, ref bool handled)
        {
            try
            {
                WriteHelper.WriteStepTarget(pegPart, isOut);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pegTarget"/>
        /// <param name="stepPlanKey"/>
        /// <param name="step"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Mozart.SeePlan.DataModel.StepTarget CREATE_STEP_TARGET0(PegTarget pegTarget, object stepPlanKey, Step step, bool isRun, ref bool handled, Mozart.SeePlan.DataModel.StepTarget prevReturnValue)
        {
            try
            {
                Mozart.SeePlan.DataModel.StepTarget st = new Mozart.SeePlan.DataModel.StepTarget(stepPlanKey, step, pegTarget.Qty, pegTarget.DueDate, isRun);
                return st;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }

            return default(Mozart.SeePlan.DataModel.StepTarget);
        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public object GET_STEP_PLAN_KEY0(PegPart pegPart, ref bool handled, object prevReturnValue)
        {
            MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;
            Tuple<string, string, string> key = Tuple.Create(pp.Product.LineID, pp.CurrentStep.StepID, pp.Product.ProductID);

            return key;
        }
    }
}
