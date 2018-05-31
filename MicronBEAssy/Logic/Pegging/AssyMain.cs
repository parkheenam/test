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
    public partial class AssyMain
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public Step GETLASTPEGGINGSTEP(Mozart.SeePlan.Pegging.PegPart pegPart)
        {
            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;
                MicronBEAssyProcess process = pp.Product.Process as MicronBEAssyProcess;

                return process.LastStep;
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

        /// <summary>
        /// </summary>
        /// <param name="parts"/>
        /// <returns/>
        public IEnumerable<PegPart> MERGEPEGPARTS(IEnumerable<PegPart> parts)
        {
            try
            {
                MergedPegPart mg = new MergedPegPart();

                foreach (var part in parts)
                {
                    mg.Merge(part);
                    mg.CurrentStep = part.CurrentStep;
                }

                List<PegPart> list = new List<PegPart>();
                list.Add(mg);

                return list;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IEnumerable<PegPart>);
            }
        }
    }
}
