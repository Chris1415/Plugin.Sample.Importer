using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Sample.Importer.Models.Parameter
{
    public class CreateOrUpdateProductParameter
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string ParentName { get; set; }

        public string CatalogName { get; set; }

        public string ProductId { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public string Brand { get; set; }

        public string Manufacturer { get; set; }

        public string TypeOfGood { get; set; }
    }
}
