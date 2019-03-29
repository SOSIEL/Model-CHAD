namespace CHAD.Model
{
    public class SOSIELResult
    {
        #region Constructors

        public SOSIELResult(double numOfAlfalfaAcres, double numOfBarleyAcres, double numOfCRPAcres,
            double numOfWheatAcres)
        {
            NumOfAlfalfaAcres = numOfAlfalfaAcres;
            NumOfBarleyAcres = numOfBarleyAcres;
            NumOfCRPAcres = numOfCRPAcres;
            NumOfWheatAcres = numOfWheatAcres;
        }

        #endregion

        #region Public Interface

        public double NumOfAlfalfaAcres { get; }

        public double NumOfBarleyAcres { get; }

        public double NumOfCRPAcres { get; }

        public double NumOfWheatAcres { get; }

        #endregion
    }
}