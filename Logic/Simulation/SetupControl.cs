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
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.SemiBE.DataModel;
using Mozart.Text;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic.Simulation
{
    [FeatureBind()]
    public partial class SetupControl
    {
        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Time GET_SETUP_TIME0(Mozart.SeePlan.Simulation.AoEquipment aeqp, IHandlingBatch hb, ref bool handled, Time prevReturnValue)
        {
            try
            {
                MicronBEAssyEqp eqp = null;
                if (InputMart.Instance.MicronBEAssyEqp.ContainsKey(aeqp.EqpID))
                    eqp = InputMart.Instance.MicronBEAssyEqp[aeqp.EqpID];
          
                if(eqp == null)
                    return default(Time);

                MicronBEAssyBELot lot = hb as MicronBEAssyBELot;

                BEStep step = lot.CurrentStep;
                StepMaster sm = InputMart.Instance.StepMasterView.FindRows(step.StepID).FirstOrDefault();
                
                foreach (var info in InputMart.Instance.SetupInfo.DefaultView)
                {
                    if (info.LINE_ID != sm.LINE_ID)
                        continue;

                    //if (UtilityHelper.StringToEnum(info.STEP_GROUP, StepGroup.NONE) != eqp.StepGroup)
                    //    continue;

                    //if (LikeUtility.Like(eqp.EqpModel, info.EQP_MODEL) == false)
                    //    continue;

                    double setupTimeBySec = (double)info.SETUP_TIME;
                    return Time.FromSeconds(setupTimeBySec);
                }

                return default(Time);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(Time);
            }            
        }
    }
}
