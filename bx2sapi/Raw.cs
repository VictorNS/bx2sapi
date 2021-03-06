﻿using System;

namespace bx2sapi
{
	public class Raw
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

		/// <summary>
		/// Английское слово
		/// </summary>
		public string Eng { get; set; }
		/// <summary>
		/// Перевод
		/// </summary>
		public string Rus { get; set; }
		/// <summary>
		/// Английский пример
		/// </summary>
		public string EngExample { get; set; }
		/// <summary>
		/// Перевод примера
		/// </summary>
		public string RusExample { get; set; }
		/// <summary>
		/// Текст, с заменёнными аббревиатурами и очищенный от лишних пробелов
		/// </summary>
		public string RusNorm { get; set; }
		/// <summary>
		/// Текст, с заменёнными аббревиатурами, очищенный от текста в скобках, а также лишних пробелов, точек и запятых
		/// </summary>
		public string RusClear { get; set; }

		/// <summary>
		/// Текст, очищенный от текста в скобках, а также лишних пробелов, точек и запятых
		/// </summary>
		public string EngClear { get; set; }
		/// <summary>
		/// Текст, очищенный от скобкок, и всех пробелов
		/// </summary>
		public string EngComparable { get; set; }
		/// <summary>
		/// Текст, очищенный от скобкок, и всех пробелов
		/// </summary>
		public string EngExampleComparable { get; set; }
		/// <summary>
		/// Текст, очищенный от скобкок, и всех пробелов
		/// </summary>
		public string RusComparable { get; set; }
		/// <summary>
		/// Текст, очищенный от скобкок, и всех пробелов
		/// </summary>
		public string RusExampleComparable { get; set; }

		/// <summary>
		/// фраза, вопрос, предложение
		/// </summary>
		public SentenceType PhraseOrQuestion { get; set; }
		/// <summary>
		/// Готовое окончание предложения, может принимать значения: "", " the phrase", " the question", " the sentence"
		/// </summary>
		public string EngPostfixPhraseOrQuestion
		{
			get
			{
				return
					PhraseOrQuestion == SentenceType.Sentence ? " the sentence" :
					PhraseOrQuestion == SentenceType.Question ? " the question" :
					PhraseOrQuestion == SentenceType.Phrase ? " the phrase" : "";
			}
		}
		/// <summary>
		/// Готовое окончание предложения, может принимать значения: "", " фраза", " вопрос", " предложение"
		/// </summary>
		public string RusPostfixPhraseOrQuestion
		{
			get
			{
				return
					PhraseOrQuestion == SentenceType.Sentence ? " предложение" :
					PhraseOrQuestion == SentenceType.Question ? " вопрос" :
					PhraseOrQuestion == SentenceType.Phrase ? " фраза" : "";
			}
		}

		/// <summary>
		/// При парсинге обнаружено (фраза)
		/// </summary>
		public bool IsPhrase { get; set; }
		/// <summary>
		/// this.IsPhrase ? "(фраза)" : ""
		/// </summary>
		public string RusPostfixPhrase
		{
			get { return this.IsPhrase ? "(фраза)" : ""; }
		}
	}

	public enum SentenceType
	{
		None = 0,
		Phrase = 1,
		Question = 2,
		Sentence = 3
	}
}
