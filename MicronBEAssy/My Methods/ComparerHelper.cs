using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.DataModel;
using Mozart.SeePlan.Simulation;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class ComparerHelper
    {
        public class LotCompare : IComparer<object>
        {
            public int Compare(object x, object y)
            {
                try
                {
                    MicronBEAssyBELot lotx = null;
                    MicronBEAssyBELot loty = null;

                    int cmp = 0;

                    if (x is WorkLot && y is WorkLot)
                    {
                        WorkLot workLotx = x as WorkLot;
                        WorkLot workLoty = y as WorkLot;

                        if (cmp == 0)
                            cmp = workLotx.AvailableTime.CompareTo(workLoty.AvailableTime);

                        lotx = workLotx.Lot as MicronBEAssyBELot;
                        loty = workLoty.Lot as MicronBEAssyBELot;
                    }

                    if (x is MicronBEAssyBELot && y is MicronBEAssyBELot)
                    {
                        lotx = x as MicronBEAssyBELot;
                        loty = y as MicronBEAssyBELot;
                    }

                    if (cmp == 0)
                        cmp = lotx.LotID.CompareTo(loty.LotID);

                    return cmp;

                }
                catch (Exception e)
                {
                    WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                    return 0;
                }
            }

            public LotCompare()
            {
            }
        }
    }
}
