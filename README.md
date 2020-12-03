# WegEx
This tool terminates all WebEx background processes the elegant way.

## Auto Quit
- At the end of a meeting, all background tasks are automatically killed.
- If WebEx is idle for longer than 10 minutes, a notification will appear in the Windows Notifications.
- If no WebEx processes are detected for 30 minutes, the user is asked to quit this app. (This is not WebEx, we do not want any background processes.)

## How it works
- The process "atmgr" is used to display the meeting window itself. If the window title equals "Cisco Webex Meetings", we expect the process to be a real meeting.
- Make sure to enable Windows Notifications, as the whole communication requires these.
- This tool requires .Net Framework 4.7.2 and Windows 10 to work.