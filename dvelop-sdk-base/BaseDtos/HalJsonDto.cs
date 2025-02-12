using System.Collections.Generic;

namespace Dvelop.Sdk.Base.Dto
{
    public class HalJsonDto
    {
        public HalJsonDto()
        {
            _links = new Dictionary<string, RelationDataDto>();
            _embedded = new Dictionary<string, object>();
        }
        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Collection of link-relations
        /// </summary>
        /// <example>
        /// {
        ///    "self": {
        ///         "href": "/myapp/api/object/123",
        ///         "templated": false
        ///     }
        /// }
        /// </example>
        public Dictionary<string, RelationDataDto> _links { get; set; }
        
        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Collection of embedded resources
        /// </summary>
        /// <example>
        /// {
        ///    "myobject": {
        ///         "someProperty": "someValue"
        ///     }
        /// }
        /// </example>
        public Dictionary<string, object> _embedded { get; set; }
        
        public bool ShouldSerialize_links()
        {
            return _links?.Count > 0;
        }
        public bool ShouldSerialize_embedded()
        {
            return _embedded?.Count > 0;
        }
    }
}
