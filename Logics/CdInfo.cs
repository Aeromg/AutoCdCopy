using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCdCopy.Logics
{
    public class CdInfo
    {
        public string Letter;
        public string Volume;
        public string Serial;
        public long Size;
        public string UserDefinedName;
        public string UserDefinedComment;

        public DateTime InitDate;
        public string DumpFolderName;
        public bool IsCopied;

        public string[] SkippedFiles;
    }
}
