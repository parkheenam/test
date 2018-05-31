using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.Projects;
using MicronBEAssy;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraCharts;
using MicronBEAssy.Outputs;
using MicronBEAssy.Inputs;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Mozart.Collections;
using Mozart.Studio.UserInterface;
using Mozart.Studio.UIComponents;
using Mozart.Studio.Documents;
using System.ComponentModel.Design;
using Mozart.Studio.Application;
using Mozart.Studio.Projects;

namespace MicronBEAssyUserInterface.WipTrendAnalysis
{
    public partial class WipTrendAnalysisView : XtraUserControlView
    {
        IExperimentResultItem _result;

        ResultDataContext _resultCtx;

        ModelDataContext _modelDataContext;

        ExpDataContext _expDataContext;

        DateTime _planStartTime;

        DateTime _startTime;

        DateTime _endTime;

        Dictionary<string, List<McpBom>> _mcpBomMappings;

        Dictionary<IComparable, ProductMaster> _productMasterMappings;

        Dictionary<IComparable, ProcessStep> _processStepMappings;

        HashedSet<string> _selectedLineIDSet;

        HashSet<string> _selectedDesignIDSet;

        HashSet<string> _selectedFinalProdIDSet;

        HashSet<string> _selectedProdIDSet;

        MainView _mainView;

        public WipTrendAnalysisView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                this._result = this.Document.GetResultItem();
                if (_result == null)
                    return;
            }

            this.LoadInit();
        }

        private void LoadInit()
        {
            this._planStartTime = this._result.StartTime;
            this.startDateEdit.DateTime = this._planStartTime;
            this._selectedLineIDSet = new HashedSet<string>();
            this._selectedDesignIDSet = new HashSet<string>();
            this._selectedFinalProdIDSet = new HashSet<string>();
            this._selectedProdIDSet = new HashSet<string>();
            this._mcpBomMappings = new Dictionary<string, List<McpBom>>();
            this._productMasterMappings = new Dictionary<IComparable, ProductMaster>();
            this._processStepMappings = new Dictionary<IComparable, ProcessStep>();
            this._startTime = this.startDateEdit.DateTime;
            this._endTime = this.startDateEdit.DateTime.AddDays((double)this.durationSpinEdit.Value);

            this._resultCtx = this._result.GetCtx<ResultDataContext>();
            this._modelDataContext = this._result.GetCtx<ModelDataContext>();
            this._expDataContext = this._result.GetCtx<ExpDataContext>();
         
            InitializeControl();

            this.InitMcpBomMappings();
            this.InitProductMasterMappings();
            this.InitProcessStepMappings();
        }

        private void InitMcpBomMappings() 
        {
            var mcpBom = this._modelDataContext.McpBom;

            foreach (var item in mcpBom) 
            {
                var key = item.FINAL_PROD_ID;
                List<McpBom> mcpBoms = null;
                if (this._mcpBomMappings.TryGetValue(key, out mcpBoms))
                    mcpBoms.Add(item);
                else 
                {
                    mcpBoms = new List<McpBom>();
                    mcpBoms.Add(item);
                    this._mcpBomMappings[key] = mcpBoms;
                }
            }
        }

        private void InitProductMasterMappings() 
        {
            var productMaster = this._modelDataContext.ProductMaster;

            foreach (var item in productMaster)
            {
                var key = Tuple.Create(item.LINE_ID, item.PRODUCT_ID);
                this._productMasterMappings[key] = item;
            }
        }

        private void InitProcessStepMappings()
        {
            var processStep = this._modelDataContext.ProcessStep;

            foreach (var item in processStep)
            {
                var key = Tuple.Create(item.LINE_ID, item.PROCESS_ID, item.STEP_ID);
                this._processStepMappings[key] = item;
            }
        }

        private void InitializeControl()
        {
            _mainView = new MainView();
            _mainView.Dock = DockStyle.Fill;
            this.mainPanel.Controls.Add(_mainView);

            this.startDateEdit.Properties.EditMask = "g";
            this.startDateEdit.Properties.Mask.EditMask = @"yyyy\-MM\-dd\ HH\:mm\:ss";
            this.startDateEdit.Properties.Mask.UseMaskAsDisplayFormat = true;
            this.startDateEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.startDateEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            this.InitLineIDComboBox();
            this.InitDesignIDComboBox();
            this.InitFinalProdIDComboBox();
        }

        private void InitLineIDComboBox()
        {
            var prodMaster = this._modelDataContext.ProductMaster;
            var lineIDSet = new HashSet<string>();

            foreach (var item in prodMaster)
            {
                if (string.IsNullOrEmpty(item.LINE_ID))
                    continue;

                if (lineIDSet.Contains(item.LINE_ID) == false)
                    lineIDSet.Add(item.LINE_ID);
            }

            foreach (var item in lineIDSet)
                this.lineIDCheckedComboBox.ListBox.Items.Add(new CheckedListBoxItem(item, true));

            this.lineIDCheckedComboBox.Select();
        }

        private void InitDesignIDComboBox()
        {
            var prodMaster = this._modelDataContext.ProductMaster;
            var designIDSet = new HashSet<string>();

            foreach (var item in prodMaster)
            {
                if (string.IsNullOrEmpty(item.DESIGN_ID))
                    continue;

                if (designIDSet.Contains(item.DESIGN_ID) == false)
                    designIDSet.Add(item.DESIGN_ID);
            }

            foreach (var item in designIDSet)
                this.designIDCheckedComboBox.ListBox.Items.Add(new CheckedListBoxItem(item, true));

            this.designIDCheckedComboBox.Select();
        }

        private void InitFinalProdIDComboBox()
        {
            var prodMaster = this._modelDataContext.ProductMaster;
            var mcpBom = this._modelDataContext.McpBom;

            var mcpFinalProdIDSet = new HashSet<string>();
            foreach (var item in mcpBom)
            {
                mcpFinalProdIDSet.Add(item.FINAL_PROD_ID);
            }

            var checkedValues = this.designIDCheckedComboBox.ListBox.Items.GetCheckedValues();
            var finalProdIDSet = new HashSet<string>();

            HashSet<string> checkedDesignIDs = new HashSet<string>();
            foreach(var item in checkedValues)
                checkedDesignIDs.Add(item.ToString());
             
            foreach (var item in prodMaster)
            {
                if (checkedDesignIDs.Contains(item.DESIGN_ID) == false)
                    continue;

                if (string.IsNullOrEmpty(item.PRODUCT_ID))
                    continue;

                if (mcpFinalProdIDSet.Contains(item.PRODUCT_ID))
                    finalProdIDSet.Add(item.PRODUCT_ID);
            }

            foreach (var item in finalProdIDSet)
                this.finalProdIDCheckedComboBox.ListBox.Items.Add(new CheckedListBoxItem(item, true));

            this.finalProdIDCheckedComboBox.Select();
        }

        private IEnumerable<ResultItem> GetDataList(string queryType)
        {
            var startTime = this._startTime;
            var endTime = this._endTime;
            var timeUnit = this.timeUnitComboBox.SelectedItem.ToString();

            var eqpPlans = this.GetEqpPlanMappings(startTime, endTime, timeUnit);
            var stepTargets = this.GetStepTargetMappings(startTime, endTime, timeUnit);
            var resultItems = this.GetResultItemList(startTime, endTime, eqpPlans, stepTargets);

            return resultItems;
        }

        private Dictionary<IComparable, StepTarget> GetStepTargetMappings(DateTime startTime, DateTime endTime, string timeUnit)
        {
            var result = this._expDataContext.Result(this._result.Name);
            var stepTargets = result.StepTarget;

            Dictionary<IComparable, StepTarget> stepTargetMappings = new Dictionary<IComparable, StepTarget>();

            foreach (var item in stepTargets)
            {
                if (startTime > item.TARGET_DATE)
                    continue;

                if (endTime < item.TARGET_DATE)
                    continue;

                if (this._selectedLineIDSet.Contains(item.LINE_ID) == false)
                    continue;

                if (this._selectedDesignIDSet.Contains(item.DESIGN_ID) == false)
                    continue;

                if (this._selectedProdIDSet.Contains(item.PRODUCT_ID) == false)
                    continue;

                StepTarget target = null;
                DateTime date = DateTime.MinValue;
                if (timeUnit == Constants.DAY)
                {
                    date = item.TARGET_DATE.StartTimeOfDayT();
                }
                if (timeUnit == Constants.SHIFT)
                {
                    date = item.TARGET_DATE.ShiftStartTimeOfDayT();
                }
                if (timeUnit == Constants.HOUR)
                {
                    date = new DateTime(item.TARGET_DATE.Year, item.TARGET_DATE.Month, item.TARGET_DATE.Day, item.TARGET_DATE.Hour, 00, 00);
                }

                var key = Tuple.Create(item.LINE_ID, item.PRODUCT_ID, item.PROCESS_ID, item.STEP_ID, date);
                if (stepTargetMappings.TryGetValue(key, out target))
                {
                    target.OUT_QTY += item.OUT_QTY;
                }
                else
                {
                    stepTargetMappings[key] = item;
                }
            }

            for (DateTime dateTime = startTime.StartTimeOfDayT(); dateTime <= endTime; )
            {
                foreach (var item in stepTargets)
                {
                    if (startTime > item.TARGET_DATE)
                        continue;

                    if (endTime < item.TARGET_DATE)
                        continue;

                    if (this._selectedLineIDSet.Contains(item.LINE_ID) == false)
                        continue;

                    if (this._selectedDesignIDSet.Contains(item.DESIGN_ID) == false)
                        continue;

                    if (this._selectedProdIDSet.Contains(item.PRODUCT_ID) == false)
                        continue;

                    var key = Tuple.Create(item.LINE_ID, item.PRODUCT_ID, item.PROCESS_ID, item.STEP_ID, dateTime);
                    StepTarget find = null;
                    if (stepTargetMappings.TryGetValue(key, out find) == false)
                    { 
                        StepTarget dummyTarget = new StepTarget();
                        dummyTarget.LINE_ID = item.LINE_ID;
                        dummyTarget.PRODUCT_ID = item.PRODUCT_ID;
                        dummyTarget.PROCESS_ID = item.PROCESS_ID;
                        dummyTarget.STEP_ID = item.STEP_ID;
                        dummyTarget.OUT_QTY = 0;
                        stepTargetMappings[key] = dummyTarget;
                    }
                }

                if (timeUnit == Constants.DAY)
                {
                    dateTime = dateTime.AddDays(1);
                }
                if (timeUnit == Constants.SHIFT)
                {
                    dateTime = dateTime.AddHours(8);
                }
                if (timeUnit == Constants.HOUR)
                {
                    dateTime = dateTime.AddHours(1);
                }
            }

            return stepTargetMappings;
        }

        private Dictionary<IComparable, EqpPlan> GetEqpPlanMappings(DateTime startTime, DateTime endTime, string timeUnit)
        {
            var result = this._expDataContext.Result(this._result.Name);
            var eqpPlans = result.EqpPlan;

            Dictionary<IComparable, EqpPlan> eqpPlanMappings = new Dictionary<IComparable, EqpPlan>();

            foreach (var item in eqpPlans)
            {
                if (startTime > item.START_TIME)
                    continue;

                if (endTime < item.END_TIME)
                    continue;

                if (item.STATUS != Constants.BUSY)
                    continue;

                if (this._selectedLineIDSet.Contains(item.LINE_ID) == false)
                    continue;

                //if (this._selectedDesignIDSet.Contains(item.DESIGN_ID) == false)
                //    continue;

                if (this._selectedProdIDSet.Contains(item.PRODUCT_ID) == false)
                    continue;

                EqpPlan plan = null;
                DateTime date = DateTime.MinValue;

                if (timeUnit == Constants.DAY)
                {
                    date = item.END_TIME.StartTimeOfDayT();
                }
                if (timeUnit == Constants.SHIFT)
                {
                    date = item.END_TIME.ShiftStartTimeOfDayT();
                }
                if (timeUnit == Constants.HOUR)
                {
                    date = new DateTime(item.END_TIME.Year, item.END_TIME.Month, item.END_TIME.Day, item.END_TIME.Hour, 00, 00);
                }

                var key = Tuple.Create(item.LINE_ID, item.PRODUCT_ID, item.PROCESS_ID, item.STEP_ID, date);

                if (eqpPlanMappings.TryGetValue(key, out plan))
                {
                    plan.QTY += item.QTY;
                }
                else
                {
                    eqpPlanMappings[key] = item;
                }
            }

            for (DateTime dateTime = startTime.StartTimeOfDayT(); dateTime <= endTime; )
            {
                foreach (var item in eqpPlans)
                {
                    if (startTime > item.START_TIME)
                        continue;

                    if (endTime < item.END_TIME)
                        continue;

                    if (item.STATUS != Constants.BUSY)
                        continue;

                    if (this._selectedLineIDSet.Contains(item.LINE_ID) == false)
                        continue;

                    //if (this._selectedDesignIDSet.Contains(item.DESIGN_ID) == false)
                    //    continue;

                    if (this._selectedProdIDSet.Contains(item.PRODUCT_ID) == false)
                        continue;

                    var key = Tuple.Create(item.LINE_ID, item.PRODUCT_ID, item.PROCESS_ID, item.STEP_ID, dateTime);
                    EqpPlan find = null;
                    if (eqpPlanMappings.TryGetValue(key, out find) == false)
                    {
                        EqpPlan dummyPlan = new EqpPlan();
                        dummyPlan.LINE_ID = item.LINE_ID;
                        dummyPlan.PRODUCT_ID = item.PRODUCT_ID;
                        dummyPlan.PROCESS_ID = item.PROCESS_ID;
                        dummyPlan.STEP_ID = item.STEP_ID;
                        dummyPlan.QTY = 0;
                        eqpPlanMappings[key] = dummyPlan;
                    }
                }

                if (timeUnit == Constants.DAY)
                {
                    dateTime = dateTime.AddDays(1);
                }
                if (timeUnit == Constants.SHIFT)
                {
                    dateTime = dateTime.AddHours(8);
                }
                if (timeUnit == Constants.HOUR)
                {
                    dateTime = dateTime.AddHours(1);
                }
            }

            return eqpPlanMappings;
        }

        private List<ResultItem> GetResultItemList(DateTime startTime, DateTime endTime, Dictionary<IComparable, EqpPlan> eqpPlanMappings, Dictionary<IComparable, StepTarget> stepTargetMappings)
        {
            var result = this._expDataContext.Result(this._result.Name);
            var list = new List<ResultItem>();
      
            var keys = eqpPlanMappings.Keys.Union(stepTargetMappings.Keys).Distinct();

            foreach (var key in keys) 
            {
                Tuple<string, string, string, string, DateTime> tuple = key as Tuple<string, string, string, string, DateTime>;

                ResultItem item = new ResultItem();

                StepTarget stepTarget = null;
                EqpPlan eqpPlan = null;
                DateTime date = tuple.Item5;
                if (stepTargetMappings.TryGetValue(tuple, out stepTarget))
                {
                    item.TargetQty = Convert.ToDouble(stepTarget.OUT_QTY);
                }
                if (eqpPlanMappings.TryGetValue(tuple, out eqpPlan))
                {
                    item.PlanQty = Convert.ToDouble(eqpPlan.QTY);
                }

                var prodMaster = this.FindProductMaster(tuple.Item1, tuple.Item2);
                var procStep = this.FindProcessStep(tuple.Item1, tuple.Item3, tuple.Item4);

                item.LineID = prodMaster.LINE_ID;
                item.DesignID = prodMaster.DESIGN_ID;
                item.ProductID = prodMaster.PRODUCT_ID;
                item.ProcessID = prodMaster.PROCESS_ID;
                item.StepID = procStep.STEP_ID;
                item.Sequence = procStep.SEQUENCE;
                item.Date = date.DbToString().Substring(0, 11);
                item.MaterialGroup = prodMaster.MATERIAL_GROUP;
                item.PkgFamily = prodMaster.PKG_FAMILY;
                item.PkgType = prodMaster.PKG_TYPE;

                list.Add(item);
            }
           
            return list;
        }

        private ProductMaster FindProductMaster(string lineID, string prodID)
        {
            ProductMaster prodMaster = null;
            var key = Tuple.Create(lineID, prodID);
            this._productMasterMappings.TryGetValue(key, out prodMaster);

            return prodMaster;
        }

        private ProcessStep FindProcessStep(string lineID, string procID, string stepID)
        {
            ProcessStep procStep = null;
            var key = Tuple.Create(lineID, procID, stepID);
            this._processStepMappings.TryGetValue(key, out procStep);

            return procStep;
        }

        private void SetMainView(IEnumerable<ResultItem> dt, string queryType)
        {
            bool isNewWindow = this.newWindowCheckBox.Checked;
            int panelCount = this._mainView.DockManager.Panels.Count;

            PivotGridControl pivotGridControl = new PivotGridControl();
            pivotGridControl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PivotGridControl_MouseClick);
          
            ChartControl chartControl = new ChartControl();
           
            DockPanel dockPanel = null;
            if (isNewWindow || panelCount == 0)
            {
                dockPanel = this._mainView.DockManager.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Float);
                dockPanel.Text = queryType;
                dockPanel.DockedAsTabbedDocument = true;
            }
            else 
            {
                dockPanel = this._mainView.DockManager.Panels[panelCount - 1];
                dockPanel.Text = queryType;
                dockPanel.Controls.Clear();
            }

            SubView subView = new SubView();
            subView.Dock = DockStyle.Fill;

            pivotGridControl.Dock = DockStyle.Fill;
            chartControl.Dock = DockStyle.Fill;

            subView.SplitContainerControl.Panel1.Controls.Add(pivotGridControl);
            subView.SplitContainerControl.Panel2.Controls.Add(chartControl);

            dockPanel.Controls.Add(subView);

            this.SetPivotGridFields(pivotGridControl);
            this.SetPivotGridData(dt, pivotGridControl);
        }

        private void PivotGridControl_MouseClick(object sender, MouseEventArgs e)
        {
            PivotGridControl pivot = sender as PivotGridControl;
            PivotGridHitInfo info = pivot.CalcHitInfo(e.Location);
            if (info.HitTest != PivotGridHitTest.Value)
                return;

            if (info.ValueInfo.Value == null)
                return;

            int fieldIdx = info.ValueInfo.Field.Index;
            int maxIdx = info.ValueInfo.MaxIndex;
            string fieldname = info.ValueInfo.Field.ToString();
            string fieldvalue = info.ValueInfo.Value.ToString();
        
            BindingList<ResultItem> chartResult = pivot.DataSource as BindingList<ResultItem>;
            string lineID = string.Empty;
            string designID = string.Empty;
            string prodID = string.Empty;
            string procID = string.Empty;
            string stepID = string.Empty;
           
            /* 차트 도입부 */
            if (fieldname == "LINE_ID")
                chartResult = chartResult.Where(x => x.LineID == fieldvalue).ToBindingList();

            else if (fieldname == "DESIGN_ID")
            {
                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx) != null)
                    lineID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx).ToString();

                BindingList<ResultItem> list = new BindingList<ResultItem>();
                foreach (var item in chartResult)
                {
                    if (string.IsNullOrEmpty(lineID) == false && item.LineID != lineID)
                        continue;

                    if (item.DesignID != fieldvalue)
                        continue;

                    list.Add(item);
                }

                chartResult = list;
                
            }
            else if (fieldname == "PRODUCT_ID")
            {
                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx) != null)
                    lineID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx) != null)
                    designID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx).ToString();

                BindingList<ResultItem> list = new BindingList<ResultItem>();
                foreach (var item in chartResult)
                {
                    if (string.IsNullOrEmpty(lineID) == false && item.LineID != lineID)
                        continue;

                    if (string.IsNullOrEmpty(designID) == false && item.DesignID != designID)
                        continue;

                    if (item.ProductID != fieldvalue)
                        continue;

                    list.Add(item);
                }

                chartResult = list;
            }
            else if (fieldname == "PROCESS_ID")
            {
                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 3], maxIdx) != null)
                    lineID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 3], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx) != null)
                    designID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx) != null)
                    prodID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx).ToString();

                BindingList<ResultItem> list = new BindingList<ResultItem>();
                foreach (var item in chartResult)
                {
                    if (string.IsNullOrEmpty(lineID) == false && item.LineID != lineID)
                        continue;

                    if (string.IsNullOrEmpty(designID) == false && item.DesignID != designID)
                        continue;

                    if (string.IsNullOrEmpty(prodID) == false && item.ProductID != prodID)
                        continue;

                    if (item.ProcessID != fieldvalue)
                        continue;

                    list.Add(item);
                }

                chartResult = list;
            }
            else if (fieldname == "STEP_ID")
            {
                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 4], maxIdx) != null)
                    lineID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 4], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 3], maxIdx) != null)
                    designID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 3], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx) != null)
                    prodID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx) != null)
                    procID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx).ToString();

                BindingList<ResultItem> list = new BindingList<ResultItem>();
                foreach (var item in chartResult)
                {
                    if (string.IsNullOrEmpty(lineID) == false && item.LineID != lineID)
                        continue;

                    if (string.IsNullOrEmpty(designID) == false && item.DesignID != designID)
                        continue;

                    if (string.IsNullOrEmpty(prodID) == false && item.ProductID != prodID)
                        continue;

                    if (string.IsNullOrEmpty(procID) == false && item.ProcessID != procID)
                        continue;

                    if (item.StepID != fieldvalue)
                        continue;

                    list.Add(item);
                }

                chartResult = list;
            }
            else if (fieldname == "SEQUENCE")
            {
                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 5], maxIdx) != null)
                    lineID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 5], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 4], maxIdx) != null)
                    designID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 4], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 3], maxIdx) != null)
                    prodID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 3], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx) != null)
                    procID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 2], maxIdx).ToString();

                if (pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx) != null)
                    stepID = pivot.GetFieldValue(info.ValueInfo.Data.Fields[fieldIdx - 1], maxIdx).ToString();

                BindingList<ResultItem> list = new BindingList<ResultItem>();
                foreach (var item in chartResult) 
                {
                    if (string.IsNullOrEmpty(lineID) == false && item.LineID != lineID)
                        continue;

                    if (string.IsNullOrEmpty(designID) == false && item.DesignID != designID)
                        continue;

                    if (string.IsNullOrEmpty(prodID) == false && item.ProductID != prodID)
                        continue;

                    if (string.IsNullOrEmpty(procID) == false && item.ProcessID != procID)
                        continue;

                    if (string.IsNullOrEmpty(stepID) == false && item.StepID != stepID)
                        continue;

                    if (item.Sequence != Convert.ToDecimal(fieldvalue))
                        continue;

                    list.Add(item);
                }

                chartResult = list;
            }

            DataTable chartTable = new DataTable();
            chartTable.Columns.Add("PRODUCT_ID", typeof(string));
            chartTable.Columns.Add("DATE", typeof(string));
            chartTable.Columns.Add("MOVE", typeof(int));
            chartTable.Columns.Add("TARGET", typeof(int));

            chartTable.PrimaryKey = new DataColumn[]
                {   
                    chartTable.Columns["PRODUCT_ID"], chartTable.Columns["DATE"]
                };

            List<string> SelProds = new List<string>();

            foreach (var row in chartResult)
            {
                DataRow eRow = chartTable.Rows.Find(new object[] { row.ProductID, row.Date });
                if (eRow != null)
                {
                    eRow["MOVE"] = Convert.ToInt32(eRow["MOVE"]) + row.PlanQty;
                    eRow["TARGET"] = Convert.ToInt32(eRow["TARGET"]) + row.TargetQty;
                }
                else
                {
                    DataRow drow = chartTable.NewRow();
                    drow["PRODUCT_ID"] = row.ProductID;
                    drow["DATE"] = row.Date;
                    drow["MOVE"] = row.PlanQty;
                    drow["TARGET"] = row.TargetQty;
                    chartTable.Rows.Add(drow);
                }

                if (SelProds.Contains(row.ProductID) == false)
                    SelProds.Add(row.ProductID);
            }

            DrawChart(chartTable, SelProds);
        }

        private void DrawChart(DataTable dtResult, List<string> SelProds)
        {
            DockPanel panel = null;
            if (this._mainView.ActiveControl != null)
                panel = this._mainView.ActiveControl as DockPanel;
            else
                panel = this._mainView.DockManager.ActivePanel;

            SubView subView = panel.ActiveControl as SubView;
            SplitGroupPanel chartPanel = subView.SplitContainerControl.Panel2;
            chartPanel.Controls.Clear();

            DataView dv = dtResult.DefaultView;
            dv.Sort = "DATE";
            dtResult = dv.ToTable();

            ChartControl chart = new ChartControl();
            chart.Dock = DockStyle.Fill;
            chartPanel.Controls.Add(chart);

            Series moveSeries = new Series("MOVE", ViewType.Line);
            Series targetSeries = new Series("TARGET", ViewType.Line);

            moveSeries.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;
            targetSeries.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

            (moveSeries.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            (targetSeries.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;

            chart.Series.Add(moveSeries);
            chart.Series.Add(targetSeries);

            Dictionary<string, List<decimal>> aggregateMappings = new Dictionary<string, List<decimal>>();
            foreach (DataRow row in dtResult.Rows)
            {
                string prodID = row["PRODUCT_ID"].ToString();
                string date = row["DATE"].ToString();
                decimal move = Convert.ToDecimal(row["MOVE"]);
                decimal target = Convert.ToDecimal(row["TARGET"]);

                if (SelProds.Contains(prodID) == false)
                    continue;

                if (SelProds.Count > 1) 
                {
                    var key = date;
                    if (aggregateMappings.ContainsKey(key))
                    {
                        aggregateMappings[key][0] += move;
                        aggregateMappings[key][1] += target;
                    }
                    else
                        aggregateMappings.Add(key, new List<decimal>() { move, target });
                    
                    continue;
                }

                SeriesPoint movePoint = new SeriesPoint(date, move);
                SeriesPoint targetPoint = new SeriesPoint(date, target);

                moveSeries.Points.Add(movePoint);
                targetSeries.Points.Add(targetPoint);
            }

            foreach (var item in aggregateMappings) 
            {
                SeriesPoint movePoint = new SeriesPoint(item.Key, item.Value.ElementAt(0));
                SeriesPoint targetPoint = new SeriesPoint(item.Key, item.Value.ElementAt(1));

                moveSeries.Points.Add(movePoint);
                targetSeries.Points.Add(targetPoint);
            }
        }

        private void SetPivotGridData(IEnumerable<ResultItem> dt, PivotGridControl pivotGrid)
        {
            this.BindBegin(pivotGrid);
            this.BindDo(dt, pivotGrid);
            this.BindEnd(pivotGrid);

            pivotGrid.BestFit();
        }

        private void BindBegin(PivotGridControl pivotGrid)
        {
            pivotGrid.BeginUpdate();
            pivotGrid.OptionsSelection.MultiSelect = true;
        }

        private void BindDo(IEnumerable<ResultItem> dt, PivotGridControl pivotGrid)
        {
            pivotGrid.DataSource = dt.ToBindingList();
        }

        private void BindEnd(PivotGridControl pivotGrid)
        {
            pivotGrid.EndUpdate();
        }

        private void SetPivotGridFields(PivotGridControl pivotGrid)
        {
            pivotGrid.Fields.Clear();

            var r1 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.LineID);
            r1.Caption = "LINE_ID";

            var r2 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.DesignID);
            r2.Caption = "DESIGN_ID";

            var r3 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ProductID);
            r3.Caption = "PRODUCT_ID";

            var r4 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ProcessID);
            r4.Caption = "PROCESS_ID";

            var r5 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.StepID);
            r5.Caption = "STEP_ID";

            var r6 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.Sequence);
            r6.Caption = "SEQUENCE";

            var c1 = pivotGrid.AddFieldColumnArea<ResultItem>((c) => c.Date);
            c1.Caption = "DATE";

            var d1 = pivotGrid.AddFieldDataArea<ResultItem>((c) => c.PlanQty);
            d1.Caption = "MOVE";

            var d2 = pivotGrid.AddFieldDataArea<ResultItem>((c) => c.TargetQty);
            d2.Caption = "TARGET";

            var f1 = pivotGrid.AddFieldFilterArea<ResultItem>((c) => c.MaterialGroup);
            f1.Caption = "MATERIAL_GROUP";

            var f2 = pivotGrid.AddFieldFilterArea<ResultItem>((c) => c.PkgFamily);
            f2.Caption = "PKG_FAMILY";

            var f3 = pivotGrid.AddFieldFilterArea<ResultItem>((c) => c.PkgType);
            f3.Caption = "PKG_TYPE";

            pivotGrid.BestFit();
        }

        class ResultItem
        {
            public string LineID { get; set; }
            public string DesignID { get; set; }
            public string ProductID { get; set; }
            public string ProcessID { get; set; }
            public string StepID { get; set; }
            public decimal Sequence { get; set; }
            public string Date { get; set; }
            public string MaterialGroup { get; set; }
            public string PkgFamily { get; set; }
            public string PkgType { get; set; }
            public double PlanQty { get; set; }
            public double TargetQty { get; set; }
        }

        private void buttonQuery_Click(object sender, EventArgs e)
        {
            this.buttonQuery.Enabled = false;

             WaitDialog waitDialog = new WaitDialog("Loading", string.Empty, new Size(260, 70), this.AppForm());

             try
             {
                 this.InitQueryOption();

                 string dataType = this.dataSelectRadioGroup.Properties.Items[this.dataSelectRadioGroup.SelectedIndex].Description;
                 string timeUnit = this.timeUnitComboBox.Text;
                 string queryType = string.Format("{0} ({1})", dataType, timeUnit);

                 waitDialog.SetCaption("Loading data...");
                 var dataList = this.GetDataList(queryType);

                 waitDialog.SetCaption(string.Format("Writing data... ({0} items)", dataList.Count()));
                 this.SetMainView(dataList, queryType);
             }
             finally
             {
                 if (waitDialog != null)
                     waitDialog.Close(this);
                 waitDialog = null;

                 this.buttonQuery.Enabled = true;
             }
        }

        private void InitQueryOption() 
        {
            this._selectedLineIDSet.Clear();
            this._selectedDesignIDSet.Clear();
            this._selectedFinalProdIDSet.Clear();
            this._selectedProdIDSet.Clear();

            foreach (var item in this.lineIDCheckedComboBox.ListBox.CheckedItems) 
            {
                this._selectedLineIDSet.Add(item.ToString());
            }

            foreach (var item in this.designIDCheckedComboBox.ListBox.CheckedItems)
            {
                this._selectedDesignIDSet.Add(item.ToString());
            }

            foreach (var item in this.finalProdIDCheckedComboBox.ListBox.CheckedItems)
            {
                this._selectedFinalProdIDSet.Add(item.ToString());
            }

            var mcpBom = this._modelDataContext.McpBom;
          
            foreach (var finalProdID in _selectedFinalProdIDSet) 
            {
                List<McpBom> mcpBoms = null;
                if (this._mcpBomMappings.TryGetValue(finalProdID, out mcpBoms)) 
                {
                    foreach (var item in mcpBoms)
                    {
                        this._selectedProdIDSet.Add(item.ASSY_IN_PROD_ID);
                        this._selectedProdIDSet.Add(item.TO_PROD_ID);
                    }
                }
            }

            this._startTime = this.startDateEdit.DateTime;
            this._endTime = this.startDateEdit.DateTime.AddDays((double)this.durationSpinEdit.Value);
        }
    }
}
