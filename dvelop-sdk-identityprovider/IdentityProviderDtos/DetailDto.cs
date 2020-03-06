
namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class DetailDto
    {
        public DetailDto(string key = null, string[] values = null)
        {
            Key = key;
            Values = values;
        }
        public string Key { get; set; }
        public string[] Values { get; set; }
    }
}
