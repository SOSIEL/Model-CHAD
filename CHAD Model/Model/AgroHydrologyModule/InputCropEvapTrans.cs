namespace CHAD.Model.AgroHydrologyModule
{
    public class InputCropEvapTrans
    {
        #region Public Interface

        public int Day { get; set; }
        public Plant Plant { get; set; }
        public double Quantity { get; set; }

        #endregion
    }
}