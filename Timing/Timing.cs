using System.Diagnostics;

namespace Test
{
    public class Timing
    {
        TimeSpan startTime;
        TimeSpan endTime;

        public Timing()
        {
            startTime = new TimeSpan(0);
            endTime = new TimeSpan(0);
        }

        public void StopTime()
        {
            endTime = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public void StartTime()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            startTime = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public TimeSpan Result()
        {
            return endTime - startTime;
        }
    }
}
