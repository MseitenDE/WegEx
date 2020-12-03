using System;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace WegEx
{
    internal static class NotificationManager
    {
        private static ToastNotifier _notifier;

        internal static void Init()
        {
            _notifier = ToastNotificationManagerCompat.CreateToastNotifier();
            ToastNotificationManagerCompat.OnActivated += OnToastButtonClicked;
        }

        internal static void ShowToast(string title, string description)
        {
            SendToast(new ToastContentBuilder().AddText(title).AddText(description));
        }

        private static void ShowToast(string title, string description, string button, string result)
        {
            SendToast(new ToastContentBuilder()
                .AddText(title)
                .AddText(description)
                .AddButton(button, ToastActivationType.Background, result)
                .AddButton("Ignore", ToastActivationType.Background, "dismiss")
            );
        }
        
        internal static void ShowIdle(int min)
        {
            ShowToast($"No WebEx instance detected for {min} minutes.", "Do you want to quit this app?", "Exit", "exit");
        }

        internal static void ShowSuccess()
        {
            ShowToast("Background tasks successfully killed.", "Do you want to quit this app?", "Exit", "exit");
        }

        internal static void ShowBackgroudTasksToasts(int count)
        {
            ShowToast($"{count} backgound tasks found.", "Do you want me to kill them for you?", "Clean up", "kill");
        }

        private static void SendToast(ToastContentBuilder builder)
        {
            var toast = new ToastNotification(builder.Content.GetXml());
            _notifier.Show(toast);
        }

        private static void OnToastButtonClicked(ToastNotificationActivatedEventArgsCompat e)
        {
            switch (e.Argument)
            {
                case "kill":
                    Program.Kill();
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
            }
        }
    }
}