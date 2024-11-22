using System;
using System.ComponentModel;

namespace Store_EF.Handlers
{
    public class BackgroudHandler
    {
        BackgroundWorker clock = new BackgroundWorker();

        Action action;

        // Trigger every day at specific time
        public BackgroudHandler(Action action)
        {
            this.action = action;

            clock.InitializeLifetimeService();
            clock.WorkerSupportsCancellation = true;
            clock.DoWork += Worker;
        }

        private void Worker(object sender, DoWorkEventArgs e)
        {
            while (!clock.CancellationPending)
            {
                action();
            }
        }

        public void Start()
        {
            clock.RunWorkerAsync();
        }

        public void Stop()
        {
            clock.CancelAsync();
        }
    }
}