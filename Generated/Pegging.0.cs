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
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan.SemiBE.Pegging;

namespace MicronBEAssy
{
    
    /// <summary>
    /// Pegging execution model class
    /// </summary>
    public partial class Pegging_Module : ExecutionModule
    {
        public override string Name
        {
            get
            {
                return "Pegging";
            }
        }
        protected override System.Type ExecutorType
        {
            get
            {
                return typeof(Mozart.SeePlan.Pegging.Pegger);
            }
        }
        protected override void Configure()
        {
            Mozart.Task.Execution.ServiceLocator.RegisterController(new ModelBuildImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new ModelRunImpl());
            new PegModelsImpl().Configure();
            Mozart.Task.Execution.ServiceLocator.RegisterController(new APPLY_ACTImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new APPLY_YIELDImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new BIN_BUFFERImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new BUILD_INPLANImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new CHANGE_PARTImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new FILTER_TARGETImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new KIT_PEGImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new MANIPULATE_DEMANDImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new PEG_WIPImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new PREPARE_TARGETImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new PREPARE_WIPImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new SHIFT_TATImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new SMOOTH_DEMANDImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new WRITE_TARGETImpl());
            Mozart.Task.Execution.ServiceLocator.RegisterController(new WRITE_UNPEGImpl());
            new RulesImpl().Configure();
            new RulePresetsImpl().Configure();
        }
        /// <summary>
        /// ModelBuild execution control implementation
        /// </summary>
        internal partial class ModelBuildImpl : Mozart.SeePlan.Pegging.PeggerModelBuildInterface
        {
        }
        /// <summary>
        /// ModelRun execution control implementation
        /// </summary>
        internal partial class ModelRunImpl : Mozart.SeePlan.Pegging.PeggerModelRunInterface
        {
        }
        internal class PegModelsImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.SeePlan.Pegging.IPeggerCfg2>(this.BuildCfg());
            }
            private MicronBEAssy.Logic.Pegging.Pegging fPegging = new MicronBEAssy.Logic.Pegging.Pegging();
            /// <summary>
            /// Pegging's Pegger Model
            /// </summary>
            private Mozart.SeePlan.Pegging.IPeggerModel2 BuildPegging()
            {
                Mozart.SeePlan.Pegging.PeggerModelBuilder2 Pegging = new Mozart.SeePlan.Pegging.PeggerModelBuilder2("Pegging");
                Mozart.SeePlan.Pegging.PeggerAreaBuilder2 area0 = new Mozart.SeePlan.Pegging.PeggerAreaBuilder2("Start", false);
                Mozart.SeePlan.Pegging.PeggerAreaBuilder2 area1 = new Mozart.SeePlan.Pegging.PeggerAreaBuilder2("PRODUCTION_LINE", false);
                Mozart.SeePlan.Pegging.PeggerNormalNodeBuilder node0 = new Mozart.SeePlan.Pegging.PeggerNormalNodeBuilder("Initialization");
                System.Collections.Generic.List<string> stgs0 = new System.Collections.Generic.List<string>();
                stgs0.Add("SmoothDemand");
                stgs0.Add("GenInitTarget");
                stgs0.Add("ApplyAct");
                stgs0.Add("InitWip");
                stgs0.Add("InitSupplyPlan");
                node0.SetStages(stgs0);
                area1.AddNode(node0);
                Mozart.SeePlan.Pegging.PeggerProcessNodeBuilder node1 = new Mozart.SeePlan.Pegging.PeggerProcessNodeBuilder("Pegging");
                node1.AddBlock("@S", "@S", Mozart.SeePlan.Pegging.BlockType.START);
                node1.AddNextBlockMap("@S", "WriteOutTarget");
                node1.AddFunction("@S", ((Mozart.SeePlan.Pegging.GetLastPeggingStepDelegate)(this.fPegging.GETLASTPEGGINGSTEP)));
                node1.AddBlock("WriteOutTarget", "WriteOutTarget", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("WriteOutTarget", "PegRunWip");
                node1.AddBlock("PegRunWip", "PegRunWip", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("PegRunWip", "ShiftRunTime");
                node1.AddBlock("ShiftRunTime", "ShiftRunTime", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("ShiftRunTime", "ApplyYield");
                node1.AddBlock("ApplyYield", "ApplyYield", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("ApplyYield", "ChangePart");
                node1.AddBlock("ChangePart", "ChangePart", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("ChangePart", "UpdateTargetInfo");
                node1.AddBlock("UpdateTargetInfo", "UpdateTargetInfo", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("UpdateTargetInfo", "WriteInTarget");
                node1.AddBlock("WriteInTarget", "WriteInTarget", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("WriteInTarget", "PegWaitWip");
                node1.AddBlock("PegWaitWip", "PegWaitWip", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("PegWaitWip", "ShiftWaitTime");
                node1.AddBlock("ShiftWaitTime", "ShiftWaitTime", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node1.AddNextBlockMap("ShiftWaitTime", "@E");
                node1.AddBlock("@E", "@E", Mozart.SeePlan.Pegging.BlockType.END);
                node1.AddFunction("@E", ((Mozart.SeePlan.Pegging.GetPrevPeggingStepDelegate)(this.fPegging.GETPREVPEGGINGSTEP)));
                area1.AddNode(node1);
                Mozart.SeePlan.Pegging.PeggerAreaBuilder2 area2 = new Mozart.SeePlan.Pegging.PeggerAreaBuilder2("DIE_BANK", false);
                Mozart.SeePlan.Pegging.PeggerProcessNodeBuilder node2 = new Mozart.SeePlan.Pegging.PeggerProcessNodeBuilder("Pegging");
                node2.AddBlock("@S", "@S", Mozart.SeePlan.Pegging.BlockType.START);
                node2.AddNextBlockMap("@S", "WriteOutTarget");
                node2.AddFunction("@S", ((Mozart.SeePlan.Pegging.GetLastPeggingStepDelegate)(this.fPegging.GETLASTPEGGINGSTEP)));
                node2.AddBlock("WriteOutTarget", "WriteOutTarget", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node2.AddNextBlockMap("WriteOutTarget", "KitPeg");
                node2.AddBlock("KitPeg", "KitPeg", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node2.AddNextBlockMap("KitPeg", "WriteInTarget");
                node2.AddBlock("WriteInTarget", "WriteInTarget", Mozart.SeePlan.Pegging.BlockType.STAGE);
                node2.AddNextBlockMap("WriteInTarget", "@E");
                node2.AddBlock("@E", "@E", Mozart.SeePlan.Pegging.BlockType.END);
                node2.AddFunction("@E", ((Mozart.SeePlan.Pegging.GetPrevPeggingStepDelegate)(this.fPegging.GETPREVPEGGINGSTEP)));
                area2.AddNode(node2);
                Mozart.SeePlan.Pegging.PeggerNormalNodeBuilder node3 = new Mozart.SeePlan.Pegging.PeggerNormalNodeBuilder("Record");
                System.Collections.Generic.List<string> stgs3 = new System.Collections.Generic.List<string>();
                stgs3.Add("WriteUnpeg");
                node3.SetStages(stgs3);
                area2.AddNode(node3);
                area1.AddNext(area2);
                area0.AddNext(area1);
                Pegging.SetStart(area0);
                return Pegging.Build();
            }
            /// <summary>
            /// Pegger Configuration
            /// </summary>
            private Mozart.SeePlan.Pegging.IPeggerCfg2 BuildCfg()
            {
                Mozart.SeePlan.Pegging.PeggerCfgBuilder2 cfg = new Mozart.SeePlan.Pegging.PeggerCfgBuilder2();
                cfg.AddModel(this.BuildPegging(), true);
                return cfg.Build();
            }
        }
        /// <summary>
        /// APPLY_ACT execution control implementation
        /// </summary>
        internal partial class APPLY_ACTImpl : Mozart.SeePlan.Pegging.ApplyActControl
        {
            private MicronBEAssy.Logic.Pegging.APPLY_ACT fAPPLY_ACT = new MicronBEAssy.Logic.Pegging.APPLY_ACT();
            public override System.Collections.Generic.IList<Mozart.SeePlan.Pegging.IMaterial> GetActs(Mozart.SeePlan.Pegging.PegPart pegPart)
            {
                bool handled = false;
                System.Collections.Generic.IList<Mozart.SeePlan.Pegging.IMaterial> returnValue = null;
                returnValue = this.fAPPLY_ACT.GET_ACTS0(pegPart, ref handled, returnValue);
                return returnValue;
            }
            public override void WriteActPeg(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, double qty)
            {
                bool handled = false;
                this.fAPPLY_ACT.WRITE_ACT_PEG0(target, m, qty, ref handled);
            }
        }
        /// <summary>
        /// APPLY_YIELD execution control implementation
        /// </summary>
        internal partial class APPLY_YIELDImpl : Mozart.SeePlan.Pegging.ApplyYieldControl
        {
            private MicronBEAssy.Logic.Pegging.APPLY_YIELD fAPPLY_YIELD = new MicronBEAssy.Logic.Pegging.APPLY_YIELD();
            public override double GetYield(Mozart.SeePlan.Pegging.PegPart pegPart)
            {
                bool handled = false;
                double returnValue = 0D;
                returnValue = this.fAPPLY_YIELD.GET_YIELD0(pegPart, ref handled, returnValue);
                return returnValue;
            }
            public override double RoundResult(double qty)
            {
                bool handled = false;
                double returnValue = 0D;
                returnValue = this.fAPPLY_YIELD.ROUND_RESULT0(qty, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// BIN_BUFFER execution control implementation
        /// </summary>
        internal partial class BIN_BUFFERImpl : Mozart.SeePlan.SemiBE.Pegging.BinBufferControl
        {
        }
        /// <summary>
        /// BUILD_INPLAN execution control implementation
        /// </summary>
        internal partial class BUILD_INPLANImpl : Mozart.SeePlan.SemiBE.Pegging.BuildInPlanControl
        {
        }
        /// <summary>
        /// CHANGE_PART execution control implementation
        /// </summary>
        internal partial class CHANGE_PARTImpl : Mozart.SeePlan.Pegging.ChangePartControl
        {
            private MicronBEAssy.Logic.Pegging.CHANGE_PART fCHANGE_PART = new MicronBEAssy.Logic.Pegging.CHANGE_PART();
            public override System.Collections.Generic.List<object> GetPartChangeInfos(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun)
            {
                bool handled = false;
                System.Collections.Generic.List<object> returnValue = null;
                returnValue = this.fCHANGE_PART.GET_PART_CHANGE_INFOS0(pegPart, isRun, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.Pegging.PegPart ApplyPartChangeInfo(Mozart.SeePlan.Pegging.PegPart pegPart, object partChangeInfo, bool isRun)
            {
                bool handled = false;
                Mozart.SeePlan.Pegging.PegPart returnValue = null;
                returnValue = this.fCHANGE_PART.APPLY_PART_CHANGE_INFO0(pegPart, partChangeInfo, isRun, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// FILTER_TARGET execution control implementation
        /// </summary>
        internal partial class FILTER_TARGETImpl : Mozart.SeePlan.Pegging.FilterTargetControl
        {
        }
        /// <summary>
        /// KIT_PEG execution control implementation
        /// </summary>
        internal partial class KIT_PEGImpl : Mozart.SeePlan.SemiBE.Pegging.KitPegControl
        {
        }
        /// <summary>
        /// MANIPULATE_DEMAND execution control implementation
        /// </summary>
        internal partial class MANIPULATE_DEMANDImpl : Mozart.SeePlan.SemiBE.Pegging.ManipulateDemandControl
        {
        }
        /// <summary>
        /// PEG_WIP execution control implementation
        /// </summary>
        internal partial class PEG_WIPImpl : Mozart.SeePlan.Pegging.PegWipControl
        {
            private MicronBEAssy.Logic.Pegging.PEG_WIP fPEG_WIP = new MicronBEAssy.Logic.Pegging.PEG_WIP();
            public override System.Collections.Generic.IList<Mozart.SeePlan.Pegging.IMaterial> GetWips(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun)
            {
                bool handled = false;
                System.Collections.Generic.IList<Mozart.SeePlan.Pegging.IMaterial> returnValue = null;
                returnValue = this.fPEG_WIP.GET_WIPS0(pegPart, isRun, ref handled, returnValue);
                return returnValue;
            }
            public override void UpdatePegInfo(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, double qty)
            {
                bool handled = false;
                this.fPEG_WIP.UPDATE_PEG_INFO0(target, m, qty, ref handled);
            }
            public override void WritePeg(Mozart.SeePlan.Pegging.PegTarget target, Mozart.SeePlan.Pegging.IMaterial m, double qty)
            {
                bool handled = false;
                this.fPEG_WIP.WRITE_PEG0(target, m, qty, ref handled);
            }
        }
        /// <summary>
        /// PREPARE_TARGET execution control implementation
        /// </summary>
        internal partial class PREPARE_TARGETImpl : Mozart.SeePlan.SemiBE.Pegging.PrepareTargetControl
        {
            private MicronBEAssy.Logic.Pegging.PREPARE_TARGET fPREPARE_TARGET = new MicronBEAssy.Logic.Pegging.PREPARE_TARGET();
            public override Mozart.SeePlan.Pegging.PegPart PrepareTarget(Mozart.SeePlan.Pegging.PegPart pegPart)
            {
                bool handled = false;
                Mozart.SeePlan.Pegging.PegPart returnValue = null;
                returnValue = this.fPREPARE_TARGET.PREPARE_TARGET0(pegPart, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// PREPARE_WIP execution control implementation
        /// </summary>
        internal partial class PREPARE_WIPImpl : Mozart.SeePlan.SemiBE.Pegging.PrepareWipControl
        {
            private MicronBEAssy.Logic.Pegging.PREPARE_WIP fPREPARE_WIP = new MicronBEAssy.Logic.Pegging.PREPARE_WIP();
            public override Mozart.SeePlan.Pegging.PegPart PrepareWip(Mozart.SeePlan.Pegging.PegPart pegPart)
            {
                bool handled = false;
                Mozart.SeePlan.Pegging.PegPart returnValue = null;
                returnValue = this.fPREPARE_WIP.PREPARE_WIP0(pegPart, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// SHIFT_TAT execution control implementation
        /// </summary>
        internal partial class SHIFT_TATImpl : Mozart.SeePlan.Pegging.ShiftTatControl
        {
            private MicronBEAssy.Logic.Pegging.SHIFT_TAT fSHIFT_TAT = new MicronBEAssy.Logic.Pegging.SHIFT_TAT();
            public override System.TimeSpan GetTat(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun)
            {
                bool handled = false;
                System.TimeSpan returnValue = default(System.TimeSpan);
                returnValue = this.fSHIFT_TAT.GET_TAT0(pegPart, isRun, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// SMOOTH_DEMAND execution control implementation
        /// </summary>
        internal partial class SMOOTH_DEMANDImpl : Mozart.SeePlan.Pegging.SmoothDemandControl
        {
            private Mozart.SeePlan.Pegging.PreRuleLogic fpPreRuleLogic = new Mozart.SeePlan.Pegging.PreRuleLogic();
            public override Mozart.SeePlan.Pegging.IInnerBucket CreateInnerBucket(string key, Mozart.SeePlan.Pegging.MoPlan moPlan, System.DateTime weekStartDate, bool isW00)
            {
                bool handled = false;
                Mozart.SeePlan.Pegging.IInnerBucket returnValue = null;
                returnValue = this.fpPreRuleLogic.CREATE_INNER_BUCKET_DEF(key, moPlan, weekStartDate, isW00, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.Pegging.IOuterBucket CreateOuterBucket(string key)
            {
                bool handled = false;
                Mozart.SeePlan.Pegging.IOuterBucket returnValue = null;
                returnValue = this.fpPreRuleLogic.CREATE_OUTER_BUCKET_DEF(key, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// WRITE_TARGET execution control implementation
        /// </summary>
        internal partial class WRITE_TARGETImpl : Mozart.SeePlan.Pegging.WriteTargetControl
        {
            private MicronBEAssy.Logic.Pegging.WRITE_TARGET fWRITE_TARGET = new MicronBEAssy.Logic.Pegging.WRITE_TARGET();
            public override void WriteTarget(Mozart.SeePlan.Pegging.PegPart pegPart, bool isOut)
            {
                bool handled = false;
                this.fWRITE_TARGET.WRITE_TARGET0(pegPart, isOut, ref handled);
            }
            public override object GetStepPlanKey(Mozart.SeePlan.Pegging.PegPart pegPart)
            {
                bool handled = false;
                object returnValue = null;
                returnValue = this.fWRITE_TARGET.GET_STEP_PLAN_KEY0(pegPart, ref handled, returnValue);
                return returnValue;
            }
            public override Mozart.SeePlan.DataModel.StepTarget CreateStepTarget(Mozart.SeePlan.Pegging.PegTarget pegTarget, object stepPlanKey, Mozart.SeePlan.DataModel.Step step, bool isRun)
            {
                bool handled = false;
                Mozart.SeePlan.DataModel.StepTarget returnValue = null;
                returnValue = this.fWRITE_TARGET.CREATE_STEP_TARGET0(pegTarget, stepPlanKey, step, isRun, ref handled, returnValue);
                return returnValue;
            }
        }
        /// <summary>
        /// WRITE_UNPEG execution control implementation
        /// </summary>
        internal partial class WRITE_UNPEGImpl : Mozart.SeePlan.Pegging.WriteUnpegControl
        {
            private MicronBEAssy.Logic.Pegging.WRITE_UNPEG fWRITE_UNPEG = new MicronBEAssy.Logic.Pegging.WRITE_UNPEG();
            public override void WriteUnpeg(Mozart.SeePlan.Pegging.PegPart pegPart)
            {
                bool handled = false;
                this.fWRITE_UNPEG.WRITE_UNPEG0(pegPart, ref handled);
            }
        }
        internal class RulesImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.SeePlan.Pegging.Rule.PeggingRuleActionMethod>(this.fRules.INIT_SUPPLY_PLAN);
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.SeePlan.Pegging.Rule.PeggingRuleActionMethod>(this.fRules.UPDATE_TARGET_INFO);
            }
            private MicronBEAssy.Logic.Pegging.Rules fRules = new MicronBEAssy.Logic.Pegging.Rules();
        }
        internal class RulePresetsImpl : IModelConfigurable
        {
            public virtual void Configure()
            {
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateInitWip());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateInitSupplyPlan());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateApplyAct());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateSmoothDemand());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateWriteOutTarget());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreatePegRunWip());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateShiftRunTime());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateApplyYield());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateChangePart());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateWriteInTarget());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreatePegWaitWip());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateShiftWaitTime());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateWriteUnpeg());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateKitPeg());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateInitTarget());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateUpdateTargetInfo());
                Mozart.Task.Execution.ServiceLocator.RegisterInstance<Mozart.RuleFlow.IRulePreset>(this.CreateGenInitTarget());
            }
            /// <summary>
            /// InitWip's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateInitWip()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("PREPARE_WIP");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("InitWip", list, props);
            }
            /// <summary>
            /// InitSupplyPlan's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateInitSupplyPlan()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("INIT_SUPPLYPLAN");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("InitSupplyPlan", list, props);
            }
            /// <summary>
            /// ApplyAct's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateApplyAct()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("APPLY_ACT");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("ApplyAct", list, props);
            }
            /// <summary>
            /// SmoothDemand's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateSmoothDemand()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("SMOOTH_DEMAND");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("SmoothDemand", list, props);
            }
            /// <summary>
            /// WriteOutTarget's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateWriteOutTarget()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("WRITE_TARGET");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", true);
                return new Mozart.RuleFlow.RulePreset("WriteOutTarget", list, props);
            }
            /// <summary>
            /// PegRunWip's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreatePegRunWip()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("PEG_WIP");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", true);
                return new Mozart.RuleFlow.RulePreset("PegRunWip", list, props);
            }
            /// <summary>
            /// ShiftRunTime's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateShiftRunTime()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("SHIFT_TAT");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", true);
                return new Mozart.RuleFlow.RulePreset("ShiftRunTime", list, props);
            }
            /// <summary>
            /// ApplyYield's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateApplyYield()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("APPLY_YIELD");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", true);
                return new Mozart.RuleFlow.RulePreset("ApplyYield", list, props);
            }
            /// <summary>
            /// ChangePart's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateChangePart()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("CHANGE_PART");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", true);
                return new Mozart.RuleFlow.RulePreset("ChangePart", list, props);
            }
            /// <summary>
            /// WriteInTarget's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateWriteInTarget()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("WRITE_TARGET");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("WriteInTarget", list, props);
            }
            /// <summary>
            /// PegWaitWip's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreatePegWaitWip()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("PEG_WIP");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", false);
                return new Mozart.RuleFlow.RulePreset("PegWaitWip", list, props);
            }
            /// <summary>
            /// ShiftWaitTime's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateShiftWaitTime()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("SHIFT_TAT");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", false);
                return new Mozart.RuleFlow.RulePreset("ShiftWaitTime", list, props);
            }
            /// <summary>
            /// WriteUnpeg's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateWriteUnpeg()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("WRITE_UNPEG");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("WriteUnpeg", list, props);
            }
            /// <summary>
            /// KitPeg's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateKitPeg()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("KIT_PEG2");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("KitPeg", list, props);
            }
            /// <summary>
            /// InitTarget's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateInitTarget()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("PREPARE_TARGET");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("InitTarget", list, props);
            }
            /// <summary>
            /// UpdateTargetInfo's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateUpdateTargetInfo()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("UPDATE_TARGET_INFO");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                props.Set("IsRun", true);
                return new Mozart.RuleFlow.RulePreset("UpdateTargetInfo", list, props);
            }
            /// <summary>
            /// GenInitTarget's RulePreset
            /// </summary>
            private Mozart.RuleFlow.RulePreset CreateGenInitTarget()
            {
                System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                list.Add("PREPARE_TARGET");
                Mozart.RuleFlow.StageProperties props = new Mozart.RuleFlow.StageProperties();
                return new Mozart.RuleFlow.RulePreset("GenInitTarget", list, props);
            }
        }
    }
}
