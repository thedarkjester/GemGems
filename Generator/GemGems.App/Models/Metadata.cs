namespace GemGems.App.Models
{
    public class Metadata
    {
        public int Cutshape { get; set; }
        public int Colour { get; set; }
        public int CutQuality { get; set; }
        public int Carat { get; set; }
        public int Clarity { get; set; }
        public string Dna => $"{Cutshape}{Colour}{CutQuality}{Carat}{Clarity}";
    }
}
