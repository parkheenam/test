using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicronBEAssyUserInterface.Class
{
    public class UIProcess
    {
        public string LineID { get; private set; }
        public string ProcessID { get; private set; }
        public Dictionary<string, UIStep> Steps { get; private set; }
        public string DAWBProcess { get; set; }
        public Dictionary<int, UIStep> DaSteps { get; set; }

        public UIProcess(string lineID, string processID)
        {
            this.LineID = lineID;
            this.ProcessID = processID;
            this.Steps = new Dictionary<string, UIStep>();
            this.DAWBProcess = string.Empty;
            this.DaSteps = new Dictionary<int, UIStep>();
        }

        public void AddStep(UIStep step)
        {
            if (Steps.ContainsKey(step.StepID) == false)
                Steps.Add(step.StepID, step);
        }

        public UIStep FindStep(string stepID)
        {
            UIStep step;
            Steps.TryGetValue(stepID, out step);

            return step;
        }

        public UIStep FindDaStep(int sequence)
        {
            UIStep daStep;
            DaSteps.TryGetValue(sequence, out daStep);

            return daStep;
        }
    }
}
