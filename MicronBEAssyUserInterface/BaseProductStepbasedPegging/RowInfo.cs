using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicronBEAssyUserInterface.Class;

namespace MicronBEAssyUserInterface.BaseProductStepbasedPegging
{
    public class RowInfo
    {
        public UIProductDetail ProductDetail { get; private set; }

        public int BaseCompQty { get; private set; }

        public decimal DemandQty { get; private set; }

        public decimal ActQty { get; private set; }

        public decimal TotalRemainQty { get; private set; }

        public RowInfo(UIProductDetail productDetail, int baseCompQty, decimal demandQty, decimal actQty, decimal totalRemainQty) 
        {
            this.ProductDetail = productDetail;
            this.BaseCompQty = baseCompQty;
            this.DemandQty = demandQty;
            this.ActQty = actQty;
            this.TotalRemainQty = totalRemainQty;
        }
    }
}
