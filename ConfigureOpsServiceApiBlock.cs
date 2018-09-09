// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureServiceApiBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.Sample
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.OData.Builder;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// Defines a block which configures the OData model
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName("SamplePluginImporterConfigureOpsServiceApiBlock")]
    public class ConfigureOpsServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="modelBuilder">
        /// The argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="ODataConventionModelBuilder"/>.
        /// </returns>
        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            // Add the entities

            // Add the entity sets

            // Add complex types

            // Add unbound functions

            // Add unbound actions
            var executeCatalogImport = modelBuilder.Action("CreateOrUpdateCatalog");
            executeCatalogImport.Parameter<string>("Name");
            executeCatalogImport.Parameter<string>("DisplayName");
            executeCatalogImport.Parameter<string>("PriceBookName");
            executeCatalogImport.Parameter<string>("PromotionBookName");
            executeCatalogImport.Parameter<string>("DefaultInventorySetName");
            executeCatalogImport.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var executeCategoryImport = modelBuilder.Action("CreateOrUpdateCategory");
            executeCategoryImport.Parameter<string>("Name");
            executeCategoryImport.Parameter<string>("DisplayName");
            executeCategoryImport.Parameter<string>("ParentNames");
            executeCategoryImport.Parameter<string>("Description");
            executeCategoryImport.Parameter<string>("CatalogName");
            executeCategoryImport.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var executeSellableItemImport = modelBuilder.Action("CreateOrUpdateSellableItem");
            executeSellableItemImport.Parameter<string>("Name");
            executeSellableItemImport.Parameter<string>("DisplayName");
            executeSellableItemImport.Parameter<string>("ParentName");
            executeSellableItemImport.Parameter<string>("Description");
            executeSellableItemImport.Parameter<string>("CatalogName");
            executeSellableItemImport.Parameter<string>("ProductId");
            executeSellableItemImport.Parameter<string>("Brand");
            executeSellableItemImport.Parameter<string>("Manufacturer");
            executeSellableItemImport.Parameter<string>("TypeOfGood");
            executeSellableItemImport.Parameter<string>("Tags");
            executeSellableItemImport.ReturnsFromEntitySet<CommerceCommand>("Commands");

            var executeSellableItemVariationImport = modelBuilder.Action("CreateOrUpdateSellableItemVariation");
            executeSellableItemVariationImport.Parameter<string>("Name");
            executeSellableItemVariationImport.Parameter<string>("DisplayName");
            executeSellableItemVariationImport.Parameter<string>("ProductName");
            executeSellableItemVariationImport.Parameter<string>("VariantName");
            executeSellableItemVariationImport.Parameter<string>("CatalogName");
            executeSellableItemVariationImport.ReturnsFromEntitySet<CommerceCommand>("Commands");

            return Task.FromResult(modelBuilder);
        }
    }
}
