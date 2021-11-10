using System;
using System.Text.RegularExpressions;
using Dvelop.Sdk.Dashboard.Dto.Registration.Base;

namespace Dvelop.Sdk.Dashboard.Dto.Registration
{
    public class WidgetRegistrationDto : AbstractDashboardDto
    {
        private string _id;

        public string Id
        {
            get => _id;
            set
            {
                AssertId(value);
                _id = value;
            }
        }

        public WidgetBaseDto Widget { get; set; }

        public PermissionDto Permission { get; set; }

        private static void AssertId(string id)
        {
            var idRegex = new Regex("([a-z-]+/[a-z][a-z0-9]{1,12})$");

            if (!idRegex.IsMatch(id))
            {
                throw new ArgumentException($"{nameof(Id)} does not match schme.");
            }
        }

    }
}
