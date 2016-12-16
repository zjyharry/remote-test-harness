//author: Jiayi Zou
//Source: Jim Fawcett
//this is the classes that can provide interface of timer, stringsender, string receiver, file sender, file receiver
//version 1.0
//11/22/2016
//public Interface:
//HiresTimer:
//ElapsedMicroseconds give the ms unit of time
//ElapsedTimeSpan gives the time stamp unit of time
//start() start the timer
//stop() stop the timer
//
//StringSender:
//stringSender(string url) start the string sender and try to connect to the endpoint(url)
//PostMessage(string msg) senf the message
//
//StringReceiver
//CreateRecvChannel(string address) create the endpoint
//getMessage() get the message
//
//FileSender
//public static IFileService CreateChannel(string url) create the file sender endpoint
//public static bool SendFile(IFileService service, string file) sendfile
//
//FileReceiver
//public static ServiceHost CreateChannel(string url,string folder) create the file receiver endpoint and set the path to save




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TestHarness
{
    public class HiResTimer
    {
        protected ulong a, b, f;

        public HiResTimer()
        {
            a = b = 0UL;
            if (QueryPerformanceFrequency(out f) == 0)
                throw new Win32Exception();
        }

        public ulong ElapsedTicks
        {
            get
            { return (b - a); }
        }

        public ulong ElapsedMicroseconds
        {
            get
            {
                ulong d = (b - a);
                if (d < 0x10c6f7a0b5edUL) // 2^64 / 1e6
                    return (d * 1000000UL) / f;
                else
                    return (d / f) * 1000000UL;
            }
        }

        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                ulong t = 10UL * ElapsedMicroseconds;
                if ((t & 0x8000000000000000UL) == 0UL)
                    return new TimeSpan((long)t);
                else
                    return TimeSpan.MaxValue;
            }
        }

        public ulong Frequency
        {
            get
            { return f; }
        }

        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out a);
        }

        public ulong Stop()
        {
            QueryPerformanceCounter(out b);
            return ElapsedTicks;
        }

        // Here, C# makes calls into C language functions in Win32 API
        // through the magic of .Net Interop

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern
           int QueryPerformanceFrequency(out ulong x);

        [DllImport("kernel32.dll")]
        protected static extern
           int QueryPerformanceCounter(out ulong x);
    }
    public class stringSender
    {
        IStringCommunicator channel;
        string lasterror = "";
        SWTools.BlockingQueue<string> sndQueue = null;
        Thread sndThrd = null;
        int tryCount = 0, maxCount = 10;
        void ThreadProc()
        {
            string msg = "";
            while (true)
            {
                try {
                    msg = sndQueue.deQ();
                    channel.PostMessage(msg);
                    if (msg == "quit")
                        break;
                }
                catch(Exception e)
                {
                    //Console.WriteLine("{0}\n{1}",msg,e.ToString()); too detail
                    Console.WriteLine("\nsending string fall "+e.Message);
                }
            }
        }
        public stringSender(string url)
        {
            sndQueue = new SWTools.BlockingQueue<string>();
            while (true)
            {
                try
                {
                    CreateSendChannel(url);
                    tryCount = 0;
                    break;
                }
                catch (Exception ex)
                {
                    if (++tryCount < maxCount)
                        Thread.Sleep(100);
                    else
                    {
                        lasterror = ex.Message;
                        break;
                    }
                }
            }
            sndThrd = new Thread(new ThreadStart(ThreadProc));
            sndThrd.Start();
        }
        public void CreateSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            BasicHttpBinding binding = new BasicHttpBinding();
            ChannelFactory<IStringCommunicator> factory
              = new ChannelFactory<IStringCommunicator>(binding, address);
            channel = factory.CreateChannel();
        }
        public void PostMessage(string msg)
        {
            sndQueue.enQ(msg);
        }

        public string GetLastError()
        {
            string temp = lasterror;
            lasterror = "";
            return temp;
        }

        public void Close()
        {
            ChannelFactory<IStringCommunicator> temp = (ChannelFactory<IStringCommunicator>)channel;
            temp.Close();
        }
    }
    public class stringReceiver : IStringCommunicator
    {
        static SWTools.BlockingQueue<string> rcvQueue = null;
        ServiceHost service = null;
        public stringReceiver()
        {
            if (rcvQueue == null)
                rcvQueue = new SWTools.BlockingQueue<string>();
        }
        public void Close()
        {
            service.Close();
        }
        public void CreateRecvChannel(string address)
        {
            try
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                Uri baseAddress = new Uri(address);
                service = new ServiceHost(typeof(stringReceiver), baseAddress);
                service.AddServiceEndpoint(typeof(IStringCommunicator), binding, baseAddress);
                service.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void PostMessage(string msg)
        {
            rcvQueue.enQ(msg);
        }

        public string GetMessage()
        {
            return rcvQueue.deQ();
        }
    }
    public class fileSender
    {
        public static IFileService CreateChannel(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IFileService> factory = new ChannelFactory<IFileService>(binding, address);
            return factory.CreateChannel();
        }
        public static bool SendFile(IFileService service, string file)
        {
            long blockSize = 512;
            FileStream fs=null;
            try
            {
                string filename = Path.GetFileName(file);
                service.OpenFileForWrite(filename);
                fs = File.Open(file, FileMode.Open, FileAccess.Read);
                int bytesRead = 0;
                while (true)
                {
                    long remainder = (int)(fs.Length - fs.Position);
                    if (remainder == 0)
                        break;
                    long size = Math.Min(blockSize, remainder);
                    byte[] block = new byte[size];
                    bytesRead = fs.Read(block, 0, block.Length);
                    service.WriteFileBlock(block);
                }
                fs.Close();
                service.CloseFile();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  can't open {0} for writing - {1}", file, ex.Message);
                service.CloseFile();
                return false;
            }
        }
    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class fileReceiver : IFileService
    {
        string filePath = "../../../sentfiles/";
        string fileSpec = "";
        FileStream fs = null;  // remove static for WSHttpBinding

        public void SetServerFilePath(string path)
        {
            filePath = path;
        }
        public bool OpenFileForWrite(string name)
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            fileSpec = filePath + "\\" + name;
            try
            {
                fs = File.Open(fileSpec, FileMode.Create, FileAccess.Write);
                Console.Write("\n------------------this is demo for requirement 6------------------\n  {0} opened", fileSpec);
                return true;
            }
            catch
            {
                Console.Write("\n  {0} filed to open", fileSpec);
                return false;
            }
        }
        public bool WriteFileBlock(byte[] block)
        {
            try
            {
                Console.Write("\n  writing block with {0} bytes", block.Length);
                fs.Write(block, 0, block.Length);
                fs.Flush();
                return true;
            }
            catch { return false; }
        }
        public bool CloseFile()
        {
            try
            {
                fs.Close();
                Console.Write("\n  {0} closed", fileSpec);
                return true;
            }
            catch { return false; }
        }
        public static ServiceHost CreateChannel(string url,string folder)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(url);
            //Type service = typeof(FileTransferService.FileService);
            fileReceiver service = new fileReceiver();
            //if we use object instead of file, we can use the service desided the directory directly
            service.SetServerFilePath(folder);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(TestHarness.IFileService), binding, baseAddress);
            return host;
        }
        public static ServiceHost CreateChannel(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(url);
            //Type service = typeof(FileTransferService.FileService);
            fileReceiver service = new fileReceiver();
            //if we use object instead of file, we can use the service desided the directory directly
            //service.SetServerFilePath("./sendfiles");
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(TestHarness.IFileService), binding, baseAddress);
            return host;
        }
    }
    class testString
    {
        stringReceiver recvr;
        stringSender sndr;
        Thread rcvThrd = null;
        string rcvdMsg = "";
        void ThreadProc()
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty
                rcvdMsg = "from sender:" + recvr.GetMessage();
                Console.WriteLine(rcvdMsg);
            }
        }
        void startListener(string endpoint)
        {
            //createlistener
            //string localport = "8080";
            //string endpoint = "http://localhost:8080/IStringCommunicator";
            try
            {
                recvr = new stringReceiver();
                recvr.CreateRecvChannel(endpoint);
                rcvThrd = new Thread(new ThreadStart(this.ThreadProc));
                rcvThrd.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        void connectToListener(string endpoint)
        {
            try
            {
                //string endpoint = "http://localhost:8080/IStringCommunicator";
                sndr = new stringSender(endpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        void sendMessage(string msg)
        {
            try
            {
                sndr.PostMessage(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
//------------------test sub---------------------
#if (TEST_STRING_SENDERANDRECEIVER)
            static void Main()
            {
                testString ts = new testString();
                ts.startListener("http://localhost:8080/IStringCommunicator");
                Console.WriteLine("Listener starting");
                ts.connectToListener("http://localhost:8080/IStringCommunicator");
                ts.sendMessage("hello world");
                ts.sendMessage("thank you, Luo");
            }
#endif
    }
    class testFile
    {
#if (TEST_FILE_SENDERANDRECEIVER)
        void createListener(string url)
        {
            ServiceHost host = fileReceiver.CreateChannel(url);
            host.Open();
        }
        void createSender(string url)
        {
            IFileService fs = null;
            int count = 0;
            while (true)
            {
                try
                {
                    fs = TestHarness.fileSender.CreateChannel(url);
                    break;
                }
                catch
                {
                    Console.Write("\n  connection to service failed {0} times - trying again", ++count);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Console.Write("\n  Connected to {0}\n", url);
            string relativeFilePath = "FilesToSend";

            string filepath = Path.GetFullPath(relativeFilePath);
            Console.Write("\n  retrieving files from\n  {0}\n", filepath);
            string[] files = Directory.GetFiles(filepath);
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                Console.Write("\n  sending file {0}", filename);

                if (!fileSender.SendFile(fs, file))
                    Console.Write("\n  could not send file");
            }
            Console.Write("\n\n");
        }
        static void Main()
        {
            string url = "http://localhost:8080/FileService";
            testFile tf = new testFile();
            tf.createListener(url);
            tf.createSender(url);
        }
#endif
    }

}


