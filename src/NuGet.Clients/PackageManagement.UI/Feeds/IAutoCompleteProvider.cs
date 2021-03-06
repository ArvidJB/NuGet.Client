﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI
{
    internal interface IAutoCompleteProvider
    {
        Task<IEnumerable<string>> IdStartsWithAsync(string packageIdPrefix, bool includePrerelease, CancellationToken cancellationToken);
        Task<IEnumerable<NuGetVersion>> VersionStartsWithAsync(string packageId, string versionPrefix, bool includePrerelease, CancellationToken cancellationToken);
    }
}
