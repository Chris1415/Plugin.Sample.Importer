using Plugin.Sample.Importer.Models.Parameter;
using Plugin.Sample.Importer.Services.Interface;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using System;       
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Services.Implementation
{
    /// <summary>
    /// Variant Importer
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public class VariantImporter : IVariantImporter
    {
        #region Properties

        /// <summary>
        /// CreateSellableItemVariationCommand
        /// </summary>
        private readonly CreateSellableItemVariationCommand _createSellableItemVariationCommand;

        /// <summary>
        /// EditSellableItemCommand
        /// </summary>
        private readonly EditSellableItemCommand _editSellableItemCommand;

        /// <summary>
        /// GetSellableItemCommand
        /// </summary>
        private readonly GetSellableItemCommand _getSellableItemCommand;

        #endregion

        #region 'ctor

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="createSellableItemVariationCommand">createSellableItemVariationCommand</param>
        /// <param name="getSellableItemCommand">getSellableItemCommand</param>
        /// <param name="editSellableItemCommand">editSellableItemCommand</param>
        public VariantImporter(
            CreateSellableItemVariationCommand createSellableItemVariationCommand,
            GetSellableItemCommand getSellableItemCommand,
            EditSellableItemCommand editSellableItemCommand)
        {
            _createSellableItemVariationCommand = createSellableItemVariationCommand;
            _getSellableItemCommand = getSellableItemCommand;
            _editSellableItemCommand = editSellableItemCommand;
        }

        #endregion

        #region Interface

        /// <summary>
        /// Interface to Create or Update Catalog
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        public async Task<CommerceCommand> ExecuteImport(CommerceContext context, CreateOrUpdateVariantParameter parameter, bool updateExisting)
        {    
            SellableItem sellableItem = await this._createSellableItemVariationCommand.Process(context, parameter.ProductName.ToEntityId<SellableItem>(), parameter.VariantName, parameter.Name, parameter.DisplayName);

            if(sellableItem == null && !updateExisting)
            {
                return this._createSellableItemVariationCommand;
            }

            if(sellableItem == null)
            {
                // TODO Not implemented - cause EditSellableITemCommand is not working correctly right now
                sellableItem = await this._getSellableItemCommand.Process(context, $"{parameter.CatalogName}|{parameter.ProductName}|", false);
                var itemVariationComponent = sellableItem.GetVariation(parameter.VariantName);
                itemVariationComponent.ListPrice = new Money(100);
                CatalogContentArgument catalogContentArgument = await this._editSellableItemCommand.Process(context, sellableItem);
            }

            return this._getSellableItemCommand;
        }

        #endregion
    }
}
