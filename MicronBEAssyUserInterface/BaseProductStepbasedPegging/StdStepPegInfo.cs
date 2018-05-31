using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicronBEAssy.Inputs;

namespace MicronBEAssyUserInterface.BaseProductStepbasedPegging
{
    public class StdStepColumnInfo
    {
        public IComparable Key 
        { 
            get 
            { 
                return Tuple.Create(this.ProductID, this.StdStepID, this.StdSequence); 
            } 
        }
       
        public string ProductID { get; private set; }

        public string StdStepID { get; private set; }

        public decimal StdSequence { get; private set; }

        public List<Wip> MatchWips { get; private set; }

        public decimal WipQty { get; set; }

        public decimal PeggedQty { get; set; }

        public decimal UnpeggedQty { get; set; }

        public decimal RemainQty { get; set; }

        public StdStepColumnInfo(string productID, string stdStepID, decimal stdSequence, List<Wip> matchWips = null, decimal wipQty = 0, decimal peggedQty = 0, decimal unpeggedQty = 0, decimal remainQty = 0) 
        {
            this.ProductID = productID;
            this.StdStepID = stdStepID;
            this.StdSequence = stdSequence;
            this.MatchWips = matchWips;
            this.WipQty = wipQty;
            this.PeggedQty = peggedQty;
            this.UnpeggedQty = unpeggedQty;
            this.RemainQty = remainQty;
        }

        public void SetMatchWips(List<Wip> matchWips) 
        {
            this.MatchWips = matchWips;
        }

        public void AddMatchWip(Wip wip)
        {
            this.MatchWips.Add(wip);
        }

    }
}
