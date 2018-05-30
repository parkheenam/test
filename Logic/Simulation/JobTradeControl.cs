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
using System.Collections;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;
using Mozart.SeePlan;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class JobTradeControl
    {
        /// <summary>
        /// </summary>
        /// <param name="step"/>
        /// <param name="context"/>
        /// <param name="reason"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public OperationType CLASSIFY_OPERATION_TYPE0(WorkStep step, JobChangeContext context, out object reason, ref bool handled, OperationType prevReturnValue)
        {
            try
            {
                reason = string.Empty;
                OperationType oType = OperationType.Keep;

                MicronBEAssyWorkStep ws = step as MicronBEAssyWorkStep;

                DateTime nowDT = FindHelper.GetNowDT();


#if DEBUG
                if (step.Key.ToString() == "S0250" && nowDT >= new DateTime(2018, 06, 12, 12, 10, 00))
                    Console.WriteLine();
#endif

                if (ws.LoadedEqps.Count > 1)
                {
                    DateTime runDownTime = JobChangeHelper.GetMinRunDownTime(ws);

                    if (runDownTime.Subtract(nowDT).TotalSeconds < ws.DownInterval.TotalSeconds)
                        return OperationType.Down;
                }

                if (ws.LoadedEqps.Count == 1)
                {
                    if (ws.ProfileAssignWips.Count <= 0)
                        return OperationType.Down;
                }

                if (ws.Wips.Count > 0)
                {
                    AoEquipment selectEqp = null;
                    DateTime minNextInTime = DateTime.MaxValue;
                    foreach (AoEquipment aeqp in ws.LoadableEqps)
                    {
                        if (ws.LoadedEqpIDs.Contains(aeqp.EqpID))
                            continue;

                        //DateTime nextInTime = (DateTime)SimulationHelper.GetNextInTime(aeqp, false);
                        DateTime nextInTime = (DateTime)aeqp.GetNextInTime(false);

                        if (selectEqp == null || minNextInTime > nextInTime)
                        {
                            selectEqp = aeqp;
                            minNextInTime = nextInTime;
                        }
                    }

                    if (selectEqp != null)
                    {
                        if (step.Key.ToString() == "S0300" && nowDT >= new DateTime(2018, 01, 26, 07, 30, 00))
                            Console.WriteLine();

                        if (step.Key.ToString() == "S0300" && nowDT >= new DateTime(2018, 01, 26, 11, 00, 00))
                            Console.WriteLine();

                        ws.AddLoadedEqp(selectEqp);
                        ws.Group.CalculateProfile(ws);
                        //SimulationHelper.CalculateProfile(ws.Group, ws);

                        DateTime runDownTime = JobChangeHelper.GetMinRunDownTime(ws);

                        ws.RemoveLoadedEqp(selectEqp);
                        ws.Group.CalculateProfile(ws);
                        //SimulationHelper.CalculateProfile(ws.Group, ws);

                        var setupControl = ServiceLocator.Resolve<SetupControl>();
                        var processControl = ServiceLocator.Resolve<ProcessControl>();

                        bool isNeedSetup = false;
                        var setupTime = 0d;

                        var wip = ws.Wips.FirstOrDefault();

                        if (wip != null)
                        {
                            var beLot = wip.Lot as MicronBEAssyBELot;

                            bool _handled = false;
                            isNeedSetup = processControl.IS_NEED_SETUP0(selectEqp, beLot, ref _handled, false);
                            if (isNeedSetup)
                            {
                                setupTime = setupControl.GET_SETUP_TIME0(selectEqp, beLot, ref _handled, default(Mozart.Simulation.Engine.Time)).TotalSeconds;
                            }

                        }

                        if (runDownTime > nowDT.AddSeconds(ws.NewUpInterval.TotalSeconds + setupTime))
                        {
#if DEBUG
                            if (step.Key.ToString() == "S0250" && nowDT >= new DateTime(2018, 06, 12, 12, 10, 00))
                                Console.WriteLine();
#endif
                            oType = OperationType.Up;
                        }
                        else
                            oType = OperationType.Keep;
                    }
                }

                return oType;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                reason = string.Empty;
                return default(OperationType);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="upWorkStep"/>
        /// <param name="assignEqps"/>
        /// <param name="context"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public List<AssignEqp> DO_FILTER_ASSIGN_EQP0(WorkStep upWorkStep, List<AssignEqp> assignEqps, JobChangeContext context, ref bool handled, List<AssignEqp> prevReturnValue)
        {
            try
            {
                List<AssignEqp> list = new List<AssignEqp>();
                foreach (AssignEqp eqp in assignEqps)
                {
                    if (eqp.WorkStep == null || eqp.WorkStep.OperationType == OperationType.Down)
                    {
                        list.Add(eqp);
                        continue;
                    }

                    int properEqpCount = SimulationHelper.CalculateProperEqpCount(eqp);
                    bool canFilter = false;
                    canFilter = SimulationHelper.CanFilterAssignEqp(eqp);

                    if (properEqpCount < eqp.WorkStep.LoadedEqpCount && canFilter)
                    {
#if DEBUG
                        DateTime nowDt = FindHelper.GetNowDT();
                        MicronBEAssyWorkStep ws = eqp.WorkStep as MicronBEAssyWorkStep;
                        if(eqp.WorkStep.Key.ToString() == "S0250")
                            Console.WriteLine();
                        Logger.MonitorInfo(string.Format("Filtered -> STEP : {0}, EQP_ID : {1}, NowDT : {2}, Available Down : {3}", eqp.WorkStep.Key.ToString(), eqp.Target.EqpID, DateUtility.DbToString(FindHelper.GetNowDT()), DateUtility.DbToString(ws.AvailableDownTime)));
#endif
                        list.Add(eqp);
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(List<AssignEqp>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="upWorkStep"/>
        /// <param name="context"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool CAN_ASSIGN_MORE0(WorkStep upWorkStep, JobChangeContext context, ref bool handled, bool prevReturnValue)
        {
            try
            {
                DateTime nowDT = FindHelper.GetNowDT();
                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(bool);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="x"/>
        /// <param name="y"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public int COMPARE_ASSIGN_EQP0(AssignEqp x, AssignEqp y, ref bool handled, int prevReturnValue)
        {
            try
            {
                //int cmp = SimulationHelper.GetNextInTime(x.Target).CompareTo(SimulationHelper.GetNextInTime(y.Target));
                var xInTime = x.Target.GetNextInTime(true);
                var yInTime = y.Target.GetNextInTime(true);

                int cmp = x.Target.GetNextInTime(true).CompareTo(y.Target.GetNextInTime(true));

                if (cmp == 0)
                    cmp = x.Target.EqpID.CompareTo(y.Target.EqpID);

                return cmp;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(int);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="step"/>
        /// <param name="reason"/>
        /// <param name="context"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public float CALCULATE_PRIORITY0(WorkStep step, object reason, JobChangeContext context, ref bool handled, float prevReturnValue)
        {
            try
            {
                float priority = 0;

                MicronBEAssyWorkStep ws = step as MicronBEAssyWorkStep;

                if (step.OperationType == OperationType.Up)
                    priority = 1000;

                priority += 100 / ws.Sequence;

                return priority;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(float);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="wstep"/>
        /// <param name="context"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<AoEquipment> SELECT_DOWN_EQP0(WorkStep wstep, JobChangeContext context, ref bool handled, IEnumerable<AoEquipment> prevReturnValue)
        {
            MicronBEAssyWorkStep ws = wstep as MicronBEAssyWorkStep;
            DateTime availableDownTime = ws.AvailableDownTime;
            DateTime nowDt = FindHelper.GetNowDT();

            if (nowDt < availableDownTime)
                return null;
            else
            {
                var t = wstep.LoadedEqps.FirstOrDefault();

                if (t != null)
                {
                    Logger.MonitorInfo(string.Format("Down Eqp -> STEP : {0}, EQP_ID : {1}, NowDT : {2}", ws.Key.ToString(), t.Target.EqpID, DateUtility.DbToString(FindHelper.GetNowDT())));


                    return new AoEquipment[] { t.Target };
                }
                else
                    return null;
            }
        }
    }
}
