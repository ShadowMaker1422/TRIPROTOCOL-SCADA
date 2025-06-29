using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MotorControl
{
    internal static class Program
    {
        public static SCADA Root = new SCADA();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Khởi tạo database
            DatabaseHelper.Instance.InitializeDatabase();
            DatabaseHelper.Instance.InsertInitialData();

           //PLC plc = new PLC("PLC_1", "192.168.1.251", "S7");
           //Root.AddPLC(plc);
           PLC plc = new PLC("PLC_3", "192.168.1.250", "MODBUS");
           Root.AddPLC(plc);
           //plc = new PLC("PLC_4", "192.168.1.253", "OPC");
           //Root.AddPLC(plc);

            Motor motor = new Motor("Motor_1", "PLC_1", 0, Root);
            Root.AddMotor(motor);
            motor = new Motor("Motor_2", "PLC_1", 1, Root);
            Root.AddMotor(motor);
            motor = new Motor("Motor_3", "PLC_1", 2, Root);
            Root.AddMotor(motor);
            motor = new Motor("Motor_4", "PLC_1", 3, Root);
            Root.AddMotor(motor);
            motor = new Motor("Motor_5", "PLC_1", 4, Root);
            Root.AddMotor(motor);
            motor = new Motor("Motor_6", "PLC_1", 5, Root);
            Root.AddMotor(motor);

          
            Meter meter = new Meter("Meter_1", "PLC_3", 0, Root);
            Root.AddMeter(meter);
            meter = new Meter("Meter_2", "PLC_3", 1, Root);
            Root.AddMeter(meter);
            meter = new Meter("Meter_3", "PLC_3", 2, Root);
            Root.AddMeter(meter);
            meter = new Meter("Meter_4", "PLC_3", 3, Root);
            Root.AddMeter(meter);
            meter = new Meter("Meter_5", "PLC_3", 4, Root);
            Root.AddMeter(meter);
            meter = new Meter("Meter_6", "PLC_3", 5, Root);
            Root.AddMeter(meter);
           

            motor = new Motor("Motor_1", "PLC_4", 0, Root);
            Root.AddMotorOPC(motor);
            motor = new Motor("Motor_2", "PLC_4", 1, Root);
            Root.AddMotorOPC(motor);
            motor = new Motor("Motor_3", "PLC_4", 2, Root);
            Root.AddMotorOPC(motor);
            motor = new Motor("Motor_4", "PLC_4", 3, Root);
            Root.AddMotorOPC(motor);
            motor = new Motor("Motor_5", "PLC_4", 4, Root);
            Root.AddMotorOPC(motor);
            motor = new Motor("Motor_6", "PLC_4", 5, Root);
            Root.AddMotorOPC(motor);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
