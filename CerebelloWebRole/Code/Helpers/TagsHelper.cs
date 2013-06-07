using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CerebelloWebRole.Code
{
    public static class TagsHelper
    {
        /// <summary>
        /// Validates whether the tags within the passed text are valid
        /// </summary>
        /// <param name="tagsText"></param>
        /// <returns></returns>
        public static bool ValidateTagsCountAndFormat(String tagsText)
        {
            if (String.IsNullOrEmpty(tagsText))
                return false;

            List<string> distilledTags = DistillTagFormattedInput(tagsText);
            if (distilledTags.Count > 5 || distilledTags.Count < 1)
                return false;

            foreach (var tagName in distilledTags)
            {
                if (!ValidateSingleTag(tagName))
                    return false;
            }

            return true;
        }

        public static bool ValidateSingleTag(String tagText)
        {
            return !Regex.IsMatch(tagText, @"[\s,]+") && tagText.Length >= 2 && tagText.Length <= 30;
        }

        /// <summary>
        /// Recebe as tags que o usuário entrou para um link e retorna uma lista de strings correspondendo às tags individualmente
        /// </summary>
        /// <param name="rawTagInput"></param>
        /// <param name="isAdministrator"></param>
        /// <returns></returns>
        public static List<string> DistillTagFormattedInput(string rawTagInput)
        {
            // transformo em lower case e removo qualquer acento
            rawTagInput = rawTagInput.ToLower();
            rawTagInput = StringHelper.RemoveDiacritics(rawTagInput);

            rawTagInput = new string((from c in rawTagInput where char.IsLetterOrDigit(c) || new char[] { '-', ' ', ',', '&' }.Contains(c) select c).ToArray());

            string[] tagArray = Regex.Split(rawTagInput, @"\s*,\s*").Where(t => !String.IsNullOrEmpty(t) && !Regex.IsMatch(t, @"\s+")).ToArray();
            List<string> tags = new List<string>();
            foreach (string tag in tagArray)
            {
                // removo os espaços no início e no fim da frase
                var processedTag = tag;
                // removo múltiplas ocorrências de - por uma única ocorrência
                processedTag = Regex.Replace(processedTag, @"([-]+)", x => "-");

                if (!tags.Contains(processedTag))
                    tags.Add(processedTag);
            }

            return tags;
        }
    }
}
