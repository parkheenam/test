using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class UtilityHelper
    {
        public static double GetTimeBySeconds(double time, string timeUOM)
        {
            try
            {
                TimeUnit timeUom = UtilityHelper.StringToEnum(timeUOM, TimeUnit.SEC);

                if (timeUom == TimeUnit.HOUR)
                    time *= Math.Pow(60, 2);

                if (timeUom == TimeUnit.MIN)
                    time *= Math.Pow(60, 1);

                return time;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return 0d;
            }
        }

        public static string IsYN(bool isTrue)
        {
            if (isTrue)
                return Constants.Y;

            return Constants.N;
        }

        public static T StringToEnum<T>(this string src, T defValue)
        {
            try
            {
                if (src.IsNullOrEmpty())
                    return defValue;

                if (Enum.IsDefined(typeof(T), src))
                    return (T)Enum.Parse(typeof(T), src, true);

                return defValue;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(T);
            }
        }

        internal static string Trim(string str)
        {
            if (str.IsNullOrEmpty())
                return string.Empty;

            return str.Trim();
        }
    }
}
