using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCdCopy
{
    class SizeHumanizator
    {
        public static string GetHumanReadableSizeFromBytes(long bytes)
        {
            string sizeSuffix;
            double unitsCount = bytes;
            int divideCount = 0;

            while (unitsCount >= 1024)
            {
                unitsCount = unitsCount / 1024;
                divideCount++;
            }

            sizeSuffix = GetHumanSizeSuffixByRange(divideCount);

            return String.Format("{0:0.00}{1}", unitsCount, sizeSuffix);
        }

        public static string GetHumanSizeSuffixByRange(int range)
        {
            switch (range)
            {
                case 0:
                    return "B";
                case 1:
                    return "KB";
                case 2:
                    return "MB";
                case 3:
                    return "GB";
                case 4:
                    return "TB";
                default:
                    throw new Exception();
            }
        }
    }
}
