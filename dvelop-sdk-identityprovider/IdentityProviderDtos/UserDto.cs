using System.Collections.Generic;

namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public NameDto Name { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Locale { get; set; }
        public string PreferredLanguage { get; set; }
        public IEnumerable<ValueDto> Emails { get; set; }
        public IEnumerable<ValueDto> PhoneNumbers { get; set; }
        public IEnumerable<ValueDto> Groups { get; set; }
        public IEnumerable<ValueDto> Photos { get; set; }

        public IEnumerable<DetailDto> Details { get; set; }
    }
}
