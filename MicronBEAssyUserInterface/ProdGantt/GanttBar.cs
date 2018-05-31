using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using System.Drawing;
using Mozart.Studio.TaskModel.UserLibrary;

namespace MicronBEAssyUserInterface.ProdGantt
{
    public class GanttBar : Bar
    {
        public string LineID { get; set; }
        public string ProductID { get; set; }
        public string ProcessID { get; set; }
        public string StepID { get; set; }
        public string EqpID { get; set; }
        public string LotID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Qty { get; set; }
        public string Status { get; set; }

        public Color BackColor { get; set; }

        public string BarKey
        {
            get
            {
                return this.ProductID;
            }
        }

        public GanttBar(
            EqpPlanItem item,
            DateTime tkIn,
            DateTime tkOut,
            double tiQty,
            double toQty,
            EqpState state
            )
            : base(tkIn, tkOut, tiQty, toQty, state)
        {
            this.LineID = item.LineID;
            this.ProductID = item.ProductID;
            this.ProcessID = item.ProcessID;
            this.StepID = item.StepID;
            this.EqpID = item.EqpID;
            this.LotID = item.LotID;
            this.StartTime = item.StartTime;
            this.EndTime = item.EndTime;
            this.Qty = item.Qty;
            this.Status = item.Status;
        }

        public string GetDetail(bool simple = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("- EQP_ID : {0}", this.EqpID));
            sb.AppendLine(String.Format("- PRODUCT_ID : {0}", this.State == EqpState.SETUP ? StringUtility.IdentityNull : this.ProductID));
            sb.AppendLine(String.Format("- LOT_ID : {0}", this.LotID));
            sb.AppendLine(String.Format("- TIME(START_TIME / END_TIME) : {0} / {1}", this.StartTime, this.EndTime));
            sb.AppendLine(String.Format("- TIME(TkInTike / TkOutTime)  : {0} / {1}", this.TkinTime.ToString(), this.TkoutTime.ToString()));
            sb.AppendLine(String.Format("- PLAN_QTY : {0}", this.TOQty));
            sb.AppendLine(String.Format("- STEP_ID : {0}", this.StepID));
            //sb.AppendLine(String.Format("- PROCESS TIME : {0}", Math.Ceiling(this.EndTime.Subtract(this.StartTime).TotalHours)));
            sb.AppendLine(String.Format("- PROCESS TIME : {0}", Math.Round(this.EndTime.Subtract(this.StartTime).TotalMinutes / 60, 2)));

            return sb.ToString();
        }

        public string GetTitle()
        {
            if (this.ProductID == Constants.COLUMN_PM || this.ProductID == Constants.COLUMN_SETUP)
                return this.ProductID;

            if (this.State != EqpState.BUSY)
                return string.Format("{0}", this.State);

            return string.Format("{0}/{1}({2})/{3}", this.ProductID, this.EqpID, TIQty.ToString(), this.StepID);
        }
    }
}
