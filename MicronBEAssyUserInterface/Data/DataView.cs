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
using Mozart.Data.Entity;
using System.Reflection;
using MicronBEAssy.Outputs;
using DevExpress.XtraPivotGrid;
using System.Collections;
using MicronBEAssy.Inputs;
using DevExpress.XtraGrid;
using Mozart.Mapping;
using Mozart.Task.Model;
using Mozart.DataActions;
using Mozart.Studio.Projects;
using DevExpress.XtraLayout;
using DevExpress.XtraSpreadsheet;
using DevExpress.Spreadsheet;
using DevExpress.XtraBars.Docking;

namespace MicronBEAssyUserInterface.Data
{
    public partial class DataView : XtraGridControlView
    {
        #region Variables

        IExperimentResultItem _result;
        ResultDataContext _resultCtx;
        MainView _mainView;

        #endregion

        #region Constructors

        public DataView()
            : base()
        {
            InitializeComponent();
        }

        public DataView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                this._result = this.Document.GetResultItem();
                if (_result == null)
                    return;
            }

            InitializeControl();
        }

        private void InitializeControl()
        {
            this._resultCtx = this._result.GetCtx<ResultDataContext>();

            InitializeComboBox(this._resultCtx);

            _mainView = new MainView();
            _mainView.Dock = DockStyle.Fill;
            this.panelMain.Controls.Add(_mainView);
        }

        private void InitializeComboBox(ResultDataContext resultDataContext)
        {
            SetComboBoxData(resultDataContext.ModelContext.Target.Inputs.ItemArray, this.comboBoxInputs);

            SetComboBoxData(resultDataContext.ModelContext.Target.Outputs.ItemArray, this.comboBoxOutPuts);
        }

        private void SetComboBoxData(DataItem[] dataItem, ComboBox comboBox)
        {
            foreach (DataItem item in dataItem)
            {
                comboBox.Items.Add(item.Name);
            }

            comboBox.SelectedIndex = 0;
        }
        #endregion

        private void SetMainView(DataTable dt, string item)
        {
            DockPanel newPanel = _mainView.DockManager.AddPanel(DockingStyle.Float);
            newPanel.Text = item;
            newPanel.DockedAsTabbedDocument = true;

            if (this.radioButtonPivot.Checked)
            {
                PivotGridControl newPivot = new PivotGridControl();
                newPivot.Dock = DockStyle.Fill;
                newPanel.Controls.Add(newPivot);

                SetPivotGridData(dt, newPivot);
            }
            else if (this.radioButtonSpreadSheet.Checked)
            {
                SpreadsheetControl spreadSheet = new SpreadsheetControl();
                spreadSheet.Dock = DockStyle.Fill;
                newPanel.Controls.Add(spreadSheet);

                SetSpreadSheetData(dt, spreadSheet, item);
            }
        }

        private void SetSpreadSheetData(DataTable dt, SpreadsheetControl spreadSheet, string item)
        {
            spreadSheet.Document.BeginUpdate();

            Worksheet sheet = spreadSheet.Document.Worksheets[0];
            sheet.Name = item;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                Cell cell = sheet.Rows[0][i];
                cell.Font.Bold = true;
                cell.Font.Size = 9;
                cell.Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                cell.FillColor = Color.Pink;
                cell.SetValue(dt.Columns[i].ToString());
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    Cell cell = sheet.Rows[i + 1][j];
                    cell.Font.Size = 9;
                    cell.Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
                    cell.SetValue(dt.Rows[i][j]);
                }
            }

            sheet.GetUsedRange().AutoFitColumns();
            sheet.GetUsedRange().AutoFitRows();

            spreadSheet.Document.EndUpdate();
        }

        private void SetPivotGridData(DataTable dt, PivotGridControl pivotGrid)
        {
            XtraPivotGridHelper.DataViewTable dataTable = new XtraPivotGridHelper.DataViewTable();

            foreach (DataColumn info in dt.Columns)
                dataTable.AddColumn(info.ColumnName, info.ColumnName, info.DataType, PivotArea.RowArea, null, null);

            pivotGrid.BeginUpdate();
            pivotGrid.ClearPivotGridFields();
            pivotGrid.CreatePivotGridFields(dataTable);
            pivotGrid.DataSource = dt;
            pivotGrid.EndUpdate();
            if(dt.Rows.Count < 10000)
                pivotGrid.BestFit();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {

        }

        private void buttonView_Click(object sender, EventArgs e)
        {
            TabPage page = this.tabControl1.SelectedTab;

            string item = string.Empty;
            DataTable dt = null;
            if (page == this.tabPageInputs)
            {
                item = this.comboBoxInputs.SelectedItem.ToString();
                dt = this._result.LoadInput(item);
            }
            else if (page == this.tabPageOutputs)
            {
                item = this.comboBoxOutPuts.SelectedItem.ToString();
                dt = this._result.LoadOutput(item);
            }

            SetMainView(dt, item);
        }
    }
}
