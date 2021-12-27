
namespace HyperVLauncher.Contracts.Models
{
    public class TrayMessageData
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public TrayMessageData(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
