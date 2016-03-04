using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.IdentityModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.VisualStudio;
using NuGetVSExtension;

namespace NuGet.VsExtension.Test
{
    namespace TeamSystem.NuGetCredentialProvider
    {
        public class VisualStudioAccountProvider : IVsCredentialProvider
        {
            public Task<IVsCredentialResponse> Get(Uri uri, IWebProxy proxy, bool isProxyRequest, bool isRetry, bool nonInteractive,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }

    [TestClass]
    public class VsCredentialProviderImporterTests
    {
        private readonly Mock<DTE> _mockDte = new Mock<DTE>();
        private readonly Func<Credentials.ICredentialProvider> _fallbackProviderFactory = () => new VisualStudioAccountProvider(null, null);
        private readonly List<string> _errorMessages = new List<string>();
        private readonly Action<string> _errorDelegate;

        public VsCredentialProviderImporterTests()
        {
            _errorDelegate = s => _errorMessages.Add(s);
        }

        private VsCredentialProviderImporter GetTestableImporter(List<IVsCredentialProvider> testImports = null)
        {
            return new VsCredentialProviderImporter(_mockDte.Object, _fallbackProviderFactory, _errorDelegate);
        }

        [TestMethod]
        public void WhenVstsImportNotFound_WhenDev14_ThenInsertBuiltInProvider()
        {
            _mockDte.Setup(x => x.Version).Returns("14.0.247200.00");
            var importer = GetTestableImporter();

            var result = importer.GetProviders().ToList();

            Assert.AreEqual(1, result.Count());
            Assert.IsInstanceOfType(result[0], typeof(VisualStudioAccountProvider));
        }

        [TestMethod]
        public void WhenVstsImportNotFound_WhenNotDev14_ThenDoNotInsertBuiltInProvider()
        {
            _mockDte.Setup(x => x.Version).Returns("15.0.123456.00");
            var importer = GetTestableImporter();

            var result = importer.GetProviders().ToList();

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void WhenVstsImportFound_ThenDoNotInsertBuiltInProvider()
        {
            _mockDte.Setup(x => x.Version).Returns("14.0.247200.00");
            var importer = GetTestableImporter();
            var testableProvider = new Lazy<IVsCredentialProvider>(() => new TeamSystem.NuGetCredentialProvider.VisualStudioAccountProvider() );
            importer.Providers = new List<Lazy<IVsCredentialProvider>>() {testableProvider};

            var result = importer.GetProviders().ToList();

            Assert.AreEqual(1, result.Count());
            Assert.IsInstanceOfType(result[0], typeof(VsCredentialProviderAdapter));
        }

    }
}
