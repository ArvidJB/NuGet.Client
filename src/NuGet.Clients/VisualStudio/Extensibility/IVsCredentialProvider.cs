using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.VisualStudio
{
    /// <summary>
    /// Contains methods to get credentials for NuGet operations.
    /// </summary>
    [ComImport]
    [Guid("970BF144-6885-4766-BD85-9DFDFD9DA3C6")]
    public interface IVsCredentialProvider
    {
        /// <summary>
        /// Get credentials for the supplied package source Uri.
        /// </summary>
        /// <param name="uri">The NuGet package source Uri for which credentials are being requested. Implementors are
        /// expected to first determine if this is a package source for which they can supply credentials.
        /// If not, then VsCredentialStatus.ProviderNotApplicable should be returned.</param>
        /// <param name="proxy">Web proxy to use when comunicating on the network.  Null if there is no proxy
        /// authentication configured.</param>
        /// <param name="isProxyRequest">True if if this request is to get proxy authentication
        /// credentials. If the implementation is not valid for acquiring proxy credentials, then
        /// VsCredentialStatus.ProviderNotApplicable should be returned.</param>
        /// <param name="isRetry">True if credentials were previously acquired for this uri, but
        /// the supplied credentials did not allow authorized access.</param>
        /// <param name="nonInteractive">If true, then interactive prompts must not be allowed.</param>
        /// <param name="cancellationToken">This cancellation token should be checked to determine if the
        /// operation requesting credentials has been cancelled.</param>
        /// <param name="credentials">The returned credentials.</param>
        /// <returns>An integer status code, defined in VsCredentialStatus.</returns>
        /// <remarks>If the credential provider does handle credentials for the given uri, but is unable
        /// to supply them, then an exception should be thrown.</remarks>
        Task<IVsCredentialResponse> Get(Uri uri,
            IWebProxy proxy,
            bool isProxyRequest,
            bool isRetry,
            bool nonInteractive,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Contains the response to a credential request.
    /// </summary>
    [ComImport]
    [Guid("EBA024E7-124C-43EA-AAD4-BB065DE07A63")]
    public interface IVsCredentialResponse
    {
        /// <summary>
        /// Response credentials
        /// </summary>
        ICredentials Credentials { get; }

        /// <summary>
        /// Response status code
        /// </summary>
        VsCredentialStatus Status { get; }
    }

    /// <summary>
    /// Return values for IVsCredentialProvider.Get
    /// </summary>
    public enum VsCredentialStatus
    {
        /// <summary>
        /// Credentials have been successfully acquired.
        /// </summary>
        Success,

        /// <summary>
        /// The current provider does not handle credentials for the given Uri.
        /// </summary>
        ProviderNotApplicable
    }
}
