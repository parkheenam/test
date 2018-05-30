using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.Engine;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class FilterControl
    {
        /// <summary>
        /// </summary>
        /// <param name="eqp"/>
        /// <param name="wips"/>
        /// <param name="ctx"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<Mozart.SeePlan.Simulation.IHandlingBatch> DO_FILTER1(Mozart.SeePlan.Simulation.AoEquipment eqp, IList<IHandlingBatch> wips, IDispatchContext ctx, ref bool handled, IList<IHandlingBatch> prevReturnValue)
        {
#if DEBUG
            if(eqp.EqpID == "DA03")
                Console.WriteLine();
#endif

            return wips;
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="wips"/>
        /// <param name="waitDownTime"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_PREVENT_DISPATCHING0(AoEquipment aeqp, IList<IHandlingBatch> wips, Mozart.Simulation.Engine.Time waitDownTime, ref bool handled, bool prevReturnValue)
        {
            return false;
        }
    }
}
