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
using MicronBEAssy;
using Mozart.DataActions;
using Mozart.Studio.TaskModel.Projects;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraCharts;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraEditors.Repository;

namespace MicronBEAssyUserInterface.Data
{
    public partial class AnalysisView : XtraGridControlView
    {
        IExperimentResultItem _result;
        ResultDataContext _resultDataContext = null;
        MainView _mainView = null;

        public AnalysisView()
            : base()
        {
            InitializeComponent();
        }

        public AnalysisView(IServiceProvider serviceProvider)
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

            InitializeControl();
        }

        private void InitializeControl()
        {
            SetMainView();

            SetComboxInput();
        }

        private void SetMainView()
        {
            _mainView = new MainView();
            _mainView.Dock = DockStyle.Fill;
            this.panelMain.Controls.Add(_mainView);
        }

        private void SetComboxInput()
        {
            List<DataItem> items = new List<DataItem>(_resultDataContext.ModelContext.Target.Inputs.ItemArray);
            items.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (DataItem item in items)
            {
                this.comboBoxInputs.Items.Add(item.Name);
            }

            this.comboBoxInputs.SelectedIndex = 0;
        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            string itemName = this.comboBoxInputs.SelectedItem.ToString();
            DataTable dt = this._result.LoadInput(itemName);

            DockPanel dockPanel = this._mainView.DockManager.AddPanel(DockingStyle.Float);
            dockPanel.DockedAsTabbedDocument = true;
            dockPanel.Text = itemName;

            Panel newPanel = new Panel();
            newPanel.Dock = DockStyle.Fill;
            dockPanel.Controls.Add(newPanel);

            MainView subView = new MainView();
            subView.Dock = DockStyle.Fill;
            newPanel.Controls.Add(subView);

            CreateGridView(subView, itemName, dt);

            CreatePivotGridView(subView, itemName, dt);

            //CreateChartView(subView, itemName, dt);
        }

        private void CreatePivotGridView(MainView subView, string itemName, DataTable dt)
        {
            DockPanel dockPanel = subView.DockManager.AddPanel(DockingStyle.Float);
            dockPanel.DockedAsTabbedDocument = true;
            dockPanel.Text = "Pivot";
            dockPanel.Options.ShowCloseButton = false;

            PivotGridControl pivot = new PivotGridControl();

            XtraPivotGridHelper.DataViewTable pivotData = new XtraPivotGridHelper.DataViewTable();

            foreach (DataColumn dc in dt.Columns)
                pivotData.AddColumn(dc.ColumnName, dc.ColumnName, dc.DataType, PivotArea.RowArea, null, null);

            pivot.Dock = DockStyle.Fill;
            pivot.ClearPivotGridFields();
            pivot.CreatePivotGridFields(pivotData);
            pivot.DataSource = dt;
            //pivot.BestFit();
            dockPanel.Controls.Add(pivot);

        }

        private void CreateChartView(MainView subView, string itemName, DataTable dt)
        {
            DockPanel dockPanel = subView.DockManager.AddPanel(DockingStyle.Float);
            dockPanel.DockedAsTabbedDocument = true;
            dockPanel.Text = "Chart";
            dockPanel.Options.ShowCloseButton = false;

            ChartControl chart = new ChartControl();
            chart.Dock = DockStyle.Fill;
            dockPanel.Controls.Add(chart);

            Dictionary<string, double> qtyInfos = new Dictionary<string, double>();
            foreach (DataRow row in dt.Rows)
            {
                string productID = row["PRODUCT_ID"].ToString();
                double qty = double.Parse(row["ACT_QTY"].ToString());

                if (qtyInfos.ContainsKey(productID) == false)
                    qtyInfos.Add(productID, 0);

                qtyInfos[productID] += qty;
            }

            int topCount = 10;

            Series topseries = new Series(string.Format("Top{0}", topCount), ViewType.Bar);
            Series etcSeries = new Series("Etc", ViewType.Bar);
            chart.Series.Add(topseries);
            chart.Series.Add(etcSeries);
            List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>(qtyInfos);
            list.Sort((x, y) => y.Value.CompareTo(x.Value));

            int i = 0;
            foreach (KeyValuePair<string, double> info in list)
            {
                i++;
                string key = string.IsNullOrEmpty(info.Key) ? "-" : info.Key;

                SeriesPoint point = new SeriesPoint(key, info.Value);

                if (i > topCount)
                {
                    etcSeries.Points.Add(point);
                }
                else
                    topseries.Points.Add(point);
            }

            (chart.Diagram as XYDiagram).EnableAxisXZooming = true;
            //(chart.Diagram as XYDiagram).EnableAxisYZooming = true;
            (chart.Diagram as XYDiagram).EnableAxisXScrolling = true;
            //(chart.Diagram as XYDiagram).EnableAxisYScrolling = true;
            ChartTitle title = new ChartTitle();
            title.Text = itemName;
            chart.Titles.Add(title);
        }

        private void CreateGridView(MainView subView, string itemName, DataTable dt)
        {
            DockPanel dockPanel = subView.DockManager.AddPanel(DockingStyle.Float);
            dockPanel.DockedAsTabbedDocument = true;
            dockPanel.Text = "Grid";
            dockPanel.Options.ShowCloseButton = false;

            GridControl grid = new GridControl();
            GridView view = new GridView(grid);
            view.OptionsView.ShowAutoFilterRow = true;
            grid.MainView = view;
            grid.DataSource = dt;
            grid.Dock = DockStyle.Fill;
            dockPanel.Controls.Add(grid);
            view.BestFitColumns();
            view.OptionsView.RowAutoHeight = true;
            view.OptionsView.ColumnAutoWidth = true;

            view.RowStyle += view_RowStyle;
        }

        private void view_RowStyle(object sender, RowStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                if (e.RowHandle % 2 == 0)
                {
                    e.Appearance.BackColor = Color.White;
                    e.Appearance.BackColor2 = Color.LightYellow;
                }
            }
        }
    }
}
