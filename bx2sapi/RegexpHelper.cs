using System.Text.RegularExpressions;

namespace bx2sapi
{
	public class RegexpHelper
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
		public string RemoveTextInBrackets(string input)
		{
			return _rx1.Replace(input, "");
		}
		public string RemoveBrackets(string input)
		{
			return _rx2.Replace(input, "").ToLower().Replace("(", "").Replace(")", "").Replace(".", "").Replace("!", "").Replace("?", "").Replace("’", "").Replace("'", "").Replace(" ", "");
		}
	}
}
