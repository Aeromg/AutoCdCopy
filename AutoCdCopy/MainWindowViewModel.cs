using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AutoCdCopy.Logics;

namespace AutoCdCopy
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        string _storagePath;
        bool _storageLocked;
        bool _cdVisible;
        DriveInfo _storageDrive;
        bool _isGlobalProgress;

        public ObservableCollection<DriveViewModel> Drives { get; set; }
        public string StoragePath { 
            get {
                return _storagePath;
            } 
            set {
                if (_storagePath == value)
                    return;

                _storagePath = value;
                OnPropertyChanged("StoragePath");
            } 
        }
        public bool StorageLocked
        {
            get
            {
                return _storageLocked;
            }
            set
            {
                if (_storageLocked == value)
                    return;

                _storageLocked = value;
                OnPropertyChanged("StorageLocked");
                OnPropertyChanged("IsStorageUnlocked");
            }
        }
        public bool IsStorageUnlocked
        {
            get
            {
                return !StorageLocked;
            }
        }
        public long StorageFreeBytes
        {
            get
            {
                return StorageLocked ? _storageDrive.AvailableFreeSpace : 0;
            }
        }
        public long StorageTotalSize
        {
            get
            {
                return StorageLocked ? _storageDrive.TotalSize : 0;
            }
        }
        public int StorageSpacePercents
        {
            get
            {
                if(StorageLocked)
                    return 100-(int)(((double)StorageFreeBytes / (double)StorageTotalSize) * 100);

                return 0;
            }
        }
        public string StorageSpaceInfo
        {
            get
            {
                if(StorageLocked)
                    return GetStorageSpaceInfo();

                return "";
            }
        }
        public bool StorageDriveSelected
        {
            get
            {
                return _storageDrive != null;
            }
        }
        public Visibility StorageSpaceVisibility
        {
            get
            {
                return StorageDriveSelected ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public string StorageLockActionText
        {
            get
            {
                return StorageLocked ? "Unlock storage" : "Lock storage";
            }
        }
        public ICommand LockUnlockAction { get; set; }
        public ICommand BrowseStorage { get; set; }
        public Visibility CDVisibility
        {
            get
            {
                return CDVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility NoCDVisibility
        {
            get
            {
                return CDVisible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        bool CDVisible
        {
            get
            {
                return _cdVisible; 
            }
            set
            {
                if (_cdVisible == value)
                    return;

                _cdVisible = value;
                OnPropertyChanged("CDVisibility");
                OnPropertyChanged("NoCDVisibility");
            }
        }
        public bool Autostart
        {
            get
            {
                return App.AutostartCopy;
            }
            set
            {
                var autostart = App.AutostartCopy;
                if (autostart == value)
                    return;

                App.AutostartCopy = value;
                OnPropertyChanged("Autostart");
            }
        }

        public string ReadBlockSizeString
        {
            get
            {
                return String.Format("{0:0}KB", ReadBlockSize / 1024);
            }
        }
        public int ReadBlockSize {
            get
            {
                return App.ReadBlockSize;
            }
            set
            {
                if (App.ReadBlockSize == value)
                    return;

                App.ReadBlockSize = value;
                OnPropertyChanged("ReadBlockSize");
                OnPropertyChanged("ReadBlockSizeString");
            }
        }
        public int ReadBlockTries
        {
            get
            {
                return App.ReadBlockTries;
            }
            set
            {
                if (App.ReadBlockTries == value)
                    return;

                App.ReadBlockTries = value;
                OnPropertyChanged("ReadBlockTries");
            }
        }
        public int ReadBlockSizeLevel
        {
            get
            {
                return (int)Math.Log(ReadBlockSize, 2) - 9;
            }
            set
            {
                ReadBlockSize = (int)Math.Pow(2, value + 9);
                //OnPropertyChanged("ReadBlockSizeLevel");
                OnPropertyChanged("ReadBlockSize");
            }
        }
        public bool SkipUnreadableFiles
        {
            get
            {
                return App.SkipUnreadableFiles;
            }
            set
            {
                if (App.SkipUnreadableFiles == value)
                    return;

                App.SkipUnreadableFiles = value;
                OnPropertyChanged("SkipUnreadableFiles");
                OnPropertyChanged("ReadBlockTriesEnabled");
            }
        }
        public bool ReadBlockTriesEnabled
        {
            get
            {
                return !SkipUnreadableFiles;
            }
        }
        public string RepoFileName
        {
            get
            {
                return App.RepoFileName;
            }
            set
            {
                if (App.RepoFileName == value)
                    return;

                App.RepoFileName = value;
                OnPropertyChanged("RepoFileName");
            }
        }
        public bool IsGlobalProgress
        {
            get
            {
                return _isGlobalProgress;
            }
            set
            {
                if (_isGlobalProgress == value)
                    return;

                _isGlobalProgress = value;
                OnPropertyChanged("IsGlobalProgress");
            }
        }
        public bool IsGlobalIdle
        {
            get
            {
                return !IsGlobalProgress;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Drives = new ObservableCollection<DriveViewModel>();
            UpdateDrives();
            StoragePath = "";
            LockUnlockAction = new StorageLockCommand(this);
            BrowseStorage = new BrowseStorageCommand(this);
            OnPropertyChanged("StorageDriveSelected");
            CDVisible = Drives.Count > 0;
            App.ProgressChanged += App_ProgressChanged;
        }

        void App_ProgressChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateGlobalProgress();
            });
        }

        void UpdateGlobalProgress()
        {
            var processes =
                from drive in Drives
                where drive.IsProcess
                select drive;

            double overallProgress = 0;
            if(processes.Count() > 0)
                overallProgress = 
                    (from process in processes
                     select process.ProgressPercents).Sum() / processes.Count() / 100;

            App.CurrentProgress = overallProgress;
            App.CurrentProcessesIntermediate = (processes.FirstOrDefault((p) => { return p.IsMediaInfoRequired; }) != null);
            // App.CurrentProcessesError = (processes.FirstOrDefault((p) => { return p.IsErrorPresented; }) != null);

            OnPropertyChanged("IsGlobalIdle");
            OnPropertyChanged("IsGlobalProgress");
        }

        public void UpdateStorageDrive()
        {
            if (String.IsNullOrEmpty(StoragePath))
                return;

            if (StorageLocked)
                UnlockStorage();
            else
                LockStorage();

            App.StorageLocked = StorageLocked;
        }

        void UnlockStorage()
        {
            StorageLocked = false;
            _storageDrive = null;

            OnPropertyChanged("StorageFreeBytes");
            OnPropertyChanged("StorageTotalSize");
            OnPropertyChanged("StorageSpaceInfo");
            OnPropertyChanged("StorageSpaceVisibility");
            OnPropertyChanged("StorageLockActionText");
            OnPropertyChanged("StorageSpacePercents");
        }

        void LockStorage()
        {
            DirectoryInfo storageDirectory;
            try
            {
                storageDirectory = new DirectoryInfo(StoragePath);
                if (!storageDirectory.Exists)
                    return;

                _storageDrive = new DriveInfo(storageDirectory.Root.FullName);
            }
            catch
            {
                return;
            }

            if (!_storageDrive.IsReady)
            {
                _storageDrive = null;
                return;
            }
            StorageLocked = true;

            App.CurrentStoragePath = storageDirectory.FullName;

            OnPropertyChanged("StorageFreeBytes");
            OnPropertyChanged("StorageTotalBytes");
            OnPropertyChanged("StorageSpaceInfo");
            OnPropertyChanged("StorageSpaceVisibility");
            OnPropertyChanged("StorageLockActionText");
            OnPropertyChanged("StorageSpacePercents");
        }

        void UpdateDrives()
        {
            Drives.Clear();
            foreach (var driveName in CdDrive.GetDriveNames())
            {
                Drives.Add(new DriveViewModel(new CdDrive(driveName)));
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        string GetStorageSpaceInfo()
        {
            var free = StorageFreeBytes;
            var total = StorageTotalSize;

            var freeString = SizeHumanizator.GetHumanReadableSizeFromBytes(free);
            var totalString = SizeHumanizator.GetHumanReadableSizeFromBytes(total);

            return String.Format("{0} of {1} total", freeString, totalString);
        }
        
    }

    public class StorageLockCommand : ICommand
    {
        MainWindowViewModel ViewModel { get; set; }

        public StorageLockCommand(MainWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if (e.PropertyName == "StoragePath" && CanExecuteChanged != null)
                // CanExecuteChanged(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            return true; //!String.IsNullOrEmpty(ViewModel.StoragePath);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ViewModel.UpdateStorageDrive();
        }
    }

    public class BrowseStorageCommand : ICommand
    {
        MainWindowViewModel ViewModel { get; set; }

        public BrowseStorageCommand(MainWindowViewModel model)
        {
            ViewModel = model;
            App.StorageLockedChanged += App_StorageLockedChanged;
        }

        void App_StorageLockedChanged(string currentPath, bool locked)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return !App.StorageLocked;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = App.CurrentStoragePath;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.StoragePath = dialog.SelectedPath;
            }
        }
    }
}
