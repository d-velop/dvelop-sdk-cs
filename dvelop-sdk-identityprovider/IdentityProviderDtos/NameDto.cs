namespace Dvelop.Sdk.IdentityProvider.Dto
{
    public class NameDto
    {
        public NameDto(string familyName=null, string givenName=null)
        {
            FamilyName = familyName;
            GivenName = givenName;
        }

        public string FamilyName { get; set; }
        public string GivenName { get; set; }
    }
}
