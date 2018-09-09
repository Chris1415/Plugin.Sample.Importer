// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.Sample
{
    using global::Plugin.Sample.Importer.Services.Implementation;
    using global::Plugin.Sample.Importer.Services.Interface;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //services.RegisterAllPipelineBlocks(assembly);

            services.AddTransient<ICatalogImporter, CatalogImporter>();
            services.AddTransient<ICategoryImporter, CategoryImporter>();
            services.AddTransient<IProductImporter, ProductImporter>();
            services.AddTransient<IVariantImporter, VariantImporter>();

            services.Sitecore().Pipelines(config => config
               .ConfigurePipeline<IConfigureOpsServiceApiPipeline>(c => { c.Add<ConfigureOpsServiceApiBlock>(); }));

            //services.RegisterAllCommands(assembly);
        }
    }
}