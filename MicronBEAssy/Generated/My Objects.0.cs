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

namespace MicronBEAssy
{
    
    /// <summary>
    /// DataModel part 
    /// </summary>
    public partial class InputMart : InputRepository
    {
        private Dictionary<IComparable, MicronBEAssyProcess> _MicronBEAssyProcess;
        public Dictionary<IComparable, MicronBEAssyProcess> MicronBEAssyProcess
        {
            get
            {
                if ((this._MicronBEAssyProcess == null))
                {
                    this._MicronBEAssyProcess = new Dictionary<IComparable, MicronBEAssyProcess>();
                }
                return this._MicronBEAssyProcess;
            }
        }
        private MultiDictionary<string, MicronBEAssyWipInfo> _MicronBEAssyWipInfo;
        public MultiDictionary<string, MicronBEAssyWipInfo> MicronBEAssyWipInfo
        {
            get
            {
                if ((this._MicronBEAssyWipInfo == null))
                {
                    this._MicronBEAssyWipInfo = new MultiDictionary<string, MicronBEAssyWipInfo>();
                }
                return this._MicronBEAssyWipInfo;
            }
        }
        private Dictionary<Tuple<string, string>, ProductDetail> _ProductDetail;
        public Dictionary<Tuple<string, string>, ProductDetail> ProductDetail
        {
            get
            {
                if ((this._ProductDetail == null))
                {
                    this._ProductDetail = new Dictionary<Tuple<string, string>, ProductDetail>();
                }
                return this._ProductDetail;
            }
        }
        private Dictionary<string, MicronBEAssyBEMoMaster> _MicronBEAssyBEMoMaster;
        public Dictionary<string, MicronBEAssyBEMoMaster> MicronBEAssyBEMoMaster
        {
            get
            {
                if ((this._MicronBEAssyBEMoMaster == null))
                {
                    this._MicronBEAssyBEMoMaster = new Dictionary<string, MicronBEAssyBEMoMaster>();
                }
                return this._MicronBEAssyBEMoMaster;
            }
        }
        private MultiDictionary<string, MicronBEAssyPlanWip> _MicronBEAssyPlanWip;
        public MultiDictionary<string, MicronBEAssyPlanWip> MicronBEAssyPlanWip
        {
            get
            {
                if ((this._MicronBEAssyPlanWip == null))
                {
                    this._MicronBEAssyPlanWip = new MultiDictionary<string, MicronBEAssyPlanWip>();
                }
                return this._MicronBEAssyPlanWip;
            }
        }
        private Dictionary<string, MicronBEAssyEqp> _MicronBEAssyEqp;
        public Dictionary<string, MicronBEAssyEqp> MicronBEAssyEqp
        {
            get
            {
                if ((this._MicronBEAssyEqp == null))
                {
                    this._MicronBEAssyEqp = new Dictionary<string, MicronBEAssyEqp>();
                }
                return this._MicronBEAssyEqp;
            }
        }
        private Dictionary<Tuple<string, string>, MicronBEAssyBatch> _MicronBEAssyBatch;
        public Dictionary<Tuple<string, string>, MicronBEAssyBatch> MicronBEAssyBatch
        {
            get
            {
                if ((this._MicronBEAssyBatch == null))
                {
                    this._MicronBEAssyBatch = new Dictionary<Tuple<string, string>, MicronBEAssyBatch>();
                }
                return this._MicronBEAssyBatch;
            }
        }
        private MultiDictionary<IComparable, MicronBEAssy.DataModel.MicronBEAssyPlanWip> _MicronBEAssyActPlanWips;
        public MultiDictionary<IComparable, MicronBEAssy.DataModel.MicronBEAssyPlanWip> MicronBEAssyActPlanWips
        {
            get
            {
                if ((this._MicronBEAssyActPlanWips == null))
                {
                    this._MicronBEAssyActPlanWips = new MultiDictionary<IComparable, MicronBEAssy.DataModel.MicronBEAssyPlanWip>();
                }
                return this._MicronBEAssyActPlanWips;
            }
        }
        private MultiDictionary<Tuple<AssyMcpProduct, AssyMcpPart>, MicronBEAssy.DataModel.MicronBEAssyBELot> _MatchingLotList;
        public MultiDictionary<Tuple<AssyMcpProduct, AssyMcpPart>, MicronBEAssy.DataModel.MicronBEAssyBELot> MatchingLotList
        {
            get
            {
                if ((this._MatchingLotList == null))
                {
                    this._MatchingLotList = new MultiDictionary<Tuple<AssyMcpProduct, AssyMcpPart>, MicronBEAssy.DataModel.MicronBEAssyBELot>();
                }
                return this._MatchingLotList;
            }
        }
        private DoubleDictionary<Mozart.SeePlan.DataModel.Step, object, Mozart.SeePlan.DataModel.StepPlan> _MicronBEAssyRemainStepPlans;
        public DoubleDictionary<Mozart.SeePlan.DataModel.Step, object, Mozart.SeePlan.DataModel.StepPlan> MicronBEAssyRemainStepPlans
        {
            get
            {
                if ((this._MicronBEAssyRemainStepPlans == null))
                {
                    this._MicronBEAssyRemainStepPlans = new DoubleDictionary<Mozart.SeePlan.DataModel.Step, object, Mozart.SeePlan.DataModel.StepPlan>();
                }
                return this._MicronBEAssyRemainStepPlans;
            }
        }
        private MultiDictionary<Tuple<string, string>, MicronBEAssy.DataModel.AssyMcpPart> _FindAssyInPartCache;
        public MultiDictionary<Tuple<string, string>, MicronBEAssy.DataModel.AssyMcpPart> FindAssyInPartCache
        {
            get
            {
                if ((this._FindAssyInPartCache == null))
                {
                    this._FindAssyInPartCache = new MultiDictionary<Tuple<string, string>, MicronBEAssy.DataModel.AssyMcpPart>();
                }
                return this._FindAssyInPartCache;
            }
        }
        private Dictionary<string, System.DateTime> _ArrivalTime;
        public Dictionary<string, System.DateTime> ArrivalTime
        {
            get
            {
                if ((this._ArrivalTime == null))
                {
                    this._ArrivalTime = new Dictionary<string, System.DateTime>();
                }
                return this._ArrivalTime;
            }
        }
        private Dictionary<string, MicronBEAssy.Outputs.EqpPlan> _EqpPlans;
        public Dictionary<string, MicronBEAssy.Outputs.EqpPlan> EqpPlans
        {
            get
            {
                if ((this._EqpPlans == null))
                {
                    this._EqpPlans = new Dictionary<string, MicronBEAssy.Outputs.EqpPlan>();
                }
                return this._EqpPlans;
            }
        }
        private Dictionary<IComparable, double> _TactTimeCache;
        public Dictionary<IComparable, double> TactTimeCache
        {
            get
            {
                if ((this._TactTimeCache == null))
                {
                    this._TactTimeCache = new Dictionary<IComparable, double>();
                }
                return this._TactTimeCache;
            }
        }
        private Dictionary<Tuple<string, string, bool, bool, int>, Mozart.SeePlan.SemiBE.DataModel.Product> _MicronBEProducts;
        public Dictionary<Tuple<string, string, bool, bool, int>, Mozart.SeePlan.SemiBE.DataModel.Product> MicronBEProducts
        {
            get
            {
                if ((this._MicronBEProducts == null))
                {
                    this._MicronBEProducts = new Dictionary<Tuple<string, string, bool, bool, int>, Mozart.SeePlan.SemiBE.DataModel.Product>();
                }
                return this._MicronBEProducts;
            }
        }
        protected override void ClearMyObjects()
        {
            base.ClearMyObjects();
            this.DisposeIfNeeds(this._MicronBEAssyProcess);
            this._MicronBEAssyProcess = null;
            this.DisposeIfNeeds(this._MicronBEAssyWipInfo);
            this._MicronBEAssyWipInfo = null;
            this.DisposeIfNeeds(this._ProductDetail);
            this._ProductDetail = null;
            this.DisposeIfNeeds(this._MicronBEAssyBEMoMaster);
            this._MicronBEAssyBEMoMaster = null;
            this.DisposeIfNeeds(this._MicronBEAssyPlanWip);
            this._MicronBEAssyPlanWip = null;
            this.DisposeIfNeeds(this._MicronBEAssyEqp);
            this._MicronBEAssyEqp = null;
            this.DisposeIfNeeds(this._MicronBEAssyBatch);
            this._MicronBEAssyBatch = null;
            this.DisposeIfNeeds(this._MicronBEAssyActPlanWips);
            this._MicronBEAssyActPlanWips = null;
            this.DisposeIfNeeds(this._MatchingLotList);
            this._MatchingLotList = null;
            this.DisposeIfNeeds(this._MicronBEAssyRemainStepPlans);
            this._MicronBEAssyRemainStepPlans = null;
            this.DisposeIfNeeds(this._FindAssyInPartCache);
            this._FindAssyInPartCache = null;
            this.DisposeIfNeeds(this._ArrivalTime);
            this._ArrivalTime = null;
            this.DisposeIfNeeds(this._EqpPlans);
            this._EqpPlans = null;
            this.DisposeIfNeeds(this._TactTimeCache);
            this._TactTimeCache = null;
            this.DisposeIfNeeds(this._MicronBEProducts);
            this._MicronBEProducts = null;
        }
    }
    /// <summary>
    /// Type bindings registration
    /// </summary>
    public partial class MyTypeRegistrar : ModelConfiguratorBase
    {
        protected override void Configure()
        {
            base.Configure();
            Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Simulation.WorkStep), typeof(MicronBEAssyWorkStep), null);
            Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Simulation.WorkLot), typeof(MicronBEAssyWorkLot), null);
        }
    }
    /// <summary>
    /// MicronBEAssy Conatant class
    /// </summary>
    public partial class Constants
    {
        public const string ChgType = "SPLIT";
        public const string WorkAgentName = "ASSY";
        public const string Hyphen = "-";
        public const string DieAttach = "DIE ATTACH";
        public const string WireBond = "WIRE BOND";
        public const string Y = "Y";
        public const string N = "N";
        public const string RUNNING = "RUNNING";
        public const string NULL = "NULL";
        public const string YES = "YES";
        public const string LOTSRECEIVED = "LOTS RECEIVED";
    }
    /// <summary>
    /// ErrorLevel Enumerations
    /// </summary>
    public enum ErrorLevel
    {
        WARNING,
        FATAL,
    }
    /// <summary>
    /// UnpegReason Enumerations
    /// </summary>
    public enum UnpegReason
    {
        EXCESS,
        NO_TARGET,
        MASTER_DATA,
    }
    /// <summary>
    /// TimeUnit Enumerations
    /// </summary>
    public enum TimeUnit
    {
        DAY,
        HOUR,
        MIN,
        SEC,
    }
    /// <summary>
    /// LotType Enumerations
    /// </summary>
    public enum LotType
    {
        ACT,
    }
    /// <summary>
    /// SetupType Enumerations
    /// </summary>
    public enum SetupType
    {
        NONE = 0,
        PART_CHG = 1,
    }
    /// <summary>
    /// LoadState Enumerations
    /// </summary>
    public enum LoadState
    {
        BUSY,
        WAIT,
    }
    /// <summary>
    /// MasterDataErrorEventType Enumerations
    /// </summary>
    public enum MasterDataErrorEventType
    {
        WIP,
        LOT,
        TARGET,
        EQP,
        PRODUCT,
        PROCESS,
    }
}
