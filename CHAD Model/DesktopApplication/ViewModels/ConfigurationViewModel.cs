using DesktopApplication.Models;
using DesktopApplication.Tools;

namespace DesktopApplication.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        #region Fields

        private readonly Configuration _configuration;
        private bool _isConfigured;

        #endregion

        #region Constructors

        public ConfigurationViewModel(Configuration configuration)
        {
            _configuration = configuration;
        }

        public ConfigurationViewModel(ConfigurationViewModel simulationViewModel)
        {
            _configuration = new Configuration();
            Name = simulationViewModel.Name;
            IsConfigured = _isConfigured;
        }

        #endregion

        #region Properties, Indexers

        public bool IsConfigured
        {
            get => _isConfigured;
            private set
            {
                _isConfigured = value;
                OnPropertyChanged(nameof(IsConfigured));
            }
        }

        public string Name
        {
            get => _configuration.Name;
            set
            {
                if (value == _configuration.Name) return;
                _configuration.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public decimal MeanBushelsAlfalfaPerAcre
        {
            get => _configuration.Parameters.MeanBushelsAlfalfaPerAcre;
            set
            {
                if (value == _configuration.Parameters.MeanBushelsAlfalfaPerAcre)
                    return;
                _configuration.Parameters.MeanBushelsAlfalfaPerAcre = value;
                OnPropertyChanged();
            }
        }

        public decimal MeanBushelsBarleyPerAcre
        {
            get => _configuration.Parameters.MeanBushelsBarleyPerAcre;
            set
            {
                if (value == _configuration.Parameters.MeanBushelsBarleyPerAcre)
                    return;
                _configuration.Parameters.MeanBushelsBarleyPerAcre = value;
                OnPropertyChanged();
            }
        }

        public decimal MeanBushelsWheatPerAcre
        {
            get => _configuration.Parameters.MeanBushelsWheatPerAcre;
            set
            {
                if (value == _configuration.Parameters.MeanBushelsWheatPerAcre)
                    return;
                _configuration.Parameters.MeanBushelsWheatPerAcre = value;
                OnPropertyChanged();
            }
        }

        public decimal CostAlfalfa
        {
            get => _configuration.Parameters.CostAlfalfa;
            set
            {
                if (value == _configuration.Parameters.CostAlfalfa)
                    return;
                _configuration.Parameters.CostAlfalfa = value;
                OnPropertyChanged();
            }
        }

        public decimal CostBarley
        {
            get => _configuration.Parameters.CostBarley;
            set
            {
                if (value == _configuration.Parameters.CostBarley)
                    return;
                _configuration.Parameters.CostBarley = value;
                OnPropertyChanged();
            }
        }

        public decimal CostWheat
        {
            get => _configuration.Parameters.CostWheat;
            set
            {
                if (value == _configuration.Parameters.CostWheat)
                    return;
                _configuration.Parameters.CostWheat = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterCurtailmentBase
        {
            get => _configuration.Parameters.WaterCurtailmentBase;
            set
            {
                if (value == _configuration.Parameters.WaterCurtailmentBase)
                    return;
                _configuration.Parameters.WaterCurtailmentBase = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterCurtailmentRate
        {
            get => _configuration.Parameters.WaterCurtailmentRate;
            set
            {
                if (value == _configuration.Parameters.WaterCurtailmentRate)
                    return;
                _configuration.Parameters.WaterCurtailmentRate = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterUsageMax
        {
            get => _configuration.Parameters.WaterUsageMax;
            set
            {
                if (value == _configuration.Parameters.WaterUsageMax)
                    return;
                _configuration.Parameters.WaterUsageMax = value;
                OnPropertyChanged();
            }
        }

        public decimal LeakAquiferFrac
        {
            get => _configuration.Parameters.LeakAquiferFrac;
            set
            {
                if (value == _configuration.Parameters.LeakAquiferFrac)
                    return;
                _configuration.Parameters.LeakAquiferFrac = value;
                OnPropertyChanged();
            }
        }

        public decimal PercFromFieldFrac
        {
            get => _configuration.Parameters.PercFromFieldFrac;
            set
            {
                if (value == _configuration.Parameters.PercFromFieldFrac)
                    return;
                _configuration.Parameters.PercFromFieldFrac = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterInAquifer
        {
            get => _configuration.Parameters.WaterInAquifer;
            set
            {
                if (value == _configuration.Parameters.WaterInAquifer) return;
                _configuration.Parameters.WaterInAquifer = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterInAquiferMax
        {
            get => _configuration.Parameters.WaterInAquiferMax;
            set
            {
                if (value == _configuration.Parameters.WaterInAquiferMax) return;
                _configuration.Parameters.WaterInAquiferMax = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterStorCap
        {
            get => _configuration.Parameters.WaterStorCap;
            set
            {
                if (value == _configuration.Parameters.WaterStorCap) return;
                _configuration.Parameters.WaterStorCap = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region All other members

        public void Configure()
        {
            IsConfigured = true;
        }

        public override string ToString()
        {
            return Name;
        }

        public void CopyTo(ConfigurationViewModel configurationViewModel)
        {
            configurationViewModel.Name = Name;

            configurationViewModel.MeanBushelsAlfalfaPerAcre = MeanBushelsAlfalfaPerAcre;
            configurationViewModel.MeanBushelsBarleyPerAcre = MeanBushelsBarleyPerAcre;
            configurationViewModel.MeanBushelsWheatPerAcre = MeanBushelsWheatPerAcre;

            configurationViewModel.CostAlfalfa = CostAlfalfa;
            configurationViewModel.CostBarley = CostBarley;
            configurationViewModel.CostWheat = CostWheat;
            configurationViewModel.WaterCurtailmentBase = WaterCurtailmentBase;
            configurationViewModel.WaterCurtailmentRate = WaterCurtailmentRate;
            configurationViewModel.WaterUsageMax = WaterUsageMax;

            configurationViewModel.LeakAquiferFrac = LeakAquiferFrac;
            configurationViewModel.PercFromFieldFrac = PercFromFieldFrac;
            configurationViewModel.WaterInAquifer = WaterInAquifer;
            configurationViewModel.WaterInAquiferMax = WaterInAquiferMax;
            configurationViewModel.WaterStorCap = WaterStorCap;
        }

        #endregion
    }
}