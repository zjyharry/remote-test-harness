//author: Jiayi Zou
//this is the repository we need in the project 4
//the function of repository is:
//it can responed and send required files(if have) to the client or TH,(if not have) send the message to the client or TH to tell them that the file not exist
//the detailed command table can be seen in readme.txt
//version 10.0
//11/22/2016
//public Interface:
//void startRepository() 
//start the repository
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using TestHarness;
using System.ServiceModel;

namespace Repository
{
    class Repository
    {
        private string rcvdMsg;
        private stringReceiver recvr;
        private stringSender sndr;
        private Thread rcvThrd = null;
        string THFRcvr = "http://localhost:8080/THFRcvr";
        private bool success = true;
        string relativeFilePath = "../../../repo/";
        HiResTimer timer = new HiResTimer();
        void strThreadProc()//string receive thread
        {
            while (true)
            {
                // get message out of receive queue - will block if queue is empty, 
                rcvdMsg = recvr.GetMessage();//get message
                Console.WriteLine("\nthis is for requirement 10, message received by WCF and the message is :\n" + rcvdMsg);
                CommandData cmdData = parsingMessage(rcvdMsg);//parsing message
                procMsg(cmdData);//
            }
        }
        void startStringListener(string endpoint)//start string listener
        {
            try
            {
                recvr = new stringReceiver();
                recvr.CreateRecvChannel(endpoint);
                rcvThrd = new Thread(new ThreadStart(this.strThreadProc));
                rcvThrd.Start();
                Console.WriteLine("string listener start");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        CommandData parsingMessage(string msg)//pasing the received string
        {
            CommandData cmdData = new CommandData();
            CommandDecoder cmdDecoder = new CommandDecoder(msg);
            cmdData = cmdDecoder.parse();
            return cmdData;
        }
        void caseRequestFile(CommandData cmdData)//command to request file
        {
            success = true;
            IFileService fs = null;
            int count = 0;
            while (true)
            {
                try
                {
                    fs = TestHarness.fileSender.CreateChannel(THFRcvr);
                    break;
                }
                catch
                {
                    Console.Write("\n  connection to service failed {0} times - trying again", ++count);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Console.Write("\n  Connected to {0}\n", THFRcvr);
            string filepath = Path.GetFullPath(relativeFilePath);
            List<string> dllFiles;
            dllFiles = XmlDecoder.getDLLFiles("../../../repo/"+cmdData.testAuthor+"_"+cmdData.dateTime+"_"+cmdData.xmlFile);
            dllFiles.Add(cmdData.xmlFile);
            foreach (string dllfile in dllFiles){
                Console.WriteLine("-------------this is for requirement 2----------:\nthe file required in the XML include:" + dllfile);
                string file = filepath + cmdData.testAuthor + "_" + cmdData.dateTime +"_"+ dllfile;
                timer.Start();
                if (!fileSender.SendFile(fs, file))
                {
                    timer.Stop();
                    Console.Write("\n  could not send file");
                    cmdData.command = "FileNotMatch";
                    cmdData.from = "Repository";
                    cmdData.to = "TestHarness";
                    success = false;
                }
                else {
                    timer.Stop();
                    Console.WriteLine("send file :{0} in {1} ",file,timer.ElapsedTimeSpan); }
                Console.Write("\n\n");
            }
            if (success == true) { sendRunTest(cmdData); } else { sendFileNotMatch(cmdData); }
        }
        void sendFileNotMatch(CommandData cmdData)//when we not find the match
        {
            cmdData.command = "FileNotMatch";
            cmdData.from = "Repo";
            cmdData.to = "TH";
            try
            {
                stringSender sndr = new stringSender("http://localhost:8080/THStrRcvr");
                CommandEncoder cmdEnocoder = new CommandEncoder(cmdData);
                sndr.PostMessage(cmdEnocoder.encode());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        void sendRunTest(CommandData cmdData)//tell TH to run the test
        {
            cmdData.command = "RunTest";
            cmdData.from = "Repo";
            cmdData.to = "TH";
            try
            {
                stringSender sndr = new stringSender("http://localhost:8080/THStrRcvr");
                CommandEncoder cmdEnocoder = new CommandEncoder(cmdData);
                sndr.PostMessage(cmdEnocoder.encode());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        void caseRequestQuary(CommandData cmdData)//command from client to ask a query
        {
            IFileService fs = null;
            string CFRcvr = cmdData.url + "/CFRcvr";
            int count = 0;
            while (true)
            {
                try
                {
                    fs = TestHarness.fileSender.CreateChannel(CFRcvr);
                    break;
                }
                catch
                {
                    Console.Write("\n  connection to service failed {0} times - trying again", ++count);
                    Thread.Sleep(500);
                    continue;
                }
            }
            Console.Write("\n  Connected to {0}\n", CFRcvr);
            string relativeFilePath = "../../../repo/";
            string filepath = Path.GetFullPath(relativeFilePath);
            string file = filepath + cmdData.testAuthor + "_" + cmdData.dateTime + ".txt";
            try
            {
                Console.Write("\n  sending file {0}", file);
                timer.Start();
                if (!fileSender.SendFile(fs, file)){
                    timer.Stop();
                    Console.Write("\n  could not send file");
                    cmdData.command = "NoResult";
                    cmdData.from = "Repository";
                    cmdData.to = "Client";
                    string csl = cmdData.url + "/CStrRcvr";
                    try{
                        sndr = new stringSender(csl);
                        CommandEncoder cmdEnocoder = new CommandEncoder(cmdData);
                        sndr.PostMessage(cmdEnocoder.encode());
                    }
                    catch (Exception ex){
                        Console.WriteLine(ex.ToString());
                    }
                }
                else{
                    timer.Stop();
                    Console.WriteLine("send file :{0} in {1} ", file, timer.ElapsedTimeSpan);
                }
            }
            catch{}
        }
        void procMsg(CommandData cmdData)//parse command
        {
            //command table can be found in readme.txt
            switch (cmdData.command)
            {
                case "Request File":
                    caseRequestFile(cmdData);
                    break;
                case "Request Query":
                    caseRequestQuary(cmdData);
                    break;
                default:
                    Console.WriteLine("invaild requirement");
                    break;
            }
        }
        public void startRepository()//start the repository
        {
            int i = 0;
            while (i < 5) { 
            try
            {
                startStringListener("http://localhost:8081/RepoStrRcvr");
                ServiceHost host = fileReceiver.CreateChannel("http://localhost:8081/RepoFRcvr", "../../../repo/");
                host.Open();
                    break;
            }
            catch (Exception ex)
            {
                    Console.WriteLine(ex.ToString());
                    i++;
            }
        }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Repository repo = new Repository();
            repo.startRepository();
            //------------------test sub------------------
#if (TEST_REPOSITORY)
            CommandData cmd = new CommandData();
            cmd.from = "client";
            cmd.to = "repo";
            cmd.testAuthor = "me";
            cmd.testName = "test";
            cmd.command = "Request Query";
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
                stringSender sndr = new stringSender("http://localhost:8081/RepoStrRcvr");
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
