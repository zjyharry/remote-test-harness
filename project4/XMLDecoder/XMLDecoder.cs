//Author: Jiayi Zou
//this is the XML decoder, which contians a TestData class which contains all imformation we need in a single test and a decoder which can decode a xml stream and get all testdata in it
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
//version 2.0
//modify:
//add the CommandData, CommandDecoder, CommandEncoder class to set the standard of the command in the project
//public interface:
//TestData:
//public void show() show all information in the testdata
//commandData:
//commandData() constructor
//commandDecoder:
//public CommandDecoder(string str) constructor, set the string need to be decoded
//public CommandData parse() decode tje command, stored in a commandData object
//commandEncoder:
//public CommandEncoder(CommandData cmd) constructor, set the commandDate need to be incode
//public string encode() encode the commandData into a string


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TestHarness
{
    
    public class TestData
    {
        public bool success { get; set; }
        public string testName { get; set; }
        public string author { get; set; }
        public string datetime { get; set; }
        public DateTime timeStamp { get; set; }
        public String testDriver { get; set; }
        public List<string> testCode { get; set; }
        public String repo { get; set; }
        public void show()//show all information in the testdata
        {
            Console.Write("\n  {0,-12} : {1}", "test name", testName);
            Console.Write("\n  {0,12} : {1}", "author", author);
            Console.Write("\n  {0,12} : {1}", "reposetory", repo);
            Console.Write("\n  {0,12} : {1}", "time stamp", timeStamp);
            Console.Write("\n  {0,12} : {1}", "test driver", testDriver);
            foreach (string library in testCode)
            {
                Console.Write("\n  {0,12} : {1}", "library", library);
            }
        }
    }
    public class CommandData//include all the imformation for the test harness to find the file in the repository and so on
    {
        public CommandData()//constructor
        {
            dllFiles = new List<string>();
        }
        public string from { get; set; }
        public string to { get; set; }
        public string command { get; set; }
        public string testAuthor { get; set; }
        public string testName { get; set; }
        public string dateTime { get; set; }
        public string xmlFile { get; set; }
        public List<string> dllFiles { get; set; }
        public string url { get; set; }
    }

    public class CommandDecoder
    {
        private XDocument _doc;
        private List<string> _dllFiles;
        public CommandDecoder(string str)
        {
            _doc = new XDocument();
            _doc = XDocument.Parse(str);
            _dllFiles = new List<string>();
        }
        public CommandData parse()
        {
            CommandData data = new CommandData();
            if (_doc == null)
            {
                data.command = "NOCOMMAND";
            }
            data.from = _doc.Descendants("from").First().Value;
            data.to = _doc.Descendants("to").First().Value;
            data.testAuthor = _doc.Descendants("testAuthor").First().Value;
            data.command = _doc.Descendants("command").First().Value;
            data.testName = _doc.Descendants("testName").First().Value;
            data.dateTime = _doc.Descendants("dateTime").First().Value;
            data.xmlFile = _doc.Descendants("xmlFile").First().Value;
            data.url = _doc.Descendants("url").First().Value;
            XElement tx = _doc.Descendants("dllFiles").First();
            XElement[] x = tx.Descendants("dllFile").ToArray();
            foreach (var xfile in x)
            {
                data.dllFiles.Add(xfile.Value);
            }

            return data;
        }
    }

    public class CommandEncoder
    {
        private XDocument xmlstr;
        private CommandData _cmd;
        public CommandEncoder(CommandData cmd)
        {
            xmlstr = new XDocument();
            _cmd = cmd;
        }
        public string encode()
        {
            xmlstr = new XDocument();
            xmlstr.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XElement root = new XElement("message");
            XElement from = new XElement("from", _cmd.from);
            XElement to = new XElement("to", _cmd.to);
            XElement author = new XElement("testAuthor",_cmd.testAuthor);
            XElement command = new XElement("command",_cmd.command);
            XElement testName = new XElement("testName",_cmd.testName);
            XElement dateTime = new XElement("dateTime",_cmd.dateTime);
            XElement xmlFile = new XElement("xmlFile",_cmd.xmlFile);
            XElement url = new XElement("url",_cmd.url);
            XElement dllfiles = new XElement("dllFiles");
            xmlstr.Add(root);
            root.Add(from);
            root.Add(to);
            root.Add(author);
            root.Add(command);
            root.Add(testName);
            root.Add(dateTime);
            root.Add(xmlFile);
            root.Add(url);
            root.Add(dllfiles);
            foreach(string file in _cmd.dllFiles)
            {
                XElement temp = new XElement("dllFile", file);
                dllfiles.Add(temp);
            }
            return xmlstr.ToString();
        }
    }
    public class XmlDecoder
    {
        private XDocument doc_;
        private List<TestData> testList_;
        public XmlDecoder()//initializaiton
        {
            doc_ = new XDocument();
            testList_ = new List<TestData>();
        }
        public static List<string> getDLLFiles(string xml)
        {
            XDocument doc = XDocument.Load(xml);
            TestData test;
            List<string> temp = new List<string>();
            XElement[] xtests = doc.Descendants("test").ToArray();
            int numTests = xtests.Count();
            try
            {
                for (int i = 0; i < numTests; ++i)
                {
                    test = new TestData();
                    test.testName = xtests[i].Attribute("name").Value;
                    if(xtests[i].Element("testDriver").Value!="END")
                    temp.Add(xtests[i].Element("testDriver").Value);
                    IEnumerable<XElement> xtestCode = xtests[i].Elements("library");
                    foreach (var xlibrary in xtestCode)
                    {
                        if (xlibrary.Value != "END")
                        {
                            temp.Add(xlibrary.Value);
                        }
                    }
                }
            }
            catch
            {

            }
            return temp.Distinct().ToList();
        }
        public void parse(System.IO.Stream xml, SWTools.BlockingQueue<TestData> q)//decode a xml file and put all test data into a blocking queue
        {
            TestData test;
            doc_ = XDocument.Load(xml);
            if (doc_ == null)
            {
                test = new TestData();
                test.success = false;
                test.author = "unknown";
                test.timeStamp = DateTime.Now;
                test.testName = "END";
                q.enQ(test);
                return;
            }
            string datetime = doc_.Descendants("datetime").First().Value;
            string author = doc_.Descendants("author").First().Value;
            string repo = doc_.Descendants("reposetory").First().Value;
            XElement[] xtests = doc_.Descendants("test").ToArray();
            int numTests = xtests.Count();
            try {
                for (int i = 0; i < numTests; ++i)
                {
                    test = new TestData();
                    test.success = true;
                    test.testCode = new List<string>();
                    test.author = author;
                    test.datetime = datetime;
                    test.timeStamp = DateTime.Now;
                    test.repo = repo;
                    test.testName = xtests[i].Attribute("name").Value;
                    test.testDriver = xtests[i].Element("testDriver").Value;
                    IEnumerable<XElement> xtestCode = xtests[i].Elements("library");
                    foreach (var xlibrary in xtestCode)
                    {
                        test.testCode.Add(xlibrary.Value);
                    } 
                    q.enQ(test);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("\n\n  {0}", ex.Message);
            }
            return;
        }
    }
//-----------------------test sub------------------------------
#if (TEST_XMLDECODER)
    class program{
        public static void Main(string[] args)
        {
            //XmlDecoder decoder = new XmlDecoder();
            //string path = "../../XMLFile1.xml";
            //System.IO.FileStream xml = new System.IO.FileStream(path, System.IO.FileMode.Open);
            //SWTools.BlockingQueue<TestData> p = new SWTools.BlockingQueue<TestData>();
            //decoder.parse(xml, p);
            //while (p.size() != 0)
            //{
            //    TestData td = p.deQ();
            //    td.show();
            //}
            CommandData cmd = new CommandData();
            cmd.from = "localhost";
            cmd.to = "localhost";
            cmd.testAuthor = "me";
            cmd.testName = "test";
            cmd.command = "testcmd";
            cmd.dateTime = "now";
            cmd.url = "localhost";
            cmd.dllFiles.Add("1");
            cmd.dllFiles.Add("2");
            cmd.xmlFile = "testxml";
            CommandEncoder cmde = new CommandEncoder(cmd);
            Console.WriteLine(cmde.encode());
            CommandDecoder cmdd = new CommandDecoder(cmde.encode());
            cmd = cmdd.parse();
            CommandEncoder cmde2 = new CommandEncoder(cmd);
            Console.WriteLine(cmde2.encode());
        }
    }
#endif
}
