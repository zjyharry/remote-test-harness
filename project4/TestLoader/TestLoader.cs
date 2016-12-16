//Author: Jiayi Zou
//this is the test loader, which function is load DLL we need, let test driver run the test, and write the log
//while a test name is END, it is a symbol that the test request is end  
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
//public interface:
//TestLoader(): constructor
//public void loadTest(SWTools.BlockingQueue<TestData> p, TestLogger.TestLogger log)//dequeue test in the test queue, and write log to the txt file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SWTools;
using TestLogger;
using TestHarness;
using System.IO;

namespace TestLoader
{
   public class TestLoader
    {
        void changename(TestData testData)
        {
            try
            {
                File.Delete(testData.repo + testData.testDriver);
            }
            catch
            {

            }
            try
            {
                File.Move(testData.repo + testData.author + "_" + testData.datetime + "_" + testData.testDriver, testData.repo + testData.testDriver);
                foreach (string lib in testData.testCode)
                {
                    File.Move(testData.repo + testData.author + "_" + testData.datetime + "_" + lib, testData.repo + lib);
                }
            }
            catch
            {

            }
        }
        void exec(TestData testData,TestLogger.TestLogger log,bool summary = true)//load dll file, and let the dll file run the test, write the log
        {
            changename(testData);
            try
            {
                Assembly assem = Assembly.LoadFrom(testData.repo +testData.testDriver);
                Type[] type = assem.GetExportedTypes();
                foreach (Type t in type)
                {
                    if (t.IsClass && typeof(ITestInterface).IsAssignableFrom(t))
                    {
                        ITestInterface tdr = (ITestInterface)Activator.CreateInstance(t);    // create instance of test driver
                        foreach (string lib in testData.testCode)
                        {
                            log.writeLine("test library is " + lib);
                            Console.WriteLine(tdr.getLog());
                            Console.WriteLine("--------this is for requirement 5-------\r\n Calling ITestInterface Test()");//this is just for demo
                            if (tdr.Test() == true)
                            {
                                log.writeLine("Test result : pass"); Console.WriteLine(" test() return true");
                            }
                            else
                            {
                                log.writeLine("Test result : fail"); Console.WriteLine(" test() return false");
                                summary = false;
                            }
                        }
                    }
                }
                if (summary == true)
                {
                    log.writeLine("Summary : pass\r\n\r\n\r\n");
                }
                else
                    log.writeLine("Summary : fail\r\n\r\n\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" catch error");//this is just for demo use
                log.writeLine("error: " + ex.Message);
                log.writeLine("time: " + DateTime.Now);
                log.writeLine("Summary : not execuatable\r\n\r\n\r\n");
            }
        }
        public void loadTest(SWTools.BlockingQueue<TestData> p, TestLogger.TestLogger log)//dequeue test in the test queue, and write log to the txt file
        {   bool show = true;
            TestData testData;
            while (p.size() != 0)
            {
                testData = p.deQ();
                if (show==true)
                  Console.WriteLine("Processing Test Authoried by"+testData.author);show = false;//this is just for demo use, which means it will be removed in test 4.
                if (testData.testName != "END")
                {
                    log.writeLine("Test Name  =  " + testData.testName);
                    log.writeLine("Test Author = " + testData.author);
                    log.writeLine("Test Time = " + testData.timeStamp);
                    bool summary = true;
                    exec(testData,log,summary);
                }
                else
                {
                    string filename = testData.author + "_" + testData.datetime + ".txt";
                    filename = log.getFolder() + filename;
                    log.writeToFile(filename);
                    //Console.WriteLine("---------------this is demo for requirement 7-------------\r\n" + "the key is the log file name and the file name is" + filename);
                    //Console.WriteLine("---------------this is demo for requirement 6-------------\r\n writing to file:\r\n" + filename);
                    //Console.WriteLine("\r\n getting log from logger" + log.getLog());
                }
            }
        }
    }
//---------------------test sub------------------------------
#if (TEST_TESTLOADER)
    class program
    {
        static void Main(string[] args)
        {
            TestLoader tl = new TestLoader();
            TestLogger.TestLogger log = new TestLogger.TestLogger();
            BlockingQueue<TestData> p = new BlockingQueue<TestData>();
            TestData test1 = new TestData();
            TestData test2 = new TestData();
            test1.author = "Jiayi Zou";
            test1.repo = "../../../reposetory/";
            test1.success = true;
            test1.testName = "TestDemo 1";
            test1.testDriver = "td1.dll";
            test1.timeStamp = DateTime.Now;
            test1.testCode = new List<string>();
            test1.testCode.Add("tc1.dll");
            test1.testCode.Add("tc2.dll");
            p.enQ(test1);
            test2.author = "Jiayi Zou";
            test2.repo = "../../../reposetory/";
            test2.success = true;
            test2.testName = "TestDemo 2";
            test2.testDriver = "td2.dll";
            test2.timeStamp = new DateTime(2016,11,20,09,00,00);
            test2.testCode = new List<string>();
            test2.testCode.Add("tc1.dll");
            test2.testCode.Add("tc2.dll");
            p.enQ(test2);
            tl.loadTest(p, log);
            log.show();
        }
    }
#endif
}
