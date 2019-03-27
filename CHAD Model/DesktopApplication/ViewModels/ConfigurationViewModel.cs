using CHAD.DesktopApplication.Tools;
using CHAD.Model;

namespace CHAD.DesktopApplication.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        #region Constructors

        public ConfigurationViewModel(Configuration configuration)
        {
            Configuration = configuration;
        }

        public ConfigurationViewModel(ConfigurationViewModel simulationViewModel)
        {
            Configuration = new Configuration();
            Name = simulationViewModel.Name;
        }

        #endregion

        #region Public Interface

        public Configuration Configuration { get; }

        public string Name
        {
            get => Configuration.Name;
            set
            {
                if (value == Configuration.Name) return;
                Configuration.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public int SimulationsCount
        {
            get => Configuration.Parameters.NumOfSimulations;
            set
            {
                if(value == Configuration.Parameters.NumOfSimulations)
                    return;
                Configuration.Parameters.NumOfSimulations = value;
                RaisePropertyChanged(nameof(SimulationsCount));
            }
        }

        public int SeasonsCount
        {
            get => Configuration.Parameters.NumOfSeasons;
            set
            {
                if (value == Configuration.Parameters.NumOfSeasons)
                    return;
                Configuration.Parameters.NumOfSeasons = value;
                RaisePropertyChanged(nameof(SeasonsCount));
            }
        }

        public decimal MeanBushelsAlfalfaPerAcre
        {
            get => Configuration.Parameters.MeanBushelsAlfalfaPerAcre;
            set
            {
                if (value == Configuration.Parameters.MeanBushelsAlfalfaPerAcre)
                    return;
                Configuration.Parameters.MeanBushelsAlfalfaPerAcre = value;
                RaisePropertyChanged();
            }
        }

        public decimal MeanBushelsBarleyPerAcre
        {
            get => Configuration.Parameters.MeanBushelsBarleyPerAcre;
            set
            {
                if (value == Configuration.Parameters.MeanBushelsBarleyPerAcre)
                    return;
                Configuration.Parameters.MeanBushelsBarleyPerAcre = value;
                RaisePropertyChanged();
            }
        }

        public decimal MeanBushelsWheatPerAcre
        {
            get => Configuration.Parameters.MeanBushelsWheatPerAcre;
            set
            {
                if (value == Configuration.Parameters.MeanBushelsWheatPerAcre)
                    return;
                Configuration.Parameters.MeanBushelsWheatPerAcre = value;
                RaisePropertyChanged();
            }
        }

        public decimal ClimateChangePrecipMean
        {
            get => Configuration.Parameters.ClimateChangePrecipMean;
            set
            {
                if(value == Configuration.Parameters.ClimateChangePrecipMean)
                    return;
                Configuration.Parameters.ClimateChangePrecipMean = value;
                RaisePropertyChanged();
            }
        }

        public decimal ClimateChangePrecipSD 
        {
            get => Configuration.Parameters.ClimateChangePrecipSD;
            set
            {
                if(value == Configuration.Parameters.ClimateChangePrecipSD )
                    return;
                Configuration.Parameters.ClimateChangePrecipSD  = value;
                RaisePropertyChanged();
            }
        }

        public decimal ClimateChangeTempMean 
        {
            get => Configuration.Parameters.ClimateChangeTempMean ;
            set
            {
                if(value == Configuration.Parameters.ClimateChangeTempMean )
                    return;
                Configuration.Parameters.ClimateChangeTempMean  = value;
                RaisePropertyChanged();
            }
        }

        public decimal ClimateChangeTempSD
        {
            get => Configuration.Parameters.ClimateChangeTempSD;
            set
            {
                if(value == Configuration.Parameters.ClimateChangeTempSD)
                    return;
                Configuration.Parameters.ClimateChangeTempSD = value;
                RaisePropertyChanged();
            }
        }


        public decimal CostAlfalfa
        {
            get => Configuration.Parameters.CostAlfalfa;
            set
            {
                if (value == Configuration.Parameters.CostAlfalfa)
                    return;
                Configuration.Parameters.CostAlfalfa = value;
                RaisePropertyChanged();
            }
        }

        public decimal CostBarley
        {
            get => Configuration.Parameters.CostBarley;
            set
            {
                if (value == Configuration.Parameters.CostBarley)
                    return;
                Configuration.Parameters.CostBarley = value;
                RaisePropertyChanged();
            }
        }

        public decimal ProfitCRP
        {
            get => Configuration.Parameters.ProfitCRP;
            set
            {
                if (value == Configuration.Parameters.ProfitCRP)
                    return;
                Configuration.Parameters.ProfitCRP = value;
                RaisePropertyChanged();
            }
        }

        public decimal CostWheat
        {
            get => Configuration.Parameters.CostWheat;
            set
            {
                if (value == Configuration.Parameters.CostWheat)
                    return;
                Configuration.Parameters.CostWheat = value;
                RaisePropertyChanged();
            }
        }

        public decimal WaterCurtailmentBase
        {
            get => Configuration.Parameters.WaterCurtailmentBase;
            set
            {
                if (value == Configuration.Parameters.WaterCurtailmentBase)
                    return;
                Configuration.Parameters.WaterCurtailmentBase = value;
                RaisePropertyChanged();
            }
        }

        public decimal WaterCurtailmentRate
        {
            get => Configuration.Parameters.WaterCurtailmentRate;
            set
            {
                if (value == Configuration.Parameters.WaterCurtailmentRate)
                    return;
                Configuration.Parameters.WaterCurtailmentRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal LeakAquiferFrac
        {
            get => Configuration.Parameters.LeakAquiferFrac;
            set
            {
                if (value == Configuration.Parameters.LeakAquiferFrac)
                    return;
                Configuration.Parameters.LeakAquiferFrac = value;
                RaisePropertyChanged();
            }
        }

        public decimal PercFromFieldFrac
        {
            get => Configuration.Parameters.PercFromFieldFrac;
            set
            {
                if (value == Configuration.Parameters.PercFromFieldFrac)
                    return;
                Configuration.Parameters.PercFromFieldFrac = value;
                RaisePropertyChanged();
            }
        }

        public decimal WaterInAquifer
        {
            get => Configuration.Parameters.WaterInAquifer;
            set
            {
                if (value == Configuration.Parameters.WaterInAquifer) return;
                Configuration.Parameters.WaterInAquifer = value;
                RaisePropertyChanged();
            }
        }

        public decimal WaterInAquiferMax
        {
            get => Configuration.Parameters.WaterInAquiferMax;
            set
            {
                if (value == Configuration.Parameters.WaterInAquiferMax) return;
                Configuration.Parameters.WaterInAquiferMax = value;
                RaisePropertyChanged();
            }
        }

        public decimal WaterStoreCap
        {
            get => Configuration.Parameters.WaterStoreCap;
            set
            {
                if (value == Configuration.Parameters.WaterStoreCap) return;
                Configuration.Parameters.WaterStoreCap = value;
                RaisePropertyChanged();
            }
        }

        public decimal SustainableLevelAquifer
        {
            get => Configuration.Parameters.SustainableLevelAquifer;
            set
            {
                if (value == Configuration.Parameters.SustainableLevelAquifer) return;
                Configuration.Parameters.SustainableLevelAquifer = value;
                RaisePropertyChanged();
            }
        }

        public decimal MeltingPoint
        {
            get => Configuration.Parameters.MeltingPoint;
            set
            {
                if (value == Configuration.Parameters.MeltingPoint) return;
                Configuration.Parameters.MeltingPoint = value;
                RaisePropertyChanged();
            }
        }

        public decimal MeltingRate
        {
            get => Configuration.Parameters.MeltingRate;
            set
            {
                if (value == Configuration.Parameters.MeltingRate) return;
                Configuration.Parameters.MeltingRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal WaterInSnowpack
        {
            get => Configuration.Parameters.WaterInSnowpack;
            set
            {
                if (value == Configuration.Parameters.WaterInSnowpack) return;
                Configuration.Parameters.WaterInSnowpack = value;
                RaisePropertyChanged();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public void CopyTo(ConfigurationViewModel configurationViewModel)
        {
            configurationViewModel.Name = Name;

            configurationViewModel.Configuration.Parameters = Configuration.Parameters.Clone();
        }

        #endregion
    }
}