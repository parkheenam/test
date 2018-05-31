using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.Projects;

namespace MicronBEAssyUserInterface
{
    public static class UIHelper
    {
        public static DateTime GetEngineEndTime(IExperimentResultItem result)
        {
            double period = double.Parse(result.Experiment.GetArgument("period").ToString());

            DateTime startTime = result.StartTime;

            return startTime.AddDays(period);
        }
    }
}
