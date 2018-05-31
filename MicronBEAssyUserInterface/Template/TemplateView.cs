using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;

namespace MicronBEAssyUserInterface.Template
{
    public partial class TemplateView : XtraUserControlView
    {
        public TemplateView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }
    }
}
