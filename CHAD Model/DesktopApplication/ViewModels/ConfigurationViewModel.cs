using System.Collections.Generic;
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

        public string SosielConfiguration
        {
            get => Configuration.Parameters.SosielConfiguration;
            set
            {
                Configuration.Parameters.SosielConfiguration = value;
                RaisePropertyChanged(nameof(SosielConfiguration));
            }
        }

        public List<string> AvailableSosielConfigurations => Configuration.AvailableSosielConfigurations;

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

        public int DaysCount
        {
            get => Configuration.Parameters.NumOfDays;
            set
            {
                if (value == Configuration.Parameters.NumOfDays)
                    return;
                Configuration.Parameters.NumOfDays = value;
                RaisePropertyChanged(nameof(DaysCount));
            }
        }

        public double MeanAlfalfa
        {
            get => Configuration.Parameters.MeanAlfalfa;
            set
            {
                Configuration.Parameters.MeanAlfalfa = value;
                RaisePropertyChanged();
            }
        }

        public double MeanBarley
        {
            get => Configuration.Parameters.MeanBarley;
            set
            {
                Configuration.Parameters.MeanBarley = value;
                RaisePropertyChanged();
            }
        }

        public double MeanWheat
        {
            get => Configuration.Parameters.MeanWheat;
            set
            {
                Configuration.Parameters.MeanWheat = value;
                RaisePropertyChanged();
            }
        }

        public double ClimateChangePrecipMean
        {
            get => Configuration.Parameters.ClimateChangePrecipMean;
            set
            {
                Configuration.Parameters.ClimateChangePrecipMean = value;
                RaisePropertyChanged();
            }
        }

        public double ClimateChangePrecipSD 
        {
            get => Configuration.Parameters.ClimateChangePrecipSD;
            set
            {
                Configuration.Parameters.ClimateChangePrecipSD  = value;
                RaisePropertyChanged();
            }
        }

        public double ClimateChangeTempMean 
        {
            get => Configuration.Parameters.ClimateChangeTempMean ;
            set
            {
                Configuration.Parameters.ClimateChangeTempMean  = value;
                RaisePropertyChanged();
            }
        }

        public double ClimateChangeTempSD
        {
            get => Configuration.Parameters.ClimateChangeTempSD;
            set
            {
                Configuration.Parameters.ClimateChangeTempSD = value;
                RaisePropertyChanged();
            }
        }


        public double CostAlfalfa
        {
            get => Configuration.Parameters.CostAlfalfa;
            set
            {
                Configuration.Parameters.CostAlfalfa = value;
                RaisePropertyChanged();
            }
        }

        public double CostBarley
        {
            get => Configuration.Parameters.CostBarley;
            set
            {
                Configuration.Parameters.CostBarley = value;
                RaisePropertyChanged();
            }
        }

        public double CostWheat
        {
            get => Configuration.Parameters.CostWheat;
            set
            {
                Configuration.Parameters.CostWheat = value;
                RaisePropertyChanged();
            }
        }

        public double WaterUseBase
        {
            get => Configuration.Parameters.WaterUseBase;
            set
            {
                Configuration.Parameters.WaterUseBase = value;
                RaisePropertyChanged();
            }
        }

        public double WaterUseRedFrac
        {
            get => Configuration.Parameters.WaterUseRedFrac;
            set
            {
                Configuration.Parameters.WaterUseRedFrac = value;
                RaisePropertyChanged();
            }
        }

        public double LeakAquiferFrac
        {
            get => Configuration.Parameters.LeakAquiferFrac;
            set
            {
                Configuration.Parameters.LeakAquiferFrac = value;
                RaisePropertyChanged();
            }
        }

        public double PercFromFieldFrac
        {
            get => Configuration.Parameters.PercFromFieldFrac;
            set
            {
                Configuration.Parameters.PercFromFieldFrac = value;
                RaisePropertyChanged();
            }
        }

        public double WaterInAquifer
        {
            get => Configuration.Parameters.WaterInAquifer;
            set
            {
                Configuration.Parameters.WaterInAquifer = value;
                RaisePropertyChanged();
            }
        }

        public double WaterInAquiferMax
        {
            get => Configuration.Parameters.WaterInAquiferMax;
            set
            {
                Configuration.Parameters.WaterInAquiferMax = value;
                RaisePropertyChanged();
            }
        }

        public double FieldDepth
        {
            get => Configuration.Parameters.FieldDepth;
            set
            {
                Configuration.Parameters.FieldDepth = value;
                RaisePropertyChanged(nameof(FieldDepth));
            }
        }
        

        public double SustainableLevelAquifer
        {
            get => Configuration.Parameters.SustainableLevelAquifer;
            set
            {
                Configuration.Parameters.SustainableLevelAquifer = value;
                RaisePropertyChanged();
            }
        }

        public double MeltingPoint
        {
            get => Configuration.Parameters.MeltingPoint;
            set
            {
                Configuration.Parameters.MeltingPoint = value;
                RaisePropertyChanged();
            }
        }

        public double SnowInSnowpack
        {
            get => Configuration.Parameters.SnowInSnowpack;
            set
            {
                Configuration.Parameters.SnowInSnowpack = value;
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