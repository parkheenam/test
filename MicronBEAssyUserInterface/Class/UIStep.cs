using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicronBEAssyUserInterface.Class
{
    public class UIStep
    {
        public string StepID { get; private set; }
        public decimal Sequence { get; private set; }
        public UIProcess Process { get; private set; }
        public string StepGroup { get; private set; }
        public UIStepCategory Categroy { get; set; }
        public int DaThroughCount { get; set; }

        public UIStep(UIProcess process, string stepID, decimal sequence, string stepGroup)
        {
            this.Process = process;
            this.StepID = stepID;
            this.Sequence = sequence;
            this.StepGroup = stepGroup;
        }
    }
}
