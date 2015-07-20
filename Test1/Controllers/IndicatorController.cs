using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using IndInv.Models;
using IndInv.Models.ViewModels;
using IndInv.Helpers;

using ClosedXML.Excel;
using SpreadsheetLight;
using SpreadsheetLight.Drawing;
using System.Net;
using System.Collections.Specialized;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data.Objects.SqlClient;


namespace IndInv.Controllers
{

    public class IndicatorController : Controller
    {
        private InventoryDBContext db = new InventoryDBContext();

        //
        // GET: /Indicator/

        [HttpGet]
        public void Index(Int16? fiscalYear)
        {
            /*
            if (!fiscalYear.HasValue)
            {
                fiscalYear = 2;
            }
            var indexViewModel = new indexViewModel()
            {
                allIndicators = db.Indicators.Where(x => x.Indicator_CoE_Map.FirstOrDefault().CoE_ID > 10 && x.Indicator_CoE_Map.FirstOrDefault().CoE_ID < 20).ToList(),
                allAnalysts = db.Analysts.ToList(),
                allAreas = db.Areas.Where(x => x.Area_ID == 1 || x.Area_ID == 3 || x.Area_ID == 4 || x.Area_ID == 5).ToList(),
                allCoEs = db.CoEs.Where(x => x.CoE_ID > 10 && x.CoE_ID < 20 && x.CoE_ID != 14).ToList(),
                allFootnotes = db.Footnotes.ToList(),

                Fiscal_Year = fiscalYear.Value,
            };

            return View(indexViewModel);*/

			Response.Redirect("indicator/editInventory?fiscalYear=3");
			// return null;
        }

		public ActionResult dashboard(Int16? fiscalYear)
		{

			if (!fiscalYear.HasValue)
			{
				fiscalYear = 2;
			}
			var indexViewModel = new indexViewModel()
			{
				allIndicators = db.Indicators.Where(x => x.Indicator_CoE_Map.FirstOrDefault().CoE_ID > 10 && x.Indicator_CoE_Map.FirstOrDefault().CoE_ID < 20).ToList(),
				allAnalysts = db.Analysts.ToList(),
				allAreas = db.Areas.Where(x => x.Area_ID == 1 || x.Area_ID == 3 || x.Area_ID == 4 || x.Area_ID == 5).ToList(),
				allCoEs = db.CoEs.Where(x => x.CoE_ID > 10 && x.CoE_ID < 20 && x.CoE_ID != 14).ToList(),
				allFootnotes = db.Footnotes.ToList(),

				Fiscal_Year = fiscalYear.Value,
			};

			return View(indexViewModel);
		}

        [HttpPost]
        public ActionResult searchAdvanced(IList<searchViewModel> advancedSearch)
        {
            TempData["search"] = advancedSearch.FirstOrDefault();
            return Json(Url.Action("searchResults", "Indicator"));
        }

        public ActionResult searchResults()
        {
            TempData.Keep();
            searchViewModel advancedSearch = (searchViewModel)TempData["search"];

            if (advancedSearch == null)
            {
                return RedirectToAction("Index");
            }

            //List<Indicators> indicatorList = db.Indicators.ToList();
            List<Indicators> indicatorList = db.Indicators.Where(x => x.Area_ID.Equals(1)).Where(y => y.Indicator_CoE_Map.Any(x => x.CoE_ID.Equals(10) || x.CoE_ID.Equals(27) || x.CoE_ID.Equals(30) || x.CoE_ID.Equals(40) || x.CoE_ID.Equals(50))).ToList();
            List<Indicators> indicatorListString = new List<Indicators>();

            string searchString = advancedSearch.searchString;
            if (searchString != null)
            {
                string[] searchStrings;
                searchStrings = searchString.Split(' ');
                foreach (var sS in searchStrings)
				{
                    indicatorList = indicatorList.Where(s => s.Indicator != null && s.Indicator.ToLower().Contains(sS.ToLower())).ToList();
                }
            }

            List<Indicators> indicatorListCoE = new List<Indicators>();
            List<selectedCoEs> searchCoEs;
            searchCoEs = advancedSearch.selectedCoEs;
            if (searchCoEs != null)
            {
                foreach (var coe in searchCoEs)
                {
                    indicatorListCoE.AddRange(db.Indicators.Where(s => s.Indicator_CoE_Map.Any(x => x.CoE_ID == coe.CoE_ID)).ToList());
                }
                indicatorList = indicatorList.Intersect(indicatorListCoE).ToList();
            }

            List<Indicators> indicatorListAreas = new List<Indicators>();
            List<selectedAreas> searchAreas;
            searchAreas = advancedSearch.selectedAreas;
            if (searchAreas != null)
            {
                foreach (var area in searchAreas)
                {
                    indicatorListAreas.AddRange(db.Indicators.Where(s => s.Area_ID == area.Area_ID).ToList());
                }
                indicatorList = indicatorList.Intersect(indicatorListAreas).ToList();
            }

            List<Indicators> indicatorListTypes = new List<Indicators>();
            List<selectedTypes> searchTypes;
            searchTypes = advancedSearch.selectedTypes;
            if (searchTypes != null)
            {
                foreach (var type in searchTypes)
                {
                    indicatorListTypes.AddRange(db.Indicators.Where(s => s.Indicator_Type.Replace("/", "").Replace("&", "").Replace(" ", "") == type.Indicator_Type).ToList());
                }
                indicatorList = indicatorList.Intersect(indicatorListTypes).ToList();
            }

            List<Indicators> indicatorListFootnotes = new List<Indicators>();
            List<selectedFootnotes> searchFootnotes;
            searchFootnotes = advancedSearch.selectedFootnotes;
            if (searchFootnotes != null)
            {
                foreach (var footnote in searchFootnotes)
                {
                    indicatorListFootnotes.AddRange(db.Indicators.Where(s => s.Indicator_Footnote_Map.Any(x => x.Footnote_ID == footnote.Footnote_ID)).ToList());
                }
                indicatorList = indicatorList.Intersect(indicatorListFootnotes).ToList();
            }

            if (ModelState.IsValid)
            {
                var viewModel = new indexViewModel
                {
                    allIndicators = indicatorList.Distinct().ToList(),

                    //                    allCoEs = db.CoEs.ToList(),
                    //                    allAreas = db.Areas.ToList(),
                    //                    allFootnotes = db.Footnotes.ToList()
                };
                return View(viewModel);
            }

            return View();
        }

        public ActionResult viewPR(Int16 fiscalYear, Int16? analystID, Int16? coeID)
        {
            var allMaps = new List<Indicator_CoE_Maps>();

            if (analystID.HasValue)
            {
                allMaps = db.Indicator_CoE_Maps.Where(x => x.Indicator.Analyst_ID == analystID).ToList();
            }
            else
            {
                allMaps = db.Indicator_CoE_Maps.ToList();
            }

            var allCoEs = new List<CoEs>();
            if (coeID.HasValue)
            {
                allCoEs = db.CoEs.Where(x => x.CoE_ID == coeID).ToList();
                allMaps = allMaps.Where(x => x.CoE.CoE_ID == coeID || x.CoE_ID == 0).ToList();
            }
            else
            {
                allCoEs = db.CoEs.Where(x=>x.CoE_ID != 0).ToList();
            }
            ModelState.Clear();

			var obj = allCoEs.FirstOrDefault();
            var type = obj.GetType();
			//var test = type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Draft").GetValue(obj, null) == null;
			var isDraft = (bool)(type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Draft").GetValue(obj,null) ?? false);

            var viewModel = new PRViewModel
            {
                //allCoEs = db.CoEs.ToList(),
                allAnalysts = db.Analysts.ToList(),
                allCoEs = allCoEs,
                allMaps = allMaps,
                allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
                allFootnotes = db.Footnotes.ToList(),
                allAreas = db.Areas.ToList(),
                Fiscal_Year = fiscalYear,
                Analyst_ID = analystID,
                allColors = db.Color_Types.ToList(),
                allDirections = db.Color_Directions.ToList(),
                allThresholds = db.Color_Thresholds.ToList(),
				allFormats = db.Formats.ToList(),
				allIndicators = null,
				isDraft = isDraft
            };

            return View(viewModel);
        }

		public ActionResult viewPRdbl(Int16 fiscalYear, Int16? analystID, Int16 coeID, Int16 coeID2)
		{
			var allMaps = new List<Indicator_CoE_Maps>();

			if (analystID.HasValue)
			{
				allMaps = db.Indicator_CoE_Maps.Where(x => x.Indicator.Analyst_ID == analystID).ToList();
			}
			else
			{
				allMaps = db.Indicator_CoE_Maps.ToList();
			}

			var allCoEs = db.CoEs.Where(x => x.CoE_ID == coeID).ToList();
			allMaps = allMaps.Where(x => x.CoE.CoE_ID == coeID || x.CoE_ID == 0).ToList();

			ModelState.Clear();
			var viewModel = new PRViewModel
			{
				//allCoEs = db.CoEs.ToList(),
				allAnalysts = db.Analysts.ToList(),
				allCoEs = allCoEs,
				allMaps = allMaps,
				allIndicators = db.Indicators.ToList(),
				allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
				allFootnotes = db.Footnotes.ToList(),
				allAreas = db.Areas.ToList(),
				Fiscal_Year = fiscalYear,
				Analyst_ID = analystID,
				allColors = db.Color_Types.ToList(),
				allDirections = db.Color_Directions.ToList(),
				allThresholds = db.Color_Thresholds.ToList(),
				allFormats = db.Formats.ToList(),

				coeID2= coeID2
			};

			return View(viewModel);
		}

		public ActionResult EditInventoryTD(Int16 fiscalYear, Int16? analystID, Int16? coeID)
		{
			var allMaps = new List<Indicator_CoE_Maps>();

			if (analystID.HasValue)
			{
				allMaps = db.Indicator_CoE_Maps.Where(x => x.Indicator.Analyst_ID == analystID).ToList();
			}
			else
			{
				allMaps = db.Indicator_CoE_Maps.ToList();
			}

			var allCoEs = new List<CoEs>();
			if (coeID.HasValue)
			{
				allCoEs = db.CoEs.Where(x => x.CoE_ID == coeID).ToList();
				allMaps = allMaps.Where(x => x.CoE.CoE_ID == coeID || x.CoE_ID == 0).ToList();
			}
			else
			{
				allCoEs = db.CoEs.Where(x => x.CoE_ID != 0).ToList();
			}

			ModelState.Clear();
			var viewModel = new PRViewModel
			{
				//allCoEs = db.CoEs.ToList(),
				allAnalysts = db.Analysts.ToList(),
				allCoEs = allCoEs,
				allMaps = allMaps,
				allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
				allFootnotes = db.Footnotes.ToList(),
				allAreas = db.Areas.ToList(),
				Fiscal_Year = fiscalYear,
				Analyst_ID = analystID,
				allColors = db.Color_Types.ToList(),
				allDirections = db.Color_Directions.ToList(),
				allThresholds = db.Color_Thresholds.ToList(),
				allFormats = db.Formats.ToList()
			};

			return View(viewModel);
		}

        public ActionResult EditInventoryTD2(Int16 fiscalYear, Int16? analystID, Int16? coeID)
        {
            var allMaps = new List<Indicator_CoE_Maps>();

            if (analystID.HasValue)
            {
                allMaps = db.Indicator_CoE_Maps.Where(x => x.Indicator.Analyst_ID == analystID).ToList();
            }
            else
            {
                allMaps = db.Indicator_CoE_Maps.ToList();
            }

            var allCoEs = new List<CoEs>();
            if (coeID.HasValue)
            {
                allCoEs = db.CoEs.Where(x => x.CoE_ID == coeID).ToList();
                allMaps = allMaps.Where(x => x.CoE.CoE_ID == coeID || x.CoE_ID == 0).ToList();
            }
            else
            {
                allCoEs = db.CoEs.Where(x => x.CoE_ID != 0).ToList();
            }

			allMaps.AddRange(db.Indicator_CoE_Maps.Where(x => x.Indicator.Indicator_N_Value == true).ToList());

            ModelState.Clear();
            var viewModel = new PRViewModel
            {
                //allCoEs = db.CoEs.ToList(),
                allAnalysts = db.Analysts.ToList(),
                allCoEs = allCoEs,
                allMaps = allMaps,
                allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
                allFootnotes = db.Footnotes.ToList(),
                allAreas = db.Areas.ToList(),
                Fiscal_Year = fiscalYear,
                Analyst_ID = analystID,
                allColors = db.Color_Types.ToList(),
                allDirections = db.Color_Directions.ToList(),
                allThresholds = db.Color_Thresholds.ToList(),
				allFormats = db.Formats.ToList()
            };

            return View(viewModel);
        }

        public ActionResult viewPRSimple(Int16 fiscalYear, Int16? analystID)
        {
            var allMaps = new List<Indicator_CoE_Maps>();

            if (analystID.HasValue)
            {
                allMaps = db.Indicator_CoE_Maps.Where(x => x.Indicator.Analyst_ID == analystID).ToList();
            }
            else
            {
                allMaps = db.Indicator_CoE_Maps.ToList();
            }

            ModelState.Clear();
            var viewModel = new PRViewModel
            {
                //allCoEs = db.CoEs.ToList(),
                allAnalysts = db.Analysts.ToList(),
                allCoEs = db.CoEs.ToList(),
                allMaps = allMaps,
                allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
                allFootnotes = db.Footnotes.ToList(),
                Fiscal_Year = fiscalYear,
                Analyst_ID = analystID,
                allColors = db.Color_Types.ToList(),
            };

            return View(viewModel);
        }

		public ActionResult viewPRSimpleDbl(Int16 fiscalYear, Int16? analystID)
		{
			var allMaps = new List<Indicator_CoE_Maps>();

			if (analystID.HasValue)
			{
				allMaps = db.Indicator_CoE_Maps.Where(x => x.Indicator.Analyst_ID == analystID).ToList();
			}
			else
			{
				allMaps = db.Indicator_CoE_Maps.ToList();
			}

			ModelState.Clear();
			var viewModel = new PRViewModel
			{
				//allCoEs = db.CoEs.ToList(),
				allAnalysts = db.Analysts.ToList(),
				allCoEs = db.CoEs.ToList(),
				allMaps = allMaps,
				allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
				allFootnotes = db.Footnotes.ToList(),
				Fiscal_Year = fiscalYear,
				Analyst_ID = analystID,
				allColors = db.Color_Types.ToList(),
				allIndicators = db.Indicators.ToList()
			};

			return View(viewModel);
		}

        [AllowAnonymous]
        public ActionResult viewPRSimple_Header()
        {
            return View();

        }

		public static class ExcelGlobalVariables
		{
			public static ClosedXML.Excel.XLColor prBlue { get { return XLColor.FromArgb(0, 51, 102); } }
			public static ClosedXML.Excel.XLColor prGreen { get { return XLColor.FromArgb(0, 118, 53); } }
			public static ClosedXML.Excel.XLColor prYellow { get { return XLColor.FromArgb(255, 192, 0); } }
			public static ClosedXML.Excel.XLColor prRed { get { return XLColor.FromArgb(255, 0, 0); } }
			public static ClosedXML.Excel.XLColor prHeader1Fill { get { return prBlue; } }
			public static ClosedXML.Excel.XLColor prHeader1Font { get { return XLColor.White; } }
			public static ClosedXML.Excel.XLColor prHeader2Fill { get { return XLColor.White; } }
			public static ClosedXML.Excel.XLColor prHeader2Font { get { return XLColor.White; } }

			public static ClosedXML.Excel.XLColor prBorder { get { return XLColor.FromArgb(0, 0, 0); } }
			public static ClosedXML.Excel.XLColor prAreaFill { get { return XLColor.FromArgb(192, 192, 192); } }
			public static ClosedXML.Excel.XLColor prAreaFont { get { return XLColor.Black; } }
		}

        public ActionResult viewPRExcel(Int16 fiscalYear, Int16? coeID)
        {
            ModelState.Clear();
            var viewModel = new PRViewModel
            {
                //allCoEs = db.CoEs.ToList(),
                allCoEs = db.CoEs.ToList(),
                allMaps = db.Indicator_CoE_Maps.ToList(),
                allFootnoteMaps = db.Indicator_Footnote_Maps.ToList()
            };

            // Create the workbook
            var wb = new XLWorkbook();

			var prBlue = ExcelGlobalVariables.prBlue;// XLColor.FromArgb(0, 51, 102);
			var prGreen = ExcelGlobalVariables.prGreen;//XLColor.FromArgb(0, 118, 53);
			var prYellow = ExcelGlobalVariables.prYellow; //XLColor.FromArgb(255, 192, 0);
			var prRed = ExcelGlobalVariables.prRed;// XLColor.FromArgb(255, 0, 0);
			var prHeader1Fill = ExcelGlobalVariables.prHeader1Fill;//prBlue;
			var prHeader1Font = ExcelGlobalVariables.prHeader1Font;//XLColor.White;
			var prHeader2Fill = ExcelGlobalVariables.prHeader2Fill;//XLColor.White;
			var prHeader2Font = ExcelGlobalVariables.prHeader2Font;//XLColor.Black;
            var prBorder = ExcelGlobalVariables.prBorder;//XLColor.FromArgb(0, 0, 0);
            var prAreaFill = ExcelGlobalVariables.prAreaFill;//XLColor.FromArgb(192, 192, 192);
			var prAreaFont = ExcelGlobalVariables.prAreaFont;//XLColor.Black;
            var prBorderWidth = XLBorderStyleValues.Thin;
            var prFontSize = 10;
            var prTitleFont = 20;
            var prFootnoteSize = 8;
            var prHeightSeperator = 7.5;

            var prAreaObjectiveFontsize = 8;
            var indentLength = 2;
            var newLineHeight = 12.6;

            var defNote = "Portal data from the Canadian Institute for Health Information (CIHI) has been used to generate data within this report with acknowledgement to CIHI, the Ministry of Health and Long-Term Care (MOHLTC) and Stats Canada (as applicable). Views are not those of the acknowledged sources. Facility identifiable data other than Mount Sinai Hospital (MSH) is not to be published without the consent of that organization (except where reported at an aggregate level). As this is not a database supported by MSH, please demonstrate caution with use and interpretation of the information. MSH is not responsible for any changes derived from the source data/canned reports. Data may be subject to change.";

            var prNumberWidth = 4;
            var prIndicatorWidth = 55;
            var prValueWidth = 11;
            var prDefWidth = 100;
            var prRatiWidth = 50;
            var prCompWidth = 50;

            //var fitRatio = 3.77;
            var fitRatio = 1.7;
            List<int> fitAdjustableRows = new List<int>();

            var prFootnoteCharsNewLine = 125;
            var prObjectivesCharsNewLine = 226;

            var allCoes = new List<CoEs>();
            if (coeID != 0 && coeID != null)
            {
                allCoes = viewModel.allCoEs.Where(x => x.CoE_ID == coeID).ToList();
            }
            else
            {
                allCoes = viewModel.allCoEs.ToList();
            }

            foreach (var coe in allCoes)
            {
				var wsPRName = coe.CoE_Abbr != null && coe.CoE_Abbr != "" ? coe.CoE_Abbr : "Indicators";
				var wsDefName = coe.CoE_Abbr != null && coe.CoE_Abbr != "" ? "Def_" + coe.CoE_Abbr : "Def_Indicators";
                var wsPR = wb.Worksheets.Add(wsPRName);
                var wsDef = wb.Worksheets.Add(wsDefName);
                List<IXLWorksheet> wsList = new List<IXLWorksheet>();
                wsList.Add(wsPR);
                wsList.Add(wsDef);

                foreach (var ws in wsList)
                {
                    var currentRow = 4;
                    ws.Row(2).Height = 21;
                    int startRow;
                    int indicatorNumber = 1;

                    ws.PageSetup.Margins.Top = 0;
                    ws.PageSetup.Margins.Header = 0;
                    ws.PageSetup.Margins.Left = 0.5;
                    ws.PageSetup.Margins.Right = 0.5;
                    ws.PageSetup.Margins.Bottom = 0.5;
                    ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    ws.PageSetup.PaperSize = XLPaperSize.LegalPaper;
                    ws.PageSetup.FitToPages(1, 1);

                    string[,] columnHeaders = new string[0, 0];
                    if (ws.Name == wsPRName)
                    {
                        var prHeadder2Title = FiscalYear.FYStrFull("FY_", fiscalYear) + "Performance";
                        prHeadder2Title = prHeadder2Title.Replace("_", " ");
                        columnHeaders = new string[,]{
                            {"Number",""},
                            {"Indicator",""},
                            {FiscalYear.FYStrFull("FY_3", fiscalYear), ""},
                            {FiscalYear.FYStrFull("FY_2", fiscalYear),""},
                            {FiscalYear.FYStrFull("FY_1", fiscalYear),""},
                            {prHeadder2Title,"Q1"},
                            {prHeadder2Title,"Q2"},
                            {prHeadder2Title,"Q3"},
                            {prHeadder2Title,"Q4"},
                            {prHeadder2Title,"YTD"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Target",""},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Performance_Threshold",""},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Comparator",""}
                        };
                    }
                    else if (ws.Name == wsDefName)
                    {
                        columnHeaders = new string[,]{
                            {"Number",""},
                            {"Indicator",""},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Definition_Calculation",""},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Target_Rationale",""},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Comparator_Source",""}
                        };
                    }

                    var currentCol = 1;
                    var prHeader2ColStart = 99;
                    var prHeader2ColEnd = 1;
                    int maxCol = columnHeaders.GetUpperBound(0) + 1;

                    var prTitle = ws.Cell(currentRow, 1);
                    prTitle.Value = coe.CoE;
                    prTitle.Style.Font.FontSize = prTitleFont;
                    prTitle.Style.Font.Bold = true;
                    prTitle.Style.Font.FontColor = prHeader1Font;
                    prTitle.Style.Fill.BackgroundColor = prHeader1Fill;
                    ws.Range(ws.Cell(currentRow, 1), ws.Cell(currentRow, maxCol)).Merge();
                    ws.Range(ws.Cell(currentRow + 1, 1), ws.Cell(currentRow + 1, maxCol)).Merge();
                    ws.Row(currentRow + 1).Height = prHeightSeperator;
                    currentRow += 2;
                    startRow = currentRow;

                    for (int i = 0; i <= columnHeaders.GetUpperBound(0); i++)
                    {
                        if (columnHeaders[i, 1] == "")
                        {
                            var columnField = columnHeaders[i, 0];
                            string cellValue;
                            Type t = typeof(Indicators);
                            cellValue = t.GetProperty(columnField) != null ?
                                ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(Indicators), columnField).DisplayName :
                                ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(Indicator_CoE_Maps), columnField).DisplayName;
                            ws.Cell(currentRow, currentCol).Value = cellValue;
                            ws.Range(ws.Cell(currentRow, currentCol), ws.Cell(currentRow + 1, currentCol)).Merge();
                            currentCol++;
                        }
                        else
                        {
                            var columnField = columnHeaders[i, 1];
                            var columnFieldTop = columnHeaders[i, 0];
                            ws.Cell(currentRow + 1, currentCol).Value = columnField;
                            ws.Cell(currentRow, currentCol).Value = columnFieldTop;
                            if (currentCol < prHeader2ColStart) { prHeader2ColStart = currentCol; }
                            if (currentCol > prHeader2ColEnd) { prHeader2ColEnd = currentCol; }
                            currentCol++;
                        }
                    }
                    currentCol--;
                    ws.Range(ws.Cell(currentRow, prHeader2ColStart).Address, ws.Cell(currentRow, prHeader2ColEnd).Address).Merge();
                    var prHeader1 = ws.Range(ws.Cell(currentRow, 1).Address, ws.Cell(currentRow + 1, currentCol).Address);
                    var prHeader2 = ws.Range(ws.Cell(currentRow + 1, prHeader2ColStart).Address, ws.Cell(currentRow + 1, prHeader2ColEnd).Address);

                    prHeader1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    prHeader1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    prHeader1.Style.Fill.BackgroundColor = prHeader1Fill;
                    prHeader1.Style.Font.FontColor = prHeader1Font;

                    prHeader2.Style.Fill.BackgroundColor = prHeader2Fill;
                    prHeader2.Style.Font.FontColor = prHeader2Font;

                    currentRow += 2;

                    List<Footnotes> footnotes = new List<Footnotes>();
                    foreach (var areaMap in coe.Area_CoE_Map.Where(x => x.Fiscal_Year == fiscalYear).OrderBy(x => x.Area.Sort))
                    {
                        var cellLengthObjective = 0;
                        var prArea = ws.Range(ws.Cell(currentRow, 1), ws.Cell(currentRow, maxCol));
                        //fitAdjustableRows.Add(currentRow);
                        prArea.Merge();
                        prArea.Style.Fill.BackgroundColor = prAreaFill;
                        prArea.Style.Font.FontColor = prAreaFont;
                        prArea.FirstCell().RichText.AddText(areaMap.Area.Area).Bold = true;
                        cellLengthObjective += areaMap.Area.Area.Length;

                        if (ws == wsPR)
                        {
                            var indent = new string('_', indentLength);

                            var stringSeperators = new string[] { "•" };
                            if (areaMap.Objective != null)
                            {
                                var objectives = Regex.Matches(areaMap.Objective, @"\[.*?\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();	
                                //for (var i = 1; i < objectives.Length; i++)
                                var i = 1;
                                foreach (var objective in objectives)
                                {
                                    prArea.FirstCell().RichText.AddNewLine();
                                    ws.Row(currentRow).Height += newLineHeight;
                                    prArea.FirstCell().RichText.AddText(indent).SetFontColor(prAreaFill).SetFontSize(prAreaObjectiveFontsize);
                                    prArea.FirstCell().RichText.AddText(" " + i +". " + objective).FontSize = prAreaObjectiveFontsize;
                                    i++;
                                }
                            }
                        }

                        currentRow++;

                        var allMaps = viewModel.allMaps.Where(x => x.Fiscal_Year == fiscalYear).Where(e => e.Indicator.Area.Equals(areaMap.Area)).Where(d => d.CoE.CoE != null && d.CoE.CoE.Contains(coe.CoE)).OrderBy(f => f.Number).ToList();
                        var allNValues = new List<Indicator_CoE_Maps>();
                        if (ws.Name == wsPRName)
                        {
                            allNValues = viewModel.allMaps.Where(x => x.Fiscal_Year == fiscalYear && x.Indicator.Indicator_N_Value == true).ToList();
                        }
                        var allMapsWithNValues = new List<Indicator_CoE_Maps>();
                        foreach (var nValue in allNValues)
                        {
                            var indicatorIndex = allMaps.FirstOrDefault(x => x.Indicator_ID == nValue.Indicator.Indicator_N_Value_ID);
                            if (indicatorIndex != null)
                            {
                                var position = allMaps.IndexOf(indicatorIndex);
                                allMapsWithNValues.Add(indicatorIndex);
                                allMaps.Insert(position + 1, nValue);
                            }
                        }
                        foreach (var map in allMaps)
                        {
                            fitAdjustableRows.Add(currentRow);
                            currentCol = 1;

                            int rowSpan = 1;
                            if (allMapsWithNValues.Contains(map) || !allNValues.Contains(map))
                            {
                                if (allMapsWithNValues.Contains(map))
                                {
                                    rowSpan = 2;
                                    ws.Range(ws.Cell(currentRow, currentCol), ws.Cell(currentRow + 1, currentCol)).Merge();
                                    ws.Range(ws.Cell(currentRow, currentCol + 1), ws.Cell(currentRow + 1, currentCol + 1)).Merge();
                                }
                                ws.Cell(currentRow, currentCol).Style.Border.OutsideBorder = prBorderWidth;
                                ws.Cell(currentRow, currentCol).Style.Border.OutsideBorderColor = prBorder;
                                ws.Cell(currentRow, currentCol).Value = indicatorNumber;
                                indicatorNumber++;
                                ws.Cell(currentRow, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                currentCol++;

                                ws.Cell(currentRow, currentCol).Style.Border.OutsideBorder = prBorderWidth;
                                ws.Cell(currentRow, currentCol).Style.Border.OutsideBorderColor = prBorder;
                                int j = 0;
                                ws.Cell(currentRow, currentCol).Value = map.Indicator.Indicator;
                                foreach (var footnote in map.Indicator.Indicator_Footnote_Map.Where(x => x.Fiscal_Year == fiscalYear).Where(e => e.Indicator_ID == map.Indicator_ID).OrderBy(e => e.Indicator_ID))
                                {
                                    if (!footnotes.Contains(footnote.Footnote)) { footnotes.Add(footnote.Footnote); }
                                    if (j != 0)
                                    {
                                        ws.Cell(currentRow, currentCol).RichText.AddText(",").VerticalAlignment = XLFontVerticalTextAlignmentValues.Superscript;
                                    }
                                    ws.Cell(currentRow, currentCol).RichText.AddText(footnote.Footnote.Footnote_Symbol).VerticalAlignment = XLFontVerticalTextAlignmentValues.Superscript;
                                    j++;
                                }
                                ws.Cell(currentRow, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                currentCol++;
                            }
                            else
                            {
                                ws.Cell(currentRow, currentCol).Style.Border.OutsideBorder = prBorderWidth;
                                ws.Cell(currentRow, currentCol).Style.Border.OutsideBorderColor = prBorder;
                                currentCol += 2;
                                rowSpan = 0;
                            }

                            if (ws.Name == wsPRName)
                            {
                                for (var i = 3; i <= 15; i++)
                                {
                                    ws.Column(i).Width = ws.Name == wsPRName ? prValueWidth : prDefWidth;
                                }

                                var obj = map.Indicator;
                                var type = obj.GetType();
                                string[,] columnIndicators = new string[,]{
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_3",fiscalYear)).GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_3",fiscalYear) + "_Sup").GetValue(obj,null),
                                     "",
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_2",fiscalYear)).GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_2",fiscalYear) + "_Sup").GetValue(obj,null),
                                     "",
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_1",fiscalYear)).GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_1",fiscalYear) + "_Sup").GetValue(obj,null),
                                     "",
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q1").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q1_Sup").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q1_Color").GetValue(obj,null),
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q2").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q2_Sup").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q2_Color").GetValue(obj,null),
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q3").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q3_Sup").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q3_Color").GetValue(obj,null),
                                     "1",
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q4").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q4_Sup").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Q4_Color").GetValue(obj,null),
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "YTD").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "YTD_Sup").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "YTD_Color").GetValue(obj,null),
                                     "1"
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Target").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Target_Sup").GetValue(obj,null),
                                     "",
                                     rowSpan.ToString()
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Performance_Threshold").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Performance_Threshold_Sup").GetValue(obj,null),
                                     "",
                                    rowSpan.ToString()
                                    },
                                    {(string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Comparator").GetValue(obj,null),
                                     (string)type.GetProperty(FiscalYear.FYStrFull("FY_",fiscalYear) + "Comparator_Sup").GetValue(obj,null),
                                     "",
                                     rowSpan.ToString()
                                    },
                                };
                                var startCol = currentCol;
                                int k = 1;
                                for (var i = 0; i <= columnIndicators.GetUpperBound(0); i++)
                                {
                                    for (var j = 0; j <= columnIndicators.GetUpperBound(1); j++)
                                    {
                                        if (columnIndicators[i, j] != null)
                                        {
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("<b>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("</b>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("<u>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("</u>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("<i>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("</i>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("<sup>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("</sup>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("<sub>", "");
                                            columnIndicators[i, j] = columnIndicators[i, j].Replace("</sub>", "");
                                        }
                                    }
                                    if (i != columnIndicators.GetUpperBound(0) && columnIndicators[i, 0] == "=")
                                    {
                                        k = 1;
                                        while (columnIndicators[i + k, 0] == "=") { k++; }
                                        ws.Range(ws.Cell(currentRow, startCol + i - 1), ws.Cell(currentRow, startCol + i + k - 1)).Merge();
                                        i += k - 1;
                                        k = 1;
                                    }
                                    else if (columnIndicators[i, 0] != "=")
                                    {
                                        ws.Cell(currentRow, currentCol + i).Style.Border.OutsideBorder = prBorderWidth;
                                        ws.Cell(currentRow, currentCol + i).Style.Border.OutsideBorderColor = prBorder;
                                        if (columnIndicators[i, 3] != "0")
                                        {
                                            if (columnIndicators[i, 3] == "2") {
                                                ws.Range(ws.Cell(currentRow, currentCol + i), ws.Cell(currentRow + 1, currentCol + i)).Merge();
                                            }
                                            if (allNValues.Contains(map))
                                            {
                                                ws.Cell(currentRow, currentCol + i).Style.Border.TopBorder = XLBorderStyleValues.None;
                                            }
                                            else if (allMapsWithNValues.Contains(map))
                                            {
                                                ws.Cell(currentRow, currentCol + i).Style.Border.BottomBorder = XLBorderStyleValues.None;
                                            }
                                            var cell = ws.Cell(currentRow, currentCol + i);
                                            string cellValue = "";

                                            if (columnIndicators[i, 0] != null)
                                            {
                                                cellValue = columnIndicators[i, 0].ToString();
                                            }

                                            if (cellValue.Contains("$"))
                                            {
                                            }

                                            cell.Value = "'" + cellValue;
                                            if (columnIndicators[i, 1] != null)
                                            {
                                                cell.RichText.AddText(columnIndicators[i, 1]).VerticalAlignment = XLFontVerticalTextAlignmentValues.Superscript;
                                            }
                                            switch (columnIndicators[i, 2])
                                            {
                                                case "cssWhite":
                                                    cell.RichText.SetFontColor(XLColor.Black);
                                                    cell.Style.Fill.BackgroundColor = XLColor.White;
                                                    break;
                                                case "cssGreen":
                                                    cell.RichText.SetFontColor(XLColor.White);
                                                    cell.Style.Fill.BackgroundColor = prGreen;
                                                    break;
                                                case "cssYellow":
                                                    cell.RichText.SetFontColor(XLColor.Black);
                                                    cell.Style.Fill.BackgroundColor = prYellow;
                                                    break;
                                                case "cssRed":
                                                    cell.RichText.SetFontColor(XLColor.White);
                                                    cell.Style.Fill.BackgroundColor = prRed;
                                                    break;
                                            }
                                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                        }
                                    }
                                }
                                currentRow++;
                            }
                            else if (ws.Name == wsDefName)
                            {
                                ws.Column(3).Width = prDefWidth;
                                ws.Column(4).Width = prRatiWidth;
                                ws.Column(5).Width = prCompWidth;

                                var obj = map.Indicator;
                                var type = obj.GetType();

                                string defn = (string)type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Definition_Calculation").GetValue(obj, null);
                                string rationale = (string)type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Target_Rationale").GetValue(obj, null);
                                string comp = (string)type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Comparator_Source").GetValue(obj, null);

                                double maxLines = 1;
                                double lines;

                                if (defn != null)
                                {
                                    lines = defn.Length / ws.Column(currentCol).Width;
                                    maxLines = maxLines < lines ? lines : maxLines;
                                    ws.Cell(currentRow, currentCol).Value = defn;
                                }
                                ws.Cell(currentRow, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                currentCol++;

                                if (rationale != null)
                                {
                                    lines = rationale.Length / ws.Column(currentCol).Width;
                                    maxLines = maxLines < lines ? lines : maxLines;
                                    ws.Cell(currentRow, currentCol).Value = rationale;
                                }
                                ws.Cell(currentRow, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                currentCol++;

                                if (comp != null)
                                {
                                    lines = comp.Length / ws.Column(currentCol).Width;
                                    maxLines = maxLines < lines ? lines : maxLines;
                                    ws.Cell(currentRow, currentCol).Value = comp;
                                }
                                ws.Cell(currentRow, currentCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                currentCol++;

                                ws.Row(currentRow).Height = newLineHeight * Math.Ceiling(maxLines);
                                currentRow++;
                            }
                        }
                    }

                    var footnoteRow = ws.Range(ws.Cell(currentRow, 1), ws.Cell(currentRow, maxCol));
                    footnoteRow.Merge();
                    footnoteRow.Style.Font.FontSize = prFootnoteSize;

                    /*Footnotes defaultFootnote = db.Footnotes.FirstOrDefault(x => x.Footnote_Symbol == "*");
                    if (!footnotes.Contains(defaultFootnote))
                    {
                        footnotes.Add(defaultFootnote);
                    }*/

                    int cellLengthFootnote = 0;
                    if (ws.Name == wsPRName)
                    {
                        foreach (var footnote in footnotes.OrderBy(x=>x.Footnote_Order))
                        {
                            ws.Cell(currentRow, 1).RichText.AddText(footnote.Footnote_Symbol).VerticalAlignment = XLFontVerticalTextAlignmentValues.Superscript;
                            ws.Cell(currentRow, 1).RichText.AddText(" " + footnote.Footnote + ";");
                            ws.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                            cellLengthFootnote += footnote.Footnote_Symbol.ToString().Length + footnote.Footnote.ToString().Length + 2;
                            if (cellLengthFootnote > prFootnoteCharsNewLine)
                            {
                                ws.Cell(currentRow, 1).RichText.AddNewLine();
                                cellLengthFootnote = 0;
                                ws.Row(currentRow).Height += newLineHeight;
                            }
                        }
                    }
                    else
                    {
                        ws.Cell(currentRow, 1).Value = defNote;
                        ws.Row(currentRow).Height = 28;
                    }

                    var pr = ws.Range(ws.Cell(startRow, 1), ws.Cell(currentRow - 1, maxCol));

                    if (pr.Worksheet.Name == wsDefName)
                    {
                        pr.Style.Border.InsideBorder = prBorderWidth;
                        pr.Style.Border.InsideBorderColor = prBorder;
                    }
                    pr.Style.Border.OutsideBorder = prBorderWidth;
                    pr.Style.Border.OutsideBorderColor = prBorder;
                    pr.Style.Font.FontSize = prFontSize;

                    pr = ws.Range(ws.Cell(startRow, 1), ws.Cell(currentRow, maxCol));
                    pr.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    pr.Style.Alignment.WrapText = true;

                    ws.Column(1).Width = prNumberWidth;
                    ws.Column(2).Width = prIndicatorWidth;
                    footnotes.Clear();
                    indicatorNumber = 1;

                    var totalHeight = ExcelFunctions.getTotalHeight(ws, 4);
                    var totalWidth = ExcelFunctions.getTotalWidth(ws, 1);
                    var fitHeight = (int)(totalWidth / fitRatio);
                    var fitWidth = (int)(totalHeight * fitRatio);

                    if (ws.Name == "Def_WIH Obs") { System.Diagnostics.Debugger.Break(); }

                    if (fitHeight > totalHeight)
                    {
                        var fitAddHeightTotal = (fitHeight - totalHeight);
                        var fitAddHeightPerRow = fitAddHeightTotal / fitAdjustableRows.Count;
                        foreach (var row in fitAdjustableRows)
                        {
                            ws.Row(row).Height += fitAddHeightPerRow;
                        }
                    }
                    else
                    {
                        while ((fitWidth - totalWidth) / fitWidth > 0.001)
                        {
                            var fitAddWidthTotal = (fitWidth - totalWidth) / 10;
                            var fitAddWidthPerRow = fitAddWidthTotal / (ws.LastColumnUsed().ColumnNumber() - 1);
                            foreach (var col in ws.Columns(2, ws.LastColumnUsed().ColumnNumber()))
                            {
                                col.Width += fitAddWidthPerRow / 5.69;
                            }
                            ExcelFunctions.AutoFitWorksheet(ws, 2, 3, newLineHeight);
                            totalHeight = ExcelFunctions.getTotalHeight(ws, 4);
                            totalWidth = ExcelFunctions.getTotalWidth(ws, 1);
                            fitHeight = (int)(totalWidth / fitRatio);
                            fitWidth = (int)(totalHeight * fitRatio);
                        }
                    }
                }
            }

            MemoryStream preImage = new MemoryStream();
            wb.SaveAs(preImage);

            //Aspose.Cells.Workbook test = new Aspose.Cells.Workbook(preImage);
            //test.Save(this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/logo.pdf"), Aspose.Cells.SaveFormat.Pdf);

            MemoryStream postImage = new MemoryStream();
            SLDocument postImageWb = new SLDocument(preImage);

            string picPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/logo.png");
            SLPicture picLogo = new SLPicture(picPath);
            string picPathOPEO = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/logoOPEO.png");
            SLPicture picLogoOPEO = new SLPicture(picPathOPEO);
            string picMonthlyPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/Monthly.png");
            SLPicture picMonthly = new SLPicture(picMonthlyPath);
            string picQuaterlyPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/quaterly.png");
            SLPicture picQuaterly = new SLPicture(picQuaterlyPath);
            string picNAPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/na.png");
            SLPicture picNA = new SLPicture(picNAPath);
            string picTargetPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/target.png");
            SLPicture picTarget = new SLPicture(picTargetPath);

            foreach (var ws in wb.Worksheets)
            {
                postImageWb.SelectWorksheet(ws.Name);

                for (int i = 1; i < 20; ++i)
                {
                    var a = postImageWb.GetRowHeight(i);
                }

                picLogo.SetPosition(0, 0);
                picLogo.ResizeInPercentage(25, 25);
                postImageWb.InsertPicture(picLogo);

                picLogoOPEO.SetRelativePositionInPixels(0, ws.LastColumnUsed().ColumnNumber() + 1, -140, 0);
                picLogoOPEO.ResizeInPercentage(45, 45);
                postImageWb.InsertPicture(picLogoOPEO);

                if (ws.Name.Substring(0, 3) != "Def")
                {
                    picTarget.SetRelativePositionInPixels(ws.LastRowUsed().RowNumber() + 1, ws.LastColumnUsed().ColumnNumber() + 1, -240, 1);
                    picNA.SetRelativePositionInPixels(ws.LastRowUsed().RowNumber() + 1, ws.LastColumnUsed().ColumnNumber() + 1, -400, 1);
                    picMonthly.SetRelativePositionInPixels(ws.LastRowUsed().RowNumber() + 1, ws.LastColumnUsed().ColumnNumber() + 1, -500, 1);
                    picQuaterly.SetRelativePositionInPixels(ws.LastRowUsed().RowNumber() + 1, ws.LastColumnUsed().ColumnNumber() + 1, -490, 1);

                    picMonthly.ResizeInPercentage(70, 70);
                    picQuaterly.ResizeInPercentage(70, 70);
                    picNA.ResizeInPercentage(70, 70);
                    picTarget.ResizeInPercentage(70, 70);

                    postImageWb.InsertPicture(picMonthly);
                    postImageWb.InsertPicture(picQuaterly);
                    postImageWb.InsertPicture(picNA);
                    postImageWb.InsertPicture(picTarget);
                }
            }

            // Prepare the response
            HttpResponse httpResponse = this.HttpContext.ApplicationInstance.Context.Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"test.xlsx\"");
            //httpResponse.ContentType = "application/pdf";
            //httpResponse.AddHeader("content-disposition", "attachment;filename=\"test.pdf\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                postImageWb.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }

            httpResponse.End();

            return View(viewModel);
        }

        public ActionResult viewPRPdf(Int16 fiscalYear, Int16? coeID)
        {
            var allCoEs = db.CoEs.ToList();
            if (coeID != 0 && coeID != null)
            {
                allCoEs = allCoEs.Where(x => x.CoE_ID == coeID).ToList();
            }
            else
            {
                allCoEs = allCoEs.Where(x => x.CoE_ID != 0).ToList();
            }

            var allMaps = new List<Indicator_CoE_Maps>();
            allMaps = db.Indicator_CoE_Maps.ToList();
            ModelState.Clear();
            var viewModel = new PRViewModel
            {
                //allCoEs = db.CoEs.ToList(),
                allCoEs = allCoEs,
                allAnalysts = db.Analysts.ToList(),
                allMaps = allMaps,
                allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
                allFootnotes = db.Footnotes.ToList(),
                Fiscal_Year = fiscalYear,
                Analyst_ID = null,
                allColors = db.Color_Types.ToList(),
            };

            HttpResponse httpResponse = this.HttpContext.ApplicationInstance.Context.Response;
            httpResponse.Clear();
            httpResponse.ContentType = "application/pdf";
            httpResponse.AddHeader("content-disposition", "attachment;filename=\"test.pdf\"");

            MemoryStream memoryStream = new MemoryStream();
            string apiKey = "2429a8e1-7cf6-4a77-9f7f-f4a85a9fcc14";
            var test = (this.RenderView("viewPRSimple", viewModel));
            string value = "<meta charset='UTF-8' />" + test;

			List<string> coeNotes;
			var topMargin = 6;
			if (allCoEs.FirstOrDefault(x => x.CoE_ID == coeID).CoE_Notes != null && allCoEs.FirstOrDefault(x => x.CoE_ID == coeID).CoE_Notes != "")
			{
				coeNotes = Regex.Matches(allCoEs.FirstOrDefault(x => x.CoE_ID == coeID).CoE_Notes, @"\[.*?\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToList();
				var coeNotesCount = coeNotes.Count();
				topMargin+=coeNotesCount * 5;
			}

            using (var client = new WebClient())
            {
                NameValueCollection options = new NameValueCollection();
                options.Add("apikey", apiKey);
                options.Add("value", value);
                options.Add("DisableJavascript", "false");
                options.Add("PageSize", "Legal");
                options.Add("UseLandscape", "true");
                options.Add("Zoom", "1.0");
                options.Add("MarginLeft", "2");
                options.Add("MarginTop", "10");
				options.Add("MarginBottomn", "5");
                options.Add("MarginRight", "2");
                //options.Add("HeaderUrl", this.HttpContext.ApplicationInstance.Server.MapPath("viewPRSimple_Header"));
                byte[] result = client.UploadValues("http://api.html2pdfrocket.com/pdf", options);
                //httpResponse.BinaryWrite(result);
                memoryStream.Write(result, 0, result.Length);
            }

            string picPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/SHSheader.png");
            Image logo = Image.GetInstance(picPath);
            //string picPathOPEO = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/logoOPEO.png");
            //Image logoOPEO = Image.GetInstance(picPathOPEO);
            string footerPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/footer.png");
            Image footer = Image.GetInstance(footerPath);
            string picMonthlyPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/Monthly.png");
            string picQuaterlyPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/quaterly.png");
            string picNAPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/na.png");
            string picTargetPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/target.png");
			string picDraftPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/draft.png");
			Image picDraft = Image.GetInstance(picDraftPath);

            var pdfDocument  = new iTextSharp.text.Document();
            var outStream = new MemoryStream();
            var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(pdfDocument, outStream);
            
            pdfDocument.Open();
            var reader = new iTextSharp.text.pdf.PdfReader(memoryStream.ToArray());

            for (var page = 1; page <= reader.NumberOfPages; page++)
            {
                pdfDocument.SetPageSize(reader.GetPageSizeWithRotation(page));
                pdfDocument.NewPage();
                var importedPage = writer.GetImportedPage(reader, page);
                var pageRotation = reader.GetPageRotation(page);
                var pageWidth = reader.GetPageSizeWithRotation(page).Width;
                var pageHeight = reader.GetPageSizeWithRotation(page).Height;
                switch (pageRotation)
                {
                    case 0:
                        writer.DirectContent.AddTemplate(importedPage, 1f, 0, 0, 1f, 0, 0);
                        break;

                    case 90:
                        writer.DirectContent.AddTemplate(importedPage, 0, -1f, 1f, 0, 0, pageHeight);
                        break;

                    case 180:
                        writer.DirectContent.AddTemplate(
                            importedPage, -1f, 0, 0, -1f, pageWidth, pageHeight);
                        break;

                    case 270:
                        writer.DirectContent.AddTemplate(importedPage, 0, 1f, -1f, 0, pageWidth, 0);
                        break;
                }
                pdfDocument.SetPageSize(pdfDocument.PageSize);

                logo.Alignment = Element.ALIGN_CENTER;
                logo.ScalePercent(70,70);
				logo.SetAbsolutePosition(5, reader.GetPageSizeWithRotation(page).Height - logo.ScaledHeight-15);
                writer.DirectContent.AddImage(logo);

				var obj = db.CoEs.FirstOrDefault(x => x.CoE_ID == coeID);
				var type = obj.GetType();
				var isDraft = (bool)(type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Draft").GetValue(obj, null) ?? false);
				if (isDraft)
				{
					picDraft.Alignment = Element.ALIGN_CENTER;
					picDraft.ScalePercent(70, 70);
					picDraft.SetAbsolutePosition(reader.GetPageSizeWithRotation(page).Width / 4, 0);
					writer.DirectContent.AddImage(picDraft);
				}

                //logoOPEO.Alignment = Element.ALIGN_CENTER;
                //logoOPEO.ScalePercent(20, 20);
                //logoOPEO.SetAbsolutePosition(reader.GetPageSizeWithRotation(page).Width - logoOPEO.ScaledWidth - 5, reader.GetPageSizeWithRotation(page).Height - logoOPEO.ScaledHeight - 5);
                //writer.DirectContent.AddImage(logoOPEO);

                if (page == 1)
                {
                    footer.Alignment = Element.ALIGN_CENTER;
                    footer.ScalePercent(45, 45);
                    footer.SetAbsolutePosition(reader.GetPageSizeWithRotation(page).Width - footer.ScaledWidth - 6, 10);
                    writer.DirectContent.AddImage(footer);
                }
            }

            writer.CloseStream = false;
            pdfDocument.Close();

            outStream.WriteTo(httpResponse.OutputStream);
            outStream.Close();
            httpResponse.End();

            return View(viewModel);
        }

		public ActionResult viewPRDblPdf(Int16 fiscalYear, Int16? coeID)
		{
			var allCoEs = db.CoEs.ToList();
			if (coeID != 0 && coeID != null)
			{
				allCoEs = allCoEs.Where(x => x.CoE_ID == coeID).ToList();
			}
			else
			{
				allCoEs = allCoEs.Where(x => x.CoE_ID != 0).ToList();
			}

			var allMaps = new List<Indicator_CoE_Maps>();
			allMaps = db.Indicator_CoE_Maps.ToList();
			ModelState.Clear();
			var viewModel = new PRViewModel
			{
				//allCoEs = db.CoEs.ToList(),
				allCoEs = allCoEs,
				allAnalysts = db.Analysts.ToList(),
				allMaps = allMaps,
				allFootnoteMaps = db.Indicator_Footnote_Maps.ToList(),
				allFootnotes = db.Footnotes.ToList(),
				Fiscal_Year = fiscalYear,
				Analyst_ID = null,
				allColors = db.Color_Types.ToList(),
				allIndicators = db.Indicators.ToList()
			};

			HttpResponse httpResponse = this.HttpContext.ApplicationInstance.Context.Response;
			httpResponse.Clear();
			httpResponse.ContentType = "application/pdf";
			httpResponse.AddHeader("content-disposition", "attachment;filename=\"test.pdf\"");

			MemoryStream memoryStream = new MemoryStream();
			string apiKey = "2429a8e1-7cf6-4a77-9f7f-f4a85a9fcc14";
			var test = (this.RenderView("viewPRSimpleDbl", viewModel));
			string value = "<meta charset='UTF-8' />" + test;
			using (var client = new WebClient())
			{
				NameValueCollection options = new NameValueCollection();
				options.Add("apikey", apiKey);
				options.Add("value", value);
				options.Add("DisableJavascript", "false");
				options.Add("PageSize", "Legal");
				options.Add("UseLandscape", "true");
				options.Add("Zoom", "1.1");
				options.Add("MarginLeft", "2");
				options.Add("MarginTop", "5");
				options.Add("MarginBottomn", "1");
				options.Add("MarginRight", "2");
				//options.Add("HeaderUrl", this.HttpContext.ApplicationInstance.Server.MapPath("viewPRSimple_Header"));
				byte[] result = client.UploadValues("http://api.html2pdfrocket.com/pdf", options);
				//httpResponse.BinaryWrite(result);
				memoryStream.Write(result, 0, result.Length);
			}

			string picPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/SHSheader.png");
			Image logo = Image.GetInstance(picPath);
			//string picPathOPEO = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/logoOPEO.png");
			//Image logoOPEO = Image.GetInstance(picPathOPEO);
			string footerPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/footer.png");
			Image footer = Image.GetInstance(footerPath);
			string picMonthlyPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/Monthly.png");
			string picQuaterlyPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/quaterly.png");
			string picNAPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/na.png");
			string picTargetPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/target.png");
			string picDraftPath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/draft.png");
			Image picDraft = Image.GetInstance(picDraftPath);

			var pdfDocument = new iTextSharp.text.Document();
			var outStream = new MemoryStream();
			var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(pdfDocument, outStream);

			pdfDocument.Open();
			var reader = new iTextSharp.text.pdf.PdfReader(memoryStream.ToArray());

			for (var page = 1; page <= reader.NumberOfPages; page++)
			{
				pdfDocument.SetPageSize(reader.GetPageSizeWithRotation(page));
				pdfDocument.NewPage();
				var importedPage = writer.GetImportedPage(reader, page);
				var pageRotation = reader.GetPageRotation(page);
				var pageWidth = reader.GetPageSizeWithRotation(page).Width;
				var pageHeight = reader.GetPageSizeWithRotation(page).Height;
				switch (pageRotation)
				{
					case 0:
						writer.DirectContent.AddTemplate(importedPage, 1f, 0, 0, 1f, 0, 0);
						break;

					case 90:
						writer.DirectContent.AddTemplate(importedPage, 0, -1f, 1f, 0, 0, pageHeight);
						break;

					case 180:
						writer.DirectContent.AddTemplate(
							importedPage, -1f, 0, 0, -1f, pageWidth, pageHeight);
						break;

					case 270:
						writer.DirectContent.AddTemplate(importedPage, 0, 1f, -1f, 0, pageWidth, 0);
						break;
				}
				pdfDocument.SetPageSize(pdfDocument.PageSize);

				logo.Alignment = Element.ALIGN_CENTER;
				logo.ScalePercent(70, 70);
				logo.SetAbsolutePosition(5, reader.GetPageSizeWithRotation(page).Height - logo.ScaledHeight);
				writer.DirectContent.AddImage(logo);

				var obj = db.CoEs.FirstOrDefault(x => x.CoE_ID == coeID);
				var type = obj.GetType();
				var isDraft = (bool)(type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Draft").GetValue(obj, null) ?? false);
				if (isDraft)
				{
					picDraft.Alignment = Element.ALIGN_CENTER;
					picDraft.ScalePercent(70, 70);
					picDraft.SetAbsolutePosition(reader.GetPageSizeWithRotation(page).Width / 4, 0);
					writer.DirectContent.AddImage(picDraft);
				}

				//logoOPEO.Alignment = Element.ALIGN_CENTER;
				//logoOPEO.ScalePercent(20, 20);
				//logoOPEO.SetAbsolutePosition(reader.GetPageSizeWithRotation(page).Width - logoOPEO.ScaledWidth - 5, reader.GetPageSizeWithRotation(page).Height - logoOPEO.ScaledHeight - 5);
				//writer.DirectContent.AddImage(logoOPEO);

				if (page == 1)
				{
					footer.Alignment = Element.ALIGN_CENTER;
					footer.ScalePercent(45, 45);
					footer.SetAbsolutePosition(reader.GetPageSizeWithRotation(page).Width - footer.ScaledWidth - 6, 10);
					writer.DirectContent.AddImage(footer);
				}
			}

			writer.CloseStream = false;
			pdfDocument.Close();

			outStream.WriteTo(httpResponse.OutputStream);
			outStream.Close();
			httpResponse.End();

			return View(viewModel);
		}

        public void deleteCoEMaps(Int16 mapID)
        {
            var delMap = db.Indicator_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);
            db.Entry(delMap).State = EntityState.Deleted;
            db.SaveChanges();
        }

		public void deleteAreaCoEMap(Int16 mapID)
		{
			var delMap = db.Area_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);
			db.Entry(delMap).State = EntityState.Deleted;
			db.SaveChanges();
		}

		public void deleteIndicator(Int16 indicatorID)
		{
			var delIndicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
			db.Entry(delIndicator).State = EntityState.Deleted;
			db.SaveChanges();
		}

        public JsonResult addNValues(Int16 indicatorID, Int16 fiscalYear)
        {
            var indicatorNValue = db.Indicators.FirstOrDefault(x => x.Indicator_N_Value == true && x.Indicator_N_Value_ID == indicatorID);
			var title = "N-Values: " + db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID).Indicator;
            if (indicatorNValue == null)
            {
                indicatorNValue = new Indicators()
                {
                    Indicator_N_Value = true,
                    Indicator_N_Value_ID = indicatorID,
					Indicator = title
                };
                db.Indicators.Add(indicatorNValue);
                db.SaveChanges();
                ModelState.Clear();
            }
            var indicatorNValueMap = new Indicator_CoE_Maps(){
                Fiscal_Year = fiscalYear,
                CoE_ID = 0,
            };
            if (!db.Indicator_CoE_Maps.Any(x => x.Indicator.Indicator_N_Value_ID == indicatorID && x.Fiscal_Year == fiscalYear))
            {
                indicatorNValueMap.Indicator_ID = indicatorNValue.Indicator_ID;
                db.Indicator_CoE_Maps.Add(indicatorNValueMap);
                db.SaveChanges();
            }

            return Json(new { indicatorID = indicatorNValue.Indicator_ID, mapID = indicatorNValueMap.Map_ID, coeID = indicatorNValueMap.CoE_ID, areaID = indicatorNValueMap.Indicator.Area_ID }, JsonRequestBehavior.AllowGet);
        }
		public void removeNValues(Int16 indicatorID, Int16 fiscalYear)
		{
			var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_N_Value_ID == indicatorID);
			
			var indicatorMap = db.Indicator_CoE_Maps.FirstOrDefault(x=>x.Indicator.Indicator_ID == indicator.Indicator_ID);

            db.Entry(indicatorMap).State = EntityState.Deleted;
            db.SaveChanges();
		}

        public void unmergeCell(Int16 indicatorID, string startField, List<String> allFields)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
            var type = indicator.GetType();
            bool start = false; 
            foreach (var field in allFields)
            {
                var property = type.GetProperty(field);
                try
                {
                    var value = (string)property.GetValue(indicator, null);
                    if (start & value != "=")
                    {
                        db.Entry(indicator).State = EntityState.Modified;
                        db.SaveChanges();
                        break;
                    }
                    if (property.Name == startField)
                    {
                        start = true;
                    }
                    if (start & value == "=")
                    {
                        property.SetValue(indicator, "", null);
                    }
                }
                catch (Exception)
                {
                    //intentional empty catch
                    //if unable to convert to string we ignore the property and go to the next property
                }
            }
        }

        public void setNewORder(List<Int16> newOrder, Int16[] areaIDs)
        {
            Int16 i = 1;
            foreach (var mapID in newOrder)
            {
                var areaID = areaIDs[i - 1];
                var map = db.Indicator_CoE_Maps.FirstOrDefault(x=>x.Map_ID == mapID);
                map.Number = i;
                map.Indicator.Area_ID = areaID;
                db.Entry(map).State = EntityState.Modified;
                i++;
            }
            db.SaveChanges();
        }

        public void moveCoEMapUp(Int16 mapID, Int16 fiscalYear, Int16? areaChange)
        {
            var moveMap = db.Indicator_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);
            var currNum = moveMap.Number;

            if (!areaChange.HasValue)
            {
                var newNum = db.Indicator_CoE_Maps.Where(x => x.Number < currNum &&
                                                         x.CoE_ID == moveMap.CoE_ID &&
                                                         x.Fiscal_Year == fiscalYear &&
                                                         x.Indicator.Area_ID == moveMap.Indicator.Area_ID).Max(x => x.Number);
                var replaceMap = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == moveMap.CoE_ID &&
                                                             x.Fiscal_Year == fiscalYear &&
                                                             x.Indicator.Area_ID == moveMap.Indicator.Area_ID).FirstOrDefault(x => x.Number == newNum);

                moveMap.Number = newNum;
                replaceMap.Number = currNum;
                db.Entry(moveMap).State = EntityState.Modified;
                db.Entry(replaceMap).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Int16 newNum;
                Int16 replaceNum;

                var newAreaSort = db.Areas.FirstOrDefault(x => x.Area_ID == moveMap.Indicator.Area_ID).Sort + areaChange;
                var newArea = db.Areas.FirstOrDefault(x => x.Sort == newAreaSort).Area_ID;
                if (newArea != null)
                {

                    var newMap = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == moveMap.CoE_ID &&
                                                x.Fiscal_Year == fiscalYear &&
                                                x.Indicator.Area_ID == newArea);
                    if (newMap.Count() == 0)
                    {
                        newNum = moveMap.Number;
                    }
                    else
                    {
                        newNum = newMap.Max(x => x.Number);
                        replaceNum = (Int16)(newNum - 1);

                        var replaceMap = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == moveMap.CoE_ID &&
                                                                        x.Fiscal_Year == fiscalYear &&
                                                                        x.Indicator.Area_ID == newArea).FirstOrDefault(x => x.Number == newNum);
                        replaceMap.Number = replaceNum;
                        db.Entry(replaceMap).State = EntityState.Modified;
                    }

                    moveMap.Number = newNum;
                    db.Entry(moveMap).State = EntityState.Modified;
                    db.SaveChanges();

                    var moveIndicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == moveMap.Indicator_ID);
                    moveIndicator.Area_ID = newArea;
                    db.Entry(moveIndicator).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void moveCoEMapDown(Int16 mapID, Int16 fiscalYear, Int16? areaChange)
        {
            var moveMap = db.Indicator_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);
            var currNum = moveMap.Number;

            if (!areaChange.HasValue)
            {
                var newNum = db.Indicator_CoE_Maps.Where(x => x.Number > currNum &&
                                                            x.CoE_ID == moveMap.CoE_ID &&
                                                            x.Fiscal_Year == fiscalYear &&
                                                            x.Indicator.Area_ID == moveMap.Indicator.Area_ID).Min(x => x.Number);
                var replaceMap = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == moveMap.CoE_ID &&
                                                                x.Fiscal_Year == fiscalYear &&
                                                                x.Indicator.Area_ID == moveMap.Indicator.Area_ID).FirstOrDefault(x => x.Number == newNum);
                moveMap.Number = newNum;
                replaceMap.Number = currNum;
                db.Entry(moveMap).State = EntityState.Modified;
                db.Entry(replaceMap).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Int16 newNum;
                Int16 replaceNum;

                var newAreaSort = db.Areas.FirstOrDefault(x => x.Area_ID == moveMap.Indicator.Area_ID).Sort + areaChange;
                var newArea = db.Areas.FirstOrDefault(x => x.Sort == newAreaSort).Area_ID;
                if (newArea != null)
                {

                    var newMap = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == moveMap.CoE_ID &&
                                                x.Fiscal_Year == fiscalYear &&
                                                x.Indicator.Area_ID == newArea);
                    if (newMap.Count() == 0)
                    {
                        newNum = moveMap.Number;
                    }
                    else
                    {
                        newNum = newMap.Min(x => x.Number);
                        replaceNum = (Int16)(newNum + 1);

                        var replaceMap = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == moveMap.CoE_ID &&
                                                                        x.Fiscal_Year == fiscalYear &&
                                                                        x.Indicator.Area_ID == newArea).FirstOrDefault(x => x.Number == newNum);
                        replaceMap.Number = replaceNum;
                        db.Entry(replaceMap).State = EntityState.Modified;
                    }

                    moveMap.Number = newNum;
                    db.Entry(moveMap).State = EntityState.Modified;
                    db.SaveChanges();

                    var moveIndicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == moveMap.Indicator_ID);
                    moveIndicator.Area_ID = newArea;
                    db.Entry(moveIndicator).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public ActionResult editCoEMaps(Int16 fiscalYear)
        {
            var viewModel = new Indicator_CoE_MapsViewModel
            {
                allIndicators = db.Indicators.ToList(),
                allCoEs = db.CoEs.ToList(),
                allMaps = db.Indicator_CoE_Maps.Where(x => x.Fiscal_Year == fiscalYear).ToList(),
                fiscalYear = fiscalYear,
            };
            return View(viewModel);
        }

        [HttpPost]
        public void editCoEMaps(IList<Indicator_CoE_MapsViewModel> mapChange)
        {
            Indicator_CoE_Maps map = mapChange.FirstOrDefault().allMaps.FirstOrDefault();
            if (map.Fiscal_Year == 0)
            {
                map.Fiscal_Year = db.Indicator_CoE_Maps.Max(x => x.Fiscal_Year);
            }

            Indicator_CoE_Maps existingMap = db.Indicator_CoE_Maps.Where(x => x.Indicator_ID == map.Indicator_ID && x.CoE_ID == map.CoE_ID).FirstOrDefault();
            if (existingMap != null)
            {
                map.Map_ID = existingMap.Map_ID;
            }
            if (map.Map_ID != 0)
            {
                var mapID = map.Map_ID;
                var deleteMap = db.Indicator_CoE_Maps.Find(mapID);
                if (deleteMap != null)
                {
                    db.Indicator_CoE_Maps.Remove(deleteMap);
                    db.SaveChanges();
                }
            }
            else
            {
                db.Indicator_CoE_Maps.Add(map);
                db.SaveChanges();
            }
        }

		[HttpGet]
		public JsonResult editObjectives(Int16 mapID)
		{
			var objectiveMap = db.Area_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);

			var objectives = Regex.Matches(objectiveMap.Objective, @"\[.*?\]").Cast<Match>().Select(m => m.Value.Substring(1,m.Value.Length - 2)).ToArray();

			var viewModel = objectives.Select(x => new ObjectiveViewModel
			{
				Map_ID = mapID,
				Objective = x,
			}).ToList();

			// return View(viewModel);
			return Json(viewModel, JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult editObjectives(Int16 mapID, string[] objectives)
		{
			var objectiveString = "";
			foreach (var obj in objectives)
			{
				if (obj != null && obj.Length > 0)
				{
					objectiveString += "[" + obj + "]";
				}
			}

			var map = db.Area_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);
			map.Objective = objectiveString;
			db.Entry(map).State = EntityState.Modified;
			db.SaveChanges();

			return Json(new {objectiveString = objectiveString}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult editIndicator_Links(Int16 fiscalYear)
		{
			var viewModel = new Edit_Indicator_LinksViewModel()
			{
				allIndicator_Links = db.Indicator_Links.ToList(),
				allIndicators = db.Indicators.OrderBy(x=>x.Indicator).Where(x=>x.Indicator != "").ToList(),
				allFields = db.Link_Fields.ToList(),
				Fiscal_Year = fiscalYear
			};

			return View(viewModel);
		}


		public ActionResult viewIndicator_Links(Int16 fiscalYear)
		{
			var viewModel = new Edit_Indicator_LinksViewModel()
			{
				allIndicator_Links = db.Indicator_Links.ToList(),
				allIndicators = db.Indicators.OrderBy(x => x.Indicator).Where(x => x.Indicator != "").ToList(),
				allFields = db.Link_Fields.ToList(),
				Fiscal_Year = fiscalYear
			};

			return View(viewModel);
		}

		[HttpPost]
		public JsonResult addIndicatorLinks(int[] Indicator_ID_List, int[] Field_ID_List)
		{
			var newLink = db.Indicator_Links.Create();
			db.Indicator_Links.Add(newLink);
			db.SaveChanges();

			foreach(var fieldID in Field_ID_List){
				var newLinkField = db.Indicator_Link_Fields.Create();
				newLinkField.Link_ID = newLink.Link_ID;
				newLinkField.Link_Field_ID = (Int16)fieldID;
				db.Indicator_Link_Fields.Add(newLinkField);
				db.SaveChanges();
			}

			foreach (var indicatorID in Indicator_ID_List)
			{
				var newLinkIndicator = db.Indicator_Link_Indicators.Create();
				newLinkIndicator.Link_ID = newLink.Link_ID;
				newLinkIndicator.Indicator_ID = (Int16)indicatorID;
				db.Indicator_Link_Indicators.Add(newLinkIndicator);
				db.SaveChanges();
			}
			return Json("");
		}

		[HttpPost]
		public JsonResult deleteIndicatorLink(Int16 linkID)
		{
			var link = db.Indicator_Links.FirstOrDefault(x => x.Link_ID == linkID);
			db.Indicator_Links.Remove(link);
			db.SaveChanges();

			return Json("");
		}

		[HttpGet]
		public JsonResult editCoE_Notes(Int16 coeID)
		{
			var coe = db.CoEs.FirstOrDefault(x => x.CoE_ID == coeID);

			string[] coeNotes;
			if (coe.CoE_Notes != null)
			{
				coeNotes = Regex.Matches(coe.CoE_Notes, @"\[.*?\]").Cast<Match>().Select(m => m.Value.Substring(1, m.Value.Length - 2)).ToArray();
			}
			else
			{
				coeNotes = new string[] {""};
			}

			var viewModel = coeNotes.Select(x => new CoE_NoteViewModel
			{
				CoE_ID = coeID,
				CoE_Note = x,
			}).ToList();

			// return View(viewModel);
			return Json(viewModel, JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult editCoE_Notes(Int16 coeID, string[] coeNotes)
		{
			var coeNoteString = "";
			foreach (var obj in coeNotes)
			{
				if (obj != null && obj.Length > 0)
				{
					coeNoteString += "[" + obj + "]";
				}
			}

			var coe = db.CoEs.FirstOrDefault(x => x.CoE_ID == coeID);
			coe.CoE_Notes = coeNoteString;
			db.Entry(coe).State = EntityState.Modified;
			db.SaveChanges();

			return Json(new { coeNoteString = coeNoteString }, JsonRequestBehavior.AllowGet);
		}

        [HttpGet]
        public ActionResult editFootnotes(String Footnote_ID_Filter)
        {
            var viewModelItems = db.Footnotes.ToArray();
            var viewModel = viewModelItems.OrderBy(x => x.Footnote_ID).Select(x => new FootnotesViewModel
            {
                Footnote_ID = x.Footnote_ID,
                Footnote = x.Footnote,
                Footnote_Symbol = x.Footnote_Symbol,
				Footnote_Order = x.Footnote_Order
            }).ToList();
            if (Request.IsAjaxRequest())
            {
                if (Footnote_ID_Filter == "")
                {
                    var newFootnote = db.Footnotes.Create();
                    db.Footnotes.Add(newFootnote);
                    db.SaveChanges();

                    viewModel = new List<FootnotesViewModel>();
                    var newViewModelItem = new FootnotesViewModel
                    {
                        Footnote_ID = newFootnote.Footnote_ID,
                        Footnote = newFootnote.Footnote,
                        Footnote_Symbol = newFootnote.Footnote_Symbol,
						Footnote_Order = newFootnote.Footnote_Order
                    };
                    viewModel.Add(newViewModelItem);

                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(viewModel.Where(x => x.Footnote_ID.ToString().Contains(Footnote_ID_Filter == null ? "" : Footnote_ID_Filter)), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return View(viewModel);
            }

        }

        [HttpPost]
        public void deleteFootnotes(Int16 footnoteID)
        {
            var deleteFootnote = db.Footnotes.FirstOrDefault(x => x.Footnote_ID == footnoteID);
            db.Footnotes.Remove(deleteFootnote);
            db.SaveChanges();
        }

        [HttpPost]
        public ActionResult editFootnotes(IList<Footnotes> footnoteChange)
        {
            var footnoteID = footnoteChange[0].Footnote_ID;
            if (db.Footnotes.Any(x => x.Footnote_ID == footnoteID))
            {
                if (ModelState.IsValid)
                {
                    db.Entry(footnoteChange[0]).State = EntityState.Modified;
                    db.SaveChanges();
                    return View();
                }
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    db.Footnotes.Add(footnoteChange[0]);
                    db.SaveChanges();
                    return View();
                }
                return View();
            }

        }


		[HttpGet]
		public ActionResult editWords(String Word_ID_Filter)
		{
			var viewModelItems = db.Words.ToList();
			var viewModel = viewModelItems.OrderBy(x => x.ID).Select(x => new WordsViewModel
			{
				ID = x.ID,
				Word = x.Word
			}).ToList();
			if (Request.IsAjaxRequest())
			{
				if (Word_ID_Filter == "")
				{
					var newWord = db.Words.Create();
					db.Words.Add(newWord);
					db.SaveChanges();

					viewModel = new List<WordsViewModel>();
					var newViewModelItem = new WordsViewModel
					{
						ID = newWord.ID,
						Word = newWord.Word
					};
					viewModel.Add(newViewModelItem);

					return Json(viewModel, JsonRequestBehavior.AllowGet);
				}
				else
				{
					return Json(viewModel.Where(x => x.ID.ToString().Contains(Word_ID_Filter == null ? "" : Word_ID_Filter)), JsonRequestBehavior.AllowGet);
				}
			}
			else
			{
				return View(viewModel);
			}

		}

		[HttpPost]
		public void deleteWords(Int16 wordID)
		{
			var deleteWord = db.Words.FirstOrDefault(x => x.ID == wordID);
			db.Words.Remove(deleteWord);
			db.SaveChanges();
		}

		[HttpPost]
		public ActionResult editWords(IList<Words> wordChange)
		{
			var wordID = wordChange[0].ID;
			if (db.Words.Any(x => x.ID == wordID))
			{
				if (ModelState.IsValid)
				{
					db.Entry(wordChange[0]).State = EntityState.Modified;
					db.SaveChanges();
					return View();
				}
				return View();
			}
			else
			{
				if (ModelState.IsValid)
				{
					db.Words.Add(wordChange[0]);
					db.SaveChanges();
					return View();
				}
				return View();
			}

		}

        [HttpGet]
		public ActionResult editCoEs(String CoE_ID_Filter)
        {
            var viewModelItems = db.CoEs.ToArray();
            var viewModel = viewModelItems.OrderBy(x => x.CoE_ID).Select(x => new CoEsViewModel
            {
                CoE_ID = x.CoE_ID,
                CoE = x.CoE,
                CoE_Abbr = x.CoE_Abbr,
                CoE_Notes = x.CoE_Notes,
                CoE_Subtitle = x.CoE_Subtitle,
                CoE_Type = x.CoE_Type,
				CoE_Title = x.CoE_Title
            }).ToList();
            if (Request.IsAjaxRequest())
            {
                if (CoE_ID_Filter == "")
                {
                    var newCoE = db.CoEs.Create();
					newCoE.CoE_Abbr = "";
                    db.CoEs.Add(newCoE);
                    db.SaveChanges();


					var newIndicator = new Indicators();
					var newAreaCoEMap = new Area_CoE_Maps();
					var newIndicatorCoEMap = new Indicator_CoE_Maps();

					var allFiscalYear = db.Fiscal_Years.ToList();
					var allAreas = db.Areas.ToList();
					foreach (var fiscalYear in allFiscalYear)
					{
						foreach (var area in allAreas)
						{
							for (var i = 1; i <= 5; i++)
							{
								newIndicator = db.Indicators.Create();
								db.Indicators.Add(newIndicator);
								db.SaveChanges();
								newIndicator.Area_ID = area.Area_ID;
								db.Entry(newIndicator).State = EntityState.Modified;
								db.SaveChanges();

								newIndicatorCoEMap = new Indicator_CoE_Maps
								{
									CoE_ID = newCoE.CoE_ID,
									Fiscal_Year = fiscalYear.Fiscal_Year_Code,
									Indicator_ID = newIndicator.Indicator_ID
								};
								db.Indicator_CoE_Maps.Add(newIndicatorCoEMap);
								db.SaveChanges();
							}
							newAreaCoEMap = new Area_CoE_Maps{
								Area_ID = area.Area_ID,
								CoE_ID = newCoE.CoE_ID,
								Fiscal_Year = fiscalYear.Fiscal_Year_Code,
								Objective = ""
							};
							db.Area_CoE_Maps.Add(newAreaCoEMap);
							db.SaveChanges();
						}
					}


                    viewModel = new List<CoEsViewModel>();
                    var newViewModelItem = new CoEsViewModel
                    {
                        CoE_ID = newCoE.CoE_ID,
                        CoE = newCoE.CoE,
                        CoE_Abbr = newCoE.CoE_Abbr,
                        CoE_Notes = newCoE.CoE_Notes,
                        CoE_Subtitle = newCoE.CoE_Subtitle,
                        CoE_Type = newCoE.CoE_Type,
						CoE_Title = newCoE.CoE_Title
                    };
                    viewModel.Add(newViewModelItem);

                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(viewModel.Where(x => x.CoE_ID.ToString().Contains(CoE_ID_Filter == null ? "" : CoE_ID_Filter)), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return View(viewModel);
            }

        }

        [HttpPost]
        public void deleteCoEs(Int16 CoEID)
        {
            var deleteCoE = db.CoEs.FirstOrDefault(x => x.CoE_ID == CoEID);
            db.CoEs.Remove(deleteCoE);
            db.SaveChanges();
        }

        [HttpPost]
        public ActionResult editCoEs(IList<CoEs> CoEChange)
        {
            var CoEID = CoEChange[0].CoE_ID;
            if (db.CoEs.Any(x => x.CoE_ID == CoEID))
            {
                if (ModelState.IsValid)
                {
					// CoEChange[0].CoE_Abbr = "";
                    db.Entry(CoEChange[0]).State = EntityState.Modified;
                    db.SaveChanges();
					return new EmptyResult();
                }
				return new EmptyResult();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    db.CoEs.Add(CoEChange[0]);

					
					return new EmptyResult();
                }
				return new EmptyResult();
            }

        }

        [HttpGet]
        public ActionResult editAnalysts(String Analyst_ID_Filter)
        {
            var viewModelItems = db.Analysts.ToArray();
            var viewModel = viewModelItems.OrderBy(x => x.Analyst_ID).Select(x => new AnalystViewModel
            {
                Analyst_ID = x.Analyst_ID,
                First_Name = x.First_Name,
                Last_Name = x.Last_Name,
                Position = x.Position,
                Order = x.Order,
            }).ToList();
            if (Request.IsAjaxRequest())
            {
                if (Analyst_ID_Filter == "")
                {
                    var newAnalyst = db.Analysts.Create();
                    db.Analysts.Add(newAnalyst);
                    db.SaveChanges();

                    viewModel = new List<AnalystViewModel>();
                    var newViewModelItem = new AnalystViewModel
                    {
                        Analyst_ID = newAnalyst.Analyst_ID,
                        First_Name = newAnalyst.First_Name,
                        Last_Name = newAnalyst.Last_Name,
                        Position = newAnalyst.Position,
                        Order = newAnalyst.Order,
                    };
                    viewModel.Add(newViewModelItem);

                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(viewModel.Where(x => x.Analyst_ID.ToString().Contains(Analyst_ID_Filter == null ? "" : Analyst_ID_Filter)), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return View(viewModel);
            }

        }

        [HttpPost]
        public void deleteAnalyst(Int16 analystID)
        {
            var deleteAnalyst = db.Analysts.FirstOrDefault(x => x.Analyst_ID == analystID);
            db.Analysts.Remove(deleteAnalyst);
            db.SaveChanges();
        }

        [HttpPost]
        public ActionResult editAnalysts(IList<Analysts> analystChange)
        {
            var analystID = analystChange[0].Analyst_ID;
            if (db.Analysts.Any(x => x.Analyst_ID == analystID))
            {
                if (ModelState.IsValid)
                {
                    db.Entry(analystChange[0]).State = EntityState.Modified;
                    db.SaveChanges();
                    return View();
                }
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    db.Analysts.Add(analystChange[0]);
                    db.SaveChanges();
                    return View();
                }
                return View();
            }

        }

		public ActionResult Query1(Int16 fiscalYear)
		{
			var allIndicators = db.Indicators.ToList();
			var allDirections = db.Color_Directions.ToList();
			foreach (var indicator in allIndicators)
			{

				var target = (string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(indicator, null);
				if (target != null)
				{
					if(target.Length>1){
						var direction = target.Substring(0,1);
						Int16 directionID = 0;
						foreach (var directionO in allDirections)
						{
							if (directionO.Direction == direction)
							{
								directionID = directionO.Direction_ID;
							}
						}

						if (directionID > 0)
						{
							indicator.FY_13_14_Direction_ID = directionID;
							indicator.FY_14_15_Direction_ID = directionID;
							indicator.FY_15_16_Direction_ID = directionID;
							db.Entry(indicator).State = EntityState.Modified;
							db.SaveChanges();
						}
					}
				}
			}

			return null;
		}

		public ActionResult Query2()
		{
			var allIndicators = db.Indicators.ToList();
			foreach (var indicator in allIndicators)
			{
				if (indicator.FY_13_14_Direction_ID == 0)
				{
					indicator.FY_13_14_Direction_ID = 1;
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
				if (indicator.FY_14_15_Direction_ID == 0)
				{
					indicator.FY_14_15_Direction_ID = 1;
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
				if (indicator.FY_15_16_Direction_ID == 0)
				{
					indicator.FY_15_16_Direction_ID = 1;
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
				if (indicator.FY_13_14_Color_ID == 0)
				{
					indicator.FY_13_14_Color_ID = 1;
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
				if (indicator.FY_14_15_Color_ID == 0)
				{
					indicator.FY_14_15_Color_ID = 1;
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
				if (indicator.FY_15_16_Color_ID == 0)
				{
					indicator.FY_15_16_Color_ID = 1;
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
			}

			return null;
		}

		public ActionResult Query3()
		{
			var allIndicators = db.Indicators.ToList();
			foreach (var indicator in allIndicators)
			{
				indicator.FY_10_11_YTD = indicator.FY_10_11_YTD != null ? indicator.FY_10_11_YTD.Replace("NULL", "") : null;
				indicator.FY_10_11_YTD_Sup = indicator.FY_10_11_YTD_Sup != null ? indicator.FY_10_11_YTD_Sup.Replace("NULL", "") : null;
				indicator.FY_11_12_YTD = indicator.FY_11_12_YTD != null ? indicator.FY_11_12_YTD.Replace("NULL", "") : null;
				indicator.FY_11_12_YTD_Sup = indicator.FY_11_12_YTD_Sup != null ? indicator.FY_11_12_YTD_Sup.Replace("NULL", "") : null;
				indicator.FY_12_13_YTD = indicator.FY_12_13_YTD != null ? indicator.FY_12_13_YTD.Replace("NULL", "") : null;
				indicator.FY_12_13_YTD_Sup = indicator.FY_12_13_YTD_Sup != null ? indicator.FY_12_13_YTD_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Q1 = indicator.FY_13_14_Q1 != null ? indicator.FY_13_14_Q1.Replace("NULL", "") : null;
				indicator.FY_13_14_Q1_Sup = indicator.FY_13_14_Q1_Sup != null ? indicator.FY_13_14_Q1_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Q2 = indicator.FY_13_14_Q2 != null ? indicator.FY_13_14_Q2.Replace("NULL", "") : null;
				indicator.FY_13_14_Q2_Sup = indicator.FY_13_14_Q2_Sup != null ? indicator.FY_13_14_Q2_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Q3 = indicator.FY_13_14_Q3 != null ? indicator.FY_13_14_Q3.Replace("NULL", "") : null;
				indicator.FY_13_14_Q3_Sup = indicator.FY_13_14_Q3_Sup != null ? indicator.FY_13_14_Q3_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Q4 = indicator.FY_13_14_Q4 != null ? indicator.FY_13_14_Q4.Replace("NULL", "") : null;
				indicator.FY_13_14_Q4_Sup = indicator.FY_13_14_Q4_Sup != null ? indicator.FY_13_14_Q4_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_YTD = indicator.FY_13_14_YTD != null ? indicator.FY_13_14_YTD.Replace("NULL", "") : null;
				indicator.FY_13_14_YTD_Sup = indicator.FY_13_14_YTD_Sup != null ? indicator.FY_13_14_YTD_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Target = indicator.FY_13_14_Target != null ? indicator.FY_13_14_Target.Replace("NULL", "") : null;
				indicator.FY_13_14_Target_Sup = indicator.FY_13_14_Target_Sup != null ? indicator.FY_13_14_Target_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Comparator = indicator.FY_13_14_Comparator != null ? indicator.FY_13_14_Comparator.Replace("NULL", "") : null;
				indicator.FY_13_14_Comparator_Sup = indicator.FY_13_14_Comparator_Sup != null ? indicator.FY_13_14_Comparator_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Performance_Threshold_Sup = indicator.FY_13_14_Performance_Threshold_Sup != null ? indicator.FY_13_14_Performance_Threshold_Sup.Replace("NULL", "") : null;
				indicator.FY_13_14_Definition_Calculation = indicator.FY_13_14_Definition_Calculation != null ? indicator.FY_13_14_Definition_Calculation.Replace("NULL", "") : null;
				indicator.FY_13_14_Target_Rationale = indicator.FY_13_14_Target_Rationale != null ? indicator.FY_13_14_Target_Rationale.Replace("NULL", "") : null;
				indicator.FY_13_14_Comparator_Source = indicator.FY_13_14_Comparator_Source != null ? indicator.FY_13_14_Comparator_Source.Replace("NULL", "") : null;
				indicator.FY_13_14_Data_Source_MSH = indicator.FY_13_14_Data_Source_MSH != null ? indicator.FY_13_14_Data_Source_MSH.Replace("NULL", "") : null;
				indicator.FY_13_14_Data_Source_Benchmark = indicator.FY_13_14_Data_Source_Benchmark != null ? indicator.FY_13_14_Data_Source_Benchmark.Replace("NULL", "") : null;
				indicator.FY_13_14_OPEO_Lead = indicator.FY_13_14_OPEO_Lead != null ? indicator.FY_13_14_OPEO_Lead.Replace("NULL", "") : null;
				indicator.FY_13_14_Comment = indicator.FY_13_14_Comment != null ? indicator.FY_13_14_Comment.Replace("NULL", "") : null;
				indicator.FY_14_15_Q1 = indicator.FY_14_15_Q1 != null ? indicator.FY_14_15_Q1.Replace("NULL", "") : null;
				indicator.FY_14_15_Q1_Sup = indicator.FY_14_15_Q1_Sup != null ? indicator.FY_14_15_Q1_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_Q2 = indicator.FY_14_15_Q2 != null ? indicator.FY_14_15_Q2.Replace("NULL", "") : null;
				indicator.FY_14_15_Q2_Sup = indicator.FY_14_15_Q2_Sup != null ? indicator.FY_14_15_Q2_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_Q3 = indicator.FY_14_15_Q3 != null ? indicator.FY_14_15_Q3.Replace("NULL", "") : null;
				indicator.FY_14_15_Q3_Sup = indicator.FY_14_15_Q3_Sup != null ? indicator.FY_14_15_Q3_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_Q4 = indicator.FY_14_15_Q4 != null ? indicator.FY_14_15_Q4.Replace("NULL", "") : null;
				indicator.FY_14_15_Q4_Sup = indicator.FY_14_15_Q4_Sup != null ? indicator.FY_14_15_Q4_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_YTD = indicator.FY_14_15_YTD != null ? indicator.FY_14_15_YTD.Replace("NULL", "") : null;
				indicator.FY_14_15_YTD_Sup = indicator.FY_14_15_YTD_Sup != null ? indicator.FY_14_15_YTD_Sup.Replace("NULL", "") : null; 
				indicator.FY_14_15_Target = indicator.FY_14_15_Target != null ? indicator.FY_14_15_Target.Replace("NULL", "") : null;
				indicator.FY_14_15_Target_Sup = indicator.FY_14_15_Target_Sup != null ? indicator.FY_14_15_Target_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_Comparator = indicator.FY_14_15_Comparator != null ? indicator.FY_14_15_Comparator.Replace("NULL", "") : null;
				indicator.FY_14_15_Comparator_Sup = indicator.FY_14_15_Comparator_Sup != null ? indicator.FY_14_15_Comparator_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_Performance_Threshold_Sup = indicator.FY_14_15_Performance_Threshold_Sup != null ? indicator.FY_14_15_Performance_Threshold_Sup.Replace("NULL", "") : null;
				indicator.FY_14_15_Definition_Calculation = indicator.FY_14_15_Definition_Calculation != null ? indicator.FY_14_15_Definition_Calculation.Replace("NULL", "") : null;
				indicator.FY_14_15_Target_Rationale = indicator.FY_14_15_Target_Rationale != null ? indicator.FY_14_15_Target_Rationale.Replace("NULL", "") : null;
				indicator.FY_14_15_Comparator_Source = indicator.FY_14_15_Comparator_Source != null ? indicator.FY_14_15_Comparator_Source.Replace("NULL", "") : null;
				indicator.FY_14_15_Data_Source_MSH = indicator.FY_14_15_Data_Source_MSH != null ? indicator.FY_14_15_Data_Source_MSH.Replace("NULL", "") : null;
				indicator.FY_14_15_Data_Source_Benchmark = indicator.FY_14_15_Data_Source_Benchmark != null ? indicator.FY_14_15_Data_Source_Benchmark.Replace("NULL", "") : null;
				indicator.FY_14_15_OPEO_Lead = indicator.FY_14_15_OPEO_Lead != null ? indicator.FY_14_15_OPEO_Lead.Replace("NULL", "") : null;
				indicator.FY_14_15_Comment = indicator.FY_14_15_Comment != null ? indicator.FY_14_15_Comment.Replace("NULL", "") : null;
				indicator.FY_15_16_Q1 = indicator.FY_15_16_Q1 != null ? indicator.FY_15_16_Q1.Replace("NULL", "") : null;
				indicator.FY_15_16_Q1_Sup = indicator.FY_15_16_Q1_Sup != null ? indicator.FY_15_16_Q1_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Q2 = indicator.FY_15_16_Q2 != null ? indicator.FY_15_16_Q2.Replace("NULL", "") : null;
				indicator.FY_15_16_Q2_Sup = indicator.FY_15_16_Q2_Sup != null ? indicator.FY_15_16_Q2_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Q3 = indicator.FY_15_16_Q3 != null ? indicator.FY_15_16_Q3.Replace("NULL", "") : null;
				indicator.FY_15_16_Q3_Sup = indicator.FY_15_16_Q3_Sup != null ? indicator.FY_15_16_Q3_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Q4 = indicator.FY_15_16_Q4 != null ? indicator.FY_15_16_Q4.Replace("NULL", "") : null;
				indicator.FY_15_16_Q4_Sup = indicator.FY_15_16_Q4_Sup != null ? indicator.FY_15_16_Q4_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_YTD = indicator.FY_15_16_YTD != null ? indicator.FY_15_16_YTD.Replace("NULL", "") : null;
				indicator.FY_15_16_YTD_Sup = indicator.FY_15_16_YTD_Sup != null ? indicator.FY_15_16_YTD_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Target = indicator.FY_15_16_Target != null ? indicator.FY_15_16_Target.Replace("NULL", "") : null;
				indicator.FY_15_16_Target_Sup = indicator.FY_15_16_Target_Sup != null ? indicator.FY_15_16_Target_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Comparator = indicator.FY_15_16_Comparator != null ? indicator.FY_15_16_Comparator.Replace("NULL", "") : null;
				indicator.FY_15_16_Comparator_Sup = indicator.FY_15_16_Comparator_Sup != null ? indicator.FY_15_16_Comparator_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Performance_Threshold_Sup = indicator.FY_15_16_Performance_Threshold_Sup != null ? indicator.FY_15_16_Performance_Threshold_Sup.Replace("NULL", "") : null;
				indicator.FY_15_16_Definition_Calculation = indicator.FY_15_16_Definition_Calculation != null ? indicator.FY_15_16_Definition_Calculation.Replace("NULL", "") : null;
				indicator.FY_15_16_Target_Rationale = indicator.FY_15_16_Target_Rationale != null ? indicator.FY_15_16_Target_Rationale.Replace("NULL", "") : null;
				indicator.FY_15_16_Comparator_Source = indicator.FY_15_16_Comparator_Source != null ? indicator.FY_15_16_Comparator_Source.Replace("NULL", "") : null;
				indicator.FY_15_16_Data_Source_MSH = indicator.FY_15_16_Data_Source_MSH != null ? indicator.FY_15_16_Data_Source_MSH.Replace("NULL", "") : null;
				indicator.FY_15_16_Data_Source_Benchmark = indicator.FY_15_16_Data_Source_Benchmark != null ? indicator.FY_15_16_Data_Source_Benchmark.Replace("NULL", "") : null;
				indicator.FY_15_16_OPEO_Lead = indicator.FY_15_16_OPEO_Lead != null ? indicator.FY_15_16_OPEO_Lead.Replace("NULL", "") : null;
				indicator.FY_15_16_Comment = indicator.FY_15_16_Comment != null ? indicator.FY_15_16_Comment.Replace("NULL", "") : null;
				db.Entry(indicator).State = EntityState.Modified;
				db.SaveChanges();
			}


			return null;
		}

		[HttpGet]
		public JsonResult getFiscal_Years()
		{
			return Json(db.Fiscal_Years.OrderBy(x => x.Fiscal_Year_Code).Select(x => new { x.Fiscal_Year_Code, x.Fiscal_Year }).ToList(), JsonRequestBehavior.AllowGet);
		}

        [HttpGet]
        public JsonResult getCoEs()
        {
			var allCoEs = db.CoEs.Where(x => x.Indicator_CoE_Map.Count() == 0).Select(x => new Link_CoEViewModel
			{
				CoE_ID = x.CoE_ID,
				CoE = x.CoE,
				CoE_Title = x.CoE_Title,
				Linked_CoE = false,
				CoE_ID_1 = 0,
				CoE_ID_2 = 0
			}).ToList();
			var allLinkedCoEs = db.Link_CoEs.Select(x => new Link_CoEViewModel { 
				Linked_CoE = true,
				CoE_ID_1 = x.Link_CoE_Maps.FirstOrDefault().CoE_ID,
				CoE_ID_2 = x.Link_CoE_Maps.Skip(1).FirstOrDefault().CoE_ID,
				CoE_Title = x.CoE_Title,
				CoE = x.CoE
			}).ToList();

			allCoEs.AddRange(allLinkedCoEs);

            return Json(allCoEs.OrderBy(x => x.CoE_Title).Where(x => x.CoE_ID != 0).Select(x => new { x.CoE_ID, x.CoE, x.CoE_Title, x.CoE_ID_1, x.CoE_ID_2, x.Linked_CoE}).ToList(), JsonRequestBehavior.AllowGet);
        }

		[HttpGet]
		public ActionResult getFootnotes()
		{
			return Json(db.Footnotes.OrderBy(x => x.Footnote_Order).Select(x => new { Footnote_ID = x.Footnote_ID, Footnote = x.Footnote, Footnote_Symbol = x.Footnote_Symbol, Footnote_Order = x.Footnote_Order}).ToList(), JsonRequestBehavior.AllowGet);
		}

        [HttpGet]
        public JsonResult getAnalysts()
        {
            return Json(db.Analysts.OrderBy(x=>x.First_Name).Select(x => new { x.Analyst_ID, Analyst = x.First_Name }).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getAreaMap(Int16 mapID, Int16 fiscalYear)
        {
            var objectives = db.Area_CoE_Maps.Where(x => x.Fiscal_Year == fiscalYear).FirstOrDefault(x => x.Map_ID == mapID).Objective;
            return Json(objectives, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void setAreaMap(Int16 mapID, string objective, Int16 fiscalYear)
        {
            var map = db.Area_CoE_Maps.FirstOrDefault(x => x.Map_ID == mapID);
            map.Objective = objective;

            if (ModelState.IsValid)
            {
                db.Entry(map).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

		[HttpPost]
		public void setCoEDraft(bool isDraft, Int16 fiscalYear, Int16 coeID)
		{
			var obj = db.CoEs.FirstOrDefault(x=>x.CoE_ID == coeID);
            var type = obj.GetType();
			type.GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Draft").SetValue(obj, isDraft, null);
			db.Entry(obj).State = EntityState.Modified;
			db.SaveChanges();
		}

        public ActionResult editFootnoteMaps(Int16 fiscalYear, Int16? indicatorID)
        {
            List<Indicator_Footnote_Maps> footnoteMaps = new List<Indicator_Footnote_Maps>();
            foreach (var footnote in db.Indicator_Footnote_Maps.Where(x => x.Fiscal_Year == fiscalYear).OrderBy(e => e.Map_ID).ToList())
            {
                footnoteMaps.Add(footnote);
            }

            var allIndicator = new List<Indicators>();
            if (indicatorID.HasValue)
            {
                allIndicator = db.Indicators.Where(x => x.Indicator_ID == indicatorID).OrderBy(x => x.Indicator_ID).ToList();
            }
            else
            {
                allIndicator = db.Indicators.OrderBy(x => x.Indicator_ID).ToList();
            }

            var viewModel = allIndicator.Select(x => new Indicator_Footnote_MapsViewModel
            {
                Indicator_ID = x.Indicator_ID,
                Indicator = x.Indicator,
                Fiscal_Year = fiscalYear,
            }).ToList();

            viewModel.FirstOrDefault().allFootnotes = new List<string>();

            viewModel.FirstOrDefault().allFootnotes.AddRange(db.Footnotes.Select(x => x.Footnote_Symbol + ", " + x.Footnote).ToList());

            foreach (var Indicator in viewModel)
            {
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 0)
                {
                    Indicator.Footnote_ID_1 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).First().Footnote_ID;
                    Indicator.Map_ID_1 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).First().Map_ID;
                    Indicator.Footnote_Symbol_1 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_1).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 1)
                {
                    Indicator.Footnote_ID_2 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(1).First().Footnote_ID;
                    Indicator.Map_ID_2 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(1).First().Map_ID;
                    Indicator.Footnote_Symbol_2 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_2).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 2)
                {
                    Indicator.Footnote_ID_3 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(2).First().Footnote_ID;
                    Indicator.Map_ID_3 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(2).First().Map_ID;
                    Indicator.Footnote_Symbol_3 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_3).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 3)
                {
                    Indicator.Footnote_ID_4 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(3).First().Footnote_ID;
                    Indicator.Map_ID_4 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(3).First().Map_ID;
                    Indicator.Footnote_Symbol_4 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_4).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 4)
                {
                    Indicator.Footnote_ID_5 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(4).First().Footnote_ID;
                    Indicator.Map_ID_5 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(4).First().Map_ID;
                    Indicator.Footnote_Symbol_5 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_5).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 5)
                {
                    Indicator.Footnote_ID_6 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(5).First().Footnote_ID;
                    Indicator.Map_ID_6 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(5).First().Map_ID;
                    Indicator.Footnote_Symbol_6 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_6).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 6)
                {
                    Indicator.Footnote_ID_7 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(6).First().Footnote_ID;
                    Indicator.Map_ID_7 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(6).First().Map_ID;
                    Indicator.Footnote_Symbol_7 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_7).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 7)
                {
                    Indicator.Footnote_ID_8 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(7).First().Footnote_ID;
                    Indicator.Map_ID_8 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(7).First().Map_ID;
                    Indicator.Footnote_Symbol_8 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_8).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 8)
                {
                    Indicator.Footnote_ID_9 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(8).First().Footnote_ID;
                    Indicator.Map_ID_9 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(8).First().Map_ID;
                    Indicator.Footnote_Symbol_9 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_9).Footnote_Symbol;
                }
                if (footnoteMaps.Count(e => e.Indicator_ID == Indicator.Indicator_ID) > 9)
                {
                    Indicator.Footnote_ID_10 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(9).First().Footnote_ID;
                    Indicator.Map_ID_10 = footnoteMaps.Where(e => e.Indicator_ID == Indicator.Indicator_ID).OrderBy(e => e.Map_ID).Skip(9).First().Map_ID;
                    Indicator.Footnote_Symbol_10 = db.Footnotes.FirstOrDefault(e => e.Footnote_ID == Indicator.Footnote_ID_10).Footnote_Symbol;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult editFootnoteMaps(IList<Indicator_Footnote_MapsViewModel> newMapsViewModel)
        {

            var newMaps = new Indicator_Footnote_Maps();
            newMaps.Indicator_ID = newMapsViewModel.FirstOrDefault().Indicator_ID;
            newMaps.Footnote_ID = newMapsViewModel.FirstOrDefault().Footnote_Symbol_1 == null ? (Int16)0 : db.Footnotes.ToList().FirstOrDefault(x => x.Footnote_Symbol == newMapsViewModel.FirstOrDefault().Footnote_Symbol_1).Footnote_ID;
            newMaps.Map_ID = newMapsViewModel.FirstOrDefault().Map_ID_1;
            newMaps.Fiscal_Year = newMapsViewModel.FirstOrDefault().Fiscal_Year;

            var mapID = newMaps.Map_ID;
            var footnoteID = newMaps.Footnote_ID;
            if (footnoteID == 0)
            {
                var deleteMap = db.Indicator_Footnote_Maps.Find(newMaps.Map_ID);
                if (deleteMap != null)
                {
                    db.Indicator_Footnote_Maps.Remove(deleteMap);
                    db.SaveChanges();
                }
                return View();
            }
            else if (db.Indicator_Footnote_Maps.Any(x => x.Map_ID == mapID))
            {
                if (ModelState.IsValid && db.Footnotes.Any(x => x.Footnote_ID == footnoteID))
                {
                    db.Entry(newMaps).State = EntityState.Modified;
                    db.SaveChanges();
                    return View();
                }
                else
                {
                    var oldMap = db.Indicator_Footnote_Maps.Find(newMaps.Map_ID);
                    var viewModel = new
                    {
                        Map_ID = oldMap.Map_ID,
                        Footnote_ID = oldMap.Footnote_ID,
                        Indicator_ID = oldMap.Indicator_ID,
                        State = "InvalidChange"
                    };
                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (ModelState.IsValid && db.Footnotes.Any(x => x.Footnote_ID == footnoteID))
                {
                    db.Indicator_Footnote_Maps.Add(newMaps);
                    db.SaveChanges();
                    var viewModel = new
                    {
                        Map_ID = newMaps.Map_ID,
                        Footnote_ID = newMaps.Footnote_ID,
                        Indicator_ID = newMaps.Indicator_ID,
                        State = "NewID"
                    };
                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
                else if (ModelState.IsValid && !db.Footnotes.Any(x => x.Footnote_ID == footnoteID))
                {
                    var viewModel = new
                    {
                        State = "InvalidAdd"
                    };
                    return Json(viewModel, JsonRequestBehavior.AllowGet);
                }
            }
            return View();
        }
        [HttpPost]
        public void setFootnoteMaps(Int16 indicatorID, string footnoteStr)
        {

        }

        [HttpGet]
        public ActionResult getColors()
        {
            List<Color_Types> allColors = db.Color_Types.ToList();
            return Json(allColors, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getColorDirections()
        {
            List<Color_Directions> allDirections = db.Color_Directions.ToList();
            return Json(allDirections, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getColorThreshold()
        {
            List<Color_Thresholds> allThresholds = db.Color_Thresholds.ToList();
            return Json(allThresholds, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getIndicatorList()
        {
            var viewModel = db.Indicators.OrderBy(x => x.Indicator_ID).Select(x => new IndicatorListViewModel
            {
                Indicator_ID = x.Indicator_ID,
                Indicator = x.Indicator,
                Area = x.Area.Area,
            }).ToList();
            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult indicatorList()
        {
            var indexViewModel = new indexViewModel()
            {
                allIndicators = db.Indicators.Where(x => 
					x.Indicator_CoE_Map.FirstOrDefault().CoE_ID > 10 &&
					x.Indicator_CoE_Map.FirstOrDefault().CoE_ID < 20 &&
					x.Indicator_N_Value != true
				).ToList(),
                allAnalysts = db.Analysts.ToList(),
                allAreas = db.Areas.Where(x => x.Area_ID == 1 || x.Area_ID == 3 || x.Area_ID == 4 || x.Area_ID == 5).ToList(),
                allCoEs = db.CoEs.Where(x => x.CoE_ID > 10 && x.CoE_ID < 20 && x.CoE_ID != 14).ToList(),
                allFootnotes = db.Footnotes.ToList(),
            };

            return View(indexViewModel);
        }

		[HttpGet]
		public JsonResult getIndicatorColor(Int16 indicatorID, Int16 fiscalYear)
		{
			var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

			var FY_Q1_Color = (string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Color").GetValue(indicator, null);
			var FY_Q2_Color = (string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Color").GetValue(indicator, null);
			var FY_Q3_Color = (string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Color").GetValue(indicator, null);
			var FY_Q4_Color = (string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Color").GetValue(indicator, null);
			var FY_YTD_Color = (string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Color").GetValue(indicator, null);

			return Json(new {Q1_Color = FY_Q1_Color, Q2_Color = FY_Q2_Color, Q3_Color = FY_Q3_Color, Q4_Color = FY_Q4_Color, YTD_Color = FY_YTD_Color}, JsonRequestBehavior.AllowGet);

		}

		bool IsAllUpper(string input)
		{
			for (int i = 0; i < input.Length; i++)
			{
				if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
					return false;
			}
			return true;
		}

		[HttpPost]
		public JsonResult spellCheckTD(List<TableCellsViewModel> tableCells)
		{
			var incorrectCells = new List<TableCellsViewModel>();

			foreach (var tableCell in tableCells)
			{
				var isIncorrectCell = false;
				var incorrectCell = new TableCellsViewModel();
				var returnText = "";

				var value = tableCell.Value ?? "";
				Regex r = new Regex("^[a-zA-Z]*$");

				using (NHunspell.Hunspell hunspell = new NHunspell.Hunspell(this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/en_us.aff"), this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/en_us.dic")))
				{
					var words = value.Split(' ');
					foreach (var word in words)
					{
						var returnWord = word + " ";
						var chkWord = word;
						chkWord = chkWord.Replace("(", "");
						chkWord = chkWord.Replace(")", "");
						chkWord = chkWord.Replace("*", "");
						chkWord = chkWord.Replace("/", "");
						chkWord = chkWord.Replace("\\", "");
						if (r.IsMatch(chkWord))
						{
							if (!hunspell.Spell(chkWord))
							{
								if (db.Words.Where(x => x.Word == chkWord).Count() == 0)
								{
									if (!IsAllUpper(chkWord))
									{
										//correct = false;
										incorrectCell.Field = tableCell.Field;
										incorrectCell.Indicator_ID = tableCell.Indicator_ID;
										returnWord = "<span class='misspell'>" + returnWord + "</span>" + " ";
										returnText += returnWord;
										returnWord = "";
										isIncorrectCell = true;
									}
								}
							}
						}
						returnText += returnWord;
						incorrectCell.Value = returnText;
					}
				}
				if (isIncorrectCell) incorrectCells.Add(incorrectCell);
			}
			return Json(incorrectCells);
		}

        [HttpGet]
        public ActionResult getValue(Int16 indicatorID, string field, Int16 fiscalYear, bool? convertToFull)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

			if (convertToFull.HasValue && convertToFull.Value)
			{
				field = FiscalYear.FYStrFull(field, fiscalYear);
			}

            var property = indicator.GetType().GetProperty(field);
            var propertySup = indicator.GetType().GetProperty(field + "_Sup");
			string value;
			try
			{
				value = property.GetValue(indicator, null).ToString();
			}
			catch
			{
				return null;
			}
            var valueSup = "";

            var area = indicator.Area_ID;
            var colorProp = indicator.GetType().GetProperty(field + "_Color");
            string color = "";
            if (colorProp != null)
            {
                color = (string)colorProp.GetValue(indicator, null);
            }

            if (propertySup != null)
            {
                valueSup = (string)propertySup.GetValue(indicator, null);
            }
            else
            {
                string footnoteStr = "";
                var allFootnotes = db.Footnotes.ToList();
                int j = 0;
                foreach (var footnote in db.Indicator_Footnote_Maps.Where(x => x.Fiscal_Year == fiscalYear).Where(e => e.Indicator_ID == indicatorID).OrderBy(e => e.Indicator_ID))
                {
                    if (j != 0) { footnoteStr += ","; }
                    footnoteStr += allFootnotes.FirstOrDefault(x => x.Footnote_ID == footnote.Footnote_ID).Footnote_Symbol;
                    j++;
                }
                valueSup = footnoteStr;
            }

            var viewModel = new valueViewModel()
            {
                Value = value,
                Value_Sup = valueSup,
                Area_ID = area,
                Color = color,
            };

            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult setValue(Int16 indicatorID, string updateProperty, string updateValue, string updateValueSup, Int16 fiscalYear)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

            var type = indicator.GetType();
            var property = type.GetProperty(updateProperty);
            property.SetValue(indicator, Convert.ChangeType(updateValue, property.PropertyType), null);

            if (ModelState.IsValid)
            {
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }

            var propertyColor = type.GetProperty(updateProperty + "_Color");
            if (propertyColor != null)
            {
                var color = propertyColor.GetValue(indicator, null);
                return Json(color, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

		[HttpPost]
		public JsonResult setValueRow(List<IndicatorRowViewModel> indicatorRows)
		{
			foreach (var indicatorValues in indicatorRows)
			{
				var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorValues.indicatorID);

				var type = indicator.GetType();
				var property = type.GetProperty(indicatorValues.updateProperty);
				if (!property.GetGetMethod().IsVirtual)
				{
					property.SetValue(indicator, Convert.ChangeType(indicatorValues.updateValue, property.PropertyType), null);
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
				}
			}
			return Json("", JsonRequestBehavior.AllowGet);
		}

        [HttpPost]
        public JsonResult setValueFootnotes(Int16 indicatorID, string updateProperty, string updateValue, string updateValueSup, Int16 fiscalYear)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

            var footnotes = updateValueSup.Split(',');
            foreach (var map in db.Indicator_Footnote_Maps.Where(x => x.Indicator_ID == indicatorID).ToList())
            {
                db.Indicator_Footnote_Maps.Remove(map);
            }
            db.SaveChanges();
            foreach (var footnote in footnotes)
            {
                if (footnote != "%NULL%")
                {
                    Footnotes footnoteObj = db.Footnotes.FirstOrDefault(x => x.Footnote_Symbol == footnote.Trim());
                    if (footnoteObj != null)
                    {
                        Int16 footnoteID = footnoteObj.Footnote_ID;
                        var newMap = new Indicator_Footnote_Maps
                        {
                            Footnote_ID = footnoteID,
                            Indicator_ID = indicatorID,
                            Fiscal_Year = fiscalYear,
                        };
                        db.Indicator_Footnote_Maps.Add(newMap);
                        db.SaveChanges();
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

		public void updateLinks(Int16 indicatorID, string updateValue, string updateProperty, Int16 fiscalYear)
		{
			var allLinks = db.Indicator_Links.Where(x => x.Indicator_Link_Indicators.Any(y => y.Indicator_ID == indicatorID)).ToList();

			var updatePropertyAbbr = FiscalYear.FYStrReverse(updateProperty, (int)fiscalYear);
			foreach (var link in allLinks)
			{
				if (link.Indicator_Link_Fields.Any(x=>x.Link_Field.Link_Fields_Properties.Select(y => y.Link_Field_Property).Contains(updatePropertyAbbr)))
				{
					foreach (var link_indicator in link.Indicator_Link_Indicators.Where(x=>x.Indicator_ID != indicatorID))
					{
						var indicator = link_indicator.Indicator;
						//var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == link.Indicator_ID_2);
						var type = indicator.GetType();
						var property = type.GetProperty(updateProperty);
						property.SetValue(indicator, Convert.ChangeType(updateValue, property.PropertyType), null);
						db.Entry(indicator).State = EntityState.Modified;
						db.SaveChanges();
					}
				}
			}
		}

        [HttpPost]
		public JsonResult setValueOld(Int16 indicatorID, string updateProperty, string updateValue, string updateValueSup, Int16 fiscalYear, bool? convertToFull)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

			if (updateProperty == "Format_ID"){
				string formatStr = "";
				try
				{
					indicator.Format_ID = Int16.Parse(updateValue);
					db.Entry(indicator).State = EntityState.Modified;
					db.SaveChanges();
					
				}
				catch
				{
				}
				finally
				{
					var format = db.Formats.FirstOrDefault(x => x.Format_ID == indicator.Format_ID);
					if (format != null)
					{
						formatStr = format.Format_Code;
					}
				}
				return Json(formatStr, JsonRequestBehavior.AllowGet);
			}

			if (convertToFull.HasValue && convertToFull.Value)
			{
				updateProperty = FiscalYear.FYStrFull(updateProperty, fiscalYear);
			}

            var type = indicator.GetType();
            var property = type.GetProperty(updateProperty);
			property.SetValue(indicator, Convert.ChangeType(updateValue, property.PropertyType), null);

			updateLinks(indicator.Indicator_ID, updateValue, updateProperty, fiscalYear);

            if (updateProperty != "Indicator")
            {
                if (updateValueSup != "%NULL%")
                {
                    var propertySup = indicator.GetType().GetProperty(updateProperty + "_Sup");
                    if (propertySup != null)
                    {
                        propertySup.SetValue(indicator, Convert.ChangeType(updateValueSup, property.PropertyType), null);
                    }
                }
            }
            else
            {
				if (updateValueSup != null)
				{
					var footnotes = updateValueSup.Split(',');
					foreach (var map in db.Indicator_Footnote_Maps.Where(x => x.Indicator_ID == indicatorID).ToList())
					{
						db.Indicator_Footnote_Maps.Remove(map);
					}
					db.SaveChanges();
					foreach (var footnote in footnotes)
					{
						if (footnote != "%NULL%")
						{
							Footnotes footnoteObj = db.Footnotes.FirstOrDefault(x => x.Footnote_Symbol == footnote.Trim());
							if (footnoteObj != null)
							{
								Int16 footnoteID = footnoteObj.Footnote_ID;
								var newMap = new Indicator_Footnote_Maps
								{
									Footnote_ID = footnoteID,
									Indicator_ID = indicatorID,
									Fiscal_Year = fiscalYear,
								};
								db.Indicator_Footnote_Maps.Add(newMap);
								db.SaveChanges();
							}
						}
					}
				}

            }

            if (ModelState.IsValid)
            {
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }

            var propertyColor = type.GetProperty(updateProperty + "_Color");
            if (propertyColor != null)
            {
                var color = propertyColor.GetValue(indicator, null);
                return Json(color, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void formatIndicator(Int16 indicatorID, Int16 formatID)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
            var fieldList = new List<string>();

            var format = db.Formats.FirstOrDefault(x => x.Format_ID == formatID).Format_Code;
            if (format != null)
            {

                for (var fiscalYear = 1; fiscalYear < 99; fiscalYear++)
                {
                    fieldList.Clear();
                    fieldList.Add(FiscalYear.FYStrFull("FY_3", fiscalYear));
                    fieldList.Add(FiscalYear.FYStrFull("FY_2", fiscalYear));
                    fieldList.Add(FiscalYear.FYStrFull("FY_1", fiscalYear));
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Q1");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Q2");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Q3");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Q4");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "YTD");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Target");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Performance_Threshold");
                    fieldList.Add(FiscalYear.FYStrFull("FY_", fiscalYear) + "Comparator");

                    var type = indicator.GetType();

                    var fieldTest = FiscalYear.FYStrFull("FY_", fiscalYear) + "Q1";
                    var propertyTest = type.GetProperty(fieldTest);
                    if (propertyTest == null)
                    {
                        fiscalYear = 100;
                    }
                    else
                    {
                        foreach (var field in fieldList)
                        {
                            var property = type.GetProperty(field);
                            if (property != null)
                            {
                                var num = Helpers.Color.getNum((string)property.GetValue(indicator, null));
                                if (num != null)
                                {
                                    var currValue = float.Parse(num);
                                    var formatedValue = currValue.ToString(format);
                                    property.SetValue(indicator, Convert.ChangeType(formatedValue, property.PropertyType), null);
                                    db.Entry(indicator).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }

        [HttpPost]
        public void setCustomColor(Int16 indicatorID, string field, string color, Int16 fiscalYear)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

            var type = indicator.GetType();
            var property = type.GetProperty(field + "_Custom_Color");
            if (property != null)
            {
                property.SetValue(indicator, Convert.ChangeType(color, property.PropertyType), null);
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

		[HttpPost]
		public ActionResult changeFormat(Int16 indicatorID, Int16 formatID)
		{
			var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
			indicator.Format_ID = formatID;
			db.Entry(indicator).State = EntityState.Modified;
			db.SaveChanges();

			var formatStr = db.Formats.FirstOrDefault(x=>x.Format_ID == formatID).Format_Code;
			return Json(new { formatStr = formatStr }, JsonRequestBehavior.AllowGet);
		}

        [HttpPost]
        public ActionResult changeColor(Int16 indicatorID, Int16 colorID, Int16 fiscalYear)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
            var field = FiscalYear.FYStrFull("FY_", fiscalYear);

            var type = indicator.GetType();
            var colorIDField = field + "Color_ID";
            var property = type.GetProperty(colorIDField);

            if (colorID != -1)
            {
                property.SetValue(indicator, Convert.ChangeType(colorID, property.PropertyType), null);
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }

            var propertyColorQ1 = type.GetProperty(field + "Q1_Color");
            var colorQ1 = (string)propertyColorQ1.GetValue(indicator, null);
            var propertyColorQ2 = type.GetProperty(field + "Q2_Color");
            var colorQ2 = (string)propertyColorQ2.GetValue(indicator, null);
            var propertyColorQ3 = type.GetProperty(field + "Q3_Color");
            var colorQ3 = (string)propertyColorQ3.GetValue(indicator, null);
            var propertyColorQ4 = type.GetProperty(field + "Q4_Color");
            var colorQ4 = (string)propertyColorQ4.GetValue(indicator, null);
            var propertyColorYTD = type.GetProperty(field + "YTD_Color");
            var colorYTD = (string)propertyColorYTD.GetValue(indicator, null);

            var viewModel = new colorViewModel()
            {
                Q1_Color = colorQ1,
                Q2_Color = colorQ2,
                Q3_Color = colorQ3,
                Q4_Color = colorQ4,
                YTD_Color = colorYTD,
            };
            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult changeThreshold(Int16 indicatorID, Int16 thresholdID, Int16 fiscalYear)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
            var field = FiscalYear.FYStrFull("FY_", fiscalYear);

            var type = indicator.GetType();
            var thresholdIDField = field + "Threshold_ID";
            var property = type.GetProperty(thresholdIDField);

            if (thresholdID != -1)
            {
                property.SetValue(indicator, Convert.ChangeType(thresholdID, property.PropertyType), null);
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }

            var propertyColorQ1 = type.GetProperty(field + "Q1_Color");
            var colorQ1 = (string)propertyColorQ1.GetValue(indicator, null);
            var propertyColorQ2 = type.GetProperty(field + "Q2_Color");
            var colorQ2 = (string)propertyColorQ2.GetValue(indicator, null);
            var propertyColorQ3 = type.GetProperty(field + "Q3_Color");
            var colorQ3 = (string)propertyColorQ3.GetValue(indicator, null);
            var propertyColorQ4 = type.GetProperty(field + "Q4_Color");
            var colorQ4 = (string)propertyColorQ4.GetValue(indicator, null);
            var propertyColorYTD = type.GetProperty(field + "YTD_Color");
            var colorYTD = (string)propertyColorYTD.GetValue(indicator, null);

            var viewModel = new colorViewModel()
            {
                Q1_Color = colorQ1,
                Q2_Color = colorQ2,
                Q3_Color = colorQ3,
                Q4_Color = colorQ4,
                YTD_Color = colorYTD,
            };
            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult changeDirection(Int16 indicatorID, Int16 directionID, Int16 fiscalYear)
        {
            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);
            var field = FiscalYear.FYStrFull("FY_", fiscalYear);

            var type = indicator.GetType();
            var directionIDField = field + "Direction_ID";
            var property = type.GetProperty(directionIDField);

            if (directionID != -1)
            {
                property.SetValue(indicator, Convert.ChangeType(directionID, property.PropertyType), null);
                db.Entry(indicator).State = EntityState.Modified;
                db.SaveChanges();
            }

            var propertyColorQ1 = type.GetProperty(field + "Q1_Color");
            var colorQ1 = (string)propertyColorQ1.GetValue(indicator, null);
            var propertyColorQ2 = type.GetProperty(field + "Q2_Color");
            var colorQ2 = (string)propertyColorQ2.GetValue(indicator, null);
            var propertyColorQ3 = type.GetProperty(field + "Q3_Color");
            var colorQ3 = (string)propertyColorQ3.GetValue(indicator, null);
            var propertyColorQ4 = type.GetProperty(field + "Q4_Color");
            var colorQ4 = (string)propertyColorQ4.GetValue(indicator, null);
            var propertyColorYTD = type.GetProperty(field + "YTD_Color");
            var colorYTD = (string)propertyColorYTD.GetValue(indicator, null);

            var viewModel = new colorViewModel()
            {
                Q1_Color = colorQ1,
                Q2_Color = colorQ2,
                Q3_Color = colorQ3,
                Q4_Color = colorQ4,
                YTD_Color = colorYTD,
				Direction = db.Color_Directions.FirstOrDefault(x=> x.Direction_ID == directionID).Direction
            };
            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		public JsonResult inventoryToExcel(List<InventoryViewModel> indicatorList, int fiscalYear)
		{
			var wb = new XLWorkbook();
			var wsName = "output";
			var ws = wb.Worksheets.Add(wsName);

			var allIndicators = db.Indicators.ToList();

			var columnHeaders = new string[,]{
                            {"Number"},
                            {"Identifier"},
							{"CoE"},
							{"Indicator"},
							{"Area"},
							{"Type"},
                            {FiscalYear.FYStrFull("FY_3", fiscalYear)},
                            {FiscalYear.FYStrFull("FY_2", fiscalYear)},
                            {FiscalYear.FYStrFull("FY_1", fiscalYear)},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Q1"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Q2"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Q3"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Q4"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "YTD"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Target"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Performance_Threshold"},
                            {FiscalYear.FYStrFull("FY_", fiscalYear) + "Comparator"},
							{"Definition"},
							{"Target Rationale"},
							{"Comparator Source"}
                        };

			var headerRow = 1;
			var currentRow = headerRow;
			for (var i = 0; i <= columnHeaders.GetUpperBound(0); i++)
			{
				var cell = ws.Cell(currentRow, 1 + i);
				var cellValue = columnHeaders[i, 0];
				cell.Value = "'" + cellValue;

				cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
				cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

				cell.Style.Fill.BackgroundColor = ExcelGlobalVariables.prHeader1Fill;
				cell.Style.Font.FontColor = ExcelGlobalVariables.prHeader1Font;
			}

			var startRow = 2;
			currentRow = startRow;
			foreach (var indicatorListItem in indicatorList)
			{
				var indicator = allIndicators.FirstOrDefault(x => x.Indicator_ID == indicatorListItem.Indicator_ID);
				string[,] columnIndicators = new string[,]{
					{indicator.Indicator_ID.ToString(),
						"",
						"cssWhite"
					},
					{indicator.Identifier,
						"",
						"cssWhite"
					},
					{indicator.Indicator_CoE_Map != null ?
							(indicator.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault() != null ? indicator.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault().CoE.CoE : ""):
							"",
						"",
						"cssWhite"
					},
					{indicator.Indicator, 
						string.Join(",", indicator.Indicator_Footnote_Map.Select(z=>z.Footnote.Footnote_Symbol).ToList()),
						"cssWhite"
					},
					{indicator.Area.Area, "",
						"cssWhite"
					},
					{indicator.Indicator_Type, "",
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD_Sup").GetValue(indicator, null),
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD_Sup").GetValue(indicator, null),
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD_Sup").GetValue(indicator, null),
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Sup").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Color").GetValue(indicator, null)
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Sup").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Color").GetValue(indicator, null)
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Sup").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Color").GetValue(indicator, null)
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Sup").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Color").GetValue(indicator, null)
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Sup").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Color").GetValue(indicator, null)
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Sup").GetValue(indicator, null),
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold_Sup").GetValue(indicator, null),
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator").GetValue(indicator, null),
						(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Sup").GetValue(indicator, null),
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Definition_Calculation").GetValue(indicator, null),
						"",
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Rationale").GetValue(indicator, null),
						"",
						"cssWhite"
					},
					{(string)indicator.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Source").GetValue(indicator, null),
						"",
						"cssWhite"
					},
				};

				for (var i = 0; i <= columnIndicators.GetUpperBound(0); i++){
					var cell = ws.Cell(currentRow, 1 + i);
					var cellValue = columnIndicators[i,0];
					var cellValueSup = columnIndicators[i,1];

					cell.Value = "'" + cellValue;
                    if (columnIndicators[i, 1] != null)
                    {
                        cell.RichText.AddText(columnIndicators[i, 1]).VerticalAlignment = XLFontVerticalTextAlignmentValues.Superscript;
                    }
					switch (columnIndicators[i, 2])
					{
						case "cssWhite":
							cell.RichText.SetFontColor(XLColor.Black);
							cell.Style.Fill.BackgroundColor = XLColor.White;
							break;
						case "cssGreen":
							cell.RichText.SetFontColor(XLColor.White);
							cell.Style.Fill.BackgroundColor = ExcelGlobalVariables.prGreen;
							break;
						case "cssYellow":
							cell.RichText.SetFontColor(XLColor.Black);
							cell.Style.Fill.BackgroundColor = ExcelGlobalVariables.prYellow;
							break;
						case "cssRed":
							cell.RichText.SetFontColor(XLColor.White);
							cell.Style.Fill.BackgroundColor = ExcelGlobalVariables.prRed;
							break;
					}
				}

				currentRow++;
			}

			var rand = new Random();
			var fileName = "export" +  DateTime.Now.ToString("yyyymmddhhmmssffff") + rand.Next(1, 99).ToString() + ".xlsx";
			var filePath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/" + fileName);
			//var fileStream = System.IO.File.Create(filePath);

			wb.SaveAs(filePath);

			return Json(fileName, JsonRequestBehavior.AllowGet) ;
		}

		public ActionResult getFile(string fileName){
			var filePath = this.HttpContext.ApplicationInstance.Server.MapPath("~/App_Data/" + fileName);
			var fileStream = new System.IO.FileStream(filePath, FileMode.Open, System.IO.FileAccess.Read);

			HttpResponse httpResponse = this.HttpContext.ApplicationInstance.Context.Response;
			httpResponse.Clear();
			httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			httpResponse.AddHeader("content-disposition", "attachment;filename=\"" + fileName  + "\"");
			//httpResponse.ContentType = "application/pdf";
			//httpResponse.AddHeader("content-disposition", "attachment;filename=\"test.pdf\"");

			// Flush the workbook to the Response.OutputStream
			using (MemoryStream memoryStream = new MemoryStream())
			{
				fileStream.CopyTo(memoryStream);
				memoryStream.WriteTo(httpResponse.OutputStream);
				memoryStream.Close();
			}
			httpResponse.End();

			fileStream.Close();
			System.IO.File.Delete(@filePath);

			return View();
		}

        [HttpGet]
        public ActionResult editInventory(String indicatorID, Int16? analystID, int fiscalYear, Int16? Link_ID)
        {
            //}
            //var viewModelItems = db.Indicators.Where(x => x.Area_ID.Equals(1)).Where(y => y.Indicator_CoE_Map.Any(x => x.CoE_ID.Equals(10) || x.CoE_ID.Equals(27) || x.CoE_ID.Equals(30) || x.CoE_ID.Equals(40) || x.CoE_ID.Equals(50))).ToArray();
			var allLinks = db.Indicator_Links.ToList();
            var viewModelItems = new List<Indicators>();
            if (analystID.HasValue && analystID.Value > 0)
            {
                //viewModelItems = db.Indicators.Where(x => x.Analyst_ID == analystID).ToList();
                var analystName = db.Analysts.FirstOrDefault(x=>x.Analyst_ID == analystID).First_Name;
                viewModelItems = db.Indicators.Where(x => 
                    x.FY_13_14_Data_Source_Benchmark.Contains(analystName) ||
                    x.FY_13_14_Data_Source_MSH.Contains(analystName) ||
                    x.FY_14_15_Data_Source_Benchmark.Contains(analystName) ||
                    x.FY_14_15_Data_Source_MSH.Contains(analystName)
                    ).ToList();
            }
            else
            {
                viewModelItems = db.Indicators.ToList();
            }
            if (indicatorID != null)
            {
                viewModelItems = viewModelItems.Where(x => x.Indicator_ID == Int16.Parse(indicatorID)).ToList();
            }
			if (Link_ID.HasValue && Link_ID > 0)
			{
				viewModelItems = db.Indicator_Links.FirstOrDefault(x => x.Link_ID == Link_ID).Indicator_Link_Indicators.Select(x => x.Indicator).ToList();
			}
			var allFootnotes = db.Indicator_Footnote_Maps.ToList();

            var viewModel = viewModelItems.Where(x=>x.Indicator_N_Value != true).OrderBy(x => x.Indicator_ID).Select(x => new InventoryViewModel
            {
                Indicator_ID = x.Indicator_ID,
                Area_ID = x.Area_ID,
                CoE = x.Indicator_CoE_Map != null ?
                        (x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault() != null ? x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault().CoE.CoE : "")
                      : "",
                Indicator = x.Indicator,
				Indicator_Type = x.Indicator_Type,
				Identifier = x.Identifier,
				Area = x.Area.Area,
				//Footnote = string.Join(",", allFootnotes.Where(y=>y.Indicator.Indicator_ID == x.Indicator_ID).ToList()),
				Footnote = string.Join(",", x.Indicator_Footnote_Map.Select(z=>z.Footnote.Footnote_Symbol).ToList()),
                FY_3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD").GetValue(x, null),
                FY_3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD_Sup").GetValue(x, null),
                FY_2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD").GetValue(x, null),
                FY_2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD_Sup").GetValue(x, null),
                FY_1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD").GetValue(x, null),
                FY_1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD_Sup").GetValue(x, null),
                FY_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1").GetValue(x, null),
                FY_Q1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Sup").GetValue(x, null),
                FY_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2").GetValue(x, null),
                FY_Q2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Sup").GetValue(x, null),
                FY_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3").GetValue(x, null),
                FY_Q3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Sup").GetValue(x, null),
                FY_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4").GetValue(x, null),
                FY_Q4_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Sup").GetValue(x, null),
                FY_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD").GetValue(x, null),
                FY_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Sup").GetValue(x, null),
                FY_Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(x, null),
                FY_Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Sup").GetValue(x, null),
                FY_Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator").GetValue(x, null),
                FY_Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Sup").GetValue(x, null),
				FY_Comparator_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q1").GetValue(x, null),
				FY_Comparator_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q2").GetValue(x, null),
				FY_Comparator_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q3").GetValue(x, null),
				FY_Comparator_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q4").GetValue(x, null),
                FY_Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold").GetValue(x, null),
                FY_Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold_Sup").GetValue(x, null),

                FY_Color_ID = (Int16)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Color_ID").GetValue(x, null),
                FY_YTD_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Custom_Color").GetValue(x, null),
                FY_Q1_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Custom_Color").GetValue(x, null),
                FY_Q2_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Custom_Color").GetValue(x, null),
                FY_Q3_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Custom_Color").GetValue(x, null),
                FY_Q4_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Custom_Color").GetValue(x, null),

                FY_Definition_Calculation = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Definition_Calculation").GetValue(x, null),
                FY_Target_Rationale = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Rationale").GetValue(x, null),
                FY_Comparator_Source = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Source").GetValue(x, null),

                FY_Data_Source_MSH = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_MSH").GetValue(x, null),
                FY_Data_Source_Benchmark = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_Benchmark").GetValue(x, null),
                FY_OPEO_Lead = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_OPEO_Lead").GetValue(x, null),

                FY_Q1_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Color").GetValue(x, null),
                FY_Q2_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Color").GetValue(x, null),
                FY_Q3_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Color").GetValue(x, null),
                FY_Q4_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Color").GetValue(x, null),
                FY_YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Color").GetValue(x, null),

				FY_Comment = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comment").GetValue(x, null),

                Format_Code = x.Format == null ? "" : (string)x.Format.GetType().GetProperty("Format_Code").GetValue(x.Format, null),
				N_Value = x.Indicator_N_Value.HasValue ? x.Indicator_N_Value.Value : false,
				N_Value_ID = x.Indicator_N_Value_ID == null ? "" : x.Indicator_N_Value_ID.ToString(),

                Fiscal_Year = fiscalYear,
				Analyst_ID = analystID.HasValue ? analystID.Value : 0,
				Link_ID = Link_ID.HasValue ? Link_ID.Value : 0

            }).ToList();

			for (var link_counter = 2; link_counter <= db.CoEs.Count(); link_counter++)
			{
				viewModel.AddRange(viewModelItems.Where(x => x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).Count() == link_counter).OrderBy(x => x.Indicator_ID).Select(x => new InventoryViewModel
				{
					Indicator_ID = x.Indicator_ID,
					Area_ID = x.Area_ID,
					CoE = x.Indicator_CoE_Map != null ?
							(x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).Skip(link_counter - 1).FirstOrDefault() != null ? x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).Skip(link_counter - 1).FirstOrDefault().CoE.CoE : "")
						  : "",
					Indicator = x.Indicator,
					Indicator_Type = x.Indicator_Type,
					Identifier = x.Identifier,
					Area = x.Area.Area,
					//Footnote = string.Join(",", allFootnotes.Where(y=>y.Indicator.Indicator_ID == x.Indicator_ID).ToList()),
					Footnote = string.Join(",", x.Indicator_Footnote_Map.Select(z => z.Footnote.Footnote_Symbol).ToList()),
					FY_3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD").GetValue(x, null),
					FY_3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD_Sup").GetValue(x, null),
					FY_2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD").GetValue(x, null),
					FY_2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD_Sup").GetValue(x, null),
					FY_1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD").GetValue(x, null),
					FY_1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD_Sup").GetValue(x, null),
					FY_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1").GetValue(x, null),
					FY_Q1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Sup").GetValue(x, null),
					FY_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2").GetValue(x, null),
					FY_Q2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Sup").GetValue(x, null),
					FY_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3").GetValue(x, null),
					FY_Q3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Sup").GetValue(x, null),
					FY_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4").GetValue(x, null),
					FY_Q4_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Sup").GetValue(x, null),
					FY_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD").GetValue(x, null),
					FY_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Sup").GetValue(x, null),
					FY_Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(x, null),
					FY_Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Sup").GetValue(x, null),
					FY_Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator").GetValue(x, null),
					FY_Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Sup").GetValue(x, null),
					FY_Comparator_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q1").GetValue(x, null),
					FY_Comparator_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q2").GetValue(x, null),
					FY_Comparator_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q3").GetValue(x, null),
					FY_Comparator_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q4").GetValue(x, null),
					FY_Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold").GetValue(x, null),
					FY_Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold_Sup").GetValue(x, null),

					FY_Color_ID = (Int16)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Color_ID").GetValue(x, null),
					FY_YTD_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Custom_Color").GetValue(x, null),
					FY_Q1_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Custom_Color").GetValue(x, null),
					FY_Q2_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Custom_Color").GetValue(x, null),
					FY_Q3_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Custom_Color").GetValue(x, null),
					FY_Q4_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Custom_Color").GetValue(x, null),

					FY_Definition_Calculation = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Definition_Calculation").GetValue(x, null),
					FY_Target_Rationale = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Rationale").GetValue(x, null),
					FY_Comparator_Source = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Source").GetValue(x, null),

					FY_Data_Source_MSH = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_MSH").GetValue(x, null),
					FY_Data_Source_Benchmark = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_Benchmark").GetValue(x, null),
					FY_OPEO_Lead = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_OPEO_Lead").GetValue(x, null),

					FY_Q1_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Color").GetValue(x, null),
					FY_Q2_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Color").GetValue(x, null),
					FY_Q3_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Color").GetValue(x, null),
					FY_Q4_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Color").GetValue(x, null),
					FY_YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Color").GetValue(x, null),

					FY_Comment = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comment").GetValue(x, null),

					Format_Code = x.Format == null ? "" : (string)x.Format.GetType().GetProperty("Format_Code").GetValue(x.Format, null),
					N_Value = x.Indicator_N_Value.HasValue ? x.Indicator_N_Value.Value : false,
					N_Value_ID = x.Indicator_N_Value_ID == null ? "" : x.Indicator_N_Value_ID.ToString(),

					Fiscal_Year = fiscalYear,
					Analyst_ID = analystID.HasValue ? analystID.Value : 0,
					Link_ID = Link_ID.HasValue ? Link_ID.Value : 0

				}).ToList());
			}
			viewModel = viewModel.OrderBy(x=>x.Indicator_ID).ToList();

			ModelState.Clear();
			var viewModelNValues = db.Indicators.ToList().Where(x => x.Indicator_N_Value == true).OrderBy(x => x.Indicator_ID).Select(x => new InventoryViewModel
			{
				Indicator_ID = x.Indicator_ID,
				Area_ID = x.Area_ID,
				CoE = x.Indicator_CoE_Map != null ?
						(x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault() != null ? x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault().CoE.CoE : "")
					  : "",
				Indicator = x.Indicator,
				Indicator_Type = x.Indicator_Type,
				Identifier = x.Identifier,
				Area = x.Area.Area,
				FY_3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD").GetValue(x, null),
				FY_3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD_Sup").GetValue(x, null),
				FY_2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD").GetValue(x, null),
				FY_2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD_Sup").GetValue(x, null),
				FY_1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD").GetValue(x, null),
				FY_1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD_Sup").GetValue(x, null),
				FY_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1").GetValue(x, null),
				FY_Q1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Sup").GetValue(x, null),
				FY_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2").GetValue(x, null),
				FY_Q2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Sup").GetValue(x, null),
				FY_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3").GetValue(x, null),
				FY_Q3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Sup").GetValue(x, null),
				FY_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4").GetValue(x, null),
				FY_Q4_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Sup").GetValue(x, null),
				FY_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD").GetValue(x, null),
				FY_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Sup").GetValue(x, null),
				FY_Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(x, null),
				FY_Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Sup").GetValue(x, null),
				FY_Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator").GetValue(x, null),
				FY_Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Sup").GetValue(x, null),
				FY_Comparator_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q1").GetValue(x, null),
				FY_Comparator_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q2").GetValue(x, null),
				FY_Comparator_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q3").GetValue(x, null),
				FY_Comparator_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Q4").GetValue(x, null),
				FY_Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold").GetValue(x, null),
				FY_Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold_Sup").GetValue(x, null),

				FY_Color_ID = (Int16)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Color_ID").GetValue(x, null),
				FY_Threshold_ID = (Int16)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Threshold_ID").GetValue(x, null),
				FY_Direction_ID = (Int16)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Direction_ID").GetValue(x, null),
				FY_YTD_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Custom_Color").GetValue(x, null),
				FY_Q1_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Custom_Color").GetValue(x, null),
				FY_Q2_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Custom_Color").GetValue(x, null),
				FY_Q3_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Custom_Color").GetValue(x, null),
				FY_Q4_Custom_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Custom_Color").GetValue(x, null),

				FY_Definition_Calculation = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Definition_Calculation").GetValue(x, null),
				FY_Target_Rationale = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Rationale").GetValue(x, null),
				FY_Comparator_Source = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Source").GetValue(x, null),

				FY_Data_Source_MSH = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_MSH").GetValue(x, null),
				FY_Data_Source_Benchmark = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_Benchmark").GetValue(x, null),
				FY_OPEO_Lead = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_OPEO_Lead").GetValue(x, null),

				FY_Q1_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Color").GetValue(x, null),
				FY_Q2_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Color").GetValue(x, null),
				FY_Q3_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Color").GetValue(x, null),
				FY_Q4_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Color").GetValue(x, null),
				FY_YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Color").GetValue(x, null),

				FY_Comment = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comment").GetValue(x, null),

				Format_Code = x.Format == null ? "" : (string)x.Format.GetType().GetProperty("Format_Code").GetValue(x.Format, null),
				N_Value = x.Indicator_N_Value.HasValue ? x.Indicator_N_Value.Value : false,
				N_Value_ID = x.Indicator_N_Value_ID == null ? "" : x.Indicator_N_Value_ID.ToString(),

				Fiscal_Year = fiscalYear,
				Analyst_ID = analystID.HasValue ? analystID.Value : 0,
				Link_ID = Link_ID.HasValue ? Link_ID.Value : 0

			}).ToList();

			foreach (var nValue in viewModelNValues)
			{
				if (nValue.N_Value_ID != null)
				{
					var indicatorIndex = viewModel.FirstOrDefault(x => x.Indicator_ID == Int16.Parse(nValue.N_Value_ID));
					if (indicatorIndex != null)
					{
						var position = viewModel.IndexOf(indicatorIndex);
						viewModel.Insert(position + 1, nValue);
					}
				}
			}

            if (viewModel.Count == 0)
            {
                viewModel.Add(new InventoryViewModel());
            }
            viewModel.FirstOrDefault().allAnalysts = db.Analysts.ToList();
			viewModel.FirstOrDefault().allDirections = db.Color_Directions.ToList();
			viewModel.FirstOrDefault().allColors = db.Color_Types.ToList();
			viewModel.FirstOrDefault().allThresholds = db.Color_Thresholds.ToList();
            if (Request.IsAjaxRequest())
            {
                return Json(viewModel.Where(x => x.Indicator_ID.ToString().Contains(indicatorID == null ? "" : indicatorID)), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View(viewModel);
            }

        }

        [HttpPost]
        public void editInventory(Int16 indicatorID, string updateProperty, string updateValue, int fiscalYear)
        {
            var updatePropertyFull = updateProperty;
            if (fiscalYear != 0)
            {
                updatePropertyFull = FiscalYear.FYStrFull(updateProperty, fiscalYear);
            }

            var indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == indicatorID);

            if (indicator == null)
            {
                indicator = db.Indicators.Create();
                //indicator.Indicator_ID = updateValue;
                db.Indicators.Add(indicator);
                db.SaveChanges();
            }
            else
            {
                var property = indicator.GetType().GetProperty(updatePropertyFull);
                property.SetValue(indicator, Convert.ChangeType(updateValue, property.PropertyType), null);
				updateLinks(indicator.Indicator_ID, updateValue, updatePropertyFull, (Int16)fiscalYear);
                if (ModelState.IsValid)
                {
                    db.Entry(indicator).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            //var indicatorID = indicatorChange[0].Indicator_ID;
            //if (db.Indicators.Any(x => x.Indicator_ID == indicatorID ))
            //{
            //    if (ModelState.IsValid)
            //    {
            //        db.Entry(indicatorChange[0]).State = EntityState.Modified;
            //        db.SaveChanges();
            //    }
            //} 
            //else
            //{
            //    if (ModelState.IsValid)
            //    {
            //        db.Indicators.Add(indicatorChange[0]);
            //        db.SaveChanges();
            //    }
            //}

        }

        //
        // GET: /Indicator/Details/5

        [HttpPost]
        public void deleteInventory(Int16 indicatorID)
        {
            Indicators indicators = db.Indicators.Find(indicatorID);
            db.Indicators.Remove(indicators);
            db.SaveChanges();
        }

        [HttpPost]
        public JsonResult newIndicatorAtPR(Int16 fiscalYear, Int16 areaID, Int16 coeID, Int16 indicatorID, Int16? newIndicatorID)
        {
            Indicators indicator = new Indicators();

            if (newIndicatorID.HasValue)
            {
                indicator = db.Indicators.FirstOrDefault(x => x.Indicator_ID == newIndicatorID);
            }
            else
            {
				indicator = db.Indicators.Create();//new Indicators();
				db.Indicators.Add(indicator);
				db.SaveChanges();
                indicator.Area_ID = areaID;

				var type = indicator.GetType();
				var property = type.GetProperty(Helpers.FiscalYear.FYStrFull("FY_Color_ID", fiscalYear));
				property.SetValue(indicator, Convert.ChangeType(1, property.PropertyType), null);


                indicator.Indicator = "";
				db.Entry(indicator).State = EntityState.Modified;
                //db.Indicators.Add(indicator);
                db.SaveChanges();
            }

            var newMap = new Indicator_CoE_Maps();
            newMap.Indicator_ID = indicator.Indicator_ID;

            int number = 0;
            if (indicatorID != 0)
            {
				if (db.Indicator_CoE_Maps.Where(x => x.CoE_ID == coeID && x.Fiscal_Year == fiscalYear).FirstOrDefault(x => x.Indicator_ID == indicatorID) != null)
				{
					number = db.Indicator_CoE_Maps.Where(x => x.CoE_ID == coeID && x.Fiscal_Year == fiscalYear).FirstOrDefault(x => x.Indicator_ID == indicatorID).Number + 1;
				}
				else
				{
					number = 0;
				}
            }
            newMap.Number = (Int16)number;

            var allMaps = db.Indicator_CoE_Maps.ToList();
            foreach (var map in allMaps.OrderBy(x => x.Number).Where(x => x.CoE_ID == coeID && x.Fiscal_Year == x.Fiscal_Year && x.Number >= number))
            {
                map.Number++;
                db.Entry(map).State = EntityState.Modified;
                db.SaveChanges();
            }
            newMap.CoE_ID = coeID;
            newMap.Fiscal_Year = fiscalYear;
            db.Indicator_CoE_Maps.Add(newMap);
            db.SaveChanges();

			var colorID = (Int16)newMap.Indicator.GetType().GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Color_ID").GetValue(newMap.Indicator, null);

            return Json(new { indicatorID = indicator.Indicator_ID, mapID = newMap.Map_ID, newAreaID = indicator.Area_ID, colorID = colorID}, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		public JsonResult newIndicatorAtPRdbl(Int16 fiscalYear, Int16 areaID, Int16 coeID, Int16 coeID2)
		{
            Indicators indicator = new Indicators();
			Indicators indicator2 = new Indicators();

			indicator = db.Indicators.Create();
			indicator2 = db.Indicators.Create();
			db.Indicators.Add(indicator);
			db.Indicators.Add(indicator2);
			db.SaveChanges();
            indicator.Area_ID = areaID;
			indicator2.Area_ID = areaID;

			var type = indicator.GetType();
			var property = type.GetProperty(Helpers.FiscalYear.FYStrFull("FY_Color_ID", fiscalYear));
			property.SetValue(indicator, Convert.ChangeType(1, property.PropertyType), null);
			var type2 = indicator2.GetType();
			var property2 = type.GetProperty(Helpers.FiscalYear.FYStrFull("FY_Color_ID", fiscalYear));
			property2.SetValue(indicator2, Convert.ChangeType(1, property2.PropertyType), null);

            indicator.Indicator = "";
			indicator.Indicator_Link = indicator2.Indicator_ID;
			db.Entry(indicator).State = EntityState.Modified;
            db.SaveChanges();
			indicator2.Indicator = "";
			indicator2.Indicator_Link = indicator.Indicator_ID;
			db.Entry(indicator2).State = EntityState.Modified;
			db.SaveChanges();

            var newMap = new Indicator_CoE_Maps();
            newMap.Indicator_ID = indicator.Indicator_ID;
            newMap.CoE_ID = coeID;
            newMap.Fiscal_Year = fiscalYear;
            db.Indicator_CoE_Maps.Add(newMap);
            db.SaveChanges();

			var newMap2 = new Indicator_CoE_Maps();
			newMap2.Indicator_ID = indicator2.Indicator_ID;
			newMap2.CoE_ID = coeID2;
			newMap2.Fiscal_Year = fiscalYear;
			db.Indicator_CoE_Maps.Add(newMap2);
			db.SaveChanges();

			var colorID = (Int16)newMap.Indicator.GetType().GetProperty(FiscalYear.FYStrFull("FY_", fiscalYear) + "Color_ID").GetValue(newMap.Indicator, null);

            return Json(new { indicatorID = indicator.Indicator_ID, mapID = newMap.Map_ID, newAreaID = indicator.Area_ID, colorID = colorID,
							  indicatorID2 = indicator2.Indicator_ID, mapID2 = newMap2.Map_ID, newAreaID2 = indicator2.Area_ID, colorID2 = colorID
							}, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
        public JsonResult Details(Int16 indicatorID, Int16 fiscalYear)
        {
            var viewModelItems = new List<Indicators>();
            viewModelItems = db.Indicators.Where(x => x.Indicator_ID == indicatorID).ToList();

            var viewModel = viewModelItems.OrderBy(x => x.Indicator_ID).Select(x => new GraphViewModel
            {
                Indicator_ID = x.Indicator_ID,
                Area_ID = x.Area_ID,
                //CoE = x.Indicator_CoE_Map.Count != 0 ? x.Indicator_CoE_Map.Where(y => y.Fiscal_Year == fiscalYear).FirstOrDefault().CoE.CoE : "",
                Indicator = x.Indicator,

                FY_3_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD").GetValue(x, null),
                FY_3_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD_Sup").GetValue(x, null),
                //FY_3_Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_Target").GetValue(x, null),
                //FY_3_Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_Target_Sup").GetValue(x, null),
                //FY_3_Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_Comparator").GetValue(x, null),
                //FY_3_Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_Comparator_Sup").GetValue(x, null),
                //FY_3_Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_Performance_Threshold").GetValue(x, null),
                //FY_3_Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_Performance_Threshold_Sup").GetValue(x, null),

                FY_2_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD").GetValue(x, null),
                FY_2_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD_Sup").GetValue(x, null),
                //FY_2_Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_Target").GetValue(x, null),
                //FY_2_Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_Target_Sup").GetValue(x, null),
                //FY_2_Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_Comparator").GetValue(x, null),
                //FY_2_Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_Comparator_Sup").GetValue(x, null),
                //FY_2_Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_Performance_Threshold").GetValue(x, null),
                //FY_2_Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_Performance_Threshold_Sup").GetValue(x, null),

                FY_1_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD").GetValue(x, null),
                FY_1_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD_Sup").GetValue(x, null),
                //FY_1_Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_Target").GetValue(x, null),
                //FY_1_Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_Target_Sup").GetValue(x, null),
                //FY_1_Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_Comparator").GetValue(x, null),
                //FY_1_Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_Comparator_Sup").GetValue(x, null),
                //FY_1_Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_Performance_Threshold").GetValue(x, null),
                //FY_1_Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_Performance_Threshold_Sup").GetValue(x, null),

                FY_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1").GetValue(x, null),
                FY_Q1_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Sup").GetValue(x, null),
                FY_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2").GetValue(x, null),
                FY_Q2_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Sup").GetValue(x, null),
                FY_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3").GetValue(x, null),
                FY_Q3_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Sup").GetValue(x, null),
                FY_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4").GetValue(x, null),
                FY_Q4_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Sup").GetValue(x, null),
                FY_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD").GetValue(x, null),
                FY_YTD_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Sup").GetValue(x, null),
                Target = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(x, null),
				TargetNum = Helpers.Color.getNum((string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target").GetValue(x, null)),
                Target_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Sup").GetValue(x, null),
                Comparator = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator").GetValue(x, null),
                Comparator_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Sup").GetValue(x, null),
                Performance_Threshold = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold").GetValue(x, null),
                Performance_Threshold_Sup = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Performance_Threshold_Sup").GetValue(x, null),

                Color_ID = (Int16)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Color_ID").GetValue(x, null),
                Custom_YTD = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Custom_Color").GetValue(x, null),
                Custom_Q1 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Custom_Color").GetValue(x, null),
                Custom_Q2 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Custom_Color").GetValue(x, null),
                Custom_Q3 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Custom_Color").GetValue(x, null),
                Custom_Q4 = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Custom_Color").GetValue(x, null),

                Definition_Calculation = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Definition_Calculation").GetValue(x, null),
                Target_Rationale = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Target_Rationale").GetValue(x, null),
                Comparator_Source = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Comparator_Source").GetValue(x, null),

                Data_Source_MSH = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_MSH").GetValue(x, null),
                Data_Source_Benchmark = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Data_Source_Benchmark").GetValue(x, null),
                OPEO_Lead = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_OPEO_Lead").GetValue(x, null),

                Q1_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q1_Color").GetValue(x, null),
                Q2_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q2_Color").GetValue(x, null),
                Q3_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q3_Color").GetValue(x, null),
                Q4_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_Q4_Color").GetValue(x, null),
                YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 0) + "_YTD_Color").GetValue(x, null),

				//FY_1_YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 1) + "_YTD_Color").GetValue(x, null),
				//FY_2_YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 2) + "_YTD_Color").GetValue(x, null),
				//FY_3_YTD_Color = (string)x.GetType().GetProperty(FiscalYear.FYStr(fiscalYear, 3) + "_YTD_Color").GetValue(x, null),

                Fiscal_Year = fiscalYear,

            }).FirstOrDefault();
            return Json(viewModel);
        }

        //
        // GET: /Indicator/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Indicator/Create

        [HttpPost]
        public ActionResult Create(Indicators indicators)
        {
            if (ModelState.IsValid)
            {
                db.Indicators.Add(indicators);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(indicators);
        }

        //
        // GET: /Indicator/Edit/5

        public ActionResult edit(string indicatorID)
        {
            Indicators indicator = db.Indicators.Find(indicatorID);

            if (indicator == null)
            {
                indicator = db.Indicators.Create();
            }

            var viewModel = new editViewModel
            {
                Indicator = indicator,
                allCoEs = db.CoEs.ToList(),
            };

            return View(viewModel);
        }

        //
        // POST: /Indicator/Edit/5

        [HttpPost, ValidateInput(false)]
        public ActionResult edit(Indicators indicators)
        {
            if (ModelState.IsValid)
            {
                db.Entry(indicators).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(indicators);
        }

        //
        // GET: /Indicator/Delete/5

        public ActionResult Delete(string indicatorID)
        {
            Indicators indicators = db.Indicators.Find(indicatorID);
            if (indicators == null)
            {
                return HttpNotFound();
            }
            return View(indicators);
        }

        //
        // POST: /Indicator/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Int16 Indicator_ID)
        {
            Indicators indicators = db.Indicators.Find(Indicator_ID);
            db.Indicators.Remove(indicators);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}


