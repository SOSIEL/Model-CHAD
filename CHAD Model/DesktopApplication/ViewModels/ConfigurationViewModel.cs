using DesktopApplication.Tools;
using Model;

namespace DesktopApplication.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        #region Fields

        private bool _isConfigured;

        #endregion

        #region Constructors

        public ConfigurationViewModel(Configuration configuration)
        {
            Configuration = configuration;
        }

        public ConfigurationViewModel(ConfigurationViewModel simulationViewModel)
        {
            Configuration = new Configuration();
            Name = simulationViewModel.Name;
            IsConfigured = _isConfigured;
        }

        #endregion

        #region Public Interface

        public Configuration Configuration { get; private set; }

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
            get => Configuration.Name;
            set
            {
                if (value == Configuration.Name) return;
                Configuration.Name = value;
                OnPropertyChanged(nameof(Name));
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public decimal WaterUsageMax
        {
            get => Configuration.Parameters.WaterUsageMax;
            set
            {
                if (value == Configuration.Parameters.WaterUsageMax)
                    return;
                Configuration.Parameters.WaterUsageMax = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public decimal WaterInAquifer
        {
            get => Configuration.Parameters.WaterInAquifer;
            set
            {
                if (value == Configuration.Parameters.WaterInAquifer) return;
                Configuration.Parameters.WaterInAquifer = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterInAquiferMax
        {
            get => Configuration.Parameters.WaterInAquiferMax;
            set
            {
                if (value == Configuration.Parameters.WaterInAquiferMax) return;
                Configuration.Parameters.WaterInAquiferMax = value;
                OnPropertyChanged();
            }
        }

        public decimal WaterStorCap
        {
            get => Configuration.Parameters.WaterStorCap;
            set
            {
                if (value == Configuration.Parameters.WaterStorCap) return;
                Configuration.Parameters.WaterStorCap = value;
                OnPropertyChanged();
            }
        }

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