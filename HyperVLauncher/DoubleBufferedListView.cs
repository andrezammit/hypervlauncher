using System.Windows.Forms;

namespace HyperVLauncher
{
    internal class DoubleBufferedListView : ListView
    {
        public DoubleBufferedListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            // Filter out the WM_ERASEBKGND message.

            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }
}
