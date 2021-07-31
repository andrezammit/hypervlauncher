
namespace HyperVLauncher
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnVirtualMachines = new System.Windows.Forms.Button();
            this.btnShortcuts = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlVirtualMachines = new System.Windows.Forms.Panel();
            this.pnlNav = new System.Windows.Forms.Panel();
            this.lstVirtualMachines = new HyperVLauncher.DoubleBufferedListView();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnLaunch = new System.Windows.Forms.Button();
            this.btnCreateShortcut = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.panel1.Controls.Add(this.btnSettings);
            this.panel1.Controls.Add(this.btnVirtualMachines);
            this.panel1.Controls.Add(this.btnShortcuts);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(186, 577);
            this.panel1.TabIndex = 0;
            // 
            // btnSettings
            // 
            this.btnSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSettings.Location = new System.Drawing.Point(0, 228);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnSettings.Size = new System.Drawing.Size(186, 42);
            this.btnSettings.TabIndex = 2;
            this.btnSettings.Text = "Settings";
            this.btnSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            this.btnSettings.Leave += new System.EventHandler(this.btnSettings_Leave);
            // 
            // btnVirtualMachines
            // 
            this.btnVirtualMachines.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnVirtualMachines.FlatAppearance.BorderSize = 0;
            this.btnVirtualMachines.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVirtualMachines.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnVirtualMachines.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnVirtualMachines.Location = new System.Drawing.Point(0, 186);
            this.btnVirtualMachines.Name = "btnVirtualMachines";
            this.btnVirtualMachines.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnVirtualMachines.Size = new System.Drawing.Size(186, 42);
            this.btnVirtualMachines.TabIndex = 1;
            this.btnVirtualMachines.Text = "Virtual Machines";
            this.btnVirtualMachines.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnVirtualMachines.UseVisualStyleBackColor = true;
            this.btnVirtualMachines.Click += new System.EventHandler(this.btnVirtualMachines_Click);
            this.btnVirtualMachines.Leave += new System.EventHandler(this.btnVirtualMachines_Leave);
            // 
            // btnShortcuts
            // 
            this.btnShortcuts.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnShortcuts.FlatAppearance.BorderSize = 0;
            this.btnShortcuts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShortcuts.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnShortcuts.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnShortcuts.Location = new System.Drawing.Point(0, 144);
            this.btnShortcuts.Name = "btnShortcuts";
            this.btnShortcuts.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnShortcuts.Size = new System.Drawing.Size(186, 42);
            this.btnShortcuts.TabIndex = 3;
            this.btnShortcuts.Text = "Shortcuts";
            this.btnShortcuts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnShortcuts.UseVisualStyleBackColor = true;
            this.btnShortcuts.Click += new System.EventHandler(this.btnShortcuts_Click);
            this.btnShortcuts.MouseLeave += new System.EventHandler(this.btnShortcuts_MouseLeave);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pnlVirtualMachines);
            this.panel2.Controls.Add(this.pnlNav);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(186, 144);
            this.panel2.TabIndex = 0;
            // 
            // pnlVirtualMachines
            // 
            this.pnlVirtualMachines.Location = new System.Drawing.Point(192, 0);
            this.pnlVirtualMachines.Name = "pnlVirtualMachines";
            this.pnlVirtualMachines.Size = new System.Drawing.Size(759, 577);
            this.pnlVirtualMachines.TabIndex = 1;
            // 
            // pnlNav
            // 
            this.pnlNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.pnlNav.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pnlNav.Location = new System.Drawing.Point(0, 193);
            this.pnlNav.Name = "pnlNav";
            this.pnlNav.Size = new System.Drawing.Size(3, 100);
            this.pnlNav.TabIndex = 0;
            // 
            // lstVirtualMachines
            // 
            this.lstVirtualMachines.AutoArrange = false;
            this.lstVirtualMachines.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.lstVirtualMachines.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstVirtualMachines.Font = new System.Drawing.Font("Nirmala UI Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstVirtualMachines.ForeColor = System.Drawing.SystemColors.Window;
            this.lstVirtualMachines.FullRowSelect = true;
            this.lstVirtualMachines.Location = new System.Drawing.Point(228, 82);
            this.lstVirtualMachines.MultiSelect = false;
            this.lstVirtualMachines.Name = "lstVirtualMachines";
            this.lstVirtualMachines.OwnerDraw = true;
            this.lstVirtualMachines.Size = new System.Drawing.Size(483, 495);
            this.lstVirtualMachines.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstVirtualMachines.TabIndex = 0;
            this.lstVirtualMachines.TileSize = new System.Drawing.Size(450, 50);
            this.lstVirtualMachines.UseCompatibleStateImageBehavior = false;
            this.lstVirtualMachines.View = System.Windows.Forms.View.Tile;
            this.lstVirtualMachines.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lstVirtualMachines_DrawItem);
            this.lstVirtualMachines.SelectedIndexChanged += new System.EventHandler(this.lstVirtualMachines_SelectedIndexChanged);
            this.lstVirtualMachines.MouseLeave += new System.EventHandler(this.lstVirtualMachines_MouseLeave);
            this.lstVirtualMachines.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lstVirtualMachines_MouseMove);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Nirmala UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.Window;
            this.label1.Location = new System.Drawing.Point(228, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Virtual Machines";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnCreateShortcut);
            this.panel3.Controls.Add(this.btnLaunch);
            this.panel3.Location = new System.Drawing.Point(717, 82);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(200, 495);
            this.panel3.TabIndex = 2;
            // 
            // btnLaunch
            // 
            this.btnLaunch.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnLaunch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLaunch.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnLaunch.ForeColor = System.Drawing.Color.White;
            this.btnLaunch.Location = new System.Drawing.Point(0, 0);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnLaunch.Size = new System.Drawing.Size(200, 42);
            this.btnLaunch.TabIndex = 5;
            this.btnLaunch.Text = "Launch";
            this.btnLaunch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLaunch.UseVisualStyleBackColor = true;
            // 
            // btnCreateShortcut
            // 
            this.btnCreateShortcut.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCreateShortcut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateShortcut.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnCreateShortcut.ForeColor = System.Drawing.Color.White;
            this.btnCreateShortcut.Location = new System.Drawing.Point(0, 42);
            this.btnCreateShortcut.Name = "btnCreateShortcut";
            this.btnCreateShortcut.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnCreateShortcut.Size = new System.Drawing.Size(200, 42);
            this.btnCreateShortcut.TabIndex = 4;
            this.btnCreateShortcut.Text = "Create Shortcut";
            this.btnCreateShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCreateShortcut.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.ClientSize = new System.Drawing.Size(951, 577);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstVirtualMachines);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hyper-V Launcher";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnVirtualMachines;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Panel pnlNav;
        private System.Windows.Forms.Panel pnlVirtualMachines;
        private DoubleBufferedListView lstVirtualMachines;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnShortcuts;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnCreateShortcut;
        private System.Windows.Forms.Button btnLaunch;
    }
}

