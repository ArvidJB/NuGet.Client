using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.Credentials;
using NuGet.PackageManagement.VisualStudio;
using NuGet.VisualStudio;

namespace NuGetVSExtension
{
    /// <summary>
    /// Find all MEF imports for IVsCredentialProvider, and handle inserting fallback provider
    /// for Dev14
    /// </summary>
    public class VsCredentialProviderImporter
    {
        private readonly DTE _dte;
        private readonly Action<string> _errorDelegate;
        private readonly Func<ICredentialProvider> _fallbackProviderFactory;

        /// <summary>
        /// The VSTS Credential Provider Id will contain this string
        /// </summary>
        private const string VstsCredentialProviderIdSubstring =
            "TeamSystem.NuGetCredentialProvider.VisualStudioAccountProvider";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dte">DTE instance, used to determine the Visual Studio version.</param>
        /// <param name="fallbackProviderFactory">Factory method used to create a fallback provider for
        /// Dev14 in case a VSTS credential provider can not be imported.</param>
        /// <param name="errorDelegate">Used to write error messages to the user.</param>
        public VsCredentialProviderImporter(
            EnvDTE.DTE dte,
            Func<ICredentialProvider> fallbackProviderFactory,
            Action<string> errorDelegate)
        {
            _dte = dte;
            _fallbackProviderFactory = fallbackProviderFactory;
            _errorDelegate = errorDelegate;
        }

        [ImportMany(typeof(IVsCredentialProvider))]
        public List<Lazy<IVsCredentialProvider>> Providers { get; set; }

        /// <summary>
        /// Plugin providers are entered loaded the same way as other nuget extensions,
        /// matching any extension named CredentialProvider.*.exe.
        /// </summary>
        /// <returns>An enumeration of plugin providers</returns>
        public IEnumerable<ICredentialProvider> GetProviders()
        {
            Initialize();

            var importedProviders = Providers
                .Select(x => x.Value)
                .Select(x => (ICredentialProvider)new VsCredentialProviderAdapter(x))
                .OrderBy(x=>x.Id) // any deterministic order will do
                .ToList();

            // Dev15+ will provide a credential provider for VSTS.
            // If we are in Dev14, and no imported VSTS provider is found, 
            // then fallback on the built-in VisualStudioAccountProvider
            if (IsDev14 && !importedProviders.Any(x => x.Id.Contains(VstsCredentialProviderIdSubstring)))
            {
                // Handle any type load exception constructing the provider
                try
                {
                    var fallbackProvider = this._fallbackProviderFactory();
                    importedProviders.Add(fallbackProvider);
                }
                catch (Exception e) when (e is BadImageFormatException || e is FileLoadException)
                {
                    this._errorDelegate(Resources.VsCredentialProviderImporter_ErrorLoadingBuiltInCredentialProvider);
                }
            } else if (IsDev15 && !Providers.Any())
            {
                // Remove this before committing, this is just for debugging purposes
                _errorDelegate($"debug: found {Providers.Count} providers.");

            }

            return importedProviders;
        }

        private void Initialize()
        {
            var componentModel = ServiceLocator.GetGlobalService<SComponentModel, IComponentModel>();
            using (var container = new CompositionContainer(componentModel.DefaultExportProvider))
            {
                container.ComposeParts(this);
            }
        }

        private bool IsDev14 => _dte.Version.StartsWith("14.");

        private bool IsDev15 => _dte.Version.StartsWith("15.");
    }
}
