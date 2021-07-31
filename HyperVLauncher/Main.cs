using System;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace HyperVLauncher
{
    public partial class Main : Form
    {
        private ListViewItem? _hoveredItem;

        public Main()
        {
            InitializeComponent();

            lstVirtualMachines.Columns.Add("VM Name");

            GetVirtualMachines();
        }

        private void btnVirtualMachines_Click(object sender, EventArgs e)
        {
            OnNavButtonClick(btnVirtualMachines);

            GetVirtualMachines();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            OnNavButtonClick(btnSettings);
        }

        private void OnNavButtonClick(Button navButton)
        {
            pnlNav.Height = navButton.Height;
            pnlNav.Top = navButton.Top;
            pnlNav.Left = navButton.Left;

            navButton.BackColor = Color.FromArgb(46, 51, 73);
        }

        private void OnNavButtonLeave(Button navButton)
        {
            navButton.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void btnVirtualMachines_Leave(object sender, EventArgs e)
        {
            OnNavButtonLeave(btnVirtualMachines);
        }

        private void btnSettings_Leave(object sender, EventArgs e)
        {
            OnNavButtonLeave(btnSettings);
        }

        private void lstVirtualMachines_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void GetVirtualMachines()
        {
            var scope = new ManagementScope("\\\\.\\root\\virtualization\\v2");
            scope.Connect();

            var query = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem");

            using var searcher = new ManagementObjectSearcher(scope, query);

            foreach (var queryObj in searcher.Get())
            {
                var vmName = queryObj["ElementName"].ToString();
                lstVirtualMachines.Items.Add(vmName);
            }
        }

        private void lstVirtualMachines_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var itemRect = e.Bounds;
            itemRect.Inflate(-1, -1);

            if (e.Item.Selected || _hoveredItem == e.Item)
            {
                using var brush = new SolidBrush(Color.FromArgb(255, 24, 30, 54));
                e.Graphics.FillRectangle(brush, itemRect);
            }

            var verticalOffset = 12;
            var horizontalOffset = 5;

            var font = ((ListView)sender).Font;

            using var sf = new StringFormat();

            itemRect.Y += verticalOffset;
            itemRect.X += horizontalOffset;

            e.Graphics.DrawString(
                e.Item.SubItems[0].Text,
                font,
                Brushes.White,
                itemRect,
                sf);
        }

        private void lstVirtualMachines_MouseMove(object sender, MouseEventArgs e)
        {
            var hoveredItem = lstVirtualMachines.HitTest(e.Location).Item;

            if (hoveredItem != _hoveredItem)
            {
                _hoveredItem = hoveredItem;
                lstVirtualMachines.Invalidate();
            }
        }

        private void lstVirtualMachines_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredItem != null)
            {
                _hoveredItem = null;
                lstVirtualMachines.Invalidate();
            }
        }
    }
}
