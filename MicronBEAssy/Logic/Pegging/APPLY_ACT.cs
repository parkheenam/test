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
using Mozart.Text;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class APPLY_ACT
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<Mozart.SeePlan.Pegging.IMaterial> GET_ACTS0(Mozart.SeePlan.Pegging.PegPart pegPart, ref bool handled, IList<IMaterial> prevReturnValue)
        {
            List<IMaterial> list = new List<IMaterial>();

            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;

                foreach (MicronBEAssyPlanWip wip in InputMart.Instance.MicronBEAssyActPlanWips.Values)
                {
                    if (wip.GetWipInfo().WipProductID != pp.Product.ProductID || wip.GetWipInfo().LineID != pp.Product.LineID)
                        continue;

                    list.Add(wip);

                }
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }

            return list;
        }

        /// <summary>
        /// </summary>
        /// <param name="target"/>
        /// <param name="m"/>
        /// <param name="qty"/>
        /// <param name="handled"/>
        public void WRITE_ACT_PEG0(PegTarget target, IMaterial m, double qty, ref bool handled)
        {
            try
            {
                WriteHelper.WriteActPeg(target, m, qty);
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }
    }
}
