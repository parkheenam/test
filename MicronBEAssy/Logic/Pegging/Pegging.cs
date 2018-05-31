using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Pegging;
using MicronBEAssy.DataModel;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class Pegging
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public Step GETLASTPEGGINGSTEP(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            try
            {
                string areaName = (Pegger.Current.CurrentModel as IPeggerModel2).CurrentArea.Name;

                if (areaName.IsNullOrEmpty() == false && areaName == "PRODUCTION_LINE")
                    return (pegPart as MicronBEAssyBEPegPart).Product.Process.LastStep;
                else
                {
                    //MicronBEAssyProcess proc = new MicronBEAssyProcess("-");
                    //MicronBEAssyBEStep step = new MicronBEAssyBEStep("DieBank");
                    //proc.Steps.Add(step);
                    return null;
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Step);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="currentStep"/>
        /// <returns/>
        public Step GETPREVPEGGINGSTEP(PegPart pegPart, Step currentStep)
        {
            try
            {
                return currentStep.GetDefaultPrevStep();
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Step);
            }
        }
    }
}
