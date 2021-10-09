using System.Collections.Generic;

namespace GemGems.Functions
{
    internal class Constants
    {
        internal class Storage
        {
            internal const string FinalNFTContainer = "final-nfts";
            internal const string LayersContainer = "layers";
            internal const string NFTRequestContainer = "nft-requests";
        }

        public static Dictionary<int, string> Clariaties
        {
            get => new Dictionary<int, string> {
                { 1, "0" }, 
                { 2, "FL" }, 
                { 3, "IF" }, 
                { 4, "L1" }, 
                { 5, "L2 " }, 
                { 6, "L3" }, 
                { 7, "VS1" }, 
                { 8, "VS2" }, 
                { 9, "VVS1" }, 
                { 10, "VVS2" }
            };
        }

        public static Dictionary<int, string> Backgrounds
        {
            get => new Dictionary<int, string> {
                { 1, "D-F" }, 
                { 2, "G" }, 
                { 3, "H" }, 
                { 4, "I" },//Careful 
                { 5, "J" }, 
                { 6, "J" }, 
                { 7, "K" }, 
                { 8, "L" }, 
                { 9, "M" }, 
                { 10, "N" },
                { 11, "O" },
                { 12, "P" },
                { 13, "Q" },
                { 14, "R" },
                { 15, "S-V" }
            };
        }

        public static Dictionary<int, string> Cuts
        {
            get => new Dictionary<int, string> {
                { 1, "Asscher" }, 
                { 2, "Baguette" }, 
                { 3, "Cushion" }, 
                { 4, "Emerald" },
                { 5, "Heart" }, 
                { 6, "Marquise" }, 
                { 7, "Oval" }, 
                { 8, "Pear" }, 
                { 9, "Princess" }, 
                { 10, "Radiant" },
                { 11, "Round" },
                { 12, "Trilliant" }
            };
        }

        public static Dictionary<int, string> CutTitles
        {
            get => new Dictionary<int, string> {
                { 1, "Asscher" }, 
                { 2, "Baguette" }, 
                { 3, "Cushion" }, 
                { 4, "Emerald" },
                { 5, "Heart" }, 
                { 6, "Marquise" }, 
                { 7, "Oval" }, 
                { 8, "Pear" }, 
                { 9, "Princess" }, 
                { 10, "Radiant" },
                { 11, "Round" },
                { 12, "Trilliant" }
            };
        }
    }
}
