using S7.Net;
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
using System.Globalization;
using static MotorControl.Motor;

namespace MotorControl
{
    public partial class Motor_Faceplate : Form
    {
        public Motor Parent;
        Image pump_green = Image.FromFile(@"images\pump_base_green.gif");
        Image pump_red = Image.FromFile(@"images\pump_base_red.gif");
        Image fault = Image.FromFile(@"images\Alarms.png");
        // Khai báo các đường trong biểu đồ
        private LineItem speedCurve;
        private LineItem setSpeedCurve;
        // Thêm timer để cập nhật biểu đồ
        private System.Windows.Forms.Timer graphUpdateTimer;
        // Thêm biến để lưu alarm
        // private List<AlarmEntry> alarmList = new List<AlarmEntry>();
        private BindingList<AlarmEntry> alarmList = new BindingList<AlarmEntry>();
        private bool alarmTriggered = false; // Để tránh ghi alarm liên tục
        private const float ALARM_THRESHOLD = 900f; // Ngưỡng báo động
        // Thiết lập khoảng thời gian hiển thị (mặc định 2 phút)
        private int timeRangeMinutes = 2;
        public Motor_Faceplate(Motor parent)
        {
            Parent = parent;
            InitializeComponent();

            // Thiết lập ZedGraph
            SetupZedGraph();
            // Cấu hình DataGridView cho alarm
            SetupAlarmGrid();

            // Khởi tạo timer để cập nhật biểu đồ (1 giây/lần)
            graphUpdateTimer = new System.Windows.Forms.Timer();
            graphUpdateTimer.Interval = 1000;
            graphUpdateTimer.Tick += GraphUpdateTimer_Tick;
            graphUpdateTimer.Start();
        }

        private void SetupAlarmGrid()
        {
            // Giả sử dgvMotor đã được tạo trong Designer
            dgvMotor.AutoGenerateColumns = false;
            dgvMotor.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);

            // Tạo các cột
            dgvMotor.Columns.Clear();

            DataGridViewTextBoxColumn timeColumn = new DataGridViewTextBoxColumn();
            timeColumn.HeaderText = "Time";
            timeColumn.Name = "Time";
            timeColumn.Width = 150;
            timeColumn.DataPropertyName = "Timestamp";
            dgvMotor.Columns.Add(timeColumn);

            DataGridViewTextBoxColumn motorColumn = new DataGridViewTextBoxColumn();
            motorColumn.HeaderText = "Motor";
            motorColumn.Name = "MotorName";
            motorColumn.Width = 100;
            motorColumn.DataPropertyName = "MotorName";
            dgvMotor.Columns.Add(motorColumn);

            DataGridViewTextBoxColumn speedColumn = new DataGridViewTextBoxColumn();
            speedColumn.HeaderText = "Speed";
            speedColumn.Name = "Speed";
            speedColumn.Width = 80;
            speedColumn.DataPropertyName = "Speed";
            dgvMotor.Columns.Add(speedColumn);

            DataGridViewTextBoxColumn descColumn = new DataGridViewTextBoxColumn();
            descColumn.HeaderText = "Description";
            descColumn.Name = "Description";
            descColumn.Width = 200;
            descColumn.DataPropertyName = "Description";
            dgvMotor.Columns.Add(descColumn);

            // Cấu hình DataSource
            dgvMotor.DataSource = alarmList;
        }
        private void SetupZedGraph()
        {
            // Lấy GraphPane từ ZedGraph control có sẵn
            GraphPane graphPane = zgMotor.GraphPane;

            // Xóa tất cả các đường hiện tại nếu có
            graphPane.CurveList.Clear();

            // Thiết lập tiêu đề và nhãn trục
            graphPane.Title.Text = "Trend Motor Speed";
            graphPane.XAxis.Title.Text = "Time";
            graphPane.YAxis.Title.Text = "Speed";

            // Tạo đường tốc độ thực tế (màu xanh)
            speedCurve = graphPane.AddCurve("Tốc độ thực tế",
                new PointPairList(), Color.Blue, SymbolType.None);
            speedCurve.Line.Width = 2.0F;

            // Tạo đường tốc độ đặt (màu đỏ, nét đứt)
            setSpeedCurve = graphPane.AddCurve("Tốc độ đặt",
                new PointPairList(), Color.Red, SymbolType.None);
            setSpeedCurve.Line.Width = 2.0F;
            setSpeedCurve.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

            // Định dạng trục thời gian
            graphPane.XAxis.Type = AxisType.Date;
            graphPane.XAxis.Scale.Format = "HH:mm:ss";
            graphPane.XAxis.Scale.MajorUnit = DateUnit.Second;
            graphPane.XAxis.Scale.MajorStep = 10; // 10 giây mỗi mốc lớn

            // Thiết lập trục Y
            graphPane.YAxis.Scale.Min = 0;
            graphPane.YAxis.Scale.Max = 60; // Giả sử tốc độ tối đa là 60Hz

            // Hiển thị lưới
            graphPane.XAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MajorGrid.IsVisible = true;
            graphPane.XAxis.MajorGrid.Color = Color.LightGray;
            graphPane.YAxis.MajorGrid.Color = Color.LightGray;

            // Tự động tính toán khoảng hiển thị
            zgMotor.AxisChange();
        }

        private void GraphUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            // Kiểm tra xem có dữ liệu không
            if (Parent.TimeStamps.Count == 0)
                return;

            // Lấy GraphPane từ ZedGraph control
            GraphPane graphPane = zgMotor.GraphPane;

            // Xóa dữ liệu cũ bằng cách tạo các PointPairList mới
            speedCurve.Points = new PointPairList();
            setSpeedCurve.Points = new PointPairList();

            // Thêm dữ liệu mới cho biểu đồ
            for (int i = 0; i < Parent.TimeStamps.Count; i++)
            {
                // Chuyển đổi DateTime sang XDate (định dạng thời gian của ZedGraph)
                double xDate = new XDate(Parent.TimeStamps[i]);

                // Thêm điểm cho đường tốc độ thực tế
                speedCurve.AddPoint(xDate, Parent.SpeedValues[i]);

                // Thêm điểm cho đường tốc độ đặt
                setSpeedCurve.AddPoint(xDate, Parent.SetSpeedValues[i]);
            }

            // Thiết lập giới hạn trục X (thời gian)
            double now = new XDate(DateTime.Now);
            graphPane.XAxis.Scale.Min = new XDate(DateTime.Now.AddMinutes(-timeRangeMinutes));
            graphPane.XAxis.Scale.Max = now;

            // Thiết lập giới hạn trục Y tự động
            double maxSpeed = 10.0; // Giá trị mặc định

            if (Parent.SpeedValues.Count > 0)
            {
                // Tìm giá trị tốc độ cao nhất và làm tròn lên
                maxSpeed = Math.Max(
                    Parent.SpeedValues.Max(),
                    Parent.SetSpeedValues.Max()
                );
                maxSpeed = Math.Ceiling(maxSpeed * 1.2); // Thêm 20% khoảng trống
            }

            // Đảm bảo trục Y có ít nhất là 10 đơn vị
            maxSpeed = Math.Max(maxSpeed, 10.0);

            graphPane.YAxis.Scale.Min = 0;
            graphPane.YAxis.Scale.Max = maxSpeed;

            // Cập nhật biểu đồ
            zgMotor.AxisChange();
            zgMotor.Invalidate();
        }
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            grName.Text = Parent.Name; // Lay ten cua tung motor hien thi len faceplate

            if (Parent.RunFeedback)
            {
                pbRunFeedback.BackgroundImage = pump_green;
            }
            else
            {
                pbRunFeedback.BackgroundImage = pump_red;
            }
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);


            lbSpeed.Text = Parent.Speed.ToString();
            barSpeed.Value = (int)Parent.Speed;

            // Kiểm tra ngưỡng alarm
            CheckSpeedAlarm();

            if (Parent.Fault)
            {
                pbFault.BackgroundImage = fault;
                pbFault.Visible = true;
            }
            else
            {
                pbFault.Visible = false;
            }
        }
        private void CheckSpeedAlarm()
        {
            // Kiểm tra nếu tốc độ vượt ngưỡng
            if (Parent.Speed > ALARM_THRESHOLD)
            {
                if (!alarmTriggered) // Chỉ ghi alarm một lần khi vượt ngưỡng
                {
                    // Tạo alarm mới
                    AlarmEntry newAlarm = new AlarmEntry
                    {
                        Timestamp = DateTime.Now,
                        MotorName = Parent.Name,
                        Speed = Parent.Speed,
                        Description = $"Tốc độ vượt ngưỡng {ALARM_THRESHOLD} Hz",
                        IsActive = true
                    };

                    // Thêm vào list
                    alarmList.Add(newAlarm);

                    // Cập nhật DataGridView
                    // RefreshAlarmGrid();
                    // Lưu alarm vào database
                    // Lưu alarm vào database
                    DatabaseHelper.Instance.SaveMotorAlarm(newAlarm);

                    alarmTriggered = true;
                    HighlightAlarm();

                }
            }
            else
            {
                alarmTriggered = false; // Reset flag khi tốc độ về dưới ngưỡng
                HighlightAlarm();
            }
        }

        private void RefreshAlarmGrid()
        {
            // Sử dụng BindingList để cập nhật tự động
            if (dgvMotor.DataSource == null)
            {
                dgvMotor.DataSource = new BindingList<AlarmEntry>(alarmList);
            }
            else
            {
                // Refresh the existing binding
                dgvMotor.Refresh();
            }

            // Đợi một chút để DataGridView cập nhật
            Application.DoEvents();

            // Cuộn xuống dòng mới nhất
            if (dgvMotor.Rows.Count > 0)
            {
                try
                {
                    int lastRowIndex = dgvMotor.Rows.Count - 1;
                    if (lastRowIndex >= 0)
                    {
                        dgvMotor.FirstDisplayedScrollingRowIndex = lastRowIndex;

                        // Đảm bảo dòng cuối được hiển thị
                        dgvMotor.Rows[lastRowIndex].Selected = true;
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    Console.WriteLine($"Error scrolling to last row: {ex.Message}");
                }
            }
        }

        private void HighlightAlarm()
        {

            // Làm nổi bật cảnh báo (tùy chọn)
            if (alarmTriggered)
            {
                lbSpeed.ForeColor = Color.Red;
                System.Media.SystemSounds.Exclamation.Play();
            }
            else { lbSpeed.ForeColor = Color.Black; }
        }
        // Phan dieu khien tu Scada xuong PLC

        private void btStart_MouseDown(object sender, MouseEventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBX0.0", true); }
            else if (plc.Type == "OPC") { Parent.WriteBoolean("Start", true); }
        }

        private void btStart_MouseUp(object sender, MouseEventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBX0.0", false); }
            else if (plc.Type == "OPC") { Parent.WriteBoolean("Start", false); }
        }

        private void btStop_MouseDown(object sender, MouseEventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBX0.1", true); }
            else if (plc.Type == "OPC") { Parent.WriteBoolean("Stop", true); }
        }

        private void btStop_MouseUp(object sender, MouseEventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBX0.1", false); }
            else if (plc.Type == "OPC") { Parent.WriteBoolean("Stop", false); }
        }

        private void btReset_MouseDown(object sender, MouseEventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBX0.3", true); }
            else if (plc.Type == "OPC") { Parent.WriteBoolean("Reset", true); }
        }

        private void btReset_MouseUp(object sender, MouseEventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBX0.3", false); Parent.Write("DBD2", 0); }
            else if (plc.Type == "OPC") { Parent.WriteBoolean("Reset", false); Parent.WriteFloat("SetSpeed", 0); }
        }

        private void barSetSpeed_Scroll(object sender, EventArgs e)
        {
            PLC plc = Parent.Parent.FindPLC(Parent.PLCName);
            if (plc.Type == "S7") { Parent.Write("DBD2", (float)barSetSpeed.Value); }
            else if (plc.Type == "OPC") { Parent.WriteFloat("SetSpeed", (float)barSetSpeed.Value); }
        }

        private void Motor_Faceplate_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (graphUpdateTimer != null)
                graphUpdateTimer.Stop();

            Parent.isShown = false;
        }

        private void Motor_Faceplate_Load(object sender, EventArgs e)
        {

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
            DataTable motorData = DatabaseHelper.Instance.GetMotorHistory(Parent.Name, fromDate, toDate);

            // Check if data exists
            if (motorData == null || motorData.Rows.Count == 0)
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
            saveDialog.FileName = $"MotorReport_{Parent.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            saveDialog.Title = "Save Report as CSV";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Open file for writing
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveDialog.FileName))
                    {
                        // Write header row (column names)
                        string headers = string.Join(",", motorData.Columns.Cast<DataColumn>()
                                                            .Select(column => column.ColumnName));
                        sw.WriteLine(headers);

                        // Write data rows
                        foreach (DataRow row in motorData.Rows)
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
                                   $"Records: {motorData.Rows.Count}",
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
    }
}
