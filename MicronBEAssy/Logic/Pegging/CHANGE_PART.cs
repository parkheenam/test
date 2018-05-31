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
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Pegging
{
    [FeatureBind()]
    public partial class CHANGE_PART
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public List<object> GET_PART_CHANGE_INFOS0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isRun, ref bool handled, List<object> prevReturnValue)
        {
            try
            {
                MicronBEAssyBEStep currentStep = pegPart.CurrentStep as MicronBEAssyBEStep;
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;
                List<object> list = new List<object>();

                if (isRun)
                {
                    if (pp.Product is AssyMcpProduct)
                    {
                        AssyMcpProduct mcpProduct = pp.Product as AssyMcpProduct;

                        string stepID = string.Empty;
                        if(mcpProduct.HasPrevs && mcpProduct.Prevs.ElementAt(0) is AssyMcpPart)
                            stepID = (mcpProduct.Prevs.ElementAt(0) as AssyMcpPart).PartChangeStep;

                        if (stepID == currentStep.StepID)
                            list.AddRange(mcpProduct.Prevs);
                    }
                    else if (pp.Product is AssyMcpPart)
                    {
#if DEBUG
                        if (currentStep.StepID == "DIE ATTACH")
                            Console.WriteLine();
#endif
                        AssyMcpPart mcpPart = pp.Product as AssyMcpPart;

                        if (mcpPart.IsMidPart)
                        {
                            string stepID = string.Empty;
                            if (mcpPart.HasPrevs && mcpPart.Prevs.ElementAt(0) is AssyMcpPart)
                                stepID = (mcpPart.Prevs.ElementAt(0) as AssyMcpPart).PartChangeStep;

                            if (stepID == currentStep.StepID)
                                list.AddRange(mcpPart.Prevs);
                        }
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return new List<object>();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="partChangeInfo"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart APPLY_PART_CHANGE_INFO0(PegPart pegPart, object partChangeInfo, bool isRun, ref bool handled, PegPart prevReturnValue)
        {
            try
            {
                MicronBEAssyBEPegPart pp = pegPart as MicronBEAssyBEPegPart;

                pp.Product = partChangeInfo as Product;

#if DEBUG
                if(pp.Product.ProductID == "328622")
                    Console.WriteLine();
#endif

                AssyMcpPart mcpPart = pp.Product as AssyMcpPart;

                BEStep step = null;

                if (mcpPart.FinalProduct == null)
                    return pegPart;

                if(mcpPart.FinalProduct.MaxSequence == 1 || mcpPart.IsMidPart)
                {
                    step = pp.Product.Process.FindStep(pp.CurrentStep.StepID);
                }
                else
                {
                    step = pp.Product.Process.LastStep;
                }

                pp.CurrentStep = step;

                if (mcpPart.IsMidPart == false)
                {
                    foreach (PegTarget target in pp.PegTargetList)
                    {
                        target.Qty = target.Qty * mcpPart.CompQty;
                    }
                }

                return pp;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return pegPart;
            }
        }
    }
}
