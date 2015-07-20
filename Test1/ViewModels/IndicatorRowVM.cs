using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
	public class IndicatorRowViewModel
	{
		public Int16 indicatorID { get; set; }
		public string updateProperty { get; set; }
		public string updateValue { get; set; }
		public string updateValueSup { get; set; }
		public Int16 fiscalYear { get; set; }
	}
}