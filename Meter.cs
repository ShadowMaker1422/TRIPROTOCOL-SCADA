using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MotorControl
{
    public class Meter
    {
        public bool isShown;
        public string Name;
        public string PLCName;
        public int Address;
        public float V;
        public float Vab;
        public float Vbc;
        public float Vca;
        public float I;
        public float Ia;
        public float Ib;
        public float Ic;
        public float P;
        public float Q;
        public float PF;
        public float E;
        public bool Load1;
        public bool Load2;
        public bool Load3;
        // Thêm các cấu trúc lưu trữ dữ liệu lịch sử
        private const int MAX_DATA_POINTS = 300; // Lưu 5 phút với tần suất 1 giây/điểm
        public List<DateTime> TimeStamps { get; private set; } = new List<DateTime>();
        public List<float> VValues { get; private set; } = new List<float>();
        public List<float> VabValues { get; private set; } = new List<float>();
        public List<float> VbcValues { get; private set; } = new List<float>();
        public List<float> VcaValues { get; private set; } = new List<float>();
        public List<float> IValues { get; private set; } = new List<float>();
        public List<float> IaValues { get; private set; } = new List<float>();
        public List<float> IbValues { get; private set; } = new List<float>();
        public List<float> IcValues { get; private set; } = new List<float>();
        public List<float> PValues { get; private set; } = new List<float>();
        public List<float> QValues { get; private set; } = new List<float>();
        public List<float> PFValues { get; private set; } = new List<float>();
        public List<float> EValues { get; private set; } = new List<float>();

        public SCADA Parent; //Moi motor sinh ra phai co parent cua no -> la scada 

        System.Timers.Timer UpdateTimer;
        System.Timers.Timer MonitorTimer;
        private System.Timers.Timer saveToDbTimer;

        public Meter(string name, string plc, int address, SCADA parent) 
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
            DatabaseHelper.Instance.SaveMeterData(this);
        }
        private void MonitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Lưu dữ liệu hiện tại vào lịch sử
            TimeStamps.Add(DateTime.Now);
            VValues.Add(V);
            VabValues.Add(Vab);
            VbcValues.Add(Vbc);
            VcaValues.Add(Vca);
            IValues.Add(I);
            IaValues.Add(Ia);
            IbValues.Add(Ib);
            IcValues.Add(Ic);
            PValues.Add(P);
            QValues.Add(Q);
            PFValues.Add(PF);
            EValues.Add(E);

            // Giới hạn số lượng điểm dữ liệu
            if (TimeStamps.Count > MAX_DATA_POINTS)
            {
                TimeStamps.RemoveAt(0);
                VValues.RemoveAt(0);
                VabValues.RemoveAt(0);
                VbcValues.RemoveAt(0);
                VcaValues.RemoveAt(0);
                IValues.RemoveAt(0);
                IaValues.RemoveAt(0);
                IbValues.RemoveAt(0);
                IcValues.RemoveAt(0);
                PValues.RemoveAt(0);
                QValues.RemoveAt(0);
                PFValues.RemoveAt(0);
                EValues.RemoveAt(0);
            }
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            PLC pLC = Parent.FindPLC(PLCName);
            if (pLC != null)
            {
                //gán dữ liệu đọc được từ PLC về class này 
                V = pLC.Meters[Address].V;
                Vab = pLC.Meters[Address].Vab;
                Vbc = pLC.Meters[Address].Vbc;
                Vca = pLC.Meters[Address].Vca;
                I  = pLC.Meters[Address].I;
                Ia = pLC.Meters[Address].Ia;
                Ib = pLC.Meters[Address].Ib;
                Ic = pLC.Meters[Address].Ic;
                P = pLC.Meters[Address].P;
                Q = pLC.Meters[Address].Q;
                PF = pLC.Meters[Address].PF;
                E = pLC.Meters[Address].E;

            }
        }
        public void WriteSingleCoil(int meterIndex, int coilAddr, bool Value)
        {
            meterIndex = Address ;
            PLC plc = Parent.FindPLC(PLCName);
            if (plc != null)
            {
                //////////////////
                plc.WriteSingleCoil(Address,coilAddr,Value);
            }
        }
        public class AlarmEntry
        {
            public DateTime Timestamp { get; set; }
            public string MeterName { get; set; }
            public string Parameter { get; set; }  // Tên thông số (V, I, P, etc.)
            public float Value { get; set; }       // Giá trị đo được
            public string Unit { get; set; }       // Đơn vị đo (V, A, W, etc.)
            public string Description { get; set; }
            public bool IsActive { get; set; }
            public string AlarmType { get; set; }  // "HIGH", "LOW", "CRITICAL"
        }
    }
}
