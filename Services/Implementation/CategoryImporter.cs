using Plugin.Sample.Importer.Models.Parameter;
using Plugin.Sample.Importer.Services.Interface;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
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
        /// c'tor
        /// </summary>
        /// <param name="createCategoryCommand">createCategoryCommand</param>
        /// <param name="associateCategoryToParentCommand">associateCategoryToParentCommand</param>
        /// <param name="getCategoryCommand">Get Category Command</param>
        /// <param name="editCategoryCommand">Edit Category Command</param>
        /// <param name="deleteRelationshipCommand">Delete Relationship Command</param>
        public CategoryImporter(
            CreateCategoryCommand createCategoryCommand,
            AssociateCategoryToParentCommand associateCategoryToParentCommand,
            GetCategoryCommand getCategoryCommand,
            EditCategoryCommand editCategoryCommand,
            DeleteRelationshipCommand deleteRelationshipCommand)
        {
            _createCategoryCommand = createCategoryCommand;
            _associateCategoryToParentCommand = associateCategoryToParentCommand;
            _getCategoryCommand = getCategoryCommand;
            _editCategoryCommand = editCategoryCommand;
            _deleteRelationshipCommand = deleteRelationshipCommand;
        }

        /// <summary>
        /// Interface to Create or Update Catalog
        /// Currently no dissasociation of categories is implemented
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>8
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        public async Task<CommerceCommand> ExecuteImport(CommerceContext context, CreateOrUpdateCategoryParameter parameter, bool updateExisting)
        {
            bool createdFlag = true;
            string catalogId = parameter.CatalogName.ToEntityId<Catalog>();
            string categoryId = $"{CommerceEntity.IdPrefix<Category>()}{parameter.CatalogName}-{parameter.Name}";

            // Try to get existing Category
            Category category = await this._getCategoryCommand.Process(context, categoryId);

            // If existing - Check if it should be updated
            if (category != null && !updateExisting)
            {
                return this._getCategoryCommand;
            }

            // If existing - Get Current Category
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

            // TODO: Implement Dissasociation of categories when they are not existing anymore
            // RelationshiptTypes - "CatalogToCategory","CatalogToSellableItem","CategoryToCategory","CategoryToSellableItem"
            // SourceName = Parent
            // TargetName = Current Item

            // Todo Currently blocked by Sitecore Ticket ID 515690
            string targetName = category.Id;
            string sourceName = $"{parameter.CatalogName}-Pkw4".ToEntityId<Category>();
            RelationshipArgument relationshipArgument = await this._deleteRelationshipCommand.Process(context, sourceName, targetName, "CategoryToCategory");

            // TODO find a way to get the entities from these IDs
            IEnumerable<string> parentCatalogList = category.ParentCatalogList?.Split('|').AsEnumerable() ?? new List<string>();
            IEnumerable<string> parentCategoryList = category.ParentCategoryList?.Split('|').AsEnumerable() ?? new List<string>();

            // Associate category to parent
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
