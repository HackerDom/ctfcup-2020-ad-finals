using VaporService.Helpers;

namespace VaporService.Models
{
    public class Weapon
    {
        public string Name { get; set; }
        public bool IsVorpal { get; set; }
        public double Force { get; set; }
        public string ArcaneProperty { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}