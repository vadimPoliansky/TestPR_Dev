using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
	public class Edit_Indicator_LinksViewModel
	{
		public List<Indicators> allIndicators { get; set; }
		public List<Indicator_Links> allIndicator_Links { get; set; }
		public List<Link_Fields> allFields { get; set; }

		public Int16 Fiscal_Year { get; set; }
	}
}