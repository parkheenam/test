using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicronBEAssyUserInterface.CycleTime
{
    public class CycleTimeRawData
    {
        public string DesignID { get; private set; }
        public string FinalProductID { get; private set; }
        public string ProductID { get; private set; }
        public string StepID { get; private set; }
        public decimal StepSeq { get; private set; }
        public double Run { get; set; }
        public double Wait { get; set; }
        public double RunQty { get; set; }
        public double WaitQty { get; set; }
        public int RunLotCount { get; set; }
        public int WaitLotCount { get; set; }

        public double RunDiff
        {
            get { return 0; }
        }

        public double WaitDiff
        {
            get { return 0; }
        }

        public double FlowFactor
        {
            get
            {
                if (Run == 0)
                    return 0;

                return Math.Round((Run + Wait) / Run, 2);
            }
        }

        public CycleTimeRawData(string desingID, string finalProduct, string productID, string stepID, decimal stepSeq)
        {
            this.FinalProductID = finalProduct;
            this.DesignID = desingID;
            this.ProductID = productID;
            this.StepID = stepID;
            this.StepSeq = stepSeq;
        }
    }
}
