namespace Sitecore.Commerce.Plugin.Sample
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http.OData;
    using global::Plugin.Sample.Importer.Models.Parameter;
    using global::Plugin.Sample.Importer.Services.Interface;
    using Microsoft.AspNetCore.Mvc;
    using Sitecore.Commerce.Core;

    /// <inheritdoc />
    /// <summary>
    /// Defines a controller
    /// </summary>
    /// <seealso cref="T:Sitecore.Commerce.Core.CommerceController" />
    public class CommandsController : CommerceController
    {
        /// <summary>
        /// Catalog Importer Reference
        /// </summary>
        private readonly ICatalogImporter _catalogImporter;

        /// <summary>
        /// Catagory Importer Reference
        /// </summary>
        private readonly ICategoryImporter _categoryImporter;

        /// <summary>
        /// Product Importer
        /// </summary>
        private readonly IProductImporter _productImporter;

        /// <summary>
        /// Variant Importer
        /// </summary>
        private readonly IVariantImporter _variantImporter;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Commerce.Plugin.Sample.CommandsController" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="globalEnvironment">The global environment.</param>
        /// <param name="catalogImporter">Catalog Importer</param>
        /// <param name="categoryImporter">Category Importer</param>
        /// <param name="productImporter">product importer</param>
        /// <param name="variantImporter">variant Importer</param>
        public CommandsController(
            IServiceProvider serviceProvider,
            CommerceEnvironment globalEnvironment,
            ICatalogImporter catalogImporter,
            ICategoryImporter categoryImporter,
            IProductImporter productImporter,
            IVariantImporter variantImporter)
            : base(serviceProvider, globalEnvironment)
        {
            _catalogImporter = catalogImporter;
            _categoryImporter = categoryImporter;
            _productImporter = productImporter;
            _variantImporter = variantImporter;
        }

        /// <summary>
        /// ExecuteCatalogImport command.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="IActionResult"/></returns>
        [HttpPut]
        [Route("commerceops/CreateOrUpdateCatalog()")]
        public async Task<IActionResult> CreateOrUpdateCatalog([FromBody] ODataActionParameters value)
        {
            string name = value["Name"].ToString();
            string displayName = value["DisplayName"].ToString();
            string priceBookName = value["PriceBookName"].ToString();
            string promotionBookName = value["PromotionBookName"].ToString();
            string defaultInventorySetName = value["DefaultInventorySetName"].ToString();

            var result = await _catalogImporter.ExecuteImport(this.CurrentContext, new CreateOrUpdateCatalogParameter()
            {
                DisplayName = displayName,
                Name = name,
                PriceBookName = priceBookName,
                PromotionBookName = promotionBookName,
                DefaultInventorySetName = defaultInventorySetName
            }, true);

            return new ObjectResult(result);
        }

        /// <summary>
        /// ExecuteCatalogImport command.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="IActionResult"/></returns>
        [HttpPut]
        [Route("commerceops/CreateOrUpdateCategory()")]
        public async Task<IActionResult> CreateOrUpdateCategory([FromBody] ODataActionParameters value)
        {
            string name = value["Name"].ToString();
            string displayName = value["DisplayName"].ToString();
            string parentNames = value["ParentNames"].ToString();
            string catalogName = value["CatalogName"].ToString();
            string description = value["Description"].ToString();

            var result = await _categoryImporter.ExecuteImport(this.CurrentContext, new CreateOrUpdateCategoryParameter()
            {
                DisplayName = displayName,
                Name = name,
                CatalogName = catalogName,
                Description = description,
                ParentNames = parentNames.Split('|'),
            }, true);


            return new ObjectResult(result);
        }

        /// <summary>
        /// ExecuteCatalogImport command.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="IActionResult"/></returns>
        [HttpPut]
        [Route("commerceops/CreateOrUpdateSellableItem()")]
        public async Task<IActionResult> CreateOrUpdateSellableItem([FromBody] ODataActionParameters value)
        {
            string name = value["Name"].ToString();
            string displayName = value["DisplayName"].ToString();
            string parentName = value["ParentName"].ToString();
            string catalogName = value["CatalogName"].ToString();
            string productId = value["ProductId"].ToString();
            string description = value["Description"].ToString();
            string brand = value["Brand"].ToString();
            string manufacturer = value["Manufacturer"].ToString();
            string typeOfGood = value["TypeOfGood"].ToString();
            string tags = value["Tags"].ToString();

            var result = await _productImporter.ExecuteImport(this.CurrentContext, new CreateOrUpdateProductParameter()
            {
                DisplayName = displayName,
                Name = name,
                Brand = brand,
                CatalogName = catalogName,
                Manufacturer = manufacturer,
                ParentName = parentName,
                ProductId = productId,
                TypeOfGood = typeOfGood,
                Tags = tags.Split('|'),
                Description = description,
            }, true);

            return new ObjectResult(result);
        }

        /// <summary>
        /// ExecuteCatalogImport command.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="IActionResult"/></returns>
        [HttpPut]
        [Route("commerceops/CreateOrUpdateSellableItemVariation()")]
        public async Task<IActionResult> CreateOrUpdateSellableItemVariation([FromBody] ODataActionParameters value)
        {
            string name = value["Name"].ToString();
            string displayName = value["DisplayName"].ToString();
            string productName = value["ProductName"].ToString();
            string variantName = value["VariantName"].ToString();
            string catalogName = value["CatalogName"].ToString();

            var result = await this._variantImporter.ExecuteImport(this.CurrentContext, new CreateOrUpdateVariantParameter()
            {
                DisplayName = displayName,
                Name = name,
                ProductName = productName,
                VariantName = variantName,
                CatalogName = catalogName
            }, true);

            return new ObjectResult(result);
        }
    }
}

