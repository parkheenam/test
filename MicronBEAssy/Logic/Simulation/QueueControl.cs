using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.Simulation;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class QueueControl
    {
        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_BUCKET_PROCESSING0(Mozart.SeePlan.Simulation.DispatchingAgent da, IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            try
            {
                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                MicronBEAssyBEStep step = hb.CurrentStep as MicronBEAssyBEStep;

                foreach (EqpArrange eqpArrange in InputMart.Instance.EqpArrange.DefaultView)
                {
                    if (lot.Product.LineID != eqpArrange.LINE_ID)
                        continue;

                    if (step.StepID != eqpArrange.STEP_ID)
                        continue;

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="destCount"/>
        /// <param name="handled"/>
        public void ON_NOT_FOUND_DESTINATION0(DispatchingAgent da, IHandlingBatch hb, int destCount, ref bool handled)
        {

        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool INTERCEPT_IN0(DispatchingAgent da, IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            try
            {
                MicronBEAssyBEStep currentStep = hb.CurrentStep as MicronBEAssyBEStep;

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                if (lot.Product is AssyMcpPart)
                {
                    AssyMcpPart mcpPart = lot.Product as AssyMcpPart;
                    AssyMcpProduct mcpProduct = mcpPart.FinalProduct as AssyMcpProduct;

                    if (mcpPart.PartChangeStep == lot.CurrentStepID)
                    {
                        Tuple<AssyMcpProduct, AssyMcpPart> baseKey = null;
                        Tuple<AssyMcpProduct, AssyMcpPart> sourceKey = null;

                        AssyMcpPart mergeMcpPart = McpHelper.GetMergeMcpPart(mcpPart);
                        if (mcpPart.IsBase)
                        {
                            baseKey = Tuple.Create(mcpProduct, mcpPart);
                            sourceKey = Tuple.Create(mcpProduct, mergeMcpPart);

                            InputMart.Instance.MatchingLotList.Add(baseKey, lot);
                        }
                        else
                        {
                            baseKey = Tuple.Create(mcpProduct, mergeMcpPart);
                            sourceKey = Tuple.Create(mcpProduct, mcpPart);

                            InputMart.Instance.MatchingLotList.Add(sourceKey, lot);
                        }

                        List<MicronBEAssyBELot> matchLotList = SimulationHelper.MatchingMcpLot(da, baseKey, sourceKey);

                        foreach (MicronBEAssyBELot matchLot in matchLotList)
                            da.ReEnter(matchLot);

                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(bool);
            }

        }
    }
}
