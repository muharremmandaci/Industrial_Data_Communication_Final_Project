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

        String ip1;
        String ip2;
        int port1;
        int port2;

        byte function_code = 0;
        byte number_value = 0;
        byte starting_addr = 0;
        byte[] data = new byte[7];

        public bool is_stop = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UdpClient = new UdpClient(8000);
        }

        private void update()
        {
            ip1 = txt_ip1.Text;
            port1 = Convert.ToInt32(txt_port1.Text);

            ip2 = txt_ip2.Text;
            port2 = Convert.ToInt32(txt_port2.Text);


        }

        private void btn_send_command_Click(object sender, RoutedEventArgs e)
        {
            //write coil = 3

            update();

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

            data[0] = function_code;
            data[1] = starting_addr;
            data[2] = number_value;
            data[3] = (byte)(cb_led1.IsChecked == true ? 1 : 0);
            data[4] = (byte)(cb_led2.IsChecked == true ? 1 : 0);
            data[5] = (byte)(cb_led3.IsChecked == true ? 1 : 0);
            data[6] = (byte)(cb_led4.IsChecked == true ? 1 : 0);

            if (function_code == 15) //write
            {
                byte[] sending_bytes = new byte[3 + number_value];

                sending_bytes[0] = function_code;
                sending_bytes[1] = starting_addr;
                sending_bytes[2] = number_value;

                for (int i = 3; i < 3 + number_value; i++)
                {
                    sending_bytes[i] = data[i + starting_addr];
                }

                UdpClient.Send(data, (3 + number_value), ip1, port1);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);


                string deneme = "";
                for (int i = 0; i < 3 + number_value; i++)
                {
                    deneme += sending_bytes[i];

                }

                /*if(deneme == returnData)
                {
                    MessageBox.Show("communication succesfully");
                }
                else
                {
                    MessageBox.Show("communication unsuccesfully");
                }*/

                txt_response.Text = returnData;
            }


            else if (function_code == 1)//read
            {
                byte[] sending_bytes = new byte[3];

                sending_bytes[0] = function_code;
                sending_bytes[1] = starting_addr;
                sending_bytes[2] = number_value;

                UdpClient.Send(data, 3, ip1, port1);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Byte[] receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);

                /*if (sending_bytes[0] == returnData[0] && sending_bytes[1] == returnData[1] && sending_bytes[2] == returnData[2])
                {
                    MessageBox.Show("communication succesfully");
                }
                else
                {
                    MessageBox.Show("communication unsuccesfully");
                }*/

                txt_response.Text = returnData;
            }

            else if (function_code == 3)//register
            {
                byte[] sending_bytes = new byte[3];

                sending_bytes[0] = function_code;
                sending_bytes[1] = starting_addr;
                sending_bytes[2] = number_value;

                UdpClient.Send(data, 3, ip2, port2);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Byte[] receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);
                char[] str_number = new char[number_value];
                char[] charData = returnData.ToCharArray();

                int number = 0;

                Array.Reverse(charData);

                for (int i = 0; i < number_value; i++)
                {
                    str_number[i] = charData[i];
                }
                int pos = 15;
                foreach (char b in str_number)
                {
                    number |= (((int)b) - 48) << pos;
                    pos--;
                }

                /*if (sending_bytes[0] == returnData[0] && sending_bytes[1] == returnData[1] && sending_bytes[2] == returnData[2])
                {
                    MessageBox.Show("communication succesfully");
                }
                else
                {
                    MessageBox.Show("communication unsuccesfully");
                }*/

                txt_response.Text = returnData + "  decimal : " + number;
            }

        }

        private void btn_senario_Click(object sender, RoutedEventArgs e)
        {
            update();
            is_stop = false;
            double time = 0;

            //for (int ct = 1; ct <= 30; ct++)
            //{
                DateTime start = DateTime.UtcNow;

                byte[] sending_bytes = new byte[7];

                sending_bytes[0] = 3;
                sending_bytes[1] = 0;
                sending_bytes[2] = 16;

                UdpClient.Send(sending_bytes, 3, ip2, port2);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Byte[] receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);
                char[] str_number = new char[16];
                char[] charData = returnData.ToCharArray();

                int number = 0;

                Array.Reverse(charData);

                for (int i = 0; i < 16; i++)
                {
                    str_number[i] = charData[i];
                }
                int pos = 15;
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

                RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                receiveBytes = UdpClient.Receive(ref RemoteIpEndPoint);

                returnData = Encoding.ASCII.GetString(receiveBytes);


                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                time += timeDiff.TotalMilliseconds;

                Thread.Sleep(250);
                txt_response.Text = "time: " + time  + "ct: ";
            //}
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            is_stop = true;
        }
    }
}