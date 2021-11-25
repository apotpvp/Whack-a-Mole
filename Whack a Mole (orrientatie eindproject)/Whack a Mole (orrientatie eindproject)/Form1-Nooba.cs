using System;
using System.Windows.Forms;

namespace Whack_a_Mole__orrientatie_eindproject_
{
    public partial class Form1 : Form
    {
        private Random generator = new Random();
        int locationNum = 0;
        int seconden = 0;
        private int interval = 1000;
        public static int score = 0;
        static int[] ScoreArray = new int[2];
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = interval;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            seconden = 10;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Start();

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            label1.Text = "5";
            label2.Text = "0";

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = seconden--.ToString();



            switch (locationNum)
            {
                case 1:
                    btnTarget.Left = 407;
                    btnTarget.Top = 330;
                    break;

                case 2:
                    btnTarget.Left = 157;
                    btnTarget.Top = 306;
                    break;

                case 3:
                    btnTarget.Left = 404;
                    btnTarget.Top = 478;
                    break;

                case 4:
                    btnTarget.Left = 673;
                    btnTarget.Top = 302;
                    break;

                case 5:
                    btnTarget.Left = 402;
                    btnTarget.Top = 130;
                    break;

                default:
                    break;
            }
            if (seconden == 0)
            {
                gameover();
            }
            else
                locationNum = generator.Next(1, 6);
        }
        private void btnTarget_Click(object sender, EventArgs e)
        {
            score++;
            label2.Text = score.ToString();
        }
        void gameover()
        {
            Form2 F2 = new Form2();
            timer1.Stop();
            ScoreArray[1] = score;
            F2.Show();
            this.Hide();
        }
    }
}