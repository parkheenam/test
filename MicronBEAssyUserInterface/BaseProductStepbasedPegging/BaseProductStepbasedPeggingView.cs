using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using DevExpress.XtraPivotGrid;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.Projects;
using MicronBEAssy;
using DevExpress.XtraEditors.Controls;
using Mozart.Text;
using System.Collections;
using MicronBEAssy.Inputs;
using MicronBEAssyUserInterface.Class;
using Mozart.Collections;
using MicronBEAssy.Outputs;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraWaitForm;
using System.Threading;
using System.Windows.Threading;
using Mozart.Studio.UserInterface;
using Mozart.Studio.UIComponents;
using Mozart.Studio.Documents;
using System.ComponentModel.Design;
using Mozart.Studio.Application;
using Mozart.Studio.Projects;
using DevExpress.Skins;
using DevExpress.LookAndFeel;

namespace MicronBEAssyUserInterface.BaseProductStepbasedPegging
{
    public partial class BaseProductStepbasedPeggingView : XtraUserControlView
    {
        IExperimentResultItem _result;

        ResultDataContext _resultCtx;

        ModelDataContext _modelDataContext;

        ExpDataContext _expDataContext;

        HashedSet<string> _selectedLineIDSet;

        string _selectedPlanView;

        HashSet<string> _selectedDesignIDSet;

        string _selectedProdID;

        string _selectedProdName;

        Dictionary<IComparable, RowInfo> _rowInfoMappings;

        Dictionary<IComparable, StdStepColumnInfo> _columnInfoMappings;

        Dictionary<IComparable, HashSet<UIProduct>> _demandMatchingProductMappings;

        Dictionary<string, decimal> _stdStepSequenceMappings;

        Dictionary<string, int> _UIvalueNameSequenceMappings;

        Dictionary<StdStep, List<Wip>> _stdStepWipMappings;

        Dictionary<Tuple<string, string, string, string>, decimal> _pegQtyMappings;

        Dictionary<Tuple<string, string>, List<UnPegHistory>> _unpegHistoryMappings;

        Dictionary<Tuple<string, int>, McpBom> _mcpBomMappings;

        Dictionary<Tuple<string, string, string>, ActInfo> _actInfoMappings;

        Dictionary<string, int> _firstWireBondDACountMappings;

        Dictionary<string, int> _baseCompQtyMappings;

        Dictionary<string, List<Wip>> _wipMappings;

        Dictionary<string, List<Demand>> _demandMappings;

        Dictionary<Tuple<string, string, bool, bool, int>, UIProduct> _prodRouteInfos;

        Dictionary<Tuple<string, string>, UIProductDetail> _prodDetailInfos;

        Dictionary<Tuple<string, string, int>, decimal> _stdStepDACountMappings;

        Dictionary<Tuple<string, decimal>, StdStep> _stdStepMappings;

        Dictionary<Tuple<string, string, string, decimal>, List<UnPegHistory>> _stdStepUnpegHistoryMappings;

        public BaseProductStepbasedPeggingView(IServiceProvider serviceProvider)
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
            this._selectedLineIDSet = new HashedSet<string>();
            this._selectedDesignIDSet = new HashSet<string>();
            this._selectedPlanView = string.Empty;
            this._selectedProdID = string.Empty;
            this._selectedProdName = string.Empty;
           
            this._rowInfoMappings = new Dictionary<IComparable, RowInfo>();
            this._columnInfoMappings = new Dictionary<IComparable, StdStepColumnInfo>();
            this._demandMatchingProductMappings = new Dictionary<IComparable, HashSet<UIProduct>>();
            this._stdStepSequenceMappings = new Dictionary<string, decimal>();
            this._stdStepWipMappings = new Dictionary<StdStep, List<Wip>>();
            this._mcpBomMappings = new Dictionary<Tuple<string, int>, McpBom>();
            this._actInfoMappings = new Dictionary<Tuple<string, string, string>, ActInfo>();
            this._pegQtyMappings = new Dictionary<Tuple<string, string, string, string>, decimal>();
            this._unpegHistoryMappings = new Dictionary<Tuple<string, string>, List<UnPegHistory>>();
            this._firstWireBondDACountMappings = new Dictionary<string, int>();
            this._UIvalueNameSequenceMappings = new Dictionary<string, int>();
            this._baseCompQtyMappings = new Dictionary<string, int>();
            this._wipMappings = new Dictionary<string, List<Wip>>();
            this._demandMappings = new Dictionary<string, List<Demand>>();
            this._stdStepUnpegHistoryMappings = new Dictionary<Tuple<string, string, string, decimal>, List<UnPegHistory>>();

            this._resultCtx = this._result.GetCtx<ResultDataContext>();
            this._modelDataContext = this._result.GetCtx<ModelDataContext>();
            this._expDataContext = this._result.GetCtx<ExpDataContext>();

            this._prodRouteInfos = DataHelper.LoadProductRoute(this._resultCtx);
            Dictionary<Tuple<string, string>, UIProcess> processList = DataHelper.LoadProcessStep(this._resultCtx);
            this._prodDetailInfos = DataHelper.LoadProductDetail(this._resultCtx, processList);
            this._stdStepDACountMappings = new Dictionary<Tuple<string, string, int>, decimal>();
            this._stdStepMappings = new Dictionary<Tuple<string, decimal>, StdStep>();

            InitializeControl();
        }

        private void InitializeControl()
        {
            this.InitLineIDComboBox();
            this.InitDesignIDComboBox();
            this.InitPlanViewComboBox();

            this.SetPivotGridFields(pivotGridControl);

            this.InitStepSequenceMappings();
            this.InitValueNameSequenceMappings();
            this.InitStdStepDACountMappings();
            this.InitStdStepMappings();
            this.InitPegQtyMappings();
            this.InitUnpegQtyMappings();
            this.InitMcpBomMappings();
            this.InitBaseCompQty();
            this.InitActInfoMappings();
            this.InitFirstWireBondDACount();
            this.InitWipMappings();
            this.InitDemandMappings();
        }

        private void buttonQuery_Click(object sender, EventArgs e)
        {
            this.buttonQuery.Enabled = false;
           
            WaitDialog waitDialog = new WaitDialog("Loading", string.Empty, new Size(260, 70), this.AppForm());
            
            try
            {
                this.InitQueryOption();

                waitDialog.SetCaption("Loading data...");
                var dataList = this.GetDataList();

                waitDialog.SetCaption(string.Format("Writing data... ({0} items)", dataList.Count()));
                this.InitPivotGrid(dataList, pivotGridControl);
            }
            finally
            {
                if (waitDialog != null)
                    waitDialog.Close(this);
                waitDialog = null;

                this.buttonQuery.Enabled = true;
            }
        }

        private void InitWipMappings()
        {
            var wips = this._modelDataContext.Wip;
            foreach (var wip in wips)
            {
                List<Wip> list = null;
                var key = wip.PRODUCT_ID;
                if (this._wipMappings.TryGetValue(key, out list))
                {
                    list.Add(wip);
                }
                else
                {
                    list = new List<Wip>();
                    list.Add(wip);
                    this._wipMappings[key] = list;
                }
            }
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

            //this.lineIDCheckedComboBox.ListBox.Items.Add(new CheckedListBoxItem("Select All", true));

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

            //this.designIDCheckedComboBox.ListBox.Items.Add(new CheckedListBoxItem("Select All", true));

            foreach (var item in designIDSet)
                this.designIDCheckedComboBox.ListBox.Items.Add(new CheckedListBoxItem(item, true));

            this.designIDCheckedComboBox.Select();
        }

        private void InitPlanViewComboBox()
        {
            var demand = this._modelDataContext.Demand;
            var planViewSet = new HashSet<string>();

            foreach (var item in demand)
            {
                if (string.IsNullOrEmpty(item.WEEK_NO))
                    continue;

                if (planViewSet.Contains(item.WEEK_NO) == false)
                    planViewSet.Add(item.WEEK_NO);
            }

            planViewSet.OrderBy(x => x);

            foreach (var item in planViewSet)
                this.planViewComboBox.Items.Add(item);
        }

        private void InitBaseCompQty()
        {
            var prodMaster = this._modelDataContext.ProductMaster;

            foreach (var prod in prodMaster)
            {
                int compQty = 0;
                string productID = prod.PRODUCT_ID;
                McpBom mcpBom = null;
                var key = Tuple.Create(productID, 1);
                this._mcpBomMappings.TryGetValue(key, out mcpBom);

                if (mcpBom == null)
                    continue;

                compQty = mcpBom.COMP_QTY;
                this._baseCompQtyMappings[productID] = compQty;
            }
        }

        private void InitStepSequenceMappings()
        {
            var stdStep = this._modelDataContext.StdStep;
            foreach (var item in stdStep)
            {
                string key = this.GetStepColumnKey(item);
                this._stdStepSequenceMappings[key] = item.SEQUENCE;
            }
        }

        private void InitStdStepDACountMappings()
        {
            var stdStep = this._modelDataContext.StdStep;
            int daCount = 0;

            foreach (var item in stdStep)
            {
                var lineID = item.LINE_ID;
                var stepID = item.STEP_ID;
                var seq = item.SEQUENCE;

                if (item.STEP_ID.Contains("DIE ATTACH") && item.STEP_ID.Contains("CURE") == false)
                    daCount++;

                this._stdStepDACountMappings[Tuple.Create(lineID, stepID, daCount)] = seq;
            }
        }

        private void InitStdStepMappings()
        {
            var stdStep = this._modelDataContext.StdStep;

            foreach (var item in stdStep)
            {
                var lineID = item.LINE_ID;
                var seq = item.SEQUENCE;

                this._stdStepMappings[Tuple.Create(lineID, seq)] = item;
            }
        }

        private void InitPegQtyMappings()
        {
            var pegHistory = this._resultCtx.PegHistory;
            foreach (var item in pegHistory)
            {
                if (item.IS_BASE == "N")
                    continue;

                var key = Tuple.Create(item.LINE_ID, item.STEP_ID, item.PRODUCT_ID, item.MO_PRODUCT_ID);

                if (this._pegQtyMappings.ContainsKey(key))
                    this._pegQtyMappings[key] += item.PEG_QTY;
                else
                    this._pegQtyMappings[key] = item.PEG_QTY;
            }
        }

        private void InitFirstWireBondDACount()
        {
            var prodMaster = this._modelDataContext.ProductMaster;
            foreach (var prod in prodMaster)
            {
                string lineID = prod.LINE_ID;
                string prodID = prod.PRODUCT_ID;
                
                int daCount = 0;

                UIProductDetail detail = null;
                var key = Tuple.Create(lineID, prodID);
                this._prodDetailInfos.TryGetValue(key, out detail);

                if (detail == null)
                    continue;

                if (detail.Process == null)
                    continue;

                var chars = detail.Process.DAWBProcess.GetEnumerator();
                while (chars.MoveNext())
                {
                    if (chars.Current == 'D')
                    {
                        daCount++;
                        continue;
                    }

                    if (chars.Current == 'W')
                        break;
                }

                this._firstWireBondDACountMappings[prodID] = daCount;
            }
        }

        private void InitUnpegQtyMappings()
        {
            var unpegHistory = this._resultCtx.UnPegHistory;
            foreach (var item in unpegHistory)
            {
                var key = Tuple.Create(item.LINE_ID, item.PRODUCT_ID);
                List<UnPegHistory> list = null;
                if (this._unpegHistoryMappings.TryGetValue(key, out list))
                    list.Add(item);
                else
                {
                    list = new List<UnPegHistory>();
                    list.Add(item);
                    this._unpegHistoryMappings[key] = list;
                }
            }
        }

        private void InitValueNameSequenceMappings()
        {
            this._UIvalueNameSequenceMappings.Add("WipQty", 1);
            this._UIvalueNameSequenceMappings.Add("PeggedQty", 2);
            this._UIvalueNameSequenceMappings.Add("UnpeggedQty", 3);
            this._UIvalueNameSequenceMappings.Add("RemainQty", 4);
        }

        private void InitMcpBomMappings()
        {
            var mcpBom = this._modelDataContext.McpBom;
            foreach (var item in mcpBom)
            {
                var key = Tuple.Create(item.FINAL_PROD_ID, item.COMP_SEQ);
                this._mcpBomMappings[key] = item;
            }
        }

        private void InitActInfoMappings()
        {
            var actInfo = this._modelDataContext.ActInfo;
            foreach (var item in actInfo)
            {
                var key = Tuple.Create(item.DEMAND_ID, item.LINE_ID, item.PRODUCT_ID);
                this._actInfoMappings[key] = item;
            }
        }

        private IEnumerable<ResultItem> GetDataList()
        {
            this.InitRowInfos();
            var resultItems = this.GetResultItemList();

            return resultItems;
        }

        private void InitDemandMappings()
        {
            var demand = this._modelDataContext.Demand;
            foreach (var item in demand)
            {
                List<Demand> list = null;
                var key = item.PRODUCT_ID;
                if (this._demandMappings.TryGetValue(key, out list))
                {
                    list.Add(item);
                }
                else
                {
                    list = new List<Demand>();
                    list.Add(item);
                    this._demandMappings[key] = list;
                }
            }
        }

        private void InitRowInfos()
        {
            var demand = this._modelDataContext.Demand;
            foreach (var item in this._demandMappings.Values)
            {
                var sampleDemand = item.FirstOrDefault();
                string lineID = sampleDemand.LINE_ID;
                if (this._selectedLineIDSet.Contains(lineID) == false)
                    continue;

                string prodID = sampleDemand.PRODUCT_ID;
                if (string.IsNullOrEmpty(this._selectedProdID) == false && LikeUtility.Like(prodID, this._selectedProdID) == false)
                    continue;

                var key = Tuple.Create(lineID, prodID);
                UIProductDetail prodDetail = null;
                if (this._prodDetailInfos.TryGetValue(key, out prodDetail) == false)
                    continue;

                string prodName = prodDetail.ProductName;
                if (string.IsNullOrEmpty(this._selectedProdName) == false && LikeUtility.Like(prodName, this._selectedProdName) == false)
                    continue;

                string designID = prodDetail.DesignID;
                if (this._selectedDesignIDSet.Contains(designID) == false)
                    continue;

                if (this._selectedPlanView != Constants.ALL)
                {
                    int planView = -1;
                    string planViewString = this._selectedPlanView.Remove(4, 1);
                    int.TryParse(planViewString, out planView);

                    if (planView != -1)
                    {
                        int weekNo = -1;
                        string weekNoString = sampleDemand.WEEK_NO.Remove(4, 1);
                        int.TryParse(weekNoString, out weekNo);

                        if (weekNo > planView)
                            continue;
                    }
                }
         
                this.AddDemandMathcingProducts(sampleDemand);

                int baseCompQty = this.GetBaseCompQty(prodID);
                decimal demandQty = this.GetDemandQty(prodID);
                decimal actQty = this.GetActQty(prodID, sampleDemand.LINE_ID, sampleDemand.DEMAND_ID);
                decimal totalRemainQty = demandQty - actQty;

                RowInfo rowInfo = this.CreateRowInfo(prodDetail, baseCompQty, demandQty, actQty, totalRemainQty);
                this._rowInfoMappings[prodID] = rowInfo;
            }
        }

        private decimal GetDemandQty(string productID)
        {
            List<Demand> list = null;
            this._demandMappings.TryGetValue(productID, out list);

            decimal qty = 0;
            foreach (var item in list)
            {
                if (this._selectedPlanView != Constants.ALL)
                {
                    int planView = -1;
                    string planViewString = this._selectedPlanView.Remove(4, 1);
                    int.TryParse(planViewString, out planView);

                    if (planView != -1)
                    {
                        int weekNo = -1;
                        string weekNoString = item.WEEK_NO.Remove(4, 1);
                        int.TryParse(weekNoString, out weekNo);

                        if (weekNo > planView)
                            continue;
                    }
                }

                qty += item.QTY;
            }

            return qty;
        }

        private RowInfo CreateRowInfo(UIProductDetail productDetail, int baseCompQty, decimal demandQty, decimal actQty, decimal remainQty)
        {
            RowInfo info = new RowInfo(productDetail, baseCompQty, demandQty, actQty, remainQty);
            return info;
        }

        private void AddDemandMathcingProducts(Demand demand)
        {
            UIProduct prod = DataHelper.FindProduct(demand.LINE_ID, demand.PRODUCT_ID);

            if (prod == null)
                return;

            if (prod.Prevs == null)
                return;

            var prevs = prod.Prevs.GetEnumerator();
            var key = Tuple.Create(demand.LINE_ID, demand.PRODUCT_ID);

            while (prevs.MoveNext())
            {
                var prev = prevs.Current;
                if (prev.IsBase == false && prev.StepID != "LOTS RECEIVED")
                    continue;

                HashSet<UIProduct> prodSet = null;
                if (this._demandMatchingProductMappings.TryGetValue(key, out prodSet))
                    prodSet.Add(prev);
                else
                    this._demandMatchingProductMappings.Add(key, new HashSet<UIProduct>() { prev });

                if (prev.StepID == "LOTS RECEIVED")
                    continue;

                if (prev.Prevs == null)
                    break;

                prevs = prev.Prevs.GetEnumerator();
            }

        }

        private IEnumerable<ResultItem> GetResultItemList()
        {
            var stdSteps = this._modelDataContext.StdStep.OrderByDescending(x => x.SEQUENCE);
            List<ResultItem> list = new List<ResultItem>();
           
            foreach (var rowInfo in this._rowInfoMappings.Values)
            {
                foreach (var step in stdSteps)
                {
                    var demandKey = Tuple.Create(rowInfo.ProductDetail.LineID, rowInfo.ProductDetail.ProductID);
                    HashSet<UIProduct> matchingProds = null;
                    if (this._demandMatchingProductMappings.TryGetValue(demandKey, out matchingProds) == false)
                    {
                        this.AddZeroValuedResultItems(rowInfo, step, list);
                        continue;
                    }

                    this.AddWipQty(rowInfo, step, list);
                    this.AddPeggedQty(rowInfo, step, list);
                    this.AddUnpeggedQty(rowInfo, step, list);
                    this.AddRemainQty(rowInfo, step, list);
                }

            }

            return list;
        }

        private StdStepColumnInfo CreateStdStepPegInfo(string productID, string stepID, decimal sequence, List<Wip> matchWips, decimal wipQty, decimal peggedQty, decimal unpeggedQty, decimal remainQty)
        {
            StdStepColumnInfo pegInfo = new StdStepColumnInfo(productID, stepID, sequence, matchWips, wipQty, peggedQty, unpeggedQty, remainQty);
            return pegInfo;
        }

        private IComparable GetStdStepColumnInfoKey(RowInfo rowInfo, StdStep step)
        {
            return Tuple.Create(rowInfo.ProductDetail.LineID, rowInfo.ProductDetail.ProductID, step.STEP_ID, step.SEQUENCE);
        }

        private void AddZeroValuedResultItems(RowInfo rowInfo, StdStep step, List<ResultItem> list)
        {
            UIProductDetail productDetail = rowInfo.ProductDetail;
            int baseCompQty = rowInfo.BaseCompQty;
            decimal demandQty = rowInfo.DemandQty;
            decimal actQty = rowInfo.ActQty;
            decimal totalRemainQty = rowInfo.TotalRemainQty;
          
            list.Add(this.CreateResultItem(productDetail, baseCompQty, demandQty, actQty, totalRemainQty, step, "WipQty", 0));
            list.Add(this.CreateResultItem(productDetail, baseCompQty, demandQty, actQty, totalRemainQty, step, "PeggedQty", 0));
            list.Add(this.CreateResultItem(productDetail, baseCompQty, demandQty, actQty, totalRemainQty, step, "UnpeggedQty", 0));
            list.Add(this.CreateResultItem(productDetail, baseCompQty, demandQty, actQty, totalRemainQty, step, "RemainQty", 0));

            var key = this.GetStdStepColumnInfoKey(rowInfo, step);
            this._columnInfoMappings[key] = this.CreateStdStepPegInfo(rowInfo.ProductDetail.ProductID, step.STEP_ID, step.SEQUENCE, null, 0, 0, 0, 0);
        }

        private void AddWipQty(RowInfo rowInfo, StdStep step, List<ResultItem> list)
        {
            string valueName = "WipQty";
            var demandKey = Tuple.Create(rowInfo.ProductDetail.LineID, rowInfo.ProductDetail.ProductID);
         
            HashSet<UIProduct> matchingProds = null;
            this._demandMatchingProductMappings.TryGetValue(demandKey, out matchingProds);

            decimal wipQty = this.CalculateWipQty(matchingProds, step);
            ResultItem item = this.CreateResultItem(rowInfo.ProductDetail, rowInfo.BaseCompQty, rowInfo.DemandQty, rowInfo.ActQty, rowInfo.TotalRemainQty, step, valueName, wipQty);

            list.Add(item);

            var key = this.GetStdStepColumnInfoKey(rowInfo, step);
            this._columnInfoMappings[key] = this.CreateStdStepPegInfo(rowInfo.ProductDetail.ProductID, step.STEP_ID, step.SEQUENCE, null, wipQty, 0, 0, 0);
        }

        private void AddPeggedQty(RowInfo rowInfo, StdStep step, List<ResultItem> list)
        {
            string valueName = "PeggedQty";
            var demandKey = Tuple.Create(rowInfo.ProductDetail.LineID, rowInfo.ProductDetail.ProductID);
            HashSet<UIProduct> matchingProds = null;
            this._demandMatchingProductMappings.TryGetValue(demandKey, out matchingProds);

            decimal peggedQty = this.CalculatePeggedQty(rowInfo, matchingProds, step);
            ResultItem item = this.CreateResultItem(rowInfo.ProductDetail, rowInfo.BaseCompQty, rowInfo.DemandQty, rowInfo.ActQty, rowInfo.TotalRemainQty, step, valueName, (decimal)peggedQty);

            list.Add(item);

            var key = this.GetStdStepColumnInfoKey(rowInfo, step);
            StdStepColumnInfo columnInfo = null;
            if (this._columnInfoMappings.TryGetValue(key, out columnInfo))
            {
                columnInfo.PeggedQty = (decimal)peggedQty;
            }
            else
                this._columnInfoMappings[key] = this.CreateStdStepPegInfo(rowInfo.ProductDetail.ProductID, step.STEP_ID, step.SEQUENCE, null, 0, (decimal)peggedQty, 0, 0);
        }

        private void AddUnpeggedQty(RowInfo rowInfo, StdStep step, List<ResultItem> list)
        {
            string valueName = "UnpeggedQty";
            var demandKey = Tuple.Create(rowInfo.ProductDetail.LineID, rowInfo.ProductDetail.ProductID);
            HashSet<UIProduct> matchingProds = null;
            this._demandMatchingProductMappings.TryGetValue(demandKey, out matchingProds);

            decimal unpeggedQty = this.CalculateUnppegedQty(rowInfo, matchingProds, step);
            ResultItem item = this.CreateResultItem(rowInfo.ProductDetail, rowInfo.BaseCompQty, rowInfo.DemandQty, rowInfo.ActQty, rowInfo.TotalRemainQty, step, valueName, unpeggedQty);

            list.Add(item);

            var key = this.GetStdStepColumnInfoKey(rowInfo, step);
            StdStepColumnInfo columnInfo = null;
            if (this._columnInfoMappings.TryGetValue(key, out columnInfo))
            {
                columnInfo.UnpeggedQty = (decimal)unpeggedQty;
            }
            else
                this._columnInfoMappings[key] = this.CreateStdStepPegInfo(rowInfo.ProductDetail.ProductID, step.STEP_ID, step.SEQUENCE, null, 0, 0, (decimal)unpeggedQty, 0);
        }

        private void AddRemainQty(RowInfo rowInfo, StdStep step, List<ResultItem> list)
        {
            string valueName = "RemainQty";

            decimal remainQty = this.CalculateRemainQty(rowInfo, step);
            ResultItem item = this.CreateResultItem(rowInfo.ProductDetail, rowInfo.BaseCompQty, rowInfo.DemandQty, rowInfo.ActQty, rowInfo.TotalRemainQty, step, valueName, remainQty);

            list.Add(item);

            var key = this.GetStdStepColumnInfoKey(rowInfo, step);
            StdStepColumnInfo columnInfo = null;
            if (this._columnInfoMappings.TryGetValue(key, out columnInfo))
            {
                columnInfo.RemainQty = (decimal)remainQty;
            }
            else
                this._columnInfoMappings[key] = this.CreateStdStepPegInfo(rowInfo.ProductDetail.ProductID, step.STEP_ID, step.SEQUENCE, null, 0, 0, 0, (decimal)remainQty);
        }

        private decimal CalculateWipQty(HashSet<UIProduct> matchingProds, StdStep step)
        {
            decimal qty = 0;

            List<Wip> wips = new List<Wip>();
            HashSet<string> prodIDSet = new HashSet<string>();
            foreach (var matchProd in matchingProds)
            {
                if (prodIDSet.Contains(matchProd.ProductID))
                    continue;

                prodIDSet.Add(matchProd.ProductID);

                List<Wip> wipList = null;
                this._wipMappings.TryGetValue(matchProd.ProductID, out wipList);

                if (wipList == null)
                    continue;

                foreach (var item in wipList)
                {
                    wips.Add(item);
                }

            }

            foreach (var wip in wips)
            {
                bool isMatch = this.IsWipMatch(wip, step);
                if (isMatch == false)
                    continue;

                qty += wip.LOT_QTY;

                List<Wip> matchWips = null;
                if (this._stdStepWipMappings.TryGetValue(step, out matchWips))
                {
                    matchWips.Add(wip);
                }
                else
                {
                    matchWips = new List<Wip>();
                    matchWips.Add(wip);
                    this._stdStepWipMappings[step] = matchWips;
                }
            }

            return qty;
        }

        private decimal CalculatePeggedQty(RowInfo rowInfo, HashSet<UIProduct> matchingProds, StdStep step)
        {
            decimal qty = 0;
            List<Wip> matchWips = null;
            if (this._stdStepWipMappings.TryGetValue(step, out matchWips) == false)
                return qty;

            #region DEBUG
            if(step.STEP_ID == "LOTS RECEIVED")
                Console.WriteLine();
            #endregion

            HashSet<IComparable> set = new HashSet<IComparable>();
            foreach (var wip in matchWips)
            {
                var key = Tuple.Create(wip.LINE_ID, wip.STEP_ID, wip.PRODUCT_ID, rowInfo.ProductDetail.ProductID);
                if (set.Contains(key))
                    continue;

                decimal pegQty = 0;
                if (this._pegQtyMappings.TryGetValue(key, out pegQty))
                    qty += pegQty;

                set.Add(key);
            }

            if (step.SEQUENCE < 41)
            {
                int baseCompQty = 0;
                this._baseCompQtyMappings.TryGetValue(rowInfo.ProductDetail.ProductID, out baseCompQty);

                if (baseCompQty != 0)
                    qty = qty / baseCompQty;
            }

            return qty;
        }

        private decimal CalculateUnppegedQty(RowInfo rowInfo, HashSet<UIProduct> matchingProds, StdStep step)
        {
            var unpegHistory = this._resultCtx.UnPegHistory;

            decimal qty = 0;
            List<UnPegHistory> unpegs = new List<UnPegHistory>();
            HashSet<string> prodIDSet = new HashSet<string>();
            foreach (var matchProd in matchingProds)
            {
                if (prodIDSet.Contains(matchProd.ProductID))
                    continue;

                prodIDSet.Add(matchProd.ProductID);

                var key = Tuple.Create(matchProd.LineID, matchProd.ProductID);
                List<UnPegHistory> list = null;
                this._unpegHistoryMappings.TryGetValue(key, out list);

                if (list == null)
                    continue;

                foreach (var item in list)
                    unpegs.Add(item);
            }

            foreach (var item in unpegs)
            {
                List<UnPegHistory> unpeggedList = null;
                var key = this.GetStdStepColumnInfoKey(rowInfo, step) as Tuple<string, string, string, decimal>;

                bool isMatch = this.IsWipMatch(item.LINE_ID, item.PRODUCT_ID, item.STEP_ID, step);
                if (isMatch == false)
                    continue;

                qty += item.UNPEG_QTY;

                if (this._stdStepUnpegHistoryMappings.TryGetValue(key, out unpeggedList))
                    unpeggedList.Add(item);
                else
                {
                    unpeggedList = new List<UnPegHistory>();
                    unpeggedList.Add(item);
                    this._stdStepUnpegHistoryMappings[key] = unpeggedList;
                }
            }

            return qty;
        }

        private decimal CalculateRemainQty(RowInfo rowInfo, StdStep step)
        {
            decimal qty = 0;

            StdStep nextStep = this.GetNextStdStep(step);
            StdStepColumnInfo nextColumnInfo = null;
            if (nextStep != null)
            {
                var nextKey = this.GetStdStepColumnInfoKey(rowInfo, nextStep);
                this._columnInfoMappings.TryGetValue(nextKey, out nextColumnInfo);
            }

            StdStepColumnInfo columnInfo = null;
            var key = this.GetStdStepColumnInfoKey(rowInfo, step);
            this._columnInfoMappings.TryGetValue(key, out columnInfo);

            if (nextColumnInfo == null)
            {
                if (columnInfo == null)
                    qty = rowInfo.TotalRemainQty;
                else
                    qty = rowInfo.TotalRemainQty - columnInfo.PeggedQty;
            }
            else
            {
                if (columnInfo == null)
                    qty = nextColumnInfo.RemainQty;
                else
                    qty = nextColumnInfo.RemainQty - columnInfo.PeggedQty;
            }

            return qty;
        }

        private StdStep GetNextStdStep(StdStep step)
        {
            var key = Tuple.Create(step.LINE_ID, step.SEQUENCE + 1);
            StdStep stdStep = null;
            this._stdStepMappings.TryGetValue(key, out stdStep);

            return stdStep;
        }

        private bool IsWipMatch(Wip wip, StdStep step)
        {
            if (step.STEP_ID.StartsWith("PRE WB") == false && step.STEP_ID.StartsWith("WIRE BOND") == false)
            {
                #region DEBUG
                if (wip.STEP_ID == "AWAIT STACKING INV")
                    Console.WriteLine();
                #endregion

                if (wip.STEP_ID == step.STEP_ID)
                    return true;
            }
           
            var stdStepKey = this.GetStdStepKey(step);
            bool isExist = false;
            var wipStdStepKey = this.GetStdStepKey(wip.LINE_ID, wip.PRODUCT_ID, wip.STEP_ID, out isExist);

            if (isExist == false)
                return false;

            if (stdStepKey.CompareTo(wipStdStepKey) == 0)
                return true;

            return false;
        }

        private bool IsWipMatch(string lineID, string productID, string stepID, StdStep step)
        {
            if (step.STEP_ID.StartsWith("PRE WB") == false && step.STEP_ID.StartsWith("WIRE BOND") == false)
            {
                if (stepID == step.STEP_ID)
                    return true;
            }

            var stepKey = this.GetStdStepKey(step);
            bool isExist = false;
            var wipStepKey = this.GetStdStepKey(lineID, productID, stepID, out isExist);

            if (isExist == false)
                return false;

            if (stepKey.CompareTo(wipStepKey) == 0)
                return true;

            return false;
        }

        private IComparable GetStdStepKey(string lineID, string productID, string stepID, out bool isExist)
        {
            string prodID = productID;
            string procID = string.Empty;

            UIProductDetail prodDetail = null;
            this._prodDetailInfos.TryGetValue(Tuple.Create(lineID, prodID), out prodDetail);

            if (prodDetail == null)
            {
                isExist = false;
                return default(IComparable);
            }

            UIStep step = prodDetail.Process.FindStep(stepID);

            int count = 0;
            if (step != null)
                count = step.DaThroughCount;

            if (stepID.StartsWith("PRE WB"))
                stepID = "PRE WB AR PLASMA";

            if (stepID.StartsWith("WIRE BOND"))
                stepID = "WIRE BOND";

            if (stepID.StartsWith("STRIP SORTER"))
            {
                stepID = "WIRE BOND";
                count = this.GetFirstWireBondDACount(productID);
            }

            var key = Tuple.Create(lineID, stepID, count);
            decimal seq = 0;
            if (this._stdStepDACountMappings.TryGetValue(key, out seq))
                isExist = true;
            else
                isExist = false;

            var stepKey = Tuple.Create(lineID, stepID, seq);
            return stepKey;
        }

        private IComparable GetStdStepKey(StdStep step)
        {
            return Tuple.Create(step.LINE_ID, step.STEP_ID, step.SEQUENCE);
        }

        private ResultItem CreateResultItem(UIProductDetail productDetail, int baseCompQty, decimal demandQty, decimal actQty, decimal totalRemainQty, StdStep step, string valueName, decimal value)
        {
            ResultItem item = new ResultItem();

            item.ProductID = productDetail.ProductID;
            item.ProductName = productDetail.ProductName;
            item.ProcessSeq = productDetail.Process == null ? string.Empty : productDetail.Process.DAWBProcess;
            item.BaseCompQty = baseCompQty;
            item.DemandQty = demandQty;
            item.ActQty = actQty;
            item.TotalRemainQty = totalRemainQty;
            item.StepID = this.GetStepColumnKey(step);
            item.ValueName = valueName;
            item.Value = value;

            return item;
        }
      
        private int GetFirstWireBondDACount(string productID)
        {
            int daCount = 0;
            this._firstWireBondDACountMappings.TryGetValue(productID, out daCount);
            return daCount;
        }

        private int GetBaseCompQty(string productID)
        {
            int compQty = 0;
            this._baseCompQtyMappings.TryGetValue(productID, out compQty);

            return compQty;
        }

        private decimal GetActQty(string demandID, string lineID, string productID)
        {
            decimal actQty = 0;
            Tuple<string, string, string> key = Tuple.Create(demandID, lineID, productID);

            ActInfo actInfo = null;
            this._actInfoMappings.TryGetValue(key, out actInfo);

            if (actInfo == null)
                return actQty;

            actQty = actInfo.ACT_QTY;

            return actQty;
        }

        private void InitQueryOption()
        {
            this._selectedLineIDSet.Clear();
            this._selectedDesignIDSet.Clear();
            this._selectedProdID = string.Empty;
            this._selectedProdName = string.Empty;
            this._selectedPlanView = string.Empty;

            this._rowInfoMappings.Clear();
            this._columnInfoMappings.Clear();
            this._stdStepWipMappings.Clear();
            this._stdStepUnpegHistoryMappings.Clear();
            
            foreach (var item in this.lineIDCheckedComboBox.ListBox.CheckedItems)
            {
                this._selectedLineIDSet.Add(item.ToString());
            }

            foreach (var item in this.designIDCheckedComboBox.ListBox.CheckedItems)
            {
                this._selectedDesignIDSet.Add(item.ToString());
            }

            this._selectedPlanView = this.planViewComboBox.Text;
            this._selectedProdID = this.productIDTextBox.Text;
            this._selectedProdName = this.productNameTextBox.Text;
        }
  
        private void SetPivotGridFields(PivotGridControl pivotGrid)
        {
            pivotGrid.Fields.Clear();

            var r1 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ProductID);
            r1.Caption = "PRODUCT_ID";

            var r2 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ProductName);
            r2.Caption = "PRODUCT_NAME";

            var r3 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ProcessSeq);
            r3.Caption = "PROCESS";

            var r4 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.BaseCompQty);
            r4.Caption = "BASE_COMP_QTY";

            var r5 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.DemandQty);
            r5.Caption = "DEMAND_QTY";

            var r6 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ActQty);
            r6.Caption = "ACT_QTY";

            var r7 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.TotalRemainQty);
            r7.Caption = "REMAIN_QTY";

            var r8 = pivotGrid.AddFieldRowArea<ResultItem>((c) => c.ValueName);
            r8.Caption = "VALUE_NAME";
            r8.SortMode = PivotSortMode.Custom;

            var c1 = pivotGrid.AddFieldColumnArea<ResultItem>((c) => c.StepID);
            c1.Caption = "STEP_ID";
            c1.SortMode = PivotSortMode.Custom;

            var d1 = pivotGrid.AddFieldDataArea<ResultItem>((c) => c.Value);
            d1.Caption = "VALUE";

            pivotGrid.BestFit();
        }

        private void InitPivotGrid(IEnumerable<ResultItem> dt, PivotGridControl pivotGrid)
        {
            pivotGrid.CustomFieldSort += new PivotGridCustomFieldSortEventHandler(pivotGrid_CustomFieldSort);

            this.BindBegin(pivotGrid);
            this.BindDo(dt, pivotGrid);
            this.BindEnd(pivotGrid);

            pivotGrid.BestFit();
        }

        private void InitGrid(IEnumerable<UnPegHistory> dt, GridControl grid)
        {
            this.BindBegin(grid);
            this.BindDo(dt, grid);
            this.BindEnd(grid);
        }

        private void BindBegin(GridControl grid)
        {
            grid.BeginUpdate();
        }

        private void BindDo(IEnumerable<UnPegHistory> dt, GridControl grid)
        {
            grid.DataSource = dt.ToBindingList();
            this.SetFooterSummary();
        }

        private void BindEnd(GridControl grid)
        {
            grid.EndUpdate();
        }

        private void BindBegin(PivotGridControl pivotGrid)
        {
            pivotGrid.BeginUpdate();
        }

        private void BindDo(IEnumerable<ResultItem> dt, PivotGridControl pivotGrid)
        {
            pivotGrid.DataSource = dt.ToBindingList();
        }

        private void BindEnd(PivotGridControl pivotGrid)
        {
            pivotGrid.EndUpdate();
        }

        private void SetFooterSummary()
        {
            foreach (GridColumn col in this.gridView1.Columns)
            {
                string name = col.FieldName;

                if (name.Contains("UNPEG_QTY"))
                    this.gridView1.Columns[name].SummaryItem.SetSummary(DevExpress.Data.SummaryItemType.Sum, "{0:n0}");
                else if (name.Contains("MAIN_QTY"))
                    this.gridView1.Columns[name].SummaryItem.SetSummary(DevExpress.Data.SummaryItemType.Sum, "{0:n0}");
            }
        }

        private void pivotGrid_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.FieldName == "StepID")
            {
                if (e.ListSourceRowIndex1 < 0)
                    return;

                if (e.ListSourceRowIndex2 < 0)
                    return;

                object value1 = e.GetListSourceColumnValue(e.ListSourceRowIndex1, "StepID");
                object value2 = e.GetListSourceColumnValue(e.ListSourceRowIndex2, "StepID");

                decimal seq1 = this.GetStepSequence(value1.ToString());
                decimal seq2 = this.GetStepSequence(value2.ToString());

                e.Result = Comparer.Default.Compare(seq2, seq1);
                e.Handled = true;
            }

            if (e.Field.FieldName == "ValueName")
            {
                if (e.ListSourceRowIndex1 < 0)
                    return;

                if (e.ListSourceRowIndex2 < 0)
                    return;

                object value1 = e.GetListSourceColumnValue(e.ListSourceRowIndex1, "ValueName");
                object value2 = e.GetListSourceColumnValue(e.ListSourceRowIndex2, "ValueName");

                int seq1 = this.GetValueNameSequence(value1.ToString());
                int seq2 = this.GetValueNameSequence(value2.ToString());

                e.Result = Comparer.Default.Compare(seq1, seq2);
                e.Handled = true;
            }
        }

        private decimal GetStepSequence(string stepID)
        {
            decimal seq = 0;
            this._stdStepSequenceMappings.TryGetValue(stepID, out seq);

            return seq;
        }

        private int GetValueNameSequence(string valueName)
        {
            int seq = 0;
            this._UIvalueNameSequenceMappings.TryGetValue(valueName, out seq);

            return seq;
        }

        private string GetStepColumnKey(StdStep step)
        {
            return string.Format("{0} ({1})", step.STEP_ID, step.SEQUENCE);
        }

        class ResultItem
        {
            public string ProductID { get; set; }
            public string ProductName { get; set; }
            public string ProcessSeq { get; set; }
            public int BaseCompQty { get; set; }
            public decimal DemandQty { get; set; }
            public decimal ActQty { get; set; }
            public decimal TotalRemainQty { get; set; }
            public string ValueName { get; set; }
            public decimal Value { get; set; }
            public string StepID { get; set; }
        }

        private void productIDTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.productIDTextBox.Text.Count() == 0)
                return;

            this.productNameTextBox.Clear();
        }

        private void productNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.productNameTextBox.Text.Count() == 0)
                return;

            this.productIDTextBox.Clear();
        }

        private void pivotGridControl_CustomCellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.Value == null)
            {
                e.DisplayText = "0";
                return;
            }

            if ((decimal)e.Value == 0)
            {
                e.DisplayText = "0";
            }
        }

        private void pivotGridControl_CellClick(object sender, PivotCellEventArgs e)
        {
            this.infoGridControl.DataSource = null;

            PivotGridControl pivot = sender as PivotGridControl;
            var cellInfo = pivot.Cells.GetFocusedCellInfo();

            if (e.RowField == null)
                return;

            if (e.ColumnField == null)
                return;

            var rowFieldValue = cellInfo.GetFieldValue(e.RowField);
            if (rowFieldValue == null)
                return;

            var columnFieldValue = cellInfo.GetFieldValue(e.ColumnField);
            if (columnFieldValue == null)
                return;

            if (rowFieldValue.ToString() != "UnpeggedQty")
                return;
            
            var rowFields = pivot.GetFieldsByArea(PivotArea.RowArea);
            string prodID = cellInfo.GetFieldValue(rowFields[0]).ToString();
            RowInfo rowInfo = this.GetRowInfo(prodID);

            string lineID = rowInfo.ProductDetail.LineID;

            decimal seq = 0;
            this._stdStepSequenceMappings.TryGetValue(columnFieldValue.ToString(), out seq);
            StdStep stdStep = this.GetStdStep(lineID, seq);

            List<UnPegHistory> unpeggedList = null;
            var key = this.GetStdStepColumnInfoKey(rowInfo, stdStep) as Tuple<string, string, string, decimal>;
            this._stdStepUnpegHistoryMappings.TryGetValue(key, out unpeggedList);
            decimal unpeggedQty = (decimal)cellInfo.Value;

            this.InitGrid(unpeggedList, this.infoGridControl);
        }

        private string GetUnppegedListInfo(RowInfo rowInfo, StdStep stdStep, decimal unpeggedQty, List<UnPegHistory> unpeggedList)
        {
            if (unpeggedList == null)
                return string.Empty;

            StringBuilder prodIDSb = new StringBuilder();
            HashSet<string> prodIDSet = new HashSet<string>();
            int count = 0;
            foreach (var item in unpeggedList)
            {
                count++;
                if (prodIDSet.Contains(item.PRODUCT_ID))
                    continue;

                if (count == unpeggedList.Count)
                {
                    prodIDSb.Append(item.PRODUCT_ID);
                    break;
                }

                prodIDSb.Append(item.PRODUCT_ID);
                prodIDSb.Append(", ");

                prodIDSet.Add(item.PRODUCT_ID);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("- LINE_ID : {0}", rowInfo.ProductDetail.LineID));
            sb.AppendLine(String.Format("- PRODUCT_ID : {0}", rowInfo.ProductDetail.ProductID));
            sb.AppendLine(String.Format("- STEP_ID : {0}", stdStep.STEP_ID));
            sb.AppendLine(String.Format("- SEQUENCE : {0}", stdStep.SEQUENCE));
            sb.AppendLine(String.Format("- UNPEG_QTY : {0}", unpeggedQty));
            sb.AppendLine(String.Format("- DISTINCT UNPEGGED LOTS : {0}", unpeggedList.Count));
            sb.AppendLine(String.Format("- PRODUCT_ID : {0}", prodIDSb.ToString()));
            sb.AppendLine();

            HashSet<Tuple<string, string>> set = new HashSet<Tuple<string, string>>();
            foreach (var item in unpeggedList)
            {
                var key = Tuple.Create(item.LOT_ID, item.PRODUCT_ID);
                if (set.Contains(key))
                    continue;

                sb.AppendLine(String.Format("#{0}", count + 1));
                sb.AppendLine(String.Format("- LOT_ID : {0}", item.LOT_ID));
                sb.AppendLine(String.Format("- PRODUCT_ID : {0}", item.PRODUCT_ID));
                sb.AppendLine();

                set.Add(key);
            }

            return sb.ToString();
        }

        private RowInfo GetRowInfo(string productID)
        {
            RowInfo rowInfo = null;
            this._rowInfoMappings.TryGetValue(productID, out rowInfo);

            return rowInfo;
        }

        private StdStep GetStdStep(string lineID, decimal sequnece)
        {
            StdStep stdStep = null;
            var key = Tuple.Create(lineID, sequnece);
            this._stdStepMappings.TryGetValue(key, out stdStep);

            return stdStep;
        }

        private void pivotGridControl_CustomAppearance(object sender, PivotCustomAppearanceEventArgs e)
        {
            if (e.Focused)
            {
                e.Appearance.BackColor = Color.LightCyan;
            }

            if (e.Value == null)
                return;

            if ((decimal)e.Value < 0)
            {
                e.Appearance.ForeColor = Color.Red;
            }

            if (e.RowField == null)
                return;

            if (e.ColumnField == null)
                return;
           
            var rowFieldValue = e.GetFieldValue(e.RowField);
            if (rowFieldValue == null)
                return;

            var columnFieldValue = e.GetFieldValue(e.ColumnField);
            if (columnFieldValue == null)
                return;

            if (rowFieldValue.ToString() != "RemainQty")
                return;

            decimal seq = 0;
            this._stdStepSequenceMappings.TryGetValue(columnFieldValue.ToString(), out seq);

            if (seq != 1)
                return;

            e.Appearance.ForeColor = Color.Black;
            e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);

            if ((decimal)e.Value == 0)
                e.Appearance.BackColor = Color.LightGreen;

            if ((decimal)e.Value < 0)
                e.Appearance.BackColor = Color.LightPink;

            if ((decimal)e.Value > 0)
                e.Appearance.BackColor = Color.LightYellow;

            if (e.Focused)
            {
                e.Appearance.BackColor = Color.LightCyan;
            }
        }

        //private void pivotGridControl_MouseClick(object sender, MouseEventArgs e)
        //{
        //    this.pivotGridControl.Invalidate();
        //}

        //private void pivotGridControl_CustomDrawCell(object sender, PivotCustomDrawCellEventArgs e)
        //{
        //    var info = this.pivotGridControl.CalcHitInfo(this.pivotGridControl.PointToClient(Cursor.Position));

        //    if (info.CellInfo != null && info.CellInfo.RowIndex == e.RowIndex && info.CellInfo.ColumnIndex == e.ColumnIndex)
        //    {
        //        e.Appearance.ForeColor = CommonColors.GetWarningColor(UserLookAndFeel.Default);
        //        e.Appearance.BackColor = Color.LightCyan;
        //        e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
        //    }
        //}

        //private void pivotGridControl_FocusedCellChanged(object sender, EventArgs e)
        //{
        //    var info = this.pivotGridControl.CalcHitInfo(this.pivotGridControl.PointToClient(Cursor.Position));
        //    PivotCustomDrawCellEventArgs ev = e as PivotCustomDrawCellEventArgs;
        //    if (info.CellInfo != null && info.CellInfo.RowIndex == ev.RowIndex && info.CellInfo.ColumnIndex == ev.ColumnIndex)
        //    {
        //        e.Appearance.ForeColor = CommonColors.GetWarningColor(UserLookAndFeel.Default);
        //        ev.Appearance.BackColor = Color.LightCyan;
        //        e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
        //    }
        //}

        //private void pivotGridControl_CustomDrawFieldValue(object sender, PivotCustomDrawFieldValueEventArgs e)
        //{
        //    e.Appearance.ForeColor = CommonColors.GetWarningColor(UserLookAndFeel.Default);
        //    e.Appearance.BackColor = Color.LightBlue;
        //    e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
        //}

    }
}
