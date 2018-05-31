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

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class InputBatchInit
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<Mozart.SeePlan.Simulation.ILot> INSTANCING0(ref bool handled, IEnumerable<Mozart.SeePlan.Simulation.ILot> prevReturnValue)
        {
            return default(IEnumerable<Mozart.SeePlan.Simulation.ILot>);
        }
    }
}
