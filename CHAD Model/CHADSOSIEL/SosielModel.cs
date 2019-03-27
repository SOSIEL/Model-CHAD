using System.Collections.Generic;

namespace CHADSOSIEL
{
    public class SosielModel 
    {
        public double WaterInAquifire { get; set; }

        public double WaterCurtailmentRate { get; set; }

        public ICollection<ChadField> Fields { get; set; }
    }
}
