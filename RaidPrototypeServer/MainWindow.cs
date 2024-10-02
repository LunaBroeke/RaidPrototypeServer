using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RaidPrototypeServer
{
    public partial class MainWindow : Form
    {
        public static Server server = new Server() { maxPlayers = 4, port = 2051 };
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Logger.main = this;
            PlayerListMonitor.main = this;
            InfoMonitor.main = this;
            ResourceMonitor.main = this;
            Thread monitorThread = new Thread(ResourceMonitor.WriteResourceMonitor) { IsBackground = true };
            Thread serverThread = new Thread(server.StartServer) { IsBackground = true };
            monitorThread.Start();
            serverThread.Start();
        }

        private void resources_Enter(object sender, EventArgs e)
        {

        }
        private void PlayerListBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the character index from the mouse click position
                int charIndex = PlayerListBox.GetCharIndexFromPosition(e.Location);

                // Get the line index from the character index
                int lineIndex = PlayerListBox.GetLineFromCharIndex(charIndex);

                // Get the text of the clicked line
                string clickedLine = null;
                try
                {
                    clickedLine = PlayerListBox.Lines[lineIndex];
                }
                catch { }

                // Perform some action based on the clicked line
                Server.logger.Log($"You clicked on line {lineIndex}: {clickedLine}");
                InfoMonitor.WriteInfo(Server.players[lineIndex]);
            }
            catch (ArgumentOutOfRangeException ex) { Server.logger.LogWarning("Unable to load Info on an empty player list"); }

        }

        private void ConsoleInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string m = ConsoleInput.Text.ToLower();
                new Thread(() => { ConsoleCommandHandler.ProcessCommand(m); }).Start();
                ConsoleInput.Clear();
            }
        }
    }
}
