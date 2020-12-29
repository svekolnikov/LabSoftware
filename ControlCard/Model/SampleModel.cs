using ControlCard.Utilites;

namespace ControlCard.Model
{
    class SampleModel : ViewModelBase
    {
        private int _number;
        private double _value1;
        private double _value2;
        private string _unit;
        private double _deviation;
        private string _result;
        
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged("Number");
            }
        }
        public double Value1
        {
            get { return _value1; }
            set
            {
                _value1 = value;
                OnPropertyChanged("Value1");
            } 
                
        }        
        public double Value2
        {
            get { return _value2; }
            set
            {
                _value2 = value;
                OnPropertyChanged("Value2");
            }

        }
        public string Unit
        {
            get { return _unit; }
            set
            {
                _unit = value;
                OnPropertyChanged("Unit");
            }
        }
        public double Deviation
        {
            get { return _deviation; }
            set
            {
                _deviation = value;
                OnPropertyChanged("Deviation");
            }

        }
        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }

        }
    }
}
