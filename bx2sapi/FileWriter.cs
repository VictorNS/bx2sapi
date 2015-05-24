using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace bx2sapi
{
	public static class FileWriter
	{
		public static void WriteEnRu(string outFile, IEnumerable<Raw> data, int minPause, int silentPause, bool sentenceMode)
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

						var isEngExample = !string.IsNullOrWhiteSpace(raw.EngExample);
						var isRusExample = !string.IsNullOrWhiteSpace(raw.RusExample);

						if (sentenceMode)
							writer.WriteLine(@"<silence msec=""1000""/>{0}", raw.Eng);
						else if (-1 == raw.Eng.IndexOf('('))
						{
							if (isEngExample && raw.EngExampleComparable != raw.EngComparable)
								writer.WriteLine(@"listen<silence msec=""400""/>{0}<silence msec=""400""/>translate<silence msec=""400""/>{1}"
									, raw.EngExample, raw.Eng);
							else
								writer.WriteLine(@"<silence msec=""400""/>{0}", raw.Eng);
						}
						else
						{
							if (isEngExample && raw.EngExampleComparable != raw.EngComparable)
								writer.WriteLine(@"listen<silence msec=""400""/>{0}<silence msec=""400""/>{1}<silence msec=""400""/>translate<silence msec=""400""/>{2}"
									, raw.EngExample, raw.Eng, raw.EngClear);
							else
								writer.WriteLine(@"listen<silence msec=""400""/>{0}<silence msec=""400""/>translate<silence msec=""400""/>{1}", raw.Eng, raw.EngClear);
						}

						#endregion english

						#region russian

						if (-1 == raw.RusNorm.IndexOf('(') || sentenceMode)
							writer.WriteLine(@"<silence msec=""{1}""/><lang langid=""419"">{0}</lang>"
								, raw.RusNorm, minPause);
						else
							writer.WriteLine(
								@"<silence msec=""{2}""/><lang langid=""419"">{0}</lang><silence msec=""700""/><lang langid=""419"">{1}</lang>"
								, raw.RusClear, raw.RusNorm, minPause);

						#endregion russian

						if (isRusExample && raw.RusExampleComparable != raw.RusComparable)
							writer.WriteLine(@"<silence msec=""1500""/><lang langid=""419"">{0}</lang>", raw.RusExample);

						writer.WriteLine(@"<silence msec=""{0}""/>", silentPause);
						writer.WriteLine();
					}
					writer.WriteLine(
						@"The lesson is over.<silence msec=""400""/>Come back and listen to me again!<silence msec=""1000""/>");
				}
			}
		}

		public static void WriteSamples(string outFile, IEnumerable<Raw> data, int minPause, int silentPause, bool sentenceMode)
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

		public static void WriteRuEn(string outFile, IEnumerable<Raw> data, int minPause, int silentPause, bool sentenceMode)
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
							writer.WriteLine(@"<silence msec=""1000""/><lang langid=""419"">{0}</lang>", raw.RusNorm);
						else if (-1 == raw.RusNorm.IndexOf('('))
							writer.WriteLine(@"<silence msec=""1000""/><lang langid=""419"">{0} {1}</lang>", raw.RusNorm, raw.AddRusPhrase);
						/*writer.WriteLine(@"{0}<silence msec=""400""/><lang langid=""419"">{1}</lang>", raw.RusPhraseOrQuestion, raw.RusTran);*/
						else
							writer.WriteLine(
								@"listen<silence msec=""400""/><lang langid=""419"">{0}</lang><silence msec=""400""/>translate{2}<silence msec=""400""/><lang langid=""419"">{1} {3}</lang>"
								, raw.RusNorm, raw.RusClear, raw.AddPhraseOrQuestion, raw.AddRusPhrase);

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
}