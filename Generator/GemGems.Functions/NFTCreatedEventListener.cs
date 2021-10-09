using GemGems.App.Generator;
using GemGems.App.Models;
using GemGems.Functions.Triggers.RPCTrigger;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GemGems.Functions
{
    public class NFTCreatedEventListener
    {
        private const string Abi = "[{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"Carat\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"Colour\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"Clarity\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"CutQuality\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"Cutshape\",\"type\":\"uint256\"}],\"name\":\"NFTMinted\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"mint\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"}]";
        private const string ContractAddress = "0xfBEB08784f6F3488e5873ff84A146a34a6884Fc1";
        private const string EventName = "NFTMinted";
        private readonly IStorageService uploadService;
        private NFTGeneratorConfig _config;

        public NFTCreatedEventListener(IStorageService uploadService, IOptions<NFTGeneratorConfig> config)
        {
            _config = config.Value;
            this.uploadService = uploadService;
        }

        [FunctionName(nameof(NFTCreatedEventListener))]
        public async Task Run(
            [RPCEventTrigger(abi: Abi, contractAddress: ContractAddress, eventName: EventName)] RPCEventData eventData,
            ILogger log)
        {
            string logMessage = $"Event data:\nBlock number: {eventData.BlockNumber}\n{string.Join('\n', eventData.Values.Select(value => $"{value.Key}: {value.Value}"))}";

            log.LogInformation(logMessage);

            var metadata = new Metadata
            {
                Carat = int.Parse(eventData.Values["Carat"]),
                Colour = int.Parse(eventData.Values["Colour"]),
                Clarity = int.Parse(eventData.Values["Clarity"]),
                CutQuality = int.Parse(eventData.Values["CutQuality"]),
                Cutshape = int.Parse(eventData.Values["Cutshape"])
            };

            var serializedMetadata = JsonSerializer.Serialize(metadata);

            var path = $"{metadata.Dna}.png";
            using Stream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedMetadata));
            await uploadService.UploadAsync(memoryStream, path, Constants.Storage.NFTRequestContainer, "application/json");

            log.LogInformation($"NFT Request uploaded to {path}");
        }
    }
}
