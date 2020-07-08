using TCode_Remote.Library.Events;
using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Reference.Enum;
using System;
using System.Threading;

namespace TCode_Remote.Library.Tools
{
	public class Debouncer
    {
        public event EventHandler Idled = delegate { };
        public int WaitingMilliSeconds { get; set; }
        Timer waitingTimer;

        public Debouncer(int waitingMilliSeconds = 600)
        {
            WaitingMilliSeconds = waitingMilliSeconds;
            waitingTimer = new Timer(p =>
            {
                Idled(this, EventArgs.Empty);
            });
        }
        public void Change()
        {
            waitingTimer.Change(WaitingMilliSeconds, Timeout.Infinite);
        }
    }
}
