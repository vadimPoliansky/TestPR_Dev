using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Models.ViewModels
{
	public class Link_CoEViewModel
	{
		public bool Linked_CoE { get; set; }
		public Int16 CoE_ID { get; set; }

		public Int16 CoE_ID_1 { get; set; }
		public Int16 CoE_ID_2 { get; set; }

		public string CoE { get; set; }
		public string CoE_Title { get; set; }
	}
}