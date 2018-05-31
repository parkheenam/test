using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Data.Entity;
using Mozart.Studio.TaskModel.UserLibrary;
using DevExpress.XtraPivotGrid;

namespace MicronBEAssyUserInterface.Analysis
{
    public partial class StepTargetView : XtraPivotGridControlView
    {
        #region Class Variables
        private IExperimentResultItem _result;
        private Dictionary<string, ResultItem> _dict;
        #endregion

        public StepTargetView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        #region LoadDocument : 진입점
        protected override void LoadDocument()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            LoadData();

            BindData();
        }

        private void BindData()
        {
            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();

            FillData(dt);

            DrawGrid(dt);
        }

        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            foreach (ResultItem item in _dict.Values)
            {
                dt.DataTable.Rows.Add(
                    item.LINE_ID,
                    item.PRODUCT_ID,
                    item.PROCESS_ID,
                    item.STEP_ID,
                    item.IN_QTY,
                    item.OUT_QTY
                );
            }
        }

        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;

            this.pivotGridControl1.EndUpdate();
        }

        private void LoadData()
        {
            _dict = new Dictionary<string, ResultItem>();

            EntityTable<MicronBEAssy.Outputs.StepTarget> st = _result.LoadOutput<MicronBEAssy.Outputs.StepTarget>("StepTarget").ToEntityTable();

            string product = this.prodTextBox.Text.ToUpper();

            var filteredTable = st.Rows.Where(o => o.PRODUCT_ID.Contains(product));

            // Process Data 부분
            foreach (MicronBEAssy.Outputs.StepTarget item in filteredTable)
            {
                ResultItem ri;

                // _dict에 LINE_ID + PROD_ID + PROC_ID + STEP_ID 키로 조회되는 항목이 있으면
                // 값을 합산, 없으면 새로 등록
                string key = item.LINE_ID + item.PRODUCT_ID + item.PROCESS_ID + item.STEP_ID;

                if (_dict.TryGetValue(key, out ri) == false)
                {
                    ri = new ResultItem();

                    ri.LINE_ID = item.LINE_ID;
                    ri.PRODUCT_ID = item.PRODUCT_ID;
                    ri.PROCESS_ID = item.PROCESS_ID;
                    ri.STEP_ID = item.STEP_ID;

                    _dict.Add(key, ri);
                }
                ri.IN_QTY += Convert.ToDouble(item.IN_QTY);
                ri.OUT_QTY += Convert.ToDouble(item.OUT_QTY);
            }
        }

        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn("LINE_ID", "LINE_ID", typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn("PRODUCT_ID", "PRODUCT_ID", typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn("PROCESS_ID", "PROCESS_ID", typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn("STEP_ID", "STEP_ID", typeof(string), PivotArea.RowArea, null, null);

            dt.AddColumn("IN_QTY", "IN_QTY", typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn("OUT_QTY", "OUT_QTY", typeof(float), PivotArea.DataArea, null, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns["LINE_ID"],
                        dt.Columns["PRODUCT_ID"],
                        dt.Columns["PROCESS_ID"],
                        dt.Columns["STEP_ID"]
                    }
                );

            return dt;
        }

        #region Internal Class : ResultItem
        internal class ResultItem
        {
            public string LINE_ID;
            public string PRODUCT_ID;
            public string PROCESS_ID;
            public string STEP_ID;
            public double IN_QTY;
            public double OUT_QTY;

            public ResultItem()
            {

            }
        }
        #endregion

    }
}
