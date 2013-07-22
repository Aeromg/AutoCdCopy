using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoCdCopy.Logics;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ADev.FileSystemTools;

namespace AutoCdCopy
{
    public class DriveViewModel : INotifyPropertyChanged
    {
        const int DriveStateCheckingInterval = 1000;
        #region Props
        string _driveName;
        string _name;
        string _comment;
        string _label;
        string _serial;
        long _sizeBytes;

        volatile CopyWorker Worker;
        CdDrive Drive;
        bool _mediaInfoStored;
        bool _mediaCopyProcess;
        double _progressPercents;
        volatile bool _mediaReady;
        bool _dotsVisible = false;
        bool _progressVisible = false;
        bool _isAnalyzing;
        bool _isMediaInfoStored;

        public bool IsMediaInfoRequired
        {
            get {
                return (!_mediaInfoStored && IsMediaCopied) &&
                    (String.IsNullOrEmpty(Name) || String.IsNullOrEmpty(Comment));
            }
        }
        bool IsMediaCopied
        {
            get
            {
                return ProgressPercents >= 100;
            }
        }
        bool IsMediaCopyProcessRun
        {
            get
            {
                return _mediaCopyProcess;
            }
            set
            {
                if (_mediaCopyProcess == value)
                    return;

                _mediaCopyProcess = value;
                OnPropertyChanged("ProgressPercents");
                OnPropertyChanged("MediaState");

            }
        }
        bool IsMediaReady
        {
            get
            {
                return _mediaReady;
            }
            set
            {
                if (_mediaReady == value)
                    return;

                _mediaReady = value;
                OnMediaReadyChange();
                OnPropertyChanged("NoMediaVisibility");
                OnPropertyChanged("MediaInfoVisibility");
            }
        }

        public bool IsMediaInfoStored
        {
            get
            {
                return _isMediaInfoStored;
            }
            set
            {
                if (_isMediaInfoStored == value)
                    return;

                _isMediaInfoStored = value;
                OnPropertyChanged("IsMediaInfoStored");
                OnPropertyChanged("MediaState");
            }
        }
        public bool CanStartCopy
        {
            get {
                return IsMediaReady && App.StorageLocked;
            }
        }
        public string DriveName
        {
            get
            {
                return _driveName;
            }
            set
            {
                if (_driveName == value)
                    return;

                _driveName = value;
                OnPropertyChanged("DriveName");
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value)
                    return;

                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (_comment == value)
                    return;

                _comment = value;
                OnPropertyChanged("Comment");
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
        public long SizeBytes
        {
            get
            {
                return _sizeBytes;
            }
            set
            {
                if (_sizeBytes == value)
                    return;

                _sizeBytes = value;
                OnPropertyChanged("SizeBytes");
                OnPropertyChanged("SizeString");
            }
        }
        public string SizeString
        {
            get
            {
                return SizeHumanizator.GetHumanReadableSizeFromBytes(SizeBytes);
            }
        }
        public ObservableCollection<string> ErrorFiles { get; set; }
        public bool IsErrorPresented
        {
            get
            {
                return ErrorFiles.Count > 0;
            }
        }
        public Visibility ErrorListVisibility
        {
            get
            {
                return ErrorFiles.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility MediaInfoVisibility
        {
            get
            {
                return IsMediaReady ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility NoMediaVisibility
        {
            get
            {
                return IsMediaReady ? Visibility.Hidden : Visibility.Visible;
            }
        }
        public double ProgressPercents
        {
            get
            {
                return _progressPercents;
            }
            set
            {
                if (_progressPercents == value)
                    return;

                _progressPercents = value;
                OnPropertyChanged("ProgressPercents");
                OnPropertyChanged("MediaState");
            }
        }
        public bool IsProcess
        {
            get
            {
                return IsMediaCopyProcessRun || IsAnalyzing || IsMediaInfoRequired;
            }
        }

        public bool DotsVisible
        {
            get
            {
                return _dotsVisible;
            }
            set 
            {
                if (_dotsVisible == value)
                    return;

                _dotsVisible = value;
                OnPropertyChanged("DotsVisible");
                OnPropertyChanged("DotsVisibility");
            }
        }
        public Visibility DotsVisibility
        {
            get
            {
                return DotsVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public bool ProgressVisible
        {
            get
            {
                return _progressVisible;
            }
            set
            {
                if (_progressVisible == value)
                    return;

                _progressVisible = value;
                OnPropertyChanged("ProgressVisible");
                OnPropertyChanged("ProgressVisibility");
            }
        }
        public Visibility ProgressVisibility
        {
            get
            {
                return ProgressVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }
        
        public string MediaState
        {
            get
            {
                return GetMediaState();
            }
        }
        public bool IsAnalyzing
        {
            get
            {
                return _isAnalyzing;
            }
            set
            {
                if (_isAnalyzing == value)
                    return;

                _isAnalyzing = value;
                OnPropertyChanged("IsAnalyzing");
                OnPropertyChanged("MediaState");
                DotsVisible = _isAnalyzing;
            }
        }

        public ICommand StoreMediaInfo { get; set; }
        public ICommand Eject { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public DriveViewModel() 
        {
            ErrorFiles = new ObservableCollection<string>();
            StoreMediaInfo = new StoreMediaInfoCommand(this);
            Eject = new EjectMediaCommand(this);
            App.Current.Exit += Current_Exit;

            Drive = new CdDrive(CdDrive.GetDriveNames().FirstOrDefault());

            ResetTask();

        }

        public DriveViewModel(CdDrive drive)
        {
            ErrorFiles = new ObservableCollection<string>();
            StoreMediaInfo = new StoreMediaInfoCommand(this);
            Eject = new EjectMediaCommand(this);
            App.Current.Exit += Current_Exit;

            Drive = drive;
            Drive.StateChanged += _drive_StateChanged;
            Drive.StartStateChecking(DriveStateCheckingInterval);
            App.StorageLockedChanged += App_StorageLockedChanged;

            ResetTask();
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            Drive.StopStateChecking();
            CancelCopy();
        }

        void App_StorageLockedChanged(string currentPath, bool locked)
        {
            OnPropertyChanged("CanStartCopy");
            if (locked)
            {
                var mediainfo = App.MediaInfoRepository.GetByLabelAndSerial(Label, Serial);
                if (mediainfo != null)
                    ResetTask();
            }
        }

        public void SaveMediaInfo()
        {
            var info = GetCdInfo();
            var unreaded = new List<string>();
            if(info.SkippedFiles != null)
                unreaded.AddRange(info.SkippedFiles);
            unreaded.AddRange(ErrorFiles);
            info.SkippedFiles = unreaded.ToArray();
            App.MediaInfoRepository.Update(info);
            App.UpdateMediaInfoRepository();
            IsMediaInfoStored = true;
            _mediaInfoStored = true;
            OnPropertyChanged("MediaState");
            App.OnProgressChanged();
        }

        public void EjectMedia()
        {
            if (IsMediaReady)
                Drive.Eject();
        }

        void _drive_StateChanged(CdDrive drive)
        {
            Application.Current.Dispatcher.Invoke(
                () => { IsMediaReady = drive.IsReady; }
            );
        }
        void ResetTask()
        {
            CancelCopy();

            var readerInfo = Drive.BuildCdInfo();
            CdInfo info = App.MediaInfoRepository.GetByLabelAndSerial(readerInfo.Volume, readerInfo.Serial);
            if (info == null)
            {
                _mediaInfoStored = false;
                info = readerInfo;
            }
            else
                _mediaInfoStored = true;

            DriveName = Drive.DriveLetter;
            Name = info.UserDefinedName;
            Comment = info.UserDefinedComment;
            Label = info.Volume;
            Serial = info.Serial;
            SizeBytes = info.Size;
            _mediaReady = Drive.IsReady;
            
            _mediaCopyProcess = false;
            ProgressPercents = 0;
            ProgressVisible = false;
            DotsVisible = false;
            ErrorFiles.Clear();
            if (info.SkippedFiles != null)
                foreach (var err in info.SkippedFiles)
                    ErrorFiles.Add(err);

            if (CheckIsAutostartAvailable())
                StartCopy();

            OnPropertyChanged("MediaState");
            OnPropertyChanged("ErrorListVisibility");
            App.OnProgressChanged();
        }

        void StartCopy()
        {
            if (Worker != null)
                return;
            Drive.StopStateChecking();
            Worker = new CopyWorker(DriveName, GetMediaDumpFullPath());
            Worker.ReadBlockSizeBytes = App.ReadBlockSize;
            Worker.SkipOnReadError = App.SkipUnreadableFiles;
            Worker.TriesToReadBlock = App.ReadBlockTries;
            Worker.AsyncProgress += Worker_AsyncProgress;
            Worker.FileReadError += Worker_FileReadError;
            ProgressVisible = true;
            Worker.BeginCopy();
            App.OnProgressChanged();
        }

        void CancelCopy()
        {
            if (Worker == null)
                return;

            Worker.EndCopy();
            ProgressVisible = false;

            Worker = null;
            App.OnProgressChanged();
        }

        void Worker_FileReadError(object sender, string e)
        {
            if (ErrorFiles.Contains(e))
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorFiles.Add(e);
                OnPropertyChanged("ErrorListVisibility");
            });
        }
        void Worker_AsyncProgress(object sender, CopyWorker.CopyWorkerState e)
        {
            Application.Current.Dispatcher.Invoke(
                () => { Worker_Progress(sender, e); }
            );
        }
        void Worker_Progress(object sender, CopyWorker.CopyWorkerState e)
        {
            switch (e.Stage)
            {
                case CopyWorkerProgressStage.DirectoriesReview:
                case CopyWorkerProgressStage.FilesReview:
                case CopyWorkerProgressStage.FileSystemMirroring:
                    IsAnalyzing = true;
                    DotsVisible = true;
                    break;
                case CopyWorkerProgressStage.FileCopy:
                case CopyWorkerProgressStage.FileCopyDone:
                    IsAnalyzing = false;
                    IsMediaCopyProcessRun = true;
                    break;
                case CopyWorkerProgressStage.Done:
                    OnMediaCopied();
                    break;
            }
            ProgressPercents = (e.Progress * 100);
            App.OnProgressChanged();
        }

        void OnMediaCopied() 
        {
            Drive.StartStateChecking(DriveStateCheckingInterval);
            if (!IsMediaInfoRequired)
                SaveMediaInfo();

            Worker = null;
            IsMediaCopyProcessRun = false;
            ProgressPercents = 100;
            OnPropertyChanged("MediaState");
            App.OnProgressChanged();
        }

        void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;

            if (eventHandler == null)
                return;

            Application.Current.Dispatcher.Invoke(
                () => { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
            );
        }
        void OnMediaReadyChange()
        {
            ResetTask();
        }
        string GetMediaState()
        {
            if (!Drive.IsReady)
                return "No media";

            if (IsMediaInfoRequired)
                return "Media info required";

            if (IsMediaCopied && !IsMediaInfoRequired)
                return "Done";

            if (IsMediaCopyProcessRun)
                return String.Format("Copy: {0:0.00}%", ProgressPercents);

            if (IsAnalyzing)
                return "Media analyzing";

            return "Idle";
        }
        string GetMediaDumpFullPath()
        {
            return App.CurrentStoragePath + Serial.Replace("-", "") + Label;
        }

        CdInfo GetCdInfo()
        {
            return new CdInfo()
            {
                DumpFolderName = GetMediaDumpFullPath(),
                InitDate = DateTime.Now,
                Letter = DriveName,
                Serial = Serial,
                Size = SizeBytes,
                UserDefinedComment = Comment,
                UserDefinedName = Name,
                Volume = Label,
                IsCopied = IsMediaCopied
            };
        }

        bool CheckIsAutostartAvailable()
        {
            if (!App.StorageLocked || !Drive.IsReady)
                return false;

            var readerInfo = Drive.BuildCdInfo();
            CdInfo info = App.MediaInfoRepository.GetByLabelAndSerial(readerInfo.Volume, readerInfo.Serial);
            if (info == null || !info.IsCopied)
                return App.AutostartCopy;

            return App.AutostartCopy && !info.IsCopied;
        }

        ~DriveViewModel()
        {
            if (Drive != null)
                Drive.StopStateChecking();

            App.StorageLockedChanged -= App_StorageLockedChanged;
        }
    }

    public class StoreMediaInfoCommand : ICommand
    {
        DriveViewModel ViewModel { get; set; }

        public StoreMediaInfoCommand(DriveViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = e.PropertyName;
            if (property == "Name" || property == "Comment" || property == "CanStartCopy")
            {
                var eventHandler = CanExecuteChanged;

                if (eventHandler != null)
                    eventHandler(this, EventArgs.Empty);
                
                //eventHandler.Raise(this, EventArgs.Empty);
                //Application.Current.Dispatcher.BeginInvoke(eventHandler, this, EventArgs.Empty);

                //eventHandler.Invoke(this, EventArgs.Empty);
            }
                
        }

        public bool CanExecute(object parameter)
        {
            if (!App.StorageLocked)
                return false;

            return !(String.IsNullOrEmpty(ViewModel.Name) || String.IsNullOrEmpty(ViewModel.Comment));
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ViewModel.SaveMediaInfo();
        }
    }

    public class EjectMediaCommand : ICommand
    {
        DriveViewModel ViewModel { get; set; }

        public EjectMediaCommand(DriveViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ViewModel.EjectMedia();
        }
    }
}
