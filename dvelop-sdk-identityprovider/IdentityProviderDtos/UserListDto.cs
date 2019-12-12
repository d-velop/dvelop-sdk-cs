using System.Collections.Generic;
using System.Linq;

namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class UserListDto
    {
        public string Schema => "urn:scim:schemas:core:1.0";

        public int TotalResults { get; set; }

        public int ItemsPerPage => Resources.Count();

        public int StartIndex { get; set; }

        public IEnumerable<UserDto> Resources { get; set; }
    }
}
