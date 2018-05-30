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
using Mozart.SeePlan.SemiBE.DataModel;
using System.ComponentModel;
using Mozart.SeePlan.SemiBE.Pegging;
using Mozart.SeePlan.SemiBE.Simulation;
using Mozart.SeePlan.Simulation;

namespace MicronBEAssy.DataModel
{
    
    [System.SerializableAttribute()]
    public partial class MicronBEAssyProcess : Mozart.SeePlan.SemiBE.DataModel.Process, IEntityObject, IEditableObject
    {
        public MicronBEAssyProcess()
        {
        }
        public MicronBEAssyProcess(string processID) : 
                base(processID)
        {
        }
        public virtual string ProcessType { get; set; }

        public virtual string BottleneckSteps { get; set; }

        private List<MicronBEAssy.DataModel.MicronBEAssyBEStep> _DieAttachSteps =  new List<MicronBEAssy.DataModel.MicronBEAssyBEStep>();
        public virtual List<MicronBEAssy.DataModel.MicronBEAssyBEStep> DieAttachSteps
        {
            get
            {
                return this._DieAttachSteps;
            }
            set
            {
                _DieAttachSteps = value;
            }
        }
        public virtual MicronBEAssy.DataModel.MicronBEAssyBEStep CR2OutStep { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyProduct : Mozart.SeePlan.SemiBE.DataModel.Product, IEntityObject, IEditableObject
    {
        public MicronBEAssyProduct()
        {
        }
        public MicronBEAssyProduct(string prodCode, Mozart.SeePlan.SemiBE.DataModel.Process proc) : 
                base(prodCode, proc)
        {
        }
        public virtual bool IsMcpPart { get; set; }

        public virtual bool IsMidPart { get; set; }

        public virtual int CompSeq { get; set; }

        public virtual Tuple<string,string,bool,bool,int> Key { get; set; }

        public virtual MicronBEAssy.DataModel.ProductDetail ProductDetail { get; set; }

        public virtual string PartChangeStep { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyWipInfo : Mozart.SeePlan.SemiBE.DataModel.IWipInfo
    {
        public virtual string LotID { get; set; }

        public virtual double UnitQty { get; set; }

        public virtual Mozart.SeePlan.SemiBE.DataModel.Product Product { get; set; }

        public virtual Mozart.SeePlan.SemiBE.DataModel.Process Process { get; set; }

        public virtual Mozart.SeePlan.SemiBE.DataModel.BEStep InitialStep { get; set; }

        public virtual Mozart.SeePlan.SemiBE.DataModel.Eqp InitialEqp { get; set; }

        public virtual Mozart.SeePlan.Simulation.EntityState CurrentState { get; set; }

        public virtual string WipProductID { get; set; }

        public virtual string WipProcessID { get; set; }

        public virtual string WipStepID { get; set; }

        public virtual string WipEqpID { get; set; }

        public virtual string WipState { get; set; }

        public virtual DateTime WipStateTime { get; set; }

        public virtual DateTime LastTrackInTime { get; set; }

        public virtual DateTime LastProcessStartTime { get; set; }

        public virtual DateTime LastTrackOutTime { get; set; }

        public virtual string LineID { get; set; }

        public MicronBEAssyWipInfo ShallowCopy()
        {
			var x = (MicronBEAssyWipInfo) this.MemberwiseClone();
            return x;
        }
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBEStep : Mozart.SeePlan.SemiBE.DataModel.BEStep, IEntityObject, IEditableObject
    {
        public MicronBEAssyBEStep()
        {
        }
        public MicronBEAssyBEStep(string id) : 
                base(id)
        {
        }
        public virtual string StepGroup { get; set; }

        public virtual int DaThroughCount { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class ProductDetail
    {
        public virtual string LineID { get; set; }

        public virtual string ProductID { get; set; }

        public virtual string ProductName { get; set; }

        public virtual MicronBEAssy.DataModel.MicronBEAssyProcess Process { get; set; }

        public virtual string DesignID { get; set; }

        public virtual string MaterialGroup { get; set; }

        public virtual string PkgFamily { get; set; }

        public virtual string PkgType { get; set; }

        public virtual string PkgLeadCount { get; set; }

        public virtual decimal PkgWidth { get; set; }

        public virtual decimal PkgLength { get; set; }

        public virtual decimal PkgHeight { get; set; }

        public virtual decimal NetDie { get; set; }

        public ProductDetail ShallowCopy()
        {
			var x = (ProductDetail) this.MemberwiseClone();
            return x;
        }
    }
    [System.SerializableAttribute()]
    public partial class AssyMcpProduct : Mozart.SeePlan.SemiBE.DataModel.Product, IEntityObject, IEditableObject
    {
        public AssyMcpProduct()
        {
        }
        public AssyMcpProduct(string prodCode, Mozart.SeePlan.SemiBE.DataModel.Process proc) : 
                base(prodCode, proc)
        {
        }
        public virtual MicronBEAssy.DataModel.ProductDetail ProductDetail { get; set; }

        private List<MicronBEAssy.DataModel.AssyMcpPart> _AllParts =  new List<MicronBEAssy.DataModel.AssyMcpPart>();
        public virtual List<MicronBEAssy.DataModel.AssyMcpPart> AllParts
        {
            get
            {
                return this._AllParts;
            }
            set
            {
                _AllParts = value;
            }
        }
        public virtual List<MicronBEAssy.DataModel.AssyMcpPart> AssyParts { get; set; }

        public virtual int MaxSequence { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class AssyMcpPart : Mozart.SeePlan.SemiBE.DataModel.Product, IEntityObject, IEditableObject
    {
        public AssyMcpPart()
        {
        }
        public AssyMcpPart(string prodCode, Mozart.SeePlan.SemiBE.DataModel.Process proc) : 
                base(prodCode, proc)
        {
        }
        public virtual MicronBEAssy.DataModel.AssyMcpProduct FinalProduct { get; set; }

        public virtual bool IsMidPart { get; set; }

        public virtual int CompSeq { get; set; }

        public virtual string PartChangeStep { get; set; }

        public virtual bool IsBase { get; set; }

        public virtual MicronBEAssy.DataModel.ProductDetail ProductDetail { get; set; }

        public virtual int CompQty { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyMcpPart : Mozart.SeePlan.SemiBE.DataModel.McpPart, IEntityObject, IEditableObject
    {
        public MicronBEAssyMcpPart()
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyMcpProduct : Mozart.SeePlan.SemiBE.DataModel.McpProduct, IEntityObject, IEditableObject
    {
        public MicronBEAssyMcpProduct(string prodCode, Mozart.SeePlan.SemiBE.DataModel.Process proc) : 
                base(prodCode, proc)
        {
        }
        public MicronBEAssyMcpProduct()
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBEMoMaster : Mozart.SeePlan.SemiBE.Pegging.BEMoMaster, IEntityObject, IEditableObject
    {
        public MicronBEAssyBEMoMaster()
        {
        }
        public MicronBEAssyBEMoMaster(Mozart.SeePlan.SemiBE.DataModel.Product prod, string customer) : 
                base(prod, customer)
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBEPegTarget : Mozart.SeePlan.SemiBE.Pegging.BEPegTarget, IEntityObject, IEditableObject
    {
        public MicronBEAssyBEPegTarget(Mozart.SeePlan.SemiBE.Pegging.BEPegPart pp, Mozart.SeePlan.SemiBE.Pegging.BEMoPlan mp) : 
                base(pp, mp)
        {
        }
        public MicronBEAssyBEPegTarget()
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBEMoPlan : Mozart.SeePlan.SemiBE.Pegging.BEMoPlan, IEntityObject, IEditableObject
    {
        public MicronBEAssyBEMoPlan()
        {
        }
        public MicronBEAssyBEMoPlan(Mozart.SeePlan.SemiBE.Pegging.BEMoMaster mm, float qty, System.DateTime dueDate) : 
                base(mm, qty, dueDate)
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyPlanWip : Mozart.SeePlan.SemiBE.Pegging.PlanWip, IEntityObject, IEditableObject
    {
        public MicronBEAssyPlanWip()
        {
        }
        public MicronBEAssyPlanWip(Mozart.SeePlan.SemiBE.DataModel.IWipInfo wip) : 
                base(wip)
        {
        }
        public virtual Mozart.SeePlan.SemiBE.DataModel.Product Product { get; set; }

        public virtual string LotID { get; set; }

        public virtual string LineID { get; set; }

        public virtual bool IsReleased { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBEPegPart : Mozart.SeePlan.SemiBE.Pegging.BEPegPart, IEntityObject, IEditableObject
    {
        public MicronBEAssyBEPegPart()
        {
        }
        public MicronBEAssyBEPegPart(Mozart.SeePlan.SemiBE.Pegging.BEMoMaster moMaster, Mozart.SeePlan.SemiBE.DataModel.Product product) : 
                base(moMaster, product)
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBELotBatch : Mozart.SeePlan.SemiBE.Simulation.BELotBatch, IEntityObject, IEditableObject
    {
        public MicronBEAssyBELotBatch()
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyPlanInfo : Mozart.SeePlan.SemiBE.DataModel.PlanInfo, IEntityObject, IEditableObject
    {
        public MicronBEAssyPlanInfo(Mozart.SeePlan.SemiBE.DataModel.BEStep task) : 
                base(task)
        {
        }
        public MicronBEAssyPlanInfo()
        {
        }
        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyEqp : Mozart.SeePlan.SemiBE.DataModel.Eqp, IEntityObject, IEditableObject
    {
        public MicronBEAssyEqp()
        {
        }
        public MicronBEAssyEqp(string eqpID, string eqpGroup, int shift, System.DateTime startTime, System.DateTime endTime, string simType) : 
                base(eqpID, eqpGroup, shift, startTime, endTime, simType)
        {
        }
        public virtual string EqpModel { get; set; }

        public virtual double UtilRate { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBELot : Mozart.SeePlan.SemiBE.Simulation.BELot, IEntityObject, IEditableObject
    {
        public MicronBEAssyBELot()
        {
        }
        public MicronBEAssyBELot(Mozart.SeePlan.SemiBE.DataModel.IWipInfo wip) : 
                base(wip)
        {
        }
        public MicronBEAssyBELot(string lotID, Mozart.SeePlan.SemiBE.DataModel.Product prod, string lineID) : 
                base(lotID, prod, lineID)
        {
        }
        public virtual Mozart.SeePlan.Simulation.AoEquipment ReservationEqp { get; set; }

        public virtual MicronBEAssy.DataModel.MicronBEAssyBatch AssyBatch { get; set; }

        #region IEntityObject implementations
        [System.NonSerializedAttribute()]
        private int rbtreeNodeId;
        [System.NonSerializedAttribute()]
        private long rowID = -1;
        [System.NonSerializedAttribute()]
        private Mozart.Data.Entity.IEntityChangeTracker tracker = Mozart.Data.Entity.EntityObject.DetachedTracker;
        Mozart.Data.Entity.EntityState IEntityObject.ObjectState
        {
            get
            {
                return this.tracker.GetObjectState(this);
            }
        }
        long IEntityObject.RowID
        {
            get
            {
                return this.rowID;
            }
            set
            {
                this.rowID = value;
            }
        }
        int IEntityObject.NodeCache
        {
            get
            {
                return this.rbtreeNodeId;
            }
            set
            {
                this.rbtreeNodeId = value;
            }
        }
        Mozart.Data.Entity.IEntityChangeTracker IEntityObject.ChangeTracker
        {
            get
            {
                return this.tracker;
            }
            set
            {
                this.tracker = value ?? EntityObject.DetachedTracker;
            }
        }
        protected virtual void InitCopy()
        {
rbtreeNodeId = 0;
rowID = -1;
tracker = EntityObject.DetachedTracker;
        }
        #endregion
        #region IEditableObject implements
        public virtual void BeginEdit()
        {
            tracker.BeginEdit(this);
        }
        public virtual void CancelEdit()
        {
            tracker.CancelEdit(this);
        }
        public virtual void EndEdit()
        {
            tracker.EndEdit(this);
        }
        #endregion
    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyWorkStep : Mozart.SeePlan.Simulation.WorkStep
    {
        public MicronBEAssyWorkStep(Mozart.SeePlan.Simulation.WorkGroup group, object key) : 
                base(group, key)
        {
        }
        public virtual int Sequence { get; set; }

        public virtual string AoProd { get; set; }

        public virtual DateTime AvailableDownTime { get; set; }

    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyWorkLot : Mozart.SeePlan.Simulation.WorkLot
    {
        public MicronBEAssyWorkLot(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.Simulation.Engine.Time availableTime, object stepKey, Mozart.SeePlan.DataModel.Step step) : 
                base(hb, availableTime, stepKey, step)
        {
        }
        public virtual string LotID { get; set; }

        public virtual double UnitQty { get; set; }

        public virtual Mozart.SeePlan.SemiBE.DataModel.Product Product { get; set; }

        public virtual Mozart.SeePlan.Simulation.AoEquipment ReservationEqp { get; set; }

        public virtual string P12 { get; set; }

    }
    [System.SerializableAttribute()]
    public partial class MicronBEAssyBatch
    {
        public virtual string AoProdID { get; set; }

        public virtual string LineID { get; set; }

        private Mozart.Collections.MultiDictionary<string, MicronBEAssy.DataModel.MicronBEAssyBEStep> _StepList =  new Mozart.Collections.MultiDictionary<string,MicronBEAssy.DataModel.MicronBEAssyBEStep>();
        public virtual Mozart.Collections.MultiDictionary<string, MicronBEAssy.DataModel.MicronBEAssyBEStep> StepList
        {
            get
            {
                return this._StepList;
            }
            set
            {
                _StepList = value;
            }
        }
        public MicronBEAssyBatch ShallowCopy()
        {
			var x = (MicronBEAssyBatch) this.MemberwiseClone();
            return x;
        }
    }
}
