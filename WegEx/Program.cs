using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Timer = System.Timers.Timer;

namespace WegEx
{
    public static class Program
    {
        private static int _openTime, _closedTime;
        private static Timer _scanner;

        public static void Main()
        {
            //Reduce the process priority to minimize performance impact.
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            NotificationManager.Init();

            //Launch Timer to check for WebEx instances every 60 seconds.
            //Does not display a notification every time.
            _scanner = new Timer(60000);
            _scanner.Elapsed += (_, _) => Scan(false);
            _scanner.Start();

            //Scan for the first time and display a notification if a WebEx instance is found.
            Scan(true);
            Thread.Sleep(-1);
        }

        /// <param name="instant">
        ///     Display a notification every time a WebEx instance is found, ignores other limitations
        /// </param>
        private static void Scan(bool instant)
        {
            var processes = GetMatchingProcesses(out var meeting);
            if (processes.Count > 0)
            {
                _closedTime = 0;
                if (meeting)
                {
                    //When the main window of the meeting window matches the specified name, the user is probably in a meeting.
                    var process = processes.Find(i => i.ProcessName == "atmgr" && i.MainWindowTitle == "Cisco Webex Meetings");
                    if (process == null) return;
                    
                    //Stop scanning for new processes if a meeting is detected.
                    _scanner.Stop();
                    _openTime = 0;
                        
                    NotificationManager.ShowToast("Meeting detected", "WebEx will be stopped as soon as you leave the meeting.");
                    //Wait for the meeting process to exit, which happens at the end of a meeting
                    process.WaitForExit();
                    Kill();
                        
                    _scanner.Start();
                }
                //Periodically remind the user of running WebEx processes every 10 minutes
                else if (instant || _openTime != 0 && _openTime % 10 == 0)
                {
                    NotificationManager.ShowBackgroudTasksToasts(processes.Count);
                    _openTime++;
                }
                else _openTime++;
            }
            else
            {
                _openTime = 0;
                _closedTime++;
                
                //If no WebEx instance is detected during 30 minutes, offer the user to quit this app. (This is not WebEx)
                if (_closedTime != 0 && _closedTime % 30 == 0)
                {
                    NotificationManager.ShowIdle(_closedTime);
                }
            }
        }

        /// <summary>
        ///     Kill all WebEx processes, ignoring if the user is in a meeting.
        /// </summary>
        internal static void Kill()
        {
            foreach (var process in GetMatchingProcesses(out _))
            {
                process.Kill();
            }

            //Display a notification and offer the user to quit this app.
            NotificationManager.ShowSuccess();
        }

        /// <summary>
        ///     Search for WebEx processes.
        /// </summary>
        /// <param name="meeting">
        ///     Returns true if the meeting process is running. This does not guarantee that the user is in a meeting.
        /// </param>
        private static List<Process> GetMatchingProcesses(out bool meeting)
        {
            var sMeeting = false;
            var result = Process.GetProcesses().Where(p =>
                {
                    return p.ProcessName switch
                    {
                        "atmgr" => sMeeting = true,     //The (important) meeting process of WebEx
                        "webexmta" => true,
                        "ptOIEx64" => true,
                        "ptSrv" => true,
                        "ptWbxONI" => true,
                        "ptoneclk" => true,
                        "ptMeetingsHost" => true,
                        "CiscoWebExStart" => true,
                        "ciscowebexstart" => true,
                        _ => false
                    };
                }
            ).ToList();

            meeting = sMeeting;
            return result;
        }
    }
}