namespace NuGet.Credentials
{
    /// <summary>
    /// Result of an attempt to acquire credentials.
    /// Keep in sync with NuGet.VisualStudio.VsCredentialStatus
    /// </summary>
    public enum CredentialStatus
    {
        Success,
        ProviderNotApplicable
    }
}