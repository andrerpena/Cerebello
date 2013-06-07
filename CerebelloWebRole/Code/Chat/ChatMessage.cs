namespace CerebelloWebRole.Code
{
    public class ChatMessage
    {
        public ChatUser UserFrom { get; set; }
        public ChatUser UserTo { get; set; }
        public long Timestamp { get; set; }
        public string Message { get; set; }
        public string ClientGuid { get; set; }
    }
}