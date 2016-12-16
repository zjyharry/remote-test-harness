using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TestHarness;

namespace TestLogger
{
    public class TestLogger:ITestInterface
    {
        private string LogFolder;
        private string Logger;
        public void setLogFolder(string _f)
        {
            LogFolder = _f;
        }
        public string getLogFolder()
        {
            return LogFolder;
        }
        public void clear() { Logger = ""; }
        public bool Test() { return false; }
        public string getLog()
        {
            return Logger;
        }
        public void writeLine(string str)
        {
            Logger = Logger + str + "\r\n";
        }
        public void writeToFile(string path)
        {
            System.IO.File.WriteAllText(path,Logger);
        }
        public void show()
        {
            Console.Write(Logger);
        }
    }
}
//-------------------------test sub--------------------------------
#if (TEST_TESTLOGGER)
public class program
{

    static void Main(string[] args)
    {
        TestLogger.TestLogger log = new TestLogger.TestLogger();
        log.writeLine("hello world");
        log.writeLine("681");
        log.writeLine("SMA");
        log.writeToFile("Test.txt");
    }
}
#endif
