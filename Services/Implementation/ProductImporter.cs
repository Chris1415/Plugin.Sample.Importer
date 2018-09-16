using Microsoft.Extensions.Logging;
using Plugin.Sample.Importer.Extensions;
using Plugin.Sample.Importer.Models.Parameter;
using Plugin.Sample.Importer.Services.Interface;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.EntityViews.Commands;
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
        /// Get Entity View Command
        /// </summary>
        private readonly GetEntityViewCommand _getEntityViewCommand;

        /// <summary>
        /// Do Action Command
        /// </summary>
        private readonly DoActionCommand _doActionCommand;

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
            GetSellableItemConnectCommand getSellableItemConnectCommand,
            GetEntityViewCommand getEntityViewCommand,
            DoActionCommand doActionCommand)
        {
            _createSellableItemCommand = createSellableItemCommand;
            _associateSellableItemToParentCommand = associateSellableItemToParentCommand;
            _getSellableItemCommand = getSellableItemCommand;
            _editSellableItemCommand = editSellableItemCommand;
            _getSellableItemConnectCommand = getSellableItemConnectCommand;
            _getEntityViewCommand = getEntityViewCommand;
            _doActionCommand = doActionCommand;
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

            // If existing - Check if it should be updated
            if (sellableItem == null && !updateExisting)
            {
                return this._createSellableItemCommand;
            }

            // If existing - Get Current SellableItem 
            bool createdFlag = true;
            if (sellableItem == null)
            {
                createdFlag = false;
                // Get SellableItem 
                sellableItem = await this._getSellableItemCommand.Process(context, $"{parameter.CatalogName}|{parameter.ProductId}|", false);
            }

            // Edit Composer Generated View Properties
            //******************************************

            // Get General Sellable Item Overview View
            var sellableItemEntityView = await _getEntityViewCommand.Process(context, sellableItem.Id, "Master", "", "");

            // Extract the Composer Generated View
            var composerView = sellableItemEntityView.ChildViews.Where(x => x.Name == "Custom Plugins View").FirstOrDefault() as EntityView;

            // Call an Edit View of the Composer Generated View
            EntityView composerViewForEdit = await _getEntityViewCommand.Process(context, sellableItem.Id, "EditView", "EditView", composerView.ItemId);

            // Get the Property we want to change - This time the Taxes property for demonstration reason
            // At this point we could also iterate through a various number of properties based on input parameters
            ViewProperty propertyToChange = composerViewForEdit.Properties.FirstOrDefault(element => element.Name.Equals("Taxes"));
            if (propertyToChange != null)
            {
                // Special Case - Out Taxes property has an availableSelectionPolicy we want to obtain
                // Currently only values 0.07 and 0.19 are allowed - Selection Option Contraint from Composer
                AvailableSelectionsPolicy availableSelectionPolicy = propertyToChange.Policies.FirstOrDefault(element => element is AvailableSelectionsPolicy) as AvailableSelectionsPolicy;
                string newValue = "0.19";

                // Check if our new value can be found within all selections
                Selection isAvailable = availableSelectionPolicy.List.FirstOrDefault(element => element.Name.Equals(newValue));

                if (isAvailable != null)
                {
                    // If so - change the value
                    propertyToChange.Value = newValue;
                    propertyToChange.RawValue = newValue;
                }
                else
                {
                    // If not - Obtain the constraint and dont change the value
                    context.Logger.LogDebug(string.Format("New Value {0} is not allowed for property {1}", newValue, propertyToChange.Name));
                }
            }

            // In the end update the changed view
            var result = await _doActionCommand.Process(context, composerViewForEdit);

            // End Edit Composer generated Propertis
            //*********************************************

            // Edit SellableItem only if it is not created within that call
            if (!createdFlag)
            {
                // Todo There is currently an error that if editsellableitemcommand is executed SQL server will throw a PK exception when it tries to ADD the current sellableitem again to DB
                // Sitecore Support Ticket ID 515689
                // CatalogContentArgument catalogContentArgument = await this._editSellableItemCommand.Process(context, parameter.UpdateSellableItem(sellableItem));
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
