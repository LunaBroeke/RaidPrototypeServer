using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
namespace RaidPrototypeServer
{
    public partial class MapScreen : Form
    {
        private Timer refreshTimer;
        private const int squareSize = 10;

        public MapScreen()
        {
            InitializeComponent();
        }

        private void MapScreen_Load(object sender, EventArgs e)
        {
            refreshTimer = new Timer()
            {
                Interval = 16
            };
            refreshTimer.Tick += (s, e) => MapPanel.Invalidate();
            refreshTimer.Start();
        }

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                int centerX = MapPanel.Width / 2;
                int centerY = MapPanel.Height / 2;
                g.TranslateTransform(centerX, centerY);

                Font font = new Font("Arial", 20);
                Brush brush = Brushes.White;

                foreach (ServerPlayer player in Server.players)
                {
                    if (player.player != null)
                    {
                        float x = player.player.position.x * 10;
                        float y = player.player.position.z * 10;
                        g.FillRectangle(brush, x, y, squareSize, squareSize);

                        g.DrawString(player.name, font, brush, x, y - 20);
                    }
                }
            }
            catch
            { }
        }
    }
}
