using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.Logic.Simulation;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class JobChangeHelper
    {
        public static string GetStepKey(Product product, MicronBEAssyBEStep step, bool isRun)
        {
            //try
            //{
            //    step = isRun ? step.GetDefaultNextStep() as MicronBEAssyBEStep : step;

            //    StepGroup type = StepGroup.NONE;

            //    int i = 0;
            //    while (i < 10000 && step != null)
            //    {
            //        if (step.StepGroup != StepGroup.NONE)
            //        {
            //            type = step.StepGroup;
            //            break;
            //        }

            //        step = step.GetDefaultNextStep() as MicronBEAssyBEStep;
            //    }

            //    return step.StepID;
            //}
            //catch (Exception e)
            //{
            //    WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            //    return default(string);
            //}

            return default(string);
        }

        public static bool IsRun(MicronBEAssyBELot lot)
        {
            try
            {
                if (lot.CurrentPlan.LoadedResource != null && lot.IsStarted && lot.IsFinished == false)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(bool);
            }
        }

        public static DateTime GetMinRunDownTime(MicronBEAssyWorkStep ws)
        {
            try
            {
                Dictionary<WorkEqp, DateTime> minRunTime = new Dictionary<WorkEqp, DateTime>();
              
                foreach (WorkEqp eqp in ws.LoadedEqps)
                {
                    DateTime minDownTime = (DateTime)eqp.Target.GetNextInTime(false);

                    ICollection<WorkLot> profileList;
                    if (ws.Profiles.TryGetValue(eqp, out profileList))
                    {
                        foreach (WorkLot lot in profileList)
                        {
                            if (minDownTime < lot.InTime)
                            {
                                if (minRunTime.ContainsKey(eqp) == false)
                                    minRunTime.Add(eqp, minDownTime);
                                else
                                    minRunTime[eqp] = minDownTime;
                                break;
                            }
                            else
                            {
                                if (minRunTime.ContainsKey(eqp) == false)
                                    minRunTime.Add(eqp, (DateTime)lot.OutTime);
                                else
                                    minRunTime[eqp] = (DateTime)lot.OutTime;

                                minDownTime = (DateTime)lot.OutTime;
                            }
                        }
                    }
                    else
                    {
                        minRunTime.Add(eqp, minDownTime);
                    }
                }

                DateTime minTime = DateTime.MaxValue;
                foreach (DateTime time in minRunTime.Values)
                {
                    if (minTime > time)
                        minTime = time;
                }

                return minTime;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(DateTime);
            }
        }
    }
}
