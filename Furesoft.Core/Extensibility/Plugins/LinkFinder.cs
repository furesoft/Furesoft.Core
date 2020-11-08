using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Furesoft.Core.Extensibility.Plugins
{
	internal class LinkFinder
	{
		public static List<LinkItem> Find(string file, string link)
		{
			var list = new List<LinkItem>();
			var url = new Uri(link).GetLeftPart(UriPartial.Path);

			// 1.
			// Find all matches in file.
			MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
											   RegexOptions.Singleline);

			// 2.
			// Loop over each match.
			foreach (Match m in m1)
			{
				string value = m.Groups[1].Value;
				LinkItem i = new LinkItem();

				// 3.
				// Get href attribute.
				Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
									   RegexOptions.Singleline);
				if (m2.Success)
				{
					i.Href = url + m2.Groups[1].Value;
				}

				// 4.
				// Remove inner tags from text.
				string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
										 RegexOptions.Singleline);
				i.Text = t;
				if (!i.Text.Contains("/") && i.Href != url + "?C=N;O=D" && i.Href != url + "?C=M;O=A" && i.Href != url + "?C=S;O=A" && i.Href != url + "?C=D;O=A")
					list.Add(i);
			}
			return list;
		}
	}
}