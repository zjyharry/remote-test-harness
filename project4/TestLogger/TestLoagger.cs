//Author: Jiayi Zou
//this is the test logger class
//It can receive data from other part and store it, finally store it into a .TXT file
//It can be improved to store into a noSQL database
//but it is what to do in project 4
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
//public interface:
//public void setFolder(string _f) set log Folder
//public string getFolder() get log folder
//      public void clear() clean the log
//public bool Test()  test the Test Logger
//public string getLog()  get log
//public void write(String str)//write something after last character in the log
//public void writeLine(string str)//write something after last character in the log and start a new line
//public void writeToFile(string path)//write the log to a txt file
//public void show()//show the log
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
        public void setFolder(string _f)//set log Folder
        {
            LogFolder = _f;
        }
        public string getFolder()//get log folder
        {
            return LogFolder;
        }
        public void clear() { Logger = ""; }//clean the log
        public bool Test() { return false; }//test use, need further modify if someone want to test the Test Logger
        public string getLog()//get log
        {
            return Logger;
        }
        public void write(String str)//write something after last character in the log
        {
            Logger+=str;
        }
        public void writeLine(string str)//write something after last character in the log and start a new line
        {
            Logger = Logger + str + "\r\n";
        }
        public void writeToFile(string path)//write the log to a txt file
        {
            System.IO.File.WriteAllText(path,Logger);
        }
        public void show()//show the log
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
        log.write("SMA");
        log.writeToFile("Test.txt");
    }
}
#endif
