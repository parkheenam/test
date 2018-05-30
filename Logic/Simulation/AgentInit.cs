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
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.SeePlan.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class AgentInit
    {
        /// <summary>
        /// </summary>
        /// <param name="wagent"/>
        /// <param name="handled"/>
        public void INITIALIZE_AGENT0(Mozart.SeePlan.Simulation.WorkAgent wagent, ref bool handled)
        {
            try
            {
                if (wagent.Id == Constants.WorkAgentName)
                {
                    wagent.AgentType = AgentType.TRADE;
                    wagent.Interval = TimeSpan.FromMinutes(10);
                    wagent.IsReProfiling = true;
                    wagent.IsReleaseDownEqp = true;

                    foreach (MicronBEAssyBatch batch in InputMart.Instance.MicronBEAssyBatch.Values)
                    {
                        WorkGroup group = wagent.GetGroup(batch.AoProdID);

                        foreach (KeyValuePair<string, ICollection<MicronBEAssyBEStep>> stepInfo in batch.StepList)
                        {
                            foreach (MicronBEAssyBEStep step in stepInfo.Value)
                            {
                                MicronBEAssyWorkStep wStep = group.TryGetStep(stepInfo.Key) as MicronBEAssyWorkStep;

                                if (wStep == null)
                                {
                                    wStep = group.GetStep(stepInfo.Key) as MicronBEAssyWorkStep;

                                    wStep.Sequence = step.Sequence;
                                    wStep.AoProd = batch.AoProdID;
                                }

                                if (wStep.Steps.Contains(step) == false)
                                    wStep.Steps.Add(step);

                                JobChangeInit agentInitControl = ServiceLocator.Resolve<JobChangeInit>();
                                agentInitControl.InitializeWorkStep(wStep);
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }

        }

        /// <summary>
        /// </summary>
        /// <param name="wmanager"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<string> GET_WORK_AGENT_NAMES0(WorkManager wmanager, ref bool handled, IEnumerable<string> prevReturnValue)
        {
            try
            {
                return null;

                //List<string> agentNames = new List<string>();
                //agentNames.Add(Constants.WorkAgentName);
                //return agentNames;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IEnumerable<string>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="wagent"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public object GET_WORK_GROUP_KEY0(IHandlingBatch hb, WorkAgent wagent, ref bool handled, object prevReturnValue)
        {
            try
            {
                MicronBEAssyBELot lot = hb.Sample as MicronBEAssyBELot;
                string productID = lot.Product.ProductID;
                Product prod = null;
                InputMart.Instance.MicronBEProducts.TryGetValue(productID, out prod);

                AssyMcpProduct aoProd = null;
                if (prod is AssyMcpPart)
                {
                    AssyMcpPart part = prod as AssyMcpPart;
                    aoProd = part.FinalProduct as AssyMcpProduct;
                }
                if (prod is AssyMcpProduct)
                {
                    aoProd = prod as AssyMcpProduct;
                }

                if (aoProd != null)
                    return aoProd.ProductID;

                return null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(object);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public string GET_WORK_AGENT_NAME0(IHandlingBatch hb, ref bool handled, string prevReturnValue)
        {
            try
            {
                return Constants.WorkAgentName;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(string);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="wgroup"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public object GET_WORK_STEP_KEY0(IHandlingBatch hb, WorkGroup wgroup, ref bool handled, object prevReturnValue)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

#if DEBUG
                //if(lot.LotID == "LOT05")
                //    Console.WriteLine();
#endif
                string stepKey = JobChangeHelper.GetStepKey(lot.Product, lot.CurrentStep as MicronBEAssyBEStep, JobChangeHelper.IsRun(lot));
                //Tuple<string, int> stepKey = JobChangeHelper.GetStepKey(lot.Product, lot.CurrentStep as CA_PlanningBEStep, false);

                return stepKey;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(object);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="wgroup"/>
        /// <param name="handled"/>
        public void INITIALIZE_WORK_GROUP0(WorkGroup wgroup, ref bool handled)
        {
            try
            {
                wgroup.Ordered = true;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="wstep"/>
        /// <param name="handled"/>
        public void INITIALIZE_WORK_STEP0(WorkStep wstep, ref bool handled)
        {
            try
            {
                MicronBEAssyWorkStep ws = wstep as MicronBEAssyWorkStep;

                ws.UpInterval = TimeSpan.FromHours(8);
                ws.NewUpInterval = TimeSpan.FromHours(8);
                ws.DownInterval = TimeSpan.FromMinutes(30);
                ws.AllowedArrivalGap = TimeSpan.FromHours(1);

                var agentInitControl = ServiceLocator.Resolve<JobChangeInit>();
                var loadables = agentInitControl.GetLoadableEqps(wstep);
                if (loadables != null)
                {
                    loadables.ForEach(aeqp => wstep.AddLoadableEqp(aeqp));
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public WorkStep ADD_WORK_LOT0(IHandlingBatch hb, ref bool handled, WorkStep prevReturnValue)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                AssyMcpPart mcpPart = lot.Product as AssyMcpPart;

                if (mcpPart != null && mcpPart.IsBase == false)
                    return null;

                var agentInitControl = ServiceLocator.Resolve<JobChangeInit>();
                var wagentName = agentInitControl.GetWorkAgentName(hb);
                if (string.IsNullOrEmpty(wagentName))
                    return null;
                var wmanager = AoFactory.Current.JobChangeManger;
                var wagent = wmanager.GetAgent(wagentName);
                if (wagent == null)
                    return null;
                var wgroupKey = agentInitControl.GetWorkGroupKey(hb, wagent);
                if (wgroupKey == null)
                    return null;
                var wgroup = wagent.GetGroup(wgroupKey);
                var wstepKey = agentInitControl.GetWorkStepKey(hb, wgroup);
                if (wstepKey == null)
                    return null;
                var targetStep = agentInitControl.GetTargetStep(hb, wgroup, wstepKey);
                if (targetStep == null)
                    return null;
                var wstep = wgroup.GetStep(wstepKey, targetStep);
                var availableTime = agentInitControl.GetAvailableTime(hb, wstep, targetStep);

                MicronBEAssyWorkLot wlot = CreateHelper.CreateWorkLot(hb, availableTime, wstepKey, targetStep, lot.ReservationEqp);
                wstep.AddWip(wlot);
                return wstep;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(WorkStep);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="weqp"/>
        /// <param name="handled"/>
        public void INITIALIZE_WORK_EQP0(WorkEqp weqp, ref bool handled)
        {
            weqp.IncludeBufferForAvailableTime = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="wstep"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<AoEquipment> GET_LOADABLE_EQPS0(WorkStep wstep, ref bool handled, IEnumerable<AoEquipment> prevReturnValue)
        {
            try
            {
                List<AoEquipment> equipList = new List<AoEquipment>();

                if (wstep.Steps.Count == 0)
                    return equipList;

                List<string> list = SimulationHelper.GetLoadableList(wstep.Steps[0] as MicronBEAssyBEStep);

                foreach (string eqp in list)
                {
                    AoEquipment aeqp = AoFactory.Current.GetEquipment(eqp);
                    if (aeqp == null)
                        continue;

                    equipList.Add(aeqp);
                }

                return equipList;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IEnumerable<AoEquipment>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="wgroup"/>
        /// <param name="wstepKey"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Step GET_TARGET_STEP0(IHandlingBatch hb, WorkGroup wgroup, object wstepKey, ref bool handled, Step prevReturnValue)
        {
            try
            {
                MicronBEAssyWorkStep workStep = wgroup.TryGetStep(wstepKey) as MicronBEAssyWorkStep;

                if (workStep != null)
                    return workStep.Steps.ElementAt(0);

                return null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Step);
            }
        }
    }
}
