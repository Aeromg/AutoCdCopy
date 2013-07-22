using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADev.FileSystemTools;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var tool = new Test.Tools.CopyWorker("J:\\Tmp\\emu0204", "J:\\Tmp\\1");
            tool.AsyncProgress += tool_AsyncProgress;
            tool.BeginCopy();
            //tool.Start();

            Console.ReadKey();
            tool.EndCopy();
            Console.ReadKey();
        }

        static void tool_AsyncProgress(object sender, Tools.CopyWorker.CopyWorkerState e)
        {
            
            //if (e.Stage != Tools.CopyWorkerProgressStage.FileCopyDone)
            //    return;

            Console.Clear();
            Console.WriteLine(String.Format("{0:0.00}%", e.Progress * 100));
            //Console.WriteLine(e.FilesCopiedCount);
        }
    }
}
