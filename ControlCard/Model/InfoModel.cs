using ControlCard.Utilites;

namespace ControlCard.Model
{
    class InfoModel : ViewModelBase
    {
        private string _name = string.Empty;
        private double _valueLabel = 0;
        private object _unit;
        private int _tolerance = 0;
        private string _manufucturer = string.Empty;
        private int _prodDate = 0;
        private string _lot = string.Empty;
        private string _supplier = string.Empty;
        private int _purchDate = 0;
        private int _po = 0;

        public string Name
        {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public double ValueLabel
        {
            get { return _valueLabel; }
            set {
                _valueLabel = value;
                OnPropertyChanged("ValueLabel");
            }
        }
        public object Unit
        {
            get { return _unit; }
            set
            {
                _unit = value;
                OnPropertyChanged("Unit");
            }
        }
        public int ToleranceLabel
        {
            get { return _tolerance; }
            set
            {
                _tolerance = value;
                OnPropertyChanged("ToleranceLabel");
            }
        }
        public string Manufucturer
        {
            get { return _manufucturer; }
            set {
                _manufucturer = value;
                OnPropertyChanged("Manufucturer");
            }
        }
        public int ProdDate
        {
            get { return _prodDate; }
            set
            {
                _prodDate = value;
                OnPropertyChanged("ProdDate");
            }
        }
        public string Lot
        {
            get { return _lot; }
            set
            {
                _lot = value;
                OnPropertyChanged("Lot");
            }
        }
        public string Supplier
        {
            get { return _supplier; }
            set
            {
                _supplier = value;
                OnPropertyChanged("Supplier");
            }
        }
        public int PurchDate
        {
            get { return _purchDate; }
            set
            {
                _purchDate = value;
                OnPropertyChanged("PurchDate");
            }
        }
        public int Po
        {
            get { return _po; }
            set
            {
                _po = value;
                OnPropertyChanged("Po");
            }
        }
    }
}
