using System.Diagnostics.CodeAnalysis;
using Dvelop.Sdk.BaseInterfaces;

namespace Dvelop.Sdk.ApplicationInformation.UnitTest;

[TestFixture]
[ExcludeFromCodeCoverage]
public class SemVerTest
{
    [Test]
    [TestCase(null, 0, 0, 0, "")]
    [TestCase("", 0, 0, 0, "")]
    [TestCase("0.1.0+687f3c408315fa16f32708693f88ec0d8ed7f669", 0, 1, 0, "687f3c408315fa16f32708693f88ec0d8ed7f669")]
    [TestCase("0.1.1496-feature-cdm-91-downloadseite-e78aa2d2+e78aa2d221347f0808d612bbeb05bf77de0aa94a", 0, 1, 1496,
        "feature-cdm-91-downloadseite-e78aa2d2")]
    [TestCase("0.1.1496-feature-cdm-91-downloadseite-e78aa2d2", 0, 1, 1496, "feature-cdm-91-downloadseite-e78aa2d2")]
    [TestCase("0.1.1496-main-e78aa2d2", 0, 1, 1496, "main-e78aa2d2")]
    [TestCase("", 0, 0, 0, "")]
    public void Test_FromString(string? input, int major, int minor, int patch, string qualifier)
    {
        var actual = SemVer.FromString(input);
        Assert.Multiple(() =>
        {
            Assert.That(actual.Major, Is.EqualTo(major));
            Assert.That(actual.Minor, Is.EqualTo(minor));
            Assert.That(actual.Patch, Is.EqualTo(patch));
            Assert.That(actual.Qualifier, Is.EqualTo(qualifier));
        });
    }

    [Test]
    [TestCase("0.1.0", "0.1.0", true)]
    [TestCase("0.1.0", "0.1.1", false)]
    [TestCase("0.1.1", "0.1.0", true)]
    public void Test_GTE(string left, string right, bool expected)
    {
        var actual = SemVer.FromString(left) >= SemVer.FromString(right);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("0.1.0", "0.1.0", false)]
    [TestCase("0.1.0", "0.1.1", false)]
    [TestCase("0.1.1", "0.1.0", true)]
    public void Test_GT(string left, string right, bool expected)
    {
        var actual = SemVer.FromString(left) > SemVer.FromString(right);
        Assert.That(actual, Is.EqualTo(expected));
    }


    [Test]
    [TestCase("0.1.0", "0.1.0", true)]
    [TestCase("0.1.0", "0.1.1", true)]
    [TestCase("0.1.1", "0.1.0", false)]
    public void Test_LTE(string left, string right, bool expected)
    {
        var actual = SemVer.FromString(left) <= SemVer.FromString(right);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("0.1.0", "0.1.0", false)]
    [TestCase("0.1.0", "0.1.1", true)]
    [TestCase("0.1.1", "0.1.0", false)]
    public void Test_LT(string left, string right, bool expected)
    {
        var actual = SemVer.FromString(left) < SemVer.FromString(right);
        Assert.That(actual, Is.EqualTo(expected));
    }
}
