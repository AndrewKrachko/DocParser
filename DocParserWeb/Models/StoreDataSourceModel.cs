using DocParser;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocParserWeb.Models
{
    public class StoreDataSourceModel
    {
        public string input_mode { get; set; }
        public readonly string[] input_modes = new[] { "fileSystemRadio", "webRadio" };
        public string input_address { get; set; }
        public IFormFile[] input_folder { get; set; }
        public List<StoreItem> storeItems { get; set; }
    }
}
