using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.DataModel;
using Mozart.SeePlan.SemiBE.DataModel;
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class McpHelper
    {
        public static AssyMcpPart GetMcpPart(this AssyMcpProduct mcpProduct, string mcpCode)
        {
            try
            {
                foreach (AssyMcpPart mcpPart in mcpProduct.AllParts)
                {
                    if (mcpPart.ProductID == mcpCode)
                        return mcpPart;
                }

                return null;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        public static AssyMcpPart GetMergeMcpPart(AssyMcpPart mcpPart)
        {
            try
            {
                AssyMcpPart mergeMcpPart = null;

                if (mcpPart.HasNexts)
                {
                    foreach (AssyMcpPart prev in mcpPart.Nexts.ElementAt(0).Prevs)
                    {
                        if (prev != mcpPart)
                            mergeMcpPart = prev;
                    }
                }
                else
                {
                    foreach (AssyMcpPart prev in mcpPart.FinalProduct.Prevs)
                    {
                        if (prev != mcpPart)
                            mergeMcpPart = prev;
                    }
                }

                return mergeMcpPart;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return default(AssyMcpPart);
            }
        }

        public static bool IsMcp(this Product product)
        {
            try
            {
                if (product is AssyMcpPart || product is AssyMcpProduct)
                    return true;

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
