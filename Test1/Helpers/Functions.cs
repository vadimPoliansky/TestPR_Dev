using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IndInv.Models;

namespace IndInv.Helpers
{
	public class Functions
	{
		public static string getFootnoteString(List<Indicator_Footnote_Maps> footnoteMaps)
		{
			var footnoteString = "";
			var i = 0;
			if (footnoteMaps != null)
			{
				foreach (var footnote in footnoteMaps)
				{
					if (i > 0)
					{
						footnoteString += ",";
					}
					footnoteString += footnote.Footnote.Footnote_Symbol;
					i++;
				}
			}
			return footnoteString;
		}
	}
}