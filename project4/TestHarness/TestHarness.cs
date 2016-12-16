//Author: Jiayi Zou
//this is the test harness class
//which function is test harness create the child appdomain
//child appdomain will create a Test Logger, dequeue a xml stream and call the test loader to do the test
//here also contains a class run which inherit MarshalByRefObject which means the child appdomain can run some method in this class by the instance 
//all rights reserved
//version 1.0
//Jiayi Zou 11/22/16
//version 2.0
//add support of WCF, now the test harness can be run as the local test harness or remote test harness
//
//public interface:
//testHarness(SWTools.BlockingQueue<System.IO.FileStream> _q,string _LogFolder) local test harness, need a blockqing queue which contains file stream of the xml and a addree to save the log
//public testHarness(string _url) remote test harness, need the IP and port of the testharness
//public startRemote() start remote testharness
//public startloacl() start local testharness
using System;
using System.Security.Policy;
using SWTools;
using System.Reflection;
using System.Threading;
using TestLogger;
using TestHarness;
using System.ServiceModel;
using System.IO;

namespace TestHarness
{
    public class testHarness
    {
        private AppDomain child;
        private SWTools.BlockingQueue<System.IO.FileStream> q;
        private BlockingQueue<string> receivedMsg = new BlockingQueue<string>();
        private string LogFolder;
        private string url,thStrRcvr,thFRcvr;//receivers
        private string Repourl = "http://localhost:8081/";
        private string RepoStrRcvr = "http://localhost:8081/RepoStrRcvr";
        private string RepoFRcvr = "http://localhost:8081/RepoFRcvr";
        stringReceiver recvr;
        stringSender sndr;
        Thread rcvThrd = null;
        string rcvdMsg = "";
        HiResTimer timer = new HiResTimer();
        public testHarness(SWTools.BlockingQueue<System.IO.FileStream> _q,string _LogFolder) { q = _q;LogFolder = _LogFolder; }//local testharness
        public testHarness(string _url)
        {
            url = _url;
            //in project4 the ip address of test harness shoulbe hard coded
            url = "http://localhost:8080/";
            thStrRcvr = url + "THStrRcvr";
            thFRcvr = url + "THFRcvr";
            q = new BlockingQueue<System.IO.FileStream>();
            LogFolder = "../../../THtemp/";
        }
        void strThreadProc()
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty, 
                rcvdMsg = recvr.GetMessage();//get message
                Console.WriteLine("\nthis is for requirement 10, message received by WCF and the message is :\n,{0}", rcvdMsg);
                CommandData cmdData = parsingMessage(rcvdMsg);//parsing message
                procMsg(cmdData);//
            }
        }
        void startStringListener(string endpoint)
        {
            try
            {
                recvr = new stringReceiver();
                recvr.CreateRecvChannel(endpoint);
                rcvThrd = new Thread(new ThreadStart(this.strThreadProc));
                //rcvThrd.IsBackground = true;
                rcvThrd.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        CommandData parsingMessage(string msg)
        {
            CommandData cmdData = new CommandData();
            CommandDecoder cmdDecoder = new CommandDecoder(msg);
            cmdData = cmdDecoder.parse();
            return cmdData;
        }
        void procMsg(CommandData cmdData)
        {
            //command table:
            //Test: client request a test
            //then ask repository to get the message
            //to do that, we need to open a string sender to send the request to the repository, the IP address of repository is hardcoded
            //the filename in the test repository shouble be original file name + time stamp
            switch (cmdData.command)
            {
                case "Test":
                    //we need to mofity the command, from and to
                    caseTest(cmdData);
                    break;
                case "RunTest":
                    caseRunTest(cmdData);
                    break;
                case "FileNotMatch":
                    caseFileNotMatch(cmdData);
                    break;
                default:
                    Console.WriteLine("invaild requirement");
                    break;
            }
        }
        void caseFileNotMatch(CommandData cmdData)
        {
            cmdData.from = "th";
            cmdData.to = "client";
            //then we send it to the client
            try
            {
                sndr = new stringSender(cmdData.url+ "/CStrRcvr");
                CommandEncoder cmdEnocoder = new CommandEncoder(cmdData);
                sndr.PostMessage(cmdEnocoder.encode());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void caseRunTest(CommandData cmdData)
        {
            string path = "../../../THtemp/" +cmdData.testAuthor+"_"+cmdData.dateTime+"_"+cmdData.xmlFile;
            System.IO.FileStream xml = new System.IO.FileStream(path, System.IO.FileMode.Open);
            q.enQ(xml);
            if (startlocal() == true)//now the log file is prepared and we should send it to the repository now
            {
                IFileService fs = null; IFileService fs2 = null;
                int count = 0;
                while (true)
                {
                    try
                    {
                        fs = TestHarness.fileSender.CreateChannel(RepoFRcvr);
                        fs2 = TestHarness.fileSender.CreateChannel(cmdData.url+"/CFRcvr");
                        break;
                    }
                    catch
                    {
                        Console.Write("\n  connection to service failed {0} times - trying again", ++count);
                        Thread.Sleep(500);
                        continue;
                    }
                }
                Console.Write("\n  Connected to {0}\n", RepoFRcvr);
                string relativeFilePath = "../../../THtemp/";
                string filepath = Path.GetFullPath(relativeFilePath);
                string file = filepath + cmdData.testAuthor + "_" + cmdData.dateTime + ".txt";
                    Console.Write("----------this is for requirement 7 and 8------------------\n  sending file {0} to the client({1}) and reposetory", file,cmdData.url); timer.Start();
                if (!fileSender.SendFile(fs, file)) {
                    Console.Write("\n  could not send file"); timer.Stop();
                }
                else
                {
                    timer.Stop();
                    Console.WriteLine("\nsend file {0} success in {1} ",file,timer.ElapsedTimeSpan);
                }
                if (!fileSender.SendFile(fs2, file))
                {
                    Console.Write("\n  could not send file"); timer.Stop();
                }
                else
                {
                    timer.Stop();
                    Console.WriteLine("\nsend file {0} success in {1} ", file, timer.ElapsedTimeSpan);
                }
            }   
        }

        void caseTest(CommandData cmdData)
        {
            cmdData.command = "Request File";
            cmdData.from = url;
            cmdData.to = Repourl;
            //then we send it to the repository
            try
            {
                //string endpoint = "http://localhost:8080/IStringCommunicator";
                sndr = new stringSender(RepoStrRcvr);
                CommandEncoder cmdEnocoder = new CommandEncoder(cmdData);
                sndr.PostMessage(cmdEnocoder.encode());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void startRemote()//for remote test harness only. the purpose is prepare everything we need to start the local version of test harness.
        {
            int i = 0;
            while (i < 5)
            {
                try {
                    this.startStringListener(thStrRcvr);
                    ServiceHost host = fileReceiver.CreateChannel("http://localhost:8080/THFRcvr", "../../../THtemp/");
                    host.Open();
                    Console.WriteLine("test harness string Listener started\ntest harness file listener started");
                    break;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    i++;
                }
            }
        }
        public void createChildAppDomain()  //create child appdomain 
        {
            AppDomain main = AppDomain.CurrentDomain;
            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase
              = "file:///" + System.Environment.CurrentDirectory;
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            child = AppDomain.CreateDomain("ChildDomain", null);
            if (child == null)
                return;
          Console.WriteLine("---------------this is demo for requirement 4-------------\r\n child create success and it's appdomain's assembile is " + child.GetAssemblies());//for demo use only, requirement 3
            string assemblyName = Assembly.GetExecutingAssembly().FullName;
            run r =
            (run)child.CreateInstanceAndUnwrap(
                assemblyName,
                typeof(run).FullName
            ); timer.Start();
            r.execuate(q, LogFolder);timer.Stop();Console.WriteLine("\n-------------this is demo for requirement 12 and the testing time is :{0}--------------\n",timer.ElapsedTimeSpan);
            AppDomain.Unload(child);
            Console.WriteLine("----------------this is demo for requirement 7-------------------\r\nchild appdomain unloaded");
            return;
        }
        public bool startlocal()//each Test request will have its own child appdomain
        {                       //the local start part
            bool finished = false;//here finished may be need to lock
            while (q.size() != 0)
            {
                Thread th = new Thread(new ThreadStart(createChildAppDomain));
                finished = false;
                th.Start();
                Console.WriteLine("--------------this is for requirement 4-----------------\nthread start,id={0}", th.ManagedThreadId.ToString());
                th.Join();
                finished = true;
                Console.WriteLine("--------------this is for requirement 4-----------------\nthread terminate");
                q.deQ();
                //return finished;
            }
            return finished;
        }
    }
    public class run:MarshalByRefObject//so that I can use the interface to run the test in the child appdomain
    {
        
        public run() {}
        void processMessages(SWTools.BlockingQueue<TestData> p, SWTools.BlockingQueue<System.IO.FileStream> q)  //getMessage and decode message from Queue, return metadata of the test
               //p includes test, q include xmlstream.
               //we need to transfer p to TestLoader
        {
            System.IO.FileStream current = q.deQ();
            XmlDecoder decoder = new XmlDecoder();
            decoder.parse(current, p);
        }

        public bool execuate(SWTools.BlockingQueue<System.IO.FileStream> q,string LogFolder)//execute the test
        {
            TestLogger.TestLogger log = new TestLogger.TestLogger();
            try
            {
                SWTools.BlockingQueue<TestData> p = new SWTools.BlockingQueue<TestData>();
                TestLoader.TestLoader loader = new TestLoader.TestLoader();
              //every testrequest should have its own log
                    log.clear();
                    log.setFolder(LogFolder);
                processMessages(p, q);
                    loader.loadTest(p, log);
                return true;
            }
            catch (Exception ex)
            {
                log = new TestLogger.TestLogger();
                log.writeLine(ex.Message);
                log.writeLine("error time" + DateTime.Now);
                return false;
            }
        }
    }
//--------------------------test sub--------------------------

    class program
    {
        static void Main(string[] args)
        {
            testHarness th = new testHarness("");
            th.startRemote();
#if (TEST_TESTHARNESS)
            CommandData cmd = new CommandData();
            cmd.from = "localhost";
            cmd.to = "localhost";
            cmd.testAuthor = "me";
            cmd.testName = "test";
            cmd.command = "Test";
            DateTime tm = new DateTime(2016, 11, 20, 09, 00, 00);
            cmd.dateTime = tm.ToString("MM_dd_yyyy_hh_mm_ss");
            cmd.url = "http://localhost:8082/";
            cmd.dllFiles.Add("tc1.dll");
            cmd.dllFiles.Add("tc2.dll");
            cmd.dllFiles.Add("td1.dll");
            cmd.dllFiles.Add("td2.dll");
            cmd.xmlFile = "XMLFile1.xml";
            try
            {
                stringSender sndr = new stringSender("http://localhost:8080/THStrRcvr");
                CommandEncoder cmdEnocoder = new CommandEncoder(cmd);
                sndr.PostMessage(cmdEnocoder.encode());
                cmd.testAuthor = "me1";
                cmdEnocoder = new CommandEncoder(cmd);
                sndr.PostMessage(cmdEnocoder.encode());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
#endif
        }
    }

}
