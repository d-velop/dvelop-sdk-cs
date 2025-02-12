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
        /// <summary>
        ///     Serverabsolute URI of the linked resource
        /// </summary>
        /// <example>/myapp/api/object/123</example>
        public string Href { get; set; }

        /// <summary>
        ///     Indication if the href is a template, or can be used as it is
        /// </summary>
        public bool Templated { get; set; }

        public bool ShouldSerializeTemplated()
        {
            return Templated;
        }
    }
}