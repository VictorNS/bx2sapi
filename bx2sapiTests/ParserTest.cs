using System;
using System.Collections.Generic;
using System.IO;
using bx2sapi;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bx2sapiTests
{
	[TestClass]
	public class ParserTest
	{
		[TestMethod]
		public void SimplestTest()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст",
					RusNorm = "Текст",
					RusClear = "Текст",
					RusComparable = "текст",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		#region EmptyData

		[TestMethod]
		public void AllEmptyData()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "", Eng = "", RusExample = "", EngExample = ""}
			};
			Action act = () => FileParser.ParseData(actual, false);
			act.ShouldThrow<InvalidDataException>().WithMessage("Can't parse row with columns [0]='' [2]=''");
		}

		[TestMethod]
		public void RusEmptyData()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "", Eng = "text", RusExample = "", EngExample = ""}
			};
			Action act = () => FileParser.ParseData(actual, false);
			act.ShouldThrow<InvalidDataException>().WithMessage("Can't parse row with columns [0]='text' [2]=''");
		}

		[TestMethod]
		public void EngEmptyData()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "текст", Eng = "", RusExample = "", EngExample = ""}
			};
			Action act = () => FileParser.ParseData(actual, false);
			act.ShouldThrow<InvalidDataException>().WithMessage("Can't parse row with columns [0]='' [2]='текст'");
		}

		#endregion EmptyData

		#region Rus

		[TestMethod]
		public void IsPhrase()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "текст (фраза) текст", Eng = "text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = true,
					Rus = "текст  текст",
					RusNorm = "текст текст",
					RusClear = "текст текст",
					RusComparable = "тексттекст",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "text",
					EngClear = "text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RusRemoveAllCurves()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "(1)текст (2) текст(3)(4)", Eng = "text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "(1)текст (2) текст(3)(4)",
					RusNorm = "(1) текст (2) текст (3) (4)",
					RusClear = "текст текст",
					RusComparable = "1текст2текст34",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "text",
					EngClear = "text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RusComment()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "платить(за секс) [комментарий]", Eng = "text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "платить(за секс) [комментарий]",
					RusNorm = "платить (за секс) [комментарий]",
					RusClear = "платить [комментарий]",
					RusComparable = "платитьзасекс",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "text",
					EngClear = "text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RusExample()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "текст", Eng = "text", RusExample = "Меня зовут Вася(Vasj). [примечание]", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "текст",
					RusNorm = "текст",
					RusClear = "текст",
					RusComparable = "текст",
					RusExample = "Меня зовут Вася(Vasj). [примечание]",
					RusExampleComparable = "менязовутвасяvasj",
					Eng = "text",
					EngClear = "text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		#endregion Rus

		#region Eng

		[TestMethod]
		public void EngExample()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "текст", Eng = "text", RusExample = "", EngExample = "Hi my name is Vasj(Вася). [remark]"}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "текст",
					RusNorm = "текст",
					RusClear = "текст",
					RusComparable = "текст",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "text",
					EngClear = "text",
					EngComparable = "text",
					EngExample = "Hi my name is Vasj(Вася). [remark]",
					EngExampleComparable = "himynameisvasjвася"
				}
			});
		}

		[TestMethod]
		public void EngRemoveAllCurves()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "текст", Eng = "(1)text (2) text(3)", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "текст",
					RusNorm = "текст",
					RusClear = "текст",
					RusComparable = "текст",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "(1)text (2) text(3)",
					EngClear = "text text",
					EngComparable = "1text2text3",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		#endregion Eng

		#region RegexpHelper

		[TestMethod]
		public void RegexpHelperInformal()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(разг)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(разг)",
					RusNorm = "Текст[разговорный вариант]",
					RusClear = "Текст[разговорный вариант]",
					RusComparable = "текстразг",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperAmerican()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(амер)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(амер)",
					RusNorm = "Текст[американский вариант]",
					RusClear = "Текст[американский вариант]",
					RusComparable = "текстамер",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperBritish()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(брит)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(брит)",
					RusNorm = "Текст[британский вариант]",
					RusClear = "Текст[британский вариант]",
					RusComparable = "текстбрит",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperGerman()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(нем)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(нем)",
					RusNorm = "Текст[из немецкого]",
					RusClear = "Текст[из немецкого]",
					RusComparable = "текстнем",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperPastTense()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "(я)получила(10 фунтов)(пр вр)", Eng = "text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "(я)получила(10 фунтов)(пр вр)",
					RusNorm = "(я) получила (10 фунтов) [прошедшее время]",
					RusClear = "получила [прошедшее время]",
					RusComparable = "яполучила10фунтовпрвр",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "text",
					EngClear = "text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperSingular()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(ед ч)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(ед ч)",
					RusNorm = "Текст[единственное число]",
					RusClear = "Текст[единственное число]",
					RusComparable = "текстедч",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperPluralNoun()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(мн ч)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(мн ч)",
					RusNorm = "Текст[множественное число]",
					RusClear = "Текст[множественное число]",
					RusComparable = "текстмнч",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperBiology()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(биол)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(биол)",
					RusNorm = "Текст[из биологии]",
					RusClear = "Текст[из биологии]",
					RusComparable = "текстбиол",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperJurisprudence()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(юр)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(юр)",
					RusNorm = "Текст[из юридического]",
					RusClear = "Текст[из юридического]",
					RusComparable = "текстюр",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperAnatomy()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(анат)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(анат)",
					RusNorm = "Текст[из анатомии]",
					RusClear = "Текст[из анатомии]",
					RusComparable = "текстанат",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		[TestMethod]
		public void RegexpHelperRare()
		{
			var actual = new List<Raw>
			{
				new Raw { Rus = "Текст(редко)", Eng = "Text", RusExample = "", EngExample = ""}
			};

			FileParser.ParseData(actual, false);

			actual.ShouldAllBeEquivalentTo(new List<Raw>
			{
				new Raw
				{
					IsPhrase = false,
					Rus = "Текст(редко)",
					RusNorm = "Текст[редко используется]",
					RusClear = "Текст[редко используется]",
					RusComparable = "текстредко",
					RusExample = "",
					RusExampleComparable = "",
					Eng = "Text",
					EngClear = "Text",
					EngComparable = "text",
					EngExample = "",
					EngExampleComparable = ""
				}
			});
		}

		#endregion RegexpHelper

	}
}
