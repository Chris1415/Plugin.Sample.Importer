using Plugin.Sample.Importer.Models.Parameter;
using Plugin.Sample.Importer.Services.Interface;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.ManagedLists;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Implementation
{
    /// <summary>
    /// Category Importer
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public class CategoryImporter : ICategoryImporter
    {
        /// <summary>
        /// Create Category Command
        /// </summary>
        private readonly CreateCategoryCommand _createCategoryCommand;

        /// <summary>
        /// Get Category Command
        /// </summary>
        private readonly GetCategoryCommand _getCategoryCommand;

        /// <summary>
        /// Edit Category Command
        /// </summary>
        private readonly EditCategoryCommand _editCategoryCommand;

        /// <summary>
        /// associateCategoryToParent Command
        /// </summary>
        private readonly AssociateCategoryToParentCommand _associateCategoryToParentCommand;

        /// <summary>
        /// Delete Relationship Command
        /// </summary>
        private readonly DeleteRelationshipCommand _deleteRelationshipCommand;

        /// <summary>
        /// Find Entitities in List Command
        /// </summary>
        private readonly FindEntitiesInListCommand _findEntitiesInListCommand;

        /// <summary>
        /// Associated Item Retrieval Service
        /// </summary>
        private readonly IAssociatedItemRetrievalService _associatedItemRetrievalService;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="createCategoryCommand">createCategoryCommand</param>
        /// <param name="associateCategoryToParentCommand">associateCategoryToParentCommand</param>
        /// <param name="getCategoryCommand">Get Category Command</param>
        /// <param name="editCategoryCommand">Edit Category Command</param>
        /// <param name="deleteRelationshipCommand">Delete Relationship Command</param>
        /// <param name="findEntitiesInListCommand">Find entities in List Command</param>
        /// <param name="associatedItemRetrievalService">associatedItemRetrievalService</param>
        public CategoryImporter(
            CreateCategoryCommand createCategoryCommand,
            AssociateCategoryToParentCommand associateCategoryToParentCommand,
            GetCategoryCommand getCategoryCommand,
            EditCategoryCommand editCategoryCommand,
            DeleteRelationshipCommand deleteRelationshipCommand,
            FindEntitiesInListCommand findEntitiesInListCommand,
            IAssociatedItemRetrievalService associatedItemRetrievalService)
        {
            _createCategoryCommand = createCategoryCommand;
            _associateCategoryToParentCommand = associateCategoryToParentCommand;
            _getCategoryCommand = getCategoryCommand;
            _editCategoryCommand = editCategoryCommand;
            _deleteRelationshipCommand = deleteRelationshipCommand;
            _findEntitiesInListCommand = findEntitiesInListCommand;
            _associatedItemRetrievalService = associatedItemRetrievalService;
        }

        /// <summary>
        /// Interface to Create or Update Catalog
        /// Currently no dissasociation of categories is implemented
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        public async Task<CommerceCommand> ExecuteImport(CommerceContext context, CreateOrUpdateCategoryParameter parameter, bool updateExisting)
        {
            // Try to get existing Category
            string categoryId = $"{CommerceEntity.IdPrefix<Category>()}{parameter.CatalogName}-{parameter.Name}";
            Category category = await this._getCategoryCommand.Process(context, categoryId);

            // If existing - Check if it should be updated
            if (category != null && !updateExisting)
            {
                return this._getCategoryCommand;
            }

            // If existing - Get Current Category
            bool createdFlag = true;
            if (category == null)
            {
                createdFlag = false;
                category = await this._createCategoryCommand.Process(context, parameter.CatalogName, parameter.Name, parameter.DisplayName, parameter.Description);
            }

            // Only update category if it was not created previously
            if (!createdFlag)
            {
                CatalogContentArgument editCategoryResult = await this._editCategoryCommand.Process(context, category, parameter.DisplayName, parameter.Description);
            }

            // Workaround to get all Parent Entities for the current Entity
            List<string> parentEntities = await this._associatedItemRetrievalService.GetAllParentEnitites(context, category.Name, parameter.CatalogName);

            // Check with the given paremter.ParentNames and the existing Parents, which ones have to be deleted
            foreach (string parentEntity in parentEntities)
            {
                // If both lists containt the entity, it should not be deleted
                if (parameter.ParentNames.Contains(parentEntity))
                {
                    continue;
                }
                // Otherwise disassociate the parent
                // TargetName = Current Item
                string targetName;
                // SourceName = Parent
                string sourceName;
                // RelationshiptTypes - "CatalogToCategory","CatalogToSellableItem","CategoryToCategory","CategoryToSellableItem"
                string relationshipType;

                if (parentEntity.Equals(parameter.CatalogName))
                {
                    // Disassociate Catalog To Category
                    targetName = category.Id;
                    sourceName = $"{parentEntity}".ToEntityId<Catalog>();
                    relationshipType = "CatalogToCategory";
                }
                else
                {
                    // Disassociate Category To Category
                    targetName = category.Id;
                    sourceName = $"{parameter.CatalogName}-{parentEntity}".ToEntityId<Category>();
                    relationshipType = "CategoryToCategory";
                }

                RelationshipArgument relationshipArgument = await this._deleteRelationshipCommand.Process(context, sourceName, targetName, relationshipType);
            }

            // Associate category to parent
            string catalogId = parameter.CatalogName.ToEntityId<Catalog>();
            foreach (string parentName in parameter.ParentNames)
            {
                string entityIdentifier = parentName.Equals(parameter.CatalogName)
                  ? CommerceEntity.IdPrefix<Catalog>()
                  : $"{CommerceEntity.IdPrefix<Category>()}{parameter.CatalogName}-";
                string parentId = $"{entityIdentifier}{parentName}";
                CatalogContentArgument associateCategoryToParentResult = await this._associateCategoryToParentCommand.Process(context, catalogId, parentId, category.Id);
            }

            return this._associateCategoryToParentCommand;
        }
    }
}
