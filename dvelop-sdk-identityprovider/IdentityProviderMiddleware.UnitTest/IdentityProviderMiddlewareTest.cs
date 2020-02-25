using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Dvelop.Sdk.IdentityProvider.Client;
using Dvelop.Sdk.IdentityProvider.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dvelop.Sdk.IdentityProviderMiddleware.UnitTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class IdentityProviderMiddlewareTest
    {
        private Mock<FakeHttpMessageHandler> _fakeMessageHandler;
        private const string DEFAULT_SYSTEM_BASE_URI = "https://default.mydomain.de";

        private const string VALID_AUTH_SESSION_ID =
            "aXGxJeb0q+/fS8biFi8FE7TovJPPEPyzlDxT6bh5p5pHA/x7CEi1w9egVhEMz8IWhrtvJRFnkSqJnLr61cOKf/i5eWuu7Duh+OTtTjMOt9w=&Bnh4NNU90wH_OVlgbzbdZOEu1aSuPlbUctiCdYTonZ3Ap_Zd3bVL79I-dPdHf4OOgO8NKEdqyLsqc8RhAOreXgJqXuqsreeI";

        private const string SECOND_VALID_AUTH_SESSION_ID =
            "bYGxJeb0q+/fS8biFi8FE7TovJPPEPyzlDxT6bh5p5pHA/x7CEi1w9egVhEMz8IWhrtvJRFnkSqJnLr61cOKf/i5eWuu7Duh+OTtTjMOt9w=&Bnh4NNU90wH_OVlgbzbdZOEu1aSuPlbUctiCdYTonZ3Ap_Zd3bVL79I-dPdHf4OOgO8NKEdqyLsqc8RhAOreXgJqXuqsreeI";

        private const string VALID_EXTERNAL_AUTH_SESSION_ID =
            "1XGxJeb0q+/fS8biFi8FE7TovJPPEPyzlDxT6bh5p5pHA/x7CEi1w9egVhEMz8IWhrtvJRFnkSqJnLr61cOKf/i5eWuu7Duh+OTtTjMOt9w=&Cnh4NNU90wH_OVlgbzbdZOEu1aSuPlbUctiCdYTonZ3Ap_Zd3bVL79I-dPdHf4OOgO8NKEdqyLsqc8RhAOreXgJqXuqsreeI";

        [TestInitialize]
        public void Setup()
        {
            _fakeMessageHandler = new Mock<FakeHttpMessageHandler>() { CallBase = true };
        }

        private static IEnumerable<object[]> GetTestNoAuthSessionIdData()
        {
            yield return new object[] { "GetRequestAndHtmlAccepted_Should_RedirectToIdp",
                "GET", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1"}}, false};
            yield return new object[] { "AndHeadRequestAndHtmlAccepted_Should_RedirectToIdp",
                "HEAD", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1" }}, false};
            yield return new object[] { "BasicAuthorizationAndGetRequestAndHtmlAccepted_Should_RedirectsToIdp",
                "GET", new Dictionary<string, string>{{"Accept","text/html"},{"Authorization", "Basic adabdk"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1"}}, false};
            yield return new object[] { "OtherCookieAndGetRequestAndHtmlAccepted_Should_RedirectsToIdp", 
                "GET", new Dictionary<string, string>{{"Cookie","AnyCookie=adabdk"},{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1"}}, false};
            yield return new object[] { "PostRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "POST", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false};
            yield return new object[] { "PutRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader", 
                "PUT", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false};
            yield return new object[] { "DeleteRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "DELETE", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false};
            yield return new object[] { "PatchRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "PATCH", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false};
        }
        
        [DynamicData(nameof(GetTestNoAuthSessionIdData), DynamicDataSourceType.Method ,DynamicDataDisplayName = "DisplayName")]
        [DataTestMethod]
        public async Task TestNoAuthSessionId(string testName, string requestMethod, Dictionary<string,string> requestHeader, string requestUri, int expectedStatus,  Dictionary<string,string> expectedResponseHeader,bool allowExternalValidation)
        {
            Console.WriteLine(testName);
            await TestMiddleWare(requestMethod,requestHeader,requestUri,expectedStatus,expectedResponseHeader,allowExternalValidation);
        }

        private static IEnumerable<object[]> GetTestInvalidAuthSessionIdData()
        {
            const string invalidToken = "200e7388-1834-434b-be79-3745181e1457";
            
            yield return new object[] { "GetRequestAndHtmlAccepted_Middleware_RedirectsToIdp", 
                "GET", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1"}}, false};
            yield return new object[] { "HeadRequestAndHtmlAccepted_Middleware_RedirectsToIdp",
                "HEAD", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 302,new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1"}}, false};
            yield return new object[] { "GetRequestAndHtmlNotAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "GET", new Dictionary<string, string>{{"Accept", "application/json"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false};
            yield return new object[] { "PostRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "POST", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false};
            yield return new object[] { "PutRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "PUT", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false};
            yield return new object[] { "DeleteRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "DELETE", new Dictionary<string, string>{{"Accept","text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401,new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false};
            yield return new object[] { "PatchRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "PATCH", new Dictionary<string, string>{{"Accept","text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1",401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false};
            yield return new object[] { "GetRequestAndHtmlAcceptedAndExternalValidation_Middleware_RedirectsToIdp",
                "GET", new Dictionary<string, string>{{"Accept","text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2fa%2fb%253Fq1%3dx%26q2%3d1"}}, true};
        }

        [DynamicData(nameof(GetTestInvalidAuthSessionIdData), DynamicDataSourceType.Method, DynamicDataDisplayName = "DisplayName")]
        [DataTestMethod]
        public async Task TestInvalidAuthSessionId(string testName, string requestMethod,
            Dictionary<string, string> requestHeader, string requestUri, int expectedStatus,
            Dictionary<string, string> expectedResponseHeader, bool allowExternalValidation)
        {
            Console.WriteLine(testName);
            await TestMiddleWare(requestMethod,requestHeader,requestUri,expectedStatus,expectedResponseHeader,allowExternalValidation);
        }

        
        private static IEnumerable<object[]> GetTestNoAuthSessionIdAndGetRequestAndAcceptHeaderIsData()
        {
            yield return new object[]{"", true};
            yield return new object[]{"text/", false};
            yield return new object[]{"text/*", true};
            yield return new object[]{"*/*", true};
            yield return new object[]{"application/json; q=1.0, */*; q=0.8", false}; // GO middleware says true
            yield return new object[]{"text/html", true};
            yield return new object[]{"something/else", false};
            yield return new object[]{"text/html; q=1", true};
            yield return new object[]{"text/html; q=1.0", true};
            yield return new object[]{"text/html; q=0.9", true};
            yield return new object[]{"text/html; q=0", true}; // GO middleware says false
            yield return new object[]{"text/html; q=0.0", true}; // GO middleware says false
            yield return new object[]{"application/json", false};
            yield return new object[]{"application/json; q=1.0, text/html; q=0.9", false}; // GO middleware says true 
            yield return new object[]{"application/json; q=1.0, text/html; q=0", false};
            yield return new object[]{"application/json; q=0.9, text/html; q=1.0", true};
            yield return new object[]{"application/json; q=1.0, text/html; q=0.", false};
            yield return new object[]{"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3",                true};
        }
        
        [DynamicData(nameof(GetTestNoAuthSessionIdAndGetRequestAndAcceptHeaderIsData), DynamicDataSourceType.Method, DynamicDataDisplayName = "DisplayName")]
        [DataTestMethod]
        public async Task GetTestNoAuthSessionIdAndGetRequestAndAcceptHeaderIs(string acceptHeader, bool redirectExpected)
        {
            Console.WriteLine(acceptHeader);
            var requestHeader = new Dictionary<string, string>{{"Accept", acceptHeader}};
            await TestMiddleWare("GET",requestHeader,"/a/b?q1=x&q2=1",redirectExpected?302:401,new Dictionary<string, string>(),false );
        }
        
        
        private async Task TestMiddleWare(string requestMethod, Dictionary<string,string> requestHeader, string requestUri, int expectedStatus,  Dictionary<string,string> expectedResponseHeader,bool allowExternalValidation)
        {
           
            var context = new DefaultHttpContext();
            context.Request.Method = requestMethod;
            context.Request.Path = requestUri;

            foreach (var (key, value) in requestHeader)
            {
                context.Request.Headers.Add(key, new[] { value });
            }
            
            _fakeMessageHandler.Setup(mh => mh.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });
            
            var client = new HttpClient(_fakeMessageHandler.Object);
            
            var feature = new MockResponseFeature();
            context.Features.Set<IHttpResponseFeature>(feature);

            async Task Next(HttpContext ctx)
            {
                Console.WriteLine(ctx.Response.Headers.Count);
                await feature.InvokeCallBack();
            }

            var nextMiddleware = new MiddlewareMock(Next);
            await new IdentityProvider.Middleware.IdentityProviderMiddleware(nextMiddleware.InvokeAsync,
                    new IdentityProviderOptions
                    {
                        BaseAddress = new Uri(DEFAULT_SYSTEM_BASE_URI),
                        TriggerAuthentication = true,
                        AllowExternalValidation = allowExternalValidation,
                        TenantInformationCallback = () => new TenantInformation{ TenantId = "0", SystemBaseUri = "https://localhost"},
                        HttpClient = client
                    })
                .Invoke(context);
            
            context.Response.StatusCode.Should().Be(expectedStatus);
            
            foreach (var (key, value) in expectedResponseHeader)
            {
                context.Response.Headers[key].ToString().Should().BeEquivalentTo(value);
            }
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }
        
        //Used by Tests to create a readable TestName
        public static string DisplayName(MethodInfo methodInfo, object[] data)
        {
            var displayName = $"{data[0]}";
            return string.IsNullOrWhiteSpace(displayName) ? "-" : displayName;
        }

        private class MiddlewareMock 
        {
            public bool HasBeenInvoked { get; private set; }

            // ReSharper disable once UnusedParameter.Local
            public MiddlewareMock(RequestDelegate next) 
            {
            }

            public async Task InvokeAsync(HttpContext context)
            {
                HasBeenInvoked = true;
                context.Response.Body = new MemoryStream();
                context.Response.StatusCode = 200;
                context.Response.Headers.Add("x-dv-test", "value");
                await Task.FromResult(0);
            }
        }
        
        private class MockResponseFeature : IHttpResponseFeature {
            public MockResponseFeature()
            {
                Headers = new HeaderDictionary();
            }
            
            public Stream Body { get; set; }

            public bool HasStarted { get; private set; }

            public IHeaderDictionary Headers { get; set; }

            public string ReasonPhrase { get; set; }

            public int StatusCode { get; set; }

            private Func<object, Task> _callback;
            private object _state;
            
            public void OnCompleted(Func<object, Task> callback, object state) {
                //...No-op
            }

            public void OnStarting(Func<object, Task> callback, object state) {
                _callback = callback;
                _state = state;
            }
          

            public Task InvokeCallBack() {
                HasStarted = true;
                return _callback(_state);
            }
        }
    }
}
