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
using MicronBEAssy.Outputs;
using MicronBEAssy.Inputs;
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.SeePlan;
using Mozart.Text;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.DataModel;
using MicronBEAssy.Logic.Simulation;

namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class SimulationHelper
    {
        public static void CollectEqpPlan(Mozart.SeePlan.Simulation.IHandlingBatch hb, AoEquipment equip, string status)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                string eqpID = string.Empty;
                if (equip != null)
                    eqpID = equip.EqpID;

                string waitPlanKey = SimulationHelper.GetEqpPlanKey(lot, string.Empty, LoadState.WAIT.ToString());
                if (InputMart.Instance.EqpPlans.ContainsKey(waitPlanKey))
                    InputMart.Instance.EqpPlans.Remove(waitPlanKey);

                string waitPlanKeyWithEqp = SimulationHelper.GetEqpPlanKey(lot, eqpID, LoadState.WAIT.ToString());
                if (InputMart.Instance.EqpPlans.ContainsKey(waitPlanKeyWithEqp))
                    InputMart.Instance.EqpPlans.Remove(waitPlanKeyWithEqp);

                string key = SimulationHelper.GetEqpPlanKey(lot, eqpID, status);
                string arrivalTimeKey = lot.LotID.Split('_')[0] + lot.CurrentStepID;

                EqpPlan plan;
                if (InputMart.Instance.EqpPlans.TryGetValue(key, out plan) == false)
                {
                    MicronBEAssyEqp eqp;
                    InputMart.Instance.MicronBEAssyEqp.TryGetValue(eqpID, out eqp);

                    plan = new EqpPlan();

                    plan.LINE_ID = lot.LineID;
                    plan.PRODUCT_ID = status != LoadingStates.BUSY.ToString() && status != LoadState.WAIT.ToString() ? status : lot.Product.ProductID;
                    plan.LOT_ID = status != LoadingStates.BUSY.ToString() && status != LoadState.WAIT.ToString() ? Mozart.SeePlan.StringUtility.IdentityNull : lot.LotID;
                    plan.INIT_STEP = status != LoadingStates.BUSY.ToString() && status != LoadState.WAIT.ToString() ? Mozart.SeePlan.StringUtility.IdentityNull : lot.WipInfo.InitialStep.StepID;
                    plan.ARRIVAL_TIME = status != LoadingStates.BUSY.ToString() && status != LoadState.WAIT.ToString() ? default(DateTime) : FindHelper.GetNowDT();
                    plan.PROCESS_ID = lot.CurrentStep.Process.ProcessID;
                    plan.STEP_ID = lot.CurrentStepID;
                    plan.EQP_ID = eqpID;
                    //plan.STEP_GROUP = eqp == null ? Mozart.SeePlan.StringUtility.IdentityNull : eqp.StepGroup.ToString();
                    plan.QTY = lot.UnitQty;
                    plan.STATUS = status;
                    plan.DESIGN_ID = lot.Product.DesignID();
                    plan.SEQUENCE = lot.CurrentStep.Sequence;
                    plan.COMP_SEQ = lot.Product is AssyMcpPart ? (lot.Product as AssyMcpPart).CompSeq : 1;
                    plan.IS_BASE = UtilityHelper.IsYN(lot.Product.IsBase());
                    plan.STEP_GROUP = (lot.CurrentStep as MicronBEAssyBEStep).StepGroup;

                    if (lot.Product is AssyMcpProduct)
                        plan.FINAL_PROD_ID = lot.Product.ProductID;
                    else if (lot.Product is AssyMcpPart)
                        plan.FINAL_PROD_ID = (lot.Product as AssyMcpPart).FinalProduct.ProductID;
                    else
                        plan.FINAL_PROD_ID = lot.Product.ProductID;

                    InputMart.Instance.EqpPlans.Add(key, plan);

                    if (status == LoadState.WAIT.ToString())
                    {
                        DateTime arTime;
                        if (InputMart.Instance.ArrivalTime.TryGetValue(arrivalTimeKey, out arTime) == false)
                            InputMart.Instance.ArrivalTime.Add(arrivalTimeKey, plan.ARRIVAL_TIME);
                    }
                    else
                    {
                        plan.START_TIME = FindHelper.GetNowDT();
                        plan.END_TIME = FindHelper.GetNowDT();
                    }
                }
                else
                {
                    DateTime arTime;
                    plan.END_TIME = FindHelper.GetNowDT();
                    InputMart.Instance.ArrivalTime.TryGetValue(arrivalTimeKey, out arTime);
                    plan.ARRIVAL_TIME = status == LoadingStates.SETUP.ToString() ? default(DateTime) : arTime;

                    key = SimulationHelper.GetEqpPlanKey(lot, eqpID, LoadingStates.SETUP.ToString());
                    EqpPlan setupPlan;
                    if (InputMart.Instance.EqpPlans.TryGetValue(key, out setupPlan))
                    {
                        setupPlan.PRODUCT_ID = lot.Product.ProductID;
                    }
                }

            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        public static void SetSteps(MicronBEAssyBatch batch, Product product)
        {
            try
            {
                if (product.IsMcp())
                {
                    AssyMcpProduct mcpProduct = null;

                    if (product is AssyMcpPart)
                        mcpProduct = (product as AssyMcpPart).FinalProduct as AssyMcpProduct;
                    else
                        mcpProduct = product as AssyMcpProduct;

                    foreach (AssyMcpPart mcpPart in mcpProduct.AllParts)
                    {
                        //if (mcpPart.IsMain == false)
                        //    continue;

                        MicronBEAssyProcess proc = mcpPart.Process as MicronBEAssyProcess;

                        int seq = mcpPart.CompSeq;
                        //if (proc.ProcessType == ProcessType.COW)
                        //{
                        //    if (mcpPart.IsMidPart == false && mcpPart.IsBase)//COW 1차는 Merge 후 2차 DA에서 진행하기 때문에 없다고 판단해야함.
                        //    {
                        //        continue;
                        //    }
                        //}

                        AddSteps(batch, proc, seq);
                    }

                    AddSteps(batch, mcpProduct.Process as MicronBEAssyProcess, mcpProduct.MaxSequence);
                }
                else
                {
                    AddSteps(batch, product.Process as MicronBEAssyProcess, 1);
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        private static void AddSteps(MicronBEAssyBatch batch, MicronBEAssyProcess proc, int seq)
        {
            try
            {
                foreach (MicronBEAssyBEStep step in proc.Steps)
                {
                    //if (step.StepGroup == StepGroup.NONE)
                    //    continue;

                    string key = step.StepID;
                    if (batch.StepList.ContainsKey(key) == false)
                        batch.StepList.Add(key, step);
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        public static SetupType CheckSetup(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                MicronBEAssyPlanInfo plan = aeqp.LastPlan as MicronBEAssyPlanInfo;

                if (plan == null)
                    return SetupType.NONE;

                if (plan.ProductID != lot.Product.ProductID)
                    return SetupType.PART_CHG;

                return SetupType.NONE;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return SetupType.NONE;
            }
        }

        public static double GetStdLotSize(string stepID, string lineID, string productID)
        {
            try
            {
                double stdLotSize = 0;
                foreach (StdLotSize info in InputMart.Instance.StdLotSize.DefaultView)
                {
                    if (info.STEP_ID != stepID)
                        continue;
                    if (info.LINE_ID != lineID)
                        continue;
                    if (LikeUtility.Like(productID, info.PRODUCT_ID) == false)
                        continue;

                    stdLotSize = (double)info.LOT_SIZE;
                    break;
                }

                return stdLotSize;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return 0;
            }
        }

        public static List<MicronBEAssyBELot> GetSplitLots(MicronBEAssyBELot lot, double stdLotSize)
        {
            try
            {
                List<MicronBEAssyBELot> splitLots = new List<MicronBEAssyBELot>();

                int splitQty = (int)Math.Ceiling(lot.UnitQtyDouble / stdLotSize);

                double unitQty = lot.UnitQtyDouble;

                for (int i = 1; i <= splitQty; i++)
                {
                    string splitLotID = GenerateSplitNo(lot, i);

                    double splitLotQty = stdLotSize < unitQty ? stdLotSize : unitQty;

                    MicronBEAssyBELot splitLot = lot.Clone() as MicronBEAssyBELot;
                    splitLot.LotID = splitLotID;

                    splitLot.UnitQtyDouble = splitLotQty;

                    MicronBEAssyPlanInfo plan = new MicronBEAssyPlanInfo();

                    plan.Init(splitLot.CurrentStep);
                    plan.LotID = splitLot.LotID;
                    plan.UnitQty = splitLot.UnitQtyDouble;
                    plan.ProductID = splitLot.Product.ProductID;
                    plan.ProcessID = splitLot.Process.ProcessID;

                    splitLot.SetCurrentPlan(plan);

                    unitQty -= splitLotQty;

                    splitLots.Add(splitLot);

                }

                return splitLots;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        public static List<MicronBEAssyBELot> MatchingMcpLot(DispatchingAgent da, Tuple<AssyMcpProduct, AssyMcpPart> baseKey, Tuple<AssyMcpProduct, AssyMcpPart> sourceKey)
        {
            List<MicronBEAssyBELot> matchLotList = new List<MicronBEAssyBELot>();

            return matchLotList;

            //try
            //{
            //    List<MicronBEAssyBELot> matchLotList = new List<MicronBEAssyBELot>();

            //    ICollection<MicronBEAssyBELot> baseLotList;
            //    if (InputMart.Instance.MatchingLotList.TryGetValue(baseKey, out baseLotList) == false)
            //        return matchLotList;

            //    ICollection<MicronBEAssyBELot> sourceLotList;
            //    if (InputMart.Instance.MatchingLotList.TryGetValue(sourceKey, out sourceLotList) == false)
            //        return matchLotList;

            //    List<MicronBEAssyBELot> baseLots = new List<MicronBEAssyBELot>(baseLotList);

            //    foreach (MicronBEAssyBELot baseLot in baseLots)
            //    {
            //        double sourceQty = sourceLotList.Sum(x => x.UnitQtyDouble);

            //        if (baseLot.UnitQtyDouble <= sourceQty)
            //        {
            //            baseLotList.Remove(baseLot);
            //            matchLotList.Add(baseLot);

            //            double baseRemainQty = baseLot.UnitQtyDouble;
            //            List<MicronBEAssyBELot> sourceLots = new List<MicronBEAssyBELot>(sourceLotList);

            //            foreach (MicronBEAssyBELot sourceLot in sourceLots)
            //            {
            //                double matchQty = 0d;
            //                double matchCompQty = 0d;
            //                AssyMcpPart mcpPart = FindHelper.FindProduct(sourceLot.Product.ProductID) as AssyMcpPart;

            //                if (sourceLot.UnitQtyDouble <= baseRemainQty)
            //                {
            //                    matchCompQty = mcpPart.CompQty;
            //                    matchQty = sourceLot.UnitQtyDouble;
            //                    baseRemainQty -= matchQty * matchCompQty;
            //                    sourceLot.UnitQtyDouble = 0d;

            //                    sourceLotList.Remove(sourceLot);
            //                    da.Factory.WipManager.Out(sourceLot);
            //                }
            //                else
            //                {
            //                    matchCompQty = mcpPart.CompQty;
            //                    matchQty = baseRemainQty;
            //                    sourceLot.UnitQtyDouble -= matchQty * matchCompQty;
            //                    baseRemainQty = 0d;
            //                }

            //                SimulationHelper.UpdateEqpPlanQty(sourceLot, string.Empty, LoadState.WAIT.ToString(), sourceLot.UnitQtyDouble);

            //                var part = FindHelper.FindProduct(baseLot.Product.ProductID) as AssyMcpPart;

            //                if (part != null)
            //                {
            //                    if (part.Next == null)
            //                        baseLot.Product = part.OutProduct;
            //                    else
            //                        baseLot.Product = part.Next;
            //                }

            //                //baseLot.Route = baseLot.Product.Process;
            //                MicronBEAssyPlanInfo plan = new MicronBEAssyPlanInfo();

            //                plan.Init(baseLot.Product.Process.FindStep(baseLot.CurrentStep.StepID));
            //                plan.LotID = baseLot.LotID;
            //                plan.UnitQty = baseLot.UnitQtyDouble;
            //                plan.ProductID = baseLot.Product.ProductID;
            //                plan.ProcessID = baseLot.Product.Process.ProcessID;

            //                baseLot.SetCurrentPlan(plan);

            //                if (baseRemainQty <= 0d)
            //                    break;
            //            }
            //        }
            //    }

            //    return matchLotList;
            //}
            //catch (Exception e)
            //{
            //    WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            //    return default(List<MicronBEAssyBELot>);
            //}
        }

        public static void UpdateEqpPlanQty(MicronBEAssyBELot lot, string eqpID, string status, double qty)
        {
            try
            {
                string key = SimulationHelper.GetEqpPlanKey(lot, eqpID, status);

                EqpPlan eqpPlan = null;
                if (InputMart.Instance.EqpPlans.TryGetValue(key, out eqpPlan))
                    eqpPlan.QTY = Convert.ToDecimal(qty);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        public static string GenerateSplitNo(MicronBEAssyBELot lot, int i)
        {
            try
            {
                string splitLotID;
                if (i <= 9)
                    splitLotID = lot.LotID + "_" + i.ToString();
                else if (i > 9 && i <= 35)
                    splitLotID = lot.LotID + "_" + (char)(i + 55);
                else if (i > 35 && i <= 61)
                    splitLotID = lot.LotID + "_" + (char)(i + 61);
                else
                    splitLotID = lot.LotID + "_" + (i + 61).ToString();
                return splitLotID;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(string);
            }
        }

        public static string GetEqpPlanKey(MicronBEAssyBELot lot, string eqpID, string status)
        {
            try
            {
                return string.Concat(lot.LotID, lot.CurrentStepID, eqpID, status);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(string);
            }
        }

        public static List<string> GetLoadableList(MicronBEAssyBEStep step)
        {
            try
            {
                List<string> list = new List<string>();

                foreach (MicronBEAssyEqp eqp in InputMart.Instance.MicronBEAssyEqp.Values)
                {
                    //if (step.StepGroup != eqp.StepGroup)
                    //    continue;

                    list.Add(eqp.EqpID);
                }

                return list;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(List<string>);
            }
        }

        public static double GetTactTime(string lineID, string stepID, string productID, MicronBEAssyEqp eqp)
        {
            try
            {
                Tuple<string, string, string> key = Tuple.Create(lineID, stepID, productID);
                double tactTime = 0d;
                if (InputMart.Instance.TactTimeCache.TryGetValue(key, out tactTime))
                    return tactTime;
                else
                {
                    foreach (StepTime st in InputMart.Instance.StepTime.DefaultView)
                    {
                        if (st.LINE_ID != lineID)
                            continue;

                        if (st.STEP_ID != stepID)
                            continue;

                        if (LikeUtility.Like(eqp.EqpID, st.EQP_ID) == false)
                            continue;

                        if (LikeUtility.Like(productID, st.PRODUCT_ID) == false)
                            continue;

                        tactTime = (double)st.TACT_TIME;
                        break;
                    }

                    InputMart.Instance.TactTimeCache.Add(key, tactTime);
                    return tactTime;
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(double);
            }
        }

        public static bool IsNeedSetupProfile(WorkEqp weqp, IHandlingBatch hb)
        {
            try
            {
                MicronBEAssyEqp eqp = null;
                if (InputMart.Instance.MicronBEAssyEqp.ContainsKey(weqp.Target.EqpID))
                    eqp = InputMart.Instance.MicronBEAssyEqp[weqp.Target.EqpID];

                if (eqp == null)
                    return false;

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                BEStep step = lot.CurrentStep;
                StepMaster sm = InputMart.Instance.StepMasterView.FindRows(step.StepID).FirstOrDefault();

                if (sm == null)
                    return false;

                foreach (var info in InputMart.Instance.SetupInfo.DefaultView)
                {
                    if (info.LINE_ID != eqp.LineID)
                        continue;

                    //if (UtilityHelper.StringToEnum(info.STEP_GROUP, StepGroup.NONE) != eqp.StepGroup)
                    //    continue;

                    //if (LikeUtility.Like(eqp.EqpModel, info.EQP_MODEL) == false)
                    //    continue;

                    SetupType type = SimulationHelper.CheckSetupProfile(weqp, hb);

                    if (type != SetupType.NONE)
                        return true;
                    else
                        return false;
                }

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        public static Time GetSetupTimeProfile(WorkEqp weqp, IHandlingBatch hb)
        {
            try
            {
                MicronBEAssyEqp eqp = null;
                if (InputMart.Instance.MicronBEAssyEqp.ContainsKey(weqp.Target.EqpID))
                    eqp = InputMart.Instance.MicronBEAssyEqp[weqp.Target.EqpID];

                if (eqp == null)
                    return default(Time);

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                BEStep step = lot.CurrentStep;
                StepMaster sm = InputMart.Instance.StepMasterView.FindRows(step.StepID).FirstOrDefault();

                foreach (var info in InputMart.Instance.SetupInfo.DefaultView)
                {
                    if (info.LINE_ID != sm.LINE_ID)
                        continue;

                    //if (UtilityHelper.StringToEnum(info.STEP_GROUP, StepGroup.NONE) != eqp.StepGroup)
                    //    continue;

                    //if (LikeUtility.Like(eqp.EqpModel, info.EQP_MODEL) == false)
                    //    continue;

                    double setupTimeBySec = (double)info.SETUP_TIME;
                    return Time.FromSeconds(setupTimeBySec);
                }

                return default(Time);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Time);
            }
        }

        public static SetupType CheckSetupProfile(WorkEqp weqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;
                var workLots = weqp.Step.Profiles[weqp];
                var lastLot = workLots.Last().Lot as MicronBEAssyBELot;

                if (lastLot.Product.ProductID != lot.Product.ProductID)
                    return SetupType.PART_CHG;

                return SetupType.NONE;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return SetupType.NONE;
            }
        }

        public static int CalculateProperEqpCount(AssignEqp assignEqp)
        {
            WorkStep workStep = assignEqp.WorkStep;
            int loadedEqpCount = workStep.LoadedEqpCount;
            DoubleDictionary<Step, object, StepPlan> stepPlans = StepPlanManager.Current.RunPlans;
            DateTime nowDt = FindHelper.GetNowDT();

            foreach (var stepPlan in stepPlans)
            {
                if (stepPlan.Step.StepID != workStep.Key.ToString())
                    continue;

                if (stepPlan.StepTargetList.Count == 0)
                    continue;

                Mozart.SeePlan.DataModel.StepTarget target = stepPlan.StepTargetList.First();
                Tuple<string, string, string> key = target.Key as Tuple<string, string, string>;
                string lineID = key.Item1;
                string stepID = key.Item2;
                string prodID = key.Item3;

                double remainTargetQty = target.CurrentQty;

#if DEBUG
                if (stepID == "S0100" && prodID == "ASSY_A01")
                    Console.WriteLine();
#endif
                TimeSpan remainTime = target.DueDate - nowDt;
                double remainSec = remainTime.TotalSeconds;

                int eqpCount = 0;
                double currAvailableQty = 0d;
                var loadedEqps = workStep.LoadedEqps;
                foreach (var loadedEqp in loadedEqps)
                {
                    if (currAvailableQty >= remainTargetQty)
                        break;

                    MicronBEAssyEqp eqp = FindHelper.FindEquipment(loadedEqp.Target.EqpID);
                    double tactTime = GetTactTime(lineID, stepID, prodID, eqp);

                    currAvailableQty += remainSec / tactTime;
                    eqpCount++;
                }

                return eqpCount;
            }

            return 0;
        }

        public static bool CanFilterAssignEqp(AssignEqp assignEqp)
        {
            MicronBEAssyWorkStep wstep = assignEqp.WorkStep as MicronBEAssyWorkStep;
            DateTime availableDownTime = wstep.AvailableDownTime;
            DateTime nowDt = FindHelper.GetNowDT();

            if (nowDt < availableDownTime)
                return false;

            foreach (var wip in wstep.Wips)
            {
                var lot = wip.Lot as MicronBEAssyBELot;
                if (lot.ReservationEqp != null && lot.ReservationEqp == assignEqp.Target)
                    return false;
            }

            return true;
        }

        public static DateTime CalculateAvailableDownTime(WorkStep wstep, AssignEqp assignEqp, ref double setupTime)
        {
            try
            {
                MicronBEAssyWorkStep ws = wstep as MicronBEAssyWorkStep;

                DateTime nowDt = FindHelper.GetNowDT();
                DateTime availableDownTime = nowDt.AddSeconds(wstep.NewUpInterval.TotalSeconds);

                var wip = ws.Wips.FirstOrDefault();
                if (wip == null)
                    return availableDownTime;

                WorkEqp weqp = null;
                foreach (var item in ws.LoadedEqps)
                {
                    if (item.Target == assignEqp.Target)
                    {
                        weqp = item;
                        break;
                    }

                }

                var setupControl = ServiceLocator.Resolve<SetupControl>();
                var processControl = ServiceLocator.Resolve<ProcessControl>();

                var beLot = wip.Lot as MicronBEAssyBELot;

                bool isNeedSetup = false;
                setupTime = 0d;
                ICollection<WorkLot> lastWorkLots = null;

                if (weqp.Step != null)
                    lastWorkLots = weqp.Step.Profiles[weqp];

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

                availableDownTime = availableDownTime.AddSeconds(setupTime);
                return availableDownTime;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(DateTime);
            }
        }
    }
}
