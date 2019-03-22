using System;

namespace CHAD.Model.AgroHydrologyModule
{
    public class InputCropEvapTrans
    {
        public int Day { get; set; }
        public int CropType { get; set; }
        public String CropName { get; set; }
        public decimal Quantity { get; set; }
    }
}