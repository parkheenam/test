using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using Mozart.Studio.TaskModel.UserLibrary;

using MicronBEAssy;

using MicronBEAssy.Outputs;
using MicronBEAssy.Inputs;

using DevExpress.XtraEditors;
using DevExpress.XtraSpreadsheet;
using Mozart.Text;
using Mozart.Collections;

namespace MicronBEAssyUserInterface.ProdGantt
{
    public class GanttMaster : GanttView
    {
        #region Variables

        IExperimentResultItem result;
        ModelDataContext modelCtx;
        
        HashSet<Tuple<string, string, string, string, string>> rowValueInfos;
        Dictionary<Tuple<string, string, string, string, string>, GanttInfo> ganttInfos;

        HashSet<string> visibleItems;

        Dictionary<string, Equipment> validEquipments;

        ColorGenerator colorGenerator;
        Dictionary<string, Color> colorMap;
        List<Color> usedColors;

        readonly BrushInfo emptyBrush = new BrushInfo(Color.Transparent);

        #endregion

        #region Properties

        public Dictionary<Tuple<string, string, string, string, string>, GanttInfo> GanttInfos
        {
            get
            {
                if (this.ganttInfos == null)
                    this.ganttInfos = new Dictionary<Tuple<string, string, string, string, string>, GanttInfo>();

                return this.ganttInfos;
            }
        }

        public HashSet<Tuple<string, string, string, string, string>> RowValueInfos
        {
            get
            {
                if (this.rowValueInfos == null)
                    this.rowValueInfos = new HashSet<Tuple<string, string, string, string, string>>();

                return this.rowValueInfos;
            }
        }

        public HashSet<string> VisibleItems
        {
            get
            {
                if (this.visibleItems == null)
                    this.visibleItems = new HashSet<string>();

                return this.visibleItems;
            }
        }

        public MouseSelectionType MouseSelectionType { get; set; }

        public int MergeThreshold { get; set; }

        #endregion

        #region etc


        string colorPath = "BCP_Color.dat";

        #endregion

        #region Constructors

        public GanttMaster(SpreadsheetControl grid, IExperimentResultItem result)
            : base(grid)
        {
            this.result = result;
            this.modelCtx = this.result.GetCtx<ModelDataContext>();
            
            this.colorGenerator = new ColorGenerator();
        }

        #endregion

        #region Methods

        #region Bind Data

        public void Build(
            string lineID,
            string prodID,
            string stepGroup,
            DateTime startDate,
            int duration,
            string seltype
            )
        {
            this.ClearData();

            this.FromTime = Convert.ToDateTime(startDate);
            this.ToTime = this.FromTime.AddDays(Convert.ToDouble(duration));

            this.SetValidEquipments(lineID, stepGroup);

            this.FillEqpPlan(lineID, prodID, stepGroup, startDate, duration, seltype); //this.GetLoadHist(fromTime.Date), toTime
        }

        private void ClearData()
        {
            this.Clear();
            this.GanttInfos.Clear();
            this.VisibleItems.Clear();
            this.RowValueInfos.Clear();
        }

        private void SetValidEquipments(string lineID, string stepGroup)
        {
            this.validEquipments = (from row in this.modelCtx.Equipment
                                    where (lineID == Constants.ALL ? true : row.LINE_ID == lineID && stepGroup == Constants.ALL ? true : row.STEP_GROUP == stepGroup)
                                    select row).ToDictionary(x => x.EQP_ID);
        }

        private void FillEqpPlan(
            string lineID,
            string prodID,
            string stepGroup,
            DateTime startDate,
            int duration,
            string seltype)
        {
            var resultCtx = this.result.GetCtx<ResultDataContext>();
            var inputCtx = this.result.GetCtx<ModelDataContext>();

            var eqpInfoDic = (from a in inputCtx.Equipment
                              select new { EQP_ID = a.EQP_ID, EQP_TYPE = a.STEP_GROUP }).Distinct();
            var eqpInfo = eqpInfoDic.OrderBy(x => x.EQP_ID).ToDictionary(x => x.EQP_ID, x => x.EQP_TYPE);

            var mcpBomDic = (from a in inputCtx.McpBom
                             select new { TO_PROD_ID = a.TO_PROD_ID, COMP_SEQ = a.COMP_SEQ }).Distinct();
            var mcpBom = mcpBomDic.OrderBy(x => x.TO_PROD_ID).ToDictionary(x => x.TO_PROD_ID, x => x.COMP_SEQ);

            var finalProdDic = (from a in inputCtx.McpBom
                                select new { TO_PROD_ID = a.TO_PROD_ID, FINAL_PROD_ID = a.FINAL_PROD_ID }).Distinct();
            var finalProd = finalProdDic.OrderBy(x => x.TO_PROD_ID).ToDictionary(x => x.TO_PROD_ID, x => x.FINAL_PROD_ID);

            var endDate = startDate.AddDays(duration);

            var plans = resultCtx.EqpPlan.OrderBy(x => x.START_TIME).ThenBy(x => x.STEP_ID);

            foreach (var plan in plans)
            {
                if (lineID != Constants.ALL && plan.LINE_ID != lineID)
                    continue;

                if (stepGroup != Constants.ALL && plan.STEP_GROUP != stepGroup)
                    continue;

                if (prodID != Constants.ALL && plan.PRODUCT_ID != prodID)
                    continue;

                if (!string.IsNullOrEmpty(plan.EQP_ID) && !LikeUtility.Like(plan.EQP_ID, plan.EQP_ID, true))
                    continue;

                if (!this.validEquipments.ContainsKey(plan.EQP_ID))
                    continue;

                string findAoProd = string.Empty;
                finalProd.TryGetValue(plan.PRODUCT_ID, out findAoProd);

                var key = Tuple.Create(plan.LINE_ID, plan.EQP_ID, findAoProd, plan.PRODUCT_ID, plan.STEP_ID);

                if (this.RowValueInfos.Contains(key) == false)
                    this.RowValueInfos.Add(key);
            }

            foreach (var rowInfo in this.RowValueInfos)
            {
                string findStepGroup = string.Empty;
                eqpInfo.TryGetValue(rowInfo.Item2, out findStepGroup);

                int findMcpSeq = 0;
                mcpBom.TryGetValue(rowInfo.Item4, out findMcpSeq);

                string findAoProd = string.Empty;
                finalProd.TryGetValue(rowInfo.Item4, out findAoProd);

                var info = this.TryGetGanttInfo(rowInfo.Item1, findStepGroup, rowInfo.Item2, rowInfo.Item3, rowInfo.Item4, findMcpSeq, rowInfo.Item5);
                var key = new Tuple<string, string, string, string, string>(lineID, rowInfo.Item2, rowInfo.Item3, rowInfo.Item4, rowInfo.Item5);
                List<GanttBar> items = new List<GanttBar>();

                this.AddVisibleItem(rowInfo.Item2);

                foreach (var plan in plans)
                {
                    if (rowInfo.Item2 != plan.EQP_ID)
                        continue;

                    var startTime = plan.START_TIME;
                    if (startTime < this.FromTime || startTime >= this.ToTime)
                        continue;

                    var endTime = plan.END_TIME;
                    if (startTime >= endTime)
                        continue;

                    var state = plan.STATUS == Constants.STR_PM ? EqpState.PM : plan.STATUS == Constants.STR_SETUP ? EqpState.SETUP : EqpState.BUSY;

                    var barKey = plan.PRODUCT_ID;
                    if (string.IsNullOrEmpty(barKey))
                        barKey = Mozart.Studio.TaskModel.UserLibrary.StringUtility.IdentityNull;

                    var item = new EqpPlanItem(plan);
                    var currentBar = new GanttBar(item, startTime, endTime, item.Qty, item.Qty, state);

                    items.Add(currentBar);
                }

                foreach (var item in items) 
                {
                    info.AddItem(item.BarKey, item, seltype);
                }
            }
        }

        private GanttInfo TryGetGanttInfo(string lineID, string eqpGroup, string eqpID, string aoProd, string prod, int mcpSeq, string stepID)
        {
            var key = new Tuple<string, string, string, string, string>(lineID, eqpID, aoProd, prod, stepID);

            GanttInfo info;
            if (!this.GanttInfos.TryGetValue(key, out info))
            {
                info = new GanttInfo(lineID, eqpGroup, eqpID, aoProd, prod, mcpSeq, stepID);
                this.GanttInfos.Add(key, info);
            }

            return info;
        }

        private void AddVisibleItem(string item)
        {
            if (!this.VisibleItems.Contains(item))
                this.VisibleItems.Add(item);
        }

        public void Expand(bool isDefault)
        {
            foreach (GanttInfo info in this.GanttInfos.Values)
            {
                if (this.visibleItems.Contains(info.EqpID) == false)
                    continue;

                info.Expand(isDefault);
                info.LinkBar(this, isDefault);
            }
        }

        #endregion

        #region Paint Bar

        public BrushInfo GetBrushInfo(GanttBar bar)
        {
            BrushInfo brushinfo = null;

            if (bar.State == EqpState.SETUP)
                brushinfo = new BrushInfo(Color.Orange);
            else if (bar.State == EqpState.PM)
                brushinfo = new BrushInfo(Color.Red);
            else if (bar.State == EqpState.DOWN)
                brushinfo = new BrushInfo(HatchStyle.Percent30, Color.Gray, Color.Black);
            else 
                brushinfo = new BrushInfo(GetColor(bar.BarKey));

            if (!this.EnableSelect || this.SelectedBar == null)
            {
                bar.BackColor = brushinfo.BackColor;
                return brushinfo;
            }

            if (!this.IsSameTypeBar(bar))
            {
                bar.BackColor = emptyBrush.BackColor;
                return emptyBrush;
            }

            if (this.MouseSelectionType == MouseSelectionType.ProductID && bar.State == EqpState.SETUP && this.SelectedBar.State != EqpState.SETUP)
            {
                bar.BackColor = emptyBrush.BackColor;
                return emptyBrush;
            }

            bar.BackColor = brushinfo.BackColor;
            return brushinfo;
        }

        private Color GetColor(string key)
        {
            Color color;
            if (!this.colorMap.TryGetValue(key, out color))
            {
                if (this.usedColors == null)
                    this.usedColors = new List<Color>();

                var random = new Random(10);

                var dummy = 0;
                color = this.colorGenerator.GetColor(key);
                while (usedColors.Contains(color))
                {
                    var r = -1;
                    var g = -1;
                    var b = -1;

                    // 라이브러리 상 49개의 ColorGenerator에서 (_colorGen.GetColor(key)) 한번의 조회에 49개의 색깔만 만들수 있음
                    if (usedColors.Count >= 49)
                    {
                        r = random.Next(128, 255);
                        g = random.Next(128, 255);
                        b = random.Next(128, 255);

                        color = Color.FromArgb(255, r, g, b);
                    }
                    else
                    {
                        color = this.colorGenerator.GetColor(key + dummy.ToString());
                    }

                    if ((r == 0 && g == 0 && b == 0) || (r == 255 && g == 0 && b == 0) || (r == 128 && g == 128 && b == 128) || (r == 255 && g == 165 && b == 0))
                        continue;

                    dummy++;

                    var stop = dummy >= 300 && dummy >= usedColors.Count * 2;

                    if (!usedColors.Contains(color) || stop)
                        break;
                }

                if (!this.usedColors.Contains(color))
                    this.usedColors.Add(color);

                this.colorMap[key] = color;
            }

            return color;
        }

        public void LoadColorMap()
        {
            var path = this.GetColorMapPath();
            this.usedColors = new List<Color>();

            if (!File.Exists(path))
            {
                this.colorMap = new Dictionary<string, Color>();
                foreach (var info in this.GanttInfos.Values)
                {
                    foreach (var key in info.Items.Keys)
                    {
                        foreach (GanttBar bar in info.Items[key])
                        {
                            if (bar.BarKey != null)
                                this.GetColor(bar.BarKey);
                        }
                    }
                }
            }
            else
            {
                using (var input = File.OpenRead(path))
                {
                    try
                    {
                        var bf = new BinaryFormatter();
                        this.colorMap = (Dictionary<string, Color>)bf.Deserialize(input);
                    }
                    catch
                    {
                        this.colorMap = null;
                    }
                }

                if (this.colorMap == null)
                    this.colorMap = new Dictionary<string, Color>();
            }

            if (this.colorMap != null)
            {
                foreach (var color in this.colorMap.Values)
                {
                    if (this.usedColors.Contains(color) == false)
                        this.usedColors.Add(color);
                }
            }
        }

        public void SaveColorMap()
        {
            var path = this.GetColorMapPath();
            using (var output = File.Create(path))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(output, this.colorMap);
            }
        }

        private string GetColorMapPath()
        {
            var appDataDir = Environment.ExpandEnvironmentVariables("%AppData%");
            var mozartDir = Constants.DIR_MOZART;
            var studioDir = System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace("_", " ");
            var fileName = Constants.FILE_NAME_COLOR_MAP;

            var path = Path.Combine(appDataDir, mozartDir, studioDir, fileName);
            return path;
        }

        #endregion

        #region Select Bar

        public bool IsSameTypeBar(GanttBar bar)
        {
            var selectedBar = this.SelectedBar as GanttBar;

            if (this.SelectedBar.State == EqpState.SETUP)
            {
                if (this.SelectedBar.State == bar.State)
                    return true;
            }
            else
            {
                switch (this.MouseSelectionType)
                {
                    case MouseSelectionType.ProductID:
                        return bar.ProductID == selectedBar.ProductID;

                    case MouseSelectionType.LotID:
                        return bar.LotID == selectedBar.LotID;
                }
            }

            return false;
        }

        private string GetBatchID(string batchID)
        {
            int idx = batchID.IndexOf('_');
            if (idx > 0)
                return batchID.Substring(0, idx);

            return batchID;
        }

        public void TurnOnSelectMode()
        {
            this.EnableSelect = true;
        }

        public void TurnOffSelectMode()
        {
            this.EnableSelect = false;
        }

        public string GetJobChgHourCntFormat(DateTime targetTime)
        {
            var hour = targetTime.Hour.ToString();
            var chgTime = targetTime.ToString(this.DateKeyPattern);

            return string.Format("{0}", hour);
        }

        public string GetJobChgShiftCntFormat(DateTime shiftTime)
        {
            var shift = shiftTime.ToString(this.DateGroupPattern);
            return string.Format("{0}", shift);
        }


        #endregion

        #endregion
    }
}
