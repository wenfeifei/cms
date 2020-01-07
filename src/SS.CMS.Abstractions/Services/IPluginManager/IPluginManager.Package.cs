using System.Threading.Tasks;
using SS.CMS.Enums;

namespace SS.CMS.Services
{
    public partial interface IPluginManager
    {
        IPackageMetadata GetPackageMetadataFromPluginDirectory(string directoryName, out string errorMessage);

        IPackageMetadata GetPackageMetadataFromPackages(string directoryName, out string nuspecPath, out string dllDirectoryPath, out string errorMessage);

        void DownloadPackage(string packageId, string version);

        Task<(bool IsSuccess, string ErrorMessage)> UpdatePackageAsync(string idWithVersion, PackageType packageType);
    }
}