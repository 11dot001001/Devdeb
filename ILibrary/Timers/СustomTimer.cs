using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace ILibrary.Timers
{
    //public class СustomTimer : IDisposable
    //{
    //    private readonly Timer timer;
    //    private readonly bool isRepeat;

    //    public СustomTimer(float period, bool isRepeat)
    //    {
    //        timer = new Timer(period);
    //        timer.Elapsed += Timer_Elapsed;

    //        this.isRepeat = isRepeat;
    //    }

    //    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    //    {
    //        if (!isRepeat)
    //            Stop();
    //        action();
    //    }

    //    public void Start()
    //    {
    //        timer.Start();
    //    }
    //    public void Stop()
    //    {
    //        timer.Stop();
    //    }

    //    public void Dispose()
    //    {
    //        timer.Dispose();
    //    }
    //}
}
