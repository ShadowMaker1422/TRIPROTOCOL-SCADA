using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MotorControl
{
    public partial class Main : Form
    {
        Image cabinet = Image.FromFile(@"images\ENERGY_METER.png");
        Image pump_green = Image.FromFile(@"images\pump_base_green.gif");
        Image pump_red = Image.FromFile(@"images\pump_base_red.gif");
        public Main()
        {
            InitializeComponent();
        }

        private void btMotor_1_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotor("Motor_1");
            if (motor != null) 
            { 
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotor_2_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotor("Motor_2");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotor_3_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotor("Motor_3");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }
        private void btMotor_4_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotor("Motor_4");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotor_5_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotor("Motor_5");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotor_6_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotor("Motor_6");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }
        private void btMeter_1_Click(object sender, EventArgs e)
        {
            Meter meter = Program.Root.FindMeter("Meter_1");
            if (meter != null) 
            {
                Meter_Faceplate fplt = new Meter_Faceplate(meter);
                fplt.Show();
            }
        }
        private void btMeter_2_Click(object sender, EventArgs e)
        {
            Meter meter = Program.Root.FindMeter("Meter_2");
            if (meter != null)
            {
                Meter_Faceplate fplt = new Meter_Faceplate(meter);
                fplt.Show();
            }
        }

        private void btMeter_3_Click(object sender, EventArgs e)
        {
            Meter meter = Program.Root.FindMeter("Meter_3");
            if (meter != null)
            {
                Meter_Faceplate fplt = new Meter_Faceplate(meter);
                fplt.Show();
            }
        }
        private void btMeter_4_Click(object sender, EventArgs e)
        {
            Meter meter = Program.Root.FindMeter("Meter_4");
            if (meter != null)
            {
                Meter_Faceplate fplt = new Meter_Faceplate(meter);
                fplt.Show();
            }
        }

        private void btMeter_5_Click(object sender, EventArgs e)
        {
            Meter meter = Program.Root.FindMeter("Meter_5");
            if (meter != null)
            {
                Meter_Faceplate fplt = new Meter_Faceplate(meter);
                fplt.Show();
            }
        }

        private void btMeter_6_Click(object sender, EventArgs e)
        {
            Meter meter = Program.Root.FindMeter("Meter_6");
            if (meter != null)
            {
                Meter_Faceplate fplt = new Meter_Faceplate(meter);
                fplt.Show();
            }
        }
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if(Program.Root.FindMotor("Motor_1").RunFeedback)
            {
                pbMotor_1.BackgroundImage = pump_green;
            }
            else
            {
                pbMotor_1.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotor("Motor_2").RunFeedback)
            {
                pbMotor_2.BackgroundImage = pump_green;
            }
            else
            {
                pbMotor_2.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotor("Motor_3").RunFeedback)
            {
                pbMotor_3.BackgroundImage = pump_green;
            }
            else
            {
                pbMotor_3.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotor("Motor_4").RunFeedback)
            {
                pbMotor_4.BackgroundImage = pump_green;
            }
            else
            {
                pbMotor_4.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotor("Motor_5").RunFeedback)
            {
                pbMotor_5.BackgroundImage = pump_green;
            }
            else
            {
                pbMotor_5.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotor("Motor_6").RunFeedback)
            {
                pbMotor_6.BackgroundImage = pump_green;
            }
            else
            {
                pbMotor_6.BackgroundImage = pump_red;
            }


            pbMeter_1.BackgroundImage = cabinet;
            pbMeter_2.BackgroundImage = cabinet;
            pbMeter_3.BackgroundImage = cabinet;
            pbMeter_4.BackgroundImage = cabinet;
            pbMeter_5.BackgroundImage = cabinet;
            pbMeter_6.BackgroundImage = cabinet;

            if (Program.Root.FindMotorOPC("Motor_1").RunFeedback)
            {
                pbMotorOPC_1.BackgroundImage = pump_green;
            }
            else
            {
                pbMotorOPC_1.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotorOPC("Motor_2").RunFeedback)
            {
                pbMotorOPC_2.BackgroundImage = pump_green;
            }
            else
            {
                pbMotorOPC_2.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotorOPC("Motor_3").RunFeedback)
            {
                pbMotorOPC_3.BackgroundImage = pump_green;
            }
            else
            {
                pbMotorOPC_3.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotorOPC("Motor_4").RunFeedback)
            {
                pbMotorOPC_4.BackgroundImage = pump_green;
            }
            else
            {
                pbMotorOPC_4.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotorOPC("Motor_5").RunFeedback)
            {
                pbMotorOPC_5.BackgroundImage = pump_green;
            }
            else
            {
                pbMotorOPC_5.BackgroundImage = pump_red;
            }

            if (Program.Root.FindMotorOPC("Motor_6").RunFeedback)
            {
                pbMotorOPC_6.BackgroundImage = pump_green;
            }
            else
            {
                pbMotorOPC_6.BackgroundImage = pump_red;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void btMotorOPC_1_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotorOPC("Motor_1");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotorOPC_2_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotorOPC("Motor_2");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotorOPC_3_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotorOPC("Motor_3");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotorOPC_4_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotorOPC("Motor_4");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotorOPC_5_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotorOPC("Motor_5");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }

        private void btMotorOPC_6_Click(object sender, EventArgs e)
        {
            Motor motor = Program.Root.FindMotorOPC("Motor_6");
            if (motor != null)
            {
                Motor_Faceplate fplt = new Motor_Faceplate(motor);
                fplt.Show();
            }
        }
    }
}
