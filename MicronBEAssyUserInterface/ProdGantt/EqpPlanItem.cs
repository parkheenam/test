using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicronBEAssy.Outputs;

namespace MicronBEAssyUserInterface.ProdGantt
{
    public class EqpPlanItem
    {
        public string LineID { get; set; }
        public string ProductID { get; set; }
        public string ProcessID { get; set; }
        public string StepID { get; set; }
        public string StepGroup { get; set; }
        public string EqpID { get; set; }
        public string LotID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Qty { get; set; }
        public string Status { get; set; }
        public string AoProductID { get; set; }
        public int McpSeq { get; set; }

        public EqpPlanItem(EqpPlan row)
        {
            this.LineID = row.LINE_ID;
            this.ProductID = row.PRODUCT_ID;
            this.ProcessID = row.PROCESS_ID;
            this.StepID = row.STEP_ID;
            this.StepGroup = row.STEP_GROUP;
            this.EqpID = row.EQP_ID;
            this.LotID = row.LOT_ID;
            this.StartTime = row.START_TIME;
            this.EndTime = row.END_TIME;
            this.Qty = Convert.ToDouble(row.QTY);
            this.Status = this.Status;
        }
    }
}
