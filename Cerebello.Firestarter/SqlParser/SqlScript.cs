using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Cerebello.Firestarter.SqlParser
{
	public class SqlScript
	{
		private readonly List<SqlItem> items = new List<SqlItem>();

		public List<SqlItem> Items
		{
			get
			{
				return this.items;
			}
		}

		public string TopItem
		{
			get;
			set;
		}

		public SqlScript()
		{
		}

		public void ExtractObjectsFromTables()
		{
			bool flag;
			SqlItem[] array = this.Items.ToArray();
			for (int i = 0; i < (int)array.Length; i++)
			{
				SqlItem sqlItem = array[i];
				SqlKinds? kind = sqlItem.Kind;
				flag = (kind.GetValueOrDefault() != SqlKinds.Table ? false : kind.HasValue);
				if (flag)
				{
					Regex regex = new Regex(@"
(?<CONTENT>
    CREATE \s*[^\n\r]*?\s* INDEX \s*
        (?<FULLNAME>
            \[
            (?<OBJECT>
                .*?
            )
            \]
        )
    (?:
        .*?
        \bGO\b
    )
)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
					MatchCollection matchCollections = regex.Matches(sqlItem.Content);
					foreach (Match match in matchCollections)
					{
						SqlItem nullable = new SqlItem();
						nullable.Kind = new SqlKinds?(SqlKinds.Index);
						nullable.KindStr = "Index";
						nullable.Name = match.Groups["FULLNAME"].Value;
						nullable.Content = match.Groups["CONTENT"].Value;
						this.items.Add(nullable);
					}
					sqlItem.Content = regex.Replace(sqlItem.Content, "");
				}
			}
		}

		public void Load(string contents)
		{
			SqlKinds sqlKind;
			SqlKinds? nullable;
            Match match = Regex.Match(contents, @"^(?<CONTENT>.*?(?=(?:/\*+\s*(?:Begin\s)?Object:)|$))", RegexOptions.Singleline);
			this.TopItem = match.Value.Trim();
			MatchCollection matchCollections = Regex.Matches(contents, @"
/\*+\s*(?:Begin\s)?Object:\s*
(?<KIND>
    .*?
)
\s*
(?<FULLNAME>
    (?:
        \[
        (?<SCHEMA>
            [^\n\r]*?
        )
        \]\.
    )?
    (?:
        \[
        (?<OBJECT>
            [^\n\r]*?
        )
        \]
    )
)
(?:
    \s+Script\s+Date:\s*
    (?<DATE>
        \d\d/\d\d/\d{4}\s+\d\d:\d\d:\d\d
    )
)?
\s*\*+/
(?<CONTENT>
    .*?
    (?=
        (?:
            /\*+\s*(?:End\s)?Object:
        )
        |
        $
    )
)
", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
			this.items.Clear();
			foreach (Match match1 in matchCollections)
			{
				bool flag = Enum.TryParse<SqlKinds>(match1.Groups["KIND"].Value, out sqlKind);
				List<SqlItem> sqlItems = this.items;
				SqlItem sqlItem = new SqlItem();
				SqlItem sqlItem1 = sqlItem;
				if (flag)
				{
					nullable = new SqlKinds?(sqlKind);
				}
				else
				{
					SqlKinds? nullable1 = null;
					nullable = nullable1;
				}
				sqlItem1.Kind = nullable;
				sqlItem.KindStr = match1.Groups["KIND"].Value;
				sqlItem.Name = match1.Groups["FULLNAME"].Value;
				sqlItem.Content = match1.Groups["CONTENT"].Value;
				sqlItems.Add(sqlItem);
			}
		}

		public override string ToString()
		{
			object kindStr;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(this.TopItem);
			foreach (SqlItem item in this.Items)
			{
				stringBuilder.AppendLine();
				StringBuilder stringBuilder1 = stringBuilder;
				string str = "/****** Begin Object:  {0} {1} ******/\r\n{2}\r\n/****** End Object:  {0} {1} ******/";
				SqlKinds? kind = item.Kind;
				if (!kind.HasValue)
				{
					kindStr = item.KindStr;
				}
				else
				{
					SqlKinds? nullable = item.Kind;
					kindStr = nullable.ToString();
				}
				stringBuilder1.AppendLine(string.Format(str, kindStr, item.Name, item.Content.Trim()).Trim());
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}