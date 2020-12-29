using System.Collections.ObjectModel;
using ControlCard.Utilites;

namespace ControlCard.Model
{
    class ComponentModel : ViewModelBase
    {
        #region Properties

        private int _number;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged("Number");
            }
        }

        //Information
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private double _valueLabel;
        public double ValueLabel
        {
            get => _valueLabel;
            set
            {
                _valueLabel = value;
                OnPropertyChanged("ValueLabel");
            }
        }

        private object _unit;
        public object Unit
        {
            get { return _unit; }
            set
            {
                _unit = value;
                OnPropertyChanged("Unit");
            }
        }

        private int _tolerance;
        public int ToleranceLabel
        {
            get { return _tolerance; }
            set
            {
                _tolerance = value;
                OnPropertyChanged("ToleranceLabel");
            }
        }

        private string _manufucturer = string.Empty;
        public string Manufucturer
        {
            get { return _manufucturer; }
            set
            {
                _manufucturer = value;
                OnPropertyChanged("Manufucturer");
            }
        }

        private string _prodDate = string.Empty;
        public string ProdDate
        {
            get { return _prodDate; }
            set
            {
                _prodDate = value;
                OnPropertyChanged("ProdDate");
            }
        }

        private string _lot = string.Empty;
        public string Lot
        {
            get { return _lot; }
            set
            {
                _lot = value;
                OnPropertyChanged("Lot");
            }
        }

        private string _supplier = string.Empty;
        public string Supplier
        {
            get { return _supplier; }
            set
            {
                _supplier = value;
                OnPropertyChanged("Supplier");
            }
        }

        private string _purchDate = string.Empty;
        public string PurchDate
        {
            get { return _purchDate; }
            set
            {
                _purchDate = value;
                OnPropertyChanged("PurchDate");
            }
        }

        private int _po = 0;
        public int Po
        {
            get { return _po; }
            set
            {
                _po = value;
                OnPropertyChanged("Po");
            }
        }

        //1C
        private string _article;
        public string Article
        {
            get => _article;
            set
            {
                _article = value;
                OnPropertyChanged("Article");
            }
        }

        private string _nomenclature;
        public string Nomenclature
        {
            get => _nomenclature;
            set
            {
                _nomenclature = value;
                OnPropertyChanged("Nomenclature");
            }
        }

        //Samples
        private ObservableCollection<SampleModel> _samples;
        public ObservableCollection<SampleModel> Samples
        {
            get => _samples;
            set
            {
                _samples = value;
                OnPropertyChanged("Samples");
            }
        }

        #endregion

        public ComponentModel()
        {
            Samples = new ObservableCollection<SampleModel>();
        }
    }
}
