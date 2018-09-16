using Plugin.Sample.Importer.Services.Interface;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Implementation
{
    /// <summary>
    /// Service to Get associated Items
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    class AssociatedItemRetrievalService : IAssociatedItemRetrievalService
    {
        /// <summary>
        /// Catagory To Category String
        /// </summary>
        private const string CategoryToCategory = "categorytocategory";

        /// <summary>
        /// Catalog To Category String
        /// </summary>
        private const string CatalogToCategory = "catalogtocategory";

        /// <summary>
        ///  Category Type as string
        /// </summary>
        private const string CategoryType = "Sitecore.Commerce.Plugin.Catalog.Category, Sitecore.Commerce.Plugin.Catalog";

        /// <summary>
        /// Find Entitities in List Command
        /// </summary>
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        public AssociatedItemRetrievalService(FindEntitiesInListCommand findEntitiesInListCommand)
        {
            _findEntitiesInListCommand = findEntitiesInListCommand;
        }

        /// <summary>
        /// Gets all Parent Entities of given entity
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="entityName">entity name</param>
        /// <param name="catalogName">Catalog Name</param>
        /// <returns>List of all parent entities</returns>
        public async Task<List<string>> GetAllParentEnitites(CommerceContext context, string entityName, string catalogName)
        {
            List<string> resultList = new List<string>();
            CommerceList<CommerceEntity> entityList = await this._findEntitiesInListCommand.Process(context, CategoryType, $"{CatalogToCategory}-{catalogName}", 0, 100);
            foreach (CommerceEntity childEntity in entityList.Items)
            {
                if (childEntity.Name.Equals(entityName))
                {
                    resultList.Add(catalogName);
                }

                List<string> children = await this.GetAllChildrenEntities(context, childEntity.Name, entityName, catalogName);

                resultList.AddRange(children);
            }

            return resultList;
        }

        /// <summary>
        /// Recursion Helper to run through all children of the current Entity
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="currentEntityName">current Entity</param>
        /// <param name="targetEntityName">target entity to find</param>
        /// <returns>list of parents which have the target entity as child</returns>
        private async Task<List<string>> GetAllChildrenEntities(CommerceContext context, string currentEntityName, string targetEntityName, string catalogName)
        {
            List<string> resultList = new List<string>();
            CommerceList<CommerceEntity> categoryToCategoryList = await this._findEntitiesInListCommand.Process(context, CategoryType, $"{CategoryToCategory}-{catalogName}-{currentEntityName}", 0, 100);
            foreach (CommerceEntity childEntity in categoryToCategoryList.Items)
            {
                if (childEntity.Name.Equals(targetEntityName))
                {
                    resultList.Add(currentEntityName);
                }

                List<string> children = await this.GetAllChildrenEntities(context, childEntity.Name, targetEntityName, catalogName);

                resultList.AddRange(children);
            }

            return resultList;
        }
    }
}
