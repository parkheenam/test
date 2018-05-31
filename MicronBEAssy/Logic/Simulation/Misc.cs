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
    public partial class Misc
    {
        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool USE_CUSTOM_LOAD0(Mozart.SeePlan.Simulation.AoEquipment aeqp, IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            try
            {
                foreach (var item in InputMart.Instance.StdLotSize.DefaultView)
                {
                    if (item.CHG_TYPE == Constants.ChgType)
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }
    }
}
