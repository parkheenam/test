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
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraGrid;
using Mozart.Studio.TaskModel.Projects;
using MicronBEAssy;
using MicronBEAssy.Outputs;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using MicronBEAssyUserInterface.Class;

namespace MicronBEAssyUserInterface.CycleTime
{
    public partial class CycleTimeAnalysisView : XtraGridControlView
    {
        IExperimentResultItem _result;
        ResultDataContext _resultDataContext = null;
        Dictionary<string, Dictionary<IComparable, CycleTimeRawData>> _gridDatas = null;
        Dictionary<string, Dictionary<IComparable, CycleTimeRawData>> _treeDatas = null;
        HashSet<string> _designIDList = new HashSet<string>();
        int panel1Width = 0;
        int panel2Width = 0;
        int minPanel2Width = 0;
        GridView panel2Gridview;
        TreeList panel1Tree;

        public CycleTimeAnalysisView()
            : base()
        {
            InitializeComponent();
        }

        public CycleTimeAnalysisView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();            
        }

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                _result = this.Document.GetResultItem();
                _resultDataContext = this.Document.GetResultItem().GetCtx<ResultDataContext>();
            }

            SetData();

            InitializeControl();
        }

        private void InitializeControl()
        {
            SetRadioGroupSort();
            SetCheckedComboBox();
        }

        private void SetCheckedComboBox()
        {
            foreach (string designID in _designIDList)
            {
                this.checkedComboBoxDesignID.Items.Add(designID, true);
            }

            this.checkedComboBoxDesignID.Select();
        }

        private void SetRadioGroupSort()
        {
            if (this.radioGroupQueryOption.SelectedIndex == 0)
                this.radioGroupSort.Enabled = false;
            else
                this.radioGroupSort.Enabled = true;
        }

        private void buttonQuery_Click(object sender, EventArgs e)
        {
            this.panel1.Controls.Clear();
            this.panel2.Controls.Clear();
            this.panel3.Controls.Clear();

            TreeList treeList = new TreeList();
            treeList.Dock = DockStyle.Fill;

            treeList.BeforeFocusNode += new BeforeFocusNodeEventHandler(treeList_BeforeFocusNode);

            if (this.radioGroupQueryOption.SelectedIndex == 0)
            {
                List<string> columnNames = new List<string> { "ITEMS", "Flow Factor" };

                for (int i = 0; i < columnNames.Count; i++)
                {
                    string colName = columnNames.ElementAt(i);

                    TreeListColumn column = treeList.Columns.AddField(colName);
                    column.Caption = colName;
                    column.VisibleIndex = i;
                }

                this.panel1.Controls.Add(treeList);

                BindTreeData(treeList, 0);
            }
            else
            {
                List<string> columnNames = new List<string> { "ITEMS", "Flow Factor", "TOTAL_RUN(DIFF)", "TOTAL_WAIT(DIFF)" };

                for (int i = 0; i < columnNames.Count; i++)
                {
                    string colName = columnNames.ElementAt(i);

                    TreeListColumn column = treeList.Columns.AddField(colName);
                    column.Caption = colName;
                    column.VisibleIndex = i;
                }

                this.panel1.Controls.Add(treeList);

                BindTreeData(treeList, 1);
            }
        }

        void treeList_BeforeFocusNode(object sender, BeforeFocusNodeEventArgs e)
        {
            this.panel2.Controls.Clear();
            this.panel3.Controls.Clear();

            if (e.Node.Level == 2)
            {
                GridControl grid = new GridControl();
                grid.Dock = DockStyle.Fill;

                this.panel2.Controls.Add(grid);

                string prodID = e.Node.GetValue("ITEMS").ToString();

                DataTable dt = null;

                if (this.radioGroupQueryOption.SelectedIndex == 0)
                {
                    dt = BindGridData(prodID, grid, 0);
                    DrawChart(dt, 0);
                }
                else
                {
                    dt = BindGridData(prodID, grid, 1);
                    DrawChart(dt, 1);
                }
            }
        }

        private void DrawChart(DataTable dt, int option)
        {
            if (dt == null)
                return;

            ChartControl chart = new ChartControl();
            chart.Dock = DockStyle.Fill;
            this.panel3.Controls.Add(chart);

            Series runSeries = null;
            Series waitSeries = null;

            if (option == 0)
            {
                runSeries = new Series("RUN", ViewType.Line);
                waitSeries = new Series("WAIT", ViewType.Line);
            }
            else
            {
                runSeries = new Series("RUN(DIFF)", ViewType.Line);
                waitSeries = new Series("WAIT(DIFF)", ViewType.Line);
            }

            (runSeries.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            (waitSeries.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;

            chart.Series.Add(runSeries);
            chart.Series.Add(waitSeries);

            foreach (DataRow row in dt.Rows)
            {
                if (option == 0)
                {
                    SeriesPoint runPoint = new SeriesPoint(row["STEP_ID"].ToString(), (decimal)row["RUN"]);
                    SeriesPoint waitPoint = new SeriesPoint(row["STEP_ID"].ToString(), (decimal)row["WAIT"]);

                    runSeries.Points.Add(runPoint);
                    waitSeries.Points.Add(waitPoint);
                }
                else
                {
                    SeriesPoint runPoint = new SeriesPoint(row["STEP_ID"].ToString(), (decimal)row["RUN(DIFF)"]);
                    SeriesPoint waitPoint = new SeriesPoint(row["STEP_ID"].ToString(), (decimal)row["WAIT(DIFF)"]);

                    runSeries.Points.Add(runPoint);
                    waitSeries.Points.Add(waitPoint);
                }
            }

            (chart.Diagram as XYDiagram).EnableAxisXZooming = true;
            (chart.Diagram as XYDiagram).EnableAxisXScrolling = true;
        }

        private DataTable BindGridData(string prodID, GridControl grid, int option)
        {
            Dictionary<IComparable, CycleTimeRawData> info;
            if (_gridDatas.TryGetValue(prodID, out info))
            {
                DataTable dt = new DataTable();
                DataColumn colProductID = new DataColumn("PRODUCT_ID", typeof(string));
                DataColumn colStepID = new DataColumn("STEP_ID", typeof(string));
                DataColumn colStepSeq = new DataColumn("STEP_SEQ", typeof(decimal));
                DataColumn colRun = new DataColumn("RUN", typeof(decimal));
                DataColumn colWait = new DataColumn("WAIT", typeof(decimal));
                DataColumn colRunQty = new DataColumn("RUN_QTY", typeof(decimal));
                DataColumn colWaitQty = new DataColumn("WAIT_QTY", typeof(decimal));

                dt.Columns.Add(colProductID);
                dt.Columns.Add(colStepID);
                dt.Columns.Add(colStepSeq);
                dt.Columns.Add(colRun);
                dt.Columns.Add(colWait);
                dt.Columns.Add(colRunQty);
                dt.Columns.Add(colWaitQty);

                DataColumn colRunDiff = null;
                DataColumn colWaitDiff = null;
                if (option == 1)
                {
                    colRunDiff = new DataColumn("RUN(DIFF)", typeof(decimal));
                    colWaitDiff = new DataColumn("WAIT(DIFF)", typeof(decimal));
                    dt.Columns.Add(colRunDiff);
                    dt.Columns.Add(colWaitDiff);
                }

                List<CycleTimeRawData> list = new List<CycleTimeRawData>(info.Values);

                list.Sort((x, y) => x.StepSeq.CompareTo(y.StepSeq));

                foreach (CycleTimeRawData data in list)
                {
                    DataRow row = dt.NewRow();
                    row[colProductID] = data.ProductID;
                    row[colStepID] = data.StepID;
                    row[colStepSeq] = data.StepSeq;

                    double run = data.RunLotCount == 0 ? 0 : Math.Round((data.Run / data.RunLotCount) / 60, 2);
                    double wait = data.WaitLotCount == 0 ? 0 : Math.Round((data.Wait / data.WaitLotCount) / 60, 2);

                    row[colRun] = run;
                    row[colWait] = wait;
                    row[colRunQty] = data.RunLotCount;
                    row[colWaitQty] = data.WaitLotCount;

                    if (option == 1)
                    {
                        row[colRunDiff] = data.RunDiff;
                        row[colWaitDiff] = data.WaitDiff;
                    }

                    dt.Rows.Add(row);
                }

                grid.DataSource = dt;

                GridView view = (GridView)grid.MainView;
                view.OptionsView.ShowGroupPanel = false;
                view.BestFitColumns();
                panel2Gridview = view;
                panel2Width = this.panel2.Width;
                panel2Gridview.OptionsView.ColumnAutoWidth = false;

                minPanel2Width = 0;
                for (int count = 0; count < panel2Gridview.Columns.Count(); count++)
                    minPanel2Width += panel2Gridview.Columns[count].Width;

                panel2Gridview.OptionsView.ColumnAutoWidth = true;

                return dt;
            }

            return null;
        }

        private void BindTreeData(TreeList treeList, int option)
        {
            int width = 0;
            var splitLoc = this.splitterItem1.Location;
            splitLoc.X = 127;
            this.splitterItem1.Location = splitLoc;

            HashSet<string> checkedDesignIDs = new HashSet<string>();
            foreach (var designID in this.checkedComboBoxDesignID.CheckedItems)
                checkedDesignIDs.Add(designID.ToString());

            treeList.BeginUnboundLoad();

            TreeListNode parentForRootNodes = treeList.AppendNode(new object[] { "ROOT", string.Empty }, null);

            foreach (KeyValuePair<string, Dictionary<IComparable, CycleTimeRawData>> data in _treeDatas)
            {
                if (checkedDesignIDs.Contains(data.Key) == false)
                    continue;

                double maxFlowFactor = data.Value.Values.Max(x => x.FlowFactor);

                if (option == 0)
                {
                    TreeListNode designNode = treeList.AppendNode(new object[] { data.Key, maxFlowFactor }, parentForRootNodes);

                    foreach (CycleTimeRawData info in data.Value.Values)
                    {
                        TreeListNode prodNode = treeList.AppendNode(new object[] { info.FinalProductID, info.FlowFactor }, designNode);
                    }
                }
                else
                {
                    TreeListNode designNode = treeList.AppendNode(new object[] { data.Key, maxFlowFactor, 0, 0 }, parentForRootNodes);

                    foreach (CycleTimeRawData info in data.Value.Values)
                    {
                        TreeListNode prodNode = treeList.AppendNode(new object[] { info.FinalProductID, info.FlowFactor, 0, 0 }, designNode);
                    }
                }
            }

            treeList.EndUnboundLoad();

            treeList.BestFitColumns();
            treeList.ExpandAll();

            string facCol = string.Empty;
            string runCol = string.Empty;
            string waitCol = string.Empty;

            foreach(TreeListColumn col in treeList.Columns)
            {
                string colName = col.Caption;
                if (colName.Contains("Factor"))
                    facCol = colName;
                else if (colName.Contains("RUN"))
                    runCol = colName;
                else if (colName.Contains("WAIT"))
                    waitCol = colName; 

            }
            if (radioGroupQueryOption.SelectedIndex == 0)
                treeList.Columns[facCol].SortOrder = SortOrder.Descending;
            else
                if (radioGroupSort.SelectedIndex == 0)
                    treeList.Columns[runCol].SortOrder = SortOrder.Descending;
                else
                    treeList.Columns[waitCol].SortOrder = SortOrder.Descending;

            foreach (TreeListColumn col in treeList.Columns)
            {
                col.BestFit();
                width += col.Width;
            }
            
            splitLoc.X = 50 + width;
            this.splitterItem1.Location = splitLoc;
            panel1Width = this.panel1.Width;
            panel1Tree = treeList;
        }

        private void SetData()
        {
            this._gridDatas = new Dictionary<string,Dictionary<IComparable,CycleTimeRawData>>();
            this._treeDatas = new Dictionary<string, Dictionary<IComparable, CycleTimeRawData>>();
            foreach (EqpPlan info in _resultDataContext.EqpPlan)
            {
                if (info.STATUS == "SETUP")
                    continue;

                if (info.DESIGN_ID == null)
                    continue;

#if DEBUG
                if(info.PRODUCT_ID == "ASSY_A01")
                    Console.WriteLine();
#endif

                //Base 기준으로만 집계
                bool isBase = false;
                if (info.FINAL_PROD_ID == info.PRODUCT_ID || info.COMP_SEQ == 1)
                    isBase = true;

                if (isBase == false)
                    continue;

                string finalProdID = info.FINAL_PROD_ID;

                _designIDList.Add(info.DESIGN_ID);

                Tuple<string, string, string, decimal> gridKey = new Tuple<string, string, string, decimal>(info.DESIGN_ID, info.PRODUCT_ID, info.STEP_ID, info.SEQUENCE);
                Dictionary<IComparable, CycleTimeRawData> gridData;
                if (_gridDatas.TryGetValue(finalProdID, out gridData) == false)
                    _gridDatas.Add(finalProdID, gridData = new Dictionary<IComparable, CycleTimeRawData>());

                CycleTimeRawData rawData;
                if(gridData.TryGetValue(gridKey, out rawData) == false)
                {
                    rawData = new CycleTimeRawData(info.DESIGN_ID, finalProdID, info.PRODUCT_ID, info.STEP_ID, info.SEQUENCE);
                    gridData.Add(gridKey, rawData);
                }

                DateTime engineEndTime = UIHelper.GetEngineEndTime(this._result);    
                DateTime arrivalTime = info.ARRIVAL_TIME;
                DateTime startTime = info.START_TIME;
                DateTime endTime = info.END_TIME;

                double run = 0;
                double wait = 0;

                DateTime time = startTime == DateTime.MinValue ? engineEndTime : startTime;
                wait = time.Subtract(arrivalTime).TotalSeconds;

                if (info.STATUS == "BUSY")
                {
                    run = endTime.Subtract(startTime).TotalSeconds;
                    rawData.Run += run;
                    rawData.RunQty += Convert.ToDouble(info.QTY);
                    rawData.RunLotCount++;
                }

                rawData.Wait += wait;
                rawData.WaitQty += Convert.ToDouble(info.QTY);
                rawData.WaitLotCount++;

                Dictionary<IComparable, CycleTimeRawData> treeData;
                if (_treeDatas.TryGetValue(info.DESIGN_ID, out treeData) == false)
                    _treeDatas.Add(info.DESIGN_ID, treeData = new Dictionary<IComparable, CycleTimeRawData>());

                Tuple<string, string> treeKey = new Tuple<string, string>(info.DESIGN_ID, finalProdID);
                CycleTimeRawData data;
                if (treeData.TryGetValue(treeKey, out data) == false)
                    treeData.Add(treeKey, data = new CycleTimeRawData(info.DESIGN_ID, finalProdID, info.PRODUCT_ID, string.Empty, 0));

                data.Run += run;
                data.Wait += wait;
            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            if (panel1Tree != null)
            {
                if (this.panel1.Width < panel1Width)
                    panel1Tree.OptionsView.AutoWidth = false;
                else
                    panel1Tree.OptionsView.AutoWidth = true;
            }
        }

        private void panel2_SizeChanged(object sender, EventArgs e)
        {
            if (panel2Gridview != null)
            {
                if (this.panel2.Width < panel2Width && this.panel2.Width < minPanel2Width)                  
                    panel2Gridview.OptionsView.ColumnAutoWidth = false;

                else
                    panel2Gridview.OptionsView.ColumnAutoWidth = true;
            }
        }

        public static DateTime GetResultStartTime(IExperimentResultItem _result)
        {
            string description = _result.Description;

            string[] lines = description.Split('\n');

            try
            {
                foreach (string line in lines)
                {
                    if (line.StartsWith("start-time"))
                    {
                        string periodLine = line;

                        string value = periodLine.Trim().Split('=')[1];

                        return Convert.ToDateTime(value);
                    }
                }

                return _result.StartTime;
            }
            catch
            {
                return _result.StartTime;
            }
        }

        private void radioGroupQueryOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRadioGroupSort();
        }
    }
}
