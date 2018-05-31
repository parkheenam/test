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
namespace MicronBEAssy
{
    [FeatureBind()]
    public static partial class NewHelper
    {
        internal static MicronBEAssyProduct NewMicronBeAssyProduct(ProductDetail prodDetail)
        {
            try
            {
                MicronBEAssyProduct product = new MicronBEAssyProduct();
                product.ProductID = prodDetail.ProductID;
                product.Process = prodDetail.Process;
                product.IsMcpPart = false;
                product.IsMidPart = false;
                product.CompSeq = 1;

                product.LineID = prodDetail.LineID;
                product.ProductDetail = prodDetail;
                product.Key = new Tuple<string, string, bool, bool, int>(product.LineID, product.ProductID, product.IsMcpPart, product.IsMidPart, product.CompSeq);

                return product;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        internal static AssyMcpProduct NewAssyMcpProduct(McpBom entity, ProductDetail prodDetail)
        {
            try
            {
                AssyMcpProduct mcpProduct = new AssyMcpProduct();

                mcpProduct.LineID = entity.LINE_ID;
                mcpProduct.ProductID = entity.FINAL_PROD_ID;
                mcpProduct.Process = prodDetail.Process;
                mcpProduct.ProductDetail = prodDetail;

                mcpProduct.AllParts = new List<AssyMcpPart>();
                mcpProduct.AssyParts = new List<AssyMcpPart>();

                return mcpProduct;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        internal static ProductDetail NewProductDetail(ProductMaster entity, MicronBEAssyProcess process)
        {
            try
            {
                ProductDetail productDetail = new ProductDetail();

                productDetail.LineID = entity.LINE_ID;
                productDetail.ProductID = entity.PRODUCT_ID;
                productDetail.ProductName = entity.PRODUCT_NAME;
                productDetail.Process = process;
                productDetail.DesignID = entity.DESIGN_ID;
                productDetail.MaterialGroup = entity.MATERIAL_GROUP;
                productDetail.PkgFamily = entity.PKG_FAMILY;
                productDetail.PkgType = entity.PKG_TYPE;
                productDetail.PkgLeadCount = entity.PKG_LEAD_COUNT;
                productDetail.PkgWidth = entity.PKG_WIDTH;
                productDetail.PkgLength = entity.PKG_LENGTH;
                productDetail.PkgHeight = entity.PKG_HEIGHT;
                productDetail.NetDie = entity.NET_DIE;

                return productDetail;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        internal static AssyMcpPart NewAssyMcpPart(string lineID, string productID, AssyMcpProduct finalProduct, bool isMidPart, int compSeq, int compQty, ProductDetail productDetail)
        {
            try
            {
                AssyMcpPart mcpPart = new AssyMcpPart();

                mcpPart.Process = productDetail.Process;

                mcpPart.LineID = lineID;
                mcpPart.ProductID = productID;
                mcpPart.FinalProduct = finalProduct;
                mcpPart.IsMidPart = isMidPart;
                mcpPart.CompSeq = compSeq;
                mcpPart.CompQty = compQty;

                if (mcpPart.CompQty <= 0)
                    mcpPart.CompQty = 1;

                mcpPart.ProductDetail = productDetail;

                return mcpPart;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        internal static MicronBEAssyBEMoPlan NewMicronBEAssyMoPlan(MicronBEAssyBEMoMaster moMaster, Demand entity, int i)
        {
            try
            {
                MicronBEAssyBEMoPlan moPlan = new MicronBEAssyBEMoPlan();

                moPlan.MoMaster = moMaster;
                moPlan.Qty = (double)entity.QTY;
                moPlan.DueDate = entity.DUE_DATE;
                moPlan.DemandID = entity.DEMAND_ID;
                moPlan.WeekNo = entity.WEEK_NO;
                moPlan.Priority = i.ToString();

                return moPlan;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }

        internal static MicronBEAssyProcess NewProcess(string lineID, string processID)
        {
            try
            {
                MicronBEAssyProcess process = new MicronBEAssyProcess(processID);
                process.LineID = lineID;
                process.DieAttachSteps = new List<MicronBEAssyBEStep>();

                return process;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return null;
            }
        }
    }
}
