using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraEditors;

namespace MicronBEAssyUserInterface.TargetAnalysis
{
    public partial class SubView : UserControl
    {
        public DockManager DockManager
        {
            get { return this.dockManager1; }
        }

        public DocumentManager DocumentManager
        {
            get { return this.documentManager1; }
        }

        public SubView()
        {
            InitializeComponent();
        }

        private void dockManager1_ClosedPanel(object sender, DockPanelEventArgs e)
        {
            DockManager dockManager = sender as DockManager;
            DockPanel panel = e.Panel;

            dockManager.RemovePanel(panel);
        }
    }
}
