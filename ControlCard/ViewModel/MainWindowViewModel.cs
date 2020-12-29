using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using OfficeOpenXml;
using ControlCard.Utilites;
using System.Windows.Input;
using ControlCard.Commands;
using System.Windows;
using System.Collections.ObjectModel;
using ControlCard.Model;
using System.Linq;
using System.IO.Ports;
using SerialPortLib;
using System.Text;
using OfficeOpenXml.Style;
using System.Drawing;
using ControlCard.TH2830;
using System.Deployment;

namespace ControlCard.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Fieldes
        
        private bool _radioButtonIsChecked;
        private string _nameMainForm;
        private string _history;

        private SerialPortInput _serialPort;
        private string _portSate;
        private string _lcrState;

        private ObservableCollection<ComponentModel> _components;
        private ComponentModel _selectedComponent;

        private ObservableCollection<InfoModel> _info;
        private ObservableCollection<SampleModel> _samples;
        private int _selectedSample;

        private ObservableCollection<string> _comPortsAvailable;
        private object _selectedPort;

        private ObservableCollection<UnitModel> _unitsComboBox;
        private object _selectedUnit;
        private CMD _curCMD;
        private enum LCR
        {
            L,
            C,
            R
        };
        private LCR _curFunc;
        private string _appVersion;
        private object _dropDownOpened;
        #endregion

        public MainWindowViewModel()
        {
            //_selectedSample = 0;            
            RadioButtonIsChecked = false;
            NameMainForm = "LCR";
            PortState = "Closed";
            LcrState = "Disconnected";
            
            AppVersion = string.Format("v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4)); ;

            //Scan COM ports
            ScanCOMPorts();

            _serialPort = new SerialPortInput();
            _serialPort.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
            _serialPort.MessageReceived += SerialPort_MessageReceived;

            //Collection of components
            Components = new ObservableCollection<ComponentModel>();
            OpenCheckListExcel(@"C:\List.xlsx");

            if (Components.Count>0)
            {
                SelectedComponent = Components[0];
            }

            //Collection of info
            Info = new ObservableCollection<InfoModel>();
            Info.CollectionChanged += Info_CollectionChanged;
            Info.Add(new InfoModel { });

            Units = new ObservableCollection<UnitModel>();

            //Collection of samples
            Samples = new ObservableCollection<SampleModel>();
            Samples.CollectionChanged += Samples_CollectionChanged;
            CreateItemsInSamples(10);

            //Commands
            DropDownOpenedCommand = new RelayCommand(DropDownOpened);
            SelectComponentCommand = new RelayCommand(SelectComponentExecute);
            SaveAndNewCommand = new RelayCommand(SaveAndNewExecute);
            ClosingMainWindowCommand = new RelayCommand(ClosingMainWindowExecute);
            ClearFromSamplesCommand = new RelayCommand(ClearFromSamplesExecute);
            ConnectBtnCommand = new RelayCommand(ConnectBtnExecute);

            FETCCommand = new RelayCommand(FETCExecute);
        }
            

        #region Events
        private void Info_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            GetMeasurementsCommand = new RelayCommand(FETCExecute);
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (INotifyPropertyChanged item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += Info_PropertyChanged;
                }
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    foreach (INotifyPropertyChanged item in e.OldItems.OfType<INotifyPropertyChanged>())
                    {
                        item.PropertyChanged -= Info_PropertyChanged;
                    }
                }
            }
        }
        private void Info_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string property = e.PropertyName;
            if (property == "ValueLabel" || property == "ToleranceLabel") GetResults((InfoModel)sender,null);
            if (property == "Unit")
            {
                InfoModel info = (InfoModel)sender;
                UnitModel unit = (UnitModel)info.Unit;
                if (unit == null) return;

                if (_curFunc == LCR.L)
                {                    
                    if (unit.Exp == -9) SetFreq(FREQ.f2000Hz);
                    if (unit.Exp == -6) SetFreq(FREQ.f6000Hz);                    
                }
                if (_curFunc == LCR.C)
                {
                    if (unit.Exp == -12) SetFreq(FREQ.f100000Hz);
                    if (unit.Exp == -9) SetFreq(FREQ.f500Hz);
                    if (unit.Exp == -6) SetFreq(FREQ.f500Hz);
                    if (unit.Exp == -3) SetFreq(FREQ.f500Hz);
                }
                GetResults((InfoModel)sender, null);
            }
        }
        private void Samples_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (INotifyPropertyChanged item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += Sample_PropertyChanged;
                }
                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    foreach (INotifyPropertyChanged item in e.OldItems.OfType<INotifyPropertyChanged>())
                    {
                        item.PropertyChanged -= Sample_PropertyChanged;
                    }
                }
            }
        }

        private void Component_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string property = e.PropertyName;
            if (property == "Value1")
            {
                var sample = (SampleModel)sender;
                GetResults(null, sample);
            }
        }
        private void Sample_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //string property = e.PropertyName;
            //if (property == "Value1")
            //{
            //    var sample = (SampleModel)sender;
            //    GetResults(null, sample);
            //}

        }

        #endregion

        #region Commands
        //Drop down combobox list
        public ICommand DropDownOpenedCommand { get; set; }
        public void DropDownOpened(object obj)
        {
            ScanCOMPorts();
        }
        //Connect button
        public ICommand ConnectBtnCommand { get; private set; }
        private void ConnectBtnExecute(object obj)
        {
            ConnectionToLCR(SelectedPort);
        }
        //Select current component
        public ICommand SelectComponentCommand { get; private set; }
        private void SelectComponentExecute(object obj)
        {
            var btn = (String)obj;
            switch (btn)
            {
                case "R": //Resistor
                    NameMainForm = "Resistance";
                    _curFunc = LCR.R;
                    if (LcrState == "Connected")
                    {
                        SetFunction(FUNC.RSQ);
                        SetFreq(FREQ.f500Hz);
                        SetVoltage(200);
                    }
                    Units.Clear();
                    Units.Add(new UnitModel() { Unit = "Ohm",  Exp = 0 });
                    Units.Add(new UnitModel() { Unit = "kOhm", Exp = 3 });
                    Units.Add(new UnitModel() { Unit = "MOhm", Exp = 6 });
                    break;
                case "C": //Capacitor
                    NameMainForm = "Capacitance";
                    _curFunc = LCR.C;
                    if (LcrState == "Connected")
                    {
                        SetFunction(FUNC.CPD);
                        SetFreq(FREQ.f100000Hz);
                        SetVoltage(1000);
                    }
                    Units.Clear();
                    Units.Add(new UnitModel() { Unit = "pF", Exp = -12 });
                    Units.Add(new UnitModel() { Unit = "nF", Exp = -9 });
                    Units.Add(new UnitModel() { Unit = "uF", Exp = -6 });
                    Units.Add(new UnitModel() { Unit = "mF", Exp = -3 });

                    break;
                case "L": //Inductance
                    NameMainForm = "Inductance";
                    _curFunc = LCR.L;
                    if (LcrState == "Connected")
                    {
                        SetFunction(FUNC.LSD);
                        SetFreq(FREQ.f5000Hz);
                        SetAmp(200);
                    }
                    Units.Clear();
                    Units.Add(new UnitModel() { Unit = "nH", Exp = -9 });
                    Units.Add(new UnitModel() { Unit = "uH", Exp = -6 });
                    Units.Add(new UnitModel() { Unit = "mH", Exp = -3 });

                    break;

                default:
                    break;
            }
        }
        //Fetch
        public ICommand FETCCommand { get; private set; }
        private void FETCExecute(object obj)
        {
            if (LcrState == "Connected")
            {
                GetDataFromDevice();
            }
            else
            {
                MessageBox.Show("LCR meter is not connected!", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }

        }
        //Clear
        public ICommand ClearFromSamplesCommand { get; private set; }
        private void ClearFromSamplesExecute(object obj)
        {
            if (_selectedSample > 0)
            {
                _selectedSample--;

                Samples[_selectedSample].Value1 = 0;
                Samples[_selectedSample].Value2 = 0;
                Samples[_selectedSample].Deviation = 0;
                Samples[_selectedSample].Result = String.Empty;
            }

        }
        //Button
        public ICommand GetMeasurementsCommand { get; private set; }
        //Button OK
        public ICommand SaveAndNewCommand { get; private set; }

        private void SaveAndNewExecute(object obj)
        {
            //Check all fieldes 
            if (String.IsNullOrEmpty(Info[0].Name))
            {
                MessageBox.Show("Name of component is empty 组件名称为空!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (Info[0].ValueLabel == 0)
            {
                MessageBox.Show("Value did not filled 价值没有填补!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (Info[0].Unit == null)
            {
                MessageBox.Show("Select the unit 选择单位!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (Info[0].ToleranceLabel == 0)
            {
                MessageBox.Show("Tolerance did not filled 宽容没有填补!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (String.IsNullOrEmpty(Info[0].Manufucturer))
            {
                MessageBox.Show("Manufacturer is empty 制造商是空的!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (Info[0].ProdDate == 0)
            {
                MessageBox.Show("Production date did not filled 生产日期没有填写!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (String.IsNullOrEmpty(Info[0].Lot))
            {
                MessageBox.Show("Lot is empty 地段是空的!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (String.IsNullOrEmpty(Info[0].Supplier))
            {
                MessageBox.Show("Supplier is empty 供应商是空的!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (Info[0].PurchDate == 0)
            {
                MessageBox.Show("Purchase date did not filled!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (Info[0].Po == 0)
            {
                MessageBox.Show("PO没有填补!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }          

            //Check Samples
            foreach (var item in Samples)
            {
                if (item.Value1 == 0)
                {
                    MessageBox.Show("Values did not filled 价值观没有填补!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

            }

            using (ExcelPackage excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("Worksheet1");
                var u = (UnitModel)Info[0].Unit;

                //Info
                //List<object[]> cellInfo = new List<object[]>()
                //{
                //  new object[] {FormTextName,Name},
                //  new object[] {FormTextValueLabel, Info[0].ValueLabel, u.Unit},
                //  new object[] {"Tolerance",SelectedComponent.ToleranceLabel},
                //  new object[] {FormTextManufactory, Info[0].Manufucturer },
                //  new object[] {FormTextProdDate, Info[0].ProdDate },
                //  new object[] {FormTextLotNumber, Info[0].Lot },
                //  new object[] {FormTextSupplier, Info[0].Supplier },
                //  new object[] {FormTextDateOfPurch, Info[0].PurchDate },
                //  new object[] {FormTexNumberPO, Info[0].Po }
                // };

                //Fillout the excel with Info cells
                //worksheet.Cells[1, 1].LoadFromArrays(cellInfo);

                //Samples
                List<object[]> cellSamples = new List<object[]>();
                //topper
                cellSamples.Add(new object[] { "#", "Value1", "Value2", "Deviation", "Result" });
                for (int i = 0; i < Samples.Count; i++)
                {
                    cellSamples.Add(new object[] { Samples[i].Number, Samples[i].Value1, Samples[i].Value2, Samples[i].Deviation, Samples[i].Result });
                }

                //Fillout the excel with samples cells
                worksheet.Cells[1, 5].LoadFromArrays(cellSamples);


                //Border
                foreach (var item in worksheet.Cells)
                {
                    item.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    item.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    item.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    item.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                }
                worksheet.Column(1).Width = 26;
                for (int i = 2; i < 11; i++)
                {
                    worksheet.Column(i).AutoFit();
                    worksheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                
                //Color cells
                for (int i = 5; i < 10; i++)
                {
                    worksheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                }
                

                // Location
                string foldersPath = @"C:\Control cards\";
                string folderName = Info[0].Po.ToString();
                string fileName = folderName + "-" + Info[0].Name + "-" + DateTime.Now.ToShortDateString() + ".xlsx";

                //need try catch here
                if (!Directory.Exists(foldersPath + folderName))
                {
                    DirectoryInfo di = Directory.CreateDirectory(foldersPath + folderName);
                }

                //Save all
                string fullPath = foldersPath + folderName + @"\" + fileName;
                FileInfo excelFile = new FileInfo(fullPath);
                try
                {
                    excel.SaveAs(excelFile);
                }
                catch
                {
                    MessageBox.Show("File opened", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                finally
                {
                    //Saved ok
                    //Clear values
                    Info[0].Name = String.Empty;
                    Info[0].ValueLabel = 0;
                    Info[0].ToleranceLabel = 0;
                    Samples.Clear();
                    CreateItemsInSamples(10);
                    AddHistory("Saved to: " + fullPath);
                    _selectedSample = 0;
                }
            }
        }        
        //Closing window
        public ICommand ClosingMainWindowCommand { get; private set; }
        private void ClosingMainWindowExecute(object obj)
        {
            _serialPort.Disconnect();
        }
        #endregion

        #region Properties

        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                _appVersion = value;
                OnPropertyChanged("AppVersion");
            }
        }

        public bool RadioButtonIsChecked
        {
            get { return _radioButtonIsChecked; }
            set
            {
                _radioButtonIsChecked = value;
                OnPropertyChanged("RadioButtonIsChecked");
            }
        }

        public ObservableCollection<string> ComPortsAvailable
        {
            get { return _comPortsAvailable; }
            set
            {
                _comPortsAvailable = value;
                OnPropertyChanged("ComPortsAvailable");
            }
        }

        public object SelectedPort
        {
            get { return _selectedPort; }
            set
            {
                _selectedPort = value;
                OnPropertyChanged("SelectedPort");
                //ConnectionToLCR(value);
            }
        }

        public ObservableCollection<UnitModel> Units
        {
            get { return _unitsComboBox; }
            set
            {
                _unitsComboBox = value;
                OnPropertyChanged("Units");
            }
        }
        public object SelectedUnit
        {
            get { return _selectedUnit; }

            set
            {
                _selectedUnit = value;
                OnPropertyChanged("SelectedUnit");
                Info[0].Unit = value;
            }
        }
        
        public string PortState
        {
            get { return _portSate; }
            set
            {
                _portSate = value;
                OnPropertyChanged("PortState");
            }
        }
        public string LcrState
        {
            get { return _lcrState; }
            set
            {
                _lcrState = value;
                OnPropertyChanged("LcrState");
            }
        }
        public string History
        {
            get { return _history; }
            set
            {
                _history = value;
                OnPropertyChanged("History");
            }
        }

        public ObservableCollection<ComponentModel> Components
        {
            get => _components;
            set
            {
                _components = value;
                OnPropertyChanged("Components");
            }
        }
        public ComponentModel SelectedComponent
        {
            get => _selectedComponent;
            set
            {
                _selectedComponent = value;
                OnPropertyChanged("SelectedComponent");
            }
        }

        public ObservableCollection<InfoModel> Info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged("Info");
            }
        }
        public ObservableCollection<SampleModel> Samples
        {
            get{ return _samples; }
            set
            {
                _samples = value;
                OnPropertyChanged("Samples");
            }
        }
        
        public string NameMainForm
        {
            get { return _nameMainForm; }
            set
            {
                _nameMainForm = value;
                OnPropertyChanged("NameMainForm");
            }
        }

        #endregion

        #region Methods COM port and LCR
        private void ScanCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            ComPortsAvailable = new ObservableCollection<string>(ports);
            if (!ComPortsAvailable.Any(p => p == PortState))
            {
                PortState = "Closed";
                LcrState = "Disconnected";
            }
        }
        private void ConnectionToLCR(object selectedPort)
        {
            if (LcrState == "Disconnected")
            {
                string spName = (String)selectedPort;
                _serialPort.SetPort(spName, 115200);
                try
                {
                    if (!_serialPort.IsConnected) _serialPort.Connect();
                }
                catch (Exception ex)
                {
                    AddHistory(ex.Message);
                    return;
                }
                finally
                {
                    GetDeviceID();
                }
            }
            
        }
        private void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {            
            if (args.Connected)
            {
                AddHistory("Open port " + (String)SelectedPort);
                PortState = (String)SelectedPort;
            }
            else
            {                
                PortState = "Closed";
            }
        }
        private void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            var str = Encoding.Default.GetString(args.Data);
            
            //Answers from LCR
            switch (_curCMD)
            {
                case CMD.IDN:
                    if (str == "Tonghui,TH2830,Ver 1.0.0  , Hardware Ver A3.0\n")
                    {
                        LcrState = "Connected";
                        AddHistory(str);                        
                    }
                    break;

                case CMD.FETC:
                    WriteToSamples(str);
                    break;

                default:
                    break;
            }
        }       
        private void GetDeviceID()
        {
            _curCMD = CMD.IDN;
            byte[] data = new byte[1];
            data = Encoding.ASCII.GetBytes("*idn?\r\n");
            _serialPort.SendMessage(data);
        }
        private void GetDataFromDevice()
        {
            _curCMD = CMD.FETC;
            byte[] data = new byte[1];
            data = Encoding.ASCII.GetBytes("FETC?\r\n");
            _serialPort.SendMessage(data);
        }
        private void SetFunction(FUNC f)
        {
            byte[] data = new byte[1];

            switch (f)
            {
                case FUNC.CPD:
                    data = Encoding.ASCII.GetBytes("FUNCtion:IMPedance CPD\r\n");
                    break;
                case FUNC.CSRS:
                    data = Encoding.ASCII.GetBytes("FUNCtion:IMPedance CSRS\r\n");
                    break;
                case FUNC.RSQ:
                    data = Encoding.ASCII.GetBytes("FUNCtion:IMPedance RSQ\r\n");
                    break;
                case FUNC.LSD:
                    data = Encoding.ASCII.GetBytes("FUNCtion:IMPedance LSD\r\n");
                    break;
                default:
                    break;
            }

            _serialPort.SendMessage(data);
        }
        private void SetFreq(FREQ freq)
        {
            _curCMD = 0;
            
            string f = "f";
            f = freq.ToString().TrimStart(f[0]);
            
            byte[] data = new byte[1];
            data = Encoding.ASCII.GetBytes("FREQ " + f.ToString() + "\r\n");
            _serialPort.SendMessage(data);
        }
        private void SetVoltage(int mV)
        {
            if (mV > 0 && mV < 2001)
            {
                _curCMD = 0;
                //check
                int v = (mV/10)*10;

                byte[] data = new byte[1];
                data = Encoding.ASCII.GetBytes("VOLT " + v.ToString() + "mV\r\n");
                _serialPort.SendMessage(data);
            }
        }
        private void SetAmp(int uA)
        {
            if (uA > 0 && uA < 20001)
            {
                _curCMD = 0;
                //check
                int a = (uA / 10) * 10;

                byte[] data = new byte[1];
                data = Encoding.ASCII.GetBytes("CURR " + a.ToString() + "uA\r\n");
                _serialPort.SendMessage(data);
            }
        }
        #endregion

        #region Methods main window

        private void OpenCheckListExcel(string filePath)
        {
            if (!File.Exists(filePath)) return;
            FileInfo existingFile = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                //get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                var rowCount = worksheet.Dimension.End.Row;     //get row count

                for (var i = 0; i < rowCount - 1; i++)
                {
                    var component = new ComponentModel()
                    {
                        Number = int.Parse(worksheet.Cells[2 + i, 1].Value.ToString().Trim()),
                        Article = worksheet.Cells[2 + i, 2].Value.ToString().Trim(),
                        Nomenclature = worksheet.Cells[2 + i, 3].Value.ToString().Trim()
                    };
                    
                    for (int j = 1; j < 11; j++)
                    {
                        var sample = new SampleModel { Number = j };
                        sample.PropertyChanged += Sample_PropertyChanged;
                        component.Samples.Add(sample);
                    }
                    
                    component.PropertyChanged += Component_PropertyChanged;
                    Components.Add(component);
                }
            }

        }

        private void CreateItemsInSamples(int num)
        {
            if(Components != null) return;
            
            for (int i = 1; i < num + 1; i++)
            {
                var sample = new SampleModel { Number = i };
            }
        }


        private void GetResults(InfoModel info, SampleModel sample)
        {
            double var1 = 0;
            double var2 = 0;
            double var3 = 0;

            try
            {
                if (info == null && sample != null)
                {
                    UnitModel um = (UnitModel)Info[0].Unit;
                    if (um == null) return;

                    var exp = Math.Pow(10, um.Exp);

                    var1 = Info[0].ValueLabel * exp;
                    var3 = sample.Value1 / var1;
                    var2 = (1 - var3) * 100;


                    sample.Deviation = Math.Round(Math.Abs((1 - var3) * 100), 2);
                    sample.Result = sample.Deviation < Info[0].ToleranceLabel ? "OK" : "NG";
                }
                else if (info != null && sample == null)
                {
                    UnitModel um = (UnitModel)info.Unit;
                    if (um == null) return;
                    var exp = Math.Pow(10, um.Exp);

                    foreach (var item in Samples)
                    {
                        if (info.ValueLabel == 0 || item.Value1 == 0) return;

                        var1 = info.ValueLabel * exp;
                        var3 = item.Value1 / var1;
                        var2 = (1 - var3) * 100;

                        item.Deviation = Math.Round(Math.Abs((1 - var3) * 100), 2);
                        item.Result = item.Deviation < info.ToleranceLabel ? "OK" : "NG";
                    }
                }
            }
            catch (Exception ex)
            {

                AddHistory("Error to get results.");
            }
        }
        
        private void AddHistory(string text)
        {
            History += DateTime.Now.ToShortTimeString() + " : " + text + "\n";
        }
        private void WriteToSamples(string str)
        {
            if (_selectedSample < Samples.Count)
            {
                string[] str_values = str.Split(',');
                try
                {
                    Samples[_selectedSample].Value1 = Double.Parse(str_values[0], System.Globalization.CultureInfo.InvariantCulture);
                    Samples[_selectedSample].Value2 = Double.Parse(str_values[1], System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    AddHistory(ex.Message);
                    return;
                }
                _selectedSample++;
            }
        }
        #endregion

    }
}
