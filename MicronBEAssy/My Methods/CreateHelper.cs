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
using MicronBEAssy.Inputs;
using Mozart.SeePlan.SemiBE.Pegging;
using Mozart.SeePlan.SemiBE.DataModel;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class CreateHelper
    {
        public static PlanWip CreatePlanWip(IWipInfo wip)
        {
            try
            {
                MicronBEAssyWipInfo wipInfo = wip as MicronBEAssyWipInfo;
                MicronBEAssyPlanWip planWip = new MicronBEAssyPlanWip(wipInfo);

                planWip.MapStep = wipInfo.InitialStep;
                planWip.AvailableTime = FindHelper.GetEngineStartTime();
                planWip.Product = wipInfo.Product;
                planWip.LotID = wipInfo.LotID;
                planWip.State = wipInfo.CurrentState.ToString();

                return planWip;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(PlanWip);
            }
        }

        public static MicronBEAssyWorkLot CreateWorkLot(IHandlingBatch hb, Mozart.Simulation.Engine.Time availableTime, object wstepKey, Mozart.SeePlan.DataModel.Step targetStep, AoEquipment reservationEqp)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                MicronBEAssyWorkLot wLot = new MicronBEAssyWorkLot(hb, availableTime, wstepKey, targetStep);

                wLot.LotID = lot.LotID;
                wLot.Product = lot.Product;
                wLot.UnitQty = lot.UnitQtyDouble;
                wLot.ReservationEqp = reservationEqp;

                return wLot;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(MicronBEAssyWorkLot);
            }
        }

        public static MicronBEAssyBatch CreateBatch(MicronBEAssyBELot lot)
        {
            try
            {
                AssyMcpProduct aoProd = lot.Product.GetAssyOutProduct();

                if (aoProd == null)
                    return null;

                string aoProdID = aoProd.ProductID;

                Tuple<string, string> key = Tuple.Create(aoProdID, lot.LineID);
                MicronBEAssyBatch batch;
                if (InputMart.Instance.MicronBEAssyBatch.TryGetValue(key, out batch) == false)
                {
                    batch = new MicronBEAssyBatch();
                    batch.AoProdID = aoProdID;
                    batch.LineID = lot.LineID;

                    SimulationHelper.SetSteps(batch, lot.Product);

                    InputMart.Instance.MicronBEAssyBatch.Add(key, batch);
                }

                return batch;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(MicronBEAssyBatch);
            }
        }

        public static MicronBEAssyBEStep CreateStep(ProcessStep processStep)
        {
            try
            {
                MicronBEAssyBEStep step = new MicronBEAssyBEStep();
                step.StepID = processStep.STEP_ID;
                step.Sequence = (int)(processStep.SEQUENCE);
                step.StepGroup = processStep.STEP_GROUP == null ? string.Empty : processStep.STEP_GROUP;

                return step;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(MicronBEAssyBEStep);
            }
        }

        public static MicronBEAssyPlanInfo CreatePlanInfo(ILot lot, Mozart.SeePlan.DataModel.Step task)
        {
            MicronBEAssyPlanInfo plan = new MicronBEAssyPlanInfo();
            MicronBEAssyBELot beLot = lot as MicronBEAssyBELot;

            plan.Init(task);
            plan.LotID = lot.LotID;
            plan.UnitQty = lot.UnitQty;
            plan.ProductID = beLot.Product.ProductID;
            plan.ProcessID = beLot.Process.ProcessID;

            return plan;
        }
    }
}
