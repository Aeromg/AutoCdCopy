using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCdCopyWinformsTest
{
    public partial class Form1 : Form
    {
        CopyWorker _worker;
        bool _isBusy;
        public Form1()
        {
            InitializeComponent();

            _worker = new Test.Tools.CopyWorker("J:\\Tmp\\emu0204", "J:\\Tmp\\1");
            _worker.AsyncProgress += _worker_AsyncProgress;
            progressBar1.Maximum = 100;
        }

        void _worker_AsyncProgress(object sender, CopyWorker.CopyWorkerState e)
        {
            switch (e.Stage)
            {
                case CopyWorkerProgressStage.FileCopy:
                    progressBar1.Value = (int)Math.Floor(e.Progress * 100);
                    break;
                case CopyWorkerProgressStage.FileCopyDone:
                    listBox1.Items.Add("Done. [" + e.CurrentFilePath + "]");
                    break;
                case CopyWorkerProgressStage.DirectoriesReview:
                    listBox1.Items.Add("Directories review");
                    break;
                case CopyWorkerProgressStage.FileSystemMirroring:
                    listBox1.Items.Add("FS mirroring");
                    break;
                case CopyWorkerProgressStage.Done:
                    listBox1.Items.Add("Done.");
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            _isBusy = true;
            _worker.BeginCopy();
        }
    }
}
