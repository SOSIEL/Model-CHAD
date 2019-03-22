using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Hydrology
    {
        public int Day { get; set; }
        public int Field { get; set; }
        public decimal WaterInAquifer { get; set; }
        public decimal WaterInField { get; set; }
    }
}
