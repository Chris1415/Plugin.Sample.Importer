namespace Plugin.Sample.Importer.Models.Parameter
{
    /// <summary>
    /// CreateOrUpdateCatalogParameter
    /// </summary>
    public class CreateOrUpdateCatalogParameter
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Price Book Name
        /// </summary>
        public string PriceBookName { get; set; }

        /// <summary>
        /// Promotion Book Name
        /// </summary>
        public string PromotionBookName { get; set; }

        /// <summary>
        /// Default Inventory Set Name
        /// </summary>
        public string DefaultInventorySetName { get; set; }
    }
}
