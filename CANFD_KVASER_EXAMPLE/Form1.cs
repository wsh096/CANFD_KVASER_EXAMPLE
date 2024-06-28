using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Kvaser.CanLib;
using static Kvaser.CanLib.Canlib;

namespace CANFD_KVASER_EXAMPLE
{
    public partial class Form1 : Form
    {
        const long TIMEOUT  = 1000;
        int canHandle;
        Canlib.canStatus canStatus;
        //BackgroundWorker worker;
        struct CANMessage
        {
            public int id, dlc, flag;
            public byte[] data;
            public long time;
        }
        public Form1()
        {
            InitializeComponent();
            //CANStart();
            //worker = new BackgroundWorker();
            //worker.WorkerSupportsCancellation = true; // 취소 지원 설정
            
            //worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
       // private void Worker_DoWork(object sender, DoWorkEventArgs e)
        //{
       //     CANRead();
        //}
        /*
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // 취소된 경우 처리
                CANStop();
                button1.Text = "START";
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */
        public void CANStart()
        {
            
            Canlib.canInitializeLibrary();
            int canHandle = Canlib.canOpenChannel(0, Canlib.canOPEN_CAN_FD );
            canStatus canStatus = Canlib.canSetBusParams(canHandle, Canlib.canFD_BITRATE_500K_80P, 63, 16, 16, 1);
            Canlib.canStatus canDataPhaseStatus = Canlib.canSetBusParamsFd(canHandle, Canlib.canFD_BITRATE_1M_80P, 31, 8, 8);
            Canlib.canBusOn(canHandle);
        }
        public void CANStop()
        {
            Canlib.canBusOff(canHandle);
            Canlib.canClose(canHandle);
            Canlib.canUnloadLibrary();
        }
        private void DisplayMessage(int id, int dlc, byte[] data, int flags, double time)
        {
            //string header = "Time           |   Id     | CAN_DL | D0 | D1 | D2 | D3 | D4 | D5 | D6 | D7 |";
            //string format = "{0,-14} | {1,-7} | {2,6} | {3,-2:X2} | {4,-2:X2} | {5,-2:X2} | {6,-2:X2} | {7,-2:X2} | {8,-2:X2} | {9,-2:X2} | {10,-2:X2} |";
            //string separator = new string('-', header.Length);
            //Console.WriteLine(header);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0,-14:F3} | {1,-9:X2} | {2,6} | ", time, id, dlc);
            for (int i = 0; i < dlc; i++)
            {
                sb.AppendFormat("{0, -2:X2} | ", data[i]);
            }
            sb.AppendLine();
            string result = sb.ToString();
            //DataView.Invoke((MethodInvoker)delegate { DataView.AppendText(result); });
            DataView.AppendText(result);
            Console.WriteLine(result);
            if ((flags & Canlib.canMSGERR_OVERRUN) > 0)
            {
                Console.WriteLine("**** RECEIVE OVERRUN ****");
            }
            if ((flags & Canlib.canMSG_ERROR_FRAME) == Canlib.canMSG_ERROR_FRAME)
            {
                Console.WriteLine("ErrorFrame {0}", time);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text.Equals("STOP"))
            {
                CANStop();
                
                MsgTimer.Stop();
               // worker.DoWork += Worker_DoWork;
                button1.Text = "START";
            }
            else
            {
                CANStart();
                MsgTimer.Start();
              //  worker.DoWork -= Worker_DoWork;
                button1.Text = "STOP";
            }
            
        }

        private void MsgTimer_Tick(object sender, EventArgs e)
        {
            CANMessage msg = new CANMessage();
            msg.data = new byte[64];
            string header = "Time           |   Id      | CAN_DL | Data ";
            //DataView.Invoke((MethodInvoker)delegate { DataView.AppendText(header); });
            DataView.AppendText(header);
            Console.WriteLine(header);
            canStatus = Canlib.canReadWait(canHandle, out msg.id, msg.data, out msg.dlc, out msg.flag, out msg.time, TIMEOUT);
            if (msg.dlc != 0)
            {
                DisplayMessage(msg.id, msg.dlc, msg.data, msg.flag, msg.time);
            }
        }
    }
}
