
namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class ValueDto
    {
        public ValueDto(string value=null, string display=null)
        {
            Value = value;
            Display = display;
        }
        public string Value { get; set; }

        public string Display { get; set; }
    }
}
