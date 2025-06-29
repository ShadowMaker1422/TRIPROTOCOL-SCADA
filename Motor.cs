using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace MotorControl
{
    public class Motor
    {
        
        private System.Timers.Timer saveToDbTimer;
        // Thêm các cấu trúc lưu trữ dữ liệu lịch sử
        private const int MAX_DATA_POINTS = 300; // Lưu dữ liệu 5 phút (ở 1 giây/điểm)
        public List<DateTime> TimeStamps { get; private set; } = new List<DateTime>();
        public List<float> SpeedValues { get; private set; } = new List<float>();
        public List<float> SetSpeedValues { get; private set; } = new List<float>();
        public bool isShown;
        public string Name;
        public string PLCName;
        public int Address;
        public bool Start; //giao dien
        public bool Stop; //giao dien
        public bool RunFeedback;
        public bool Reset;
        public float SetSpeed;
        public bool Fault;
        public float Speed;

        public SCADA Parent; //Moi motor sinh ra phai co parent cua no -> la scada 

        System.Timers.Timer UpdateTimer;
        System.Timers.Timer MonitorTimer;

        public Motor(string name, string plc, int address, SCADA parent)
        {
            Name = name;
            PLCName = plc;
            Address = address;
            Parent = parent;
            

            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Start();

            MonitorTimer = new System.Timers.Timer(1000);
            MonitorTimer.Elapsed += MonitorTimer_Elapsed; ;
            MonitorTimer.Start();

            // Timer lưu dữ liệu mỗi 5 giây
            saveToDbTimer = new System.Timers.Timer(5000);
            saveToDbTimer.Elapsed += SaveToDbTimer_Elapsed;
            saveToDbTimer.Start();
        }
        private void SaveToDbTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DatabaseHelper.Instance.SaveMotorData(this);
        }
        private void MonitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
           // Console.WriteLine($"{Name} {RunFeedback} {Fault} {SetSpeed} {Speed}");
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            PLC pLC = Parent.FindPLC(PLCName);
            if (pLC != null)
            {
                if (pLC.Type == "S7")
                {
                    RunFeedback = pLC.Motors[Address].RunFeedback;
                    Fault = pLC.Motors[Address].Fault;
                    SetSpeed = pLC.Motors[Address].SetSpeed;
                    Speed = pLC.Motors[Address].Speed;
                }

                else if (pLC.Type=="OPC")
                {
                    RunFeedback = pLC.MotorsOPC[Address].RunFeedback;
                    Fault = pLC.MotorsOPC[Address].Fault;
                    SetSpeed = pLC.MotorsOPC[Address].SetSpeed;
                    Speed = pLC.MotorsOPC[Address].Speed;
                }      
                
            }
            TimeStamps.Add(DateTime.Now);
            SpeedValues.Add(Speed);
            SetSpeedValues.Add(SetSpeed);

            // Giới hạn số lượng điểm dữ liệu
            if (TimeStamps.Count > MAX_DATA_POINTS)
            {
                TimeStamps.RemoveAt(0);
                SpeedValues.RemoveAt(0);
                SetSpeedValues.RemoveAt(0);
            }
        }

        public void Write(string addr, object val)
        {
            PLC plc = Parent.FindPLC(PLCName);
            if (plc != null)
            {
                plc.Write($"DB{Address + 1}.{addr}", val); // addr: dia chi tu faceplate dua xuong,
                //Address: tu Program, lay dia chi cua tung Motor gan vao thanh chuoi DB{1}.DBX0.0
            }
        }
        public bool WriteBoolean( string parameter,bool value)
        { 
            PLC plc = Parent.FindPLC(PLCName);
            if (plc != null)
            {
                return plc.WriteBoolean($"ns=3;s=\"Motor_{Address + 1}\".\"{parameter}\"", value);
            }
            return false;
        }

        public bool WriteFloat(string parameter,float value)
        {
            PLC plc = Parent.FindPLC(PLCName);
            if (plc != null)
            {
                return plc.WriteFloat($"ns=3;s=\"Motor_{Address + 1}\".\"{parameter}\"", value);
            }
            return false;
        }
        public class AlarmEntry
        {
            public DateTime Timestamp { get; set; }
            public string MotorName { get; set; }
            public float Speed { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
