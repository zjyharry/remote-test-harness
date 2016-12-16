//1130-
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
        public string testName { get; set; }
        public string author { get; set; }
        public string datetime { get; set; }
        public DateTime timeStamp { get; set; }
        public String testDriver { get; set; }
        public List<string> testCode { get; set; }
        public String repo { get; set; }
        public void show()
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
    public class Command
    {
        public Command()
        {
            dllFiles = new List<string>();
        }
        public string source { get; set; }
        public string direc { get; set; }
        public string com { get; set; }
        public string Author { get; set; }
        public string Name { get; set; }
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
        public Command parse()
        {
            Command data = new Command();
            if (_doc == null)
            {
                data.com = "NOCOMMAND";
            }
            data.source = _doc.Descendants("source").First().Value;
            data.direc = _doc.Descendants("direc").First().Value;
            data.Author = _doc.Descendants("Author").First().Value;
            data.com = _doc.Descendants("com").First().Value;

            data.Name = _doc.Descendants("Name").First().Value;
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
        private Command _cmd;
        public CommandEncoder(Command cmd)
        {
            xmlstr = new XDocument();_cmd = cmd;
        }
        public string encode()
        {
            xmlstr = new XDocument();
            xmlstr.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XElement root = new XElement("message");
            XElement from = new XElement("source", _cmd.source);
            XElement to = new XElement("direc", _cmd.direc);
            XElement author = new XElement("Author",_cmd.Author);
            XElement command = new XElement("com",_cmd.com);
            XElement testName = new XElement("Name",_cmd.Name);
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
        public XmlDecoder()
        {
            doc_ = new XDocument();
            testList_ = new List<TestData>();
        }
        public static List<string> getDLL(string xml)
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
                    if (xtests[i].Element("testDriver").Value != "END")
                    {
                        temp.Add(xtests[i].Element("testDriver").Value);
                    }
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
        private void isEnd(int num, XElement[] test) { 
}
        public void parse(System.IO.Stream xml, SWTools.BlockingQueue<TestData> q)//decode a xml file and put all test data into a blocking queue
        {
            TestData test;
            doc_ = XDocument.Load(xml);
            if (doc_ == null)
            {
                test = new TestData();
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
            Command cmd = new Command();
            cmd.source = "localhost";
            cmd.direc = "localhost";
            cmd.Author = "me";
            cmd.Name = "test";
            cmd.com = "testcmd";
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
