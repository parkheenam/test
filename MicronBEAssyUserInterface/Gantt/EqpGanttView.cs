using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.Spreadsheet;

using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using DevExpress.Skins;
using DevExpress.LookAndFeel;

namespace MicronBEAssyUserInterface.Gantt
{
    public partial class EqpGanttView : XtraGridControlView
    {
        #region Variables

        IExperimentResultItem result;

        GanttMaster gantt;

        DateTime planStartTime;

        GanttBar prebar;
        float prebarwidth;
        int index = 0;

        #region Draw Cell

        List<DateTime> drawnDates = new List<DateTime>();
        List<string> drawnWeeks = new List<string>();

        Bar currentBar;

        bool isFirst;
        string prevLineID;
        string prevEqpID;
        string prevStepGroup;

        int typeStartRowIndex = 0;
        int eqpIDStartRowIndex = 0;
        int lineIDStartRowIndex = 0;
        int stepGroupStartRowIndex = 0;

        double subTotalTIQty = 0;
        double totalTIQty = 0;
        double rowTIQty = 0;
        double rowTOQty = 0;

        Color preColor = XtraSheetHelper.AltColor;
        Color currColor = XtraSheetHelper.AltColor2;

        #endregion

        #endregion

        #region Properties

        #region Query Options
        public string SelType
        {
            get { return selectionTypeComboBox.Text; }
        }

        public string LineID
        {
            get { return lineIDComboBox.Text; }
        }
       
        public string EqpID
        {
            get
            {
                var value = equipmentIDTextBox.Text;
                return value.Length > 0 ? string.Format("%{0}%", value) : string.Empty;
            }
        }

        public string StepGroup
        {
            get { return stepGroupComboBox.Text; }
        }

        public DateTime StartDate
        {
            //get { return ShopCalendar.StartTimeOfDayT(startDateEdit.DateTime); }
            get { return planStartTime; }
        }

        public DateTime EndDate
        {
            get { return StartDate.AddHours(result.GetPlanPeriod() * 24); }
        }

        public int Duration
        {
            get { return (int)durationNumericUpDown.Value; }
        }

        public int MergeThreshold
        {
            get { return (int)this.mergeThresholdnumericUpDown.Value; }
        }

        #endregion

        #region View Options

        public int CellWidthSize
        {
            get { return ganttSizeControl.CellWidth; }
        }

        public int CellHeightSize
        {
            get { return ganttSizeControl.CellHeight; }
        }

        #endregion

        #endregion

        #region Constructors

        public EqpGanttView()
            : base()
        {
            InitializeComponent();
        }

        public EqpGanttView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        #region Initialize

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                this.result = this.Document.GetResultItem();
                if (this.result == null)
                    return;
            }

            this.InitializeUserControl();
            this.InitializeData();
        }

        protected void InitializeUserControl()
        {
            #region Query Option Control

            base.SetWaitDialogLoadCaption("Initializing query options.");

            this.planStartTime = this.result.StartTime;
            this.startDateEdit.DateTime = this.planStartTime.Date;

            var cellHeight = this.ServiceProvider.GetLocalSetting(Constants.GANTT_CELL_HEIGHT);
            var cellWidth = this.ServiceProvider.GetLocalSetting(Constants.GANTT_CELL_WIDTH);
            var stageIdx = this.ServiceProvider.GetLocalSetting(Constants.GANTT_STAGE_IDX);
            var siteIdx = this.ServiceProvider.GetLocalSetting(Constants.GANTT_SITE_IDX);
            var operIdx = this.ServiceProvider.GetLocalSetting(Constants.GANTT_OPER_IDX);
            var duration = this.ServiceProvider.GetLocalSetting(Constants.GANTT_DURATION);

            this.lineIDComboBox.SelectedIndexChanged -= this.stageIDComboBox_SelectedIndexChanged;
            this.stepGroupComboBox.SelectedIndexChanged -= this.operationIDComboBox_SelectedIndexChanged;
            this.durationNumericUpDown.ValueChanged -= durationNumericUpDown_ValueChanged;
            this.ganttSizeControl.CellHeightChanged -= ganttSizeControl_CellHeightChanged;
            this.ganttSizeControl.CellWidthChanged -= ganttSizeControl_CellWidthChanged;

            this.lineIDComboBox.Properties.Items.Clear();
            this.stepGroupComboBox.Properties.Items.Clear();

            this.result.FillValues(lineIDComboBox, Constants.TABLE_EQP_PLAN, Constants.COLUMN_LINE_ID, false, false);
            this.result.FillValues(stepGroupComboBox, Constants.TABLE_EQUIPMENT, Constants.COLUMN_STEP_GROUP, true, true);

            lineIDComboBox.SelectedIndex = string.IsNullOrEmpty(stageIdx) ? 0 : Convert.ToInt32(stageIdx);
            stepGroupComboBox.SelectedIndex = string.IsNullOrEmpty(operIdx) ? 0 : Convert.ToInt32(operIdx);

            if (!string.IsNullOrEmpty(duration))
            {
                int d;
                if (int.TryParse(duration, out d))
                    this.durationNumericUpDown.Value = d;
                else
                    this.durationNumericUpDown.Value = this.result.GetPlanPeriod(Constants.DEFAULT_PERIOD);
            }

            #endregion

            #region View Option Control

            base.SetWaitDialogLoadCaption("Initializing view options.");


            if (!string.IsNullOrEmpty(cellHeight))
            {
                int height;
                if (int.TryParse(cellHeight, out height))
                    this.ganttSizeControl.CellHeight = height;
                else
                    this.ganttSizeControl.CellHeight = Constants.DEFAULT_HEIGHT;
            }

            if (!string.IsNullOrEmpty(cellWidth))
            {
                int width;
                if (int.TryParse(cellWidth, out width))
                    this.ganttSizeControl.CellWidth = width;
                else
                    this.ganttSizeControl.CellWidth = Constants.DEFAULT_WIDTH;
            }

            foreach (var selectionType in Enum.GetValues(typeof(MouseSelectionType)))
                this.selectionTypeComboBox.Items.Add(selectionType);
            this.selectionTypeComboBox.SelectedIndex = 0;

            this.lineIDComboBox.SelectedIndexChanged += this.stageIDComboBox_SelectedIndexChanged;
            this.stepGroupComboBox.SelectedIndexChanged += this.operationIDComboBox_SelectedIndexChanged;
            this.durationNumericUpDown.ValueChanged += durationNumericUpDown_ValueChanged;
            this.ganttSizeControl.CellHeightChanged += ganttSizeControl_CellHeightChanged;
            this.ganttSizeControl.CellWidthChanged += ganttSizeControl_CellWidthChanged;

            #endregion
        }

        private void InitializeData()
        {
            base.SetWaitDialogLoadCaption("Initializing gantt infomations.");

            this.gantt = new GanttMaster(this.spreadSheet1, this.result);
            this.gantt.DefaultColumnWidth = this.CellWidthSize;
            this.gantt.DefaultRowHeight = this.CellHeightSize;
            this.gantt.CellUnitMinutes = 60;

            this.BindEvents();
        }

        private void BindEvents()
        {
            //this.gantt.DateKeyPattern = "yyyyMMdd";
            this.gantt.HeaderHourChanged += new GanttColumnHeaderEventHandler(GanttView_HeaderHourChanged);
            this.gantt.HeaderShiftChanged += new GanttColumnHeaderEventHandler(GanttView_HeaderShiftChanged);
            this.gantt.HeaderDone += new GanttColumnHeaderEventHandler(GanttView_HeaderDone);

            this.gantt.BindRowAdding += new GanttRowEventHandler(GanttView_BindRowAdding);
            this.gantt.BindBarAdded += new GanttCellEventHandler(GanttView_BindBarAdded);
            this.gantt.BindRowAdded += new GanttRowEventHandler(GanttView_BindRowAdded);
            this.gantt.BindDone += new EventHandler(GanttView_BindDone);

            this.gantt.BarDraw += new BarDrawEventHandler(GanttView_BarDraw);

            this.gantt.BarClick += new BarEventHandler(GanttView_BarClick);
        }

        private void InitializeVariables()
        {
            this.isFirst = true;
            this.prevLineID = string.Empty;

            this.preColor = XtraSheetHelper.AltColor;
            this.currColor = XtraSheetHelper.AltColor2;

            this.eqpIDStartRowIndex = 0;
            this.lineIDStartRowIndex = 0;
            this.stepGroupStartRowIndex = 0;
            this.subTotalTIQty = 0;
            this.totalTIQty = 0;
            this.rowTIQty = 0;
            this.rowTOQty = 0;
        }

        #endregion

        #region Query

        private void Search()
        {
            this.gantt.DefaultColumnWidth = this.CellWidthSize;
            this.gantt.DefaultRowHeight = this.CellHeightSize;

            this.drawnDates = new List<DateTime>();
            this.drawnWeeks = new List<string>();

            using (var wc = new Mozart.Studio.UIComponents.WaitCursor(this))
            {
                this.queryButton.Enabled = false;
                this.BindData();
                this.queryButton.Enabled = true;
            }
        }

        protected void BindData()
        {
            this.gantt.Build(
                this.LineID,
                this.StepGroup,
                this.EqpID,
                this.StartDate,
                this.Duration,
                this.MergeThreshold,
                this.SelType
            );

            this.gantt.Expand(false);

            this.gantt.LoadColorMap();

            this.BindGrid();

            this.gantt.SaveColorMap();
        }

        private void BindGrid()
        {
            this.gantt.Workbook.BeginUpdate();
            this.gantt.ResetWorksheet();

            this.SetColumnHeaders();

            this.InitializeVariables();

            var list = this.gantt.GanttInfos.Values.ToList().Distinct().OrderBy(x => x.LineID).ThenBy(x => x.EqpID);
            this.gantt.Bind(list);

            this.gantt.Worksheet.SetRowHeight(this.gantt.FirstRowIndex, this.gantt.LastRowIndex, this.CellHeightSize);
            this.gantt.Workbook.EndUpdate();

            this.gantt.Worksheet.Columns.AutoFit(0, 1);
            this.gantt.Worksheet.Columns.AutoFit(this.gantt.LastColIndex + 1, this.gantt.LastColIndex + 3);
        }

        protected void SetColumnHeaders()
        {
            var colCount = Convert.ToInt32(this.Duration) + 4;
            this.gantt.FixedColCount = 3;
            this.gantt.FixedRowCount = 2;

            this.gantt.SetColumnHeaders(
                colCount,
                new XtraSheetHelper.SfColumn(Constants.COLUMN_LINE_ID, Constants.COLUMN_LINE_ID, 85) { HAlign = SpreadsheetHorizontalAlignment.Center },
                new XtraSheetHelper.SfColumn(Constants.COLUMN_STEP_GROUP, Constants.COLUMN_STEP_GROUP, 90) { HAlign = SpreadsheetHorizontalAlignment.Center },
                new XtraSheetHelper.SfColumn(Constants.COLUMN_EQUIPMENT_ID, Constants.COLUMN_EQUIPMENT_ID, 90) { HAlign = SpreadsheetHorizontalAlignment.Center }
                );

            this.gantt.Worksheet.Rows[2].Style = this.gantt.Workbook.Styles["Header"];
            this.gantt.Worksheet.Rows[1].Style = this.gantt.Workbook.Styles["Header"];
            this.gantt.Worksheet.Rows[0].Style = this.gantt.Workbook.Styles["Header"];
        }

        #endregion

        #endregion

        #region Events

        private void queryButton_Click(object sender, EventArgs e)
        {
            this.Search();
        }

        void GanttView_HeaderHourChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            string key = args.Time.ToString(gantt.DateKeyPattern);
            string caption = gantt.GetJobChgHourCntFormat(args.Time);
            args.ColumnHeader.AddColumn(new XtraSheetHelper.SfColumn(key, caption, gantt.DefaultColumnWidth, true, false));
        }

        void GanttView_HeaderShiftChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            gantt.DateKeyPattern = "yyyyMMddHH";
            var startColName = args.Time.ToString(gantt.DateKeyPattern);
            var endColName = args.Time.AddHours(ShopCalendar.ShiftHours - 1).ToString(gantt.DateKeyPattern);

            if (this.StartDate.ShiftStartTimeOfDayT() == args.Time)
                startColName = this.StartDate.ToString(gantt.DateKeyPattern);
            else if (this.EndDate.ShiftStartTimeOfDayT() == args.Time)
                endColName = this.EndDate.AddHours(-1).ToString(gantt.DateKeyPattern);

            args.ColumnHeader.AddGroupColumn(
                new XtraSheetHelper.SfGroupColumn(gantt.GetJobChgShiftCntFormat(args.Time), startColName, endColName)
                );

        }

        void GanttView_HeaderDone(object sender, GanttColumnHeaderEventArgs e)
        {
            var colHeader = e.ColumnHeader;

            colHeader.AddColumn(new XtraSheetHelper.SfColumn(Constants.COLUMN_QTY, 60) { HAlign = SpreadsheetHorizontalAlignment.Right });
            colHeader.AddColumn(new XtraSheetHelper.SfColumn(Constants.COLUMN_TOTAL, 80) { HAlign = SpreadsheetHorizontalAlignment.Right });

            this.ganttSizeControl.LeftExceptCount = this.gantt.FixedColCount;
            this.ganttSizeControl.TopExceptCount = this.gantt.FixedRowCount;
            this.ganttSizeControl.RightExceptCount = 3;
        }

        void GanttView_BarDraw(object sender, BarDrawEventArgs args)
        {
            var bar = args.Bar as GanttBar;

            args.Background = this.gantt.GetBrushInfo(bar);
            args.DrawFrame = this.gantt.EnableSelect && this.gantt.SelectedBar != null && !this.gantt.IsSameTypeBar(bar);
            args.FrameColor = Color.Black;
            args.ForeColor = (bar.State == EqpState.PM || bar.State == EqpState.DOWN) ? Color.White : Color.Black;
            args.Text = bar.GetTitle();
            if (this.currentBar != null && this.currentBar == args.Bar)
            {
                if (prebar != bar)
                {
                    prebar = bar;
                    prebarwidth = bar.Bounds.Width;
                    bar.Bounds.Width = bar.Bounds.Width - 2.5f;
                }
                var p = new Pen(Color.Black, 4);
                args.Graphics.DrawRectangle(p, bar.Bounds.X, bar.Bounds.Y, bar.Bounds.Width + 0.5f, bar.Bounds.Height);
                
            }

            args.DrawDefault = true;
        }

        void GanttView_BarClick(object sender, BarEventArgs e)
        {
            if (this.gantt.ColumnHeader == null)
                return;

            this.spreadSheet1.BeginUpdate();


            if ((e.Mouse.Button == MouseButtons.Right && e.Bar != null))
            {
                this.gantt.MouseSelectionType = (MouseSelectionType)this.selectionTypeComboBox.SelectedItem;
                this.gantt.TurnOnSelectMode();
                this.gantt.SelectedBar = e.Bar as GanttBar;
            }
            else
            {
                this.gantt.TurnOffSelectMode();
            }

            if (e.Bar != null)
            {
                var bar = e.Bar as GanttBar;
                if (bar != prebar && prebar != null)
                    prebar.Bounds.Width = prebarwidth;
                this.currentBar = bar;
                this.infoTextBox.Text = bar.GetDetail();              
            }

            this.spreadSheet1.EndUpdate();
            this.spreadSheet1.Refresh();
        }

        void GanttView_BindRowAdding(object sender, GanttRowEventArgs args)
        {
            var worksheet = this.gantt.Worksheet;

            var info = args.Item as GanttInfo;
            var colHeader = this.gantt.ColumnHeader;

            this.SetRowHeaderValue(args.RowIndex, info.LineID, info.StepGroup, info.EqpID);
        }

        void GanttView_BindBarAdded(object sender, GanttCellEventArgs args)
        {
            args.Bar.CumulateQty(ref rowTIQty, ref rowTOQty);
        }

        void GanttView_BindRowAdded(object sender, GanttRowEventArgs args)
        {
            this.subTotalTIQty = rowTIQty;
            this.totalTIQty += rowTIQty;

            var colHeader = this.gantt.ColumnHeader;
            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(typeStartRowIndex, Constants.COLUMN_QTY), this.subTotalTIQty);
            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(lineIDStartRowIndex, Constants.COLUMN_TOTAL), this.totalTIQty);
        }

        void GanttView_BindDone(object sender, EventArgs e)
        {
            this.MergeRows(this.lineIDStartRowIndex, gantt.LastRowIndex, 0);
            this.MergeRows(this.stepGroupStartRowIndex, gantt.LastRowIndex, 1);
            this.MergeRows(this.eqpIDStartRowIndex, gantt.LastRowIndex, 2);
            this.MergeRows(this.typeStartRowIndex, gantt.LastRowIndex, 3);

            this.PaintTotColumnCell();
# if true//Cell 가운데 정렬
            for (int x = 0; x <= this.gantt.Worksheet.Rows.LastUsedIndex; x++)
            {
                this.gantt.Worksheet.Rows[x].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            }
#endif
            Skin skin = SpreadsheetSkins.GetSkin(UserLookAndFeel.Default.ActiveLookAndFeel);
            skin.Colors[SpreadsheetSkins.ColorSelectionBorder] = Color.Empty;
        }

        private void PaintTotColumnCell()
        {
            var worksheet = this.gantt.Worksheet;

            var colHeader = this.gantt.ColumnHeader;

            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_LINE_ID), Color.Bisque);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_LINE_ID), Color.White, 2);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_EQUIPMENT_ID), Color.Bisque);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_EQUIPMENT_ID), Color.White, 2);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_STEP_GROUP), Color.Bisque);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_STEP_GROUP), Color.White, 2);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_QTY), Color.FromArgb(219, 236, 216), 2);
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(Constants.COLUMN_TOTAL), Color.FromArgb(204, 255, 195), 2);
        }

        private void SetRowHeaderValue(int rowIndex, string lineID, string stepGroup, string eqpID)
        {
            if (this.isFirst)
            {
                this.prevEqpID = eqpID;
                this.prevLineID = lineID;
                this.prevStepGroup = stepGroup;
                this.typeStartRowIndex = rowIndex;
                this.stepGroupStartRowIndex = rowIndex;
                this.eqpIDStartRowIndex = rowIndex;
                this.lineIDStartRowIndex = rowIndex;

                this.isFirst = false;
            }
            else
            {
                if (lineID != this.prevLineID)
                {
                    this.MergeRows(this.lineIDStartRowIndex, rowIndex - 1, 0);

                    if (rowIndex - this.lineIDStartRowIndex > 1)
                        this.gantt.Worksheet[lineIDStartRowIndex, 0].Alignment.Vertical = SpreadsheetVerticalAlignment.Top;

                    var tmp = this.preColor;
                    this.preColor = this.currColor;
                    this.currColor = tmp;
                    this.prevLineID = lineID;
                    this.lineIDStartRowIndex = rowIndex;
                    this.totalTIQty = 0;
                    this.gantt.GroupRows.Add(rowIndex);
                }

                if (stepGroup != this.prevStepGroup)
                {
                    this.MergeRows(this.stepGroupStartRowIndex, rowIndex - 1, 1);

                    this.prevStepGroup = stepGroup;
                    this.stepGroupStartRowIndex = rowIndex;
                }

                if (eqpID != this.prevEqpID)
                {
                    this.MergeRows(this.eqpIDStartRowIndex, rowIndex - 1, 2);
                    this.MergeRows(this.typeStartRowIndex, rowIndex - 1, 3);
                  
                    this.prevEqpID = eqpID;
                    this.typeStartRowIndex = rowIndex;
                    this.eqpIDStartRowIndex = rowIndex;
                    this.subTotalTIQty = 0;
                    this.rowTIQty = 0;
                }
            }

            XtraSheetHelper.SetCellText(this.gantt.ColumnHeader.GetCellInfo(rowIndex, Constants.COLUMN_EQUIPMENT_ID), eqpID);
            XtraSheetHelper.SetCellText(this.gantt.ColumnHeader.GetCellInfo(rowIndex, Constants.COLUMN_LINE_ID), lineID);
            XtraSheetHelper.SetCellText(this.gantt.ColumnHeader.GetCellInfo(rowIndex, Constants.COLUMN_STEP_GROUP), stepGroup);
        }

        private void MergeRows(int fromRowIdx, int toRowIdx, int colIdx = 0)
        {
            var worksheet = this.gantt.Worksheet;
            worksheet.MergeRowsOneColumn(fromRowIdx, toRowIdx, colIdx);

            if (colIdx > 1)
                this.SetBorder(fromRowIdx, toRowIdx);
        }

        private void SetBorder(int fromRowIdx, int toRowIdx)
        {
            var worksheet = this.gantt.Worksheet;
            var color = System.Drawing.Color.FromArgb(224, 224, 224);

            for (int i = fromRowIdx; i <= toRowIdx; i++)
            {
                if (i == fromRowIdx)
                    worksheet.SetRowBorderTopLine(i, color, BorderLineStyle.Thin);
                else
                    worksheet.SetRowBorderTopLine(i, Color.Transparent, BorderLineStyle.Thin);

                worksheet.SetRowBorderBottomLine(i, Color.Transparent);
            }
        }

        #endregion

        private void stageIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ServiceProvider.SetLocalSetting(Constants.GANTT_STAGE_IDX, lineIDComboBox.SelectedIndex.ToString());
        }

        private void operationIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ServiceProvider.SetLocalSetting(Constants.GANTT_OPER_IDX, this.stepGroupComboBox.SelectedIndex.ToString());
        }

        private void durationNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            this.ServiceProvider.SetLocalSetting(Constants.GANTT_DURATION, this.Duration.ToString());
        }

        private void ganttSizeControl_CellHeightChanged(object sender, EventArgs e)
        {
            this.ServiceProvider.SetLocalSetting(Constants.GANTT_CELL_HEIGHT, this.ganttSizeControl.CellHeight.ToString());
        }

        private void ganttSizeControl_CellWidthChanged(object sender, EventArgs e)
        {
            prebar = null;
            prebarwidth = 0;
            this.ServiceProvider.SetLocalSetting(Constants.GANTT_CELL_WIDTH, this.ganttSizeControl.CellWidth.ToString());          
        }
    }
}
