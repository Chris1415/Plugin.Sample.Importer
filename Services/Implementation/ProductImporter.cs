﻿using Plugin.Sample.Importer.Extensions;
using Plugin.Sample.Importer.Models.Parameter;
using Plugin.Sample.Importer.Services.Interface;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Implementation
{
    /// <summary>
    /// Product Importer
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public class ProductImporter : IProductImporter
    {
        /// <summary>
        /// createSellableItemCommand
        /// </summary>
        private readonly CreateSellableItemCommand _createSellableItemCommand;

        /// <summary>
        /// GetSellableItemCommand
        /// </summary>
        private readonly GetSellableItemCommand _getSellableItemCommand;

        /// <summary>
        /// GetSellableItemConnectCommand
        /// </summary>
        private readonly GetSellableItemConnectCommand _getSellableItemConnectCommand;

        /// <summary>
        /// EditSellableItemCommand
        /// </summary>
        private readonly EditSellableItemCommand _editSellableItemCommand;

        /// <summary>
        /// AssociateSellableItemToParentCommand
        /// </summary>
        private readonly AssociateSellableItemToParentCommand _associateSellableItemToParentCommand;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="createSellableItemCommand">createSellableItemCommand</param>
        /// <param name="associateSellableItemToParentCommand">associateSellableItemToParentCommand</param>
        /// <param name="getSellableItemCommand">getSellableItemCommand</param>
        /// <param name="editSellableItemCommand">editSellableItemCommand</param>
        /// <param name="getSellableItemConnectCommand">GetSellableItemConnectCommand</param>
        public ProductImporter(
            CreateSellableItemCommand createSellableItemCommand,
            AssociateSellableItemToParentCommand associateSellableItemToParentCommand,
            GetSellableItemCommand getSellableItemCommand,
            EditSellableItemCommand editSellableItemCommand,
            GetSellableItemConnectCommand getSellableItemConnectCommand)
        {
            _createSellableItemCommand = createSellableItemCommand;
            _associateSellableItemToParentCommand = associateSellableItemToParentCommand;
            _getSellableItemCommand = getSellableItemCommand;
            _editSellableItemCommand = editSellableItemCommand;
            _getSellableItemConnectCommand = getSellableItemConnectCommand;
        }

        /// <summary>
        /// Interface to Create or Update Catalog
        /// Currently Sellable Items cannot be dissasociatioed from parent
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        public async Task<CommerceCommand> ExecuteImport(CommerceContext context, CreateOrUpdateProductParameter parameter, bool updateExisting)
        {
            bool createdFlag = true;

            // TODO There is an error, that if the first command fails with some error any further command will not work properly 
            // Sitecore Support Ticket ID 515199
            SellableItem sellableItem = await this._createSellableItemCommand.Process(
               context,
               parameter.ProductId,
               parameter.Name,
               parameter.DisplayName,
               parameter.Description,
               parameter.Brand,
               parameter.Manufacturer,
               parameter.TypeOfGood,
               parameter.Tags.ToArray());

            if (sellableItem == null && !updateExisting)
            {
                return this._createSellableItemCommand;
            }

            if (sellableItem == null)
            {
                createdFlag = false;
                // Get SellableItem 
                sellableItem = await this._getSellableItemCommand.Process(context, $"{parameter.CatalogName}|{parameter.ProductId}|", false);
            }

            if (!createdFlag)
            {
                // Todo Try to edit custom Properties
                var composerTemplateViewsComponents = sellableItem.GetComponent<ComposerTemplateViewsComponent>().Views;
                if (!composerTemplateViewsComponents.Any())
                {

                }

                foreach (var composerTemplateViewsComponent in composerTemplateViewsComponents)
                {
                    var composerView = sellableItem.GetComposerView(composerTemplateViewsComponent.Key);

                    // Extract the needed tax value from custom view property
                    ViewProperty taxProperty = composerView?.Properties
                        .FirstOrDefault(element => element.Name.Equals("Taxes"));

                    taxProperty.Value = "0.19";
                }

                // Edit SellableItem only if it is not created within that call
                CatalogContentArgument catalogContentArgument = await this._editSellableItemCommand.Process(context, parameter.UpdateSellableItem(sellableItem));
            }

            // TODO Implement Dissasociation!!!

            // Build Association Item
            string entityIdentifier = parameter.ParentName.Equals(parameter.CatalogName)
                ? CommerceEntity.IdPrefix<Catalog>()
                : CommerceEntity.IdPrefix<Category>();
            string parentId = $"{entityIdentifier}{parameter.ParentName}";

            // Associate
            CatalogReferenceArgument catalogReferenceArgument = await this._associateSellableItemToParentCommand.Process(
                context,
                parameter.CatalogName.ToEntityId<Catalog>(),
                parentId,
                sellableItem.Id);

            return this._associateSellableItemToParentCommand;
        }
    }
}
