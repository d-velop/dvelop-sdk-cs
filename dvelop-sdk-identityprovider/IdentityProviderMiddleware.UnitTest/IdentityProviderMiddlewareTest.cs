using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Dvelop.Sdk.IdentityProvider.Client;
using Dvelop.Sdk.IdentityProvider.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
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
            
            yield return
            [
                "IDP-1740_path",
                "GET", new Dictionary<string, string>{{"Accept","text/html"}}, "/hüme", 302, new Dictionary<string, string>
                {
                    {"Location","/identityprovider/login?redirect=%2fh%25C3%25BCme"}
                }, false
            ];
            
            yield return
            [
                "IDP-1740_query",
                "GET", new Dictionary<string, string>{{"Accept","text/html"}}, "/bla?path=%2Fh%C3%BCme", 302, new Dictionary<string, string>
                {
                    {"Location","/identityprovider/login?redirect=%2Fbla%3Fpath%3D%252Fh%25C3%25BCme"}
                }, false
            ];
            
            yield return
            [
                "GetRequestAndHtmlAccepted_Should_RedirectToIdp",
                "GET", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1"}}, false
            ];
            yield return
            [
                "AndHeadRequestAndHtmlAccepted_Should_RedirectToIdp",
                "HEAD", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1" }}, false
            ];
            yield return
            [
                "BasicAuthorizationAndGetRequestAndHtmlAccepted_Should_RedirectsToIdp",
                "GET", new Dictionary<string, string>{{"Accept","text/html"},{"Authorization", "Basic adabdk"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1"}}, false
            ];
            yield return
            [
                "OtherCookieAndGetRequestAndHtmlAccepted_Should_RedirectsToIdp", 
                "GET", new Dictionary<string, string>{{"Cookie","AnyCookie=adabdk"},{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1"}}, false
            ];
            yield return
            [
                "PostRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "POST", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "PutRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader", 
                "PUT", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "DeleteRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "DELETE", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "PatchRequestAndHtmlAccepted_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "PATCH", new Dictionary<string, string>{{"Accept","text/html"}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"WWW-Authenticate","Bearer" }}, false
            ];
              
        }
        
        [DynamicData(nameof(GetTestNoAuthSessionIdData), DynamicDataSourceType.Method ,DynamicDataDisplayName = "DisplayName")]
        [DataTestMethod]
        public async Task TestNoAuthSessionId(string testName, string requestMethod, Dictionary<string,string> requestHeader, string requestUri, int expectedStatus,  Dictionary<string,string> expectedResponseHeader,bool allowExternalValidation)
        {
            Console.WriteLine(testName);
            await TestMiddleWare(requestMethod,requestHeader,requestUri,false,expectedStatus,expectedResponseHeader,allowExternalValidation).ConfigureAwait(false);
        }

        private static IEnumerable<object[]> GetTestInvalidAuthSessionIdData()
        {
            const string invalidToken = "200e7388-1834-434b-be79-3745181e1457";
            
            yield return
            [
                "GetRequestAndHtmlAccepted_Middleware_RedirectsToIdp", 
                "GET", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1"}}, false
            ];
            yield return
            [
                "HeadRequestAndHtmlAccepted_Middleware_RedirectsToIdp",
                "HEAD", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 302,new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1"}}, false
            ];
            yield return
            [
                "GetRequestAndHtmlNotAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "GET", new Dictionary<string, string>{{"Accept", "application/json"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "PostRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "POST", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "PutRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "PUT", new Dictionary<string, string>{{"Accept", "text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "DeleteRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "DELETE", new Dictionary<string, string>{{"Accept","text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 401,new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "PatchRequestAndHtmlAccepted_Middleware_ReturnsStatus401AndWWW-AuthenticateBearerHeader",
                "PATCH", new Dictionary<string, string>{{"Accept","text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1",401, new Dictionary<string, string>{{"Www-Authenticate","Bearer" }}, false
            ];
            yield return
            [
                "GetRequestAndHtmlAcceptedAndExternalValidation_Middleware_RedirectsToIdp",
                "GET", new Dictionary<string, string>{{"Accept","text/html"}, {"Authorization", "Bearer " + invalidToken}}, "/a/b?q1=x&q2=1", 302, new Dictionary<string, string>{{"Location","/identityprovider/login?redirect=%2Fa%2Fb%3Fq1%3Dx%26q2%3D1"}}, true
            ];
        }

        [DynamicData(nameof(GetTestInvalidAuthSessionIdData), DynamicDataSourceType.Method, DynamicDataDisplayName = "DisplayName")]
        [DataTestMethod]
        public async Task TestInvalidAuthSessionId(string testName, string requestMethod,
            Dictionary<string, string> requestHeader, string requestUri, int expectedStatus,
            Dictionary<string, string> expectedResponseHeader, bool allowExternalValidation)
        {
            Console.WriteLine(testName);
            await TestMiddleWare(requestMethod,requestHeader,requestUri,false,expectedStatus,expectedResponseHeader,allowExternalValidation).ConfigureAwait(false);
        }

        private static IEnumerable<object[]> GetTestNoAuthSessionIdAndGetRequestAndAcceptHeaderIsData()
        {
            yield return ["", true, true];
            yield return ["text/", true, false];
            yield return ["text/*",true,  true];
            yield return ["*/*", true, true];
            yield return ["application/json; q=1.0, */*; q=0.8",true,  false]; // GO middleware says true
            yield return ["text/html", true, true];
            yield return ["something/else", true, false];
            yield return ["text/html; q=1", true, true];
            yield return ["text/html; q=1.0",true,  true];
            yield return ["text/html; q=0.9",true,  true];
            yield return ["text/html; q=0",true,  true]; // GO middleware says false
            yield return ["text/html; q=0.0",true,  true]; // GO middleware says false
            yield return ["application/json", true, false];
            yield return ["application/json; q=1.0, text/html; q=0.9",true,  false]; // GO middleware says true 
            yield return ["application/json; q=1.0, text/html; q=0", true, false];
            yield return ["application/json; q=0.9, text/html; q=1.0", true, true];
            yield return ["application/json; q=1.0, text/html; q=0.",true,  false];
            yield return ["text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3", true, true];
            
            
            yield return ["", false, true];
            yield return ["text/", false, false];
            yield return ["text/*", false, true];
            yield return ["*/*", false, true];
            yield return ["application/json; q=1.0, */*; q=0.8", false, false]; // GO middleware says true
            yield return ["text/html", false, true];
            yield return ["something/else", false, false];
            yield return ["text/html; q=1", false, true];
            yield return ["text/html; q=1.0", false, true];
            yield return ["text/html; q=0.9", false, true];
            yield return ["text/html; q=0", false, true]; // GO middleware says false
            yield return ["text/html; q=0.0", false, true]; // GO middleware says false
            yield return ["application/json", false, false];
            yield return ["application/json; q=1.0, text/html; q=0.9", false, false]; // GO middleware says true 
            yield return ["application/json; q=1.0, text/html; q=0", false, false];
            yield return ["application/json; q=0.9, text/html; q=1.0", false, true];
            yield return ["application/json; q=1.0, text/html; q=0.", false, false];
            yield return ["text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3", false, true];
        }
        
        [DynamicData(nameof(GetTestNoAuthSessionIdAndGetRequestAndAcceptHeaderIsData), DynamicDataSourceType.Method, DynamicDataDisplayName = "DisplayName")]
        [DataTestMethod]
        public async Task GetTestNoAuthSessionIdAndGetRequestAndAcceptHeaderIs(string acceptHeader,bool anonymousAllowed, bool redirectExpected)
        {
            
            Console.WriteLine(acceptHeader);
            var requestHeader = new Dictionary<string, string>{{"Accept", acceptHeader}};
            await TestMiddleWare("GET",
                requestHeader,
                "/a/b?q1=x&q2=1",
                anonymousAllowed,
                redirectExpected?302:401,
                new Dictionary<string, string>(),
                false ).ConfigureAwait(false);
        }
        
        
        private async Task TestMiddleWare(string requestMethod, Dictionary<string,string> requestHeader, string requestUri, bool allowAnonymous, int expectedStatus,  Dictionary<string,string> expectedResponseHeader,bool allowExternalValidation)
        {
            var uri = new Uri(requestUri, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                uri =new Uri(new Uri("http://localhost/", UriKind.Absolute), uri);
            }
          
            
            var context = new DefaultHttpContext
            {
                Request =
                {
                    Method = requestMethod,
                    Path = uri.AbsolutePath
                }
            };


            var y = HttpUtility.ParseQueryString(uri.Query);
            foreach (var s in y.AllKeys)
            {
                context.Request.QueryString = context.Request.QueryString.Add(s, y[s]);
            }

            
            //TODO: Path und Query splitten und einbauen

            foreach (var (key, value) in requestHeader)
            {
                context.Request.Headers[key] = new[] { value };
            }
            
            _fakeMessageHandler.Setup(mh => mh.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(""),
            });
            
            var client = new HttpClient(_fakeMessageHandler.Object);
            
            var feature = new MockResponseFeature();
            context.Features.Set<IHttpResponseFeature>(feature);

            var endpointFeatureMock = new Mock<IEndpointFeature>();
            
            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(new RouteEndpoint(_ => Task.CompletedTask, 
                    RoutePatternFactory.Parse("/"), 0, 
                    new EndpointMetadataCollection(allowAnonymous?new AllowAnonymousAttribute():new AuthorizeAttribute()), "Dummy"));
            
            context.Features.Set(endpointFeatureMock.Object);
            
            async Task Next(HttpContext ctx)
            {
                Console.WriteLine(ctx.Response.Headers.Count);
                await feature.InvokeCallBack().ConfigureAwait(false);
            }

            var nextMiddleware = new MiddlewareMock(Next);
            await new IdentityProvider.Middleware.IdentityProviderMiddleware(nextMiddleware.InvokeAsync,
                    new IdentityProviderOptions
                    {
                        BaseAddress = new Uri(DEFAULT_SYSTEM_BASE_URI),
                        AllowExternalValidation = allowExternalValidation,
                        TenantInformationCallback = () => new TenantInformation{ TenantId = "0", SystemBaseUri = "https://localhost"},
                        HttpClient = client
                    })
                .Invoke(context).ConfigureAwait(false);
            
            context.Response.StatusCode.Should().Be(expectedStatus);
            
            
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should not have been invoked if signature is wrong");
            foreach (var (key, value) in expectedResponseHeader)
            {
                context.Response.Headers[key].ToString().Should().BeEquivalentTo(value);
            }
        }
        
        //Used by Tests to create a readable TestName
        public static string DisplayName(MethodInfo methodInfo, object[] data)
        {
            var displayName = $"{data[0]} ({string.Join( ", ", data.Skip(1)  )})";
            return string.IsNullOrWhiteSpace(displayName) ? "-" : displayName;
        }

        [Authorize]
        private class MiddlewareMock(RequestDelegate next)
        {
            public bool HasBeenInvoked { get; private set; }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    HasBeenInvoked = true;
                    context.Response.Body = new MemoryStream();
                    context.Response.StatusCode = 401;

                    await next(context).ConfigureAwait(false);
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        
        private class MockResponseFeature : IHttpResponseFeature {
            public Stream Body { get; set; }

            public bool HasStarted { get; private set; }

            public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

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
