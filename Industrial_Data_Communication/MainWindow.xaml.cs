using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Threading;

namespace Industrial_Data_Communication
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        UdpClient UdpClient;
        TcpClient TcpClient;
        TcpClient TcpClient2;

        String ip1;
        String ip2;
        int port1;
        int port2;
        int ct;
        double time;

        bool is_tcp = false;

        byte function_code = 0;
        byte number_value = 0;
        byte starting_addr = 0;
        byte[] data = new byte[7];
        byte[] leds = new byte[4];

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public bool is_stop = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
        }

        private void update()
        {
            ip1 = txt_ip1.Text;
            port1 = Convert.ToInt32(txt_port1.Text);

            ip2 = txt_ip2.Text;
            port2 = Convert.ToInt32(txt_port2.Text);

            if (rb_write_coil.IsChecked == true)
            {
                function_code = 15;
            }

            else if (rb_read_coil.IsChecked == true)
            {
                function_code = 1;
            }

            else if (rb_read_hold_reg.IsChecked == true)
            {
                function_code = 3;
            }

            number_value = (byte)Convert.ToInt32(txt_number_of_values.Text);
            starting_addr = (byte)Convert.ToInt32(txt_starting_addr.Text);

            leds[0] = (byte)(cb_led1.IsChecked == true ? 1 : 0);
            leds[1] = (byte)(cb_led2.IsChecked == true ? 1 : 0);
            leds[2] = (byte)(cb_led3.IsChecked == true ? 1 : 0);
            leds[3] = (byte)(cb_led4.IsChecked == true ? 1 : 0);
        }

        #region define modbus function
        private String udp_write_coil()
        {
            byte[] sending_data = new byte[3 + number_value];

            sending_data[0] = 15;
            sending_data[1] = starting_addr;
            sending_data[2] = number_value;

            for (int i = 0; i < number_value; i++)
            {
                sending_data[3 + i] = leds[i + starting_addr];
            }

            /*int crc = CS16(sending_data,3+number_value);
            char crc1 = (char)(crc & 0xFF);
            char crc2 = (char)(crc >> 8);

            int deneme = crc2 << 8 | crc1;*/

            Byte[] receiveBytes;

            UdpClient.Send(sending_data, (3 + number_value), ip1, port1);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);
            return (Encoding.ASCII.GetString(receiveBytes));
        }

        private String tcp_write_coil()
        {
            byte[] sending_data = new byte[3 + number_value];

            sending_data[0] = 15;
            sending_data[1] = starting_addr;
            sending_data[2] = number_value;

            for (int i = 0; i < number_value; i++)
            {
                sending_data[3 + i] = leds[i + starting_addr];
            }

            Byte[] receiveBytes;

            TcpClient = new TcpClient();
            TcpClient.Connect(ip1, port1);
            NetworkStream stream = TcpClient.GetStream();
            stream.Write(sending_data, 0, sending_data.Length);
            receiveBytes = new Byte[4 + number_value];                    //15 geri dönerken 2 byte olarak dönecek
            Int32 bytes = stream.Read(receiveBytes, 0, 4 + number_value);

            TcpClient.Close();
            return (System.Text.Encoding.ASCII.GetString(receiveBytes, 0, bytes));
        }

        private String udp_read_coil()
        {
            byte[] sending_data = new byte[3];

            sending_data[0] = 1;
            sending_data[1] = starting_addr;
            sending_data[2] = number_value;

            Byte[] receiveBytes;

            UdpClient.Send(sending_data, 3, ip1, port1);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);
            return (Encoding.ASCII.GetString(receiveBytes));
        }

        private String tcp_read_coil()
        {
            byte[] sending_data = new byte[3];

            sending_data[0] = 1;
            sending_data[1] = starting_addr;
            sending_data[2] = number_value;

            Byte[] receiveBytes;

            TcpClient = new TcpClient();
            TcpClient.Connect(ip1, port1);
            NetworkStream stream = TcpClient.GetStream();
            stream.Write(sending_data, 0, sending_data.Length);
            receiveBytes = new Byte[3 + number_value];
            Int32 bytes = stream.Read(receiveBytes, 0, 3 + number_value);
            TcpClient.Close();

            return (System.Text.Encoding.ASCII.GetString(receiveBytes, 0, bytes));
        }

        private String udp_read_holding_reg()
        {
            byte[] sending_data = new byte[3];

            sending_data[0] = 3;
            sending_data[1] = starting_addr;
            sending_data[2] = number_value;

            Byte[] receiveBytes;

            UdpClient.Send(sending_data, 3, ip2, port2);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

            return (Encoding.ASCII.GetString(receiveBytes));
        }

        private String tcp_read_holding_reg()
        {
            byte[] sending_data = new byte[3];
            int def_len = number_value < 10 ? 3 : 4;

            sending_data[0] = 3;
            sending_data[1] = starting_addr;
            sending_data[2] = number_value;

            Byte[] receiveBytes;

            TcpClient = new TcpClient();
            TcpClient.Connect(ip2, port2);
            NetworkStream stream = TcpClient.GetStream();
            stream.Write(sending_data, 0, sending_data.Length);
            receiveBytes = new Byte[def_len + number_value];
            Int32 bytes = stream.Read(receiveBytes, 0, def_len + number_value);
            TcpClient.Close();

            return (System.Text.Encoding.ASCII.GetString(receiveBytes, 0, bytes));
        }
        #endregion

        private void btn_send_command_Click(object sender, RoutedEventArgs e)
        {
            update();

            String responseData = String.Empty;

            if (function_code == 15)//write coil
            {
                if (is_tcp)
                {
                    responseData = tcp_write_coil();
                }
                else
                {
                    responseData = udp_write_coil();
                }

                txt_response.Text = responseData;
            }
            else if (function_code == 1)//read coil
            {
                if (is_tcp)
                {
                    responseData = tcp_read_coil();
                }
                else
                {
                    responseData = udp_read_coil();
                }

                txt_response.Text = responseData;
            }
            else if (function_code == 3)//read hold register
            {
                if (is_tcp)
                {
                    responseData = tcp_read_holding_reg();
                }
                else
                {
                    responseData = udp_read_holding_reg();
                }

                char[] str_number = new char[number_value];
                char[] charData = responseData.ToCharArray();
                int number = 0;
                int pos = 15;

                Array.Reverse(charData);

                for (int i = 0; i < number_value; i++)
                {
                    str_number[i] = charData[i];
                }
                foreach (char b in str_number)
                {
                    number |= (((int)b) - 48) << pos;
                    pos--;
                }
                txt_response.Text = responseData + "  decimal : " + number;
            }
        }

        private void udp_senario()
        {
            update();
            is_stop = false;

            byte[] sending_bytes = new byte[7];

            sending_bytes[0] = 3;
            sending_bytes[1] = 0;
            sending_bytes[2] = 16;

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            char[] str_number = new char[16];
            int number = 0;
            int pos = 15;

            DateTime start = DateTime.UtcNow;

            UdpClient.Send(sending_bytes, 3, ip2, port2);

            Byte[] receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            char[] charData = returnData.ToCharArray();

            Array.Reverse(charData);

            for (int i = 0; i < 16; i++)
            {
                str_number[i] = charData[i];
            }

            foreach (char b in str_number)
            {
                number |= (((int)b) - 48) << pos;
                pos--;
            }

            sending_bytes[0] = 15;
            sending_bytes[1] = 0;
            sending_bytes[2] = 4;


            if (number < 256)
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 0;
                sending_bytes[5] = 0;
                sending_bytes[6] = 0;
            }
            else if (256 < number && number < 512)
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 1;
                sending_bytes[5] = 0;
                sending_bytes[6] = 0;
            }
            else if (512 < number && number < 768)
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 1;
                sending_bytes[5] = 1;
                sending_bytes[6] = 0;
            }
            else
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 1;
                sending_bytes[5] = 1;
                sending_bytes[6] = 1;
            }

            UdpClient.Send(sending_bytes, 7, ip1, port1);

            receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

            returnData = Encoding.ASCII.GetString(receiveBytes);

            DateTime end = DateTime.UtcNow;
            TimeSpan timeDiff = end - start;
            time += timeDiff.TotalMilliseconds;

            txt_response.Text = "time: " + time/ct + "  ct: " + ct;
        }

        private void tcp_senario()
        {
            update();

            byte[] sending_bytes = new byte[7];

            sending_bytes[0] = 3;
            sending_bytes[1] = 0;
            sending_bytes[2] = 16;

            char[] str_number = new char[16];
            int number = 0;
            int pos = 15;

            Byte[] receiveBytes;
            String responseData = String.Empty;

            DateTime start = DateTime.UtcNow;


            TcpClient = new TcpClient();
            TcpClient.Connect(ip2, port2);
            NetworkStream stream = TcpClient.GetStream();
            stream.Write(sending_bytes, 0, 3);
            receiveBytes = new Byte[4 + sending_bytes[2]];
            Int32 bytes = stream.Read(receiveBytes, 0, 4 + sending_bytes[2]);
            responseData = System.Text.Encoding.ASCII.GetString(receiveBytes, 0, bytes);
            TcpClient.Close();

            char[] charData = responseData.ToCharArray();

            Array.Reverse(charData);

            for (int i = 0; i < 16; i++)
            {
                str_number[i] = charData[i];
            }

            foreach (char b in str_number)
            {
                number |= (((int)b) - 48) << pos;
                pos--;
            }

            sending_bytes[0] = 15;
            sending_bytes[1] = 0;
            sending_bytes[2] = 4;

            if (number < 256)
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 0;
                sending_bytes[5] = 0;
                sending_bytes[6] = 0;
            }
            else if (256 < number && number < 512)
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 1;
                sending_bytes[5] = 0;
                sending_bytes[6] = 0;
            }
            else if (512 < number && number < 768)
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 1;
                sending_bytes[5] = 1;
                sending_bytes[6] = 0;
            }
            else
            {
                sending_bytes[3] = 1;
                sending_bytes[4] = 1;
                sending_bytes[5] = 1;
                sending_bytes[6] = 1;
            }

            TcpClient2 = new TcpClient();
            TcpClient2.Connect(ip1, port1);
            NetworkStream stream2 = TcpClient2.GetStream();
            stream2.Write(sending_bytes, 0, data.Length);
            receiveBytes = new Byte[4 + sending_bytes[2]];
            Int32 bytes2 = stream2.Read(receiveBytes, 0, 4 + sending_bytes[2]);
            responseData = System.Text.Encoding.ASCII.GetString(receiveBytes, 0, bytes2);
            TcpClient2.Close();

            DateTime end = DateTime.UtcNow;
            TimeSpan timeDiff = end - start;
            time += timeDiff.TotalMilliseconds;

            txt_response.Text = "time: " + time/ct + "  ct: "+ct;
        }

        private void btn_senario_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();

            ct = 0;
            time = 0;

            btn_stop.IsEnabled = true;
            btn_senario.IsEnabled = false;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ct++;
            if (is_tcp)
            {
                tcp_senario();
            }
            else
            {
                udp_senario();
            }
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();

            btn_stop.IsEnabled = false;
            btn_senario.IsEnabled = true;
        }

        private void btn_tcp_Click(object sender, RoutedEventArgs e)
        {
            TcpClient = new TcpClient();

            btn_send_command.IsEnabled = true;
            btn_senario.IsEnabled = true;
            //btn_udp.IsEnabled = false;

            is_tcp = true;
        }

        private void btn_udp_Click(object sender, RoutedEventArgs e)
        {
            UdpClient = new UdpClient(8000);

            btn_send_command.IsEnabled = true;
            btn_senario.IsEnabled = true;
            //btn_tcp.IsEnabled = false;

            is_tcp = false;
        }

        private void rb_read_hold_reg_Checked(object sender, RoutedEventArgs e)
        {
            txt_number_of_values.Text = 16.ToString();
        }

        private void rb_read_coil_Checked(object sender, RoutedEventArgs e)
        {
            txt_number_of_values.Text = 4.ToString();
        }

        private void rb_write_coil_Checked(object sender, RoutedEventArgs e)
        {
            txt_number_of_values.Text = 4.ToString();
        }

        public int CS16(byte[] bytes,int len)
        {
            int cs = 0;
            for (int j = 0; j < len; j++)
            {
                cs += (char)bytes[j];
            }
            return cs;
        }

        public int CS16x(byte[] bytes, int len)
        {
            int cs = 0;
            for (int j = 0; j < len; j++)
            {
                cs += bytes[j]-48;
            }
            return cs;
        }


    }
}