using CHAD.Model.RVACModule;

namespace CHAD.Model.SimulationResults
{
    public struct RVACResult
    {
        public RVACResult(double profitTotal, double profitAlfalfa, double profitBarley, double profitWheat,
            double profitCRP)
        {
            ProfitTotal = profitTotal;
            ProfitAlfalfa = profitAlfalfa;
            ProfitBarley = profitBarley;
            ProfitWheat = profitWheat;
            ProfitCRP = profitCRP;
        }

        public RVACResult(RVAC rvac)
            : this(rvac.ProfitTotal, rvac.ProfitAlfalfa, rvac.ProfitBarley, rvac.ProfitWheat, rvac.ProfitCRP)
        {
        }

        public double ProfitAlfalfa { get; }

        public double ProfitBarley { get; }

        public double ProfitCRP { get; }

        public double ProfitTotal { get; }

        public double ProfitWheat { get; }
    }
}