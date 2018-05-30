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
    public static partial class ProductHelper
    {
        public static bool IsBase(this Product product)
        {
            try
            {
                if (product is AssyMcpProduct)
                    return true;

                if (product is AssyMcpPart)
                {
                    AssyMcpPart mcpPart = product as AssyMcpPart;

                    if (mcpPart.IsBase)
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

        public static AssyMcpProduct GetAssyOutProduct(this Product product)
        {
            try
            {
                AssyMcpProduct aoProd = null;
                if (product is AssyMcpPart)
                {
                    AssyMcpPart part = product as AssyMcpPart;
                    aoProd = part.FinalProduct;
                }
                if (product is AssyMcpProduct)
                {
                    AssyMcpProduct prod = product as AssyMcpProduct;
                    aoProd = prod;
                }

                return aoProd;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        public static string DesignID(this Product product)
        {
            string designID = string.Empty;

            if (product is AssyMcpProduct)
                designID = (product as AssyMcpProduct).ProductDetail.DesignID;
            else if (product is AssyMcpPart)
                designID = (product as AssyMcpPart).ProductDetail.DesignID;
            else if (product is MicronBEAssyProduct)
                designID = (product as MicronBEAssyProduct).ProductDetail.DesignID;

            return designID;
        }
    }
}
