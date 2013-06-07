namespace CerebelloWebRole.Code
{
    public class AccessTokenJsonResult
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }
}