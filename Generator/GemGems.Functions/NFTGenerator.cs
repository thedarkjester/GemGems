using GemGems.App.Generator;
using GemGems.App.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GemGems.Functions
{
    public class NFTGenerator
    {
        private readonly IStorageService _uploadService;
        private readonly IImageFactory _imageFactory;

        public NFTGenerator(IStorageService uploadService, IImageFactory imageFactory)
        {
            this._uploadService = uploadService;
            this._imageFactory = imageFactory;
        }

        [FunctionName(nameof(NFTGenerator))]
        public async Task Run([BlobTrigger("nft-requests/{name}", Connection = "AzureStorage")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var metadata = await JsonSerializer.DeserializeAsync<Metadata>(myBlob);

            log.LogInformation($"Creating NFT With Properties:\n Carat: {metadata.Carat} \nClarity: {metadata.Clarity} \nColour: {metadata.Colour} \nClarity: {metadata.Clarity} \nCutQuality: {metadata.CutQuality} \nCutQuality: {metadata.CutQuality} \nDna: {metadata.Dna}");

            var fetchLayers = GetNFTLayers(
                ("Background", $"{Constants.Backgrounds[metadata.Colour]}.jpg", true, GetFile, CalculateBackgroundPosition),
                ("ShapeIcon", $"Icon {Constants.CutTitles[metadata.Cutshape]}.png", false, GetFile, CalculateCutPosition),
                ("Cut", $"{Constants.Cuts[metadata.Cutshape]}.png", false, GetFile, CalculateCutPosition),
                ("Clarity", $"{Constants.Clariaties[metadata.Clarity]}.jpg", false, GetFile, CalculateClarityPosition),
                ("CutTitle", $"{Constants.CutTitles[metadata.Cutshape]}.png", false, GetFile, CalculateTitlePosition)
                ).ToList();



            log.LogInformation("Fetching layers..");

            log.LogInformation("Generating complete image..");

            var completedNfts =  await _imageFactory.CombineAsync(fetchLayers);

            log.LogInformation("Uploading to final image container..");

            await _uploadService.UploadAsync(completedNfts, $"{name}", Constants.Storage.FinalNFTContainer, "image/jpeg");
        }

        private Task<Stream> GetFile(string folder, string fileName) => _uploadService.GetFileUriAsync($"{folder}/{fileName}", Constants.Storage.LayersContainer);

        public (int X, int Y, int? Scale) CalculateCutPosition(LayerPosition position)
        {
            var x = position.BaseX / 2 - position.CurrentX / 2;

            var y = position.BaseY / 2 - position.CurrentY / 2;

            return (x, y, null);
        }
        public (int X, int Y, int? Scale) CalculateTitlePosition(LayerPosition position)
        {
            var x = position.BaseX * 0.10;

            var y = position.BaseY - (position.BaseY * 0.20);

            return ((int)x, (int)y, 4);
        }

        public (int X, int Y, int? Scale) CalculateBackgroundPosition(LayerPosition position) => (0, 0, null);

        public (int X, int Y, int? Scale) CalculateClarityPosition(LayerPosition position)
        {
            var x = position.BaseX / 2 - position.CurrentX / 2 + (position.BaseX * 0.15);

            var y = position.BaseY / 2 - position.CurrentY / 2 + (position.BaseX * 0.05);

            return ((int)x, (int)y, null);
        }

        private IEnumerable<Layer> GetNFTLayers(params
            (string Folder,
            string FileName,
            bool IsBase,
            Func<string, string, Task<Stream>> GetFile,
            Func<LayerPosition, (int X, int Y, int? Scale)> CalculateOffsets)[] layers) =>
            layers.Select(x => new Layer
            {
                File = GetFile(x.Folder, x.FileName),
                Modifications = new Layer.Modification
                {
                    IsBase = x.IsBase,
                    CalculateOffsets = x.CalculateOffsets
                }
            });
    }
}
