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
using Mozart.SeePlan.Simulation;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class PEG_WIP
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<Mozart.SeePlan.Pegging.IMaterial> GET_WIPS0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun, ref bool handled, IList<IMaterial> prevReturnValue)
        {
            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;

                List<IMaterial> wips = new List<IMaterial>();

                ICollection<MicronBEAssyPlanWip> wipList = null;
                if (InputMart.Instance.MicronBEAssyPlanWip.TryGetValue(pegPart.CurrentStep.StepID, out wipList))
                {
                    foreach (MicronBEAssyPlanWip wip in wipList)
                    {
                        if (wip.Qty <= 0)
                            continue;

                        if (isRun)
                        {
                            if (wip.Wip.CurrentState != EntityState.RUN)
                                continue;
                        }
                        else
                        {
                            if (wip.Wip.CurrentState == EntityState.RUN)
                                continue;
                        }

                        if (pp.CurrentStep.StepID == Constants.LOTSRECEIVED)
                        {
#if DEBUG
                            if(pp.Product.HasPrevs && pp.Product.Prevs.Contains(wip.Product))
                                Console.WriteLine();
#endif

                            if (pp.Product.ProductID == wip.Product.ProductID
                                || (pp.Product.HasPrevs && pp.Product.Prevs.Contains(wip.Product)))
                            {
                                wip.MapCount++;
                                wips.Add(wip);
                            }
                        }
                        else
                        {
                            if (pp.Product.ProductID == wip.Product.ProductID)
                            {
                                wip.MapCount++;
                                wips.Add(wip);
                            }
                        }
                    }
                }

                return wips;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IList<Mozart.SeePlan.Pegging.IMaterial>);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="target"/>
        /// <param name="m"/>
        /// <param name="qty"/>
        /// <param name="handled"/>
        public void WRITE_PEG0(PegTarget target, IMaterial m, double qty, ref bool handled)
        {
            try
            {
                WriteHelper.WritePeg(m, target, qty);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="target"/>
        /// <param name="m"/>
        /// <param name="qty"/>
        /// <param name="handled"/>
        public void UPDATE_PEG_INFO0(PegTarget target, IMaterial m, double qty, ref bool handled)
        {
            try
            {
                MicronBEAssyPlanWip wip = m as MicronBEAssyPlanWip;
                wip.PegCount++;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }
    }
}
