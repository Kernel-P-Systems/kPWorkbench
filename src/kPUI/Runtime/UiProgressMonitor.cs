using KpExperiment.Verification.Runtime;
using KpUi.Simulation.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpUi.Runtime
{
    public class UiProgressMonitor : ISimulatorProgressMonitor, IModelCheckingProgressMonitor
    {
        private Label lMessageProgressMonitor;
        private Label lDetailsProgressMonitor;
        private ProgressBar pbProgressMonitor;

        public UiProgressMonitor(Label lMessageProgressMonitor, Label lDetailsProgressMonitor, ProgressBar pbProgressMonitor)
        {
            this.lMessageProgressMonitor = lMessageProgressMonitor;
            this.lDetailsProgressMonitor = lDetailsProgressMonitor;
            this.pbProgressMonitor = pbProgressMonitor;

            lMessageProgressMonitor.Invoke(new MethodInvoker(() => lMessageProgressMonitor.ForeColor = System.Drawing.Color.Black));
            lDetailsProgressMonitor.Invoke(new MethodInvoker(() => lDetailsProgressMonitor.ForeColor = System.Drawing.Color.Black));

            this.lMessageProgressMonitor.Visible = true;
            this.pbProgressMonitor.Visible = true;
            this.lDetailsProgressMonitor.Visible = true;
        }
        public void Start(int taskCount, string description)
        {
            lMessageProgressMonitor.Invoke(new MethodInvoker(() => lMessageProgressMonitor.Text = description));
            pbProgressMonitor.Invoke(new MethodInvoker(() => pbProgressMonitor.Maximum = taskCount));
            pbProgressMonitor.Invoke(new MethodInvoker(() => pbProgressMonitor.Value = 0));
        }

        public void LogProgress(int currentTask, string description)
        {
            lDetailsProgressMonitor.Invoke(new MethodInvoker(() => lDetailsProgressMonitor.Text = description));
            pbProgressMonitor.Invoke(new MethodInvoker(() => pbProgressMonitor.Value = currentTask));
        }

        public void Done(string description)
        {
            lDetailsProgressMonitor.Invoke(new MethodInvoker(() => lDetailsProgressMonitor.Text = description));
            pbProgressMonitor.Invoke(new MethodInvoker(() => pbProgressMonitor.Value = pbProgressMonitor.Maximum));
        }

        public void Terminate(string description)
        {
            lDetailsProgressMonitor.Invoke(new MethodInvoker(() => lDetailsProgressMonitor.Text = description));
            lMessageProgressMonitor.Invoke(new MethodInvoker(() => lMessageProgressMonitor.ForeColor = System.Drawing.Color.Red));
            lDetailsProgressMonitor.Invoke(new MethodInvoker(() => lDetailsProgressMonitor.ForeColor = System.Drawing.Color.Red));
            pbProgressMonitor.Invoke(new MethodInvoker(() => pbProgressMonitor.Value = pbProgressMonitor.Maximum));
        }
    }
}
