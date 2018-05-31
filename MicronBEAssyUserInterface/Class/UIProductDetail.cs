using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicronBEAssy.Inputs;

namespace MicronBEAssyUserInterface.Class
{
    public class UIProductDetail
    {
        public string LineID { get; private set; }
        public string ProductID { get; private set; }
        public string ProductName { get; private set; }
        public UIProcess Process { get; private set; }
        public string DesignID { get; private set; }

        public UIProductDetail(ProductMaster productMaster, UIProcess process)
        {
            this.LineID = productMaster.LINE_ID;
            this.ProductID = productMaster.PRODUCT_ID;
            this.ProductName = productMaster.PRODUCT_NAME;

            if (this.ProductName == null)
                this.ProductName = string.Empty;

            this.Process = process;
            this.DesignID = productMaster.DESIGN_ID;
        }
    }
}
