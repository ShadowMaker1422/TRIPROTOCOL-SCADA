namespace MotorControl
{
    partial class Motor_Faceplate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grName = new System.Windows.Forms.GroupBox();
            this.zgMotor = new ZedGraph.ZedGraphControl();
            this.btReset = new System.Windows.Forms.Button();
            this.pbFault = new System.Windows.Forms.PictureBox();
            this.barSetSpeed = new System.Windows.Forms.TrackBar();
            this.btStart = new System.Windows.Forms.Button();
            this.barSpeed = new System.Windows.Forms.ProgressBar();
            this.btStop = new System.Windows.Forms.Button();
            this.lbSpeed = new System.Windows.Forms.Label();
            this.pbRunFeedback = new System.Windows.Forms.PictureBox();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Dashboard = new System.Windows.Forms.TabPage();
            this.Alarm = new System.Windows.Forms.TabPage();
            this.dgvMotor = new System.Windows.Forms.DataGridView();
            this.Report = new System.Windows.Forms.TabPage();
            this.txtToDate = new System.Windows.Forms.TextBox();
            this.txtFromDate = new System.Windows.Forms.TextBox();
            this.lblFormat = new System.Windows.Forms.Label();
            this.lblToDate = new System.Windows.Forms.Label();
            this.lblFromDate = new System.Windows.Forms.Label();
            this.btReport = new System.Windows.Forms.Button();
            this.grName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFault)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barSetSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRunFeedback)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.Dashboard.SuspendLayout();
            this.Alarm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMotor)).BeginInit();
            this.Report.SuspendLayout();
            this.SuspendLayout();
            // 
            // grName
            // 
            this.grName.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.grName.Controls.Add(this.zgMotor);
            this.grName.Controls.Add(this.btReset);
            this.grName.Controls.Add(this.pbFault);
            this.grName.Controls.Add(this.barSetSpeed);
            this.grName.Controls.Add(this.btStart);
            this.grName.Controls.Add(this.barSpeed);
            this.grName.Controls.Add(this.btStop);
            this.grName.Controls.Add(this.lbSpeed);
            this.grName.Controls.Add(this.pbRunFeedback);
            this.grName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grName.Location = new System.Drawing.Point(3, 3);
            this.grName.Name = "grName";
            this.grName.Size = new System.Drawing.Size(341, 489);
            this.grName.TabIndex = 9;
            this.grName.TabStop = false;
            this.grName.Text = "Motor #1";
            // 
            // zgMotor
            // 
            this.zgMotor.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zgMotor.Location = new System.Drawing.Point(3, 317);
            this.zgMotor.Name = "zgMotor";
            this.zgMotor.ScrollGrace = 0D;
            this.zgMotor.ScrollMaxX = 0D;
            this.zgMotor.ScrollMaxY = 0D;
            this.zgMotor.ScrollMaxY2 = 0D;
            this.zgMotor.ScrollMinX = 0D;
            this.zgMotor.ScrollMinY = 0D;
            this.zgMotor.ScrollMinY2 = 0D;
            this.zgMotor.Size = new System.Drawing.Size(335, 169);
            this.zgMotor.TabIndex = 19;
            this.zgMotor.UseExtendedPrintDialog = true;
            // 
            // btReset
            // 
            this.btReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btReset.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.btReset.Location = new System.Drawing.Point(18, 129);
            this.btReset.Name = "btReset";
            this.btReset.Size = new System.Drawing.Size(111, 40);
            this.btReset.TabIndex = 18;
            this.btReset.Text = "RESET";
            this.btReset.UseVisualStyleBackColor = true;
            this.btReset.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btReset_MouseDown);
            this.btReset.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btReset_MouseUp);
            // 
            // pbFault
            // 
            this.pbFault.BackColor = System.Drawing.Color.Transparent;
            this.pbFault.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbFault.Location = new System.Drawing.Point(246, 101);
            this.pbFault.Name = "pbFault";
            this.pbFault.Size = new System.Drawing.Size(40, 36);
            this.pbFault.TabIndex = 17;
            this.pbFault.TabStop = false;
            // 
            // barSetSpeed
            // 
            this.barSetSpeed.Location = new System.Drawing.Point(18, 187);
            this.barSetSpeed.Margin = new System.Windows.Forms.Padding(2);
            this.barSetSpeed.Maximum = 1000;
            this.barSetSpeed.Name = "barSetSpeed";
            this.barSetSpeed.Size = new System.Drawing.Size(280, 45);
            this.barSetSpeed.TabIndex = 16;
            this.barSetSpeed.Scroll += new System.EventHandler(this.barSetSpeed_Scroll);
            // 
            // btStart
            // 
            this.btStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btStart.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.btStart.Location = new System.Drawing.Point(18, 19);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(111, 41);
            this.btStart.TabIndex = 1;
            this.btStart.Text = "START";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btStart_MouseDown);
            this.btStart.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btStart_MouseUp);
            // 
            // barSpeed
            // 
            this.barSpeed.Location = new System.Drawing.Point(18, 237);
            this.barSpeed.Maximum = 1000;
            this.barSpeed.Name = "barSpeed";
            this.barSpeed.Size = new System.Drawing.Size(280, 30);
            this.barSpeed.TabIndex = 7;
            // 
            // btStop
            // 
            this.btStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btStop.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.btStop.Location = new System.Drawing.Point(18, 75);
            this.btStop.Name = "btStop";
            this.btStop.Size = new System.Drawing.Size(111, 40);
            this.btStop.TabIndex = 1;
            this.btStop.Text = "STOP";
            this.btStop.UseVisualStyleBackColor = true;
            this.btStop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btStop_MouseDown);
            this.btStop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btStop_MouseUp);
            // 
            // lbSpeed
            // 
            this.lbSpeed.AutoSize = true;
            this.lbSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSpeed.Location = new System.Drawing.Point(13, 287);
            this.lbSpeed.Name = "lbSpeed";
            this.lbSpeed.Size = new System.Drawing.Size(70, 25);
            this.lbSpeed.TabIndex = 6;
            this.lbSpeed.Text = "label2";
            // 
            // pbRunFeedback
            // 
            this.pbRunFeedback.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbRunFeedback.Location = new System.Drawing.Point(193, 40);
            this.pbRunFeedback.Name = "pbRunFeedback";
            this.pbRunFeedback.Size = new System.Drawing.Size(105, 106);
            this.pbRunFeedback.TabIndex = 4;
            this.pbRunFeedback.TabStop = false;
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Enabled = true;
            this.UpdateTimer.Interval = 250;
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Dashboard);
            this.tabControl1.Controls.Add(this.Alarm);
            this.tabControl1.Controls.Add(this.Report);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(355, 521);
            this.tabControl1.TabIndex = 10;
            // 
            // Dashboard
            // 
            this.Dashboard.Controls.Add(this.grName);
            this.Dashboard.Location = new System.Drawing.Point(4, 22);
            this.Dashboard.Name = "Dashboard";
            this.Dashboard.Padding = new System.Windows.Forms.Padding(3);
            this.Dashboard.Size = new System.Drawing.Size(347, 495);
            this.Dashboard.TabIndex = 0;
            this.Dashboard.Text = "Dashboard";
            this.Dashboard.UseVisualStyleBackColor = true;
            // 
            // Alarm
            // 
            this.Alarm.Controls.Add(this.dgvMotor);
            this.Alarm.Location = new System.Drawing.Point(4, 22);
            this.Alarm.Name = "Alarm";
            this.Alarm.Padding = new System.Windows.Forms.Padding(3);
            this.Alarm.Size = new System.Drawing.Size(347, 495);
            this.Alarm.TabIndex = 1;
            this.Alarm.Text = "Alarm";
            this.Alarm.UseVisualStyleBackColor = true;
            // 
            // dgvMotor
            // 
            this.dgvMotor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMotor.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvMotor.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvMotor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMotor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMotor.Location = new System.Drawing.Point(3, 3);
            this.dgvMotor.Name = "dgvMotor";
            this.dgvMotor.Size = new System.Drawing.Size(341, 489);
            this.dgvMotor.TabIndex = 0;
            // 
            // Report
            // 
            this.Report.Controls.Add(this.txtToDate);
            this.Report.Controls.Add(this.txtFromDate);
            this.Report.Controls.Add(this.lblFormat);
            this.Report.Controls.Add(this.lblToDate);
            this.Report.Controls.Add(this.lblFromDate);
            this.Report.Controls.Add(this.btReport);
            this.Report.Location = new System.Drawing.Point(4, 22);
            this.Report.Name = "Report";
            this.Report.Size = new System.Drawing.Size(347, 495);
            this.Report.TabIndex = 2;
            this.Report.Text = "Report";
            this.Report.UseVisualStyleBackColor = true;
            // 
            // txtToDate
            // 
            this.txtToDate.Location = new System.Drawing.Point(157, 78);
            this.txtToDate.Name = "txtToDate";
            this.txtToDate.Size = new System.Drawing.Size(123, 20);
            this.txtToDate.TabIndex = 8;
            this.txtToDate.Text = "2025-05-04 00:00:00";
            this.txtToDate.TextChanged += new System.EventHandler(this.txtFromDate_TextChanged);
            this.txtToDate.Enter += new System.EventHandler(this.txtToDate_Enter);
            this.txtToDate.Leave += new System.EventHandler(this.txtToDate_Leave);
            // 
            // txtFromDate
            // 
            this.txtFromDate.Location = new System.Drawing.Point(157, 23);
            this.txtFromDate.Name = "txtFromDate";
            this.txtFromDate.Size = new System.Drawing.Size(123, 20);
            this.txtFromDate.TabIndex = 8;
            this.txtFromDate.Text = "2025-05-04 00:00:00";
            this.txtFromDate.TextChanged += new System.EventHandler(this.txtFromDate_TextChanged);
            this.txtFromDate.Enter += new System.EventHandler(this.txtFromDate_Enter);
            this.txtFromDate.Leave += new System.EventHandler(this.txtFromDate_Leave);
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFormat.Location = new System.Drawing.Point(12, 225);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(313, 25);
            this.lblFormat.TabIndex = 7;
            this.lblFormat.Text = "Format: yyyy-MM-dd HH:mm:ss";
            // 
            // lblToDate
            // 
            this.lblToDate.AutoSize = true;
            this.lblToDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToDate.Location = new System.Drawing.Point(12, 72);
            this.lblToDate.Name = "lblToDate";
            this.lblToDate.Size = new System.Drawing.Size(94, 25);
            this.lblToDate.TabIndex = 7;
            this.lblToDate.Text = "To Date:";
            // 
            // lblFromDate
            // 
            this.lblFromDate.AutoSize = true;
            this.lblFromDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFromDate.Location = new System.Drawing.Point(12, 19);
            this.lblFromDate.Name = "lblFromDate";
            this.lblFromDate.Size = new System.Drawing.Size(118, 25);
            this.lblFromDate.TabIndex = 7;
            this.lblFromDate.Text = "From Date:";
            // 
            // btReport
            // 
            this.btReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btReport.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.btReport.Location = new System.Drawing.Point(107, 142);
            this.btReport.Name = "btReport";
            this.btReport.Size = new System.Drawing.Size(111, 41);
            this.btReport.TabIndex = 2;
            this.btReport.Text = "REPORT";
            this.btReport.UseVisualStyleBackColor = true;
            this.btReport.Click += new System.EventHandler(this.btReport_Click);
            // 
            // Motor_Faceplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(355, 521);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Motor_Faceplate";
            this.Text = "Motor_Faceplate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Motor_Faceplate_FormClosing);
            this.Load += new System.EventHandler(this.Motor_Faceplate_Load);
            this.grName.ResumeLayout(false);
            this.grName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFault)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barSetSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRunFeedback)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.Dashboard.ResumeLayout(false);
            this.Alarm.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMotor)).EndInit();
            this.Report.ResumeLayout(false);
            this.Report.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grName;
        private System.Windows.Forms.PictureBox pbFault;
        private System.Windows.Forms.TrackBar barSetSpeed;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.ProgressBar barSpeed;
        private System.Windows.Forms.Button btStop;
        private System.Windows.Forms.Label lbSpeed;
        private System.Windows.Forms.PictureBox pbRunFeedback;
        private System.Windows.Forms.Button btReset;
        private System.Windows.Forms.Timer UpdateTimer;
        private ZedGraph.ZedGraphControl zgMotor;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Dashboard;
        private System.Windows.Forms.TabPage Alarm;
        private System.Windows.Forms.DataGridView dgvMotor;
        private System.Windows.Forms.TabPage Report;
        private System.Windows.Forms.Label lblToDate;
        private System.Windows.Forms.Label lblFromDate;
        private System.Windows.Forms.Button btReport;
        private System.Windows.Forms.TextBox txtFromDate;
        private System.Windows.Forms.TextBox txtToDate;
        private System.Windows.Forms.Label lblFormat;
    }
}