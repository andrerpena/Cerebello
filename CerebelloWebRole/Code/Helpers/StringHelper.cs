using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CerebelloWebRole.Code
{
    public static class StringHelper
    {
        public static string CapitalizeFirstLetters(string str, string[] excludeList)
        {
            if (excludeList == null)
                excludeList = new string[0];

            List<String> processedStrings = new List<string>();
            foreach (string s in str.Split(' '))
            {
                if (!excludeList.Contains(s))
                    if (s.Length > 1)
                        processedStrings.Add(char.ToUpper(s[0]).ToString() + s.Substring(1));
                    else if (s.Length == 1)
                        processedStrings.Add(char.ToUpper(s[0]).ToString());
            }

            return string.Join(" ", processedStrings.ToArray());
        }

        public static string RemoveDiacritics(string original)
        {
            string stFormD = original.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(stFormD[ich]);
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static bool ValidateCaptalizedWords(string title)
        {
            String[] vTitleWords = title.Split(' ');
            if (vTitleWords.Length > 1)
            {
                int lowerWordsCount = 0;

                for (int i = 1; i < vTitleWords.Length; i++)
                {
                    String vString = vTitleWords[i];
                    bool hasCapitalLetter = false;
                    for (int j = 1; j < vString.Length; j++)
                    {
                        char vChar = vString[j];
                        if (char.IsUpper(vChar))
                        {
                            hasCapitalLetter = true;
                            break;
                        }
                    }
                    if (!hasCapitalLetter)
                        lowerWordsCount++;
                }

                return ((double)lowerWordsCount / (vTitleWords.Length - 1)) >= 0.50;
            }
            return true;
        }

        /// <summary>
        /// Cria um identificador para a discussão atual sem caracteres especiais
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string GenerateUrlIdentifier(string title)
        {
            String normalizedString = title.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        stringBuilder.Append(c);
                        break;
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        stringBuilder.Append('_');
                        break;
                }
            }

            string result = stringBuilder.ToString();
            result = Regex.Replace(result, @"_+", "_"); // remove duplicate underscores
            return result;
        }

        /// <summary>
        /// Normalizes a UserName, to compare against other normalized user-names in the database.
        /// This is a security measure, the UserName must be typed exactly the same as the user entered the first time,
        /// but no other users may exist with the same normalized user-name.
        /// This leaves a gap, that makes it more difficult to guess what the UserName of a person may be.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static string NormalizeUserName(string userName)
        {
            var normalizedString = userName.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var result = Regex.Replace(normalizedString, @"\W+", "");
            return result;
        }

        /// <summary>
        /// Replaces string using a StringComparison
        /// </summary>
        static public string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }
}
