using Plugin.Sample.Importer.Models.Parameter;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Interface
{
    /// <summary>
    /// Variant Importer
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public interface IVariantImporter
    {
        /// <summary>
        /// Interface to Create or Update Catalog
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        Task<CommerceCommand> ExecuteImport(CommerceContext context, CreateOrUpdateVariantParameter parameter, bool updateExisting);
    }
}
