using System;
using System.IO;

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
				var outFileRuEn = outFile + " Russian English.txt";
				var outFileEnRu = outFile + " English Russian.txt";
				var outFileSamples = outFile + " Samples.txt";
				#endregion verify parameters

				var data = FileParser.ReadDataFromFile(inFile);
				FileParser.ParseData(data, sentenceMode);

				FileWriter.WriteRuEn(outFileRuEn, data, minPause, silentPause, sentenceMode);
				Console.WriteLine(outFileRuEn);

				FileWriter.WriteEnRu(outFileEnRu, data, minPause, silentPause, sentenceMode);
				Console.WriteLine(outFileEnRu);

				FileWriter.WriteSamples(outFileSamples, data, minPause, silentPause, sentenceMode);
				Console.WriteLine(outFileSamples);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.ReadKey();
			}
		}
	}
}
