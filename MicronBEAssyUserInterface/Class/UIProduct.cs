using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicronBEAssyUserInterface.Class
{
    public class UIProduct
    {
        public string ProductID { get; private set; }
        public string ProductName { get; private set; }
        public string FinalProductID { get; set; }
        public string LineID { get; private set; }
        public string StepID { get; set; }
        public bool IsMcpPart { get; private set; }
        public bool IsMidPart { get; private set; }
        public bool IsBase { get; private set; }
        public bool IsWaferPart { get; set; }
        public int CompSeq { get; set; }
        public int CompQty { get; set; }
        public UIProcess Process { get; private set; }
        public UIProductDetail ProductDetail { get; private set; }
        public List<UIProduct> Nexts { get; set; }
        public List<UIProduct> Prevs { get; set; }
        public Dictionary<UIProduct, UIBinSplitInfo> BinSplitInfos { get; set; }
        public Dictionary<int, UIProduct> AltProductInfos { get; set; }
        public Tuple<string, string, bool, bool, int> Key { get; private set; }
        public HashSet<UIProduct> FinalProducts { get; set; }
        public List<UIProduct> AssyParts { get; set; }
        public int MaxSequence
        {
            get
            {
                int cnt = AssyParts.Count;
                if(cnt <= 0)
                    cnt = 1;

                return cnt;
            }
        }

        public UIProduct(string lineID, string productID, string productName, bool isMcpPart, bool isMidPart, int compSeq, int compQty = 1)
        {
            this.AssyParts = new List<UIProduct>();
            this.Nexts = new List<UIProduct>();
            this.Prevs = new List<UIProduct>();
            this.BinSplitInfos = new Dictionary<UIProduct, UIBinSplitInfo>();
            this.AltProductInfos = new Dictionary<int, UIProduct>();
            this.FinalProducts = new HashSet<UIProduct>();

            this.ProductID = productID;
            this.ProductName = productName;
            this.LineID = lineID == null ? string.Empty : lineID;
            this.IsMcpPart = isMcpPart;
            this.IsMidPart = isMidPart;
            this.CompSeq = compSeq;
            this.CompQty = compQty;

            if ((isMcpPart == false && isMidPart == false && compSeq == 1) || isMidPart || (isMcpPart && isMidPart == false && compSeq == 1))//최종이거나 MidPart이거나 1차 투입 Part 이면
                this.IsBase = true;

            this.ProductDetail = DataHelper.FindProductDetail(this.LineID, this.ProductID);

            if (this.ProductDetail != null)
            {
                this.Process = this.ProductDetail.Process;
            }

            
#if DEBUG
            if(productID == "140907")
                Console.WriteLine();
#endif

            this.Key = new Tuple<string, string, bool, bool, int>(lineID, productID, isMcpPart, isMidPart, compSeq);
        }
    }
}
