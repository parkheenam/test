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
    public partial class WipInit
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<Mozart.SeePlan.Simulation.IHandlingBatch> GET_WIPS0(ref bool handled, IList<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
        {
            try
            {
                List<IHandlingBatch> wipList = new List<IHandlingBatch>();

                foreach (MicronBEAssyPlanWip planWip in InputMart.Instance.MicronBEAssyPlanWip.Values)
                {
                    if (planWip.PegCount > 0)
                    {
                        MicronBEAssyBELot lot = new MicronBEAssyBELot();
                        lot.Init(planWip.LotID, planWip.Product, planWip.GetWipInfo().LineID);
                        lot.LineID = planWip.GetWipInfo().LineID;
                        lot.Route = lot.Product.Process;
                        lot.WipInfo = planWip.GetWipInfo();
                        lot.AssyBatch = CreateHelper.CreateBatch(lot);

                        if (lot.AssyBatch == null)
                            continue;

                        lot.UnitQtyDouble = planWip.GetWipInfo().UnitQty;
                        wipList.Add(lot);
                    }
                    
                }

                return wipList;
            }

            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(IList<Mozart.SeePlan.Simulation.IHandlingBatch>);
            }            
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public string GET_LOADING_EQUIPMENT0(IHandlingBatch hb, ref bool handled, string prevReturnValue)
        {
            try
            {
                return null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(string);
            }          
        }
    }
}
