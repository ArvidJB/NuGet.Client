using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Credentials;
using NuGet.VisualStudio;

namespace NuGetVSExtension
{
    /// <summary>
    /// Wraps an IVsCredentialProvider.  IVsCredentialProvider ensures that VS Extensions 
    /// can supply credential providers implementing a stable interface across versions.
    /// </summary>
    public class VsCredentialProviderAdapter : ICredentialProvider
    {
        private readonly IVsCredentialProvider _provider;

        public VsCredentialProviderAdapter(IVsCredentialProvider provider)
        {
            _provider = provider;
        }

        public string Id => _provider.GetType().FullName;

        public async Task<CredentialResponse> Get(
            Uri uri, 
            IWebProxy proxy, 
            bool isProxyRequest, 
            bool isRetry, 
            bool nonInteractive, 
            CancellationToken cancellationToken)
        {
            var result = await _provider.Get(uri, proxy, isProxyRequest, isRetry, nonInteractive, cancellationToken);
            return new CredentialResponse(result.Credentials, ToCredentialStatus((int)result.Status));
        }

        private static CredentialStatus ToCredentialStatus(int result)
        {
            if (result < (int)CredentialStatus.Success || result > (int)CredentialStatus.ProviderNotApplicable)
            {
                throw new ProviderException(Resources.ProviderException_MalformedResponse);
            }

            return (CredentialStatus) result;
        }
    }
}
