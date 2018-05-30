using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Pegging;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class PREPARE_TARGET
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart PREPARE_TARGET0(PegPart pegPart, ref bool handled, PegPart prevReturnValue)
        {
            try
            {
                MergedPegPart mp = pegPart as MergedPegPart;

                foreach (MicronBEAssyBEMoMaster moMaster in InputMart.Instance.MicronBEAssyBEMoMaster.Values)
                {
                    MicronBEAssyBEPegPart pp = new MicronBEAssyBEPegPart(moMaster, moMaster.Product);

                    foreach (MicronBEAssyBEMoPlan moPlan in moMaster.MoPlanList)
                    {
                        MicronBEAssyBEPegTarget target = new MicronBEAssyBEPegTarget(pp, moPlan);
                        pp.AddPegTarget(target);
                    }

                    mp.Merge(pp);
                }

                return mp;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(PegPart);
            }
        }
    }
}
