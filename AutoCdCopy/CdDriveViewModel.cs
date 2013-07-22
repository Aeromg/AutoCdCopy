using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using AutoCdCopy.Logics;

namespace AutoCdCopy
{
    /*
    public class CdDriveViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //string _letter;
        //string _name;
        //string _comment;
        //bool _state;
        long _size;
        string _label;
        string _serial;
        bool _attention;
        int _percentOfCopy;
        ObservableCollection<string> _errorFileNames;
        bool _isInspected;
        CdDrive _drive;
        CdInfo _cdInfo;

        public CdInfo MediaInfo
        {
            get
            {
                return _cdInfo;
            }
            set
            {
                if (_cdInfo.Equals(value))
                    return;

                _cdInfo = value;
                OnPropertyChanged("MediaInfo");
                OnPropertyChanged("Letter");
                OnPropertyChanged("Name");
                OnPropertyChanged("Comment");
                OnPropertyChanged("StateString");
                OnPropertyChanged("State");
                OnPropertyChanged("SizeString");
                OnPropertyChanged("Size");
                OnPropertyChanged("Label");
                OnPropertyChanged("Serial");
                OnPropertyChanged("PercentOfCopy");
                OnPropertyChanged("AutoDescription");
                OnPropertyChanged("ErrorFileNames");
                OnPropertyChanged("ErrorVisibility");
                OnPropertyChanged("ErrorCount");
                OnPropertyChanged("IsInspected");
                OnPropertyChanged("DetailsVisibility");
                OnPropertyChanged("LetterFontWeight");
                OnPropertyChanged("IsAttention");
            }
        }

        public string Letter
        {
            get
            {
                return _cdInfo.Letter;
            }
        }
        public string Name
        {
            get
            {
                return _cdInfo.UserDefinedName;
            }
            set
            {
                if (_cdInfo.UserDefinedName == value)
                    return;

                _cdInfo.UserDefinedName = value;
                OnPropertyChanged("Name");
            }
        }
        public string Comment
        {
            get
            {
                return _cdInfo.UserDefinedComment;
            }
            set
            {
                if (_cdInfo.UserDefinedComment == value)
                    return;

                _cdInfo.UserDefinedComment = value;
                OnPropertyChanged("Comment");
            }
        }
        public string StateString
        {
            get
            {
                return State ? "Готов" : "Не готов";
            }
        }
        public bool State
        {
            get
            {
                return _drive.IsReady;
            }
        }
        public string SizeString
        {
            get
            {
                return String.Format("{0:0.00}MB", ((double)Size) / 1024 / 1024);
            }
        }
        public long Size
        {
            get
            {
                return _cdInfo.Size;
            }
        }
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                if (_label == value)
                    return;

                _label = value;
                OnPropertyChanged("Label");
            }
        }
        public string Serial
        {
            get
            {
                return _serial;
            }
            set
            {
                if (_serial == value)
                    return;

                _serial = value;
                OnPropertyChanged("Serial");
            }
        }
        public int PercentOfCopy
        {
            get
            {
                return _percentOfCopy;
            }
            set
            {
                if (_percentOfCopy == value)
                    return;

                _percentOfCopy = value;
                OnPropertyChanged("PercentOfCopy");
            }
        }
        public string AutoDescription
        {
            get
            {
                return State ?
                    String.Format("{0} ({1})", Letter, Label) :
                    String.Format("{0} (не готов)", Letter);
            }
        }
        public ObservableCollection<string> ErrorFileNames
        {
            get
            {
                return _errorFileNames;
            }
        }
        public Visibility ErrorVisibility
        {
            get
            {
                return ErrorCount > 0 ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }
        public int ErrorCount
        {
            get
            {
                return ErrorFileNames.Count;
            }
        }
        public bool IsInspected
        {
            get
            {
                return _isInspected;
            }
            set
            {
                if (_isInspected == value)
                    return;

                _isInspected = value;
                OnPropertyChanged("IsInspected");
            }
        }

        public CopyTaskViewModel CopyTask { get; private set; }

        public Visibility DetailsVisibility
        {
            get
            {
                return State ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public FontWeight LetterFontWeight
        {
            get
            {
                return IsAttention ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        public bool IsAttention
        {
            get
            {
                return _attention;
            }
            set
            {
                if (_attention == value)
                    return;

                _attention = value;
                OnPropertyChanged("IsAttention");
                OnPropertyChanged("LetterFontWeight");
            }
        }

        public CdDriveViewModel() 
        {
            _errorFileNames = new ObservableCollection<string>();
            _errorFileNames.CollectionChanged += _errorFileNames_CollectionChanged;
            _errorFileNames.Add("c:\\pagefile.sys");
            _errorFileNames.Add("c:\\io.sys");
        }

        public CdDriveViewModel(CdDrive drive) : this() {
            _drive = drive;
            _drive.StateChanged += _drive_StateChanged;
            _drive.StartStateChecking(1000);
            ResetModel();
        }

        void _drive_StateChanged(CdDrive drive)
        {
            ResetModel();
        }

        void ResetModel()
        {
            MediaInfo = _drive.BuildCdInfo();
        }

        void _errorFileNames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("ErrorVisibility");
            OnPropertyChanged("ErrorCount");
        }

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CdDriveDemos : ObservableCollection<CdDriveViewModel>
    {
        public CdDriveDemos()
        {
            foreach (var drive in GetDemoDrives())
                Add(drive);
        }

        public static IEnumerable<CdDriveViewModel> GetDemoDrives()
        {
            var drives =
                from driveName in CdDrive.GetDriveNames()
                select new CdDrive(driveName);

            return
                from drive in drives
                select new CdDriveViewModel(drive);
        }
    }*/
}
