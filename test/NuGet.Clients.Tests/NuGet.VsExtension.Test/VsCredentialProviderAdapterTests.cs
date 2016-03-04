using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Credentials;
using NuGet.VisualStudio;
using NuGetVSExtension;

namespace NuGet.VsExtension.Test
{
    [TestClass]
    public class VsCredentialProviderAdapterTests
    {
        private class TestVsCredentialResponse : IVsCredentialResponse
        {
            public ICredentials Credentials { get; set; }
            public VsCredentialStatus Status { get; set; }
        }

        private class TestVsCredentialProvider : IVsCredentialProvider
        {
            private readonly TestVsCredentialResponse _testResponse;

            public TestVsCredentialProvider(TestVsCredentialResponse testResponse)
            {
                _testResponse = testResponse;
            }

            public Task<IVsCredentialResponse> Get(Uri uri, IWebProxy proxy, bool isProxyRequest, bool isRetry, bool nonInteractive,
                CancellationToken cancellationToken)
            {
                return Task.FromResult<IVsCredentialResponse>(_testResponse);
            }
        }

        [TestMethod]
        public async Task WhenStatusBelowRange_ThenException()
        {
            var result = new TestVsCredentialResponse() {Status = (VsCredentialStatus)(-1)};
            var provider = new TestVsCredentialProvider(result);
            var adapter = new VsCredentialProviderAdapter(provider);

            var ex = await AssertExtensions.RecordExceptionAsync<ProviderException>(
                async ()=>await adapter.Get(new Uri("http://host"), null, false, false, false, CancellationToken.None));

            StringAssert.Contains(ex.Message, "invalid response");
        }

        [TestMethod]
        public async Task WhenStatusAboveRange_ThenException()
        {
            var result = new TestVsCredentialResponse() { Status = (VsCredentialStatus)(2) };
            var provider = new TestVsCredentialProvider(result);
            var adapter = new VsCredentialProviderAdapter(provider);

            var ex = await AssertExtensions.RecordExceptionAsync<ProviderException>(
                async () => await adapter.Get(new Uri("http://host"), null, false, false, false, CancellationToken.None));

            StringAssert.Contains(ex.Message, "invalid response");
        }

        [TestMethod]
        public async Task WhenAnyValidVsCredentialResponse_Ok()
        {
            var expected = new TestVsCredentialResponse() { Status = (VsCredentialStatus)(0) };
            var provider = new TestVsCredentialProvider(expected);
            var adapter = new VsCredentialProviderAdapter(provider);

            var result = await adapter.Get(new Uri("http://host"), null, false, false, false, CancellationToken.None);

            Assert.AreSame(expected, result);
        }
    }
}
