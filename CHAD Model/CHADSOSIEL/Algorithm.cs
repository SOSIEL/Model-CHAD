using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CHADSOSIEL.Configuration;
using CHADSOSIEL.Helpers;
using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;
using SOSIEL.Processes;

namespace CHADSOSIEL
{
    public sealed class Algorithm : SosielAlgorithm<SosielModel, ChadField>, IAlgorithm<SosielModel>
    {
        public string Name { get { return "SOSIEL"; } }

        private readonly string _configurationFolder;
        private readonly ConfigurationModel _configuration;

        private SosielModel _data;

        public static ProcessesConfiguration GetProcessConfiguration()
        {
            return new ProcessesConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                DecisionOptionSelectionEnabled = true,
                DecisionOptionSelectionPart2Enabled = true,
                SocialLearningEnabled = false,
                CounterfactualThinkingEnabled = true,
                InnovationEnabled = true,
                ReproductionEnabled = false,
                AgentRandomizationEnabled = true,
                AgentsDeactivationEnabled = false,
                AlgorithmStopIfAllAgentsSelectDoNothing = false
            };
        }

        public Algorithm(ConfigurationModel configuration) : base(1, GetProcessConfiguration())
        {
            _configurationFolder = configuration.ConfigurationPath;
            _configuration = configuration;
        }

        public SosielModel Run(SosielModel data)
        {
            _data = data;

            RunSosiel(data.Fields);
            
            return _data;
        }

        /// <summary>
        /// Executes algorithm initialization
        /// </summary>
        public void Initialize(SosielModel data)
        {
            _data = data;

            InitializeAgents();

            InitializeProbabilities();

            if (_configuration.AlgorithmConfiguration.UseDemographicProcesses)
            {
                UseDemographic();
            }

            AfterInitialization();
        }

        protected override void UseDemographic()
        {
            base.UseDemographic();

            demographic = new Demographic<ChadField>(_configuration.AlgorithmConfiguration.DemographicConfiguration,
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.BirthProbabilityTable),
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.DeathProbabilityTable));
        }

        /// <inheritdoc />
        protected override void InitializeAgents()
        {
            var agents = new List<IAgent>();

            Dictionary<string, AgentPrototype> agentPrototypes = _configuration.AgentConfiguration;

            if (agentPrototypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent prototypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = _configuration.InitialState;

            //create agents, groupby is used for saving agents numeration, e.g. FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.PrototypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentPrototype prototype = agentPrototypes[agentStateGroup.Key];
                var mentalProto = prototype.MentalProto;
                int index = 1;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        SOSIEL.Entities.Agent agent = Agent.CreateAgent(agentState, prototype);
                        agent.SetId(index);

                        agents.Add(agent);

                        index++;
                    }
                });
            });

            agentList = new AgentList(agents, agentPrototypes.Select(kvp => kvp.Value).ToList());
        }

        private void InitializeProbabilities()
        {
            var probabilitiesList = new Probabilities();

            foreach (var probabilityElementConfiguration in _configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenerecMethod(variableType, typeof(ProbabilityTableParser), "Parse");

                dynamic table = parseTableMethod.Invoke(null, new object[] { Path.Combine(_configurationFolder, probabilityElementConfiguration.FilePath), probabilityElementConfiguration.WithHeader });

                var addToListMethod =
                    ReflectionHelper.GetGenerecMethod(variableType, typeof(Probabilities), "AddProbabilityTable");

                addToListMethod.Invoke(probabilitiesList, new object[] { probabilityElementConfiguration.Variable, table });
            }

            probabilities = probabilitiesList;
        }

        protected override void AfterInitialization()
        {
            base.AfterInitialization();

            agentList.GetAgentsWithPrefix("PM").ForEach(agent =>
            {
                agent[AlgorithmVariables.WaterInAquiferMax] = _data.WaterInAquiferMax;
                agent[AlgorithmVariables.SustainableLevelAquifer] = _data.SustainableLevelAquifer;
            });

            agentList.GetAgentsWithPrefix("F").ForEach(agent =>
                {
                    agent[AlgorithmVariables.MarketPriceAlfalfa] = _data.MarketPriceAlfalfa;
                    agent[AlgorithmVariables.MarketPriceBarley] = _data.MarketPriceBarley;
                    agent[AlgorithmVariables.MarketPriceWheat] = _data.MarketPriceWheat;
                    agent[AlgorithmVariables.CostAlfalfa] = _data.CostAlfalfa;
                    agent[AlgorithmVariables.CostBarley] = _data.CostBarley;
                    agent[AlgorithmVariables.CostWheat] = _data.CostWheat;
                });
        }

        protected override void PreIterationCalculations(int iteration)
        {
            agentList.GetAgentsWithPrefix("PM").ForEach(agent => agent[AlgorithmVariables.WaterInAquifer] = _data.WaterInAquifer);

            agentList.GetAgentsWithPrefix("F").ForEach(agent =>
                {
                    //cheat
                    if (iteration == 1)
                    {
                        _data.HarvestableAlfalfa = 1;
                        _data.HarvestableBarley = 1;
                        _data.HarvestableWheat = 1;

                        agent[AlgorithmVariables.MarketPriceAlfalfa] = _data.MarketPriceAlfalfa;
                        agent[AlgorithmVariables.MarketPriceBarley] = _data.MarketPriceBarley;
                        agent[AlgorithmVariables.MarketPriceWheat] = _data.MarketPriceWheat;
                        agent[AlgorithmVariables.CostAlfalfa] = _data.CostAlfalfa;
                        agent[AlgorithmVariables.CostBarley] = _data.CostBarley;
                        agent[AlgorithmVariables.CostWheat] = _data.CostWheat;
                    }

                    if (_data.HarvestableAlfalfa > 0)
                        agent[AlgorithmVariables.HarvestableAlfalfa] = _data.HarvestableAlfalfa;

                    if (_data.HarvestableBarley > 0)
                        agent[AlgorithmVariables.HarvestableBarley] = _data.HarvestableBarley;

                    if (_data.HarvestableWheat > 0)
                        agent[AlgorithmVariables.HarvestableWheat] = _data.HarvestableWheat;

                    agent[AlgorithmVariables.ProfitMarginAlfalfa] = (agent[AlgorithmVariables.MarketPriceAlfalfa] - agent[AlgorithmVariables.CostAlfalfa]) * agent[AlgorithmVariables.HarvestableAlfalfa];
                    agent[AlgorithmVariables.ProfitMarginBarley] = (agent[AlgorithmVariables.MarketPriceBarley] - agent[AlgorithmVariables.CostBarley]) * agent[AlgorithmVariables.HarvestableBarley];
                    agent[AlgorithmVariables.ProfitMarginWheat] = (agent[AlgorithmVariables.MarketPriceWheat] - agent[AlgorithmVariables.CostWheat]) * agent[AlgorithmVariables.HarvestableWheat];
                    agent[AlgorithmVariables.ProfitMarginCRP] = _data.SubsidyCRP;
                    agent[AlgorithmVariables.ProfitTotal] = _data.ProfitTotal;

                    agent[AlgorithmVariables.MarketPriceAlfalfa] = _data.MarketPriceAlfalfa;
                    agent[AlgorithmVariables.MarketPriceBarley] = _data.MarketPriceBarley;
                    agent[AlgorithmVariables.MarketPriceWheat] = _data.MarketPriceWheat;
                    agent[AlgorithmVariables.CostAlfalfa] = _data.CostAlfalfa;
                    agent[AlgorithmVariables.CostBarley] = _data.CostBarley;
                    agent[AlgorithmVariables.CostWheat] = _data.CostWheat;
                });
        }

        /// <inheritdoc />
        protected override Dictionary<IAgent, AgentState<ChadField>> InitializeFirstIterationState()
        {
            var states = new Dictionary<IAgent, AgentState<ChadField>>();

            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState<ChadField> agentState = AgentState<ChadField>.Create(agent.Prototype.IsSiteOriented);

                //copy generated goal importance
                agent.InitialGoalStates.ForEach(kvp =>
                {
                    var goalState = kvp.Value;
                    goalState.Value = agent[kvp.Key.ReferenceVariable];

                    agentState.GoalsState[kvp.Key] = goalState;
                });

                states.Add(agent, agentState);
            });

            return states;
        }

        protected override void BeforeActionSelection(IAgent agent, ChadField site)
        {
            if (agent.Prototype.NamePrefix == "F")
            {
                agent[AlgorithmVariables.FieldHistoryCrop] = site.FieldHistoryCrop;
                agent[AlgorithmVariables.FieldHistoryNonCrop] = site.FieldHistoryNonCrop;
                agent[AlgorithmVariables.PlantInField] = AlgorithmVariables.Nothing;
            }
        }

        protected override void AfterActionTaking(IAgent agent, ChadField site)
        {
            if (agent.Prototype.NamePrefix == "PM")
            {
                _data.WaterCurtailmentRate = agent[AlgorithmVariables.WaterCurtailmentRate];
            }

            if (agent.Prototype.NamePrefix == "F")
            {
                site.Plant = agent.ContainsVariable(AlgorithmVariables.PlantInField) ? agent[AlgorithmVariables.PlantInField]: AlgorithmVariables.Nothing;
            }
        }


        protected override void Maintenance()
        {
            base.Maintenance();
        }

        protected override void PostIterationCalculations(int iteration)
        {
            base.PostIterationCalculations(iteration);
        }

        /// <inheritdoc />
        protected override void PostIterationStatistic(int iteration)
        {
            var agent = agentList.GetAgentsWithPrefix("F").First();

            _data.HarvestableAlfalfa = agent[AlgorithmVariables.HarvestableAlfalfa];
            _data.HarvestableBarley = agent[AlgorithmVariables.HarvestableBarley];
            _data.HarvestableWheat = agent[AlgorithmVariables.HarvestableWheat];

            _data.ProfitMarginAlfalfa = agent[AlgorithmVariables.ProfitMarginAlfalfa];
            _data.ProfitMarginBarley = agent[AlgorithmVariables.ProfitMarginBarley];
            _data.ProfitMarginWheat = agent[AlgorithmVariables.ProfitMarginWheat];
        }
    }
}
