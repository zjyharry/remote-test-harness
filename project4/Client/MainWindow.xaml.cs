using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;
using TestHarness;
using System.Xml;
using System.Xml.Linq;
//jiayi zou
//version 1.0
//11/22/2016
//this is a GUI to use in the project, to meet the requirement 11
//the user guide of the GUI is in the readme.txt
namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rcvdMsg = "";
        private stringReceiver strrcvr;
        private string RepoFRcvr = "http://localhost:8081/RepoFRcvr";
        Thread rcvThrd = null;
        ServiceHost host = null;
        delegate void NewMessage(string msg);
        event NewMessage OnNewMessage;
        CommandData cmd = new CommandData();
        HiResTimer timer = new HiResTimer();
        public MainWindow()
        {
            InitializeComponent();
            textBox7.Text = "";
            OnNewMessage += new NewMessage(OnNewMessageHandler);
            button1.IsEnabled = false;
            button5.IsEnabled = false;
        }
        protected override void OnClosed(EventArgs e)
        {
            try {
                strrcvr.Close();
                host.Close();
            }
            catch
            {

            }
            base.OnClosed(e);
        }

        private void IsFileExist(string filename)//if a file exist we know we get the result
        {
            int i = 0;
            while (true)
            {
                if (File.Exists(filename))
                {
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Length != 0)
                        this.Dispatcher.BeginInvoke(
                         System.Windows.Threading.DispatcherPriority.Normal,
                         OnNewMessage,
                          "\nlog received and stored in " + filename);
                    return;
                }
                Thread.Sleep(100);
                i++;
                if (i >= 100)
                {
                    return;
                }
            }
        }
        void ThreadProc()//receiving thread
        {
            while (true)
            {
                rcvdMsg = strrcvr.GetMessage();
                // call window functions on UI thread

                parseMesaage(rcvdMsg);
                this.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  OnNewMessage,
                  "\nthis is for requirement 10, message received by WCF and the message is :\n"+rcvdMsg);
            }
        }
        void parseMesaage(string msg)
        {
            CommandData cmdData = new CommandData();
            CommandDecoder cmdDecoder = new CommandDecoder(msg);
            cmdData = cmdDecoder.parse();
            if(cmdData.command== "FileNotMatch")
            {
                this.Dispatcher.BeginInvoke(
  System.Windows.Threading.DispatcherPriority.Normal,
  OnNewMessage,
  "\nthis is demo for requirement 3, the test harness cannot find the file in the repository, had you send all required dll files?\n");
            }
            if (cmdData.command == "NoResult")
            {
                this.Dispatcher.BeginInvoke(
  System.Windows.Threading.DispatcherPriority.Normal,
  OnNewMessage,
  "\nthis is for requirement 4, the result isn't prepared yet, please wait for a while\n");
            }
        }
        void changeNameAndSendToRepo(string filename, CommandData cmd)//change name so that every file will have a unique name in repository
        {
            if (File.Exists(filename))
            {
                string newname = System.IO.Path.GetDirectoryName(filename) + "/" + cmd.testAuthor + "_" + cmd.dateTime + "_" + System.IO.Path.GetFileName(filename);
                File.Move(filename, newname);
                IFileService fs = null;
                int count = 0;
                while (true)
                {
                    try
                    {
                        fs = TestHarness.fileSender.CreateChannel(RepoFRcvr);
                        break;
                    }
                    catch
                    {
                        textBox7.Text+=("\n  connection to service failed "+count+ " times - trying again");
                        count++;
                        Thread.Sleep(500);
                        continue;
                    }
                }
                textBox7.Text += ("\n  Connected to "+ RepoFRcvr);
                textBox7.Text+=("\n  sending file:"+ newname);
                timer.Start();
                if (!fileSender.SendFile(fs, newname))
                {
                    Console.Write("\n  could not send file");
                    timer.Stop();
                }
                else
                {
                    timer.Stop();
                    textBox7.Text += ("\nfile send success, usingtime: "+timer.ElapsedTimeSpan);
                }

                File.Move(newname, filename);
            }
        }
        void AddTestTime(string xmlfile, CommandData cmd)//change the datetime in the xml file
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlfile);
            XmlElement root =(XmlElement) xml.SelectSingleNode("testRequest");
            XmlElement datetime = (XmlElement)root.SelectSingleNode("datetime");
            XmlElement repo = (XmlElement)root.SelectSingleNode("reposetory");
            root.RemoveChild(repo);
            root.RemoveChild(datetime);
            datetime = xml.CreateElement("datetime");
            datetime.InnerText = cmd.dateTime;
            root.AppendChild(datetime);
            repo = xml.CreateElement("reposetory");
            repo.InnerText = "../../../THtemp/";
            root.AppendChild(repo);
            xml.Save(xmlfile);
        }
        void OnNewMessageHandler(string msg)
        {
            textBox7.Text += msg;
        }

        private void button3_Click(object sender, RoutedEventArgs e)//click event, update the list before, and call the windows API to help choose dll files
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DLLFiles|*.dll";
            ofd.Multiselect = true;
            ofd.ShowDialog();
            int i = 0;
            foreach (string name in ofd.FileNames)
            {
                if (name != "")
                {
                    listBox.Items.Insert(i, name);
                }
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)//remove selected item in the listbox
        {
            try {
                listBox.Items.RemoveAt(listBox.Items.IndexOf(listBox.SelectedItem));
            }
            catch
            { 
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)//link button, we open the string listener and file listener here
        {
            string localPort = port.Text;
            string url = "http://localhost:" + localPort ;
            string strListener = url + "/CStrRcvr";
            string FListener = url + "/CFRcvr";
            cmd.url = url;
            try
            {
                strrcvr = new stringReceiver();
                strrcvr.CreateRecvChannel(strListener);
                rcvThrd = new Thread(new ThreadStart(this.ThreadProc));
                rcvThrd.Start();
                host = fileReceiver.CreateChannel(FListener, "../../../log/");
                host.Open();
                button5.IsEnabled = true;
                button.IsEnabled = false;
            }
            catch(Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append(strListener.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button5_Click(object sender, RoutedEventArgs e)//send the query request
        {
            cmd.command = "Request Query";
            cmd.testAuthor = author.Text.Replace(" ", "_");
            cmd.dateTime = datetime.Text;
            stringSender sndr = new stringSender("http://localhost:8081/RepoStrRcvr");
            CommandEncoder cmdEnocoder = new CommandEncoder(cmd);
            sndr.PostMessage(cmdEnocoder.encode());
            if (File.Exists("../../../log/" + cmd.testAuthor + "_" + cmd.dateTime + ".txt"))
                try {
                    File.Delete("../../../log/" + cmd.testAuthor + "_" + cmd.dateTime + ".txt");
                }
                catch
                {

                }
            Thread th = new Thread(s => { IsFileExist((string)s); });
            th.Start("../../../log/" + cmd.testAuthor + "_" + cmd.dateTime + ".txt");
            textBox7.Text += "----------------this is demo for requirement 9";
        }

        private void button2_Click(object sender, RoutedEventArgs e)//call windows API to help choose a xml file
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.InitialDirectory = "../../../";
            ofd.Filter = "XMLFiles|*.xml";
            ofd.Multiselect = false;
            ofd.ShowDialog();
            if (ofd.FileName != "")
            {
                xmlfile.Text = ofd.FileName;
                button1.IsEnabled = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)//send test request
        {
            button1.IsEnabled = false;
            cmd.command = "Test";
            cmd.testAuthor = author.Text.Replace(" ","_");
            cmd.testName = name.Text;
            cmd.dateTime = DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss");
            //cmd.dateTime = "11_20_2016_09_00_00";
            cmd.xmlFile = System.IO.Path.GetFileName(xmlfile.Text);
            AddTestTime(xmlfile.Text,cmd);
            changeNameAndSendToRepo(xmlfile.Text,cmd);
            for(int i = 0; i < listBox.Items.Count; i++)
            {
                cmd.dllFiles.Add(System.IO.Path.GetFileName((string) listBox.Items.GetItemAt(i)));
                changeNameAndSendToRepo((string)listBox.Items.GetItemAt(i),cmd);
            }
            try
            {
                stringSender sndr = new stringSender("http://localhost:8080/THStrRcvr");
                CommandEncoder cmdEnocoder = new CommandEncoder(cmd);
                sndr.PostMessage(cmdEnocoder.encode());
                textBox7.Text += "\n the datetime for query is: " + cmd.dateTime;
                Thread th = new Thread(s=> { IsFileExist((string)s); });
                th.Start("../../../log/" + cmd.testAuthor + "_" + cmd.dateTime + ".txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            button1.IsEnabled = true;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
