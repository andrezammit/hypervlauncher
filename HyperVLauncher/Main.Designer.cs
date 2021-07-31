
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlVirtualMachines = new System.Windows.Forms.Panel();
            this.pnlNav = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btnSettings);
            this.panel1.Controls.Add(this.btnVirtualMachines);
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
            this.btnSettings.Location = new System.Drawing.Point(0, 186);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnSettings.Size = new System.Drawing.Size(186, 42);
            this.btnSettings.TabIndex = 3;
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
            this.btnVirtualMachines.Location = new System.Drawing.Point(0, 144);
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
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.button1.Location = new System.Drawing.Point(0, 228);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.button1.Size = new System.Drawing.Size(186, 42);
            this.button1.TabIndex = 2;
            this.button1.Text = "Virtual Machines";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.ClientSize = new System.Drawing.Size(951, 577);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hyper-V Launcher";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
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
        private System.Windows.Forms.Button button1;
    }
}

