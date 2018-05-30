using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using MicronBEAssy.Inputs;
using Mozart.Task.Execution.Persists;
using MicronBEAssy.DataModel;
using Mozart.SeePlan;
using Mozart.SeePlan.SemiBE.DataModel;
using MicronBEAssy.Outputs;
using MicronBEAssy.Persists;

namespace MicronBEAssy.Logic
{
    [FeatureBind()]
    public partial class PersistInputs
    {
        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_ProcessStep(Mozart.Task.Execution.Persists.IPersistContext context)
        {
            try
            {
                InputMart.Instance.ProcessStep.DefaultView.Sort = "SEQUENCE ASC";

                foreach (ProcessStep processStep in InputMart.Instance.ProcessStep.DefaultView)
                {
                    MicronBEAssyProcess process;

                    Tuple<string, string> key = Tuple.Create(processStep.LINE_ID, processStep.PROCESS_ID);
                    if (InputMart.Instance.MicronBEAssyProcess.TryGetValue(key, out process) == false)
                    {
                        process = NewHelper.NewProcess(processStep.LINE_ID, processStep.PROCESS_ID);

                        InputMart.Instance.MicronBEAssyProcess.Add(key, process);
                    }

                    MicronBEAssyBEStep step = CreateHelper.CreateStep(processStep);

                    if (Constants.DieAttach == step.StepGroup)
                        process.DieAttachSteps.Add(step);

                    if (Constants.DieAttach == step.StepGroup || Constants.WireBond == step.StepGroup)
                    {
                        if(step.StepID.Length > 0)
                            process.BottleneckSteps += step.StepID[0];
                        process.CR2OutStep = step;
                    }

                    step.DaThroughCount = process.DieAttachSteps.Count;
                    process.Steps.Add(step);
                }

                foreach (MicronBEAssyProcess process in InputMart.Instance.MicronBEAssyProcess.Values)
                {
                    process.LinkSteps();

                    if (process.DieAttachSteps.Count > 0 && process.CR2OutStep.StepGroup == Constants.DieAttach && process.CR2OutStep.StepID != "CONTRL SUB ATTACH")
                    {
                        WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PROCESS, process.LineID,
                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 0,
                            string.Empty, "CR2 out step is not WIRE BOND",
                            string.Format("PROCESS ID : {0}, BottleneckSteps : {1}", process.ProcessID, process.BottleneckSteps),
                            "ProcessStep");
                    }
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_ProductMaster(ProductMaster entity)
        {
            try
            {
                entity.LINE_ID = UtilityHelper.Trim(entity.LINE_ID);
                entity.PRODUCT_ID = UtilityHelper.Trim(entity.PRODUCT_ID);
                entity.PRODUCT_NAME = UtilityHelper.Trim(entity.PRODUCT_NAME);
                entity.PROCESS_ID = UtilityHelper.Trim(entity.PROCESS_ID);
                entity.DESIGN_ID = UtilityHelper.Trim(entity.DESIGN_ID);
                entity.MATERIAL_GROUP = UtilityHelper.Trim(entity.MATERIAL_GROUP);
                entity.PKG_FAMILY = UtilityHelper.Trim(entity.PKG_FAMILY);
                entity.PKG_TYPE = UtilityHelper.Trim(entity.PKG_TYPE);

                MicronBEAssyProcess process = FindHelper.FindProcess(entity.LINE_ID, entity.PROCESS_ID);

                if (process != null)
                {
                    Tuple<string, string> key = new Tuple<string, string>(entity.LINE_ID, entity.PRODUCT_ID);

                    if (InputMart.Instance.ProductDetail.ContainsKey(key) == false)
                    {
                        ProductDetail detail = NewHelper.NewProductDetail(entity, process);
                        InputMart.Instance.ProductDetail.Add(key, detail);
                    }
                }
                else
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PROCESS, entity.LINE_ID, string.Empty,
                        string.Empty, entity.PRODUCT_ID, entity.DESIGN_ID, string.Empty, 0, string.Empty,
                        ErrorMessageHelper.CANNOT_FIND_PROCESS_STEP,
                        string.Format("PROCESS_ID : {0}", entity.PROCESS_ID), entity.GetType().Name);
                }

                return false;
            }
            catch(Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        int i = 0; 
        public bool OnAfterLoad_Demand(Demand entity)
        {
            try
            {
                entity.DEMAND_ID = UtilityHelper.Trim(entity.DEMAND_ID);
                entity.LINE_ID = UtilityHelper.Trim(entity.LINE_ID);
                entity.PRODUCT_ID = UtilityHelper.Trim(entity.PRODUCT_ID);
                entity.PRODUCT_NAME = UtilityHelper.Trim(entity.PRODUCT_NAME);
                entity.CUSTOMER = UtilityHelper.Trim(entity.CUSTOMER);
                entity.WEEK_NO = UtilityHelper.Trim(entity.WEEK_NO);
                entity.DEMAND_TYPE = UtilityHelper.Trim(entity.DEMAND_TYPE);
                entity.PRODUCT_TYPE = UtilityHelper.Trim(entity.PRODUCT_TYPE);
                entity.GRADE = UtilityHelper.Trim(entity.GRADE);

                Product demandProd = FindHelper.FindProduct(entity.LINE_ID, entity.PRODUCT_ID);
                if (demandProd == null)
                    return false;

                i++;

                MicronBEAssyBEMoMaster moMaster;
                if (InputMart.Instance.MicronBEAssyBEMoMaster.TryGetValue(entity.PRODUCT_ID, out moMaster) == false)
                {
                    moMaster = new MicronBEAssyBEMoMaster();
                    moMaster.Product = demandProd;
                    InputMart.Instance.MicronBEAssyBEMoMaster.Add(entity.PRODUCT_ID, moMaster);
                }

                MicronBEAssyBEMoPlan moPlan = NewHelper.NewMicronBEAssyMoPlan(moMaster, entity, i);

                moMaster.AddMoPlan(moPlan);

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_Wip(Wip entity)
        {
            try
            {
                entity.LOT_ID = UtilityHelper.Trim(entity.LOT_ID);
                entity.LINE_ID = UtilityHelper.Trim(entity.LINE_ID);
                entity.STEP_ID = UtilityHelper.Trim(entity.STEP_ID);
                entity.PROCESS_ID = UtilityHelper.Trim(entity.PROCESS_ID);
                entity.PRODUCT_ID = UtilityHelper.Trim(entity.PRODUCT_ID);
                entity.PRODUCT_NAME = UtilityHelper.Trim(entity.PRODUCT_NAME);
                entity.HOLD_CODE = UtilityHelper.Trim(entity.HOLD_CODE);
                entity.LOT_STATE = UtilityHelper.Trim(entity.LOT_STATE);
                entity.EQP_ID = UtilityHelper.Trim(entity.EQP_ID);
                entity.ASM_RESERVATION_ID = UtilityHelper.Trim(entity.ASM_RESERVATION_ID);

#if DEBUG
                if (entity.LOT_ID.StartsWith("BF8QK4N.5X"))
                    Console.WriteLine();

                if (entity.PRODUCT_ID == "BF8QK4N.5X")
                    Console.WriteLine();
#endif

                ProductDetail productDetail = FindHelper.FindProductDetail(entity.LINE_ID, entity.PRODUCT_ID);
                Mozart.SeePlan.Simulation.EntityState state = FindHelper.FindLotState(entity.LOT_STATE, entity.HOLD_CODE, entity.EQP_ID);

                if (productDetail == null)
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.WIP, entity.LINE_ID, entity.STEP_ID,
                        entity.LOT_ID, entity.PRODUCT_ID, string.Empty, string.Empty, entity.LOT_QTY, string.Empty, "Cannot find ProductMaster", string.Empty, "Wip");

                    WriteHelper.WriteUnpeg(entity, string.Empty, false, state, UnpegReason.MASTER_DATA, "Cannot find ProductMaster");
                    return false;
                }

                MicronBEAssyProcess process = productDetail.Process;
                
                MicronBEAssyBEStep step = process.FindStep(entity.STEP_ID) as MicronBEAssyBEStep;

                if (step == null)
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.WIP, entity.LINE_ID, entity.STEP_ID,
                        entity.LOT_ID, entity.PRODUCT_ID, productDetail.DesignID, string.Empty, entity.LOT_QTY, string.Empty, "Cannot find ProcessStep", string.Format("PROCESS_ID : {0}", productDetail.Process.ProcessID), "Wip");

                    WriteHelper.WriteUnpeg(entity, productDetail.DesignID, false, state, UnpegReason.MASTER_DATA, "Cannot find ProcessStep");
                    return false;
                }

                string unpegReason = string.Empty;
                Product product = FindHelper.FindWipProduct(entity, process, step, state, out unpegReason);

                if (product == null)
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.WIP, entity.LINE_ID, entity.STEP_ID,
                        entity.LOT_ID, entity.PRODUCT_ID, productDetail.DesignID, string.Empty, entity.LOT_QTY, string.Empty, "Cannot find ProcessStep", string.Format("PROCESS_ID : {0}", productDetail.Process.ProcessID), "Wip");

                    WriteHelper.WriteUnpeg(entity, productDetail.DesignID, false, state, UnpegReason.MASTER_DATA, unpegReason);
                    return false;
                }

                MicronBEAssyWipInfo wipInfo = new MicronBEAssyWipInfo();
                wipInfo.LotID = entity.LOT_ID;
                wipInfo.Product = product;
                wipInfo.Process = process;

                wipInfo.UnitQty = (double)entity.LOT_QTY;
                wipInfo.InitialStep = process.FindStep(entity.STEP_ID);
                wipInfo.CurrentState = state;
                wipInfo.LineID = entity.LINE_ID;

                InputMart.Instance.MicronBEAssyWipInfo.Add(wipInfo.InitialStep.StepID, wipInfo);

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_McpBom(McpBom entity)
        {
            try
            {
                entity.LINE_ID = UtilityHelper.Trim(entity.LINE_ID);
                entity.STEP_ID = UtilityHelper.Trim(entity.STEP_ID);
                entity.FROM_PROD_ID = UtilityHelper.Trim(entity.FROM_PROD_ID);
                entity.FROM_PROD_NAME = UtilityHelper.Trim(entity.FROM_PROD_NAME);
                entity.TO_PROD_ID = UtilityHelper.Trim(entity.TO_PROD_ID);
                entity.TO_PROD_NAME = UtilityHelper.Trim(entity.TO_PROD_NAME);
                entity.FINAL_PROD_ID = UtilityHelper.Trim(entity.FINAL_PROD_ID);
                entity.FINAL_PROD_NAME = UtilityHelper.Trim(entity.FINAL_PROD_NAME);
                entity.ASSY_IN_PROD_ID = UtilityHelper.Trim(entity.ASSY_IN_PROD_ID);
                entity.ASSY_IN_PROD_NAME = UtilityHelper.Trim(entity.ASSY_IN_PROD_NAME);

#if DEBUG
                if (entity.ASSY_IN_PROD_ID == "357937")
                    Console.WriteLine();

                if (entity.FINAL_PROD_ID == "357937")
                    Console.WriteLine();
#endif

                AssyMcpProduct finalProduct = null;
                Tuple<string, string, bool, bool, int> key = new Tuple<string, string, bool, bool, int>(entity.LINE_ID, entity.FINAL_PROD_ID, false, false, 1);

                if (InputMart.Instance.MicronBEProducts.TryGetValue(key, out finalProduct) == false)
                {
                    ProductDetail productDetail = FindHelper.FindProductDetail(entity.LINE_ID, entity.FINAL_PROD_ID);

                    if (productDetail == null)
                    {
                        WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PRODUCT, entity.LINE_ID, string.Empty,
                            string.Empty, entity.FINAL_PROD_ID, string.Empty, string.Empty, 0, string.Empty, "Cannot find ProductMaster", string.Empty, "McpBom");
                        return false;
                    }

                    finalProduct = NewHelper.NewAssyMcpProduct(entity, productDetail);

                    InputMart.Instance.MicronBEProducts.Add(key, finalProduct);
                }

                ProductDetail inProductDetail = FindHelper.FindProductDetail(entity.LINE_ID, entity.ASSY_IN_PROD_ID);

                if (inProductDetail == null)
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PRODUCT, entity.LINE_ID, string.Empty,
                        string.Empty, entity.ASSY_IN_PROD_ID, string.Empty, string.Empty, 0, string.Empty, "Cannot find ProductMaster", string.Empty, "McpBom");
                    return false;
                }

                Tuple<string, string, bool, bool, int> inProdKey = new Tuple<string, string, bool, bool, int>(entity.LINE_ID, entity.ASSY_IN_PROD_ID, true, false, entity.COMP_SEQ);

                AssyMcpPart inMcpPart;
                if (InputMart.Instance.MicronBEProducts.TryGetValue(inProdKey, out inMcpPart) == false)
                {
                    inMcpPart = NewHelper.NewAssyMcpPart(entity.LINE_ID, entity.ASSY_IN_PROD_ID, finalProduct, false, entity.COMP_SEQ, entity.COMP_QTY, inProductDetail);
                    inMcpPart.PartChangeStep = entity.STEP_ID;
                    InputMart.Instance.MicronBEProducts.Add(inProdKey, inMcpPart);
                }

                if (finalProduct.AllParts.Contains(inMcpPart) == false)
                    finalProduct.AllParts.Add(inMcpPart);

                if (finalProduct.AssyParts.Contains(inMcpPart) == false)
                    finalProduct.AssyParts.Add(inMcpPart);

                finalProduct.MaxSequence = finalProduct.AssyParts.Count;

                return true;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="context"/>
        public void OnAction_McpBom(IPersistContext context)
        {
            try
            {
                HashSet<AssyMcpProduct> mcpProductList = new HashSet<AssyMcpProduct>();

                InputMart.Instance.McpBom.DefaultView.Sort = "COMP_SEQ";
                foreach (McpBom bom in InputMart.Instance.McpBom.DefaultView)
                {
#if DEBUG
                    if (bom.FINAL_PROD_ID == "357937")
                        Console.WriteLine();
#endif
                    Tuple<string, string, bool, bool, int> key = new Tuple<string, string, bool, bool, int>(bom.LINE_ID, bom.FINAL_PROD_ID, false, false, 1);

                    AssyMcpProduct mcpProduct = FindHelper.FindProduct(bom.LINE_ID, bom.FINAL_PROD_ID) as AssyMcpProduct;

                    if (mcpProduct == null)
                        continue;

                    mcpProductList.Add(mcpProduct);

                    AssyMcpPart inMcpPart = FindHelper.FindProduct(bom.LINE_ID, bom.ASSY_IN_PROD_ID, true, false, bom.COMP_SEQ) as AssyMcpPart;

                    if (inMcpPart == null)
                        continue;

                    AssyMcpPart fromMcpPart = null;

                    if (bom.FROM_PROD_ID == bom.ASSY_IN_PROD_ID && bom.COMP_SEQ == 1)
                    {
                        fromMcpPart = null;
                    }
                    else
                    {
                        fromMcpPart = FindHelper.FindProduct(bom.LINE_ID, bom.FROM_PROD_ID, true, true, bom.COMP_SEQ - 1) as AssyMcpPart;
                        
                        if (fromMcpPart == null)
                            continue;

                        fromMcpPart.PartChangeStep = bom.STEP_ID;
                    }

#if DEBUG
                    try
                    {
                        if (mcpProduct.MaxSequence > bom.COMP_QTY)
                            Console.WriteLine();
                    }
                    catch
                    {
                        Console.WriteLine();
                    }
#endif

                    if (mcpProduct.MaxSequence > bom.COMP_SEQ)
                    {
                        AssyMcpPart toMcpPart = FindHelper.FindProduct(bom.LINE_ID, bom.TO_PROD_ID, true, true, bom.COMP_SEQ) as AssyMcpPart;

                        if (toMcpPart == null)
                        {
                            ProductDetail midProductDetail = FindHelper.FindProductDetail(bom.LINE_ID, bom.TO_PROD_ID);

                            if (midProductDetail == null)
                                continue;

                            Tuple<string, string, bool, bool, int> midKey = new Tuple<string, string, bool, bool, int>(bom.LINE_ID, bom.TO_PROD_ID, true, true, bom.COMP_SEQ);

                            if (InputMart.Instance.MicronBEProducts.ContainsKey(midKey) == false)
                            {
                                toMcpPart = NewHelper.NewAssyMcpPart(bom.LINE_ID, bom.TO_PROD_ID, mcpProduct, true, bom.COMP_SEQ, 1, midProductDetail);
                                InputMart.Instance.MicronBEProducts.Add(midKey, toMcpPart);

                                toMcpPart.AddPrev(inMcpPart);
                                
                                inMcpPart.AddNext(toMcpPart);
                                
                                if (fromMcpPart != null)
                                {
                                    fromMcpPart.AddNext(toMcpPart);
                                    toMcpPart.AddPrev(fromMcpPart);
                                }

                                mcpProduct.AllParts.Add(toMcpPart);
                            }
                        }
                    }
                    else
                    {
                        if (fromMcpPart != null)
                            mcpProduct.AddPrev(fromMcpPart);

                        mcpProduct.AddPrev(inMcpPart);
                    }
                }

                foreach (AssyMcpProduct mcpProduct in mcpProductList)
                {
                    foreach (AssyMcpPart mcpPart in mcpProduct.AllParts)
                    {
                        if (mcpPart.IsMidPart || mcpPart.CompSeq == 1)
                            mcpPart.IsBase = true;
                    }
                }
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_ActInfo(ActInfo entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.PRODUCT_ID))
                    return false;

                MicronBEAssyWipInfo wipInfo = new MicronBEAssyWipInfo();
                wipInfo.LineID = entity.LINE_ID;
                wipInfo.WipProductID = entity.PRODUCT_ID;
                wipInfo.UnitQty = (double)entity.ACT_QTY;
                wipInfo.LotID = "ACT";

                MicronBEAssyPlanWip actWip = new MicronBEAssyPlanWip(wipInfo);
                actWip.LotID = wipInfo.LotID;

                IComparable key = actWip.GetWipInfo().WipProductID;
                InputMart.Instance.MicronBEAssyActPlanWips.Add(key, actWip);

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_Equipment(Equipment entity)
        {
            try
            {
                MicronBEAssyEqp eqp = new MicronBEAssyEqp();
                eqp.Init(entity.EQP_ID, FindHelper.GetEngineStartTime(), FindHelper.GetEngineStartTime(), entity.SIM_TYPE);
                eqp.LineID = entity.LINE_ID;
                eqp.EqpID = entity.EQP_ID;
                string stepGroup = string.Empty;
                //eqp.StepGroup = UtilityHelper.StringToEnum(entity.STEP_GROUP, StepGroup.NONE);
                eqp.EqpModel = entity.EQP_MODEL;
                eqp.UtilRate = (double)entity.UTIL_RATE;
                eqp.DispatchingRule = entity.DISPATCHING_TYPE;

                if (InputMart.Instance.MicronBEAssyEqp.ContainsKey(eqp.EqpID) == false)
                    InputMart.Instance.MicronBEAssyEqp.Add(eqp.EqpID, eqp);

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }

        
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_StepTat(StepTat entity)
        {
            entity.RUN_TAT = (decimal)UtilityHelper.GetTimeBySeconds((double)entity.RUN_TAT, entity.TIME_UOM);
            entity.WAIT_TAT = (decimal)UtilityHelper.GetTimeBySeconds((double)entity.WAIT_TAT, entity.TIME_UOM);
            entity.TAT = (decimal)UtilityHelper.GetTimeBySeconds((double)entity.TAT, entity.TIME_UOM);

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_StepTime(StepTime entity)
        {
            entity.TACT_TIME = (decimal)UtilityHelper.GetTimeBySeconds((double)entity.TACT_TIME, entity.TIME_UOM);
            entity.FLOW_TIME = (decimal)UtilityHelper.GetTimeBySeconds((double)entity.FLOW_TIME, entity.TIME_UOM);

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_SetupInfo(SetupInfo entity)
        {
            entity.SETUP_TIME = (decimal)UtilityHelper.GetTimeBySeconds((double)entity.SETUP_TIME, entity.TIME_UOM);

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_ProductRoute(ProductRoute entity)
        {
            try
            {
                entity.LINE_ID = UtilityHelper.Trim(entity.LINE_ID);
                entity.FROM_PROD_ID = UtilityHelper.Trim(entity.FROM_PROD_ID);
                entity.FROM_PROD_NAME = UtilityHelper.Trim(entity.FROM_PROD_NAME);
                entity.STEP_ID = UtilityHelper.Trim(entity.STEP_ID);
                entity.TO_PROD_ID = UtilityHelper.Trim(entity.TO_PROD_ID);
                entity.TO_PROD_NAME = UtilityHelper.Trim(entity.TO_PROD_NAME);

                ProductDetail fromProdDetail = FindHelper.FindProductDetail(entity.LINE_ID, entity.FROM_PROD_ID);

                if (fromProdDetail == null)
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PRODUCT, entity.LINE_ID, string.Empty,
                        string.Empty, entity.FROM_PROD_ID, string.Empty, string.Empty, 0, string.Empty, "Cannot find ProductMaster", string.Empty, "ProductRoute");
                    return false;
                }

                Tuple<string, string, bool, bool, int> fromProdKey = new Tuple<string, string, bool, bool, int>(entity.LINE_ID, entity.FROM_PROD_ID, false, false, 1);

                MicronBEAssyProduct fromProduct;
                if (InputMart.Instance.MicronBEProducts.TryGetValue(fromProdKey, out fromProduct) == false)
                {
                    fromProduct = NewHelper.NewMicronBeAssyProduct(fromProdDetail);
                    fromProduct.PartChangeStep = entity.STEP_ID;
                    InputMart.Instance.MicronBEProducts.Add(fromProdKey, fromProduct);
                }

                ProductDetail toProdDetail = FindHelper.FindProductDetail(entity.LINE_ID, entity.TO_PROD_ID);

                if (toProdDetail == null)
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PRODUCT, entity.LINE_ID, string.Empty,
                        string.Empty, entity.TO_PROD_ID, string.Empty, string.Empty, 0, string.Empty, "Cannot find ProductMaster", string.Empty, "ProductRoute");
                    return false;
                }

                ICollection<AssyMcpPart> assyInProducts = FindHelper.FindAssyInParts(entity.LINE_ID, entity.TO_PROD_ID);

                if (assyInProducts.IsNullOrEmpty())
                {
                    WriteHelper.WriteMasterDataErrorLog(MasterDataErrorEventType.PRODUCT, entity.LINE_ID, string.Empty,
                        string.Empty, entity.TO_PROD_ID, string.Empty, string.Empty, 0, string.Empty, "Cannot find McpBom", string.Empty, "ProductRoute");
                    return false;
                }

                foreach (AssyMcpPart inPart in assyInProducts)
                {
                    inPart.AddPrev(fromProduct);
                    fromProduct.AddNext(inPart);
                }

                return false;
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"/>
        /// <returns/>
        public bool OnAfterLoad_ProcessStep(ProcessStep entity)
        {
            try
            {
                entity.LINE_ID = UtilityHelper.Trim(entity.LINE_ID);
                entity.PROCESS_ID = UtilityHelper.Trim(entity.PROCESS_ID);
                entity.STEP_ID = UtilityHelper.Trim(entity.STEP_ID);
            }
            catch (Exception e)
            {
                WriteHelper.WriteErrorHistory(ErrorLevel.FATAL, string.Format("ErrorMessage : {0}   MethodName : {1}", e.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name));
                return false;
            }

            return true;
        }
    }
}
