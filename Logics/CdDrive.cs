using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoCdCopy.Logics
{
    public class CdDrive
    {
        public delegate void StateChangedDelegate(CdDrive drive);

        DriveInfo driveInfo;
        public string DriveLetter
        {
            get
            {
                return driveInfo.Name;
            }
        }
        string _serial;
        string _volume;
        long _size;

        Thread _stateCheckThread;

        volatile bool _lastReady;
        volatile bool _isStateChecking;
        volatile int _stateCheckInterval;

        
        public bool IsReady
        {
            get
            {
                return _lastReady;
            }
        }
        public CdDrive(string driveName)
        {
            driveInfo = new DriveInfo(driveName);
            if (driveInfo.DriveType != DriveType.CDRom)
                throw new Exception("Drive " + driveName + " is not a CD ROM drive");
        }
        public event StateChangedDelegate StateChanged;

        public CdInfo BuildCdInfo()
        {
            FetchVolumeInformation();
            if (IsReady)
                return new CdInfo()
                {
                    Letter = DriveLetter,
                    Serial = _serial,
                    Size = _size,
                    Volume = _volume
                };
            else
                return new CdInfo()
                {
                    Letter = DriveLetter,
                    Serial = "",
                    Size = 0,
                    Volume = ""
                };
        }

        void FetchVolumeInformation()
        {
            if (IsReady)
            {
                const int MAX_SIZE = 256;
                StringBuilder volname = new StringBuilder(MAX_SIZE);
                int sn;
                int maxcomplen;
                int sysflags;
                StringBuilder sysname = new StringBuilder(MAX_SIZE);
                GetVolumeInformation(DriveLetter, volname, MAX_SIZE, out sn, out maxcomplen, out sysflags, sysname, MAX_SIZE);

                _serial = sn.ToString();
                _volume = volname.ToString();
                _size = driveInfo.TotalSize;
            }
            else
            {
                _serial = "";
                _volume = "";
                _size = 0;
            }
        }

        public void Eject()
        {
            EjectMedia.Eject(DriveLetter);
        }

        public void StartStateChecking(int interval)
        {
            if (interval <= 0)
                throw new Exception();

            SetCheckStateIntervalMiliseconds(interval);
            if (_stateCheckThread == null)
                StartStateChecking();
        }

        public void StopStateChecking()
        {
            SetCheckStateIntervalMiliseconds(0);
        }

        public void SetCheckStateIntervalMiliseconds(int interval)
        {
            _stateCheckInterval = interval < 0 ? 0 : interval;
        }

        void StartStateChecking()
        {
            _stateCheckThread = new Thread(CheckStateProcedure);
            _stateCheckThread.Start();
        }

        void CheckStateProcedure()
        {
            while (_stateCheckInterval > 0)
            {
                CheckState();
                Thread.Sleep(_stateCheckInterval);
            }
            _stateCheckThread = null;
        }

        void CheckState()
        {
            if (_isStateChecking)
                return;

            _isStateChecking = true;
            var ready = driveInfo.IsReady;
            if (ready != _lastReady)
            {
                _lastReady = ready;
                OnStateChanged();
            }
            _isStateChecking = false;
        }

        void OnStateChanged()
        {
            if (StateChanged != null)
                StateChanged(this);
        }

        public static IEnumerable<string> GetDriveNames()
        {
            return
                from cd in DriveInfo.GetDrives()
                where cd.DriveType == DriveType.CDRom
                select cd.Name;
        }

        // form http://forum.vingrad.ru/forum/topic-22673.html
        [DllImport("kernel32.dll")]
        static extern int GetVolumeInformation(string strPathName,
            StringBuilder strVolumeNameBuffer,
            int lngVolumeNameSize,
            out int lngVolumeSerialNumber,
            out int lngMaximumComponentLength,
            out int lngFileSystemFlags,
            StringBuilder strFileSystemNameBuffer,
            int lngFileSystemNameSize);
    }
}
