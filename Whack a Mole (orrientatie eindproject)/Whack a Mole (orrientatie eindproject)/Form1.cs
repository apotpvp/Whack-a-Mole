using System;
using System.Windows.Forms;

namespace Whack_a_Mole__orrientatie_eindproject_
{
    
    public partial class Form1 : Form
    {
        private Random generator = new Random();
        int locationNum = 0;
        int bom = 0;
        int seconden = 0;
        int i = 0;
        private int interval = 1000;
        public static int score = 0;
        public int[] ScoreArray = new int[2];
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = interval;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            seconden = 60;
            btnTarget.Visible = false;
            btnBom.Visible = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Start();
            btnTarget.Visible = true;
            btnBom.Visible = true;
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
            if (seconden == 0)
            {
                gameover();
            }
            else
                while (i < 3)
                {
                    locationNum = generator.Next(1, 6);
                    bom = generator.Next(1, 7);
                    i++;
                }

            if (i == 3) {
                i = 0;
            }
            
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
            switch (bom)
            {
                case 1:
                    btnBom.Left = 407;
                    btnBom.Top = 330;
                    break;
                case 2:
                    btnBom.Left = 157;
                    btnBom.Top = 306;
                    break;
                case 3:
                    btnBom.Left = 404;
                    btnBom.Top = 478;
                    break;
                case 4:
                    btnBom.Left = 674;
                    btnBom.Top = 302;
                    break;
                case 5:
                    btnBom.Left = 402;
                    btnBom.Top = 130;
                    break;
                 case 6:
                    btnBom.Left = 4007;
                    btnBom.Top = 3300;
                    break;
                default:
                    break;
            }
        }
        private void btnTarget_Click(object sender, EventArgs e)
        { 
            score++;
            label2.Text = score.ToString();
            ScoreArray[1] = score;   
        }

        private void btnBom_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Form3 F3 = new Form3();
            F3.Show();
            
        }
        void gameover()
        {
            timer1.Stop();
            Form2 F2 = new Form2();
            F2.ScoreArray = ScoreArray;
            F2.ShowDialog();
            this.Close();        
        }
    }
}