using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    public partial class StartMenu : Form
    {
        public StartMenu()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Game gameForm = new Game();
            Settings setForm = new Settings();
            this.Close();
            gameForm.Close();
            setForm.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game gameForm = new Game();
            this.Hide();
            gameForm.Show();
            gameForm.FormClosed += (s, args) => this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Settings setForm = new Settings();
            setForm.Show();
        }
    }
}
