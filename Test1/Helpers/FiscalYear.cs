using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IndInv.Helpers
{
    public class FiscalYear
    {
        public static string FYStr (int fiscalYear, int offset)
        {
            return "FY_" + (FYInt(fiscalYear)-offset).ToString() + "_" + (FYInt(fiscalYear)+1-offset).ToString();
        }


        public static int FYInt(int fiscalYear){
            if (fiscalYear == 1)
            {
                return 13;
            }
            else if (fiscalYear == 2)
            {
                return 14;
			}
			else if (fiscalYear == 3)
			{
				return 15;
			}
            else
            {
                return 14;
            }
        }

        public static string FYStrFull(string fyStr, int fiscalYear)
        {
            switch (fyStr)
            {
                case "FY_3":
                    return "FY_" + (FYInt(fiscalYear) - 3).ToString() + "_" + (FYInt(fiscalYear) + 1 - 3).ToString() + "_YTD";
                case "FY_2":
                    return "FY_" + (FYInt(fiscalYear) - 2).ToString() + "_" + (FYInt(fiscalYear) + 1 - 2).ToString() + "_YTD";
                case "FY_1":
                    return "FY_" + (FYInt(fiscalYear) - 1).ToString() + "_" + (FYInt(fiscalYear) + 1 - 1).ToString() + "_YTD";
                case "FY_Header":
                    return "FY " + (FYInt(fiscalYear)).ToString() + "/" + (FYInt(fiscalYear) + 1).ToString() + " Performance";
                case "FY_3_Sup":
                    return "FY_" + (FYInt(fiscalYear) - 3).ToString() + "_" + (FYInt(fiscalYear) + 1 - 3).ToString() + "_YTD_Sup";
                case "FY_2_Sup":
                    return "FY_" + (FYInt(fiscalYear) - 2).ToString() + "_" + (FYInt(fiscalYear) + 1 - 2).ToString() + "_YTD_Sup";
                case "FY_1_Sup":
                    return "FY_" + (FYInt(fiscalYear) - 1).ToString() + "_" + (FYInt(fiscalYear) + 1 - 1).ToString() + "_YTD_Sup";
                default:
                    return fyStr.Replace(
                        "FY_", 
                        "FY_" + (FYInt(fiscalYear)).ToString() + "_" + (FYInt(fiscalYear) + 1).ToString() + "_"
                        );
            }
        }

		public static string FYStrReverse(string fyStr, int fiscalYear)
		{
			return fyStr.Replace(FYStr(fiscalYear, 0), "FY_").Replace("__","_").Replace("_Sup","");
		}

        public static string FYStrFullFormated(string fyStr, int fiscalYear)
        {
            switch (fyStr)
            {
                case "FY_3":
                    return "FY " + (FYInt(fiscalYear) - 3).ToString() + "/" + (FYInt(fiscalYear) + 1 - 3).ToString();
                case "FY_2":
                    return "FY " + (FYInt(fiscalYear) - 2).ToString() + "/" + (FYInt(fiscalYear) + 1 - 2).ToString();
                case "FY_1":
                    return "FY " + (FYInt(fiscalYear) - 1).ToString() + "/" + (FYInt(fiscalYear) + 1 - 1).ToString();
                case "FY_0":
                    return "FY " + (FYInt(fiscalYear)).ToString() + "/" + (FYInt(fiscalYear) + 1).ToString();
                default:
                    return fyStr.Replace(
                        "FY_",
                        "FY_" + (FYInt(fiscalYear)).ToString() + "_" + (FYInt(fiscalYear) + 1).ToString() + "_"
                        );
            }
        }
    }
}