using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;
using EasyModbus;
using System.IO.Ports;
using static System.Collections.Specialized.BitVector32;
using System.Windows.Forms;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace MotorControl
{
    public class PLC
    {
        public Session session = null;
        public Subscription subscription = null;
        public ModbusClient[] modbusClients; // Array to hold multiple Modbus clients
        public string IP;
        public Plc thePLC;
        public string Name;
        public string Type; // "S7" or "MODBUS" or "OPC UA"

        public List<MotorData> Motors = new List<MotorData>();
        public List<MeterData> Meters = new List<MeterData>();
        public List<MotorData> MotorsOPC = new List<MotorData>();

        System.Timers.Timer UpdateTimer;
        private bool[] connectionStatus; // Track connection status for each client

        public PLC(string name, string ip, string type)
        {
            Type = type;
            IP = ip;
            Name = name;

            if (Type == "S7")
            {
                // S7 PLC initialization (unchanged)
                for (int i = 0; i < 6; i++)
                {
                    MotorData motorData = new MotorData();
                    Motors.Add(motorData);
                }
                thePLC = new Plc(CpuType.S71500, IP, 0, 1);
                thePLC.Open();
                Console.WriteLine($"Connected to {IP} successfully using {Type}");
            }
            else if (Type == "MODBUS")
            {
                // Initialize Modbus meter connections
                int numMeters = 6; // Number of meters (ports 502, 503, 504)
                modbusClients = new ModbusClient[numMeters];
                connectionStatus = new bool[numMeters];

                // Create MeterData objects
                for (int i = 0; i < numMeters; i++)
                {
                    MeterData meterData = new MeterData();
                    Meters.Add(meterData);

                    // Initialize Modbus client for this meter
                    int port = 502 + i; // Ports are 502, 503, 504
                    modbusClients[i] = new ModbusClient(IP, port);

                    try
                    {
                        modbusClients[i].Connect();
                        connectionStatus[i] = true;
                        Console.WriteLine($"Connected to {IP}:{port} successfully");
                    }
                    catch (Exception ex)
                    {
                        connectionStatus[i] = false;
                        Console.WriteLine($"Failed to connect to {IP}:{port} - Error: {ex.Message}");
                    }
                }

                Console.WriteLine($"Modbus meters initialization complete");
            }
            else if (Type == "OPC")
            {
                for (int i = 0; i < 6; i++)
                {
                    MotorData motorData = new MotorData();
                    MotorsOPC.Add(motorData);
                }
                try
                {
                    // Địa chỉ của OPC UA Server
                    string serverUrl =  "opc.tcp://192.168.1.253:4840"; // Thay bằng địa chỉ thực của bạn

                    // Tạo endpoint mà không cần ApplicationConfiguration
                    EndpointDescription selectedEndpoint = DiscoverEndpoint(serverUrl);
                    if (selectedEndpoint == null)
                    {
                        MessageBox.Show("Không tìm thấy endpoint!");
                        return;
                    }

                    // Tạo session
                    UserIdentity userIdentity = new UserIdentity(new AnonymousIdentityToken());
                    session = CreateSession(serverUrl, userIdentity);

                    if (session != null && session.Connected)
                    {
                        //MessageBox.Show("Kết nối thành công!");
                        // Kích hoạt các nút điều khiển khác   
                    }
                    else
                    {
                       // MessageBox.Show("Không thể kết nối!");
                    }
                }
                catch (Exception ex)
                {
                  //  MessageBox.Show("Lỗi kết nối: " + ex.Message);
                }
            }

                // Set up the update timer
                UpdateTimer = new System.Timers.Timer(500);
                UpdateTimer.Elapsed += UpdateTimer_Elapsed;
                UpdateTimer.Enabled = true;
            
        }





        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Type == "S7")
            {
                // S7 data reading (unchanged)
                for (int i = 0; i < 6; i++)
                {
                    thePLC.ReadClass(Motors[i], i + 1);
                }
            }
            else if (Type == "MODBUS")
            {
                // Read data from each Modbus meter
                for (int i = 0; i < modbusClients.Length; i++)
                {
                    if (connectionStatus[i] && modbusClients[i].Connected)
                    {
                        try
                        {
                            // Read 24 registers from each meter starting at register 0 (for 4x00001)
                            int[] registers = modbusClients[i].ReadInputRegisters(0, 24);
                            // Map register values to MeterData properties
                            UpdateMeterData(Meters[i], registers);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading from meter at port {502 + i}: {ex.Message}");

                            // Try to reconnect if connection was lost
                            try
                            {
                                if (!modbusClients[i].Connected)
                                {
                                    modbusClients[i].Connect();
                                    Console.WriteLine($"Reconnected to meter at port {502 + i}");
                                }
                            }
                            catch
                            {
                                Console.WriteLine($"Failed to reconnect to meter at port {502 + i}");
                            }
                        }
                    }
                }
            }
            else if (Type == "OPC" && session != null && session.Connected)
            {
                for (int i = 0; i < 6; i++)
                {
                    MotorsOPC[i].RunFeedback = ReadBoolean($"ns=3;s=\"Motor_{i + 1}\".\"RunFeedback\"");
                    MotorsOPC[i].Fault = ReadBoolean($"ns=3;s=\"Motor_{i + 1}\".\"Fault\"");
                    MotorsOPC[i].SetSpeed = ReadFloat($"ns=3;s=\"Motor_{i + 1}\".\"SetSpeed\"");
                    MotorsOPC[i].Speed = ReadFloat($"ns=3;s=\"Motor_{i + 1}\".\"Speed\"");
                }
            }
                    
            }

        //OPC
        private EndpointDescription DiscoverEndpoint(string serverUrl)
        {
            // Đơn giản hóa việc tìm endpoint
            try
            {
                // Chuyển đổi string thành System.Uri
                System.Uri serverUri = new System.Uri(serverUrl);
                using (DiscoveryClient client = DiscoveryClient.Create(serverUri))
                {
                    EndpointDescriptionCollection endpoints = client.GetEndpoints(null);

                    // Chọn endpoint đầu tiên có sẵn
                    if (endpoints != null && endpoints.Count > 0)
                    {
                        // Thường thì nên chọn endpoint an toàn nhất, nhưng để đơn giản ta lấy cái đầu tiên
                        return endpoints[0];
                    }
                }
                return null;
            }
            catch
            {
                // Nếu không thể dùng discovery, tạo endpoint mặc định
                EndpointDescription endpoint = new EndpointDescription();
                endpoint.EndpointUrl = serverUrl;
                endpoint.SecurityMode = MessageSecurityMode.None;
                endpoint.SecurityPolicyUri = SecurityPolicies.None;
                endpoint.TransportProfileUri = Profiles.UaTcpTransport;
                return endpoint;
            }
        }
        private Session CreateSession(string serverUrl, UserIdentity userIdentity)
        {
            try
            {
                // Chuyển đổi string thành System.Uri
                System.Uri serverUri = new System.Uri(serverUrl);

                // Tạo cấu hình ứng dụng cần thiết
                Opc.Ua.ApplicationConfiguration config = new Opc.Ua.ApplicationConfiguration();
                config.ApplicationName = "Simple OPC UA Client";
                config.ApplicationType = ApplicationType.Client;
                config.SecurityConfiguration = new SecurityConfiguration();
                config.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier();
                config.SecurityConfiguration.TrustedPeerCertificates = new CertificateTrustList();
                config.SecurityConfiguration.TrustedIssuerCertificates = new CertificateTrustList();
                config.SecurityConfiguration.RejectedCertificateStore = new CertificateStoreIdentifier();
                config.TransportQuotas = new TransportQuotas();
                config.ClientConfiguration = new ClientConfiguration();
                config.TraceConfiguration = new TraceConfiguration();

                // Sử dụng Discovery để tìm endpoints
                using (DiscoveryClient client = DiscoveryClient.Create(serverUri))
                {
                    EndpointDescriptionCollection endpoints = client.GetEndpoints(null);

                    if (endpoints == null || endpoints.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy endpoints!");
                        return null;
                    }

                    EndpointDescription endpoint = endpoints[0]; // Lấy endpoint đầu tiên

                    // Tạo ConfiguredEndpoint
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(config);
                    ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, endpoint, endpointConfiguration);

                    // Tạo session với đúng số lượng tham số
                    Session newSession = Session.Create(
                        config,                    // ApplicationConfiguration
                        configuredEndpoint,        // ConfiguredEndpoint
                        false,                     // updateBeforeConnect
                        "OPC UA Client Session",   // sessionName
                        60000,                     // sessionTimeout
                        userIdentity,              // userIdentity
                        null                       // preferredLocales
                    ).GetAwaiter().GetResult();

                    return newSession;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo session: " + ex.Message);
                return null;
            }
        }
        // Đọc giá trị Float
        private float ReadFloat(string nodeIdString)
        {
            object value = ReadOpcValue(nodeIdString);
            if (value != null)
            {
                return Convert.ToSingle(value);
            }
            return 0.0f;
        }

        // Đọc giá trị Boolean
        private bool ReadBoolean(string nodeIdString)
        {
            object value = ReadOpcValue(nodeIdString);
            if (value != null)
            {
                return Convert.ToBoolean(value);
            }
            return false;
        }
        private object ReadOpcValue(string nodeIdString)
        {
            try
            {
                if (session == null || !session.Connected)
                {
                    MessageBox.Show("Chưa kết nối đến server!");
                    return null;
                }

                // Chuyển đổi chuỗi NodeId thành đối tượng NodeId
                NodeId nodeId = NodeId.Parse(nodeIdString);

                // Đọc giá trị
                DataValue value = session.ReadValue(nodeId);

                // Kiểm tra trạng thái đọc
                if (StatusCode.IsGood(value.StatusCode))
                {
                    // Trả về giá trị
                    return value.Value;
                }
                else
                {
                    Console.WriteLine ($"Lỗi khi đọc giá trị: {value.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc giá trị: {ex.Message}");
                return null;
            }
        }
        private bool WriteOpcValue(string nodeIdString, object value)
        {
            try
            {
                if (session == null || !session.Connected)
                {
                    MessageBox.Show("Chưa kết nối đến server!");
                    return false;
                }

                // Chuyển đổi chuỗi NodeId thành đối tượng NodeId
                NodeId nodeId = NodeId.Parse(nodeIdString);

                // Tạo WriteValue chứa thông tin về node và giá trị cần ghi
                WriteValue valueToWrite = new WriteValue()
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                };

                // Tạo collection chứa một hoặc nhiều giá trị cần ghi
                WriteValueCollection valuesToWrite = new WriteValueCollection();
                valuesToWrite.Add(valueToWrite);

                // Thực hiện thao tác ghi
                StatusCodeCollection results;
                DiagnosticInfoCollection diagnosticInfos;

                session.Write(
                    null,              // RequestHeader (null sử dụng các giá trị mặc định)
                    valuesToWrite,     // Các giá trị cần ghi
                    out results,       // Kết quả ghi
                    out diagnosticInfos // Thông tin chẩn đoán
                );

                // Kiểm tra trạng thái của thao tác ghi
                if (results.Count > 0 && StatusCode.IsGood(results[0]))
                {
                    Console.WriteLine($"Ghi giá trị thành công cho {nodeIdString}");
                    return true;
                }
                else
                {
                    MessageBox.Show($"Lỗi khi ghi giá trị cho {nodeIdString}: {results[0]}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi ghi giá trị: {ex.Message}");
                return false;
            }
        }
        // Ghi giá trị Boolean
        public bool WriteBoolean(string nodeIdString, bool value)
        {
            return WriteOpcValue(nodeIdString, value);
        }

        // Ghi giá trị Float
        public bool WriteFloat(string nodeIdString, float value)
        {
            return WriteOpcValue(nodeIdString, value);
        }

        // Ghi giá trị Int32
        public bool WriteInt32(string nodeIdString, int value)
        {
            return WriteOpcValue(nodeIdString, value);
        }

        // Ghi giá trị String
        public bool WriteString(string nodeIdString, string value)
        {
            return WriteOpcValue(nodeIdString, value);
        }



        // Method to update MeterData object with values from Modbus registers
        private void UpdateMeterData(MeterData meter, int[] registers)
        {
            if (registers.Length < 24) return; // Safety check

            // Map registers to MeterData properties
            // Most power meter values are stored as 32-bit floats spanning two registers
            // Using the IEEE 754 format with byte order conversion

            // Voltage measurements
            meter.V = ConvertRegistersToFloat(registers[1], registers[0]);     // Average voltage
            meter.Vab = ConvertRegistersToFloat(registers[3], registers[2]);   // Phase A-B voltage
            meter.Vbc = ConvertRegistersToFloat(registers[5], registers[4]);   // Phase B-C voltage
            meter.Vca = ConvertRegistersToFloat(registers[7], registers[6]);   // Phase C-A voltage

            // Current measurements
            meter.I = ConvertRegistersToFloat(registers[9], registers[8]);     // Average current
            meter.Ia = ConvertRegistersToFloat(registers[11], registers[10]);  // Phase A current
            meter.Ib = ConvertRegistersToFloat(registers[13], registers[12]);  // Phase B current
            meter.Ic = ConvertRegistersToFloat(registers[15], registers[14]);  // Phase C current

            // Power measurements
            meter.P = ConvertRegistersToFloat(registers[17], registers[16]);   // Active power
            meter.Q = ConvertRegistersToFloat(registers[19], registers[18]);   // Reactive power
            meter.PF = ConvertRegistersToFloat(registers[21], registers[20]);  // Power factor
            meter.E = ConvertRegistersToFloat(registers[23], registers[22]);   // Energy

            // For digital status flags, uncomment and implement if needed
            // These would typically be in a different register area for digital inputs
            // meter.Load1 = (registers[24] & 0x01) != 0;
            // meter.Load2 = (registers[24] & 0x02) != 0;
            // meter.Load3 = (registers[24] & 0x04) != 0;
        }

        // Convert two 16-bit registers to a 32-bit float using IEEE 754 format
        private float ConvertRegistersToFloat(int register1, int register2)
        {
            byte[] bytes = new byte[4];

            // Handle byte order - this implementation assumes ABCD format (most common)
            // Modify as needed based on your meter's data format
            bytes[0] = (byte)(register1 & 0xFF);         // Byte A (low byte of 1st register)
            bytes[1] = (byte)((register1 >> 8) & 0xFF);  // Byte B (high byte of 1st register)
            bytes[2] = (byte)(register2 & 0xFF);         // Byte C (low byte of 2nd register)
            bytes[3] = (byte)((register2 >> 8) & 0xFF);  // Byte D (high byte of 2nd register)

            // For CDAB format (less common), use this instead:
            // bytes[0] = (byte)(register1 & 0xFF);
            // bytes[1] = (byte)((register1 >> 8) & 0xFF);
            // bytes[2] = (byte)(register2 & 0xFF);
            // bytes[3] = (byte)((register2 >> 8) & 0xFF);

            // Convert bytes to 32-bit float
            return BitConverter.ToSingle(bytes, 0);
        }

        // Method to write to S7 PLC (unchanged)
        public void Write(string addr, object val)
        {
            if (Type == "S7" && thePLC.IsConnected)
            {
                thePLC.Write(addr, val);
            }
            
        }
        public void WriteSingleCoil(int meterIndex, int coilAddr,bool Value) 
        {
            modbusClients[meterIndex].WriteSingleCoil(coilAddr, Value);
        }
        // Clean up resources when no longer needed
        public void Dispose()
        {
            // Stop timer
            if (UpdateTimer != null)
            {
                UpdateTimer.Enabled = false;
                UpdateTimer.Elapsed -= UpdateTimer_Elapsed;
            }

            // Disconnect from Modbus clients
            if (Type == "MODBUS" && modbusClients != null)
            {
                for (int i = 0; i < modbusClients.Length; i++)
                {
                    if (modbusClients[i] != null && modbusClients[i].Connected)
                    {
                        try
                        {
                            modbusClients[i].Disconnect();
                        }
                        catch { /* Ignore disconnection errors */ }
                    }
                }
            }

            // Disconnect from S7 PLC
            if (Type == "S7" && thePLC != null && thePLC.IsConnected)
            {
                thePLC.Close();
            }
        }
    }

    // Motor data class (unchanged)
    public class MotorData
    {
        public bool Start { get; set; }
        public bool Stop { get; set; }
        public bool RunFeedback { get; set; }
        public bool Reset { get; set; }
        public byte aByte { get; set; } // 1.0
        public float SetSpeed { get; set; } // 2.0
        public bool Cmd { get; set; } //6.0
        public bool Fault { get; set; } //6.1
        public byte aByte2 { get; set; } // 7.0
        public float Speed { get; set; } // 8.0
    }

    // Meter data class (unchanged)
    public class MeterData
    {
        public float V { get; set; }
        public float Vab { get; set; }
        public float Vbc { get; set; }
        public float Vca { get; set; }
        public float I { get; set; }
        public float Ia { get; set; }
        public float Ib { get; set; }
        public float Ic { get; set; }
        public float P { get; set; }
        public float Q { get; set; }
        public float PF { get; set; }
        public float E { get; set; }
        public bool Load1 { get; set; }
        public bool Load2 { get; set; }
        public bool Load3 { get; set; }
    }
}