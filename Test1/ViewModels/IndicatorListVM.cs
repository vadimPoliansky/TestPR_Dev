using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
    public class IndicatorListViewModel
    {
        public Int16 Indicator_ID { get; set; }
        public string Indicator { get; set; }
		public string Format_ID { get; set; }
		public string Format_Str { get; set; }
		public string Threshold_ID { get; set; }
		public string Direction_ID { get; set; }
		public string Color_ID { get; set; }


        public string Area { get; set; }
    }
}