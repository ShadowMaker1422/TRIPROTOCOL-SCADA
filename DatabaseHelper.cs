using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace MotorControl
{
    public class DatabaseHelper
    {
        private string connectionString;
        private static DatabaseHelper instance;
        private static readonly object lockObject = new object();

        // Singleton pattern để đảm bảo chỉ có một instance
        // Thêm một constant để dễ thay đổi
        private const string DATABASE_NAME = "SCADA";
        private const string SERVER_NAME = @"SIEMEN_LAPTOP\SQLEXPRESS";

        public static DatabaseHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new DatabaseHelper();
                        }
                    }
                }
                return instance;
            }
        }

        private DatabaseHelper()
        {
            connectionString = $"Data Source={SERVER_NAME};Initial Catalog={DATABASE_NAME};Integrated Security=True;";
        }

        public bool InitializeDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Tạo bảng PLCs
                    string createPLCsTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PLCs' AND xtype='U')
                        CREATE TABLE PLCs (
                            PLCID INT IDENTITY(1,1) PRIMARY KEY,
                            PLCName NVARCHAR(50) NOT NULL,
                            IPAddress NVARCHAR(50) NOT NULL,
                            PLCType NVARCHAR(10) NOT NULL CHECK (PLCType IN ('S7', 'MODBUS', 'OPC')),
                            Description NVARCHAR(200),
                            IsActive BIT DEFAULT 1,
                            CreatedDate DATETIME DEFAULT GETDATE(),
                            CONSTRAINT UQ_PLC_Name UNIQUE(PLCName)
                        )";

                    ExecuteNonQuery(createPLCsTable, conn);

                    // Tạo bảng Devices
                    string createDevicesTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devices' AND xtype='U')
                        CREATE TABLE Devices (
                            DeviceID INT IDENTITY(1,1) PRIMARY KEY,
                            DeviceName NVARCHAR(50) NOT NULL,
                            DeviceType NVARCHAR(10) NOT NULL CHECK (DeviceType IN ('MOTOR', 'METER')),
                            PLCID INT NOT NULL,
                            Address INT NOT NULL,
                            Description NVARCHAR(200),
                            IsActive BIT DEFAULT 1,
                            CreatedDate DATETIME DEFAULT GETDATE(),
                            FOREIGN KEY (PLCID) REFERENCES PLCs(PLCID),
                            CONSTRAINT UQ_Device_Name UNIQUE(DeviceName)
                        )";

                    ExecuteNonQuery(createDevicesTable, conn);

                    // Tạo bảng MotorData
                    string createMotorDataTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MotorData' AND xtype='U')
                        CREATE TABLE MotorData (
                            DataID BIGINT IDENTITY(1,1) PRIMARY KEY,
                            DeviceID INT NOT NULL,
                            Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
                            RunFeedback BIT NOT NULL,
                            Fault BIT NOT NULL,
                            Speed FLOAT NOT NULL,
                            SetSpeed FLOAT NOT NULL,
                            FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID)
                        )";

                    ExecuteNonQuery(createMotorDataTable, conn);

                    // Tạo bảng MeterData
                    string createMeterDataTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MeterData' AND xtype='U')
                        CREATE TABLE MeterData (
                            DataID BIGINT IDENTITY(1,1) PRIMARY KEY,
                            DeviceID INT NOT NULL,
                            Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
                            V FLOAT NOT NULL,
                            Vab FLOAT NOT NULL,
                            Vbc FLOAT NOT NULL,
                            Vca FLOAT NOT NULL,
                            I FLOAT NOT NULL,
                            Ia FLOAT NOT NULL,
                            Ib FLOAT NOT NULL,
                            Ic FLOAT NOT NULL,
                            P FLOAT NOT NULL,
                            Q FLOAT NOT NULL,
                            PF FLOAT NOT NULL,
                            E FLOAT NOT NULL,
                            Load1 BIT NOT NULL DEFAULT 0,
                            Load2 BIT NOT NULL DEFAULT 0,
                            Load3 BIT NOT NULL DEFAULT 0,
                            FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID)
                        )";

                    ExecuteNonQuery(createMeterDataTable, conn);

                    // Tạo bảng Alarms
                    string createAlarmsTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Alarms' AND xtype='U')
                        CREATE TABLE Alarms (
                            AlarmID INT IDENTITY(1,1) PRIMARY KEY,
                            DeviceID INT NOT NULL,
                            AlarmType NVARCHAR(50) NOT NULL,
                            Parameter NVARCHAR(50),
                            Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
                            Value FLOAT NOT NULL,
                            ThresholdValue FLOAT NOT NULL,
                            Unit NVARCHAR(10),
                            Description NVARCHAR(200) NOT NULL,
                            IsActive BIT NOT NULL DEFAULT 1,
                            IsAcknowledged BIT NOT NULL DEFAULT 0,
                            AcknowledgedBy NVARCHAR(50),
                            AcknowledgedDate DATETIME,
                            FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID)
                        )";

                    ExecuteNonQuery(createAlarmsTable, conn);

                    // Tạo indexes
                    string createIndexes = @"
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MotorData_DeviceID_Timestamp')
                        CREATE INDEX IX_MotorData_DeviceID_Timestamp ON MotorData(DeviceID, Timestamp DESC);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MeterData_DeviceID_Timestamp')
                        CREATE INDEX IX_MeterData_DeviceID_Timestamp ON MeterData(DeviceID, Timestamp DESC);
                        
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Alarms_DeviceID_Timestamp')
                        CREATE INDEX IX_Alarms_DeviceID_Timestamp ON Alarms(DeviceID, Timestamp DESC);";

                    ExecuteNonQuery(createIndexes, conn);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                return false;
            }
        }

        private void ExecuteNonQuery(string query, SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertInitialData()
        {
            try
            {
                // Thêm PLC
                AddPLC("PLC_1", "192.168.1.251", "S7", "S7-1500 PLC");
                AddPLC("PLC_3", "192.168.1.250", "MODBUS", "MODBUS TCP Server");
                AddPLC("PLC_4", "192.168.1.253", "OPC", "OPC UA Server");

                // Lấy ID của các PLC
                int plc1ID = GetPLCID("PLC_1");
                int plc3ID = GetPLCID("PLC_3");
                int plc4ID = GetPLCID("PLC_4");

                // Thêm Motor S7
                for (int i = 1; i <= 6; i++)
                {
                    AddDevice($"Motor_{i}", "MOTOR", plc1ID, i - 1, $"S7 Motor {i}");
                }

                // Thêm Meter MODBUS
                for (int i = 1; i <= 6; i++)
                {
                    AddDevice($"Meter_{i}", "METER", plc3ID, i - 1, $"MODBUS Meter {i}");
                }

                // Thêm Motor OPC
                for (int i = 1; i <= 6; i++)
                {
                    AddDevice($"Motor_OPC_{i}", "MOTOR", plc4ID, i - 1, $"OPC Motor {i}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting initial data: {ex.Message}");
            }
        }

        public int AddPLC(string name, string ip, string type, string description)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"IF NOT EXISTS (SELECT 1 FROM PLCs WHERE PLCName = @PLCName)
                                   INSERT INTO PLCs (PLCName, IPAddress, PLCType, Description) 
                                   OUTPUT INSERTED.PLCID
                                   VALUES (@PLCName, @IPAddress, @PLCType, @Description)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PLCName", name);
                        cmd.Parameters.AddWithValue("@IPAddress", ip);
                        cmd.Parameters.AddWithValue("@PLCType", type);
                        cmd.Parameters.AddWithValue("@Description", description);

                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : GetPLCID(name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding PLC: {ex.Message}");
                return -1;
            }
        }

        public int AddDevice(string name, string type, int plcId, int address, string description)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"IF NOT EXISTS (SELECT 1 FROM Devices WHERE DeviceName = @DeviceName)
                                   INSERT INTO Devices (DeviceName, DeviceType, PLCID, Address, Description) 
                                   OUTPUT INSERTED.DeviceID
                                   VALUES (@DeviceName, @DeviceType, @PLCID, @Address, @Description)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceName", name);
                        cmd.Parameters.AddWithValue("@DeviceType", type);
                        cmd.Parameters.AddWithValue("@PLCID", plcId);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@Description", description);

                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : GetDeviceID(name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding device: {ex.Message}");
                return -1;
            }
        }

        public int GetPLCID(string plcName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT PLCID FROM PLCs WHERE PLCName = @PLCName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PLCName", plcName);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        public int GetDeviceID(string deviceName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT DeviceID FROM Devices WHERE DeviceName = @DeviceName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceName", deviceName);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        public bool SaveMotorData(Motor motor)
        {
            try
            {
                int deviceID = GetDeviceID(motor.Name);
                if (deviceID == -1) return false;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO MotorData 
                                   (DeviceID, RunFeedback, Fault, Speed, SetSpeed) 
                                   VALUES 
                                   (@DeviceID, @RunFeedback, @Fault, @Speed, @SetSpeed)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                        cmd.Parameters.AddWithValue("@RunFeedback", motor.RunFeedback);
                        cmd.Parameters.AddWithValue("@Fault", motor.Fault);
                        cmd.Parameters.AddWithValue("@Speed", motor.Speed);
                        cmd.Parameters.AddWithValue("@SetSpeed", motor.SetSpeed);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving motor data: {ex.Message}");
                return false;
            }
        }

        public bool SaveMeterData(Meter meter)
        {
            try
            {
                int deviceID = GetDeviceID(meter.Name);
                if (deviceID == -1) return false;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO MeterData 
                                   (DeviceID, V, Vab, Vbc, Vca, I, Ia, Ib, Ic, 
                                    P, Q, PF, E, Load1, Load2, Load3) 
                                   VALUES 
                                   (@DeviceID, @V, @Vab, @Vbc, @Vca, @I, @Ia, @Ib, @Ic, 
                                    @P, @Q, @PF, @E, @Load1, @Load2, @Load3)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                        cmd.Parameters.AddWithValue("@V", meter.V);
                        cmd.Parameters.AddWithValue("@Vab", meter.Vab);
                        cmd.Parameters.AddWithValue("@Vbc", meter.Vbc);
                        cmd.Parameters.AddWithValue("@Vca", meter.Vca);
                        cmd.Parameters.AddWithValue("@I", meter.I);
                        cmd.Parameters.AddWithValue("@Ia", meter.Ia);
                        cmd.Parameters.AddWithValue("@Ib", meter.Ib);
                        cmd.Parameters.AddWithValue("@Ic", meter.Ic);
                        cmd.Parameters.AddWithValue("@P", meter.P);
                        cmd.Parameters.AddWithValue("@Q", meter.Q);
                        cmd.Parameters.AddWithValue("@PF", meter.PF);
                        cmd.Parameters.AddWithValue("@E", meter.E);
                        cmd.Parameters.AddWithValue("@Load1", meter.Load1);
                        cmd.Parameters.AddWithValue("@Load2", meter.Load2);
                        cmd.Parameters.AddWithValue("@Load3", meter.Load3);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving meter data: {ex.Message}");
                return false;
            }
        }

        public bool SaveMotorAlarm(Motor.AlarmEntry alarm)
        {
            try
            {
                int deviceID = GetDeviceID(alarm.MotorName);
                if (deviceID == -1) return false;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Alarms 
                                   (DeviceID, AlarmType, Parameter, Value, ThresholdValue, 
                                    Unit, Description, IsActive) 
                                   VALUES 
                                   (@DeviceID, @AlarmType, @Parameter, @Value, @ThresholdValue, 
                                    @Unit, @Description, @IsActive)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                        cmd.Parameters.AddWithValue("@AlarmType", "SPEED_HIGH");
                        cmd.Parameters.AddWithValue("@Parameter", "Speed");
                        cmd.Parameters.AddWithValue("@Value", alarm.Speed);
                        cmd.Parameters.AddWithValue("@ThresholdValue", 900); // Your threshold
                        cmd.Parameters.AddWithValue("@Unit", "Hz");
                        cmd.Parameters.AddWithValue("@Description", alarm.Description);
                        cmd.Parameters.AddWithValue("@IsActive", alarm.IsActive);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving motor alarm: {ex.Message}");
                return false;
            }
        }

        public bool SaveMeterAlarm(Meter.AlarmEntry alarm)
        {
            try
            {
                int deviceID = GetDeviceID(alarm.MeterName);
                if (deviceID == -1) return false;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Alarms 
                                   (DeviceID, AlarmType, Parameter, Value, ThresholdValue, 
                                    Unit, Description, IsActive) 
                                   VALUES 
                                   (@DeviceID, @AlarmType, @Parameter, @Value, @ThresholdValue, 
                                    @Unit, @Description, @IsActive)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                        cmd.Parameters.AddWithValue("@AlarmType", alarm.AlarmType);
                        cmd.Parameters.AddWithValue("@Parameter", alarm.Parameter);
                        cmd.Parameters.AddWithValue("@Value", alarm.Value);
                        cmd.Parameters.AddWithValue("@ThresholdValue", 0); // Set appropriate threshold
                        cmd.Parameters.AddWithValue("@Unit", alarm.Unit);
                        cmd.Parameters.AddWithValue("@Description", alarm.Description);
                        cmd.Parameters.AddWithValue("@IsActive", alarm.IsActive);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving meter alarm: {ex.Message}");
                return false;
            }
        }

        // Phương thức để lấy dữ liệu lịch sử
        public DataTable GetMotorHistory(string deviceName, DateTime fromDate, DateTime toDate)
        {
            try
            {
                int deviceID = GetDeviceID(deviceName);
                if (deviceID == -1) return null;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT Timestamp, RunFeedback, Fault, Speed, SetSpeed 
                                   FROM MotorData 
                                   WHERE DeviceID = @DeviceID 
                                   AND Timestamp BETWEEN @FromDate AND @ToDate 
                                   ORDER BY Timestamp DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting motor history: {ex.Message}");
                return null;
            }
        }

        public DataTable GetMeterHistory(string deviceName, DateTime fromDate, DateTime toDate)
        {
            try
            {
                int deviceID = GetDeviceID(deviceName);
                if (deviceID == -1) return null;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT * 
                                   FROM MeterData 
                                   WHERE DeviceID = @DeviceID 
                                   AND Timestamp BETWEEN @FromDate AND @ToDate 
                                   ORDER BY Timestamp DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting meter history: {ex.Message}");
                return null;
            }
        }

        public DataTable GetActiveAlarms()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT a.*, d.DeviceName, d.DeviceType, p.PLCName 
                                   FROM Alarms a
                                   INNER JOIN Devices d ON a.DeviceID = d.DeviceID
                                   INNER JOIN PLCs p ON d.PLCID = p.PLCID
                                   WHERE a.IsActive = 1 AND a.IsAcknowledged = 0
                                   ORDER BY a.Timestamp DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active alarms: {ex.Message}");
                return null;
            }
        }
    }
}