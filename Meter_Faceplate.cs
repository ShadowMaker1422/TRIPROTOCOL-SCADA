using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace MotorControl
{
    public partial class Meter_Faceplate : Form
    {
        Image cabinet = Image.FromFile(@"images\ENERGY_METER.png");
        public Meter Parent;
        bool Load1;
        bool Load2;
        bool Load3;
        private bool isFormClosing = false;

        // Biến cho biểu đồ trend
        private Dictionary<string, LineItem> parameterCurves = new Dictionary<string, LineItem>();
        private Dictionary<string, bool> parameterSelected = new Dictionary<string, bool>();
        private System.Windows.Forms.Timer graphUpdateTimer;
        private int timeRangeMinutes = 2;

        // Mảng màu sắc cho các thông số
        private Color[] curveColors = new Color[]
        {
            Color.Blue, Color.Red, Color.Green, Color.Orange,
            Color.Purple, Color.Brown, Color.Navy, Color.Teal,
            Color.Maroon, Color.OliveDrab, Color.DarkViolet, Color.DarkGoldenrod
        };

        // Biến cho hệ thống alarm
        private BindingList<Meter.AlarmEntry> alarmList = new BindingList<Meter.AlarmEntry>();
        // Dictionary để theo dõi trạng thái alarm cho từng thông số
        private Dictionary<string, bool> alarmStates = new Dictionary<string, bool>();
        // Các ngưỡng cảnh báo
        private const float VOLTAGE_HIGH = 250f;      // V
        private const float VOLTAGE_LOW = 180f;       // V
        private const float CURRENT_HIGH = 100f;      // A
        private const float POWER_HIGH = 5000f;       // W
        private const float POWER_FACTOR_LOW = 0.85f; // Hệ số công suất

        public Meter_Faceplate(Meter parent)
        {
            Parent = parent;
            InitializeComponent();

            // Khởi tạo các từ điển thông số
            InitializeParameterDictionaries();

            // Khởi tạo trạng thái alarm
            InitializeAlarmStates();

            // Thiết lập ZedGraph
            SetupZedGraph();

            SetupAlarmGrid();

            // Thiết lập sự kiện cho các checkbox có sẵn
            SetupCheckboxEvents();

            // Khởi tạo timer để cập nhật biểu đồ
            graphUpdateTimer = new System.Windows.Forms.Timer();
            graphUpdateTimer.Interval = 1000;
            graphUpdateTimer.Tick += GraphUpdateTimer_Tick;
            graphUpdateTimer.Start();
        }
        private void SetupAlarmGrid()
        {
            // Cấu hình cơ bản cho DataGridView
            dgvMeter.AutoGenerateColumns = false;
            dgvMeter.AllowUserToAddRows = false;
            dgvMeter.AllowUserToDeleteRows = false;
            dgvMeter.ReadOnly = true;
            dgvMeter.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMeter.MultiSelect = false;

            // Cấu hình style
            dgvMeter.DefaultCellStyle.Font = new Font("Arial", 9);
            dgvMeter.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dgvMeter.ColumnHeadersHeight = 30;
            dgvMeter.RowTemplate.Height = 25;

            // Xóa các cột cũ nếu có
            dgvMeter.Columns.Clear();

            // Tạo cột thời gian
            DataGridViewTextBoxColumn timeColumn = new DataGridViewTextBoxColumn();
            timeColumn.HeaderText = "Time";
            timeColumn.Name = "Time";
            timeColumn.DataPropertyName = "Timestamp";
            timeColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            timeColumn.Width = 150;
            dgvMeter.Columns.Add(timeColumn);

            // Tạo cột tên thiết bị
            DataGridViewTextBoxColumn deviceColumn = new DataGridViewTextBoxColumn();
            deviceColumn.HeaderText = "MeterName";
            deviceColumn.Name = "MeterName";
            deviceColumn.DataPropertyName = "MeterName";
            deviceColumn.Width = 100;
            dgvMeter.Columns.Add(deviceColumn);

            // Tạo cột thông số
            DataGridViewTextBoxColumn paramColumn = new DataGridViewTextBoxColumn();
            paramColumn.HeaderText = "Parameter";
            paramColumn.Name = "Parameter";
            paramColumn.DataPropertyName = "Parameter";
            paramColumn.Width = 80;
            dgvMeter.Columns.Add(paramColumn);

            // Tạo cột giá trị
            DataGridViewTextBoxColumn valueColumn = new DataGridViewTextBoxColumn();
            valueColumn.HeaderText = "Value";
            valueColumn.Name = "Value";
            valueColumn.DataPropertyName = "Value";
            valueColumn.DefaultCellStyle.Format = "N2";
            valueColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            valueColumn.Width = 80;
            dgvMeter.Columns.Add(valueColumn);

            // Tạo cột đơn vị
            DataGridViewTextBoxColumn unitColumn = new DataGridViewTextBoxColumn();
            unitColumn.HeaderText = "Unit";
            unitColumn.Name = "Unit";
            unitColumn.DataPropertyName = "Unit";
            unitColumn.Width = 60;
            dgvMeter.Columns.Add(unitColumn);

            // Tạo cột mô tả
            DataGridViewTextBoxColumn descColumn = new DataGridViewTextBoxColumn();
            descColumn.HeaderText = "Description";
            descColumn.Name = "Description";
            descColumn.DataPropertyName = "Description";
            descColumn.Width = 250;
            dgvMeter.Columns.Add(descColumn);

            // Tạo cột loại alarm
            DataGridViewTextBoxColumn typeColumn = new DataGridViewTextBoxColumn();
            typeColumn.HeaderText = "AlarmType";
            typeColumn.Name = "AlarmType";
            typeColumn.DataPropertyName = "AlarmType";
            typeColumn.Width = 80;
            dgvMeter.Columns.Add(typeColumn);

            // Gán data source
            dgvMeter.DataSource = alarmList;
        }
        private void InitializeAlarmStates()
        {
            // Khởi tạo trạng thái cho mỗi loại alarm
            string[] parameters = { "V", "Vab", "Vbc", "Vca", "I", "Ia", "Ib", "Ic", "P", "Q", "PF" };
            string[] alarmTypes = { "_HIGH", "_LOW" };

            foreach (var param in parameters)
            {
                foreach (var type in alarmTypes)
                {
                    alarmStates[param + type] = false;
                }
            }
        }
        private void CheckAllAlarms()
        {
            // Kiểm tra điện áp
            CheckVoltageAlarm("V", Parent.V, "Điện áp pha");
            CheckVoltageAlarm("Vab", Parent.Vab, "Điện áp dây AB");
            CheckVoltageAlarm("Vbc", Parent.Vbc, "Điện áp dây BC");
            CheckVoltageAlarm("Vca", Parent.Vca, "Điện áp dây CA");

            // Kiểm tra dòng điện
            CheckCurrentAlarm("I", Parent.I, "Dòng điện tổng");
            CheckCurrentAlarm("Ia", Parent.Ia, "Dòng điện pha A");
            CheckCurrentAlarm("Ib", Parent.Ib, "Dòng điện pha B");
            CheckCurrentAlarm("Ic", Parent.Ic, "Dòng điện pha C");

            // Kiểm tra công suất
            CheckPowerAlarm("P", Parent.P, "Công suất tác dụng");
            CheckPowerAlarm("Q", Parent.Q, "Công suất phản kháng");

            // Kiểm tra hệ số công suất
            CheckPowerFactorAlarm("PF", Parent.PF, "Hệ số công suất");
        }

        private void CheckVoltageAlarm(string parameter, float value, string description)
        {
            string highKey = parameter + "_HIGH";
            string lowKey = parameter + "_LOW";

            // Kiểm tra điện áp cao
            if (value > VOLTAGE_HIGH)
            {
                if (!alarmStates[highKey])
                {
                    CreateAlarm(parameter, value, "V",
                        $"{description} Reach ({VOLTAGE_HIGH}V)", "HIGH");
                    alarmStates[highKey] = true;
                }
            }
            else
            {
                alarmStates[highKey] = false;
            }

            // Kiểm tra điện áp thấp
            if (value < VOLTAGE_LOW)
            {
                if (!alarmStates[lowKey])
                {
                    CreateAlarm(parameter, value, "V",
                        $"{description} Under ({VOLTAGE_LOW}V)", "LOW");
                    alarmStates[lowKey] = true;
                }
            }
            else
            {
                alarmStates[lowKey] = false;
            }
        }

        private void CheckCurrentAlarm(string parameter, float value, string description)
        {
            string highKey = parameter + "_HIGH";

            if (value > CURRENT_HIGH)
            {
                if (!alarmStates[highKey])
                {
                    CreateAlarm(parameter, value, "A",
                        $"{description} Under ({CURRENT_HIGH}A)", "HIGH");
                    alarmStates[highKey] = true;
                }
            }
            else
            {
                alarmStates[highKey] = false;
            }
        }

        private void CheckPowerAlarm(string parameter, float value, string description)
        {
            string highKey = parameter + "_HIGH";

            if (value > POWER_HIGH)
            {
                if (!alarmStates[highKey])
                {
                    CreateAlarm(parameter, value, "W",
                        $"{description} Reach ({POWER_HIGH}W)", "HIGH");
                    alarmStates[highKey] = true;
                }
            }
            else
            {
                alarmStates[highKey] = false;
            }
        }

        private void CheckPowerFactorAlarm(string parameter, float value, string description)
        {
            string lowKey = parameter + "_LOW";

            if (value < POWER_FACTOR_LOW)
            {
                if (!alarmStates[lowKey])
                {
                    CreateAlarm(parameter, value, "",
                        $"{description} Under ({POWER_FACTOR_LOW})", "LOW");
                    alarmStates[lowKey] = true;
                }
            }
            else
            {
                alarmStates[lowKey] = false;
            }
        }

        private void CreateAlarm(string parameter, float value, string unit,
            string description, string alarmType)
        {
            Meter.AlarmEntry newAlarm = new Meter.AlarmEntry
            {
                Timestamp = DateTime.Now,
                MeterName = Parent.Name,
                Parameter = parameter,
                Value = value,
                Unit = unit,
                Description = description,
                AlarmType = alarmType,
                IsActive = true
            };

            // Thêm vào danh sách
            alarmList.Add(newAlarm);
            // Lưu alarm vào database
            DatabaseHelper.Instance.SaveMeterAlarm(newAlarm);

            // Cuộn xuống dòng mới nhất
            if (dgvMeter.Rows.Count > 0)
            {
                dgvMeter.FirstDisplayedScrollingRowIndex = dgvMeter.Rows.Count - 1;
            }

            // Phát âm thanh cảnh báo
            System.Media.SystemSounds.Exclamation.Play();

            // Đánh dấu màu cho TextBox tương ứng
            HighlightAlarmTextBox(parameter, true);
        }

        private void HighlightAlarmTextBox(string parameter, bool isAlarm)
        {
            Color alarmColor = Color.Red;
            Color normalColor = Color.Black;

            switch (parameter)
            {
                case "V":
                    tbV.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Vab":
                    tbVab.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Vbc":
                    tbVbc.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Vca":
                    tbVca.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "I":
                    tbI.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Ia":
                    tbIa.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Ib":
                    tbIb.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Ic":
                    tbIc.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "P":
                    tbP.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "Q":
                    tbQ.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
                case "PF":
                    tbPF.ForeColor = isAlarm ? alarmColor : normalColor;
                    break;
            }
        }

        private void InitializeParameterDictionaries()
        {
            // Khởi tạo từ điển thông số và trạng thái chọn
            parameterSelected["V"] = false;
            parameterSelected["Vab"] = false;
            parameterSelected["Vbc"] = false;
            parameterSelected["Vca"] = false;
            parameterSelected["I"] = false;
            parameterSelected["Ia"] = false;
            parameterSelected["Ib"] = false;
            parameterSelected["Ic"] = false;
            parameterSelected["P"] = false;
            parameterSelected["Q"] = false;
            parameterSelected["PF"] = false;
            parameterSelected["E"] = false;
        }

        private void SetupZedGraph()
        {
            // Lấy GraphPane từ ZedGraph control có sẵn
            GraphPane graphPane = zgMeter.GraphPane;

            // Xóa tất cả các đường hiện tại nếu có
            graphPane.CurveList.Clear();

            // Thiết lập tiêu đề và nhãn trục
            graphPane.Title.Text = "Trend Meter";
            graphPane.XAxis.Title.Text = "Time";
            graphPane.YAxis.Title.Text = "Value";

            // Tạo đường cho mỗi thông số
            int colorIndex = 0;
            foreach (string parameter in parameterSelected.Keys)
            {
                Color curveColor = curveColors[colorIndex % curveColors.Length];
                LineItem curve = graphPane.AddCurve(parameter,
                    new PointPairList(), curveColor, SymbolType.None);
                curve.Line.Width = 2.0F;

                // Lưu đường vào từ điển
                parameterCurves[parameter] = curve;

                // Đặt trạng thái hiển thị dựa trên parameterSelected
                curve.IsVisible = parameterSelected[parameter];

                colorIndex++;
            }

            // Định dạng trục thời gian
            graphPane.XAxis.Type = AxisType.Date;
            graphPane.XAxis.Scale.Format = "HH:mm:ss";
            graphPane.XAxis.Scale.MajorUnit = DateUnit.Second;
            graphPane.XAxis.Scale.MajorStep = 10; // 10 giây mỗi mốc lớn

            // Thiết lập trục Y
            graphPane.YAxis.Scale.MinAuto = true;
            graphPane.YAxis.Scale.MaxAuto = true;

            // Hiển thị lưới
            graphPane.XAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MajorGrid.IsVisible = true;
            graphPane.XAxis.MajorGrid.Color = Color.LightGray;
            graphPane.YAxis.MajorGrid.Color = Color.LightGray;

            // Đặt vị trí chú thích
            graphPane.Legend.Position = ZedGraph.LegendPos.Top;
            graphPane.Legend.IsVisible = true;

            // Tự động tính toán khoảng hiển thị
            zgMeter.AxisChange();
        }

        private void SetupCheckboxEvents()
        {
            // Tìm tất cả checkbox trong form và gán sự kiện
            foreach (Control control in this.Controls)
            {
                // Nếu có checkbox trong panel hoặc groupbox
                if (control is Panel || control is GroupBox)
                {
                    foreach (Control childControl in control.Controls)
                    {
                        if (childControl is CheckBox checkbox)
                        {
                            SetupCheckBoxHandler(checkbox);
                        }
                    }
                }
                // Nếu checkbox trực tiếp trong form
                else if (control is CheckBox checkbox)
                {
                    SetupCheckBoxHandler(checkbox);
                }
            }
        }

        private void SetupCheckBoxHandler(CheckBox checkbox)
        {
            // Tên checkbox có thể là cbV, cbVab, v.v.
            // Lấy ra tên thông số (V, Vab, v.v.)
            string parameter = checkbox.Name.Replace("cb", "");

            // Nếu không có tiền tố "cb", thử sử dụng Text của checkbox
            if (parameter == checkbox.Name)
            {
                parameter = checkbox.Text;
            }

            // Kiểm tra xem thông số có trong danh sách không
            if (parameterSelected.ContainsKey(parameter))
            {
                // Đồng bộ hóa trạng thái checkbox với giá trị trong từ điển
                checkbox.Checked = parameterSelected[parameter];

                // Gán sự kiện
                checkbox.CheckedChanged += (sender, e) => {
                    if (parameterSelected.ContainsKey(parameter) && parameterCurves.ContainsKey(parameter))
                    {
                        parameterSelected[parameter] = checkbox.Checked;
                        parameterCurves[parameter].IsVisible = checkbox.Checked;
                        UpdateGraph();
                    }
                };
            }
        }

        private void GraphUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Kiểm tra trạng thái của form trước khi cập nhật biểu đồ
            if (!isFormClosing)
            {
                UpdateGraph();
            }
        }

        private void UpdateGraph()
        {
            try
            {
                // Kiểm tra form và control
                if (isFormClosing || zgMeter == null || zgMeter.IsDisposed)
                {
                    return;
                }

                GraphPane graphPane = zgMeter.GraphPane;
                if (graphPane == null)
                {
                    return;
                }

                // Kiểm tra dữ liệu
                if (Parent == null || Parent.TimeStamps == null || Parent.TimeStamps.Count == 0)
                {
                    Console.WriteLine("Không có dữ liệu để hiển thị");
                    return;
                }

                // Xóa dữ liệu cũ
                foreach (string key in parameterCurves.Keys)
                {
                    if (parameterCurves[key] != null)
                    {
                        parameterCurves[key].Clear();
                    }
                }

                // Thêm dữ liệu mới cho mỗi đường
                for (int i = 0; i < Parent.TimeStamps.Count; i++)
                {
                    double xDate = new XDate(Parent.TimeStamps[i]);

                    // Thêm điểm cho từng thông số nếu được hiển thị
                    if (parameterCurves.ContainsKey("V") && parameterSelected["V"])
                        parameterCurves["V"].AddPoint(xDate, Parent.VValues[i]);

                    if (parameterCurves.ContainsKey("Vab") && parameterSelected["Vab"])
                        parameterCurves["Vab"].AddPoint(xDate, Parent.VabValues[i]);

                    if (parameterCurves.ContainsKey("Vbc") && parameterSelected["Vbc"])
                        parameterCurves["Vbc"].AddPoint(xDate, Parent.VbcValues[i]);

                    if (parameterCurves.ContainsKey("Vca") && parameterSelected["Vca"])
                        parameterCurves["Vca"].AddPoint(xDate, Parent.VcaValues[i]);

                    if (parameterCurves.ContainsKey("I") && parameterSelected["I"])
                        parameterCurves["I"].AddPoint(xDate, Parent.IValues[i]);

                    if (parameterCurves.ContainsKey("Ia") && parameterSelected["Ia"])
                        parameterCurves["Ia"].AddPoint(xDate, Parent.IaValues[i]);

                    if (parameterCurves.ContainsKey("Ib") && parameterSelected["Ib"])
                        parameterCurves["Ib"].AddPoint(xDate, Parent.IbValues[i]);

                    if (parameterCurves.ContainsKey("Ic") && parameterSelected["Ic"])
                        parameterCurves["Ic"].AddPoint(xDate, Parent.IcValues[i]);

                    if (parameterCurves.ContainsKey("P") && parameterSelected["P"])
                        parameterCurves["P"].AddPoint(xDate, Parent.PValues[i]);

                    if (parameterCurves.ContainsKey("Q") && parameterSelected["Q"])
                        parameterCurves["Q"].AddPoint(xDate, Parent.QValues[i]);

                    if (parameterCurves.ContainsKey("PF") && parameterSelected["PF"])
                        parameterCurves["PF"].AddPoint(xDate, Parent.PFValues[i]);

                    if (parameterCurves.ContainsKey("E") && parameterSelected["E"])
                        parameterCurves["E"].AddPoint(xDate, Parent.EValues[i]);
                }

                // Đặt giới hạn thời gian
                DateTime startTime = DateTime.Now.AddMinutes(-timeRangeMinutes);
                double startXDate = new XDate(startTime);
                double endXDate = new XDate(DateTime.Now);

                // Đặt phạm vi trục X an toàn
                if (graphPane.XAxis != null && graphPane.XAxis.Scale != null)
                {
                    graphPane.XAxis.Scale.Min = startXDate;
                    graphPane.XAxis.Scale.Max = endXDate;
                }

                // Cập nhật biểu đồ
                zgMeter.AxisChange();
                zgMeter.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật biểu đồ: " + ex.Message);
            }
        }

        private void Meter_Faceplate_Load(object sender, EventArgs e)
        {
            pbMeter.BackgroundImage = cabinet;
            grName.Text = Parent.Name;
            Load1 = false;
            Load2 = false;
            Load3 = false;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);

            tbV.Text = Parent.V.ToString();
            tbVab.Text = Parent.Vab.ToString();
            tbVbc.Text = Parent.Vbc.ToString();
            tbVca.Text = Parent.Vca.ToString();
            tbI.Text = Parent.I.ToString();
            tbIa.Text = Parent.Ia.ToString();
            tbIb.Text = Parent.Ib.ToString();
            tbIc.Text = Parent.Ic.ToString();
            tbP.Text = Parent.P.ToString();
            tbQ.Text = Parent.Q.ToString();
            tbPF.Text = Parent.PF.ToString();
            tbE.Text = Parent.E.ToString();

            // Kiểm tra và tạo alarm nếu cần
            CheckAllAlarms();
        }

        private void btLoad1_Click(object sender, EventArgs e)
        {
            Load1 = !Load1;
            if (Load1 == true)
            {
                btLoad1.BackColor = Color.Red;
                Parent.WriteSingleCoil(0, 0, true);
            }
            else
            {
                btLoad1.BackColor = Color.Gray;
                Parent.WriteSingleCoil(0, 0, false);
            }
        }

        private void btLoad2_Click(object sender, EventArgs e)
        {
            Load2 = !Load2;
            if (Load2 == true)
            {
                btLoad2.BackColor = Color.Red;
                Parent.WriteSingleCoil(0, 1, true);
            }
            else
            {
                btLoad2.BackColor = Color.Gray;
                Parent.WriteSingleCoil(0, 1, false);
            }
        }

        private void btLoad3_Click(object sender, EventArgs e)
        {
            Load3 = !Load3;
            if (Load3 == true)
            {
                btLoad3.BackColor = Color.Red;
                Parent.WriteSingleCoil(0, 2, true);
            }
            else
            {
                btLoad3.BackColor = Color.Gray;
                Parent.WriteSingleCoil(0, 2, false);
            }
        }

        private void Meter_Faceplate_FormClosing(object sender, FormClosingEventArgs e)
        {
            isFormClosing = true; // Đánh dấu form đang đóng

            // Dừng timer
            if (graphUpdateTimer != null)
            {
                graphUpdateTimer.Stop();
                graphUpdateTimer.Tick -= GraphUpdateTimer_Tick;
                graphUpdateTimer.Dispose();
                graphUpdateTimer = null;
            }

            // Đặt các giá trị khác
            Parent.isShown = false;
        }

        private void cbV_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["V"] = cbV.Checked;
            parameterCurves["V"].IsVisible = cbV.Checked;
            UpdateGraph();
        }

        private void cbI_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["I"] = cbI.Checked;
            parameterCurves["I"].IsVisible = cbI.Checked;
            UpdateGraph();
        }

        private void cbVab_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Vab"] = cbVab.Checked;
            parameterCurves["Vab"].IsVisible = cbVab.Checked;
            UpdateGraph();
        }

        private void cbVbc_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Vbc"] = cbVbc.Checked;
            parameterCurves["Vbc"].IsVisible = cbVbc.Checked;
            UpdateGraph();
        }

        private void cbVca_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Vca"] = cbVca.Checked;
            parameterCurves["Vca"].IsVisible = cbVca.Checked;
            UpdateGraph();
        }

        private void cbIa_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Ia"] = cbIa.Checked;
            parameterCurves["Ia"].IsVisible = cbIa.Checked;
            UpdateGraph();
        }

        private void cbIb_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Ib"] = cbIb.Checked;
            parameterCurves["Ib"].IsVisible = cbIb.Checked;
            UpdateGraph();
        }

        private void cbIc_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Ic"] = cbIc.Checked;
            parameterCurves["Ic"].IsVisible = cbIc.Checked;
            UpdateGraph();
        }

        private void cbP_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["P"] = cbP.Checked;
            parameterCurves["P"].IsVisible = cbP.Checked;
            UpdateGraph();
        }

        private void cbQ_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["Q"] = cbQ.Checked;
            parameterCurves["Q"].IsVisible = cbQ.Checked;
            UpdateGraph();
        }

        private void cbPF_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["PF"] = cbPF.Checked;
            parameterCurves["PF"].IsVisible = cbPF.Checked;
            UpdateGraph();
        }

        private void cbE_CheckedChanged(object sender, EventArgs e)
        {
            parameterSelected["E"] = cbE.Checked;
            parameterCurves["E"].IsVisible = cbE.Checked;
            UpdateGraph();
        }

        private void btReport_Click(object sender, EventArgs e)
        {
            // Define datetime format for parsing
            string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            DateTime fromDate, toDate;

            // Validate and parse From Date
            if (!DateTime.TryParseExact(txtFromDate.Text, dateTimeFormat,
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None,
                                       out fromDate))
            {
                MessageBox.Show("Please enter valid From Date format: yyyy-MM-dd HH:mm:ss\n" +
                               "Example: 2025-05-04 00:00:00",
                               "Invalid Date Format",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFromDate.Focus();
                return;
            }

            // Validate and parse To Date
            if (!DateTime.TryParseExact(txtToDate.Text, dateTimeFormat,
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None,
                                       out toDate))
            {
                MessageBox.Show("Please enter valid To Date format: yyyy-MM-dd HH:mm:ss\n" +
                               "Example: 2025-05-04 00:00:00",
                               "Invalid Date Format",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtToDate.Focus();
                return;
            }

            // Validate date range logic
            if (fromDate >= toDate)
            {
                MessageBox.Show("From Date must be earlier than To Date",
                               "Invalid Date Range",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Retrieve data from database
            DataTable meterData = DatabaseHelper.Instance.GetMeterHistory(Parent.Name, fromDate, toDate);

            // Check if data exists
            if (meterData == null || meterData.Rows.Count == 0)
            {
                MessageBox.Show($"No data found for {Parent.Name} in the selected period:\n" +
                               $"From: {fromDate: yyyy-MM-dd HH:mm:ss}\n" +
                               $"To: {toDate: yyyy-MM-dd HH:mm:ss}",
                               "No Data Found",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show save file dialog
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveDialog.FileName = $"MeterReport_{Parent.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            saveDialog.Title = "Save Report as CSV";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Open file for writing
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveDialog.FileName))
                    {
                        // Write header row (column names)
                        string headers = string.Join(",", meterData.Columns.Cast<DataColumn>()
                                                            .Select(column => column.ColumnName));
                        sw.WriteLine(headers);

                        // Write data rows
                        foreach (DataRow row in meterData.Rows)
                        {
                            string line = string.Join(",", row.ItemArray
                                                          .Select(field => field?.ToString() ?? ""));
                            sw.WriteLine(line);
                        }
                    }

                    // Show success message
                    MessageBox.Show($"Export completed successfully!\n\n" +
                                   $"File: {saveDialog.FileName}\n" +
                                   $"Period: {fromDate: yyyy-MM-dd HH:mm:ss} to {toDate: yyyy-MM-dd HH:mm:ss}\n" +
                                   $"Records: {meterData.Rows.Count}",
                                   "Export Successful",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}",
                                   "Export Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtFromDate_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txtFromDate_Enter(object sender, EventArgs e)
        {
            if (txtFromDate.Text == "2025-05-04 00:00:00")
            {
                txtFromDate.Text = "";
                txtFromDate.ForeColor = Color.Black;
            }
        }

        private void txtFromDate_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFromDate.Text))
            {
                txtFromDate.Text = "2025-05-04 00:00:00";
                txtFromDate.ForeColor = Color.Gray;
            }
        }

        private void txtToDate_Enter(object sender, EventArgs e)
        {
            if (txtToDate.Text == "2025-05-04 00:00:00")
            {
                txtToDate.Text = "";
                txtToDate.ForeColor = Color.Black;
            }
        }

        private void txtToDate_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtToDate.Text))
            {
                txtToDate.Text = "2025-05-04 00:00:00";
                txtToDate.ForeColor = Color.Gray;
            }
        }
    }
}