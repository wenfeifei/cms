using System.Collections.Specialized;
using System.Threading.Tasks;
using SS.CMS.Models;

namespace SS.CMS.Services
{
    public partial interface IUrlManager
    {
        Task<string> GetSiteUrlAsync(int siteId);

        Task<string> GetSiteUrlAsync(int siteId, string virtualPath);

        Task<string> GetSiteUrlByFilePathAsync(string filePath);

        void PutImagePaths(Site siteInfo, Content contentInfo, NameValueCollection collection);
    }
}