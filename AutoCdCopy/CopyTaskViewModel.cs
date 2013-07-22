using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using AutoCdCopy.Logics;
using System.Collections.ObjectModel;
using ADev.FileSystemTools;

namespace AutoCdCopy
{/*
    public class CopyTaskViewModel : INotifyPropertyChanged
    {
        CopyWorker _tool;

        string _source;
        string _destination;

        public string CurrentFileName
        {
            get
            {
                return _tool.Stage.CurrentFilePath;
            }
        }
        public string TotalBytesProgress
        {
            get
            {
                double copied = _tool.Stage.BytesCopied;
                double total = _tool.Stage.BytesTotal;

                return String.Format("{0:0.00}%", copied / total);
            }
        }
        public string TotalFilesProgress
        {
            get
            {
                double copied = _tool.Stage.FilesCopiedCount;
                double total = _tool.Stage.SourceFilesCount;

                return String.Format("{0:0.00}%", copied / total);
            }
        }
        public long TotalBytes
        {
            get
            {
                return _tool.Stage.BytesTotal;
            }
        }
        public long TotalFiles
        {
            get
            {
                return _tool.Stage.SourceFilesCount;
            }
        }

        public ObservableCollection<string> ErrorFiles { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        CopyTaskViewModel(string source, string destination)
        {
            _source = source;
            _destination = destination;
            _tool = new CopyWorker(source, destination);
            ErrorFiles = new ObservableCollection<string>();
        }



        void CopyError(string file, CopyMode mode, long startByte)
        {
            ErrorFiles.Add(file);
        }

        void CopyProgress(string file, CopyState state, CopyMode mode)
        {
            if (state == CopyState.Start)
                OnPropertyChanged("CurrentFileName");

            if (mode == CopyMode.Stream)
                OnPropertyChanged("TotalBytesProgress");

            if (state == CopyState.Done)
            {
                OnPropertyChanged("TotalFilesProgress");
                OnPropertyChanged("TotalBytesProgress");
            }
        }

        void AnalyzeProgress(string path, CopyState state)
        {
            if(state == CopyState.Done && path == _source)
            {
                OnPropertyChanged("TotalBytes");
                OnPropertyChanged("TotalFiles");
                return;
            }

            if(state == CopyState.Error)
                ErrorFiles.Add(path);
        }

        void OnPropertyChanged(string fieldName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(fieldName));
        }

        public static CopyTaskViewModel CreateTask(string source, string destination)
        {
            return new CopyTaskViewModel(source, destination);
        }
    }*/
}
