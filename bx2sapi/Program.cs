using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Excel;

namespace bx2sapi
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				#region verify parameters
				if (args == null || args.Length == 0)
				{
					Console.WriteLine(@"Parameters:
	FileName
	/minpause
	/silentpause
	/sentenceMode
Example:
	d:\bx2sapi\test.xlsx /minpause:4000 /silentpause:4500
	d:\bx2sapi\pimsleur.xlsx /minpause:4000 /silentpause:4500 /sentenceMode");
					return;
				}
				var inFile = args[0];
				if (!File.Exists(inFile)) //d:\bx2sapi\test.csv
				{
					Console.WriteLine(@"File {0} does not exist. Please, define correct argument.", inFile);
					return;
				}
				var minPause = 3000;
				var silentPause = 3500;
				var sentenceMode = false;
				if (args.Length > 1) // /minpause:4000 /silentpause:4500
				{
					foreach (var arg in args)
					{
						var ar = arg.Split(':');
						if (ar.Length == 2)
						{
							if (ar[0] == "/minpause")
							{
								if (!int.TryParse(ar[1], out minPause))
									minPause = 3000;
							}
							else if (ar[0] == "/silentpause")
							{
								if (!int.TryParse(ar[1], out silentPause))
									silentPause = 3500;
							}
						}
						else
						{
							if (arg == "/sentenceMode")
							{
								sentenceMode = true;
							}
						}
					}
				}
				var outFile = Path.Combine(Path.GetDirectoryName(inFile), Path.GetFileNameWithoutExtension(inFile));
				var outFileRuEn = Path.ChangeExtension(outFile + " RuEn", "txt");
				var outFileEnRu = Path.ChangeExtension(outFile + " EnRu", "txt");
				var outFileSamples = Path.ChangeExtension(outFile + " Samples", "txt");
				#endregion verify parameters

				var data = ReadDataFromFile(inFile);

				ParseData(data, sentenceMode);

				WriteRuEn(outFileRuEn, data, minPause, silentPause, sentenceMode);
				Console.WriteLine(outFileRuEn);

				WriteEnRu(outFileEnRu, data, minPause, silentPause, sentenceMode);
				Console.WriteLine(outFileEnRu);

				WriteSamples(outFileSamples, data, minPause, silentPause, sentenceMode);
				Console.WriteLine(outFileSamples);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.ReadKey();
			}
		}

		private static void ParseData(IEnumerable<Raw> data, bool sentenceMode)
		{
			var rxR = new RegexpHelper();
			foreach (var raw in data)
			{
				raw.IsPhrase = raw.Rus.Contains("(фраза)");
				raw.Rus = raw.Rus.Replace("(фраза)", "");
				//
				if (sentenceMode)
				{
					raw.RusTran = raw.Rus;
					raw.RusClear = raw.Rus;
				}
				else
				{
					var rusprp =
						rxR.ReplaceAbr(raw.Rus)
							.Replace("(", " (")
							.Replace(")", ") ")
							.Replace("  ", " ")
							.Replace(" ,", ",")
							.Replace(" .", ".")
							.Trim();
					raw.RusTran = rusprp;
					raw.RusClear = rxR.RemoveBrackets(rusprp).Replace("  ", " ").Replace(" ,", ",").Replace(" .", ".").Trim();
				}
				if (sentenceMode)
					raw.EngClear = raw.Eng;
				else
					raw.EngClear = rxR.RemoveBrackets(raw.Eng).Replace("  ", " ").Replace(" ,", ",").Replace(" .", ".").Trim();
				//
				raw.RusPhraseOrQuestion = (raw.RusClear.EndsWith("..."))
					? " the phrase"
					: (raw.RusClear.EndsWith("?"))
						? " the question"
						: (raw.RusClear.EndsWith(".") || raw.Rus.EndsWith("!")) ? " the sentence" : "";
				raw.EngExampleNorm = rxR.Normilize(raw.EngExample);
				raw.EngNorm = rxR.Normilize(raw.Eng);
				raw.RusExampleNorm = rxR.Normilize(raw.RusExample);
				raw.RusNorm = rxR.Normilize(raw.Rus);
				if (sentenceMode)
				{
					raw.RusExample = "";
					raw.RusExampleNorm = "";
					raw.EngExample = "";
					raw.EngExampleNorm = "";
				}
			}
		}

		private static List<Raw> ReadDataFromFile(string inFile)
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

		private static void WriteEnRu(string outFile, IEnumerable<Raw> data, int minPause, int silentPause, bool sentenceMode)
		{
			if (File.Exists(outFile))
				File.Delete(outFile);
			using (var fs = new FileStream(outFile, FileMode.OpenOrCreate))
			{
				using (var writer = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251)))
				{
					writer.WriteLine(@"file {0}<silence msec=""400""/>", Path.GetFileNameWithoutExtension(outFile));
					foreach (var raw in data)
					{

						#region english

						var isExample = !String.IsNullOrWhiteSpace(raw.EngExample) && raw.EngExampleNorm != raw.EngNorm;

						if (sentenceMode)
							writer.WriteLine(@"<silence msec=""1000""/>{0}", raw.Eng);
						else if (-1 == raw.Eng.IndexOf('('))
						{
							if (isExample)
								writer.WriteLine(@"listen<silence msec=""400""/>{0}<silence msec=""400""/>translate<silence msec=""400""/>{1}"
									, raw.EngExample, raw.Eng);
							else
								writer.WriteLine(@"<silence msec=""400""/>{0}", raw.Eng);
						}
						else
						{
							if (isExample)
								writer.WriteLine(@"listen<silence msec=""400""/>{0}<silence msec=""400""/>{1}<silence msec=""400""/>translate<silence msec=""400""/>{2}"
									, raw.EngExample, raw.Eng, raw.EngClear);
							else
								writer.WriteLine(@"listen<silence msec=""400""/>{0}<silence msec=""400""/>translate<silence msec=""400""/>{1}", raw.Eng, raw.EngClear);
						}

						#endregion english

						#region russian

						if (-1 == raw.RusTran.IndexOf('(') || sentenceMode)
							writer.WriteLine(@"<silence msec=""{1}""/><lang langid=""419"">{0}</lang>"
								, raw.RusTran, minPause);
						else
							writer.WriteLine(
								@"<silence msec=""{2}""/><lang langid=""419"">{0}</lang><silence msec=""700""/><lang langid=""419"">{1}</lang>"
								, raw.RusClear, raw.RusTran, minPause);

						#endregion russian

						if (isExample && !String.IsNullOrWhiteSpace(raw.RusExample) && raw.RusExampleNorm != raw.RusNorm)
							writer.WriteLine(@"<silence msec=""1500""/><lang langid=""419"">{0}</lang>", raw.RusExample);

						writer.WriteLine(@"<silence msec=""{0}""/>", silentPause);
						writer.WriteLine();
					}
					writer.WriteLine(
						@"The lesson is over.<silence msec=""400""/>Come back and listen to me again!<silence msec=""1000""/>");
				}
			}
		}

		private static void WriteSamples(string outFile, IEnumerable<Raw> data, int minPause, int silentPause, bool sentenceMode)
		{
			if (File.Exists(outFile))
				File.Delete(outFile);
			if (sentenceMode)
				return;
			var dataSamples = data.Where(it => !String.IsNullOrWhiteSpace(it.EngExample))
				.Where(it => !String.IsNullOrWhiteSpace(it.RusExample))
				.GroupBy(it => it.EngExample);
			using (var fs = new FileStream(outFile, FileMode.OpenOrCreate))
			{
				using (var writer = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251)))
				{
					writer.WriteLine(@"file {0}<silence msec=""400""/>", Path.GetFileNameWithoutExtension(outFile));
					foreach (var dataSample in dataSamples)
					{
						var raw = dataSample.FirstOrDefault();
						writer.WriteLine(raw.EngExample);
						writer.WriteLine(@"<silence msec=""{0}""/>", minPause);
						writer.WriteLine(@"<lang langid=""419"">{0}</lang>", raw.RusExample);
						writer.WriteLine(@"<silence msec=""1500""/>{0}", raw.EngExample);
						writer.WriteLine(@"<silence msec=""{0}""/>", silentPause);
						writer.WriteLine();
					}
					writer.WriteLine(
						@"The lesson is over.<silence msec=""400""/>Come back and listen to me again!<silence msec=""1000""/>");
				}
			}
		}

		private static void WriteRuEn(string outFile, IEnumerable<Raw> data, int minPause, int silentPause, bool sentenceMode)
		{
			if (File.Exists(outFile))
				File.Delete(outFile);
			using (var fs = new FileStream(outFile, FileMode.OpenOrCreate))
			{
				using (var writer = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251)))
				{
					writer.WriteLine(@"file {0}<silence msec=""400""/>", Path.GetFileNameWithoutExtension(outFile));
					foreach (var raw in data)
					{
						#region russian

						if (sentenceMode)
							writer.WriteLine(@"<silence msec=""1000""/><lang langid=""419"">{0}</lang>", raw.RusTran);
						else if (-1 == raw.RusTran.IndexOf('('))
							writer.WriteLine(@"<silence msec=""1000""/><lang langid=""419"">{0} {1}</lang>", raw.RusTran, (raw.IsPhrase ? "(фраза)" : ""));
							/*writer.WriteLine(@"{0}<silence msec=""400""/><lang langid=""419"">{1}</lang>", raw.RusPhraseOrQuestion, raw.RusTran);*/
						else
							writer.WriteLine(
								@"listen<silence msec=""400""/><lang langid=""419"">{0}</lang><silence msec=""400""/>translate{2}<silence msec=""400""/><lang langid=""419"">{1} {3}</lang>"
								, raw.RusTran, raw.RusClear, raw.RusPhraseOrQuestion, (raw.IsPhrase ? "(фраза)" : ""));

						#endregion russian

						#region english

						writer.WriteLine(@"<silence msec=""{0}""/>", minPause);
						if (-1 == raw.Eng.IndexOf('(') || sentenceMode)
						{
							if (-1 == raw.Eng.Trim().IndexOf(' ') && -1 == raw.Eng.Trim().IndexOf('/'))
								writer.WriteLine(@"{0}<silence msec=""400""/>{0}", raw.Eng);
							else
								writer.WriteLine(@"{0}", raw.Eng);
						}
						else
						{
							writer.WriteLine(@"{0}<silence msec=""700""/>{1}", raw.EngClear, raw.Eng);
						}

						#endregion english

						#region example

						//if (!String.IsNullOrWhiteSpace(raw.EngExample))
						//{
						//	if (raw.EngExampleNorm != raw.EngNorm)
						//	{
						//		writer.WriteLine(@"<silence msec=""1900""/>{0}", raw.EngExample);
						//		if (!String.IsNullOrWhiteSpace(raw.RusExample))
						//			writer.WriteLine(@"<silence msec=""1500""/><lang langid=""419"">{0}</lang>", raw.RusExample);
						//	}
						//}

						#endregion example

						writer.WriteLine(@"<silence msec=""{0}""/>", silentPause);
						writer.WriteLine();
					}
					writer.WriteLine(
						@"The lesson is over.<silence msec=""400""/>Come back and listen to me again!<silence msec=""1000""/>");
				}
			}
		}
	}

	#region
	class RegexpHelper
	{
		/*var rx = new Regex(@"\{(?>[^\{\}]+|\{(?<Br>)|\}(?<-Br>))*(?(Br)(?!))\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			var s = "aaa {dd} {ff} ddd {vv {{bb}} ff}";
			s = rx.Replace(s, "");*/
		readonly Regex _rx1 = new Regex(@"\(.*?\)", RegexOptions.Compiled);
		readonly Regex _rx2 = new Regex(@"\[.*?\]", RegexOptions.Compiled);
		#region _rxR
		readonly Regex[] _rxR =
		{
			new Regex(@"\bсравн\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bв тч\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bв т.ч.\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bи тп\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bи т. п.\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bи пр\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bкакого-л\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bкому-л\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bкого-л\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bчего-л\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new Regex(@"\bчему-л\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
		};
		readonly string[] _toR =
		{
			"сравнительная степень",
			"в том числе",
			"в том числе",
			"и тому подобное",
			"и тому подобное",
			"и прочее",
			"какого-либо",
			"кому-либо",
			"кого-либо",
			"чего-либо",
			"чему-либо"
		};
		#endregion _rxR
		public string ReplaceAbr(string input)
		{
			input = input
				.Replace("(разг)", "[разговорный вариант]")
				.Replace("(амер)", "[американский вариант]")
				.Replace("(брит)", "[британский вариант]")
				.Replace("(нем)", "[из немецкого]")
				.Replace("(пр вр)", "[прошедшее время]")
				.Replace("(мн ч)", "[множественное число]")
				.Replace("(биол)", "[из биологии]")
				.Replace("(юр)", "[из юридического]")
				.Replace("(анат)", "[из анатомии]")
				.Replace("(редко)", "[редко используется]");
			for (var i = 0; i < _rxR.Length; i++)
				input = _rxR[i].Replace(input, _toR[i]);
			return input;
		}
		public string RemoveBrackets(string input)
		{
			return _rx1.Replace(input, "");
		}
		public string Normilize(string input)
		{
			return _rx2.Replace(input, "").ToLower().Replace("(", "").Replace(")", "").Replace(".", "").Replace("!", "").Replace("?", "").Replace("’", "").Replace("'", "").Replace(" ", "");
		}
	}

	class Raw
	{
		public Raw()
		{

		}
		public Raw(string csv)
		{
			var el = csv.Split(';');
			this.Eng = el[0];
			this.Rus = el[2];
			this.EngExample = el[3];
			this.RusExample = el[4];
		}

		public string Rus { get; set; }
		public string Eng { get; set; }
		public string RusExample { get; set; }
		public string EngExample { get; set; }

		public string RusTran { get; set; }
		public string RusClear { get; set; }
		public string RusPhraseOrQuestion { get; set; }
		public string EngClear { get; set; }

		public string EngNorm { get; set; }
		public string EngExampleNorm { get; set; }
		public string RusNorm { get; set; }
		public string RusExampleNorm { get; set; }

		public bool IsPhrase { get; set; }
	}
	#endregion
}
