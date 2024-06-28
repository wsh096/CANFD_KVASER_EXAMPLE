using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kvaser.CanLib;

namespace CANFD_KVASER_EXAMPLE
{
    public partial class Form1 : Form
    {
        const long TIMEOUT  = 1000;
        struct CANMessage
        {
            public int id, dlc, flag;
            public byte[] data;
            public long time;
        }
        public Form1()
        {
            InitializeComponent();
            CANStart();
        }
 
        public void CANStart()
        {
            CANMessage msg = new CANMessage();
            msg.data = new byte[64];
            Canlib.canInitializeLibrary();
            int canHandle = Canlib.canOpenChannel(0, Canlib.canOPEN_CAN_FD );
            Canlib.canStatus canStatus = Canlib.canSetBusParams(canHandle, Canlib.canFD_BITRATE_500K_80P, 63, 16, 16, 1);
            Canlib.canStatus canDataPhaseStatus = Canlib.canSetBusParamsFd(canHandle, Canlib.canFD_BITRATE_1M_80P, 31, 8, 8);

            Canlib.canBusOn(canHandle);
            string header = "Time           |   Id      | CAN_DL | Data ";
            Console.WriteLine(header);
            while (true)
            {
                canStatus = Canlib.canReadWait(canHandle, out msg.id, msg.data, out msg.dlc, out msg.flag, out msg.time, TIMEOUT); 
                DisplayMessage(msg.id, msg.dlc, msg.data, msg.flag, msg.time);
            }
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
    }
}
