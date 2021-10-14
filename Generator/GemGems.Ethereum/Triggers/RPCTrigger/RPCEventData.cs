using Nethereum.Hex.HexTypes;
using System.Collections.Generic;

namespace GemGems.Functions.Triggers.RPCTrigger
{
    public class RPCEventData
    {
        public Dictionary<string, string> Values { get; internal set; }
        public HexBigInteger BlockNumber { get; internal set; }
    }
}
