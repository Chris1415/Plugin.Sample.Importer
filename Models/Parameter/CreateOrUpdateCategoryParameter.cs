using System.Collections.Generic;

namespace Plugin.Sample.Importer.Models.Parameter
{
    public class CreateOrUpdateCategoryParameter
    {
        public string CatalogName { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> ParentNames { get; set; }
    }
}
