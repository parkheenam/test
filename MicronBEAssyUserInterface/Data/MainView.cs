using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;

namespace MicronBEAssyUserInterface.Data
{
    public partial class MainView : UserControl
    {
        public DockManager DockManager
        {
            get { return this.dockManager1; }
        }

        public MainView()
        {
            InitializeComponent();
        }
    }
}
