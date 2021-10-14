using System;
using System.IO;
using System.Threading.Tasks;

namespace GemGems.App.Models
{
    public class Layer
    {
        public Layer()
        {
            Modifications = new Modification
            {

            };
        }

        public int Id { get; set; }
        public string Folder { get; set; }
        public string FileName { get; set; }
        public Task<Stream> File { get; set; }

        public Modification Modifications { get; set; }

        public class Modification
        {
            public int? XOffset { get; set; }
            public int? YOffset { get; set; }
            public bool IsBase { get; set; }

            public int? Scale { get; set; }

            public Func<LayerPosition, (int X, int Y, int? Scale)> CalculateOffsets { get; set; }
        }

        public void SetFile(Func<Task<Stream>> getFile)
        {
            File = getFile();
        }
    }
}


