using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicronBEAssyUserInterface.Gantt
{
    public class Constants
    {
        public const string ALL = "ALL";
        public const string ANY_STR = "%";
        public const string ANY_CHAR = "_";

        public const string GANTT_CELL_HEIGHT = "ganttCellHeight";
        public const string GANTT_CELL_WIDTH = "ganttCellWidth";
        public const string GANTT_DURATION = "ganttDuration";
        public const string GANTT_STAGE_IDX = "ganttStageID";
        public const string GANTT_SITE_IDX = "ganttSiteID";
        public const string GANTT_OPER_IDX = "ganttOperID";

        public const string TABLE_EQP_PLAN = "EqpPlan";
        public const string TABLE_EQUIPMENT = "Equipment";

        public const string COLUMN_LINE_ID = "LINE_ID";
        public const string COLUMN_EQUIPMENT_ID = "EQUIPMENT_ID";
        public const string COLUMN_STEP_GROUP = "STEP_GROUP";
        public const string COLUMN_QTY = "QTY";
        public const string COLUMN_TOTAL = "TOTAL";

        public const string COLUMN_PM = "PM";
        public const string COLUMN_SETUP = "SETUP";
        public const string COLUMN_LOAD = "LOAD";
        public const string COLUMN_CHANGE = "CHANGE";

        public const string COLUMN_TARGET_QTY = "TARGET_QTY";
        public const string COLUMN_PLAN_QTY = "PLAN_QTY";

        public const string COLUMN_CATEGORY = "CATEGORY";
        public const string COLUMN_PRODUCT_ID = "PRODUCT_ID";

        public const string COLUMN_DATE = "DATE";

        public const string COLUMN_RATIO = "RATIO";

        public const string STR_PM = "PM";
        public const string STR_BUSY = "BUSY";
        public const string STR_SETUP = "SETUP";
        public const string STR_IN = "IN";
        public const string STR_OUT = "OUT";

        public const string STR_RATIO = "RATIO";
        public const string STR_RATIO_GAP = "RATIO or DUE_GAP";
        public const string STR_REASON = "REASON";

        public const string STR_PLAN_SCENARIO = "PlanScenario";
        public const string STR_PLANNING = "Planning";

        public const string STR_DUMMY = "DUMMY";

        public const int DEFAULT_PERIOD = 120;
        public const int DEFAULT_HEIGHT = 20;
        public const int DEFAULT_WIDTH = 20;

        public const string DIR_MOZART = "Mozart";
        public const string FILE_NAME_COLOR_MAP = "color_map.dat";
    }
}
