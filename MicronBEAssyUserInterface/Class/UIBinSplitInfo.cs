using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicronBEAssyUserInterface.Class
{
    public class UIBinSplitInfo
    {
        public UIProduct FromProduct { get; private set; }
        public UIProduct ToProduct { get; private set; }
        public decimal Portion { get; private set; }
        public string Grade { get; private set; }
        public int BinRank { get; private set; }

        public UIBinSplitInfo(UIProduct fromProduct, UIProduct toProduct, decimal portion, string grade, int binRank)
        {
            this.FromProduct = fromProduct;
            this.ToProduct = toProduct;
            this.Portion = portion;
            this.Grade = grade;
            this.BinRank = binRank;
        }
    }
}
