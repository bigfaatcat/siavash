using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using RMU_Project.Properties;
using FTD2XX_NET;
using System.IO;

namespace RMU_Project
{
    using OxyPlot;
    using OxyPlot.Series;
    using OxyPlot.Axes;
    
    public partial class Form1 : Form
    {
        public Point mouseLocation;
        System.Timers.Timer t;
        int h, m, s;
        byte[] Header = new byte[3];
        byte[] tx_byte = new byte[20];
        byte temp1,temp2, tempF, step=1, TempH1,TempH2,tempRX;
        UInt16 temp3,temp3buf,temp4,tempFrame, temp3a,temp3b,temp3c;
        float temp5,temp5buf, temp6,temp5a,temp5b;
        double tempfilter; 
        UInt16[] RAM = new UInt16[8192];
        int i = 0;
        int DateTime = 1;
        int logger = 1;
        int saver = 1;
        UInt16[] filterdata = new UInt16[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        int filterpointer=0;
        UInt64 filtersum = 0;
        float avgPoints, Temporal1, Temporal2;
        StreamWriter DataLog1, DataLog2;
        bool StrmWriting = false;
        public enum state
        {
            Header, FirstByte, SecondByte, Byte3rd, Byte4th, Byte5th, Byte6th, Byte7th, Byte8th, Byte9th, Byte10th, Byte11th, Byte12th, Byte13th, Byte14th

        };
        state rx_state;
        double x1 = 0;
        double x2 = 0;
        int Thre_cnt = 50;
        int RefreshCounter1 = 0;
        int RefreshCounter2 = 0;
        int RefreshCounter3 = 0;
        int RefreshCounter4 = 0;
  
        int    timeCS = 0;
        int    timeSec = 0;
        int    timeMin = 0;
        int    timeHrs = 0;
        int flag1 = 0;
        int flag2 = 0;
        int flag3 = 1;
        int flag4 = 1;
        int flg_alram = 0;


        public Form1()
        {
            
            InitializeComponent();
            tx_byte[0] = 0x05;
            tx_byte[1] = 0x01;
            timeCS = 0;
            timeSec = 0;
            timeMin = 0;
            timeHrs = 0;
            this.comboBox1.SelectedItem = 0;
            this.comboBox2.SelectedItem = 0;
            this.comboBox3.SelectedItem = 0;
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            this.AutoScroll = true;
            this.AutoSize = true;

            var myModel1 = new PlotModel()
            {
   
                Title = "Reserve for ADC (Digit)",
                TextColor = OxyColors.White,
                PlotMargins = new OxyPlot.OxyThickness(40, 20, 20, 30)
            };
            var line_position1 = new LineSeries { 
                
                LineStyle = LineStyle.Solid , StrokeThickness = 1,  Color = OxyColors.Red
                                /*Smooth = false,
                                StrokeThickness = 0.5,
                                Color = OxyColors.Transparent,
                                MarkerStroke = OxyColors.White,
                                MarkerFill   = OxyColors.Red,
                                MarkerType   = MarkerType.Circle*/
            
                                               };

            OxyPlot.Axes.LinearAxis LAY1 = new OxyPlot.Axes.LinearAxis()
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                AbsoluteMaximum = 655536,
                AbsoluteMinimum = 0,
            };

            myModel1.Axes.Add(LAY1);
            myModel1.Series.Add(line_position1);
            this.plot1.Model = myModel1;
            plot1.Model.Series[0].IsVisible = true;


            var myModel2 = new PlotModel()
            {

                Title = "Reserve for Heater Current (A)",
                TextColor = OxyColors.White,
                PlotMargins = new OxyPlot.OxyThickness(40, 20, 20, 30)
            };
            var line_position2 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 1,  Color = OxyColors.Red};
            //var line_position3 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 1, Smooth = false, Color = OxyColors.Blue};
            OxyPlot.Axes.LinearAxis LAY2 = new OxyPlot.Axes.LinearAxis()
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                AbsoluteMaximum = 20f,
                AbsoluteMinimum = -20f,
            };

            myModel2.Axes.Add(LAY2);
            myModel2.Series.Add(line_position2);
            //myModel2.Series.Add(line_position3);
            this.plot2.Model = myModel2;
            plot2.Model.Series[0].IsVisible = true;
            //plot2.Model.Series[1].IsVisible = true;




        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 3;
            this.comboBox3.SelectedIndex = 0;
            t = new System.Timers.Timer();
            t.Interval = 1000;
            timer1.Interval = 1; 
            t.Elapsed += OnTimeEvent;

            tx_byte[0] = 0x07;
            tx_byte[1] = 0x3A;
            tx_byte[2] = 0xB6;
            ckbx_OpMode.SelectedIndex = 1;
            
            Array ports = System.IO.Ports.SerialPort.GetPortNames();


            for (int x = 0; x < ports.Length; x++)
            {
                cmbbx_ComPort.Items.Add(ports.GetValue(x));
            }
            //cmbbx_ChannelSelect.SelectedIndex = 0;
          
           
            try
            {

                if (SerialPort_RMU_RX.IsOpen == true)

                {
                    SerialPort_RMU_RX.Close();
                }
                SerialPort_RMU_RX.PortName = cmbbx_ComPort.Text;
                SerialPort_RMU_RX.Open();

            }
            catch
            {
                //MessageBox.Show("Incorrect Port");
            }

          
            
            
            Control.CheckForIllegalCrossThreadCalls = false;



         
            rx_state = state.Header;
                     

        }

        private void OnTimeEvent(object sender, System.Timers.ElapsedEventArgs e)
        {

            Invoke(new Action(() =>
            {

                s += 1;
                if (s == 60)
                {
                    s = 0;
                    m += 1;

                }
                if (m == 60)
                {
                    m = 0;
                    h += 1;

                }
                textTimer.Text = string.Format("{0}:{1}:{2}", h.ToString().PadLeft(2, '0'), m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
            }));
        }

        private void Btn_IAGC_On_Click(object sender, EventArgs e)
        {

        }

        private void TxtBx_Rp_TextChanged(object sender, EventArgs e)
        {
            try { }
            catch
            {
                txtbx_FontDiffAzAtt.Text = "0";
            }
        }

        private void TxtBx_GainSLB_TextChanged(object sender, EventArgs e)
        {
            try { }
            catch
            {
                TxtBx_RearAtt.Text = "0";
            }

        }

        private void TxtBx_GainRX_TextChanged(object sender, EventArgs e)
        {
            try { }
            catch
            {
                TxtBx_FrontSumAtt.Text = "0";
            }

        }





        private void SerialPort_RMU_RX_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                byte Rx_Byte = 0;
            next_byte:

                Rx_Byte = (byte)SerialPort_RMU_RX.ReadByte();
                //TxtBx_RX_Command.AppendText(" "+Rx_Byte.ToString("X"));

               
                switch (rx_state)
                {
                    case state.Header:
                        Header[0] = Header[1];
                        Header[1] = Header[2];
                        Header[2] = Rx_Byte;

                        if (Convert.ToChar(Header[0]) == 0x07 && Convert.ToChar(Header[1]) == 0x3A &&
                            Convert.ToChar(Header[2]) == 0xB6 )
                        {
                            rx_state = state.FirstByte;
                        }
                        break;


                    case state.FirstByte:
                        TempH1 = Rx_Byte;
                        rx_state = state.SecondByte;
                        break;

                    case state.SecondByte:
                        TempH2 = Rx_Byte;
                        rx_state = state.Byte3rd;
                        break;

                    case state.Byte3rd:
                        if (TempH1 == 0x00 && TempH2 == 0x00)
                        {
                            temp1 = Rx_Byte;
                            rx_state = state.Byte4th;
                        }
                        else
                        {
                            rx_state = state.Header;
                            temp1 = 0;
                        }
                        break;

                    case state.Byte4th:
                    
                         temp3 = ((UInt16)((temp1 << 8) + Rx_Byte));
                         //temp3 = Convert.ToUInt16(txtbx_Ferequency.Text);
                         if (temp3 > 8193)
                         {
                             temp3 = temp3;

                         }                  

                         temp5 = (temp3) * (2.5f / 4096.0f);
                         if (temp3 < 1250) temp5 = (temp3) * (2.25f / 4096.0f);
                       

                            if (temp5 < 0f)
                            {
                                radLabel17.Text = "Under_Current";
                                radLabel17.ForeColor = Color.Blue;
                                //StrmWriting = false;

                            }
                            else if (temp5 > 20f)
                            {
                                radLabel17.Text = "Over_Current";
                                radLabel17.ForeColor = Color.Red;
                            }
                            else
                            {
                                radLabel17.Text = "Normal";
                                radLabel17.ForeColor = Color.Green;
                            }

                         txtbx_FreqDev.Text = (temp5).ToString();
                         txtbx_Ferequency.Text = (((UInt16)temp1 << 8) + Rx_Byte).ToString(); 
                         //string[] msg1 = { "Digital:", temp3.ToString(), "Voltage:", temp5.ToString()};
                         //string[] msg2 = { "Digital:", temp4.ToString(), "Voltage:", temp6.ToString()};

                         if (StrmWriting == true)
                         {
                             
                             try
                             {

                                 if (logger == 1)
                                 {
                                     DateTime dt = dateTimePicker1.Value;
                                     DataLog1.WriteLine(dt.ToString());
                                     DataLog2.WriteLine(dt.ToString());
                                     DataLog1.WriteLine(Version.Text.ToString());
                                     DataLog2.WriteLine(Version.Text.ToString());
                                     logger = 2;
                                 }

                                 RefreshCounter1++;
                                 //RAM [RefreshCounter1] = temp3;
                                 //if (RefreshCounter1 == 8191)
                                 //{   
                                  // RefreshCounter1 = 0;
                                // }

                                 if (RefreshCounter1 == 26)
                                {

                                     //DataLog1.WriteLine(temp3);
                                     //DataLog1.WriteLine(textBox1.Text.ToString());
                                     //DataLog2.WriteLine(temp5);
                                     //DataLog2.WriteLine(textBox1.Text.ToString());
                                     //RefreshCounter1 = 0;
                                     //DateTime = 1;
                                     //radLabel18.Text = "-";

                                     if (temp5 > 0.25f)
                                     {

                                         DataLog1.WriteLine(temp3);
                                         DataLog1.WriteLine(textBox1.Text.ToString());
                                         DataLog2.WriteLine(temp5);
                                         DataLog2.WriteLine(textBox1.Text.ToString());
                                         RefreshCounter1 = 0;
                                         DateTime = 1;
                                         radLabel18.Text = "Logging";
                                         radLabel18.ForeColor = Color.Green;
                                     }
                                     else
                                     {
                                         RefreshCounter1 = 0;
                                         radLabel18.Text = " Under-Current Det. \r\n Press Stop Logging";
                                         if (flg_alram == 0)
                                         {
                                             flg_alram = 1;
                                             radLabel18.ForeColor = Color.White;
                                         }
                                         else
                                         {
                                             flg_alram = 0;
                                             radLabel18.ForeColor = Color.Red;
                                         }
                                         if (DateTime == 1)
                                         {
                                             DateTime = 2;
                                             DateTime dt = dateTimePicker1.Value;
                                             DataLog1.WriteLine(dt.ToString());
                                             DataLog2.WriteLine(dt.ToString());
                                             DataLog1.WriteLine(Version.Text.ToString());
                                             DataLog2.WriteLine(Version.Text.ToString());

                                         }
                                      
                                         
                                     }
                                      
                                      


                                 }


                                 RefreshCounter3++;

                                 if (RefreshCounter3 ==10)
                                 {

                                     if (flag3 == 1)
                                     {
                                         x1 = ((LineSeries)this.plot1.Model.Series[0]).Points.Count > 0 ? ((LineSeries)this.plot1.Model.Series[0]).Points[((LineSeries)this.plot1.Model.Series[0]).Points.Count - 1].X + 1 : 0;

                                         if (((LineSeries)this.plot1.Model.Series[0]).Points.Count >= 250)
                                         {
                                             ((LineSeries)this.plot1.Model.Series[0]).Points.RemoveAt(0);
                                         }
                                         ((LineSeries)this.plot1.Model.Series[0]).Points.Add(new DataPoint(x1, temp3));

                                     }
                                     if (flag4 == 1)
                                     {
                                         x2 = ((LineSeries)this.plot2.Model.Series[0]).Points.Count > 0 ? ((LineSeries)this.plot2.Model.Series[0]).Points[((LineSeries)this.plot2.Model.Series[0]).Points.Count - 1].X + 1 : 0;

                                         if (((LineSeries)this.plot2.Model.Series[0]).Points.Count >= 250)
                                         {
                                             ((LineSeries)this.plot2.Model.Series[0]).Points.RemoveAt(0);
                                             //((LineSeries)this.plot2.Model.Series[1]).Points.RemoveAt(0);
                                         }
                                         ((LineSeries)this.plot2.Model.Series[0]).Points.Add(new DataPoint(x2, temp5));
                                         //((LineSeries)this.plot2.Model.Series[1]).Points.Add(new DataPoint(x2, temp6));
                                     }



                                    RefreshCounter2++;
                                    if (RefreshCounter2 == 10)
                                    {
                                        RefreshCounter2 = 0;
                                        this.plot1.InvalidatePlot(true);
                                        this.plot2.InvalidatePlot(true);
                                    }


                                     RefreshCounter3 = 0;
                                 }

                             }
                             catch
                             {
                                 MessageBox.Show("failed to write into file");
                             }


                         }


                         rx_state = state.Header;
                         break;
                   
                }
                if (SerialPort_RMU_RX.BytesToRead != 0) goto next_byte;
            }
            catch { };
        }

        private void cmbbx_ComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SerialPort_RMU_RX.IsOpen == true)
                {
                    SerialPort_RMU_RX.Close();
                }
                SerialPort_RMU_RX.PortName = cmbbx_ComPort.Text;
                SerialPort_RMU_RX.BaudRate = Convert.ToInt32(txtbx_SerialPortBaudRate.Text);
                SerialPort_RMU_RX.Open();


            }
            catch
            {
                MessageBox.Show("Incorrect Port");
            }
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {

            Application.Exit();
        }

        private void chkbx_cont_send_CheckedChanged(object sender, EventArgs e)
        {
            if (chkbx_cont_send.Checked)
            {
                send_timer.Enabled = true;

            }
            else
            {
                send_timer.Enabled = false;
            }
        }

        private void send_timer_Tick(object sender, EventArgs e)
        {

            button1_Click(null, e);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            UInt16 temptx1 = 0;
            UInt16 temptx2 = 0;
            UInt16 temptx3 = 0;
            UInt16 temptx3Buff = 0;
            UInt16 temptx4 = 0;
            UInt16 temptx5 = 0;
            UInt16 temptx6 = 0;

            byte Btemp = 0;
                try
                {

                    temptx1 = Convert.ToUInt16(txtbx_digital_sumAtt.Text);
                    if (temptx1 > 101)
                    {
                        temptx1 = 10; //default 10-us
                        radLabel22.Text = "Over Range (Def. 10-uS)";
                    }
                    else
                    {
                        radLabel22.Text = "-";
                    }

                    radLabel7.Text = (comboBox2.GetItemText(comboBox2.SelectedItem)) + "-uS";
                    temptx2 = Convert.ToUInt16(comboBox2.GetItemText(comboBox2.SelectedItem));

                            /*if (temptx2 == 5)  { tx_byte[4] = 0x05; }
                            if (temptx2 == 10) { tx_byte[4] = 0x0A; }
                            if (temptx2 == 15) { tx_byte[4] = 0x0F; }
                            if (temptx2 == 20) { tx_byte[4] = 0x14; }
                            if (temptx2 == 25) { tx_byte[4] = 0x19; }
                            if (temptx2 == 30) { tx_byte[4] = 0x1E; }
                            if (temptx2 == 35) { tx_byte[4] = 0x23; }
                            if (temptx2 == 40) { tx_byte[4] = 0x28; }
                            if (temptx2 == 50) { tx_byte[4] = 0x32; }
                            if (temptx2 == 60) { tx_byte[4] = 0x3C; }
                            if (temptx2 == 70) { tx_byte[4] = 0x46; }
                            if (temptx2 == 80) { tx_byte[4] = 0x50; }*/

                    radLabel23.Text = (comboBox1.GetItemText(comboBox1.SelectedItem)) + "-Hz";
                    temptx3Buff = Convert.ToUInt16(comboBox1.GetItemText(comboBox1.SelectedItem));

                            if (temptx3Buff == 500)  { temptx3 = 1999; }
                            if (temptx3Buff == 750)  { temptx3 = 1332; }
                            if (temptx3Buff == 1000) { temptx3 = 999; }
                            if (temptx3Buff == 1500) { temptx3 = 666; }
                            if (temptx3Buff == 2000) { temptx3 = 499; }
                            if (temptx3Buff == 2500) { temptx3 = 399; }
                            if (temptx3Buff == 3000) { temptx3 = 332; }
                            if (temptx3Buff == 4000) { temptx3 = 249; }
                            if (temptx3Buff == 5000) { temptx3 = 199; }

                    radLabel24.Text = (comboBox3.GetItemText(comboBox3.SelectedItem)) + "-mS";
                    temptx4 = Convert.ToUInt16(comboBox3.GetItemText(comboBox3.SelectedItem));


                    temptx5 = Convert.ToUInt16(txtbx_digital_diffazAtt.Text);
                    if (temptx5 > 151)
                    {
                        temptx5 = 30; //default 30-us
                        radLabel26.Text = "Over Range (Def. 30-uS)";
                    }
                    else
                    {
                        radLabel26.Text = "-";
                    }


                    temptx6 = Convert.ToUInt16(txtbx_digital_diffelAtt.Text);
                    if (temptx6 > 80)
                    {
                        temptx6 = 16; //default 30-us
                        radLabel26.Text = "Over Range (Def. 16-uS)";
                    }
                    else
                    {
                        radLabel27.Text = "-";
                    }

                }
                catch
                {
                    temptx1 = 10;
                    txtbx_digital_sumAtt.Text = "10";

                    temptx5 = 30;
                    txtbx_digital_diffazAtt.Text = "30";
                  
                }

                tx_byte[3] = (byte)(temptx1);
                tx_byte[4] = (byte)(temptx2);
                tx_byte[5] = (byte)(temptx3 >> 8);
                tx_byte[6] = (byte)(temptx3);
                tx_byte[7] = (byte)(temptx4 >> 8);
                tx_byte[8] = (byte)(temptx4);
                tx_byte[9] = (byte)(temptx5);
                tx_byte[10] = (byte)(temptx6);
                tx_byte[11] = 0x00;
                tx_byte[12] = 0x00;

                SerialPort_RMU_RX.Write(tx_byte, 0, 13);

        }

        private void txtbx_digital_index_TextChanged(object sender, EventArgs e)

        {
            try
            {
                //lbl_index_hex.Text = Convert.ToUInt16(txtbx_digital_index.Text).ToString("X");
            }
            catch { }
        }

        private void txtbx_step_TextChanged(object sender, EventArgs e)
        {
            try
            {
                step = Convert.ToByte(txtbx_step.Text);
            }
            catch { };
        }

        private void txtbx_digital_index_KeyUp(object sender, KeyEventArgs e)
        {
            UInt16 temp = 0;
           try{
            //temp= Convert.ToUInt16(txtbx_digital_index.Text);
            if (e.KeyCode == Keys.Up )
            {
                if (temp <= 40000 - step)
                {
                    temp =(UInt16)(temp + step);
                }
                //txtbx_digital_index.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (temp >= 0+ step)
                {
                    temp = (UInt16)(temp - step);
                }
                //txtbx_digital_index.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Enter)
            {
               // txtbx_digital_index.Text = temp.ToString();
                button1_Click(null, e);
            }
           }
           catch { }
        }

        private void txtbx_digital_rearAtt_KeyUp(object sender, KeyEventArgs e)
        {
            byte temp = 0;
            try
            {
                //temp = Convert.ToByte(txtbx_digital_rearAtt.Text);
                if (e.KeyCode == Keys.Up)
                {
                    if (temp <= 63 - step)
                    {
                        temp = (byte)(temp + step);
                    }
                    //txtbx_digital_rearAtt.Text = temp.ToString();
                    button1_Click(null, e);
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (temp >= 0 + step)
                    {
                        temp = (byte)(temp - step);
                    }
                    //txtbx_digital_rearAtt.Text = temp.ToString();
                    button1_Click(null, e);
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    //txtbx_digital_rearAtt.Text = temp.ToString();
                    button1_Click(null, e);
                }
            }
            catch { }
        }

        private void txtbx_digital_sumAtt_KeyUp(object sender, KeyEventArgs e)
        {
            byte temp = 0;
            try{
            temp = Convert.ToByte(txtbx_digital_sumAtt.Text);
            if (e.KeyCode == Keys.Up)
            {
                if (temp <= 63 - step)
                {
                    temp = (byte)(temp + step);
                }
                txtbx_digital_sumAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (temp >= 0 + step)
                {
                    temp = (byte)(temp - step);
                }
                txtbx_digital_sumAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                txtbx_digital_sumAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            }
            catch { }
        }

        private void txtbx_digital_diffazAtt_KeyUp(object sender, KeyEventArgs e)
        {
            byte temp = 0;
            try{
            temp = Convert.ToByte(txtbx_digital_diffazAtt.Text);
            if (e.KeyCode == Keys.Up)
            {
                if (temp <= 63 - step)
                {
                    temp = (byte)(temp + step);
                }
                txtbx_digital_diffazAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (temp >= 0 + step)
                {
                    temp = (byte)(temp - step);
                }
                txtbx_digital_diffazAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                txtbx_digital_diffazAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            }
            catch { }
        }

        private void txtbx_digital_diffelAtt_KeyUp(object sender, KeyEventArgs e)
        {
            byte temp = 0;
            try{
            temp = Convert.ToByte(txtbx_digital_diffelAtt.Text);
            if (e.KeyCode == Keys.Up)
            {
                if (temp <= 63 - step)
                {
                    temp = (byte)(temp + step);
                }
                txtbx_digital_diffelAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (temp >= 0 + step)
                {
                    temp = (byte)(temp - step);
                }
                txtbx_digital_diffelAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                txtbx_digital_diffelAtt.Text = temp.ToString();
                button1_Click(null, e);
            }
            }
            catch { }
        }

        private void btn_log_Click(object sender, EventArgs e)
        {

            if (StrmWriting == true)
            {
                t.Stop();
                Application.DoEvents();
                ResetTimer();
                radLabel18.Text = " Log stoped";
                radLabel18.ForeColor = Color.White;

            }
            if (StrmWriting == false)
            {
                t.Start();
                if (saver == 1)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig1.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur1.txt");
                }
                if (saver == 2)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig2.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur2.txt");
                }
                if (saver == 3)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig3.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur3.txt");
                }
                if (saver == 4)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig4.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur4.txt");
                }
                if (saver == 5)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig5.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur5.txt");
                }

                if (saver == 6)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig6.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur6.txt");
                }

                if (saver == 7)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig7.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur7.txt");
                }
                if (saver == 8)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig8.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur8.txt");
                }
                if (saver == 9)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig9.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur9.txt");
                }
                if (saver == 10)
                {
                    DataLog1 = new StreamWriter("D:\\LogDig10.txt");
                    DataLog2 = new StreamWriter("D:\\LogCur10.txt");
                }

                logger = 1;
                StrmWriting = true;
                btn_log.Text = "Stop Logging";
            }
            else
            {
                try
                {
                    DataLog1.Close();
                    DataLog2.Close();
                    StrmWriting = false;
                    btn_log.Text = "Start Logging";
                    Pointer.Text = (saver).ToString();
                    if (saver == 10) 
                        Mover.Text = ("Over Write Point").ToString();
                    else
                        Mover.Text = ("-").ToString();
                    saver++;
                    if (saver == 11) saver = 1;

                }
                catch { };

            }
        }

        private void ResetTimer()
        {
            s = 0;
            m = 0;
            h = 0;
            timeCS = 0;
            timeSec = 0;
            timeMin = 0;
            timeHrs = 0;

        }

        private void ckbx_OpMode_DropDownClosed(object sender, EventArgs e)
        {
            button1_Click(null, e);
        }

        private void ckbx_OpMode_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radLabel2_Click(object sender, EventArgs e)
        {

        }

        private void btnTimerReset_Click(object sender, EventArgs e)
        {

            ResetTimer();

        }

        private void radLabel20_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Interval = 1; 
            if (StrmWriting == true)
            {
                timeCS = timeCS+2;
                if (timeCS >= 130)
                {
                    timeSec++;
                    timeCS = 0;
                    if (timeSec >= 60)
                    {
                        timeMin++;
                        timeSec = 0;
                        if (timeMin >= 60)
                        {
                            timeHrs++;
                            timeMin = 0;
                            if (timeHrs == 24) timeHrs = 0;
                        }
                    }

                }
                textBox1.Text = string.Format("{0}:{1}:{2}:{3}", timeHrs.ToString().PadLeft(2, '0'), timeMin.ToString().PadLeft(2, '0'), timeSec.ToString().PadLeft(2, '0'), timeCS.ToString().PadLeft(2, '0'));

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked & flag1 == 1)
            {
                flag3 = 1;
                plot1.Controller = new OxyPlot.PlotController();
                plot1.Controller.UnbindAll();
                plot1.Controller.UnbindMouseDown(OxyMouseButton.Right);
                plot1.Controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
            }
            else if (flag1 == 0) flag1 = 1;
            else
            {
                flag3 = 0;

            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked & flag2 == 1)
            {
                flag4 = 1;
                plot2.Controller = new OxyPlot.PlotController();
                plot2.Controller.UnbindAll();
                plot2.Controller.UnbindMouseDown(OxyMouseButton.Right);
                plot2.Controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt);
            }
            else if (flag2 == 0) flag2 = 1;
            else
            {
                flag4 = 0;
            }
        }

        private void mouse_Down(object sender, MouseEventArgs e)
        {
            mouseLocation = new Point(-e.X, -e.Y);
        }

        private void mouse_Move(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePose = Control.MousePosition;
                mousePose.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePose;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
        }



       

 
       
      
      



       



         

     

      

    




    }

      

       

       

}
 