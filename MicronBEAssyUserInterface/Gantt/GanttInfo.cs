using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;

namespace MicronBEAssyUserInterface.Gantt
{
    public class GanttInfo : GanttItem
    {
        public string LineID { get; set; }
        public string StepGroup { get; set; }
        public string EqpID { get; set; }

        public GanttInfo(string lineID, string StepGroup, string eqpID)
            : base()
        {
            this.LineID = lineID;
            this.EqpID = eqpID;
            this.StepGroup = StepGroup;
        }

        public void AddItem(string key, GanttBar bar, string seltype)
        {
            BarList list;
            if (!this.Items.TryGetValue(key, out list))
            {
                this.Items.Add(key, list = new BarList());
                list.Add(bar);
                return;
            }

            foreach (GanttBar it in list)
            {
                if (seltype.StartsWith("Product"))
                {
                    if (it.BarKey != bar.BarKey || it.State != bar.State)
                        continue;

                    if (it.Merge(bar))
                    {
                        it.TOQty += bar.TOQty;
                        it.LotID += "/" + bar.LotID;
                        it.EndTime = bar.EndTime;
                        return;
                    }
                }
                else
                {
                    if (it.LotID != bar.LotID || it.State != bar.State)
                        continue;

                    if (it.Merge(bar))
                    {
                        it.TOQty += bar.TOQty;
                        it.ProductID += "/" + bar.ProductID;
                        it.EndTime = bar.EndTime;
                        return;
                    }
                }
            }

            list.Add(bar);
        }
    }
}
