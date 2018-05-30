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
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class Rules
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public PegPart INIT_SUPPLY_PLAN(PegPart pegPart)
        {
            try
            {
                return pegPart;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(PegPart);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <returns/>
        public PegPart UPDATE_TARGET_INFO(PegPart pegPart)
        {
            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;
                BEStep step = pp.Product.Process.FindStep(pp.CurrentStep.StepID);
            
                if (pp.Product is AssyMcpPart)
                {
                    AssyMcpPart mcpPart = pp.Product as AssyMcpPart;
                }

                return pp;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(PegPart);
            }
        }
    }
}
