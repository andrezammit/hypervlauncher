using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Models
{
    public class Shortcut
    {
        public bool RemoteTriggerEnabled { get; set; }

        public int ListenPort { get; set; }
        public int RemotePort { get; set; }

        public string Id { get; init; }
        public string VmId { get; init; }
        public string Name { get; set; }

        public CloseAction CloseAction { get; set; }

        public Shortcut(
            string id,
            string vmId,
            string name,
            bool remoteTriggerEnabled = false,
            int listenPort = 0,
            int remotePort = 0,
            CloseAction closeAction = CloseAction.None)
        {
            Id = id;
            VmId = vmId;
            Name = name;
            ListenPort = listenPort;
            RemotePort = remotePort;
            CloseAction = closeAction;
            RemoteTriggerEnabled = remoteTriggerEnabled;
        }
    }
}
