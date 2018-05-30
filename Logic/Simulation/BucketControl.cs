using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class BucketControl
    {
        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="bucketer"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Time GET_BUCKET_TIME0(Mozart.SeePlan.Simulation.IHandlingBatch hb, AoBucketer bucketer, ref bool handled, Time prevReturnValue)
        {
            try
            {
                Time tm = new Time();

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                MicronBEAssyBEStep step = hb.CurrentStep as MicronBEAssyBEStep;
                
                foreach (StepTat time in InputMart.Instance.StepTat.DefaultView)
                {
                    if (step.StepID != time.STEP_ID)
                        continue;

                    if (lot.Product.LineID != time.LINE_ID)
                        continue;

                    if (lot.Product.ProductID != time.PRODUCT_ID)
                        continue;

                    double runTatBySec = (double)time.RUN_TAT;
                    tm = TimeSpan.FromSeconds(runTatBySec);

                    break;
                }

                return tm;
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Time);
            }
        }
    }
}
