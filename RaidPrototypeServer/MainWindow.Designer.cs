namespace RaidPrototypeServer
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ConsoleBox = new TextBox();
            ConsoleInput = new TextBox();
            resources = new GroupBox();
            ResourcesBox = new TextBox();
            playerList = new GroupBox();
            PlayerListBox = new TextBox();
            selectInfo = new GroupBox();
            InfoBox = new TextBox();
            resources.SuspendLayout();
            playerList.SuspendLayout();
            selectInfo.SuspendLayout();
            SuspendLayout();
            // 
            // ConsoleBox
            // 
            ConsoleBox.AcceptsReturn = true;
            ConsoleBox.AcceptsTab = true;
            ConsoleBox.BackColor = SystemColors.Window;
            ConsoleBox.Location = new Point(6, 12);
            ConsoleBox.Multiline = true;
            ConsoleBox.Name = "ConsoleBox";
            ConsoleBox.ReadOnly = true;
            ConsoleBox.ScrollBars = ScrollBars.Vertical;
            ConsoleBox.Size = new Size(685, 526);
            ConsoleBox.TabIndex = 0;
            // 
            // ConsoleInput
            // 
            ConsoleInput.Location = new Point(6, 544);
            ConsoleInput.Name = "ConsoleInput";
            ConsoleInput.Size = new Size(685, 27);
            ConsoleInput.TabIndex = 1;
            ConsoleInput.KeyDown += ConsoleInput_KeyDown;
            // 
            // resources
            // 
            resources.Controls.Add(ResourcesBox);
            resources.Location = new Point(697, 6);
            resources.Name = "resources";
            resources.Size = new Size(400, 186);
            resources.TabIndex = 2;
            resources.TabStop = false;
            resources.Text = "Resources";
            resources.Enter += resources_Enter;
            // 
            // ResourcesBox
            // 
            ResourcesBox.AcceptsReturn = true;
            ResourcesBox.AcceptsTab = true;
            ResourcesBox.BackColor = SystemColors.Control;
            ResourcesBox.Location = new Point(6, 26);
            ResourcesBox.Multiline = true;
            ResourcesBox.Name = "ResourcesBox";
            ResourcesBox.ReadOnly = true;
            ResourcesBox.Size = new Size(388, 154);
            ResourcesBox.TabIndex = 0;
            // 
            // playerList
            // 
            playerList.Controls.Add(PlayerListBox);
            playerList.Location = new Point(697, 198);
            playerList.Name = "playerList";
            playerList.Size = new Size(400, 195);
            playerList.TabIndex = 3;
            playerList.TabStop = false;
            playerList.Text = "PlayerList";
            // 
            // PlayerListBox
            // 
            PlayerListBox.AcceptsReturn = true;
            PlayerListBox.AcceptsTab = true;
            PlayerListBox.Location = new Point(6, 26);
            PlayerListBox.Multiline = true;
            PlayerListBox.Name = "PlayerListBox";
            PlayerListBox.ReadOnly = true;
            PlayerListBox.ScrollBars = ScrollBars.Vertical;
            PlayerListBox.Size = new Size(388, 163);
            PlayerListBox.TabIndex = 0;
            PlayerListBox.MouseDown += PlayerListBox_MouseClick;
            // 
            // selectInfo
            // 
            selectInfo.Controls.Add(InfoBox);
            selectInfo.Location = new Point(697, 399);
            selectInfo.Name = "selectInfo";
            selectInfo.Size = new Size(400, 172);
            selectInfo.TabIndex = 4;
            selectInfo.TabStop = false;
            selectInfo.Text = "Info";
            // 
            // InfoBox
            // 
            InfoBox.AcceptsReturn = true;
            InfoBox.AcceptsTab = true;
            InfoBox.Location = new Point(6, 26);
            InfoBox.Multiline = true;
            InfoBox.Name = "InfoBox";
            InfoBox.ReadOnly = true;
            InfoBox.ScrollBars = ScrollBars.Vertical;
            InfoBox.Size = new Size(388, 140);
            InfoBox.TabIndex = 0;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1123, 583);
            Controls.Add(selectInfo);
            Controls.Add(playerList);
            Controls.Add(resources);
            Controls.Add(ConsoleInput);
            Controls.Add(ConsoleBox);
            Name = "MainWindow";
            Text = "Raid Prototype Server";
            Load += MainWindow_Load;
            resources.ResumeLayout(false);
            resources.PerformLayout();
            playerList.ResumeLayout(false);
            playerList.PerformLayout();
            selectInfo.ResumeLayout(false);
            selectInfo.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public static TextBox textBox1;
        public TextBox ConsoleInput;
        public TextBox ConsoleBox;
        public GroupBox resources;
        public GroupBox playerList;
        public GroupBox selectInfo;
        public TextBox PlayerListBox;
        public TextBox ResourcesBox;
        public TextBox InfoBox;
    }
}