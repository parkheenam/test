using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.SeePlan.SemiBE.Pegging;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class PREPARE_WIP
    {

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart PREPARE_WIP0(PegPart pegPart, ref bool handled, PegPart prevReturnValue)
        {
            try
            {
                List<IWipInfo> wipInfoList = PrepareWipRuleHelper.GetWipInfoList();

                List<PlanWip> planWips = new List<PlanWip>();
                foreach (IWipInfo wipInfo in wipInfoList)
                {
                    List<PlanWip> planWipList = PrepareWipRuleHelper.GetPlanWips(wipInfo);

                    if (planWipList != null && planWipList.Count > 0)
                        planWips.AddRange(planWipList);
                }

                PrepareWipRuleHelper.RegisterInputMart(planWips);

                return pegPart;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(PegPart);
            }
        }
    }
}
