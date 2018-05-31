using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.Engine;
using MicronBEAssy.DataModel;
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class JobProfileControl
    {
        /// <summary>
        /// </summary>
        /// <param name="wl"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public WorkEqp SELECT_PROFILE_EQP0(WorkLoader wl, ref bool handled, WorkEqp prevReturnValue)
        {
            try
            {
                WorkEqp selectEqp = null;
                Time minAvailableTime = DateTime.MaxValue;

#if DEBUG
                //var a = FindHelper.GetNowDT();

                //if (FindHelper.GetNowDT().Hour == 11 && FindHelper.GetNowDT().Minute == 40)
                //    Console.WriteLine();
#endif

                List<WorkEqp> list = new List<WorkEqp>(wl.EqpList);

                if (list.Count == 0)
                    return selectEqp;

                foreach (WorkEqp eqp in list)
                {
                    if (eqp.IsDone())
                        continue;

                    if (selectEqp == null || minAvailableTime > eqp.AvailableTime)
                    {
                        minAvailableTime = eqp.AvailableTime;
                        selectEqp = eqp;
                    }
                }

                return selectEqp;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(WorkEqp);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="wlot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public double GET_PROFILE_LOT_QTY0(WorkLot wlot, ref bool handled, double prevReturnValue)
        {
            try
            {
#if DEBUG
                //if (FindHelper.GetNowDT().Hour == 22 && FindHelper.GetNowDT().Minute == 30)
                //    Console.WriteLine();
#endif

                //return (wlot.Batch as MicronBEAssyBELot).UnitQtyDouble;
                return 1;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(double);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="wstep"/>
        /// <param name="wlot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<WorkLot> ADVANCE0(WorkStep wstep, WorkLot wlot, ref bool handled, IEnumerable<WorkLot> prevReturnValue)
        {
            try
            {
                var nowDt = FindHelper.GetNowDT();

#if DEBUG
                if (nowDt >= new DateTime(2018, 01, 26, 07, 00, 00))
                    Console.WriteLine();

                if (wstep.Key.ToString() == "S0250" && wlot.Lot.LotID == "LOT10_1")
                    Console.WriteLine();

                if (wlot.Lot.LotID == "LOT11_1")
                    Console.WriteLine();
#endif


                if (wlot.OutTime > FindHelper.GetNowDT().AddHours(10))
                    return null;

#if DEBUG
                //if (wstep.Key.ToString() == "S0100")
                //    Console.WriteLine();
#endif
                List<WorkLot> list = new List<WorkLot>();

                MicronBEAssyWorkLot currentWLot = wlot as MicronBEAssyWorkLot;

                MicronBEAssyBEStep currentStep = wstep.Steps[0] as MicronBEAssyBEStep;

                string stepKey = JobChangeHelper.GetStepKey(currentWLot.Product, currentStep.GetDefaultNextStep() as MicronBEAssyBEStep, false);

                //string stepKey = currentStep.GetDefaultNextStep().StepID;

                if (stepKey != null)
                {
                    MicronBEAssyWorkStep nextWorkStep = wstep.Group.TryGetStep(stepKey) as MicronBEAssyWorkStep;

                    if (nextWorkStep != null)
                    {
                        MicronBEAssyWorkLot newWorkLot = CreateHelper.CreateWorkLot(wlot.Batch, wlot.OutTime, nextWorkStep.Key, nextWorkStep.Steps.ElementAt(0), null);

                        newWorkLot.LotID = currentWLot.LotID;
                        newWorkLot.Product = currentWLot.Product;
                        newWorkLot.UnitQty = currentWLot.UnitQty;

                        //if (currentWLot.Product is AssyMcpPart)
                        //{
                        //    MicronBEAssyBEStep workTargetStep = wstep.Steps[0] as MicronBEAssyBEStep;

                        //    if (workTargetStep.StepAction != null && workTargetStep.StepAction.FWTractIn == StepActionInfo.MCP)
                        //    {
                        //        AssyMcpPart mcpPart = currentWLot.Product as AssyMcpPart;

                        //        if (mcpPart.SequenceResolved != (wstep as MicronBEAssyWorkStep).Sequence)
                        //            newWorkLot.Product = mcpPart.Next == null ? mcpPart.OutProduct as Product : mcpPart.Next as Product;
                        //    }
                        //}

                        if (newWorkLot.Product != null)
                            list.Add(newWorkLot);
                    }

                }

                return list;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IEnumerable<WorkLot>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="list"/>
        /// <param name="step"/>
        /// <param name="lot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public WorkStep UPDATE_FIND_STEP0(IList<WorkStep> list, WorkStep step, WorkLot lot, ref bool handled, WorkStep prevReturnValue)
        {
            try
            {
                MicronBEAssyBELot beLot = lot.Batch.Sample as MicronBEAssyBELot;

                bool isRun = JobChangeHelper.IsRun(lot.Batch as MicronBEAssyBELot);

                string stepKey = JobChangeHelper.GetStepKey(beLot.Product, beLot.CurrentStep as MicronBEAssyBEStep, isRun);

                if (stepKey == null)
                    return null;

                WorkStep ws = null;
                if (step.Group.Steps != null)
                {
                    try
                    {
                        ws = step.Group.TryGetStep(stepKey);
                    }
                    catch
                    {
                        Console.WriteLine();
                    }
                }
                else
                    Console.WriteLine();

                return ws;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(WorkStep);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="wstep"/>
        /// <param name="wlot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public WorkLot UPDATE0(WorkStep wstep, WorkLot wlot, ref bool handled, WorkLot prevReturnValue)
        {
            try
            {
                MicronBEAssyBELot lot = wlot.Batch as MicronBEAssyBELot;

                bool isRun = JobChangeHelper.IsRun(lot);

                DateTime availableTime = FindHelper.GetNowDT();

                MicronBEAssyEqp eqp = FindHelper.FindEquipment(lot.CurrentPlan.ResID);
                double tactTime = SimulationHelper.GetTactTime(lot.LineID, lot.CurrentStepID, lot.Product.ProductID, eqp);

                StepTat stepTat = FindHelper.FindTAT(lot.Product.ProductID, lot.CurrentStepID, lot.LineID);

                if (isRun)
                {
#if DEBUG
                    //var nowDt = FindHelper.GetNowDT();

                    if (lot.LotID == "LOT11_1" && lot.CurrentStepID == "S0200")
                        Console.WriteLine();

                    //if (nowDt >= new DateTime(2018, 06, 12, 11, 40, 00))
                    //    Console.WriteLine();
#endif
                    double runTat = stepTat == null ? 0 : (double)stepTat.RUN_TAT;
#if DEBUG
                    if (runTat > 0)
                        Console.WriteLine();
#endif
                    availableTime = availableTime.AddSeconds(tactTime * lot.UnitQtyDouble + runTat);
                }
                else
                {
                    double waitTat = stepTat == null ? 0 : (double)stepTat.WAIT_TAT;

#if DEBUG
                    if(waitTat > 0)
                        Console.WriteLine();
#endif
                    availableTime = availableTime.AddSeconds(waitTat);
                }

                AoEquipment reservationEqp = isRun ? null : lot.ReservationEqp;

                MicronBEAssyWorkLot newWorkLot = CreateHelper.CreateWorkLot(wlot.Batch, availableTime, wstep.Key, wstep.Steps[0], reservationEqp);

                return newWorkLot;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(WorkLot);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<IHandlingBatch> GET_MAPPING_LOT0(IHandlingBatch hb, ref bool handled, IEnumerable<IHandlingBatch> prevReturnValue)
        {
            try
            {
                List<IHandlingBatch> list = new List<IHandlingBatch>();

                list.Add(hb);

                return list;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IEnumerable<IHandlingBatch>);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="weqp"/>
        /// <param name="wlot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public TimeSpan GET_TACT_TIME0(WorkEqp weqp, WorkLot wlot, ref bool handled, TimeSpan prevReturnValue)
        {
            try
            {
                MicronBEAssyWorkLot lot = wlot as MicronBEAssyWorkLot;

                double unitQty = (wlot.Batch as MicronBEAssyBELot).UnitQtyDouble;
                MicronBEAssyBELot beLot = wlot.Batch as MicronBEAssyBELot;

                var setupControl = ServiceLocator.Resolve<SetupControl>();
                var processControl = ServiceLocator.Resolve<ProcessControl>();

                MicronBEAssyEqp eqp = FindHelper.FindEquipment(weqp.Target.EqpID);
                double tactTime = SimulationHelper.GetTactTime(beLot.LineID, lot.Step.StepID, lot.Product.ProductID, eqp);

                if (beLot.CurrentStepID != weqp.Step.Key.ToString())
                    return TimeSpan.FromSeconds(tactTime * unitQty);

#if DEBUG
                //if (lot.LotID == "LOT10_3")
                //    Console.WriteLine();

                //if (FindHelper.GetNowDT().Hour == 22 && FindHelper.GetNowDT().Minute == 30)
                //    Console.WriteLine();

                var nowDt = FindHelper.GetNowDT();
                if (lot.LotID == "LOT10_9" && lot.Step.Key.ToString() == "S0300" && weqp.Target.EqpID == "DA01" && nowDt >= new DateTime(2018, 01, 26, 12, 00, 00))
                    Console.WriteLine();
#endif

                bool isNeedSetup = false;
                double setupTime = 0d;

                var lastWorkLots = weqp.Step.Profiles[weqp];

                if (lastWorkLots != null && lastWorkLots.Count != 0)
                {
                    isNeedSetup = SimulationHelper.IsNeedSetupProfile(weqp, beLot);
                    if (isNeedSetup)
                    {
                        setupTime = SimulationHelper.GetSetupTimeProfile(weqp, beLot).TotalSeconds;
                    }
                }
                else
                {
                    bool _handled = false;
                    isNeedSetup = processControl.IS_NEED_SETUP0(weqp.Target, beLot, ref _handled, false);
                    if (isNeedSetup)
                    {
                        setupTime = setupControl.GET_SETUP_TIME0(weqp.Target, beLot, ref _handled, default(Mozart.Simulation.Engine.Time)).TotalSeconds;
                    }
                }

                double tactTimeWithSetup = setupTime + (tactTime * unitQty);
                return TimeSpan.FromSeconds(tactTimeWithSetup);
               
                //return TimeSpan.FromSeconds(tactTime);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(TimeSpan);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="weqp"/>
        /// <param name="wstep"/>
        /// <param name="list"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public List<WorkLot> DO_FILTER_LOT0(WorkEqp weqp, WorkStep wstep, List<WorkLot> list, ref bool handled, List<WorkLot> prevReturnValue)
        {
            try
            {
#if DEBUG
                if(weqp.Target.EqpID == "DA03" && wstep.Key.ToString() == "S0100")
                    Console.WriteLine();
#endif
                MicronBEAssyWorkStep ws = wstep as MicronBEAssyWorkStep;

                List<WorkLot> lotList = new List<WorkLot>();
                foreach (MicronBEAssyWorkLot lot in list)
                {
                    AssyMcpPart mcpPart = lot.Product as AssyMcpPart;

                    if (mcpPart != null && mcpPart.IsBase == false)
                        continue;

                    MicronBEAssyBELot beLot = lot.Batch as MicronBEAssyBELot;

                    if (lot.ReservationEqp != null && lot.ReservationEqp != weqp.Target)
                        continue;
                  
                    lotList.Add(lot);
                }

                if (lotList.IsNullOrEmpty())
                    weqp.Stop = true;

                return lotList;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(List<WorkLot>);
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="wstep"/>
        /// <param name="list"/>
        /// <param name="handled"/>
        public void SORT_PROFILE_LOT0(WorkStep wstep, WorkEqp weqp, List<WorkLot> list, ref bool handled)
        {
            try
            {
                list.Sort(new ComparerHelper.LotCompare());
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            } 
        }

        /// <summary>
        /// </summary>
        /// <param name="wl"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        public void ON_BEGIN_PROFILING0(WorkLoader wl, Time now, ref bool handled)
        {
            
        }

        /// <summary>
        /// </summary>
        /// <param name="wl"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        public void ON_END_PROFILING0(WorkLoader wl, Time now, ref bool handled)
        {
            
                
        }
    }
}
