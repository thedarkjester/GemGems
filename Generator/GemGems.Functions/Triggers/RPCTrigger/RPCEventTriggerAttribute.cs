using Microsoft.Azure.WebJobs.Description;
using System;

namespace GemGems.Functions.Triggers.RPCTrigger
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RPCEventTriggerAttribute : Attribute
    {
        private const string DefaultUrl = "http://localhost:7545/";

        public RPCEventTriggerAttribute(string abi, string contractAddress, string eventName, string url = DefaultUrl)
        {
            Url = url;
            Abi = abi;
            ContractAddress = contractAddress;
            EventName = eventName;
        }

        public string Url { get; private set; }
        public string Abi { get; private set; }
        public string ContractAddress { get; private set; }
        public string EventName { get; set; }
    }
}
