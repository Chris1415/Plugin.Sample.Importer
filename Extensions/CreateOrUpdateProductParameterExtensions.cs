using Plugin.Sample.Importer.Models.Parameter;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using System.Linq;

namespace Plugin.Sample.Importer.Extensions
{
    /// <summary>
    /// CreateOrUpdateProductParameterExtensions
    /// </summary>
    /// <author>
    /// Christian Hahn
    /// </author>
    public static class CreateOrUpdateProductParameterExtensions
    {
        /// <summary>
        /// Converty input update parameter to updated sellableitem
        /// </summary>
        /// <param name="input">input parameter</param>
        /// <param name="sellableItem">existing sellableitem</param>
        /// <returns>updated sellable item</returns>
        public static SellableItem UpdateSellableItem(this CreateOrUpdateProductParameter input, SellableItem sellableItem)
        {
            sellableItem.Brand = input.Brand;
            sellableItem.DisplayName = input.DisplayName;
            sellableItem.Manufacturer = input.Manufacturer;
            sellableItem.TypeOfGood = input.TypeOfGood;
            sellableItem.Tags = input.Tags.Select(element => new Tag(element)).ToList();
            sellableItem.Description = input.Description;

            return sellableItem;
        }
    }
}
