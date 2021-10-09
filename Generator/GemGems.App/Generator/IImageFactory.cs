using GemGems.App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GemGems.App.Generator
{
    public interface IImageFactory
    {
        Task<byte[]> CombineAsync(IEnumerable<Layer> layers);
    }
}

