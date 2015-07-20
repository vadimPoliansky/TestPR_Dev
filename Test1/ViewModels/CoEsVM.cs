using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
    public class CoEsViewModel
    {
        public Int16 CoE_ID { get; set; }
        public string CoE { get; set; }
        public string CoE_Abbr { get; set; }
        public string CoE_Type { get; set; }
        public string CoE_Notes { get; set; }
        public string CoE_Subtitle { get; set; }
		public Int16 Fiscal_Year { get; set; }
		public string CoE_Title { get; set; }
    }
}