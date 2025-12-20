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
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            this.Load += Settings_Load;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }
        private void Settings_Load(object sender, EventArgs e)
        {
            InitializeSpeedComboBox();
            InitializeModeComboBox();
            InitializeRenderStyleComboBox();
            InitializeFruitViewComboBox();
            InitializeMusicSetting();
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            this.comboBox4.SelectedIndexChanged += new System.EventHandler(this.comboBox4_SelectedIndexChanged);
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
        }
        private void InitializeSpeedComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "Slow", "Normal", "Fast", "Insane" });

            int currentInterval = GlobalSettings.GameTimeInterval;

            if (currentInterval >= 200) comboBox1.SelectedIndex = 0;
            else if (currentInterval == 150) comboBox1.SelectedIndex = 1;
            else if (currentInterval == 100) comboBox1.SelectedIndex = 2;
            else if (currentInterval <= 50) comboBox1.SelectedIndex = 3;
            else comboBox1.SelectedIndex = 1;
        }
        private void InitializeModeComboBox()
        {
            comboBox2.Items.Clear();
            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                comboBox2.Items.Add(mode.ToString());
            }
            comboBox2.SelectedItem = GlobalSettings.SelectedMode.ToString();
        }
        private void InitializeRenderStyleComboBox()
        {
            comboBox3.Items.Clear();
            foreach (RenderStyle style in Enum.GetValues(typeof(RenderStyle)))
            {
                comboBox3.Items.Add(style.ToString());
            }
            comboBox3.SelectedItem = GlobalSettings.CurrentRenderStyle.ToString();
        }
        private void InitializeFruitViewComboBox()
        {
            comboBox4.Items.Clear();
            foreach (FruitView mode in Enum.GetValues(typeof(FruitView)))
            {
                comboBox4.Items.Add(mode.ToString());
            }
            comboBox4.SelectedItem = GlobalSettings.SelectedMode.ToString();
        }
        private void InitializeMusicSetting()
        {
            checkBox1.Checked = GlobalSettings.IsMusicEnabled;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    GlobalSettings.GameTimeInterval = 200;
                    break;
                case 1:
                    GlobalSettings.GameTimeInterval = 150;
                    break;
                case 2:
                    GlobalSettings.GameTimeInterval = 100;
                    break;
                case 3:
                    GlobalSettings.GameTimeInterval = 50;
                    break;
                default:
                    GlobalSettings.GameTimeInterval = 150;
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                string selected = comboBox2.SelectedItem.ToString();
                GlobalSettings.SelectedMode = (GameMode)Enum.Parse(typeof(GameMode), selected);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem != null)
            {
                string selected = comboBox3.SelectedItem.ToString();
                GlobalSettings.CurrentRenderStyle = (RenderStyle)Enum.Parse(typeof(RenderStyle), selected);
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedItem != null)
            {
                string selected = comboBox4.SelectedItem.ToString();
                GlobalSettings.CurrentFruitStyle = (FruitView)Enum.Parse(typeof(FruitView), selected);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            GlobalSettings.IsMusicEnabled = checkBox1.Checked;
        }
    }
}