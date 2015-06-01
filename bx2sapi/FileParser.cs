using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Excel;

namespace bx2sapi
{
	public static class FileParser
	{
		public static List<Raw> ReadDataFromFile(string inFile)
		{
			var data = new List<Raw>();

			if (".xlsx" == Path.GetExtension(inFile))
			{
				var worksheet = Workbook.Worksheets(inFile).FirstOrDefault();
				if (worksheet == null)
				{
					Console.WriteLine("Workbook is empty.");
					return data;
				}
				foreach (var excelRow in worksheet.Rows)
				{
					data.Add(new Raw
					{
						Eng = excelRow.GetText(0),
						Rus = excelRow.GetText(2),
						EngExample = excelRow.GetText(3),
						RusExample = excelRow.GetText(4)
					});
				}
			}
			else
			{
				using (var file = new StreamReader(inFile, System.Text.Encoding.GetEncoding(1251)))
				{
					string line;
					while ((line = file.ReadLine()) != null)
						data.Add(new Raw(line));
				}
			}

			return data;
		}

		public static void ParseData(IEnumerable<Raw> data, bool sentenceMode)
		{
			var rxR = new RegexpHelper();
			foreach (var raw in data)
			{
				raw.IsPhrase = raw.Rus.Contains("(фраза)");
				raw.Rus = raw.Rus.Replace("(фраза)", "");

				if (sentenceMode)
				{
					raw.EngClear = raw.Eng;
					//
					raw.RusNorm = raw.Rus;
					raw.RusClear = raw.Rus;
				}
				else
				{
					raw.EngClear = rxR.RemoveTextInBrackets(raw.Eng).RemoveSpaces();
					//
					var rusprp = rxR.ReplaceAbr(raw.Rus).RemoveSpaces();
					raw.RusNorm = rusprp;
					raw.RusClear = rxR.RemoveTextInBrackets(rusprp).RemoveSpaces();
				}

				raw.PhraseOrQuestion =
					raw.RusClear.EndsWith("...") ? SentenceType.Phrase :
						raw.RusClear.EndsWith("?") ? SentenceType.Question :
							raw.RusClear.EndsWith(".") || raw.Rus.EndsWith("!") ? SentenceType.Sentence : SentenceType.None;
				raw.EngExampleComparable = rxR.RemoveBrackets(raw.EngExample);
				raw.EngComparable = rxR.RemoveBrackets(raw.Eng);
				raw.RusExampleComparable = rxR.RemoveBrackets(raw.RusExample);
				raw.RusComparable = rxR.RemoveBrackets(raw.Rus);
				if (sentenceMode)
				{
					raw.RusExample = "";
					raw.RusExampleComparable = "";
					raw.EngExample = "";
					raw.EngExampleComparable = "";
				}
			}
		}

		static string RemoveSpaces(this string inputString)
		{
			return inputString
				.Replace("(", " (")
				.Replace(")", ") ")
				.Replace(" ,", ",")
				.Replace(" .", ".")
				.Replace("  ", " ")
				.Trim();
		}
	}
}
