using System.Collections.Generic;
using System.Globalization;
using Dvelop.Sdk.Logging.Abstractions.State.Attribute;

namespace Dvelop.Sdk.Logging.Abstractions.State.Default
{
    public class DatabaseLogAttributeState : CustomLogAttributeState
    {
        /// <summary>
        /// If no <a href="https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/database.md#call-level-attributes-for-specific-technologies">tech-specific attribute</a>
        /// is defined, this attribute is used to report the name of the database being accessed.
        /// For commands that switch the database, this should be set to the
        /// target database (even if the command fails).
        /// <example><c>customers</c>; <c>main</c></example>
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The database statement being executed.
        /// <example><c>SELECT * FROM wuser_table</c>; <c>SET mykey "WuValue"</c></example>
        /// </summary>
        public string Statement { get; set; }
        /// <summary>
        /// The name of the operation being executed, e.g. the
        /// <a href="https://docs.mongodb.com/manual/reference/command/#database-operations">MongoDB command name</a>
        /// such as findAndModify, or the SQL keyword.
        /// <example><c>findAndModify</c>; <c>HMSET</c>; <c>SELECT</c></example>
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// Measures the duration of the db request in milliseconds.
        /// </summary>
        public long Duration { get; set; }

        public override IEnumerable<CustomLogAttribute> Attributes
        {
            get
            {
                var attributes = new List<CustomLogAttribute>();

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
                if (Duration > 0)
                {
                    databaseAttributes.Add(new CustomLogAttributeProperty("duration", Duration.ToString()));
                }

                attributes.Add(new CustomLogAttributeObject("db", databaseAttributes));

                return attributes;
            }
        }
    }
}
