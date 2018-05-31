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
using Mozart.Simulation.Engine;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;
using Mozart.SeePlan;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class JobChangeEvents
    {
        /// <summary>
        /// </summary>
        /// <param name="step"/>
        /// <param name="assignedEqps"/>
        /// <param name="context"/>
        /// <param name="handled"/>
        public void ON_AFTER_ASSIGN_EQP0(WorkStep step, List<AssignEqp> assignedEqps, JobChangeContext context, ref bool handled)
        {
            try
            {

                Logger.MonitorInfo(string.Format("Assign Eqp -> STEP : {0}, EQP_ID : {1}, NowDT : {2}", step.Key.ToString(), assignedEqps.ElementAt(0).Target.EqpID, DateUtility.DbToString(FindHelper.GetNowDT())));

                MicronBEAssyWorkStep ws = step as MicronBEAssyWorkStep;
                double setupTime = 0d;
                ws.AvailableDownTime = SimulationHelper.CalculateAvailableDownTime(step, assignedEqps.ElementAt(0), ref setupTime);

                Logger.MonitorInfo(string.Format("Available Down Time : {0}, Setup Time : {1}", ws.AvailableDownTime, setupTime));
               
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }

            //foreach (AssignEqp eqp in assignedEqps)
            //{
            //    eqp.Target.WakeUp();
            //}
        }

        /// <summary>
        /// </summary>
        /// <param name="wagent"/>
        /// <param name="handled"/>
        int count = 0;
        public void ON_AFTER_RUN0(WorkAgent wagent, ref bool handled)
        {
            if (wagent.Groups == null)
                return;

            else
            {
                var nowDT = FindHelper.GetNowDT();
                //if (count > 1)
                //    return;

                foreach (var wg in wagent.Groups)
                {
                    foreach (var ws in wg.Steps)
                    {

                        foreach (var profile in ws.Profiles)
                        {
                            foreach (var lot in profile.Value)
                            {
                                if (ws.Key.ToString() == "S0250" && profile.Key.Target.EqpID == "WB02" && nowDT >= new DateTime(2018, 01, 12, 12, 10, 00))
                                {

                                    if (ws.Inflows.Contains(lot))
                                    {

                                        Logger.MonitorInfo("Inflow Profile -> STEP : {0}, EQP_ID : {1}, LOT_ID : {2}, IN_TIME : {3}, OUT_TIME : {4}, NowDT : {5}", ws.Key.ToString(), profile.Key.Target.EqpID, lot.Lot.LotID, lot.InTime, lot.OutTime, wagent.NowDT);
                                    }

                                    else
                                        Logger.MonitorInfo("Profile -> STEP : {0}, EQP_ID : {1}, LOT_ID : {2}, IN_TIME : {3}, OUT_TIME : {4}, NowDT : {5}", ws.Key.ToString(), profile.Key.Target.EqpID, lot.Lot.LotID, lot.InTime, lot.OutTime, wagent.NowDT);

                                }

                            }

                        }
                    }


                }

                count++;



            }
        }
    }
}
