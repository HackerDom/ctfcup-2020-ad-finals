using VaporService.Helpers;

namespace VaporService.Models
{
    public class Jabberwocky
    {
        public string BreedingSeed { get; set; }
        public string Breeder { get; set; }
        public string ArcaneProperty { get; set; }
        public double Force { get; set; }
        public bool HasJawsThatBite { get; set; }
        public bool HasClawsThatCatch { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}