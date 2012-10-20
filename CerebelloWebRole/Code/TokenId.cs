using System;
using System.Text.RegularExpressions;

namespace CerebelloWebRole.Code
{
    public class TokenId
    {
        public TokenId(string tokenIdAndValue)
        {
            var match = Regex.Match(tokenIdAndValue, @"^(?<ID>\d+)-(?<VALUE>[\da-fA-F]{32})$");
            if (match.Success)
            {
                this.Id = int.Parse(match.Groups["ID"].Value);
                this.Value = match.Groups["VALUE"].Value;
            }
            else
            {
                throw new ArgumentException("Invalid value of tokenIdAndValue.", "tokenIdAndValue");
            }
        }

        public TokenId(int id, string token)
        {
            this.Id = id;
            this.Value = token;
        }

        /// <summary>
        /// Unique identity number of the token.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 32 char token.
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}", this.Id, this.Value);
        }
    }
}