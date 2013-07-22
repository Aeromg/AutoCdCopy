using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutoCdCopy.Logics;
using System.IO;

namespace AutoCdCopy
{
    public delegate void StorageLockedChangedDelegate(string currentPath, bool locked);

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        static bool _storageLocked;
        static string _currentStoragePath;
        
        static bool _currentProcessesError;
        static bool _currentProcessesIntermediate;

        static double _currentProgress;

        public static string RepoFileName = "catalog.xml";
        public static int ReadBlockSize = 1024*32;
        public static int ReadBlockTries = 1;
        public static bool SkipUnreadableFiles = true;

        public static CdInfoRepo MediaInfoRepository { get; set; }
        public static string CurrentStoragePath 
        { 
            get 
            {
                return _currentStoragePath;
            }
            set
            {
                if (_currentStoragePath == value)
                    return;

                _currentStoragePath = value == null ? "" : value;
                if(!String.IsNullOrEmpty(_currentStoragePath)) {
                    var lastChar = _currentStoragePath.Substring(_currentStoragePath.Length-1, 1).ToCharArray()[0];
                    if (lastChar != Path.DirectorySeparatorChar)
                        _currentStoragePath = _currentStoragePath + Path.DirectorySeparatorChar;
                }
            }
        }
        public static bool StorageLocked
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

                ReloadMediaInfoRepository();

                if (StorageLockedChanged != null)
                    StorageLockedChanged(CurrentStoragePath, StorageLocked);
            }
        }
        public static bool AutostartCopy { get; set; }

        static void ReloadMediaInfoRepository()
        {
            MediaInfoRepository.Clear();
            if (!StorageLocked)
                return;
            
            var repo = CdInfoRepo.ReadFromFile(Path.Combine(CurrentStoragePath, RepoFileName));
            foreach (var item in repo)
                MediaInfoRepository.Add(item);
        }
        public static void UpdateMediaInfoRepository()
        {
            if(StorageLocked)
                CdInfoRepo.SaveToFile(MediaInfoRepository, Path.Combine(CurrentStoragePath, RepoFileName));
        }

        public static double CurrentProgress
        {
            get
            {
                return _currentProgress;
            }
            set
            {
                if (_currentProgress == value)
                    return;

                _currentProgress = value;
                //OnProgressChanged();
            }
        }
        public static bool CurrentProcessesError
        {
            get
            {
                return _currentProcessesError;
            }
            set
            {
                if (_currentProcessesError == value)
                    return;

                _currentProcessesError = value;
                //OnProgressChanged();
            }
        }
        public static bool CurrentProcessesIntermediate
        {
            get
            {
                return _currentProcessesIntermediate;
            }
            set
            {
                if (_currentProcessesIntermediate == value)
                    return;

                _currentProcessesIntermediate = value;
                //OnProgressChanged();
            }
        }

        public static event StorageLockedChangedDelegate StorageLockedChanged;
        public static event EventHandler ProgressChanged;

        public static void OnProgressChanged()
        {
            var eventHandler = ProgressChanged;
            if (eventHandler == null)
                return;

            eventHandler(null, EventArgs.Empty);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MediaInfoRepository = new CdInfoRepo();
            base.OnStartup(e);
        }

        static App() {
            AutostartCopy = true;
            MediaInfoRepository = new CdInfoRepo();
        }
    }
}
