namespace CerebelloWebRole.Code
{
    public class JsonError
    {
        public bool success { get; set; }

        public string text { get; set; }

        public bool error { get; set; }

        public string errorType { get; set; }

        public string errorMessage { get; set; }

        public int status { get; set; }
    }
}