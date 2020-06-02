namespace Dvelop.Sdk.Base.Dto
{
    public class RelationDataDto
    {
        public RelationDataDto()
        {
            Templated = false;
        }
        
        public RelationDataDto(string href, bool templated = false)
        {
            Href = href;
            Templated = templated;
        }
        public string Href { get; set; }

        public bool Templated { get; set; }

        public bool ShouldSerializeTemplated()
        {
            return Templated;
        }
    }
}