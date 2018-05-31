using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.DataModel;
using MicronBEAssy.DataModel;
using MicronBEAssy.Inputs;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class EqpInit
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IEnumerable<Mozart.SeePlan.DataModel.Resource> GET_EQP_LIST0(ref bool handled, IEnumerable<Mozart.SeePlan.DataModel.Resource> prevReturnValue)
        {
            try
            {
                List<Resource> eqpList = new List<Resource>();
                foreach (MicronBEAssyEqp eqp in InputMart.Instance.MicronBEAssyEqp.Values)
                    eqpList.Add(eqp);

                return eqpList;
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IEnumerable<Mozart.SeePlan.DataModel.Resource>);
            }                        
        }
    }
}
