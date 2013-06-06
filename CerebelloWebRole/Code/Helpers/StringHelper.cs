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

            var processedStrings = new List<string>();
            foreach (string s in str.Split(' ').Where(s => !excludeList.Contains(s)))
            {
                if (s.Length > 1)
                    processedStrings.Add(char.ToUpper(s[0]).ToString() + s.Substring(1));
                else if (s.Length == 1)
                    processedStrings.Add(char.ToUpper(s[0]).ToString());
            }

            return string.Join(" ", processedStrings.ToArray());
        }

        public static string RemoveDiacritics(string original)
        {
            var stFormD = original.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var t in from t in stFormD let uc = CharUnicodeInfo.GetUnicodeCategory(t) where uc != UnicodeCategory.NonSpacingMark select t)
                sb.Append(t);

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static bool ValidateCaptalizedWords(string title)
        {
            var vTitleWords = title.Split(' ');
            if (vTitleWords.Length > 1)
            {
                var lowerWordsCount = 0;

                for (var i = 1; i < vTitleWords.Length; i++)
                {
                    var vString = vTitleWords[i];
                    var hasCapitalLetter = false;
                    for (var j = 1; j < vString.Length; j++)
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
            if (title == null)
                return null;

            var normalizedString = title.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        stringBuilder.Append(char.ToLowerInvariant(c));
                        break;
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        //stringBuilder.Append('_');
                        break;
                }
            }

            string result = stringBuilder.ToString();
            //result = Regex.Replace(result, @"_+", "_"); // remove duplicate underscores
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
            var result = Regex.Replace(normalizedString, @"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}]+", "");
            return result;
        }

        /// <summary>
        /// Replaces string using a StringComparison
        /// </summary>
        static public string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            var sb = new StringBuilder();

            var previousIndex = 0;
            var index = str.IndexOf(oldValue, comparison);
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

        /// <summary>
        /// Replaces fields in the text with the corresponding value in the object.
        /// Fields are denoted by "&lt;%PropertyPath%&gt;".
        /// The PropertyPath can contain a property name or a property path using '.' operator.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="rootObj"></param>
        /// <returns></returns>
        public static string ReflectionReplace(string inputText, object rootObj)
        {
            // supported: properties, indirections
            // unsupported: indexer, dynamic, methods, operators

            if (inputText == null)
                return null;

            var result = Regex.Replace(
                inputText,
                @"<%(.*?)%>",
                m =>
                {
                    var propNames = new Queue<string>(m.Groups[1].Value.Split('.'));
                    object currentObj = rootObj;
                    while (propNames.Count > 0 && currentObj != null)
                    {
                        var propInfo = currentObj.GetType().GetProperty(propNames.Dequeue().Trim());
                        currentObj = propInfo == null ? null : propInfo.GetValue(currentObj, null);
                    }
                    var result2 = string.Format("{0}", currentObj);
                    return result2;
                });

            return result;
        }

        private static bool AllEmpty(this IEnumerable<string> strs)
        {
            return (strs ?? new string[0]).All(string.IsNullOrWhiteSpace);
        }

        public static bool AllFilled(this IEnumerable<string> strs)
        {
            return !AnyEmpty(strs);
        }

        private static bool AnyEmpty(this IEnumerable<string> strs)
        {
            return (strs ?? new string[0]).Any(string.IsNullOrWhiteSpace);
        }

        public static bool AnyFilled(this IEnumerable<string> strs)
        {
            return !AllEmpty(strs);
        }

        /// <summary>
        /// Joins multiple string using separators between them, and using a special last separator.
        /// (e.g. "a, b, c AND d" is the result of Join(", ", " AND ", "a", "b", "c", "d"))
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="lastSeparator"></param>
        /// <param name="itemsToJoin"></param>
        /// <returns></returns>
        public static string Join(string separator, string lastSeparator, params string[] itemsToJoin)
        {
            if (itemsToJoin == null)
                throw new ArgumentNullException("itemsToJoin");

            if (itemsToJoin.Length == 0)
                return "";

            if (itemsToJoin.Length == 1)
                return itemsToJoin[0];

            if (itemsToJoin.Length == 2)
                return itemsToJoin[0] + lastSeparator + itemsToJoin[1];

            return string.Join(separator, itemsToJoin.Take(itemsToJoin.Length - 1)) + lastSeparator + itemsToJoin[itemsToJoin.Length - 1];
        }

        /// <summary>
        /// Gets a decimal value in money format.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatMoney(decimal value)
        {
            var ptBr = CultureInfo.GetCultureInfo("pt-BR");
            return value.ToString("R$ 0.00", ptBr);
        }

        /// <summary>
        /// Indicates whether the string representing a file name is an image file name.
        /// </summary>
        /// <param name="fileName">Name of the file to test.</param>
        /// <returns>Returns true if the file name is an image file name; otherwise false.</returns>
        public static bool IsImageFileName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();
            return fileName.EndsWith(".jpg") || fileName.EndsWith(".png") || fileName.EndsWith(".bmp") || fileName.EndsWith(".jpeg") || fileName.EndsWith(".gif");
        }

        /// <summary>
        /// Indicates whether the string representing a file name is a text document file name.
        /// </summary>
        /// <param name="fileName">Name of the file to test.</param>
        /// <returns>Returns true if the file name is text document file name; otherwise false.</returns>
        public static bool IsDocumentFileName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();
            return fileName.EndsWith(".txt")
                || fileName.EndsWith(".odt")
                || fileName.EndsWith(".pdf")
                || fileName.EndsWith(".xps")
                || fileName.EndsWith(".mht")
                || fileName.EndsWith(".mhtml")
                || fileName.EndsWith(".htm")
                || fileName.EndsWith(".html")
                || fileName.EndsWith(".wri")
                || fileName.EndsWith(".rtf")
                || fileName.EndsWith(".doc")
                || fileName.EndsWith(".docx");
        }

        /// <summary>
        /// Indicates whether the string representing a file name is a spread sheet file name.
        /// </summary>
        /// <param name="fileName">Name of the file to test.</param>
        /// <returns>Returns true if the file name is a spread sheet file name; otherwise false.</returns>
        public static bool IsSpreadSheetFileName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();
            return fileName.EndsWith(".csv")
                || fileName.EndsWith(".xls")
                || fileName.EndsWith(".xlsx")
                || fileName.EndsWith(".ods");
        }

        /// <summary>
        /// Gets the first non-empty string from the list.
        /// </summary>
        /// <param name="strings">Array of string to get the first non-empty entry from.</param>
        /// <returns>The first non-empty string or null if none exists.</returns>
        public static string FirstNonEmpty(params string[] strings)
        {
            if (strings == null)
                return null;

            return strings.FirstOrDefault(s => !string.IsNullOrEmpty(s));
        }

        public static object NormalizeFileName(string fileName)
        {
            return Regex.Replace(StringHelper.RemoveDiacritics(fileName.ToLowerInvariant()), @"\s+", "-");
        }
    }
}
