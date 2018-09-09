using Plugin.Sample.Importer.Models.Parameter;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Interface
{
    /// <summary>
    /// Category Importer
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public interface ICategoryImporter
    {
        /// <summary>
        /// Interface to Create or Update Catalog
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        Task<CommerceCommand> ExecuteImport(CommerceContext context, CreateOrUpdateCategoryParameter parameter, bool updateExisting);
    }
}
