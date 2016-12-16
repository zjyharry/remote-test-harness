//Author: Jiayi Zou
//this is test driver 1
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace td1
{
    using TestHarness;
    public class td1 : ITestInterface
    {
            private tc.tc1 tc1;
            private tc.tc2 tc2;
        public td1()
        {
            tc1 = new tc.tc1();
            tc2 = new tc.tc2();
        }
        public string getLog() { return "---------------this is demo for requirement 4-------------\r\n getLog() returns:this is testdriver1"; }
        public static ITestInterface create()
        {
            return new td1();
        }
        int i = 0;
        public bool Test()//make sure the test order is in the same order in the XML file
        {
            if (i == 0)
            {
                i++;
                return tc1.result();//get result from the test code
            }
            
            
            return tc2.result();
        }
#if (TEST_TD1)
        static void Main(string[] args)
        {
            td1 td = new td1();
            bool a = td.Test();
            bool b = td.Test();
        }
#endif
    }
}
