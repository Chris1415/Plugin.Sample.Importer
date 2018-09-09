using Plugin.Sample.Importer.Models.Parameter;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Commerce.Plugin.Inventory;
using Plugin.Sample.Importer.Services.Interface;

namespace Plugin.Sample.Importer.Services.Implementation
{
    /// <summary>
    /// Catalog Importer
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public class CatalogImporter : ICatalogImporter
    {
        /// <summary>
        /// Get Catalog Command
        /// </summary>
        private readonly GetCatalogCommand _getCatalogCommand;

        /// <summary>
        /// Create Catalog Command
        /// </summary>
        private readonly CreateCatalogCommand _createCatalogCommand;

        /// <summary>
        /// EditCatalogCommand
        /// </summary>
        private readonly EditCatalogCommand _editCatalogCommand;

        /// <summary>
        /// Price Book - AssociateCatalogToBookCommand
        /// </summary>
        private readonly Sitecore.Commerce.Plugin.Pricing.AssociateCatalogToBookCommand _associateCatalogToPriceBookCommand;

        /// <summary>
        /// Promotion Book - AssociateCatalogToBookCommand
        /// </summary>
        private readonly Sitecore.Commerce.Plugin.Promotions.AssociateCatalogToBookCommand _associateCatalogToPromotionBookCommand;

        /// <summary>
        /// Inventory Set - AssociateCatalogToInventorySetCommand
        /// </summary>
        private readonly AssociateCatalogToInventorySetCommand _associateCatalogToInventorySetCommand;

        /// <summary>
        /// Does Entity Exist Pipeline
        /// </summary>
        private readonly FindEntityCommand _findEntityCommand;

        /// <summary>
        /// Promotion Book - DisassociateCatalogFromBookCommand
        /// </summary>
        private readonly Sitecore.Commerce.Plugin.Promotions.DisassociateCatalogFromBookCommand _disassociateCatalogFromPromotionBookCommand;

        /// <summary>
        /// Price Book - DisassociateCatalogFromBookCommand
        /// </summary>
        private readonly Sitecore.Commerce.Plugin.Pricing.DisassociateCatalogFromBookCommand _disassociateCatalogFromPriceBookCommand;

        /// <summary>
        /// DisassociateCatalogFromInventorySetCommand
        /// </summary>
        private readonly DisassociateCatalogFromInventorySetCommand _disassociateCatalogFromInventorySetCommand;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="getCatalogCommand">getCatalogCommand</param>
        /// <param name="createCatalogCommand">Create Catalog Command</param>
        /// <param name="editCatalogCommand">Edit Catalog Command</param>
        /// <param name="associateCatalogToPromotionBookCommand">associateCatalogToPromotionBookCommand</param>
        /// <param name="associateCatalogToPriceBookCommand">associateCatalogToPriceBookCommand</param>
        /// <param name="doesEntityExistPipeline">doesEntityExistPipeline</param>
        /// <param name="findEntityCommand">findEntityCommand</param>
        /// <param name="associateCatalogToInventorySetCommand">associateCatalogToInventorySetCommand</param>
        /// <param name="disassociateCatalogFromPromotionBookCommand">disassociateCatalogFromPromotionBookCommand</param>
        /// <param name="disassociateCatalogFromPriceBookCommand">disassociateCatalogFromPriceBookCommand</param>
        /// <param name="disassociateCatalogFromInventorySetCommand">DisassociateCatalogFromInventorySetCommand</param>
        public CatalogImporter(
            GetCatalogCommand getCatalogCommand,
            CreateCatalogCommand createCatalogCommand,
            EditCatalogCommand editCatalogCommand,
            Sitecore.Commerce.Plugin.Promotions.AssociateCatalogToBookCommand associateCatalogToPromotionBookCommand,
            Sitecore.Commerce.Plugin.Pricing.AssociateCatalogToBookCommand associateCatalogToPriceBookCommand,
            FindEntityCommand findEntityCommand,
            AssociateCatalogToInventorySetCommand associateCatalogToInventorySetCommand,
            Sitecore.Commerce.Plugin.Promotions.DisassociateCatalogFromBookCommand disassociateCatalogFromPromotionBookCommand,
            Sitecore.Commerce.Plugin.Pricing.DisassociateCatalogFromBookCommand disassociateCatalogFromPriceBookCommand,
            DisassociateCatalogFromInventorySetCommand disassociateCatalogFromInventorySetCommand)
        {
            _getCatalogCommand = getCatalogCommand;
            _createCatalogCommand = createCatalogCommand;
            _editCatalogCommand = editCatalogCommand;
            _associateCatalogToPromotionBookCommand = associateCatalogToPromotionBookCommand;
            _associateCatalogToPriceBookCommand = associateCatalogToPriceBookCommand;
            _findEntityCommand = findEntityCommand;
            _associateCatalogToInventorySetCommand = associateCatalogToInventorySetCommand;
            _disassociateCatalogFromPromotionBookCommand = disassociateCatalogFromPromotionBookCommand;
            _disassociateCatalogFromPriceBookCommand = disassociateCatalogFromPriceBookCommand;
            _disassociateCatalogFromInventorySetCommand = disassociateCatalogFromInventorySetCommand;
        }

        /// <summary>
        /// Interface to Create or Update Catalog
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="parameter">parameter</param>
        /// <param name="updateExisting">Flag to determine if an existing catalog should be updated</param>
        /// <returns>Commerce Command</returns>
        public async Task<CommerceCommand> ExecuteImport(
            CommerceContext context,
            CreateOrUpdateCatalogParameter parameter,
            bool updateExisting = false)
        {
            CommerceEntity priceBookEntity = null;
            CommerceEntity promotionBookEntity = null;
            CommerceEntity inventorySetEntity = null;

            // Try to create a new catalog
            Catalog catalog = await this._createCatalogCommand.Process(context, parameter.Name, parameter.DisplayName);

            // If the catalog already exist - check if it should be updated
            if (catalog == null && !updateExisting)
            {
                return this._createCatalogCommand;
            }

            // If it does already exist - get it
            if (catalog == null)
            {
                catalog = await this._getCatalogCommand.Process(context, parameter.Name.ToEntityId<Catalog>());
            }

            // Association of Price Book
            if (!string.IsNullOrEmpty(parameter.PriceBookName))
            {
                if (!string.IsNullOrEmpty(catalog.PriceBookName) && !catalog.PriceBookName.Equals(parameter.PriceBookName))
                {
                    bool success = await this._disassociateCatalogFromPriceBookCommand.Process(context, catalog.PriceBookName, catalog.Name);
                }

                priceBookEntity = await this._findEntityCommand.Process(context, typeof(PriceBook), parameter.PriceBookName.ToEntityId<PriceBook>(), false);
                if (priceBookEntity != null)
                {
                    bool priceBookAssociationResult = await this._associateCatalogToPriceBookCommand.Process(context, parameter.PriceBookName, parameter.Name);
                }
            }

            // Association of Promotion Book
            if (!string.IsNullOrEmpty(parameter.PromotionBookName))
            {
                if (string.IsNullOrEmpty(catalog.PromotionBookName) && !catalog.PromotionBookName.Equals(parameter.PromotionBookName))
                {
                    bool success = await this._disassociateCatalogFromPromotionBookCommand.Process(context, catalog.PromotionBookName, catalog.Name);
                }

                promotionBookEntity = await this._findEntityCommand.Process(context, typeof(PromotionBook), parameter.PromotionBookName.ToEntityId<PromotionBook>(), false);
                if (promotionBookEntity != null)
                {
                    bool promotionBookAssociationResult = await this._associateCatalogToPromotionBookCommand.Process(context, parameter.PromotionBookName, parameter.Name);
                }
            }

            // Association of Default Inventory Set Book
            if (!string.IsNullOrEmpty(parameter.DefaultInventorySetName))
            {
                if (string.IsNullOrEmpty(catalog.DefaultInventorySetName) && !catalog.DefaultInventorySetName.Equals(parameter.DefaultInventorySetName))
                {
                    bool success = await this._disassociateCatalogFromInventorySetCommand.Process(context, catalog.DefaultInventorySetName, catalog.Name);
                }

                inventorySetEntity = await this._findEntityCommand.Process(context, typeof(InventorySet), parameter.DefaultInventorySetName.ToEntityId<InventorySet>(), false);
                if (inventorySetEntity != null)
                {
                    bool inventorySetAssociationResult = await this._associateCatalogToInventorySetCommand.Process(context, parameter.DefaultInventorySetName, parameter.Name);
                }
            }

            // Edit existing catalog
            Catalog editedCatalog = await this._editCatalogCommand.Process(
                context,
                catalog,
                parameter.DisplayName,
                priceBookEntity != null ? parameter.PriceBookName : catalog.PriceBookName,
                promotionBookEntity != null ? parameter.PromotionBookName : catalog.PromotionBookName,
                inventorySetEntity != null ? parameter.DefaultInventorySetName : catalog.DefaultInventorySetName);
            return this._editCatalogCommand;
        }
    }
}
