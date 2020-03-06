using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TenantMiddlewareTest
    {

        private const string SystemBaseUriHeader = "x-dv-baseuri";
        private const string TenantIdHeader = "x-dv-tenant-id";
        private const string SignatureHeader = "x-dv-sig-1";
        private const string DefaultTenantId = "0";
        private const string DefaultSystemBaseUri = "https://default.mydomain.de";

        private readonly byte[] _signatureKey =
        {
            166, 219, 144, 209, 189, 1, 178, 73, 139, 47, 21, 236, 142, 56, 71, 245, 43, 188, 163, 52, 239, 102, 94,
            153, 255, 159, 199, 149, 163, 145, 161, 24
        };
        
        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public void TenantMiddlewareOptionsIsNull_ShouldThrowException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action useMiddleware = () => new TenantMiddleware(new Mock<RequestDelegate>().Object, null);
            useMiddleware.ShouldThrow<ArgumentNullException>().WithMessage("*tenantMiddlewareOptions*");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public void OnTenantIdentifiedCallbackIsNull_ShouldThrowException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action useMiddleware = () => new TenantMiddleware(new Mock<RequestDelegate>().Object,
                new TenantMiddlewareOptions { OnTenantIdentified = null });
            useMiddleware.ShouldThrow<ArgumentNullException>().WithMessage("*OnTenantIdentified*");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public void DefaultSystemBaseUriIsNoValidUri_ShouldThrowException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action useMiddleware = () => new TenantMiddleware(new Mock<RequestDelegate>().Object,
                new TenantMiddlewareOptions
                {
                    OnTenantIdentified = (a, b) => { },
                    DefaultSystemBaseUri = "http:/"
                });
            useMiddleware.ShouldThrow<ArgumentException>().WithMessage("*DefaultSystemBaseUri*");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task BaseUriHeaderAndNullDefaultBaseUri_ShouldUseHeaderAndInvokeNext()
        {
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(SystemBaseUriHeader, new StringValues(new[] {systemBaseUriFromHeader}));
            context.Request.Headers.Add(SignatureHeader, new[] {GetBase64SignatureFor(systemBaseUriFromHeader, _signatureKey)});

            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultSystemBaseUri = null,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { systemBaseUriSetByMiddleware = systemBaseUri; }
                    })
                .InvokeAsync(context);

            systemBaseUriSetByMiddleware.Should().Be(systemBaseUriFromHeader);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        
        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoBaseUriHeaderAndDefaultBaseUri_ShouldUseDefaultBaseUriAndInvokeNext()
        {
            var context = new DefaultHttpContext();
            
            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { systemBaseUriSetByMiddleware = systemBaseUri; }
                    })
                .InvokeAsync(context);

            systemBaseUriSetByMiddleware.Should().Be(DefaultSystemBaseUri);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task BaseUriHeaderAndDefaultBaseUri_ShouldUseHeaderAndInvokeNext()
        {
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(SystemBaseUriHeader, new StringValues(new[] {systemBaseUriFromHeader}));
            context.Request.Headers.Add(SignatureHeader, new[] {GetBase64SignatureFor(systemBaseUriFromHeader, _signatureKey)});

            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { systemBaseUriSetByMiddleware = systemBaseUri; }
                    })
                .InvokeAsync(context);

            systemBaseUriSetByMiddleware.Should().Be(systemBaseUriFromHeader);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoBaseUriHeaderAndNullDefaultBaseUri_ShouldUseNullAndInvokeNext()
        {
            var context = new DefaultHttpContext();

            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultSystemBaseUri = null,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { systemBaseUriSetByMiddleware = systemBaseUri; }
                    })
                .InvokeAsync(context);

            systemBaseUriSetByMiddleware.Should().BeNull();
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task TenantIdHeaderAndNullDefaultTenantId_ShouldUseHeaderAndInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new StringValues(new[] {tenantIdFromHeader}));
            context.Request.Headers.Add(SignatureHeader, new[] {GetBase64SignatureFor(tenantIdFromHeader, _signatureKey)});


            var tenantIdSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = null,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { tenantIdSetByMiddleware = tenantId; }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(tenantIdFromHeader);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoTenantIdHeaderAndDefaultTenantId_ShouldUseDefaultTenantIdAndInvokeNext()
        {
            var context = new DefaultHttpContext();
            

            var tenantIdSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { tenantIdSetByMiddleware = tenantId; }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(DefaultTenantId);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task TenantIdHeaderAndDefaultTenantId_ShouldUseHeaderAndInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new StringValues(new[] {tenantIdFromHeader}));
            context.Request.Headers.Add(SignatureHeader, new[] {GetBase64SignatureFor(tenantIdFromHeader, _signatureKey)});


            var tenantIdSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { tenantIdSetByMiddleware = tenantId; }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(tenantIdFromHeader);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoTenantIdHeaderAndNullDefaultTenantId_ShouldUseNullAndInvokeNext()
        {
            var context = new DefaultHttpContext();
            
            var tenantIdSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = null,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { tenantIdSetByMiddleware = tenantId; }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().BeNull();
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task TenantIdHeaderAndBaseUriHeader_ShouldUseHeaderAndInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[]
                {
                    GetBase64SignatureFor(systemBaseUriFromHeader + tenantIdFromHeader, _signatureKey)
                });
                  

            var tenantIdSetByMiddleware = "null";
            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) =>
                        {
                            tenantIdSetByMiddleware = tenantId;
                            systemBaseUriSetByMiddleware = systemBaseUri;
                        }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(tenantIdFromHeader);
            systemBaseUriSetByMiddleware.Should().Be(systemBaseUriFromHeader);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task TenantIdHeaderAndNoBaseUriHeader_ShouldUseTenantIdHeaderAndDefaultSystemBaseUriAndInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                    new[] {GetBase64SignatureFor(tenantIdFromHeader, _signatureKey)});
                    

            var tenantIdSetByMiddleware = "null";
            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) =>
                        {
                            tenantIdSetByMiddleware = tenantId;
                            systemBaseUriSetByMiddleware = systemBaseUri;
                        }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(tenantIdFromHeader);
            systemBaseUriSetByMiddleware.Should().Be(DefaultSystemBaseUri);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task SystemBaseUriHeaderAndNoTenantIdHeader_ShouldUseSystemBaseUriHeaderAndDefaultTenantIdAndInvokeNext()
        {
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[] {GetBase64SignatureFor(systemBaseUriFromHeader, _signatureKey)});
                   
            var tenantIdSetByMiddleware = "null";
            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) =>
                        {
                            tenantIdSetByMiddleware = tenantId;
                            systemBaseUriSetByMiddleware = systemBaseUri;
                        }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(DefaultTenantId);
            systemBaseUriSetByMiddleware.Should().Be(systemBaseUriFromHeader);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoHeadersButDefaults_ShouldUseDefaultsAndInvokeNext()
        {
            var context = new DefaultHttpContext();

            var tenantIdSetByMiddleware = "null";
            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        OnTenantIdentified = (tenantId, systemBaseUri) =>
                        {
                            tenantIdSetByMiddleware = tenantId;
                            systemBaseUriSetByMiddleware = systemBaseUri;
                        }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(DefaultTenantId);
            systemBaseUriSetByMiddleware.Should().Be(DefaultSystemBaseUri);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoHeadersButDefaultsAndNoSignatureSecretKey_ShouldUseDefaultsAndInvokeNext()
        {
            var context = new DefaultHttpContext();

            var tenantIdSetByMiddleware = "null";
            var systemBaseUriSetByMiddleware = "null";
            var nextMiddleware = new MiddlewarMock(null);
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = null,
                        OnTenantIdentified = (tenantId, systemBaseUri) =>
                        {
                            tenantIdSetByMiddleware = tenantId;
                            systemBaseUriSetByMiddleware = systemBaseUri;
                        }
                    })
                .InvokeAsync(context);

            tenantIdSetByMiddleware.Should().Be(DefaultTenantId);
            systemBaseUriSetByMiddleware.Should().Be(DefaultSystemBaseUri);
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task BothHeadersAndWrongSignatureWithValidSignatureKey_ShouldReturn403AndNotInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[] {GetBase64SignatureFor("wrong data", _signatureKey)});
                   
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(403);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task SystemBaseUriHeaderAndWrongSignatureWithValidSignatureKey_ShouldReturn403AndNotInvokeNext()
        {
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[] {GetBase64SignatureFor("wrong data", _signatureKey)});
                    
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(403);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task TenantIdHeaderAndWrongSignatureWithValidSignatureKey_ShouldReturn403AndNotInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[] {GetBase64SignatureFor("wrong data", _signatureKey)});
                    
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(403);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }
        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task TenantIdHeaderAndWrongSignatureButIgnoredSignature_ShouldReturn200AndInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[] {GetBase64SignatureFor("wrong data", _signatureKey)});
        
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            var logIsWorking = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        LogCallback = (level, message) =>
                        {
                            if (level == TenantMiddlewareLogLevel.Error)
                            {
                                logIsWorking = true;
                            }
                        },
                        IgnoreSignature = true,
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(200);
            onTenantIdentifiedHasBeenInvoked.Should().BeTrue("onTenantIdentified should not have been invoked if wrong signature is ignored");
            nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked if wrong signature is ignored");
            logIsWorking.Should().BeTrue("log callback has not been invoked");
        }
        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task NoneBase64Signature_ShouldReturn403AndNotInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader, new[] {"abc+(9-!"});
                    

            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(403);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task WrongSignatureKey_ShouldReturn403AndNotInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var wrongSignatureKey = new byte[]
            {
                167, 219, 144, 209, 189, 1, 178, 73, 139, 47, 21, 236, 142, 56, 71, 245, 43, 188, 163, 52, 239, 102, 94, 153, 255, 159, 199, 149, 163,
                145, 161, 24
            };

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[]
                {
                    GetBase64SignatureFor(systemBaseUriFromHeader + tenantIdFromHeader,
                        wrongSignatureKey)
                });
                    
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(403);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task HeadersWithoutSignature_ShouldReturn403AndNotInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
        
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        SignatureSecretKey = _signatureKey,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(403);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }

        [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
        public async Task HeadersAndNoSignatureSecretKey_ShouldReturn500AndNotInvokeNext()
        {
            const string tenantIdFromHeader = "a12be5";
            const string systemBaseUriFromHeader = "https://sample.mydomain.de";

            var context = new DefaultHttpContext();
            context.Request.Headers.Add(TenantIdHeader, new[] {tenantIdFromHeader});
            context.Request.Headers.Add(SystemBaseUriHeader, new[] {systemBaseUriFromHeader});
            context.Request.Headers.Add(SignatureHeader,
                new[]
                {
                    GetBase64SignatureFor(systemBaseUriFromHeader + tenantIdFromHeader, _signatureKey)
                });
                    
            var nextMiddleware = new MiddlewarMock(null);
            var onTenantIdentifiedHasBeenInvoked = false;
            await new TenantMiddleware(nextMiddleware.InvokeAsync,
                    new TenantMiddlewareOptions
                    {
                        DefaultTenantId = DefaultTenantId,
                        DefaultSystemBaseUri = DefaultSystemBaseUri,
                        OnTenantIdentified = (tenantId, systemBaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                    })
                .InvokeAsync(context);

            context.Response.StatusCode.Should().Be(500);
            onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
            nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
        }

        private string GetBase64SignatureFor(string message, byte[] sigKey)
        {
            var encoding = new ASCIIEncoding();
            var messageBytes = encoding.GetBytes(message);
            using (var mac = new HMACSHA256(sigKey))
            {
                var hash = mac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        public class MiddlewarMock 
        {
            public bool HasBeenInvoked { get; private set; }

            public MiddlewarMock(RequestDelegate next) 
            {
            }

            public async Task InvokeAsync(HttpContext context)
            {
                HasBeenInvoked = true;
                await Task.FromResult(0);
            }
        }
    }
}
