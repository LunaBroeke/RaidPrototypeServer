namespace RaidPrototypeServer
{
    partial class MapScreen
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
            MapPanel = new Panel();
            SuspendLayout();
            // 
            // MapPanel
            // 
            MapPanel.BackColor = Color.Black;
            MapPanel.Dock = DockStyle.Fill;
            MapPanel.Location = new Point(0, 0);
            MapPanel.Name = "MapPanel";
            MapPanel.Size = new Size(782, 553);
            MapPanel.TabIndex = 0;
            MapPanel.Paint += MapPanel_Paint;
            // 
            // MapScreen
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 553);
            Controls.Add(MapPanel);
            Name = "MapScreen";
            Text = "MapScreen";
            Load += MapScreen_Load;
            ResumeLayout(false);
        }

        #endregion

        private Panel MapPanel;
    }
}