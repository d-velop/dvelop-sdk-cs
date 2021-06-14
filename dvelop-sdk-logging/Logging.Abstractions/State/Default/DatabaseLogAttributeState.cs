using System.Collections.Generic;
using System.Globalization;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State.Default
{
    public class DatabaseLogAttributeState : CustomLogAttributeState
    {
        public double TimeUsed { get; set; }
        public string Name { get; set; }
        public string Statement { get; set; }
        public string Operation { get; set; }

        public override IEnumerable<CustomLogAttribute> Attributes
        {
            get
            {
                var attributes = new List<CustomLogAttribute>();

                if (TimeUsed > 0)
                {
                    attributes.Add(new CustomLogAttributeProperty("timeUsed", TimeUsed.ToString(CultureInfo.InvariantCulture)));
                }

                var databaseAttributes = new List<CustomLogAttribute>();

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    databaseAttributes.Add(new CustomLogAttributeProperty("name", Name));
                }
                if (!string.IsNullOrWhiteSpace(Statement))
                {
                    databaseAttributes.Add(new CustomLogAttributeProperty("statement", Statement));
                }
                if (!string.IsNullOrWhiteSpace(Operation))
                {
                    databaseAttributes.Add(new CustomLogAttributeProperty("operation", Operation));
                }

                attributes.Add(new CustomLogAttributeObject("db", databaseAttributes));

                return attributes;
            }
        }
    }
}
