namespace Cerebello.Firestarter.SqlParser
{
	public class SqlItem
	{
		public string Content
		{
			get;
			set;
		}

		public SqlKinds? Kind
		{
			get;
			set;
		}

		public string KindStr
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public SqlItem()
		{
		}

		public override string ToString()
		{
			object kindStr;
			string str = "{0} {1}";
			SqlKinds? kind = this.Kind;
			if (!kind.HasValue)
			{
				kindStr = this.KindStr;
			}
			else
			{
				SqlKinds? nullable = this.Kind;
				kindStr = nullable.ToString();
			}
			return string.Format(str, kindStr, this.Name);
		}
	}
}