using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;


namespace 串口调试助手
{
   
    public partial class MainForm : Form
    {
        private SerialPort serialPort;
        // private string sendTextBox = "01 03 00 00 00 01 84 0A"; //发送接收温度值指令

        // private string textBox2 = "00 06 00 04 00 00 c9 da"; //广播自动发送不使能
        // private string textBox2 = "00 06 00 04 00 01 08 1a"; //广播自动发送使能
        
        //private serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
        public MainForm()
        {
            serialPort = new SerialPort();     
            InitializeComponent();
          
        }

        /// <summary>
        /// 初始化串口设置
        /// </summary>
        /// 
        private void MainForm_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false; //禁用了所有的控件合法性检查
            baudRateComboBox.Items.Add(4800); //波特率参数设置
            baudRateComboBox.Items.Add(9600);
            baudRateComboBox.Items.Add(19200);
            baudRateComboBox.Items.Add(38400);
            baudRateComboBox.Text = baudRateComboBox.Items[1].ToString(); //波特率默认值

            dataBitComboBox.Items.Add(5);  //数据位参数设置
            dataBitComboBox.Items.Add(6);
            dataBitComboBox.Items.Add(7);
            dataBitComboBox.Items.Add(8);
            dataBitComboBox.Text = dataBitComboBox.Items[3].ToString();  //数据位默认值

            stopBitComboBox.Items.Add(1);  //停止位参数设置
            stopBitComboBox.Items.Add(1.5);
            stopBitComboBox.Items.Add(2);
            stopBitComboBox.Text = stopBitComboBox.Items[0].ToString(); //停止位设置


        string[] ArrayComPortsName = SerialPort.GetPortNames(); //获取当前串口个数名称
        if (ArrayComPortsName.Length != 0)
        {
            Array.Sort(ArrayComPortsName);

        for (int i = 0; i < ArrayComPortsName.Length; i++)
        {
            portComboBox.Items.Add(ArrayComPortsName[i]);
        }
        portComboBox.Text = ArrayComPortsName[0];

            if (portComboBox.Items.Count == 1)
                serialPortStated.Text = portComboBox.Items[0].ToString() + " is Connected !";
            else
                serialPortStated.Text = portComboBox.Items[portComboBox.SelectedIndex].ToString() + " is Connected !";

        }
       
            openButton.Enabled = true;  //“打开按钮”可用
            sendButton.Enabled = false; //“发送按钮”不可用，因为没打开串口，直接发送指令会报错
            closeButton.Enabled = false; //“关闭按钮”不可用，没有打开，不能关闭；没有关闭，不能打开
            groupBox7.Enabled = false;           
        }

        /// <summary>
        /// 显示COM状态栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void portComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (portComboBox.Items.Count == 0)
                serialPortStated.Text = "Not Connected！";
            //if (portComboBox.Items.Count == 1)
            //    serialPortStated.Text = portComboBox.Items[0].ToString() + " is Connected !";
            else
                serialPortStated.Text = portComboBox.Items[portComboBox.SelectedIndex].ToString() + " is Connected !";


        }

        /// <summary>
        /// “串口”数据接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                //* Byte数组的个数是不确定的，根据实时返回的指令个数为准,而且返回的都是10进制数
                do
                {
                    //serialPort.ReadBufferSize();
                    int count = serialPort.BytesToRead;  // 串口的接收缓冲区，
                    if (count <= 0)   //count <= 0 ：表示没有接收到指令
                        break;
                    Byte[] dataBuff = new Byte[count];
                    serialPort.Read(dataBuff, 0, count);  //串口读取接收缓存区的数据
                    string str = null;
                    string strAscii = null;
                    string tempStr = null;


                    for (int i = 0; i < count; i++)
                    {
                        strAscii += ((char)dataBuff[i]).ToString() + ' '; //转换为字符
                        string s = dataBuff[i].ToString("X");
                        if (s.Length == 1)   //当16进制数只有一位是，在前面补上0；
                            s = "0" + s;
                        if (i == 3 || i == 4)
                        {
                            tempStr += s;
                        }
                        str += s + ' ';

                    }

                    double temp = (double)(Convert.ToInt32(tempStr, 16)) / 10.0; //计算温度值

                    tempTextBox.Text = Convert.ToString(temp); //tempTextBox显示温度值

                    if (hexReceRadioButton.Checked) //如果接收模式是16进制
                    {
                        receiveTextBox.AppendText("接收16进制指令：" + str + "\r\n\r\n");
                    }

                    else//接收模式是ASCII
                    {
                        //int int10 = 1;
                        //strAscii = ((char)int10).ToString();
                        System.Diagnostics.Debug.Write("接收Ascii:" + Encoding.Default.GetString(dataBuff));
                        receiveTextBox.AppendText("接收Ascii指令：" + Encoding.Default.GetString(dataBuff) + "\r\n\r\n");
                    }


                    // System.Diagnostics.Debug.Write(str);
                    // System.Diagnostics.Debug.Write("温度："+tempStr+"\t"+temp);

                } while (serialPort.BytesToRead > 0);

            }
            catch (Exception ex)
            {
                MessageBox.Show("error:接收返回信息异常："+ex.Message);
            }      
            
        }


        /// <summary>
        /// "关于" 按钮的操作，启动关于窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutform = new AboutForm();
            aboutform.StartPosition = FormStartPosition.CenterScreen; //开始的窗体位置在屏幕中间
            aboutform.Show();
        }
      

        /// <summary>
        /// “打开” 按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openButton_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort.PortName = portComboBox.Text;
                serialPort.BaudRate = Convert.ToInt32(baudRateComboBox.Text);  //转换为10进制
                serialPort.DataBits = Convert.ToInt16(dataBitComboBox.Text);
                serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBitComboBox.Text);
                serialPort.Open();

                serialPort.ReceivedBytesThreshold = 7; //获取或设置 System.IO.Ports.SerialPort.DataReceived 事件发生前内部输入缓冲区中的字节数。
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived); //串口接收指令事件

                openButton.Enabled = false;  //打开按钮不可用
                closeButton.Enabled = true;
                groupBox7.Enabled = true;
                sendButton.Enabled = true;
                groupBox1.Enabled = false;
             

                serialPortStated.Text = portComboBox.Text + " is Ok !";
            }
            catch
            {
                MessageBox.Show("串口设置错误");
            }
        }

        /// <summary>
        /// “关闭”按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            serialPort.Close();
            openButton.Enabled = true;
            sendButton.Enabled = false;
            closeButton.Enabled = false; //关闭按钮不可用

            groupBox1.Enabled = true;          
            groupBox7.Enabled = false;
            autoCheckBox.Checked = false;

            if (portComboBox.Items.Count == 0)
                serialPortStated.Text = "Not Connected！";
            //if (portComboBox.Items.Count == 1)
            //    serialPortStated.Text = portComboBox.Items[0].ToString() + " is Connected !";
            else
                serialPortStated.Text = portComboBox.Items[portComboBox.SelectedIndex].ToString() + " is Connected !";


        }

            /// <summary>
            /// “发送”按钮事件
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void sendButton_Click(object sender, EventArgs e)
            {
                bool flag = true;
                if (serialPort.IsOpen)  //判断串口是否打开
                {
                    if (sendTextBox.Text != " ") //如果发送窗口不为空，便可发送指令
                    {
            if (hexSendRadioButton.Checked) //如果选中了16进制发送
            {
                receiveTextBox.AppendText("发送16进制指令："+sendTextBox.Text+"\r\n"); 
                string sendText = (sendTextBox.Text).Replace(" ", "");  //我假定输入发送框的指令之间都是以空格分隔的。
                if ((sendText.Length % 2)!=0)  //这个操作为了下面的 byteData[] 的分配正常
                {
                    sendText += " ";
                }
                byte[] byteData = new byte[sendText.Length / 2];
                for(int i=0;i<byteData.Length;i++)
                {
                    byteData[i] = Convert.ToByte(sendText.Substring(i * 2, 2), 16);  //这个是将 16进制转成字节类型数据。
                }
                try
                {
                    serialPort.Write(byteData, 0, byteData.Length);  //serialPort.Write既可以写字节型数据，也可以写字符型数据
                                    }
                catch
                {
                    MessageBox.Show("发送16进制指令失败");
                }
                     
                      
            }
            else //如果选中的发送模式是ASCII
            {
                receiveTextBox.AppendText("发送Ascii指令：" + sendTextBox.Text + "\r\n");
                string sendText = (sendTextBox.Text).Replace(" ", "");
                byte[] temp = Encoding.Default.GetBytes(sendText);
                try
                {
                    serialPort.Write(temp, 0, temp.Length);
                }
                catch
                {
                    MessageBox.Show("发送Ascii指令失败");
                }
            }

        //下面就是储存指令的操作
        for (int i = 0; i < StoreComboBox.Items.Count; i++)
        {
            if (string.Equals(sendTextBox.Text, StoreComboBox.Items[i].ToString())) //判断每一次发送的指令是否和之前的一样，假如不同就储存起来
            {
                flag = false;
            }
        }
        if (flag == true)
        {
            StoreComboBox.Items.Add(sendTextBox.Text);  //将每次发送不同的指令储存起来
        }

        if (StoreComboBox.Items.Count == 1)   //假如存储的指令只有一条，那么 ComboBox 的默认值就是第一个
            StoreComboBox.Text = StoreComboBox.Items[0].ToString();
        else
            StoreComboBox.Text = StoreComboBox.Items[StoreComboBox.SelectedIndex].ToString(); //否则，就将默认值设为光标处。
                }
            }
            else
            {
                MessageBox.Show("串口还没打开，先按打开按钮！！");
            }
        }

        /// <summary>
        /// timer控件事件,自动发送指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)  //时钟事件
        {
            sendButton_Click(sender, e);
           // System.Diagnostics.Debug.WriteLine("123");
        }

        /// <summary>
        /// “自动发送”复选框事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoCheckBox_CheckedChanged_1(object sender, EventArgs e)  //自动发送事件
        {
            if (autoCheckBox.Checked)  //判断自动发送是否被选择，如果选择了，则启用timer时钟
            {
                setTimeTextBox.Enabled = false;  //设置的时间间距不能修改
                timer.Enabled = true;
                //timer.Interval = 1000;
                timer.Interval = int.Parse(setTimeTextBox.Text);
            }
            else
            {
                timer.Enabled = false; //timer不能启用
                setTimeTextBox.Enabled = true;  // //设置的时间间距可以修改
            }
        }


        /// <summary>
        /// “全选”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void All_Click(object sender, EventArgs e)
        {
            if (sendTextBox.Focused) //当光标在这个TextBox 上面的时候
                    {               
                sendTextBox.SelectAll(); 
            }
            if (receiveTextBox.Focused)
            {                
                receiveTextBox.SelectAll();
            }
            if(tempTextBox.Focused)
            {
                tempTextBox.SelectAll();
            }
        }


        /// <summary>
        /// “退出”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitClick_Click(object sender, EventArgs e)
        {
            this.Dispose();  //清空正在使用的所有资源
            this.Close();
        }

        /// <summary>
        /// “清除”按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearButton_Click(object sender, EventArgs e)
        {
            receiveTextBox.Text = "";
        }

        /// <summary>
        /// “复制”按钮，1、将TextBox被选中的文本添加到剪贴板上：Clipboard.SetText(string);
        /// 2、TextBoxBase.Copy();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_Click(object sender, EventArgs e)
        {
            if (sendTextBox.Focused) //当光标在这个TextBox 上面的时候
                    {
                //Clipboard.SetText(sendTextBox.SelectedText);  //这里注意是被选中的字符
                sendTextBox.Copy();
            }
            if (receiveTextBox.Focused)
            {
                //Clipboard.SetText(receiveTextBox.SelectedText);
                receiveTextBox.Copy();
            }
            if (tempTextBox.Focused)
            {
                // Clipboard.SetText(tempTextBox.SelectedText);
                tempTextBox.Copy();
            }
        }


        /// <summary>
        /// “粘贴”按钮，1、获取剪贴板的文本Clipboard.GetText();然后TextBox.AppendText()将其添加的文本末
        ///2、TextBoxBase.Paste();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Paste_Click(object sender, EventArgs e)
        {
            if (sendTextBox.Focused) //当光标在这个TextBox 上面的时候
            {
                //sendTextBox.AppendText( Clipboard.GetText());
                sendTextBox.Paste();
            }
            if (receiveTextBox.Focused)
            {
                // receiveTextBox.AppendText(Clipboard.GetText());
                receiveTextBox.Paste();
            }
            if (tempTextBox.Focused)
            {
                //tempTextBox.AppendText( Clipboard.GetText());
                tempTextBox.Paste();
            }
        }


        /// <summary>
        /// “剪切”按钮，TextBoxBase.Cut();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cut_Click(object sender, EventArgs e)
        {
            if (sendTextBox.Focused)
            {
                sendTextBox.Cut();
            }
            if (receiveTextBox.Focused)
            {
                receiveTextBox.Cut();
            }
            if (tempTextBox.Focused)
            {
                tempTextBox.Cut();
            }
        }

        /// <summary>
        /// “撤销”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Undo_Click(object sender, EventArgs e)
        {
            if (sendTextBox.Focused)
            {
                sendTextBox.Undo();
            }
            if (receiveTextBox.Focused)
            {
                receiveTextBox.Undo();
            }
            if (tempTextBox.Focused)
            {
                tempTextBox.Undo();
            }
        }

        /// <summary>
        /// “新建”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void New_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
        }

        /// <summary>
        /// “储存指令”事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StoreComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            sendTextBox.Text = StoreComboBox.Text;
        }

        /// <summary>
        /// 当前时间获取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = "         Time: " + DateTime.Now.ToString();
        }

          
        /// <summary>
        /// 添加双击托盘图标事件（双击显示窗口）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                WindowState = FormWindowState.Normal;//还原窗体显示
                this.Visible = true;
                //this.Activate();  //激活窗体并给予它焦点
                this.ShowInTaskbar = true; //任务栏显示图标       
            }
            
        }

        /// <summary>
        /// 判断是否最小化，然后显示托盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        /// <summary>
        /// 确认是否退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("是否确认退出程序？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                this.Dispose(); //清空所有资源
                this.Close();
            }
            else
                e.Cancel = true;
        }

    

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 托盘右键推出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

    }
    
}
