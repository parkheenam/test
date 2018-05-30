using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.SeePlan.SemiBE.Pegging;
using Mozart.SeePlan.DataModel;
using MicronBEAssy.DataModel;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class PrepareWipRuleHelper
    {
        public static List<IWipInfo> GetWipInfoList()
        {
            try
            {
                List<IWipInfo> wipInfoList = new List<IWipInfo>(InputMart.Instance.MicronBEAssyWipInfo.Values);

                return wipInfoList;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(List<IWipInfo>);
            }
        }

        public static List<PlanWip> GetPlanWips(IWipInfo wipInfo)
        {
            try
            {
                List<PlanWip> wips = new List<PlanWip>();

                PlanWip planWip = CreateHelper.CreatePlanWip(wipInfo);
                wips.Add(planWip);

                return wips;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(List<PlanWip>);
            }
        }

        private static Step GetCurrentStep(PlanWip planWip)
        {
            try
            {
                return planWip.MapStep;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Step);
            }
        }
        
        public static void RegisterInputMart(List<PlanWip> planWips)
        {
            try
            {
                foreach (MicronBEAssyPlanWip wip in planWips)
                {
                    InputMart.Instance.MicronBEAssyPlanWip.Add(wip.MapStep.StepID, wip);
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }
    }
}
