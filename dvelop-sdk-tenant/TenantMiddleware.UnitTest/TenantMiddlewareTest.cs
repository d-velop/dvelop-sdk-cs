using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dvelop.Sdk.TenantMiddleware.UnitTest
{
    [TestClass]
    public class TenantMiddlewareTest
    {
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

        private const string SYSTEM_BASE_URI_HEADER = "x-dv-baseuri";
        private const string TENANT_ID_HEADER = "x-dv-tenant-id";
        private const string SIGNATURE_HEADER = "x-dv-sig-1";
        private const string DEFAULT_TENANT_ID = "0";
        private const string DEFAULT_SYSTEM_BASE_URI = "https://default.mydomain.de";

        private readonly byte[] _signatureKey =
        {
            166, 219, 144, 209, 189, 1, 178, 73, 139, 47, 21, 236, 142, 56, 71, 245, 43, 188, 163, 52, 239, 102, 94,
            153, 255, 159, 199, 149, 163, 145, 161, 24
        };
        /*
                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task BaseUriHeaderAndNullDefaultBaseUri_ShouldUseHeaderAndInvokeNext()
                {
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";
                    var owinContext = new HttpContextStub(new HttpRequest("","","")
                    {
                        Headers =
                        {
                            new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                            new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(SYSTEM_BASE_URI_FROM_HEADER, _signatureKey)})
                        }
                    });


                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultSystemBaseUri = null,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { systemBaseUriSetByMiddleware = systembaseUri; }
                            })
                        .InvokeAsync(owinContext);

                    systemBaseUriSetByMiddleware.Should().Be(SYSTEM_BASE_URI_FROM_HEADER);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoBaseUriHeaderAndDefaultBaseUri_ShouldUseDefaultBaseUriAndInvokeNext()
                {
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest()
                    };

                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                OnTenantIdentified = (tenantId, systembaseUri) => { systemBaseUriSetByMiddleware = systembaseUri; }
                            })
                        .InvokeAsync(owinContext);

                    systemBaseUriSetByMiddleware.Should().Be(DEFAULT_SYSTEM_BASE_URI);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task BaseUriHeaderAndDefaultBaseUri_ShouldUseHeaderAndInvokeNext()
                {
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(SYSTEM_BASE_URI_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { systemBaseUriSetByMiddleware = systembaseUri; }
                            })
                        .InvokeAsync(owinContext);

                    systemBaseUriSetByMiddleware.Should().Be(SYSTEM_BASE_URI_FROM_HEADER);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoBaseUriHeaderAndNullDefaultBaseUri_ShouldUseNullAndInvokeNext()
                {
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest()
                    };

                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultSystemBaseUri = null,
                                OnTenantIdentified = (tenantId, systembaseUri) => { systemBaseUriSetByMiddleware = systembaseUri; }
                            })
                        .InvokeAsync(owinContext);

                    systemBaseUriSetByMiddleware.Should().BeNull();
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task TenantIdHeaderAndNullDefaultTenantId_ShouldUseHeaderAndInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(TENANT_ID_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var tenantIdSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = null,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { tenantIdSetByMiddleware = tenantId; }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(TENANT_ID_FROM_HEADER);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoTenantIdHeaderAndDefaultTenantId_ShouldUseDefaultTenantIdAndInvokeNext()
                {
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest()
                    };

                    var tenantIdSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                OnTenantIdentified = (tenantId, systembaseUri) => { tenantIdSetByMiddleware = tenantId; }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(DEFAULT_TENANT_ID);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task TenantIdHeaderAndDefaultTenantId_ShouldUseHeaderAndInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(TENANT_ID_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var tenantIdSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { tenantIdSetByMiddleware = tenantId; }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(TENANT_ID_FROM_HEADER);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoTenantIdHeaderAndNullDefaultTenantId_ShouldUseNullAndInvokeNext()
                {
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest()
                    };

                    var tenantIdSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = null,
                                OnTenantIdentified = (tenantId, systembaseUri) => { tenantIdSetByMiddleware = tenantId; }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().BeNull();
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task TenantIdHeaderAndBaseUriHeader_ShouldUseHeaderAndInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(SYSTEM_BASE_URI_FROM_HEADER+TENANT_ID_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var tenantIdSetByMiddleware = "null";
                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) =>
                                {
                                    tenantIdSetByMiddleware = tenantId;
                                    systemBaseUriSetByMiddleware = systembaseUri;
                                }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(TENANT_ID_FROM_HEADER);
                    systemBaseUriSetByMiddleware.Should().Be(SYSTEM_BASE_URI_FROM_HEADER);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task TenantIdHeaderAndNoBaseUriHeader_ShouldUseTenantIdHeaderAndDefaultSystemBaseUriAndInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(TENANT_ID_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var tenantIdSetByMiddleware = "null";
                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) =>
                                {
                                    tenantIdSetByMiddleware = tenantId;
                                    systemBaseUriSetByMiddleware = systembaseUri;
                                }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(TENANT_ID_FROM_HEADER);
                    systemBaseUriSetByMiddleware.Should().Be(DEFAULT_SYSTEM_BASE_URI);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task SystemBaseUriHeaderAndNoTenantIdHeader_ShouldUseSystemBaseUriHeaderAndDefaultTenantIdAndInvokeNext()
                {
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(SYSTEM_BASE_URI_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var tenantIdSetByMiddleware = "null";
                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) =>
                                {
                                    tenantIdSetByMiddleware = tenantId;
                                    systemBaseUriSetByMiddleware = systembaseUri;
                                }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(DEFAULT_TENANT_ID);
                    systemBaseUriSetByMiddleware.Should().Be(SYSTEM_BASE_URI_FROM_HEADER);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoHeadersButDefaults_ShouldUseDefaultsAndInvokeNext()
                {
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest()
                    };

                    var tenantIdSetByMiddleware = "null";
                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                OnTenantIdentified = (tenantId, systembaseUri) =>
                                {
                                    tenantIdSetByMiddleware = tenantId;
                                    systemBaseUriSetByMiddleware = systembaseUri;
                                }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(DEFAULT_TENANT_ID);
                    systemBaseUriSetByMiddleware.Should().Be(DEFAULT_SYSTEM_BASE_URI);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoHeadersButDefaultsAndNoSignatureSecretKey_ShouldUseDefaultsAndInvokeNext()
                {
                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest()
                    };

                    var tenantIdSetByMiddleware = "null";
                    var systemBaseUriSetByMiddleware = "null";
                    var nextMiddleware = new MiddlewarMock(null);
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = null,
                                OnTenantIdentified = (tenantId, systembaseUri) =>
                                {
                                    tenantIdSetByMiddleware = tenantId;
                                    systemBaseUriSetByMiddleware = systembaseUri;
                                }
                            })
                        .InvokeAsync(owinContext);

                    tenantIdSetByMiddleware.Should().Be(DEFAULT_TENANT_ID);
                    systemBaseUriSetByMiddleware.Should().Be(DEFAULT_SYSTEM_BASE_URI);
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task BothHeadersAndWrongSignatureWithValidSignatureKey_ShouldReturn403AndNotInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor("wrong data", _signatureKey)})
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(403);
                    onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
                    nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task SystemBaseUriHeaderAndWrongSignatureWithValidSignatureKey_ShouldReturn403AndNotInvokeNext()
                {
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor("wrong data", _signatureKey)})
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(403);
                    onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
                    nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task TenantIdHeaderAndWrongSignatureWithValidSignatureKey_ShouldReturn403AndNotInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor("wrong data", _signatureKey)})
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(403);
                    onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
                    nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
                }
                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task TenantIdHeaderAndWrongSignatureButIgnoredSignature_ShouldReturn200AndInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor("wrong data", _signatureKey)})
                            }
                        }
                    };

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
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(200);
                    onTenantIdentifiedHasBeenInvoked.Should().BeTrue("onTenantIdentified should not have been invoked if wrong signature is ignored");
                    nextMiddleware.HasBeenInvoked.Should().BeTrue("next middleware should have been invoked if wrong signature is ignored");
                    logIsWorking.Should().BeTrue("log callback has not been invoked");
                }
                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task NoneBase64Signature_ShouldReturn403AndNotInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {"abc+(9-!"})
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(403);
                    onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
                    nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task WrongSignatureKey_ShouldReturn403AndNotInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var wrongSignatureKey = new byte[]
                    {
                        167, 219, 144, 209, 189, 1, 178, 73, 139, 47, 21, 236, 142, 56, 71, 245, 43, 188, 163, 52, 239, 102, 94, 153, 255, 159, 199, 149, 163,
                        145, 161, 24
                    };

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(SYSTEM_BASE_URI_FROM_HEADER+TENANT_ID_FROM_HEADER, wrongSignatureKey)})
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(403);
                    onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
                    nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task HeadersWithoutSignature_ShouldReturn403AndNotInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                SignatureSecretKey = _signatureKey,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(403);
                    onTenantIdentifiedHasBeenInvoked.Should().BeFalse("onTenantIdentified should not have been invoked if signature is wrong");
                    nextMiddleware.HasBeenInvoked.Should().BeFalse("next middleware should not have been invoked if signature is wrong");
                }

                [TestMethod, UnitUnderTest(typeof(TenantMiddleware))]
                public async Task HeadersAndNoSignatureSecretKey_ShouldReturn500AndNotInvokeNext()
                {
                    const string TENANT_ID_FROM_HEADER = "a12be5";
                    const string SYSTEM_BASE_URI_FROM_HEADER = "https://sample.mydomain.de";

                    var owinContext = new HttpContextStub
                    {
                        Request = new OwinRequest
                        {
                            Headers =
                            {
                                new KeyValuePair<string, string[]>(TENANT_ID_HEADER, new[] {TENANT_ID_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SYSTEM_BASE_URI_HEADER, new[] {SYSTEM_BASE_URI_FROM_HEADER}),
                                new KeyValuePair<string, string[]>(SIGNATURE_HEADER, new[] {GetBase64SignatureFor(SYSTEM_BASE_URI_FROM_HEADER+TENANT_ID_FROM_HEADER, _signatureKey)})
                            }
                        }
                    };

                    var nextMiddleware = new MiddlewarMock(null);
                    var onTenantIdentifiedHasBeenInvoked = false;
                    await new TenantMiddleware(nextMiddleware.InvokeAsync,
                            new TenantMiddlewareOptions
                            {
                                DefaultTenantId = DEFAULT_TENANT_ID,
                                DefaultSystemBaseUri = DEFAULT_SYSTEM_BASE_URI,
                                OnTenantIdentified = (tenantId, systembaseUri) => { onTenantIdentifiedHasBeenInvoked = true; }
                            })
                        .InvokeAsync(owinContext);

                    owinContext.Response.StatusCode.Should().Be(500);
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
                */
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

                internal class HttpContextStub : HttpContext
                {
                    private HttpRequest _httpRequest;
                    public HttpContextStub(HttpRequest request=null)
                    {
                        _httpRequest = request;
                    }
                    public T Get<T>(string key)
                    {
                        throw new NotImplementedException();
                    }

                    public HttpContext Set<T>(string key, T value)
                    {
                        throw new NotImplementedException();
                    }

                    public override void Abort()
                    {
                        throw new NotImplementedException();
                    }

                    public override IFeatureCollection Features { get; }
                    public override HttpRequest Request
                    {
                        get { return _httpRequest; }
                    }
                    public override HttpResponse Response { get; }

                    public override ConnectionInfo Connection { get; }
                    public override WebSocketManager WebSockets { get; }
#pragma warning disable 618
                    public override AuthenticationManager Authentication { get; }
#pragma warning restore 618


                    public override ClaimsPrincipal User { get; set; }
                    public override IDictionary<object, object> Items { get; set; }
                    public override IServiceProvider RequestServices { get; set; }
                    public override CancellationToken RequestAborted { get; set; }
                    public override string TraceIdentifier { get; set; }
                    public override ISession Session { get; set; }

                    public IDictionary<string, object> Environment
                    {
                        get { throw new NotImplementedException(); }
                    }

                    public TextWriter TraceOutput
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
    }

}
