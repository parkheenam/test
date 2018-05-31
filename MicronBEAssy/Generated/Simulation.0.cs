using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Mapping;
using Mozart.Data;
using Mozart.Data.Entity;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.Optimization;
using Mozart.SeePlan.StatModel;

namespace MicronBEAssy
{
    
    /// <summary>
    /// Simulation execution model class
    /// </summary>
    public partial class Simulation_Module : ExecutionModule
    {
        public override string Name
        {
            get
            {
                return "Simulation";
            }
        }
        protected override System.Type ExecutorType
        {
            get
            {
                return typeof(Mozart.SeePlan.Simulation.LoadingSimulator);
            }
        }
        protected override void Configure()
        {
            Mozart.Task.Execution.ServiceLocator.RegisterController(new EqpInitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new BucketInitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new FactoryInitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new FactoryEventsImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new WipInitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new InputBatchInitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new RouteImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new ForwardPegImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new InOutControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new MergeControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new TransferControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new TransferSystemInterfaceImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new QueueControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new FilterControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new DispatcherControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new ProcessControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new SetupControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new DownControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new MiscImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new EqpEventsImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new BucketControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new BucketEventsImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new ToolControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new ToolEventsImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new AgentInitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new JobProfileControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new JobChangeControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new JobTradeControlImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new JobChangeEventsImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new InitImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new RunImpl());
            new StatSheetsImpl().Configure();
            new WeightsImpl().Configure();
            new CustomEventsImpl().Configure();
            new FiltersImpl().Configure();
        }
        /// <summary>
        /// EqpInit execution control implementation
        /// </summary>
        internal partial class EqpInitImpl : Mozart.SeePlan.Simulation.EquipmentInitiator
        {
            private MicronBEAssy.Logic.Simulation.EqpInit fEqpInit = new MicronBEAssy.Logic.Simulation.EqpInit();
            protected override System.Collections.Generic.IEnumerable<Mozart.SeePlan.DataModel.Resource> GetEqpList()
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.DataModel.Resource> returnValue = null;
                returnValue = this.fEqpInit.GET_EQP_LIST0(ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.FactoryLogic fpFactoryLogic = new Mozart.SeePlan.Simulation.FactoryLogic();
            protected override string GetDispatcherType(Mozart.SeePlan.DataModel.Resource eqp)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fpFactoryLogic.GET_DISPATCHER_TYPE_DEF(eqp, ref handled, returnValue);
                return returnValue;
            }
            protected override string GetEqpSimType(Mozart.SeePlan.DataModel.Resource eqp)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fpFactoryLogic.GET_EQP_SIM_TYPE_DEF(eqp, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// BucketInit execution control implementation
        /// </summary>
        internal partial class BucketInitImpl : Mozart.SeePlan.Simulation.BucketInit
        {
        }
        /// <summary>
        /// FactoryInit execution control implementation
        /// </summary>
        internal partial class FactoryInitImpl : Mozart.SeePlan.Simulation.FactoryInit
        {
        }
        /// <summary>
        /// FactoryEvents execution control implementation
        /// </summary>
        internal partial class FactoryEventsImpl : Mozart.SeePlan.Simulation.FactoryEvents
        {
            private MicronBEAssy.Logic.Simulation.FactoryEvents fFactoryEvents = new MicronBEAssy.Logic.Simulation.FactoryEvents();
            public override void OnDone(Mozart.SeePlan.Simulation.AoFactory aoFactory)
            {
                bool handled = false;
                this.fFactoryEvents.ON_DONE0(aoFactory, ref handled);
            }
        }
        /// <summary>
        /// WipInit execution control implementation
        /// </summary>
        internal partial class WipInitImpl : Mozart.SeePlan.Simulation.WipInitiator
        {
            private MicronBEAssy.Logic.Simulation.WipInit fWipInit = new MicronBEAssy.Logic.Simulation.WipInit();
            protected override System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> GetWips()
            {
                bool handled = false;
                System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> returnValue = null;
                returnValue = this.fWipInit.GET_WIPS0(ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.EntityLogic fpEntityLogic = new Mozart.SeePlan.Simulation.EntityLogic();
            protected override int CompareWip(Mozart.SeePlan.Simulation.IHandlingBatch x, Mozart.SeePlan.Simulation.IHandlingBatch y)
            {
                bool handled = false;
                int returnValue = 0;
                returnValue = this.fpEntityLogic.COMPARE_WIP_DEF(x, y, ref handled, returnValue);
                return returnValue;
            }
            protected override void LocateForDispatch(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fpEntityLogic.LOCATE_FOR_DISPATCH_DEF(factory, hb, ref handled);
            }
            protected override void LocateForRun(Mozart.SeePlan.Simulation.AoFactory factory, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fpEntityLogic.LOCATE_FOR_RUN_DEF(factory, hb, ref handled);
            }
            public override string GetLoadingEquipment(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fWipInit.GET_LOADING_EQUIPMENT0(hb, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.Simulation.Engine.ISimEntity FixBatch(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.Simulation.Engine.ISimEntity entity)
            {
                bool handled = false;
                Mozart.Simulation.Engine.ISimEntity returnValue = null;
                returnValue = this.fpEntityLogic.FIX_BATCH_DEF(aeqp, entity, ref handled, returnValue);
                return returnValue;
            }
            public override System.DateTime FixStartTime(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                System.DateTime returnValue = default(System.DateTime);
                returnValue = this.fpEntityLogic.FIX_START_TIME_DEF(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// InputBatchInit execution control implementation
        /// </summary>
        internal partial class InputBatchInitImpl : Mozart.SeePlan.Simulation.BatchInitiator
        {
            private MicronBEAssy.Logic.Simulation.InputBatchInit fInputBatchInit = new MicronBEAssy.Logic.Simulation.InputBatchInit();
            protected override System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.ILot> Instancing()
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.ILot> returnValue = null;
                returnValue = this.fInputBatchInit.INSTANCING0(ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.EntityLogic fpEntityLogic = new Mozart.SeePlan.Simulation.EntityLogic();
            protected override int CompareLot(Mozart.SeePlan.Simulation.ILot x, Mozart.SeePlan.Simulation.ILot y)
            {
                bool handled = false;
                int returnValue = 0;
                returnValue = this.fpEntityLogic.COMPARE_LOT_DEF(x, y, ref handled, returnValue);
                return returnValue;
            }
            protected override void DoReserve(System.Collections.Generic.List<Mozart.SeePlan.Simulation.ILot> lots)
            {
                bool handled = false;
                this.fpEntityLogic.DO_RESERVE_DEF(lots, ref handled);
            }
            public override void ReserveOne(System.Collections.Generic.List<Mozart.SeePlan.Simulation.ILot> lots, ref int index)
            {
                bool handled = false;
                this.fpEntityLogic.RESERVE_ONE_DEF(lots, ref index, ref handled);
            }
        }
        /// <summary>
        /// Route execution control implementation
        /// </summary>
        internal partial class RouteImpl : Mozart.SeePlan.Simulation.EntityControl
        {
            private MicronBEAssy.Logic.Simulation.Route fRoute = new MicronBEAssy.Logic.Simulation.Route();
            public override void OnDispatchIn(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fRoute.ON_DISPATCH_IN0(da, hb, ref handled);
            }
            public override System.Collections.Generic.IList<string> GetLoadableEqpList(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                System.Collections.Generic.IList<string> returnValue = null;
                returnValue = this.fRoute.GET_LOADABLE_EQP_LIST0(da, hb, ref handled, returnValue);
                return returnValue;
            }
            public override void OnStartTask(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.Simulation.Engine.ActiveObject ao, System.DateTime now)
            {
                bool handled = false;
                this.fRoute.ON_START_TASK0(hb, ao, now, ref handled);
            }
            public override void OnEndTask(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.Simulation.Engine.ActiveObject ao, System.DateTime now)
            {
                bool handled = false;
                this.fRoute.ON_END_TASK0(hb, ao, now, ref handled);
            }
            private Mozart.SeePlan.Simulation.EntityLogic fpEntityLogic = new Mozart.SeePlan.Simulation.EntityLogic();
            public override Mozart.SeePlan.DataModel.Step GetNextStep(Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.LoadInfo loadInfo, Mozart.SeePlan.DataModel.Step step, System.DateTime now)
            {
                bool handled = false;
                Mozart.SeePlan.DataModel.Step returnValue = null;
                returnValue = this.fpEntityLogic.GET_NEXT_STEP_DEF(lot, loadInfo, step, now, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.DataModel.LoadInfo CreateLoadInfo(Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.Step task)
            {
                bool handled = false;
                Mozart.SeePlan.DataModel.LoadInfo returnValue = null;
                returnValue = this.fRoute.CREATE_LOAD_INFO0(lot, task, ref handled, returnValue);
                return returnValue;
            }
            public override bool IsDone(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpEntityLogic.IS_DONE_DEF(hb, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// ForwardPeg execution control implementation
        /// </summary>
        internal partial class ForwardPegImpl : Mozart.SeePlan.Simulation.ForwardPeg
        {
        }
        /// <summary>
        /// InOutControl execution control implementation
        /// </summary>
        internal partial class InOutControlImpl : Mozart.SeePlan.Simulation.InOutControl
        {
        }
        /// <summary>
        /// MergeControl execution control implementation
        /// </summary>
        internal partial class MergeControlImpl : Mozart.SeePlan.Simulation.EntityMergeControl
        {
        }
        /// <summary>
        /// TransferControl execution control implementation
        /// </summary>
        internal partial class TransferControlImpl : Mozart.SeePlan.Simulation.TransferControl
        {
            private MicronBEAssy.Logic.Simulation.TransferControl fTransferControl = new MicronBEAssy.Logic.Simulation.TransferControl();
            public override Mozart.Simulation.Engine.Time GetTransferTime(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                Mozart.Simulation.Engine.Time returnValue = default(Mozart.Simulation.Engine.Time);
                returnValue = this.fTransferControl.GET_TRANSFER_TIME0(hb, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// TransferSystemInterface execution control implementation
        /// </summary>
        internal partial class TransferSystemInterfaceImpl : Mozart.SeePlan.Simulation.TransferExtControl
        {
            private Mozart.SeePlan.Simulation.TransferLogic fpTransferLogic = new Mozart.SeePlan.Simulation.TransferLogic();
            public override void OnDelivered(Mozart.SeePlan.Simulation.TransportAdapter ta, string key, string sourceLocation, string targetLocation)
            {
                bool handled = false;
                this.fpTransferLogic.ON_DELIVERED_DEF(ta, key, sourceLocation, targetLocation, ref handled);
            }
        }
        /// <summary>
        /// QueueControl execution control implementation
        /// </summary>
        internal partial class QueueControlImpl : Mozart.SeePlan.Simulation.QueueControl
        {
            private MicronBEAssy.Logic.Simulation.QueueControl fQueueControl = new MicronBEAssy.Logic.Simulation.QueueControl();
            public override bool InterceptIn(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fQueueControl.INTERCEPT_IN0(da, hb, ref handled, returnValue);
                return returnValue;
            }
            public override bool IsBucketProcessing(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fQueueControl.IS_BUCKET_PROCESSING0(da, hb, ref handled, returnValue);
                return returnValue;
            }
            public override void OnNotFoundDestination(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb, int destCount)
            {
                bool handled = false;
                this.fQueueControl.ON_NOT_FOUND_DESTINATION0(da, hb, destCount, ref handled);
            }
        }
        /// <summary>
        /// FilterControl execution control implementation
        /// </summary>
        internal partial class FilterControlImpl : Mozart.SeePlan.Simulation.DispatchFilterControl
        {
            private MicronBEAssy.Logic.Simulation.FilterControl fFilterControl = new MicronBEAssy.Logic.Simulation.FilterControl();
            public override bool IsPreventDispatching(Mozart.SeePlan.Simulation.AoEquipment aeqp, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.Simulation.Engine.Time waitDownTime)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fFilterControl.IS_PREVENT_DISPATCHING0(aeqp, wips, waitDownTime, ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.DispatchingLogic fpDispatchingLogic = new Mozart.SeePlan.Simulation.DispatchingLogic();
            public override System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> DoFilter(Mozart.SeePlan.Simulation.AoEquipment eqp, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IDispatchContext ctx)
            {
                bool handled = false;
                System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> returnValue = null;
                returnValue = this.fpDispatchingLogic.DO_FILTER_DEF(eqp, wips, ctx, ref handled, returnValue);
                if (handled)
                {
                    return returnValue;
                }
                returnValue = this.fFilterControl.DO_FILTER1(eqp, wips, ctx, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// DispatcherControl execution control implementation
        /// </summary>
        internal partial class DispatcherControlImpl : Mozart.SeePlan.Simulation.DispatchControl
        {
            private Mozart.SeePlan.Simulation.DispatchingLogic fpDispatchingLogic = new Mozart.SeePlan.Simulation.DispatchingLogic();
            public override System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> SortLotGroupContents(Mozart.SeePlan.Simulation.DispatcherBase db, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> list, Mozart.SeePlan.Simulation.IDispatchContext ctx)
            {
                bool handled = false;
                System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> returnValue = null;
                returnValue = this.fpDispatchingLogic.SORT_LOT_GROUP_CONTENTS_DEF(db, list, ctx, ref handled, returnValue);
                return returnValue;
            }
            private MicronBEAssy.Logic.Simulation.DispatcherControl fDispatcherControl = new MicronBEAssy.Logic.Simulation.DispatcherControl();
            public override System.Type GetLotBatchType()
            {
                bool handled = false;
                System.Type returnValue = null;
                returnValue = this.fDispatcherControl.GET_LOT_BATCH_TYPE0(ref handled, returnValue);
                return returnValue;
            }
            public override void OnDispatch(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.AoEquipment aeqp, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IEntityDispatcher dispatcher)
            {
                bool handled = false;
                this.fDispatcherControl.ON_DISPATCH0(da, aeqp, wips, dispatcher, ref handled);
            }
            public override Mozart.SeePlan.Simulation.IHandlingBatch[] DoSelect(Mozart.SeePlan.Simulation.DispatcherBase db, Mozart.SeePlan.Simulation.AoEquipment aeqp, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IDispatchContext ctx)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.IHandlingBatch[] returnValue = null;
                returnValue = this.fpDispatchingLogic.DO_SELECT_DEF(db, aeqp, wips, ctx, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> Evaluate(Mozart.SeePlan.Simulation.DispatcherBase db, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IDispatchContext ctx)
            {
                bool handled = false;
                System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> returnValue = null;
                returnValue = this.fDispatcherControl.EVALUATE0(db, wips, ctx, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.Simulation.IHandlingBatch[] Select(Mozart.SeePlan.Simulation.DispatcherBase db, Mozart.SeePlan.Simulation.AoEquipment aeqp, System.Collections.Generic.IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.IHandlingBatch[] returnValue = null;
                returnValue = this.fpDispatchingLogic.SELECT_DEF(db, aeqp, wips, ref handled, returnValue);
                return returnValue;
            }
            public override string AddDispatchWipLog(Mozart.SeePlan.DataModel.Resource eqp, Mozart.SeePlan.Simulation.EntityDispatchInfo info, Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.WeightPreset wp)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fpDispatchingLogic.ADD_DISPATCH_WIP_LOG_DEF(eqp, info, lot, wp, ref handled, returnValue);
                return returnValue;
            }
            public override string GetSelectedWipLog(Mozart.SeePlan.DataModel.Resource eqp, Mozart.SeePlan.Simulation.IHandlingBatch[] sels)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fpDispatchingLogic.GET_SELECTED_WIP_LOG_DEF(eqp, sels, ref handled, returnValue);
                return returnValue;
            }
            public override void OnDispatched(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch[] wips)
            {
                bool handled = false;
                this.fDispatcherControl.ON_DISPATCHED0(da, aeqp, wips, ref handled);
            }
        }
        /// <summary>
        /// ProcessControl execution control implementation
        /// </summary>
        internal partial class ProcessControlImpl : Mozart.SeePlan.Simulation.EqpProcessControl
        {
            private MicronBEAssy.Logic.Simulation.ProcessControl fProcessControl = new MicronBEAssy.Logic.Simulation.ProcessControl();
            public override void OnCustomLoad(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fProcessControl.ON_CUSTOM_LOAD0(aeqp, hb, ref handled);
            }
            public override bool IsNeedSetup(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fProcessControl.IS_NEED_SETUP0(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.DataModel.ProcTimeInfo GetProcessTime(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                Mozart.SeePlan.DataModel.ProcTimeInfo returnValue = default(Mozart.SeePlan.DataModel.ProcTimeInfo);
                returnValue = this.fProcessControl.GET_PROCESS_TIME0(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.EquipmentLogic fpEquipmentLogic = new Mozart.SeePlan.Simulation.EquipmentLogic();
            public override double GetProcessUnitSize(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                double returnValue = 0D;
                returnValue = this.fpEquipmentLogic.GET_PROCESS_UNIT_SIZE_DEF(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
            public override string[] GetLoadableChambers(Mozart.SeePlan.Simulation.AoChamberProc2 cproc, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                string[] returnValue = null;
                returnValue = this.fpEquipmentLogic.GET_LOADABLE_CHAMBERS_DEF(cproc, hb, ref handled, returnValue);
                return returnValue;
            }
            public override void OnEntered(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.AoProcess proc, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fpEquipmentLogic.MODIFY_DOWN_SCHEDULE_DEF(aeqp, proc, hb, ref handled);
            }
        }
        /// <summary>
        /// SetupControl execution control implementation
        /// </summary>
        internal partial class SetupControlImpl : Mozart.SeePlan.Simulation.EqpSetupControl
        {
            private Mozart.SeePlan.Simulation.EquipmentLogic fpEquipmentLogic = new Mozart.SeePlan.Simulation.EquipmentLogic();
            public override string GetSetupCrewKey(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fpEquipmentLogic.GET_SETUP_CREW_KEY_DEF(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
            private MicronBEAssy.Logic.Simulation.SetupControl fSetupControl = new MicronBEAssy.Logic.Simulation.SetupControl();
            public override Mozart.Simulation.Engine.Time GetSetupTime(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                Mozart.Simulation.Engine.Time returnValue = default(Mozart.Simulation.Engine.Time);
                returnValue = this.fSetupControl.GET_SETUP_TIME0(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.DataModel.LoadInfo SetLastLoadingInfo(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                Mozart.SeePlan.DataModel.LoadInfo returnValue = null;
                returnValue = this.fpEquipmentLogic.SET_LAST_LOADING_INFO_DEF(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// DownControl execution control implementation
        /// </summary>
        internal partial class DownControlImpl : Mozart.SeePlan.Simulation.EqpDownControl
        {
            private Mozart.SeePlan.Simulation.EquipmentLogic fpEquipmentLogic = new Mozart.SeePlan.Simulation.EquipmentLogic();
            public override void OnPMEvent(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.DataModel.PMSchedule fs, Mozart.SeePlan.Simulation.DownEventType det)
            {
                bool handled = false;
                this.fpEquipmentLogic.ON_PM_EVENT_DEF(aeqp, fs, det, ref handled);
            }
            public override void OnFailureEvent(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.DataModel.FailureSchedule fs, Mozart.SeePlan.Simulation.DownEventType det)
            {
                bool handled = false;
                this.fpEquipmentLogic.ON_FAILURE_EVENT_DEF(aeqp, fs, det, ref handled);
            }
        }
        /// <summary>
        /// Misc execution control implementation
        /// </summary>
        internal partial class MiscImpl : Mozart.SeePlan.Simulation.EqpMisc
        {
            private Mozart.SeePlan.Simulation.EquipmentLogic fpEquipmentLogic = new Mozart.SeePlan.Simulation.EquipmentLogic();
            public override bool IsBatchType(Mozart.SeePlan.Simulation.AoEquipment aeqp)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpEquipmentLogic.IsBatchType(aeqp, ref handled, returnValue);
                return returnValue;
            }
            private MicronBEAssy.Logic.Simulation.Misc fMisc = new MicronBEAssy.Logic.Simulation.Misc();
            public override bool UseCustomLoad(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fMisc.USE_CUSTOM_LOAD0(aeqp, hb, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// EqpEvents execution control implementation
        /// </summary>
        internal partial class EqpEventsImpl : Mozart.SeePlan.Simulation.EqpEvents
        {
            private MicronBEAssy.Logic.Simulation.EqpEvents fEqpEvents = new MicronBEAssy.Logic.Simulation.EqpEvents();
            public override void LoadingStateChanged(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.LoadingStates state)
            {
                bool handled = false;
                this.fEqpEvents.LOADING_STATE_CHANGED0(aeqp, hb, state, ref handled);
            }
            public override void ProcessStateChanged(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.ProcessStates state)
            {
                bool handled = false;
                this.fEqpEvents.PROCESS_STATE_CHANGED0(aeqp, hb, state, ref handled);
            }
        }
        /// <summary>
        /// BucketControl execution control implementation
        /// </summary>
        internal partial class BucketControlImpl : Mozart.SeePlan.Simulation.BucketControl
        {
            private Mozart.SeePlan.Simulation.BucketingLogic fpBucketingLogic = new Mozart.SeePlan.Simulation.BucketingLogic();
            public override void AddBucketMove(Mozart.SeePlan.Simulation.CapacityBucket cb, Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fpBucketingLogic.ADD_BUCKET_MOVE_DEF(cb, hb, ref handled);
            }
            private MicronBEAssy.Logic.Simulation.BucketControl fBucketControl = new MicronBEAssy.Logic.Simulation.BucketControl();
            public override Mozart.Simulation.Engine.Time GetBucketTime(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.AoBucketer bucketer)
            {
                bool handled = false;
                Mozart.Simulation.Engine.Time returnValue = default(Mozart.Simulation.Engine.Time);
                returnValue = this.fBucketControl.GET_BUCKET_TIME0(hb, bucketer, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.Simulation.Engine.Time GetBucketInputDelay(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.AoBucketer bucketer)
            {
                bool handled = false;
                Mozart.Simulation.Engine.Time returnValue = default(Mozart.Simulation.Engine.Time);
                returnValue = this.fpBucketingLogic.GET_BUCKET_INPUT_DELAY_DEF(hb, bucketer, ref handled, returnValue);
                return returnValue;
            }
            public override void BucketRolling(Mozart.SeePlan.Simulation.CapacityBucket cb, System.DateTime now, bool atBoundary, bool atDayChanged)
            {
                bool handled = false;
                this.fpBucketingLogic.BUCKET_ROLLING_DEF(cb, now, atBoundary, atDayChanged, ref handled);
            }
        }
        /// <summary>
        /// BucketEvents execution control implementation
        /// </summary>
        internal partial class BucketEventsImpl : Mozart.SeePlan.Simulation.BucketEvents
        {
        }
        /// <summary>
        /// ToolControl execution control implementation
        /// </summary>
        internal partial class ToolControlImpl : Mozart.SeePlan.Simulation.ToolControl
        {
            private Mozart.SeePlan.Simulation.SecondResourceLogic fpSecondResourceLogic = new Mozart.SeePlan.Simulation.SecondResourceLogic();
            public override Mozart.SeePlan.Simulation.ToolSettings GetLastToolSettings(Mozart.SeePlan.Simulation.AoEquipment aeqp)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.ToolSettings returnValue = null;
                returnValue = this.fpSecondResourceLogic.GET_LAST_TOOL_SETTINGS_DEF(aeqp, ref handled, returnValue);
                return returnValue;
            }
            public override bool IsValidToolInfo(Mozart.SeePlan.Simulation.ToolSettings tool, Mozart.SeePlan.Simulation.ToolItem current)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpSecondResourceLogic.IS_VALID_TOOL_INFO_DEF(tool, current, ref handled, returnValue);
                return returnValue;
            }
            public override bool IsReadyTool(Mozart.SeePlan.Simulation.ToolSettings tool, Mozart.SeePlan.Simulation.ToolItem current, Mozart.SeePlan.Simulation.ToolItem last)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpSecondResourceLogic.IS_READY_TOOL_DEF(tool, current, last, ref handled, returnValue);
                return returnValue;
            }
            public override void AttachTool(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fpSecondResourceLogic.ATTACH_TOOL_DEF(hb, ref handled);
            }
            public override void DetachTool(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                this.fpSecondResourceLogic.DETACH_TOOL_DEF(hb, ref handled);
            }
        }
        /// <summary>
        /// ToolEvents execution control implementation
        /// </summary>
        internal partial class ToolEventsImpl : Mozart.SeePlan.Simulation.ToolEvents
        {
        }
        /// <summary>
        /// AgentInit execution control implementation
        /// </summary>
        internal partial class AgentInitImpl : Mozart.SeePlan.Simulation.JobChangeInit
        {
            private Mozart.SeePlan.Simulation.JobChangeAgentLogic fpJobChangeAgentLogic = new Mozart.SeePlan.Simulation.JobChangeAgentLogic();
            public override void InitializeWorkManager(Mozart.SeePlan.Simulation.WorkManager wmanager)
            {
                bool handled = false;
                this.fpJobChangeAgentLogic.INITIALIZE_WORK_MANAGER_DEF(wmanager, ref handled);
            }
            private MicronBEAssy.Logic.Simulation.AgentInit fAgentInit = new MicronBEAssy.Logic.Simulation.AgentInit();
            public override System.Collections.Generic.IEnumerable<string> GetWorkAgentNames(Mozart.SeePlan.Simulation.WorkManager wmanager)
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<string> returnValue = null;
                returnValue = this.fAgentInit.GET_WORK_AGENT_NAMES0(wmanager, ref handled, returnValue);
                return returnValue;
            }
            public override void InitializeAgent(Mozart.SeePlan.Simulation.WorkAgent wagent)
            {
                bool handled = false;
                this.fAgentInit.INITIALIZE_AGENT0(wagent, ref handled);
            }
            public override Mozart.SeePlan.Simulation.WorkStep AddWorkLot(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.WorkStep returnValue = null;
                returnValue = this.fAgentInit.ADD_WORK_LOT0(hb, ref handled, returnValue);
                return returnValue;
            }
            public override string GetWorkAgentName(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                string returnValue = null;
                returnValue = this.fAgentInit.GET_WORK_AGENT_NAME0(hb, ref handled, returnValue);
                return returnValue;
            }
            public override object GetWorkGroupKey(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.WorkAgent wagent)
            {
                bool handled = false;
                object returnValue = null;
                returnValue = this.fAgentInit.GET_WORK_GROUP_KEY0(hb, wagent, ref handled, returnValue);
                return returnValue;
            }
            public override object GetWorkStepKey(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.WorkGroup wgroup)
            {
                bool handled = false;
                object returnValue = null;
                returnValue = this.fAgentInit.GET_WORK_STEP_KEY0(hb, wgroup, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.DataModel.Step GetTargetStep(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.SeePlan.Simulation.WorkGroup wgroup, object wstepKey)
            {
                bool handled = false;
                Mozart.SeePlan.DataModel.Step returnValue = null;
                returnValue = this.fAgentInit.GET_TARGET_STEP0(hb, wgroup, wstepKey, ref handled, returnValue);
                return returnValue;
            }
            public override void InitializeWorkGroup(Mozart.SeePlan.Simulation.WorkGroup wgroup)
            {
                bool handled = false;
                this.fAgentInit.INITIALIZE_WORK_GROUP0(wgroup, ref handled);
            }
            public override void InitializeWorkStep(Mozart.SeePlan.Simulation.WorkStep wstep)
            {
                bool handled = false;
                this.fAgentInit.INITIALIZE_WORK_STEP0(wstep, ref handled);
            }
            public override System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.AoEquipment> GetLoadableEqps(Mozart.SeePlan.Simulation.WorkStep wstep)
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.AoEquipment> returnValue = null;
                returnValue = this.fAgentInit.GET_LOADABLE_EQPS0(wstep, ref handled, returnValue);
                return returnValue;
            }
            public override void InitializeWorkEqp(Mozart.SeePlan.Simulation.WorkEqp weqp)
            {
                bool handled = false;
                this.fAgentInit.INITIALIZE_WORK_EQP0(weqp, ref handled);
            }
        }
        /// <summary>
        /// JobProfileControl execution control implementation
        /// </summary>
        internal partial class JobProfileControlImpl : Mozart.SeePlan.Simulation.JobProfileControl
        {
            private MicronBEAssy.Logic.Simulation.JobProfileControl fJobProfileControl = new MicronBEAssy.Logic.Simulation.JobProfileControl();
            public override Mozart.SeePlan.Simulation.WorkEqp SelectProfileEqp(Mozart.SeePlan.Simulation.WorkLoader wl)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.WorkEqp returnValue = null;
                returnValue = this.fJobProfileControl.SELECT_PROFILE_EQP0(wl, ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.JobChangeAgentLogic fpJobChangeAgentLogic = new Mozart.SeePlan.Simulation.JobChangeAgentLogic();
            public override int CompareProfileEqp(Mozart.SeePlan.Simulation.WorkEqp x, Mozart.SeePlan.Simulation.WorkEqp y)
            {
                bool handled = false;
                int returnValue = 0;
                returnValue = this.fpJobChangeAgentLogic.COMPARE_PROFILE_EQP_DEF(x, y, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.List<Mozart.SeePlan.Simulation.WorkLot> DoFilterLot(Mozart.SeePlan.Simulation.WorkEqp weqp, Mozart.SeePlan.Simulation.WorkStep wstep, System.Collections.Generic.List<Mozart.SeePlan.Simulation.WorkLot> list)
            {
                bool handled = false;
                System.Collections.Generic.List<Mozart.SeePlan.Simulation.WorkLot> returnValue = null;
                returnValue = this.fJobProfileControl.DO_FILTER_LOT0(weqp, wstep, list, ref handled, returnValue);
                return returnValue;
            }
            public override void SortProfileLot(Mozart.SeePlan.Simulation.WorkStep wstep, Mozart.SeePlan.Simulation.WorkEqp weqp, System.Collections.Generic.List<Mozart.SeePlan.Simulation.WorkLot> list)
            {
                bool handled = false;
                this.fJobProfileControl.SORT_PROFILE_LOT0(wstep, weqp, list, ref handled);
            }
            public override double GetProfileLotQty(Mozart.SeePlan.Simulation.WorkLot wlot)
            {
                bool handled = false;
                double returnValue = 0D;
                returnValue = this.fJobProfileControl.GET_PROFILE_LOT_QTY0(wlot, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.WorkLot> Advance(Mozart.SeePlan.Simulation.WorkStep wstep, Mozart.SeePlan.Simulation.WorkLot wlot)
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.WorkLot> returnValue = null;
                returnValue = this.fJobProfileControl.ADVANCE0(wstep, wlot, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.Simulation.WorkStep UpdateFindStep(System.Collections.Generic.IList<Mozart.SeePlan.Simulation.WorkStep> list, Mozart.SeePlan.Simulation.WorkStep step, Mozart.SeePlan.Simulation.WorkLot lot)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.WorkStep returnValue = null;
                returnValue = this.fJobProfileControl.UPDATE_FIND_STEP0(list, step, lot, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.Simulation.WorkLot Update(Mozart.SeePlan.Simulation.WorkStep wstep, Mozart.SeePlan.Simulation.WorkLot wlot)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.WorkLot returnValue = null;
                returnValue = this.fJobProfileControl.UPDATE0(wstep, wlot, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.IHandlingBatch> GetMappingLot(Mozart.SeePlan.Simulation.IHandlingBatch hb)
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.IHandlingBatch> returnValue = null;
                returnValue = this.fJobProfileControl.GET_MAPPING_LOT0(hb, ref handled, returnValue);
                return returnValue;
            }
            public override System.TimeSpan GetTactTime(Mozart.SeePlan.Simulation.WorkEqp weqp, Mozart.SeePlan.Simulation.WorkLot wlot)
            {
                bool handled = false;
                System.TimeSpan returnValue = default(System.TimeSpan);
                returnValue = this.fJobProfileControl.GET_TACT_TIME0(weqp, wlot, ref handled, returnValue);
                return returnValue;
            }
            public override void OnBeginProfiling(Mozart.SeePlan.Simulation.WorkLoader wl, Mozart.Simulation.Engine.Time now)
            {
                bool handled = false;
                this.fJobProfileControl.ON_BEGIN_PROFILING0(wl, now, ref handled);
            }
            public override void OnEndProfiling(Mozart.SeePlan.Simulation.WorkLoader wl, Mozart.Simulation.Engine.Time now)
            {
                bool handled = false;
                this.fJobProfileControl.ON_END_PROFILING0(wl, now, ref handled);
            }
        }
        /// <summary>
        /// JobChangeControl execution control implementation
        /// </summary>
        internal partial class JobChangeControlImpl : Mozart.SeePlan.Simulation.JobChangeControl
        {
            private Mozart.SeePlan.Simulation.JobChangeAgentLogic fpJobChangeAgentLogic = new Mozart.SeePlan.Simulation.JobChangeAgentLogic();
            public override bool IsNeedDownStep(Mozart.SeePlan.Simulation.WorkStep ws)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpJobChangeAgentLogic.IS_NEED_DOWN_STEP_DEF(ws, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.AoEquipment> SelectDownEqp(Mozart.SeePlan.Simulation.WorkStep wstep)
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.AoEquipment> returnValue = null;
                returnValue = this.fpJobChangeAgentLogic.SELECT_DOWN_EQP_DEF(wstep, ref handled, returnValue);
                return returnValue;
            }
            public override bool IsNeedUpStep(Mozart.SeePlan.Simulation.WorkStep ws)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpJobChangeAgentLogic.IS_NEED_UP_STEP_DEF(ws, ref handled, returnValue);
                return returnValue;
            }
            public override bool CanUp(Mozart.SeePlan.Simulation.AoEquipment aeqp, Mozart.SeePlan.Simulation.WorkStep wstep)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fpJobChangeAgentLogic.CAN_UP_DEF(aeqp, wstep, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// JobTradeControl execution control implementation
        /// </summary>
        internal partial class JobTradeControlImpl : Mozart.SeePlan.Simulation.JobTradeControl
        {
            private MicronBEAssy.Logic.Simulation.JobTradeControl fJobTradeControl = new MicronBEAssy.Logic.Simulation.JobTradeControl();
            public override Mozart.SeePlan.Simulation.OperationType ClassifyOperationType(Mozart.SeePlan.Simulation.WorkStep step, Mozart.SeePlan.Simulation.JobChangeContext context, out object reason)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.OperationType returnValue = default(Mozart.SeePlan.Simulation.OperationType);
                returnValue = this.fJobTradeControl.CLASSIFY_OPERATION_TYPE0(step, context, out reason, ref handled, returnValue);
                return returnValue;
            }
            public override float CalculatePriority(Mozart.SeePlan.Simulation.WorkStep step, object reason, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                float returnValue = 0F;
                returnValue = this.fJobTradeControl.CALCULATE_PRIORITY0(step, reason, context, ref handled, returnValue);
                return returnValue;
            }
            private Mozart.SeePlan.Simulation.JobChangeAgentLogic fpJobChangeAgentLogic = new Mozart.SeePlan.Simulation.JobChangeAgentLogic();
            public override Mozart.SeePlan.Simulation.WorkStep SelectUpStep(System.Collections.Generic.List<Mozart.SeePlan.Simulation.WorkStep> upWorkSteps, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                Mozart.SeePlan.Simulation.WorkStep returnValue = null;
                returnValue = this.fpJobChangeAgentLogic.SELECT_UP_STEP_DEF(upWorkSteps, context, ref handled, returnValue);
                return returnValue;
            }
            public override int CompareUpStep(Mozart.SeePlan.Simulation.WorkStep x, Mozart.SeePlan.Simulation.WorkStep y)
            {
                bool handled = false;
                int returnValue = 0;
                returnValue = this.fpJobChangeAgentLogic.COMPARE_UP_STEP_PRIORITY_DEF(x, y, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> DoFilterAssignEqp(Mozart.SeePlan.Simulation.WorkStep upWorkStep, System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> assignEqps, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> returnValue = null;
                returnValue = this.fJobTradeControl.DO_FILTER_ASSIGN_EQP0(upWorkStep, assignEqps, context, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> SelectAssignEqp(Mozart.SeePlan.Simulation.WorkStep upWorkStep, System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> assignEqps, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> returnValue = null;
                returnValue = this.fpJobChangeAgentLogic.SELECT_ASSIGN_EQP_DEF(upWorkStep, assignEqps, context, ref handled, returnValue);
                return returnValue;
            }
            public override int CompareAssignEqp(Mozart.SeePlan.Simulation.AssignEqp x, Mozart.SeePlan.Simulation.AssignEqp y)
            {
                bool handled = false;
                int returnValue = 0;
                returnValue = this.fJobTradeControl.COMPARE_ASSIGN_EQP0(x, y, ref handled, returnValue);
                return returnValue;
            }
            public override bool CanAssignMore(Mozart.SeePlan.Simulation.WorkStep upWorkStep, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                bool returnValue = false;
                returnValue = this.fJobTradeControl.CAN_ASSIGN_MORE0(upWorkStep, context, ref handled, returnValue);
                return returnValue;
            }
            public override System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.AoEquipment> SelectDownEqp(Mozart.SeePlan.Simulation.WorkStep wstep, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                System.Collections.Generic.IEnumerable<Mozart.SeePlan.Simulation.AoEquipment> returnValue = null;
                returnValue = this.fJobTradeControl.SELECT_DOWN_EQP0(wstep, context, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// JobChangeEvents execution control implementation
        /// </summary>
        internal partial class JobChangeEventsImpl : Mozart.SeePlan.Simulation.JobChangeEvents
        {
            private MicronBEAssy.Logic.Simulation.JobChangeEvents fJobChangeEvents = new MicronBEAssy.Logic.Simulation.JobChangeEvents();
            public override void OnAfterAssignEqp(Mozart.SeePlan.Simulation.WorkStep step, System.Collections.Generic.List<Mozart.SeePlan.Simulation.AssignEqp> assignedEqps, Mozart.SeePlan.Simulation.JobChangeContext context)
            {
                bool handled = false;
                this.fJobChangeEvents.ON_AFTER_ASSIGN_EQP0(step, assignedEqps, context, ref handled);
            }
            public override void OnAfterRun(Mozart.SeePlan.Simulation.WorkAgent wagent)
            {
                bool handled = false;
                this.fJobChangeEvents.ON_AFTER_RUN0(wagent, ref handled);
            }
        }
        /// <summary>
        /// Init execution control implementation
        /// </summary>
        internal partial class InitImpl : Mozart.SeePlan.Optimization.InitializeOptimizerControl
        {
        }
        /// <summary>
        /// Run execution control implementation
        /// </summary>
        internal partial class RunImpl : Mozart.SeePlan.Optimization.RunOptimizerControl
        {
        }
        internal class StatSheetsImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
            }
        }
        internal class WeightsImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
            }
        }
        internal class CustomEventsImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
            }
        }
        internal class FiltersImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
            }
        }
    }
}
