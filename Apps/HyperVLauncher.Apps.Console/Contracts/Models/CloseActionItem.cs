using HyperVLauncher.Contracts.Enums;

namespace HyperVLauncher.Contracts.Models
{
    public class CloseActionItem
    {
        public CloseAction CloseAction { get; private set; }

        private readonly string _actionText;

        public CloseActionItem(CloseAction closeAction, string actionText)
        {
            CloseAction = closeAction;

            _actionText = actionText;
        }

        public override string ToString()
        {
            return _actionText;
        }
    }
}
