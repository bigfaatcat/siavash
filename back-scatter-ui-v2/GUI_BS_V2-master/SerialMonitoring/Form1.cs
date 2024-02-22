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
using System.Globalization;

namespace RMU_Project
{
    using OxyPlot;
    using OxyPlot.Series;
    using OxyPlot.Axes;
    
    public partial class Form1 : Form
    {
        private Label counterLabel;
        private System.Windows.Forms.Timer cnt_timer;
        private int counterValue;
        public Point mouseLocation;
        private PictureBox redLight;
        private PictureBox yellowLight;
        private PictureBox greenLight;
        System.Timers.Timer t;
        int h, m, s;
        int refresh = 2;
        byte[] Header = new byte[3];
        byte[] tx_byte = new byte[21];
        byte HeadE1, HeadE2, HeadE3, HeadE4;
        byte temp1,temp2, tempF, step=1;
        byte Vtemp1, Vtemp2, Vtemp3, CurrTemp1,CurrTemp2;
        UInt16 temp3, temp4,tempFrame,FS_PW,SS_PW, tempoffset=300;
        UInt32 TempSumF1, TempsumF2, TempsumF3, TempSumS1, TempsumS2, TempsumS3, TempSumF, TempSumS;
        UInt32 NormFS = 0, NormSS = 0, TempMultF, TempMultS;
        UInt32 SumFS, SumSS;
        UInt32 intvalx1, intvalx2 = 0, intvaldiff=0, CurrVal = 0;
        ushort[] data1 = new ushort[1024];
        ushort[] data2 = new ushort[1024];
        ushort[] data3 = new ushort[1024];
        float temp5, temp6,tempc5=0;
        float temp7, temp8;
        int flag1 = 0;
        int flag2 = 0;
        int flag3 = 0;
        int flag4 = 0;
        int flag5 = 0;
        int PWflgF = 0, PWflgS = 0;
        int statuscnt = 0;
        int statusflg = 0;
        byte Ver = 0x00;
        byte SS_Act = 0x00;
        byte FPGA_Act = 0x00;
        UInt16 plotcnt = 0;
        int restflg = 1;

        int ratioref = 160;

        UInt16 temp9, temp10,temp11,tempc3;
        UInt16 temp9buff, temp10buff;

        UInt16 buffer1=0, buffer2=0;
        UInt16[] RAM = new UInt16[100];

        UInt16 AVG_FN = 0, AVG_SN = 0,AVG_VN = 0;
        UInt16 SUMAVG_FN = 0, SUMAVG_SN = 0, SUMAVG_VN = 0;

        UInt16 Cnt_FN = 0, Cnt_SN = 0, Cnt_VN = 0;


        byte AVG_FH, AVG_SH,AVG_VH;
        int FNVal, SNVal,VNVal;
        UInt64 FrameRec = 0;
        UInt64 FPGARec = 0;
        UInt64 FPGARec_buff = 0;
        UInt64 FPGARec_Diff = 0;
        UInt64 mult = 0;
        int versioncnt = 0;
        int i = 0;
        int logger = 1;
        int firstline = 1;
        int saver = 1;
        StreamWriter DataLog1, DataLog2, DataLog3, DataLog4, DataLog5, DataLog6;
        bool StrmWriting = false;
        float Thre_FS_Volt, Thre_SS_Volt, Thre_Ver_Volt;
        const int maxRetries = 3;
        int retryCount = 0;
        public enum state
        {
            Header, FirstByte, SecondByte, Byte3rd, Byte4th, Byte5th, Byte6th, Byte7th, Byte8th, Byte9th, Byte10th, Byte11th, Byte12th, Byte13th, Byte14th, Byte15th, Byte16th, Byte17th, Byte18th, Byte19th, Byte20th,
            Byte21th, Byte22th, Byte23th, Byte24th, Byte25th, Byte26th, Byte27th, Byte28th, Byte29th, Byte30th, Byte31th, Byte32th, Byte33th, Byte34th

        };
        state rx_state;
        double x1 = 0;
        double x2 = 0;
        double x3 = 0;
        int RefreshCounter1 = 0;
        int RefreshCounter2 = 0;
        int RefreshCounter3 = 0;
        float roundedTemp5 = 0, roundedTemp6 = 0;
        int timeCS = 0;
        int timeSec = 0;
        int timeMin = 0;
        int timeHrs = 0;
        int ratio_cnt=1;
        float ratio = 1;
        float ratio_dynamic = 1;
        float multiplier = 1000.0f;
        private TrackBar trackBar;


        private Button clearButton;
        private BackgroundWorker dataReceiveWorker;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            InitializeUI_T();
            InitializeTimer();
            InitializeUILED();
            StartTrafficLight();

            // Initialize the BackgroundWorker
            dataReceiveWorker = new BackgroundWorker();
            dataReceiveWorker.DoWork += DataReceiveWorker_DoWork;

            timeCS = 0;
            timeSec = 0;
            timeMin = 0;
            timeHrs = 0;
            clearButton = new Button();
            clearButton.Text = "Clear Plot";
            clearButton.Location = new Point(600, 25); // Adjust the location as needed
            clearButton.ForeColor = Color.Gold;
            clearButton.BackColor = Color.Black;
            clearButton.Click += ClearButton_Click;
            this.Controls.Add(clearButton);

            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            checkBox4.Checked = true;
            checkBox5.Checked = true;
            this.AutoScroll = true;
            this.AutoSize = true;
            var myModel1 = new PlotModel()
            {
   
                Title = "Scatter",
                TextColor = OxyColors.White,
                PlotMargins = new OxyPlot.OxyThickness(40, 10, 10, 20)
            };
            var line_position1 = new LineSeries { 
                
                //LineStyle = LineStyle.Solid , StrokeThickness = 1,   Color = OxyColors.White
                               
                                StrokeThickness = 0.5,
                                Color = OxyColors.Transparent,
                                MarkerStroke = OxyColors.White,
                                MarkerFill   = OxyColors.Red,
                                MarkerType   = MarkerType.Circle            
                                               };

            OxyPlot.Axes.LinearAxis LAY1 = new OxyPlot.Axes.LinearAxis()
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                AbsoluteMaximum = 3f,
                AbsoluteMinimum = -3f,
            };

            myModel1.Axes.Add(LAY1);
            myModel1.Series.Add(line_position1);
            this.plot1.Model = myModel1;
            plot1.Model.Series[0].IsVisible = true;


            var myModel2 = new PlotModel()
            {

                Title = "Peak FS & SS (V)",
                TextColor = OxyColors.White,
                PlotMargins = new OxyPlot.OxyThickness(40, 10, 10, 20)
            };
            var line_position2 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 1,   Color = OxyColors.Blue};
            var line_position3 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 1,   Color = OxyColors.Green};
            OxyPlot.Axes.LinearAxis LAY2 = new OxyPlot.Axes.LinearAxis()
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                AbsoluteMaximum = 3f,
                AbsoluteMinimum = -3f,
            };

            myModel2.Axes.Add(LAY2);
            myModel2.Series.Add(line_position2);
            myModel2.Series.Add(line_position3);
            this.plot2.Model = myModel2;
            plot2.Model.Series[0].IsVisible = true;
            plot2.Model.Series[1].IsVisible = true;


            var myModel3 = new PlotModel()
            {

                Title = "current",
                TextColor = OxyColors.White,
                PlotMargins = new OxyPlot.OxyThickness(40, 10, 10, 20)
            };
            var line_position4 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 0.5,   Color = OxyColors.Yellow };

            OxyPlot.Axes.LinearAxis LAY4 = new OxyPlot.Axes.LinearAxis()
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                AbsoluteMaximum = 4f,
                AbsoluteMinimum = -4f,
            };

            myModel3.Axes.Add(LAY4);
            myModel3.Series.Add(line_position4);
            this.plot3.Model = myModel3;
            plot3.Model.Series[0].IsVisible = true;

        }




        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Clear the plot1 and plot2
            plot1.Model.Series.Clear();
            plot2.Model.Series.Clear();
            plot3.Model.Series.Clear();

            // Add new LineSeries to plot1
            var line_position1 = new LineSeries
            {
               
                StrokeThickness = 0.5,
                Color = OxyColors.Transparent,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Red,
                MarkerType = MarkerType.Circle
            };

            // Add new LineSeries to plot2
            var line_position2 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 1,   Color = OxyColors.Blue };
            var line_position3 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 1,   Color = OxyColors.Green };
            var line_position4 = new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 0.5,   Color = OxyColors.Yellow };

            // Add axes to the new models
            var LAY1 = new LinearAxis()
            {
                Position = AxisPosition.Left,
                AbsoluteMaximum = 3f,
                AbsoluteMinimum = -3f,
            };

            var LAY2 = new LinearAxis()
            {
                Position = AxisPosition.Left,
                AbsoluteMaximum = 3f,
                AbsoluteMinimum = -3f,
            };

            var LAY4 = new LinearAxis()
            {
                Position = AxisPosition.Left,
                AbsoluteMaximum = 4f,
                AbsoluteMinimum = -4f,
            };

            // Add new models to the plots
            var myModel1 = new PlotModel()
            {
                Title = "Scatter",
                TextColor = OxyColors.White,
                PlotMargins = new OxyThickness(40, 10, 10, 20)
            };
            myModel1.Axes.Add(LAY1);
            myModel1.Series.Add(line_position1);
            plot1.Model = myModel1;

            var myModel2 = new PlotModel()
            {
                Title = "Peak FS & SS (V)",
                TextColor = OxyColors.White,
                PlotMargins = new OxyThickness(40, 10, 10, 20)
            };
            myModel2.Axes.Add(LAY2);
            myModel2.Series.Add(line_position2);
            myModel2.Series.Add(line_position3);
            plot2.Model = myModel2;

            var myModel3 = new PlotModel()
            {
                Title = "Current",
                TextColor = OxyColors.White,
                PlotMargins = new OxyThickness(40, 10, 10, 20)
            };
            myModel3.Axes.Add(LAY4);
            myModel3.Series.Add(line_position4);
            plot3.Model = myModel3;

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            t = new System.Timers.Timer();
            t.Interval = 1000;
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
            // Check if the worker is not already running
            if (!dataReceiveWorker.IsBusy)
            {
                // Start the BackgroundWorker
                dataReceiveWorker.RunWorkerAsync();
            }
        }

        private void DataReceiveWorker_DoWork(object sender, DoWorkEventArgs e)
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

                                            if (Convert.ToChar(Header[0]) == 0x07 & Convert.ToChar(Header[1]) == 0x3A &
                                                Convert.ToChar(Header[2]) == 0xB6 )
                                            {
                                                rx_state = state.FirstByte;
                                            }
                                            break;

                                        case state.FirstByte:
                                            temp1 = Rx_Byte;
                                            rx_state = state.SecondByte;
                                            break;
                                        case state.SecondByte:
  
                                                temp9 = (UInt16)(((UInt16)temp1 << 8) + Rx_Byte);
                                                if (temp9 > 8100) temp9 = 0;
                                                if (temp9 < 0) temp9 = 0;
                                                txtbx_Ferequency.Text = (((UInt16)temp1 << 8) + Rx_Byte).ToString();
                                                temp9 = temp9;
                                             rx_state = state.Byte3rd;
                                             break;

                                        case state.Byte3rd:
                                             temp2 = Rx_Byte;
                                             rx_state = state.Byte4th;
                                             break;
                                        case state.Byte4th:

                             
                                                 temp10 = (UInt16)(((UInt16)temp2 << 8) + Rx_Byte);
                                                 if (temp10 > 8100) temp10 = 0;
                                                 if (temp10 < 0) temp10 = 0;
                                                 txtbx_FreqDev.Text = (((UInt16)temp2 << 8) + Rx_Byte).ToString();
                                                 temp10 = temp10;

                                             rx_state = state.Byte5th;
                                             break;



                                        case state.Byte5th:
                                             tempF = Rx_Byte;
                                             rx_state = state.Byte6th;
                                             break;


                                        case state.Byte6th:
                                             tempFrame = (UInt16)(((UInt16)tempF << 8) + Rx_Byte);
                                             buffer2 = buffer1;
                                             buffer1 = tempFrame;
                                             if (buffer2>61400 && buffer1<100) mult = mult + 1;
                                             FPGARec = (mult * 61439) + tempFrame;
                                             if (FPGARec == 18446744073709551610) FPGARec = 0;
                                             TxtBx_RearAtt.Text = (FPGARec).ToString();
                         
                                             rx_state = state.Byte7th;
                                             break;


                                        case state.Byte7th:
                                             FS_PW = (UInt16)Rx_Byte;
                                             if (FS_PW > 41) { FS_PW = 40; PWflgF = 1; } else PWflgF = 0;
                                             TxtBx_FrontSumAtt.Text = ((UInt16)Rx_Byte).ToString();
                                             rx_state = state.Byte8th;
                                             break;

                                        case state.Byte8th:
                                             SS_PW = (UInt16)Rx_Byte;
                                             if (SS_PW > 41) { SS_PW = 40; PWflgS = 1; } else PWflgS = 0;
                                             txtbx_FontDiffAzAtt.Text = ((UInt16)Rx_Byte).ToString();
                                             rx_state = state.Byte9th;
                                             break;


                                        case state.Byte9th:
                                                TempSumF1=(UInt32)Rx_Byte;
                                                rx_state = state.Byte10th;
                                                break;


                                        case state.Byte10th:
                                                TempsumF2 = (UInt32)Rx_Byte;
                                                rx_state = state.Byte11th;
                                                break;

                                        case state.Byte11th:
                                                TempsumF3 = (UInt32)Rx_Byte;
                                                rx_state = state.Byte12th;
                                                break;

                                        case state.Byte12th:
                                                TempSumS1 = (UInt32)Rx_Byte;
                                                rx_state = state.Byte13th;
                                                break;


                                        case state.Byte13th:
                                                TempsumS2 = (UInt32)Rx_Byte;
                                                rx_state = state.Byte14th;
                                                break;

                                        case state.Byte14th:

                                             TempsumS3 = (UInt32)Rx_Byte;        
                                             rx_state = state.Byte15th;
                                             break;

                                        case state.Byte15th:
                                                Vtemp1 = Rx_Byte;
                                                rx_state = state.Byte16th;
                                                break;

                                        case state.Byte16th:
                                                Vtemp2 = Rx_Byte;
                                                rx_state = state.Byte17th;
                                                break;

                                        case state.Byte17th:
                                                Vtemp3 = Rx_Byte;
                                                intvalx1 = ((UInt32)Vtemp1 << 16) + ((UInt32)Vtemp2 << 8) + ((UInt32)Vtemp3);
                                                rx_state = state.Byte18th;
                                                break;



                                        case state.Byte18th:
                                                CurrTemp1 = Rx_Byte;
                                                rx_state = state.Byte19th;
                                                break;

                                        case state.Byte19th:
                                                CurrTemp2 = Rx_Byte;
                                                rx_state = state.Byte20th;
                                                break;


                                        case state.Byte20th:
                                                AVG_FH = Rx_Byte;
                                                rx_state = state.Byte21th;
                                                break;

                                        case state.Byte21th:
                                                AVG_FN = (UInt16)(((UInt16)AVG_FH << 8) + Rx_Byte);
                                                if (AVG_FN >= 32000)
                                                {
                                                    FNVal = 0;
                                                }
                                                data1[Cnt_FN] = AVG_FN;
                                                Cnt_FN++;
                                                if (Cnt_FN == 1024)
                                                {
                                                    ushort averageValue1 = CalculateFilteredAverage(data1);
                                                    radLabel37.ForeColor = Color.LightGreen;
                                                    if (averageValue1 >= 32768)
                                                    {
                                                        FNVal = (averageValue1 - 65536);
                                                        radLabel37.ForeColor = Color.Red;
                                                    }
                                                    else
                                                    {
                                                        FNVal = averageValue1;
                                                       radLabel37.ForeColor = Color.LightGreen;
                                                    }
                                                    radLabel37.Text = FNVal.ToString("+#;-#;0");
                                                    Cnt_FN = 0;

                                                }
                                                rx_state = state.Byte22th;
                                                break;

                                        case state.Byte22th:
                                                AVG_SH = Rx_Byte;
                                                rx_state = state.Byte23th;
                                                break;

                                        case state.Byte23th:
                                                AVG_SN = (UInt16)(((UInt16)AVG_SH << 8) + Rx_Byte);
                                                if (AVG_SN >= 32000)
                                                {
                                                    SNVal = 0;
                                                }
                                                data2[Cnt_SN] = AVG_SN;
                                                Cnt_SN++;
                                                if (Cnt_SN == 1024)
                                                {
                                                    ushort averageValue2 = CalculateFilteredAverage(data2);
                                                    radLabel38.ForeColor = Color.LightGreen;
                                                    if (averageValue2 >= 32768)
                                                    {
                                                        SNVal = (averageValue2 - 65536);
                                                        radLabel38.ForeColor = Color.Red;
                                                    }
                                                    else
                                                    {
                                                        SNVal = averageValue2;
                                                       radLabel38.ForeColor = Color.LightGreen;
                                                    }
                                                    radLabel38.Text = SNVal.ToString("+#;-#;0");
                                                    Cnt_SN = 0;

                                                }


                                                rx_state = state.Byte24th;
                                                break;


                                        case state.Byte24th:
                                                AVG_VH = Rx_Byte;
                                                rx_state = state.Byte25th;
                                                break;

                                        case state.Byte25th:
                                                AVG_VN = (UInt16)(((UInt16)AVG_VH << 8) + Rx_Byte);
                                                if (AVG_VN >= 32000)
                                                {
                                                    VNVal = 0;
                                                }
                                                data3[Cnt_VN] = AVG_VN;
                                                Cnt_VN++;
                                                if (Cnt_VN == 1024)
                                                {
                                                    ushort averageValue3 = CalculateFilteredAverage(data3);
                                                    radLabel40.ForeColor = Color.LightGreen;
                                                    if (averageValue3 >= 32768)
                                                    {
                                                        VNVal = (averageValue3 - 65536);
                                                        radLabel40.ForeColor = Color.Red;
                                                    }
                                                    else
                                                    {
                                                        VNVal = averageValue3;
                                                       radLabel40.ForeColor = Color.LightGreen;
                                                    }
                                                    radLabel40.Text = VNVal.ToString("+#;-#;0");
                                                    Cnt_VN = 0;

                                                }
                                                radLabel40.Text = VNVal.ToString("+#;-#;0");



                                                rx_state = state.Byte26th;
                                                break;


                                        case state.Byte26th:
                                                HeadE1= Rx_Byte;
                                                rx_state = state.Byte27th;
                                                break;

                                        case state.Byte27th:
                                                HeadE2 = Rx_Byte;
                                                rx_state = state.Byte28th;
                                                break;

                                        case state.Byte28th:
                                                HeadE3 = Rx_Byte;
                                                rx_state = state.Byte29th;
                                                break;


                                        case state.Byte29th:
                                                HeadE4 = Rx_Byte;
                                                if (Convert.ToChar(HeadE1) == 0xAA & Convert.ToChar(HeadE2) == 0x55 & Convert.ToChar(HeadE3) == 0xCC & Convert.ToChar(HeadE4) == 0x33)
                                                //if (Convert.ToChar(HeadE1) == 0x77 && Convert.ToChar(HeadE2) == 0x66 && Convert.ToChar(HeadE3) == 0x55)
                                                {
                                                    temp9buff = temp9;
                                                    temp10buff = temp10;
                                                    tempc3 = ((UInt16)((CurrTemp1 << 8) + CurrTemp2));
                                                    if (tempc3 > 8192) tempc3 = 0;


                                                    if (FPGARec == 0 || intvalx1 == 0) textBox2.Text = ("NoData").ToString();
                                                    else
                                                    { 
                                                        ratio = ((float)intvalx1) / ((float)FPGARec);
                                                        ratio = (float)Math.Round(ratio, 8);
                                                        ratio_cnt++;
                                                        if (ratio_cnt > ratioref)
                                                        {
                                                            textBox2.Text = (ratio).ToString();
                                                            ratio_cnt = 0;
                                                        }

                                                    }



                                                      //tempc5 = (tempc3) * (3.5f / 4096.0f);
                                                      //tempc5 = float.Parse(tempc5.ToString("0.000#"));
                                                       tempc5 = tempc3 * (3.8f / 4096.0f);
                                                      float roundedTempc5 = (float)Math.Round(tempc5, 3);

                                                     statuscnt++;
                                                     if (statuscnt == 50)
                                                     {
                                                         if (roundedTempc5 < 0.5f)
                                                         {
                                         
                                                             if (statusflg == 1)
                                                             {
                                                                 radLabel36.ForeColor = System.Drawing.Color.Yellow;
                                                                 SetLightColor(redLight, Color.Yellow);
                                                                 statusflg = 0;
                                                             }
                                                             else
                                                             {
                                                                 radLabel36.ForeColor = System.Drawing.Color.White;
                                                                 SetLightColor(redLight, Color.White);
                                                                 statusflg = 1;
                                                             }
                                                             radLabel36.Text = ("Under Current").ToString();
                                                         }

                                                         else if (roundedTempc5 > 0.5f && roundedTempc5 < 2.5f)
                                                         {
                                                             SetLightColor(redLight, Color.Green);
                                                             radLabel36.Text = ("Normal Current").ToString();
                                                             radLabel36.ForeColor = System.Drawing.Color.Green;
                                                         }

                                                         else
                                                         {
                                         
                                                             if (statusflg == 1)
                                                             {
                                                                 radLabel36.ForeColor = System.Drawing.Color.Red;
                                                                 SetLightColor(redLight, Color.Red);
                                                                 statusflg = 0;
                                                             }
                                                             else
                                                             {
                                                                 SetLightColor(redLight, Color.White);
                                                                 radLabel36.ForeColor = System.Drawing.Color.White;
                                                                 statusflg = 1;
                                                             }
                                                             radLabel36.Text = ("Over Current").ToString();
                                                         }

                                                         statuscnt = 0;

                                                     }


                                                      radTextBox4.Text = (roundedTempc5).ToString();

                                                    //if ((intvalx1 == 473782) || (intvalx1 > 8388608)) intvalx1 = intvalx1; else intvalx2 = intvalx1;
                                                    if ((PWflgF == 0) && (PWflgS == 0))
                                                    {
                                                        FrameRec++;
                                                        radTextBox3.Text = (FrameRec).ToString();
                                                        TempSumF = (TempSumF1 << 16) + (TempsumF2 << 8) + TempsumF3;
                                                        TempSumS = (TempSumS1 << 16) + (TempsumS2 << 8) + TempsumS3;
                                                        //temp5 = (Convert.ToSingle(temp3) * 1.8f) / 16384f;
                                                        //temp6 = (Convert.ToSingle(temp4) * 1.8f) / 16384f;
                                                        try
                                                        {
                                                            temp5 = temp9buff * (3.3f / 8192.0f);
                                                            //roundedTemp5 = (float)Math.Round(temp5, 3);
                                                            roundedTemp5 = (float)Math.Round(temp5 * multiplier) / multiplier;
   
                                                        }
                                                        catch
                                                        {
                                                            roundedTemp5 = roundedTemp5;
                                                        }
                                                        try
                                                        {
                                                            temp6 = temp10buff * (3.3f / 8192.0f);
                                                            //roundedTemp6 = (float)Math.Round(temp6, 3);
                                                            //roundedTemp6 = float.Parse(temp6.ToString("N3"));
                                                            roundedTemp6 = (float)Math.Round(temp6 * multiplier) / multiplier;
                                                        }
                                                        catch
                                                        {
                                                            roundedTemp6 = roundedTemp6;
                                                        }
                                                        //double d1 = temp5;
                                                        //d1 = Math.Round(d1, 3);
                                                        //temp5 = (float)d1;
                                                        //double d2 = temp6;
                                                        //d2 = Math.Round(d2, 3);
                                                        //temp6 = (float)d2;
                                    
                                    


                                                        TempMultF = NormFS * Convert.ToUInt32(FS_PW);
                                                        TempMultS = NormSS * Convert.ToUInt32(SS_PW);
                                                        if (TempSumF >= TempMultF)
                                                        {
                                                            SumFS = TempSumF - TempMultF;

                                                        }

                                                        if (TempSumS >= TempMultS)
                                                        {
                                                            SumSS = TempSumS - TempMultS;

                                                        }

                                                        plotcnt++;
                                                        if (plotcnt == 5)
                                                        {
                                                            plotcnt = 0;
                                                            NormFS = Convert.ToUInt32(txtbx_digital_index.Text);
                                                            NormSS = Convert.ToUInt32(txtbx_digital_rearAtt.Text);
                                                            txtbx_FontDiffElAtt.Text = ((UInt32)SumFS).ToString();
                                                            txtbx_Kindex.Text = ((UInt32)SumSS).ToString();
                                                            if ((intvalx1 == 473782) || (intvalx1 > 8388608)) intvalx1 = intvalx1;
                                                            else
                                                                radTextBox2.Text = (((UInt16)Vtemp1 << 16) + ((UInt16)Vtemp2 << 8) + Vtemp3).ToString();

                                                        }


                                                    }

                                                    if (StrmWriting == true)
                                                    {
                                                        try
                                                        {
                                                            if (logger == 1)
                                                            {
                                                                DateTime dt = dateTimePicker1.Value;
                                                                DataLog1.WriteLine(dt.ToString());
                                                                DataLog1.WriteLine(Version.Text.ToString());

                                                                logger = 2;
                                                            }

                                                            if (temp5 < 1.5 && temp6 < 1.5 && temp5 > 0.05 && temp6 > 0.05)
                                                            {
                                     

                                                                RefreshCounter3++;

                                                                if (RefreshCounter3 == 2)
                                                                {



                                                                    x1 = ((LineSeries)this.plot1.Model.Series[0]).Points.Count > 0 ? ((LineSeries)this.plot1.Model.Series[0]).Points[((LineSeries)this.plot1.Model.Series[0]).Points.Count - 1].X + 1 : 0;

                                                                    if (((LineSeries)this.plot1.Model.Series[0]).Points.Count >= 5000)
                                                                    {
                                                                        ((LineSeries)this.plot1.Model.Series[0]).Points.RemoveAt(0);
                                                                    }
                                                                    ((LineSeries)this.plot1.Model.Series[0]).Points.Add(new DataPoint(roundedTemp5, roundedTemp6));


                                                                    x2 = ((LineSeries)this.plot2.Model.Series[0]).Points.Count > 0 ? ((LineSeries)this.plot2.Model.Series[0]).Points[((LineSeries)this.plot2.Model.Series[0]).Points.Count - 1].X + 1 : 0;

                                                                    if (((LineSeries)this.plot2.Model.Series[0]).Points.Count >= 5000)
                                                                    {
                                                                        ((LineSeries)this.plot2.Model.Series[0]).Points.RemoveAt(0);
                                                                        ((LineSeries)this.plot2.Model.Series[1]).Points.RemoveAt(0);
                                                                    }
                                                                    ((LineSeries)this.plot2.Model.Series[0]).Points.Add(new DataPoint(x2, roundedTemp5));
                                                                    ((LineSeries)this.plot2.Model.Series[1]).Points.Add(new DataPoint(x2, roundedTemp6));


                                                                    x3 = ((LineSeries)this.plot3.Model.Series[0]).Points.Count > 0 ? ((LineSeries)this.plot3.Model.Series[0]).Points[((LineSeries)this.plot3.Model.Series[0]).Points.Count - 1].X + 1 : 0;

                                                                    if (((LineSeries)this.plot3.Model.Series[0]).Points.Count >= 5000)
                                                                    {
                                                                        ((LineSeries)this.plot3.Model.Series[0]).Points.RemoveAt(0);
                                                                    }
                                                                    ((LineSeries)this.plot3.Model.Series[0]).Points.Add(new DataPoint(x3, tempc5));



                                                                    RefreshCounter2++;
                                                                    if (RefreshCounter2 == 4)
                                                                    {
                                                                        RefreshCounter2 = 0;
                                                                        this.plot1.InvalidatePlot(true);
                                                                        this.plot2.InvalidatePlot(true);
                                                                        this.plot3.InvalidatePlot(true);

                                                                    }

                                                                    RefreshCounter3 = 0;



                                                                }



                                                            }

                                                        }


                                                        catch
                                                        {
                                                            MessageBox.Show("failed to write into file");
                                                        }



                                                        DataLog1.WriteLine(roundedTemp5 + ";" + FS_PW + ";" + TempSumF + ";" + roundedTemp6 + ";" + SS_PW + ";" + TempSumS + ";" + intvalx1 + ";" + FPGARec + ";" + tempc5 + ";" + ratio + ";");

                                                    }

                                                }

                                             rx_state = state.Header;
                                             break;
                   
                                    }
                                    if (SerialPort_RMU_RX.BytesToRead != 0) goto next_byte;
                         }
                       
                                
                                catch (Exception ex)
                        {
                            // Handle exceptions or log them
                            MessageBox.Show(String.Format("The port close: {0}", ex.Message));
                        }

       
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
            UInt16 temptx4 = 0;
            UInt16 temptx5 = 0;
            UInt16 temptx6 = 0;
            byte temptx7 = 0;
            byte temptx8 = 0;
            byte temptx9 = 0;

            byte Btemp = 0;
                try
                {
                    temptx1 = Convert.ToUInt16(txtbx_digital_index.Text);
                    temptx2 = Convert.ToUInt16(txtbx_digital_rearAtt.Text);
                    temptx3 = Convert.ToUInt16(radTextBox1.Text);

                    temptx4 = Convert.ToUInt16(txtbx_digital_diffazAtt.Text);
                    temptx5 = Convert.ToUInt16(txtbx_digital_diffelAtt.Text);
                    temptx6 = Convert.ToUInt16(txtbx_digital_sumAtt.Text);


                    if (temptx3 > 75)
                    {
                        temptx3 = 75;
                        radTextBox1.Text = "40";
                        radLabel8.ForeColor = Color.Red;
                        radLabel8.Text = "OVR-75u";
                    }
                    else
                    {
                        radLabel8.ForeColor = Color.White;
                        radLabel8.Text = "us";
                    }


                    if (temptx4 > 61)
                    {
                        temptx4 = 20;
                        txtbx_digital_diffazAtt.Text = "20";
                        radLabel10.ForeColor = Color.Red;
                        radLabel10.Text = "OVR-60u";
                    }
                    else
                    {
                        radLabel10.ForeColor = Color.White;
                        radLabel10.Text = "us";
                    }

                    if (temptx5 > 201)
                    {
                        temptx5 = 0;
                        txtbx_digital_diffelAtt.Text = "0";
                        radLabel25.ForeColor = Color.Red;
                        radLabel25.Text = "OVR-60u";
                    }
                    else
                    {
                        radLabel25.ForeColor = Color.White;
                        radLabel25.Text = "us";
                    }

                    temptx7 = Ver;
                    temptx8 = SS_Act;

                    temptx9 = FPGA_Act;

                    //tx_byte[7] = 0x0A;
                    //Btemp = Convert.ToByte(txtbx_digital_diffazAtt.Text);
                    //tx_byte[8] = 0x8C;
                    //Btemp = Convert.ToByte(txtbx_digital_diffelAtt.Text);
                    //tx_byte[9] = Btemp;
                    //tx_byte[10] = (byte)ckbx_OpMode.SelectedIndex;
                    //tx_byte[10] = Btemp;
                    tx_byte[15] = Btemp;
                    tx_byte[16] = Btemp;


                }
                catch
                {
                    temptx1 = 0;
                    txtbx_digital_index.Text = "900";
                    temptx2 = 0;
                    txtbx_digital_rearAtt.Text ="900";
                    temptx3 = 0;
                    radTextBox1.Text ="39";
                    temptx4 = 20;
                    txtbx_digital_diffazAtt.Text ="20";
                    temptx5 = 0;
                    txtbx_digital_diffelAtt.Text ="0";
                    temptx6 = 0;
                    txtbx_digital_sumAtt.Text ="900";
                    temptx7 = 0x00;
                }

                Thre_FS_Volt = float.Parse(txtbx_digital_index.Text.ToString(), CultureInfo.InvariantCulture.NumberFormat);
                Thre_SS_Volt = float.Parse(txtbx_digital_rearAtt.Text.ToString(), CultureInfo.InvariantCulture.NumberFormat);
                Thre_Ver_Volt = float.Parse(txtbx_digital_sumAtt.Text.ToString(), CultureInfo.InvariantCulture.NumberFormat);
                Thre_FS_Volt = (Thre_FS_Volt * 3.33f) / 8192f;
                Thre_SS_Volt = (Thre_SS_Volt * 3.33f) / 8192f;
                Thre_Ver_Volt = (Thre_Ver_Volt * 3.33f) / 8192f;
                //Thre_Ver_Volt = ((Thre_Ver_Volt-600) * 1.65f) / 8192f;
                Thre_FS_Volt = Thre_FS_Volt;
                Thre_SS_Volt = Thre_SS_Volt;
                Thre_Ver_Volt = Thre_Ver_Volt+0.05f;
                
                if (Thre_FS_Volt < 0.905f)
                    radLabel18.Text = Thre_FS_Volt.ToString("0.000");
                else
                    radLabel18.Text = "OV";
                if (Thre_SS_Volt < 0.905f)
                   radLabel7.Text  = Thre_SS_Volt.ToString("0.000");
                else
                    radLabel7.Text = "OV";

                if (Thre_Ver_Volt < 0.905f)
                {
                    Thre_Ver_Volt = (float)Math.Round(Thre_Ver_Volt, 3);
                    radLabel13.Text = ("| " + Thre_Ver_Volt + " |").ToString();
                }
                else
                    radLabel13.Text = "OV";

                
                tx_byte[3] = (byte)(temptx1 >> 8);
                tx_byte[4] = (byte)(temptx1);
                tx_byte[5] = (byte)(temptx2 >> 8);
                tx_byte[6] = (byte)(temptx2);
                tx_byte[7] = (byte)(temptx3 >> 8);
                tx_byte[8] = (byte)(temptx3);
                tx_byte[9] = (byte)(temptx4);
                tx_byte[10] = (byte)(temptx5);
                tx_byte[11] = (byte)(temptx6 >> 8);
                tx_byte[12] = (byte)(temptx6);
                tx_byte[13] = temptx7;
                tx_byte[14] = temptx8;
                tx_byte[15] = temptx9;

                if (temptx9 == 0x01) mult = 0;

                SerialPort_RMU_RX.Write(tx_byte, 0, 18);

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
            temp= Convert.ToUInt16(txtbx_digital_index.Text);
            if (e.KeyCode == Keys.Up )
            {
                if (temp <= 40000 - step)
                {
                    temp =(UInt16)(temp + step);
                }
                txtbx_digital_index.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (temp >= 0+ step)
                {
                    temp = (UInt16)(temp - step);
                }
                txtbx_digital_index.Text = temp.ToString();
                button1_Click(null, e);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                txtbx_digital_index.Text = temp.ToString();
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
                temp = Convert.ToByte(txtbx_digital_rearAtt.Text);
                if (e.KeyCode == Keys.Up)
                {
                    if (temp <= 63 - step)
                    {
                        temp = (byte)(temp + step);
                    }
                    txtbx_digital_rearAtt.Text = temp.ToString();
                    button1_Click(null, e);
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (temp >= 0 + step)
                    {
                        temp = (byte)(temp - step);
                    }
                    txtbx_digital_rearAtt.Text = temp.ToString();
                    button1_Click(null, e);
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    txtbx_digital_rearAtt.Text = temp.ToString();
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
                TimerReset();
                intvaldiff = 0;
                FPGARec_Diff = 0;
                FrameRec = 0;
                

            }
            if (StrmWriting == false)
            {
                button6_Click_1(null, e);
                t.Start();
                if (saver < 1001)
                {
                    versioncnt++;
                    if (versioncnt == 1000) versioncnt = 0;
                                string nameFromTextBox = Version.Text;
                                string numberFromTextBox = versioncnt.ToString();
                            //string folderName = $"{DateTime.Now:yyyyMMdd-HHmmss}-{nameFromTextBox}-{numberFromTextBox}";
                            //string folderName = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-Name-123";// Replace "Name" and "123" with your specific values
                            string folderName = string.Format("{0:yyyyMMdd-HHmmss}-{1}-{2}", DateTime.Now, nameFromTextBox, numberFromTextBox);
                            string folderPath = Path.Combine("C:\\log", folderName);
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }
                            string filePath = Path.Combine(folderPath, "LogPKFS.txt");
                            DataLog1 = new StreamWriter(filePath);

                            //DataLog1 = new StreamWriter("C:\\log\\LogPKFS1.txt");
                    //DataLog2 = new StreamWriter("C:\\Log\\logPKSS1.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS1.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS1.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS1.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS1.txt");

                }

                /*
                if (saver == 2)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS2.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS2.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS2.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS2.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS2.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS2.txt");
                }
                if (saver == 3)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS3.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS3.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS3.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS3.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS3.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS3.txt");
                }
                if (saver == 4)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS4.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS4.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS4.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS4.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS4.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS4.txt");
                }
                if (saver == 5)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS5.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS5.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS5.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS5.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS5.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS5.txt");
                }

                if (saver == 6)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS6.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS6.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS6.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS6.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS6.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS6.txt");
                }

                if (saver == 7)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS7.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS7.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS7.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS7.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS7.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS7.txt");
                }
                if (saver == 8)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS8.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS8.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS8.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS8.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS8.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS8.txt");
                }
                if (saver == 9)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS9.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS9.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS9.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS9.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS9.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS9.txt");
                }
                if (saver == 10)
                {
                    DataLog1 = new StreamWriter("C:\\log\\LogPKFS10.txt");
                    //DataLog2 = new StreamWriter("C:\\log\\LogPKSS10.txt");
                    //DataLog3 = new StreamWriter("C:\\log\\LogPWFS10.txt");
                    //DataLog4 = new StreamWriter("C:\\log\\LogPWSS10.txt");
                    //DataLog5 = new StreamWriter("C:\\log\\LogSUMFS10.txt");
                    //DataLog6 = new StreamWriter("C:\\log\\LogSUMSS10.txt");
                }

                */

                firstline = 1;
                logger = 1;
                StrmWriting = true;
                btn_log.Text = "Stop Logging";
            }
            else
            {
                try
                {
                    DataLog1.Close();
                    //DataLog2.Close();
                    StrmWriting = false;
                    btn_log.Text = "Start Logging";
                    Pointer.Text = (saver).ToString();
                    if (saver == 1000) 
                        Mover.Text = ("Over Write Point").ToString();
                    else
                        Mover.Text = ("-").ToString();
                    saver++;
                    if (saver == 1001) saver = 1;

                }
                catch { };

            }
        }

        private void ResetTimer()
        {
            s = 0;
            m = 0;
            h = 0;

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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox1.Checked & flag1 == 1)
            {

                plot2.Model.Series[0].IsVisible = true;
                buttonFS.BackColor = Color.Blue;
            }
            else if (flag1 == 0) flag1 = 1;
            else
            {
                plot2.Model.Series[0].IsVisible = false;
                buttonFS.BackColor = Color.Gray;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked & flag2 == 1)
            {

                plot2.Model.Series[1].IsVisible = true;
                buttonSS.BackColor = Color.Green;
            }
            else if (flag2 == 0) flag2 = 1;
            else
            {
                plot2.Model.Series[1].IsVisible = false;
                buttonSS.BackColor = Color.Gray;
            }

        }

        private void radLabel23_Click(object sender, EventArgs e)
        {

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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox3.Checked & flag3 == 1)
            {

                button2.Text = "on";
                button2.BackColor = Color.Yellow;
                Ver = 0x00;
            }
            else if (flag3 == 0) flag3 = 1;
            else
            {
                button2.Text = "off";
                button2.BackColor = Color.Gray;
                Ver = 0x01;
            }

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked & flag4 == 1)
            {

                button3.Text = "on";
                button3.BackColor = Color.Yellow;
                SS_Act = 0x00;
            }
            else if (flag4 == 0) flag4 = 1;
            else
            {
                button3.Text = "off";
                button3.BackColor = Color.Gray;
                SS_Act = 0x01;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FrameRec = 0;
            radTextBox3.Text = (FrameRec).ToString();

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked & flag5 == 1)
            {

                button5.Text = "on";
                button5.BackColor = Color.Yellow;
                FPGA_Act = 0x00;
            }
            else if (flag5 == 0) flag5 = 1;
            else
            {
                button5.Text = "off";
                button5.BackColor = Color.Gray;
                FPGA_Act = 0x01;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)

        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Interval = 1;
            if (StrmWriting == true)
            {
                timeCS = timeCS + 2;
                if (timeCS >= 125)
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


        private void TimerReset()
        {
            timeCS = 0;
            timeSec = 0;
            timeMin = 0;
            timeHrs = 0;
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
                FPGA_Act = 0x01;
                Ver = 0x01;
                button1_Click(null, e);
                for (int i = 0; i < 2; i++)
                 {
                        Thread.Sleep(50);
                 }
                
                FPGA_Act = 0x00;
                Ver = 0x00;
                button1_Click(null, e);
                intvaldiff = 0;
                FPGARec_Diff = 0;
                FrameRec = 0;
             
        }



        private void InitializeUI()
        {
            trackBar1.Minimum = 10;
            trackBar1.Maximum = 180;
            trackBar1.SmallChange = 10;
            trackBar1.LargeChange = 10;
            trackBar1.TickFrequency = 10;
            //trackBar1.BackColor = System.Drawing.Color.Black;
            //trackBar1.ForeColor = System.Drawing.Color.Yellow;
            trackBar1.Value = 160;
            trackBar1.Scroll += trackBar1_Scroll;
            trackBar1.Paint += TrackBar_Paint;
            trackBar1.MouseDown += TrackBar_MouseDown;

        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            // Add controls to the form
            Controls.Add(trackBar1);
            ratioref = trackBar1.Value;

        }

        private void TrackBar_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the gradient color based on the thumb position
            float percent = (float)(trackBar.Value - trackBar.Minimum) / (trackBar.Maximum - trackBar.Minimum);
            Color color = InterpolateColor(Color.Blue, Color.Yellow, percent);

            // Set the brush color and fill the track
            using (SolidBrush brush = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(brush, trackBar.ClientRectangle);
            }
            using (Pen pen = new Pen(Color.White, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, trackBar.Width - 1, trackBar.Height - 1);
            }
        }

        private void TrackBar_MouseDown(object sender, MouseEventArgs e)
        {
            // Display the TrackBar value at the mouse cursor location
            int trackBarValue1 = trackBar1.Value;
            radLabel34.Text = trackBarValue1.ToString();
        }


        private Color InterpolateColor(Color color1, Color color2, float percent)
        {
            int r = (int)(color1.R + (color2.R - color1.R) * percent);
            int g = (int)(color1.G + (color2.G - color1.G) * percent);
            int b = (int)(color1.B + (color2.B - color1.B) * percent);
            return Color.FromArgb(r, g, b);
        }



        private void InitializeUI_T()
        {
            counterLabel = new Label();
            counterLabel.Location = new System.Drawing.Point(10, 10);
            counterLabel.Size = new System.Drawing.Size(200, 30);
            counterLabel.Text = "Counter: 0";

            Controls.Add(counterLabel);
        }

        private void InitializeTimer()
        {
            cnt_timer = new System.Windows.Forms.Timer(); // Fully qualify the Timer class
            cnt_timer.Interval = 1000; // Update every 1000 milliseconds (1 second)
            cnt_timer.Tick += Timer_Tick;

            // Start the timer
            cnt_timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            intvaldiff = intvalx1 - intvalx2;
            FPGARec_Diff = FPGARec - FPGARec_buff;

            if (intvaldiff == 0 || FPGARec_Diff == 0 || intvaldiff < 0 || FPGARec_Diff<0) radLabel35.Text = ("NoData").ToString();
            else
            {
                ratio_dynamic = ((float)intvaldiff) / ((float)FPGARec_Diff);
                ratio_dynamic = (float)Math.Round(ratio_dynamic, 8);
                if (ratio_dynamic < 0.1)
                {
                    radLabel35.ForeColor = Color.Yellow;
                    radLabel35.Text = string.Format("DRate/S : {0}", (ratio_dynamic).ToString(), "Attension Drop");
                }
                else if (ratio_dynamic >= 0.1 && ratio_dynamic < 2)
                {
                    radLabel35.ForeColor = Color.Green;
                    radLabel35.Text = string.Format("DRate/S : {0}", (ratio_dynamic).ToString(), "Normal");
                }
                else
                {
                    radLabel35.ForeColor = Color.Red;
                    radLabel35.Text = string.Format("DRate/S : {0}", (ratio_dynamic).ToString(), "Over Range");
                }


            }

            FPGARec_buff=FPGARec;
            intvalx2 = intvalx1;
        }


        private void InitializeUILED()
        {
            redLight = new PictureBox();
            redLight.Size = new Size(60, 30);
            redLight.BackColor = Color.Gray;
            redLight.Location = new Point(1600, 45);

            yellowLight = new PictureBox();
            yellowLight.Size = new Size(60, 30);
            yellowLight.BackColor = Color.Gray;
            yellowLight.Location = new Point(1600, 45);

            greenLight = new PictureBox();
            greenLight.Size = new Size(60, 30);
            greenLight.BackColor = Color.Gray;
            greenLight.Location = new Point(1600, 45);

            Controls.Add(redLight);
            Controls.Add(yellowLight);
            Controls.Add(greenLight);
        }
        private void StartTrafficLight()
        {
        }

        private void SetLightColor(PictureBox light, Color color)
        {
            // Invokes to update UI elements from the thread
            if (light.InvokeRequired)
            {
                light.Invoke(new MethodInvoker(delegate
                {
                    light.BackColor = color;
                }));
            }
            else
            {
                light.BackColor = color;
            }
        }


        static ushort CalculateFilteredAverage(ushort[] data)
        {
            const int windowSize = 5; // Adjust this value based on your requirements

            ushort[] filteredData = new ushort[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                // Apply a simple moving average filter
                filteredData[i] = CalculateMovingAverage(data, i, windowSize);
            }

            // Calculate the final average of the filtered data
            double finalAverage = CalculateAverage(filteredData);

            // Reset any temporary data or buffers here
            // For example, you can clear the filteredData array
            Array.Clear(filteredData, 0, filteredData.Length);

            // Round the final average and cast it to ushort
            ushort roundedAverage = (ushort)Math.Round(finalAverage);

            // Reset any other data if needed

            return roundedAverage;
        }

        static ushort CalculateMovingAverage(ushort[] data, int index, int windowSize)
        {
            int sum = 0;
            int count = 0;

            for (int i = Math.Max(0, index - windowSize / 2); i < Math.Min(data.Length, index + windowSize / 2 + 1); i++)
            {
                sum += data[i];
                count++;
            }

            return (ushort)(sum / count);
        }

        static double CalculateAverage(ushort[] data)
        {
            int sum = 0;

            foreach (ushort value in data)
            {
                sum += value;
            }

            return (double)sum / data.Length;
        }



    }
       
       

}
 