using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [ExcludeFromCodeCoverage]
    public sealed class UnitUnderTestAttribute : CategoryAttribute
    {
        public UnitUnderTestAttribute(Type classUnderTest) : base($"Unittests for [{classUnderTest.Name}]") { }

        public UnitUnderTestAttribute(Type classUnderTest, string context) : base($"Unittests for [{classUnderTest.Name}] {context}") { }

        public UnitUnderTestAttribute(Type classUnderTest, Type context) : base($"Unittests for [{classUnderTest.Name}] {context.Name}") { }
    }
}
