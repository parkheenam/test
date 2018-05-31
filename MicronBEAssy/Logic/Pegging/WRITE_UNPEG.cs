using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Pegging;
using MicronBEAssy.Outputs;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class WRITE_UNPEG
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        public void WRITE_UNPEG0(Mozart.SeePlan.Pegging.PegPart pegPart, ref bool handled)
        {
            try
            {
                foreach (MicronBEAssyPlanWip planWip in InputMart.Instance.MicronBEAssyPlanWip.Values)
                {
                    if (planWip.Qty <= 0)
                        continue;

                    if (planWip.MapCount > 0)
                        WriteHelper.WriteUnpeg(planWip, UnpegReason.EXCESS);
                    else
                        WriteHelper.WriteUnpeg(planWip, UnpegReason.NO_TARGET);
                }

                List<string> list = new List<string>();
                foreach (var key in InputMart.Instance.MicronBEAssyBEMoMaster.Keys)
                    list.Add(key);

                foreach (MicronBEAssyPlanWip wip in InputMart.Instance.MicronBEAssyActPlanWips.Values)
                {
                    if (wip.Qty <= 0)
                        continue;

                    if (wip.MapCount > 0)
                        WriteHelper.WriteUnpeg(wip, UnpegReason.EXCESS);

                    else
                    {
                        if (list.Contains(wip.GetWipInfo().WipProductID))
                            WriteHelper.WriteUnpeg(wip, UnpegReason.EXCESS);
                        else
                            WriteHelper.WriteUnpeg(wip, UnpegReason.NO_TARGET);
                    }
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }

        }
    }
}
