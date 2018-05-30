using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Pegging;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class SHIFT_TAT
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public TimeSpan GET_TAT0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun, ref bool handled, TimeSpan prevReturnValue)
        {
            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;
                StepTat stepTat = FindHelper.FindTAT(pp.Product.ProductID, pp.CurrentStep.StepID, pp.Product.LineID);

                if (stepTat == null)
                    return TimeSpan.FromSeconds(0);

                double tat = 0;
                if (isRun)
                    tat = (double)stepTat.RUN_TAT;
                else
                    tat = (double)stepTat.WAIT_TAT;

                return TimeSpan.FromSeconds(tat);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(TimeSpan);
            }
        }
    }
}
