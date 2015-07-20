using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
    public class InventoryViewModel
    {
        public Int16 Indicator_ID { get; set; }
		public string Identifier { get; set; }
		public string Area { get; set; }
		public string Footnote { get; set; }
        public Int16 Area_ID { get; set; }
        public string CoE {get; set; }
        public string Indicator { get; set; }
		public string Indicator_Type { get; set; }
        public string FY_3 { get; set; }
        public string FY_3_Sup { get; set; }
        public string FY_2 { get; set; }
        public string FY_2_Sup { get; set; }
        public string FY_1 { get; set; }
        public string FY_1_Sup { get; set; }
        public string FY_Q1 { get; set; }
        public string FY_Q1_Sup { get; set; }
        public string FY_Q2 { get; set; }
        public string FY_Q2_Sup { get; set; }
        public string FY_Q3 { get; set; }
        public string FY_Q3_Sup { get; set; }
        public string FY_Q4 { get; set; }
        public string FY_Q4_Sup { get; set; }
        public string FY_YTD { get; set; }
        public string FY_YTD_Sup { get; set; }
        public string FY_Target { get; set; }
        public string FY_Target_Sup { get; set; }
        public string FY_Comparator { get; set; }
        public string FY_Comparator_Sup { get; set; }
		public string FY_Comparator_Q1 { get; set; }
		public string FY_Comparator_Q2 { get; set; }
		public string FY_Comparator_Q3 { get; set; }
		public string FY_Comparator_Q4 { get; set; }
        public string FY_Performance_Threshold { get; set; }
        public string FY_Performance_Threshold_Sup { get; set; }

        public Int16 FY_Color_ID { get; set; }
		public Int16 FY_Direction_ID { get; set; }
		public Int16 FY_Threshold_ID { get; set; }
        public string FY_YTD_Custom_Color { get; set; }
        public string FY_Q1_Custom_Color { get; set; }
        public string FY_Q2_Custom_Color { get; set; }
        public string FY_Q3_Custom_Color { get; set; }
        public string FY_Q4_Custom_Color { get; set; }

        public string FY_Definition_Calculation { get; set; }
        public string FY_Target_Rationale { get; set; }
        public string FY_Comparator_Source { get; set; }

        public string FY_Data_Source_MSH { get; set; }
        public string FY_Data_Source_Benchmark { get; set; }
        public string FY_OPEO_Lead { get; set; }

        public string FY_Q1_Color { get; set; }
        public string FY_Q2_Color { get; set; }
        public string FY_Q3_Color { get; set; }
        public string FY_Q4_Color { get; set; }
        public string FY_YTD_Color { get; set; }

        public int Fiscal_Year { get; set; }
		public int Analyst_ID { get; set; }
		public int Link_ID { get; set; }

        public List<Analysts> allAnalysts { get; set; }
		public List<Color_Directions> allDirections { get; set; }
		public List<Color_Thresholds> allThresholds { get; set; }
		public List<Color_Types> allColors { get; set; }

        public string Format_Code { get; set; }

		public bool N_Value { get; set; }
		public string N_Value_ID { get; set; }

		public string FY_Comment { get; set; }
    }
}