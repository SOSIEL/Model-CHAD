using Newtonsoft.Json;
using SOSIEL.Configuration;

namespace CHADSOSIEL.Configuration
{
    /// <summary>
    /// Algorithm configuration model. Used to parse section "AlgorithmConfiguration".
    /// </summary>
    public class AlgorithmConfiguration
    {
        public bool UseDemographicProcesses { get; set; }

        public DemographicProcessesConfiguration DemographicConfiguration { get; set; }

        public ProbabilitiesConfiguration[] ProbabilitiesConfiguration { get; set; }
    }
}
