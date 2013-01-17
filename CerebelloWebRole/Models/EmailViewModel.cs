namespace CerebelloWebRole.Models
{
    public class EmailViewModel
    {
        public string PersonName { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// Full token in the format Id-Guid.
        /// </summary>
        public string Token { get; set; }

        public string PracticeIdentifier { get; set; }

        public bool IsBodyHtml { get; set; }
    }
}
