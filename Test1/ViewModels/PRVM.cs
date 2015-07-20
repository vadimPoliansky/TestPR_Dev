using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
    public class PRViewModel
    {
        public List<Analysts> allAnalysts { get; set; }
        public List<CoEs> allCoEs { get; set; }
        public List<Indicator_CoE_Maps> allMaps { get; set; }
        public List<Indicator_Footnote_Maps> allFootnoteMaps { get; set; }
        public List<Color_Types> allColors { get; set; }
        public List<Color_Directions> allDirections { get; set; }
        public List<Color_Thresholds> allThresholds { get; set; }
        public List<Footnotes> allFootnotes { get; set; }
        public List<Areas> allAreas { get; set; }
		public List<Formats> allFormats { get; set; }
		public List<Indicators> allIndicators { get; set; }

		public Boolean isDraft { get; set; }

        public Int16 Fiscal_Year { get; set; }
        public Int16? Analyst_ID { get; set; }

		public Int16? coeID2 { get; set; }

    }
}