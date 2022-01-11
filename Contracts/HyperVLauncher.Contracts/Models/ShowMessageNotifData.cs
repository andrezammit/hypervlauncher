
namespace HyperVLauncher.Contracts.Models
{
    public class ShowMessageNotifData
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public ShowMessageNotifData(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }

    public class ShortcutCreatedNotifData
    {
        public string VmId { get; private set; }
        public string ShortcutName { get; private set; }

        public ShortcutCreatedNotifData(string vmId, string shortcutName)
        {
            VmId = vmId;
            ShortcutName = shortcutName;
        }
    }

    public class ShortcutPromptNotifData
    {
        public string VmId { get; private set; }
        public string VmName { get; private set; }

        public ShortcutPromptNotifData(string vmId, string vmName)
        {
            VmId = vmId;
            VmName = vmName;
        }
    }
}
