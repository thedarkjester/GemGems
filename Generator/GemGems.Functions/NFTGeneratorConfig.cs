using System.Collections.Generic;

namespace GemGems.Functions
{
    public class NFTGeneratorConfig
    {
        public string ContractABI { get; set; }
        public Dictionary<string,string> Cuts { get; set; }
    }
}