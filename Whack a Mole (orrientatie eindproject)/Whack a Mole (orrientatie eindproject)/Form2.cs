using System;
using System.Windows.Forms;

namespace Whack_a_Mole__orrientatie_eindproject_
{
    public partial class Form2 : Form
    {

        public int[] ScoreArray = new int[2];
        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            lbScoredisplay.Text = ScoreArray[1].ToString();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }
}
