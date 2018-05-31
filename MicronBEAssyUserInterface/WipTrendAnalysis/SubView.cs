using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace MicronBEAssyUserInterface.WipTrendAnalysis
{
    public partial class SubView : UserControl
    {
        public SplitContainerControl SplitContainerControl
        {
            get { return this.splitContainerControl1; }
        }

        public SubView()
        {
            InitializeComponent();
        }
    }
}
