using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dvelop.TenantMiddleware.UnitTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [ExcludeFromCodeCoverage]
    public sealed class UnitUnderTestAttribute : TestCategoryBaseAttribute
    {
        public override IList<string> TestCategories { get; }

        public UnitUnderTestAttribute(Type classUnderTest) : this(classUnderTest.Name) { }

        public UnitUnderTestAttribute(Type classUnderTest, string context) : this(classUnderTest.Name, context) { }

        public UnitUnderTestAttribute(Type classUnderTest, Type context) : this(classUnderTest.Name, context.Name) { }

        public UnitUnderTestAttribute(string unit, string context = null)
        {
            TestCategories = new List<string> { $"Unittests for [{unit}] {context}" };
        }
    }
}
