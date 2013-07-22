using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;
using ADev.FileSystemTools;
using System.Diagnostics;

namespace ADev.FileSystemTools
{
    public enum CopyWorkerProgressStage
    {
        DirectoriesReview,
        FilesReview,
        FileSystemMirroring,
        FileCopy,
        FileCopyDone,
        Done
    }

    public class CopyWorker
    {
        class TaskInterruptedException : Exception { }

        public class CopyWorkerState
        {
            CopyWorker _worker;

            internal CopyWorkerState(CopyWorker worker)
            {
                _worker = worker;
            }

            public double Progress
            {
                get
                {
                    return (double)_worker._bytesCopied / (double)_worker._sourceBytesTotal;
                }
            }
            public int SourceDirectoriesCount
            {
                get
                {
                    return _worker._sourceDirectoriesCount;
                }
            }
            public int SourceFilesCount
            {
                get
                {
                    return _worker._sourceFilesCount;
                }
            }
            public int FilesCopiedCount
            {
                get
                {
                    return _worker._filesCopiedCount;
                }
            }
            public long BytesTotal
            {
                get
                {
                    return _worker._sourceBytesTotal;
                }
            }
            public long BytesCopied
            {
                get
                {
                    return _worker._bytesCopied;
                }
            }
            public CopyWorkerProgressStage Stage
            {
                get
                {
                    return _worker._stage;
                }
            }
            public string CurrentFilePath
            {
                get
                {
                    return _worker._currentFileCopy.FullName;
                }
            }
        }

        const string ArgumentIsNullOrEmptyMessage = "Argument is null or empty";
        const string FileNotFoundMessage = "File not found";

        byte[] ZeroBuffer;

        DirectoryInfo _sourceDirectory;
        DirectoryInfo _destinationDirectory;

        int _sourceDirectoriesCount;
        int _sourceFilesCount;
        int _filesCopiedCount;
        long _sourceBytesTotal;
        long _bytesCopied;

        FileInfo _currentFileCopy;

        bool _interrupted;
        volatile bool _isAsync;
        bool IsAsync
        {
            get
            {
                return _worker != null;
            }
        }

        string _sourcePath;
        string _destinationPath;

        public int ReadBlockSizeBytes { get; set; }
        public bool SkipOnReadError { get; set; }
        public int TriesToReadBlock { get; set; }

        DirectoryInfo[] _sourceDirectories;
        FileInfo[] _sourceFiles;

        public event EventHandler<EventArgs> ReviewProgress;
        public event EventHandler<string> FileCopyStart;
        public event EventHandler<string> FileCopyEnd;
        public event EventHandler<string> FileReadError;
        public event EventHandler<double> FileCopyProgress;
        public event EventHandler Progress;
        public event EventHandler<CopyWorkerState> AsyncProgress;

        Stopwatch EventStopwatch;

        public CopyWorkerState Stage
        {
            get
            {
                return _state;
            }
        }

        Thread _worker;
        CopyWorkerState _state;
        CopyWorkerProgressStage _stage;

        public CopyWorker(string sourcePath, string destinationPath)
        {
            ReadBlockSizeBytes = 1024 * 32;
            SkipOnReadError = true;
            TriesToReadBlock = 0;
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
        }

        public void Copy()
        {
            Init();
            EventStopwatch = new Stopwatch();

            _interrupted = false;

            try
            {
                ReviewFileSystem();
                CopyFileSystem();
            }
            catch
            {
                _interrupted = true;
            }

            _stage = CopyWorkerProgressStage.Done;
            OnDone();
        }

        public void BeginCopy() 
        {
            lock (this)
            {
                if (_isAsync)
                    return;

                _isAsync = true;
            }

            _worker = new Thread(() =>
            {
                Copy();
                EndCopy();
            });
            _worker.Start();
        }

        public void EndCopy()
        {
            _interrupted = true;
            _worker = null;
        }

        void ReviewFileSystem()
        {
            _stage = CopyWorkerProgressStage.DirectoriesReview;
            OnReviewProgress();

            _sourceDirectoriesCount = 0;
            _sourceFilesCount = 0;
            _sourceBytesTotal = 0;
            _sourceFiles = null;

            var sourceDirs = new List<DirectoryInfo>();
            sourceDirs.Add(_sourceDirectory);
            sourceDirs.AddRange(_sourceDirectory.EnumerateDirectories("*", SearchOption.AllDirectories));
            _sourceDirectories = sourceDirs.ToArray();
            _sourceDirectoriesCount = _sourceDirectories.Length;
            
            var files = new List<FileInfo>();
            
            foreach (var dir in _sourceDirectories)
            {
                CheckInterrupted();
                foreach (var file in Directory.EnumerateFiles(dir.FullName, "*"))
                {
                    try
                    {
                        files.Add(new FileInfo(file));
                    }
                    catch {
                    }
                }
            }

            _sourceFiles = files.ToArray();
            _stage = CopyWorkerProgressStage.FilesReview;
            foreach (var file in _sourceFiles)
            {
                CheckInterrupted();
                _sourceFilesCount++;
                _sourceBytesTotal += file.Length;
                OnReviewProgress();
            }
        }

        void CopyFileSystem()
        {
            CreateMirrorDirectoriesStructure();
            CopyContent();
        }

        void CreateMirrorDirectoriesStructure()
        {
            _stage = CopyWorkerProgressStage.FileSystemMirroring;
            OnFileSystemMirroring();

            foreach (var directory in _sourceDirectories)
                CreateMirrorDirectory(directory);
        }

        void CreateMirrorDirectory(DirectoryInfo subDirectory)
        {
            CheckInterrupted();

            if (_destinationDirectory.Exists)
                return;

            _destinationDirectory.Create();

            var destinationRelative = FileSystemUtils.GetRelativePath(_sourceDirectory, subDirectory);
            var destinationFullPath = Path.Combine(_destinationDirectory.FullName, destinationRelative);
            var destinationDirectory = Directory.CreateDirectory(destinationFullPath);
            
            CopyMeta(subDirectory, destinationDirectory);
        }

        void CopyContent()
        {
            _stage = CopyWorkerProgressStage.FileCopy;

            foreach (var file in _sourceFiles)
                CopyFile(file);
        }

        void CopyFile(FileInfo sourceFile)
        {
            _currentFileCopy = sourceFile;

            if (!sourceFile.Exists)
                return;

            CheckInterrupted();

            var destinationRelative = FileSystemUtils.GetRelativePath(_sourceDirectory, sourceFile);
            var destinationFullPath = Path.Combine(_destinationDirectory.FullName, destinationRelative);
            var destinationFile = new FileInfo(destinationFullPath);

            _stage = CopyWorkerProgressStage.FileCopy;
            OnFileCopyStart();

            if(destinationFile.Exists)
                if (destinationFile.Length != sourceFile.Length)
                    destinationFile.Delete();

            CopyFileData(sourceFile, destinationFile);
            CopyMeta(sourceFile, destinationFile);
            
            _filesCopiedCount++;

            _stage = CopyWorkerProgressStage.FileCopyDone;
            OnFileCopyEnd();
        }

        void CopyMeta(FileSystemInfo source, FileSystemInfo destination)
        {
            try
            {
                destination.CreationTime = source.CreationTime;
                destination.LastAccessTime = source.LastAccessTime;
                destination.LastWriteTime = source.LastWriteTime;
                destination.Attributes = source.Attributes;
            }
            catch { }
        }

        void CopyFileData(FileInfo source, FileInfo destination)
        {
            using (var sourceStream = source.OpenRead())
            {
                using (var destinationStream = destination.OpenWrite())
                {
                    CopyFileDataStream(sourceStream, destinationStream);
                    //destinationStream.Flush();
                    //destinationStream.Close();
                }
                sourceStream.Close();
            }
        }

        void CopyFileDataStream(FileStream source, FileStream destination)
        {
            EventStopwatch.Restart();

            var readBuffer = new byte[ReadBlockSizeBytes];
            byte[] writeBuffer;
            int bytesReaded = -1;
            long subTotalBytesReaded = 0;
            int tries = 0;
            long sourceLength = source.Length;

            while (bytesReaded != 0) {
                CheckInterrupted();

                try
                {
                    bytesReaded = source.Read(readBuffer, 0, ReadBlockSizeBytes);
                    writeBuffer = readBuffer;
                }
                catch
                {
                    // on file copy error
                    OnFileReadError();

                    if (tries < TriesToReadBlock)
                    {
                        tries++;
                        continue;
                    }

                    if (SkipOnReadError)
                    {
                        AppendBytesCopied(sourceLength - subTotalBytesReaded);
                        return;
                    }

                    bytesReaded = sourceLength > subTotalBytesReaded + ReadBlockSizeBytes ?
                        ReadBlockSizeBytes :
                        (int)(sourceLength - subTotalBytesReaded);

                    source.Seek(bytesReaded, SeekOrigin.Current);

                    writeBuffer = ZeroBuffer;
                }

                destination.Write(writeBuffer, 0, bytesReaded);
                destination.Flush();

                subTotalBytesReaded += bytesReaded;
                AppendBytesCopied(bytesReaded);
                tries = 0;

                if (EventStopwatch.ElapsedMilliseconds > 100)
                {
                    OnFileCopyProgress();
                    EventStopwatch.Restart();
                }
            }
        }

        void AppendBytesCopied(long bytesCopied)
        {
            _bytesCopied += bytesCopied;
        }

        void FillZero(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0;
        }

        void Init()
        {
            _sourceDirectory = new DirectoryInfo(_sourcePath);
            _destinationDirectory = new DirectoryInfo(_destinationPath);

            if (!_sourceDirectory.Exists)
                throw new FileNotFoundException(FileNotFoundMessage, _sourceDirectory.FullName);

            ZeroBuffer = new byte[ReadBlockSizeBytes];
            FillZero(ZeroBuffer);
            _state = new CopyWorkerState(this);
        }

        void OnReviewProgress()
        {
            OnEvent<EventArgs>(ReviewProgress, EventArgs.Empty);
        }
        void OnFileSystemMirroring()
        {
            OnProgress();
        }
        void OnFileCopyStart()
        {
            _stage = CopyWorkerProgressStage.FileCopy;
            OnEvent<string>(FileCopyStart, _currentFileCopy.FullName);
        }
        void OnFileCopyEnd()
        {
            OnEvent<string>(FileCopyEnd, _currentFileCopy.FullName);
        }
        void OnFileCopyProgress()
        {
            OnEvent<double>(FileCopyProgress, _state.Progress);
        }
        void OnFileReadError()
        {
            OnEvent(FileReadError, _currentFileCopy.FullName);
        }
        void OnDone()
        {
            OnEvent<string>(FileCopyEnd, _sourceDirectory.FullName);
        }

        void OnEvent<TArg>(EventHandler<TArg> eventToFire, TArg arg)
        {
            if (IsAsync)
                OnAsyncProgress();

            var eventHandler = eventToFire;

            if (eventHandler == null)
                return;

            eventHandler(this, arg);
        }
        void OnProgress()
        {
            if (IsAsync)
                OnAsyncProgress();

            var eventHandler = Progress;

            if (eventHandler == null)
                return;

            eventHandler(this, EventArgs.Empty);
        }
        void OnAsyncProgress()
        {
            var eventHandler = AsyncProgress;

            if (eventHandler == null)
                return;

            eventHandler.Invoke(this, _state);
        }

        void CheckInterrupted()
        {
            if (_interrupted)
                throw new TaskInterruptedException();
        }
    }
}
