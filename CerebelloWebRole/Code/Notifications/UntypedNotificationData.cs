namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Class for notifications coming from the DB therefore I don't need to know their
    /// type (they're going to be interpreted by JavaScript)
    /// </summary>
    public class UntypedNotificationData
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}