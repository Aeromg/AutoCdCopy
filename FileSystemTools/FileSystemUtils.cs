using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ADev.FileSystemTools
{
    public static class FileSystemUtils
    {
        /// <summary>
        /// Returns subdirectory relative path about base directory.
        /// Method returns subdirectory absolute path without a drive name if it out of base directory bound.
        /// </summary>
        /// <param name="baseDirectory">Base directory</param>
        /// <param name="subElement">Subdirectory</param>
        /// <returns>Relative or absolute path</returns>
        public static string GetRelativePath(DirectoryInfo baseDirectory, FileSystemInfo subElement)
        {
            var baseName = baseDirectory.FullName;
            var subName = subElement.FullName;

            string result;

            if (!subName.StartsWith(baseName))
                result = subName.Substring(3);
            else if (baseDirectory == subElement)
                return Path.DirectorySeparatorChar.ToString();
            else
                result = subName.Substring(baseName.Length);

            if (result[0] == Path.DirectorySeparatorChar)
                result = result.Substring(1);

            return result;
        }
    }
}
