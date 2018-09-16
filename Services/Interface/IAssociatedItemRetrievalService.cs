using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Interface
{
    /// <summary>
    /// Service to Get associated Items
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public interface IAssociatedItemRetrievalService
    {
        /// <summary>
        /// Gets all Parent Entities of given entity
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="entityName">entity name</param>
        /// <param name="catalogName">Catalog Name</param>
        /// <returns>List of all parent entities</returns>
        Task<List<string>> GetAllParentEnitites(CommerceContext context, string entityName, string catalogName);
    }
}
