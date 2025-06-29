using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorControl
{
    public class SCADA
    {
        public List<PLC> PLCs = new List<PLC>();
        public List<Motor> Motors = new List<Motor>();
        public List<Meter> Meters = new List<Meter>();
        public List<Motor> MotorsOPC = new List<Motor>();
        // PLC
        public void AddPLC(PLC plc)
        {
            PLCs.Add(plc);
        }

        public PLC FindPLC(string name)
        {
            foreach (PLC plc in PLCs)
            {
                if(plc.Name == name) 
                    return plc;
            }
            return null;
        }

        // Motor
        public void AddMotor(Motor motor)
        {
            Motors.Add(motor);
        }

        public Motor FindMotor(string name)
        {
            foreach (Motor motor in Motors)
            {
                if (motor.Name == name)

                    return motor;
            }
            return null;
        }

        public void AddMeter(Meter meter)
        {
            Meters.Add(meter);
        }

        public Meter FindMeter(string name)
        {
            foreach (Meter meter in Meters)
            {
                if (meter.Name == name)

                    return meter;
            }
            return null;
        }
        public void AddMotorOPC(Motor motor)
        {
            MotorsOPC.Add(motor);
        }

        public Motor FindMotorOPC(string name)
        {
            foreach (Motor motor in MotorsOPC)
            {
                if (motor.Name == name)

                    return motor;
            }
            return null;
        }
    }

}
