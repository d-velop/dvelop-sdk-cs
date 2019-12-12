using System.Collections.Generic;

namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class GroupDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<ValueDto> Members { get; set; }
    }
}
