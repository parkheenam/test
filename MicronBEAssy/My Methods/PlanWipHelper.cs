using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class PlanWipHelper
    {
        public static MicronBEAssyWipInfo GetWipInfo(this MicronBEAssyPlanWip planWip)
        {
            try
            {
                return planWip.Wip as MicronBEAssyWipInfo;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(MicronBEAssyWipInfo);
            }
        }
    }
}
