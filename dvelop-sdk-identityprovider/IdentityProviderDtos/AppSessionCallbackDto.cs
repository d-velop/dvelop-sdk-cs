namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class AppSessionCallbackDto
    {
        public string AuthSessionId { get; set; }
        public string Expire { get; set; }
        public string Sign { get; set; }
    }
}