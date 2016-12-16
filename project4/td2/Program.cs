//Author: Jiayi Zou
//this is test driver 2
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace td2
{
    using TestHarness;
    public class td2 : ITestInterface
    {
        //private bool b;
        private tc.tc1 tc1;
        private tc.tc2 tc2;
        public td2()
        {
            tc1 = new tc.tc1();
            tc2 = new tc.tc2();
        }
        public static ITestInterface create()
        {
            return new td2();
        }
        int i = 0;
        public string getLog() { return "---------------this is demo for requirement 4-------------\r\n getLog() returns: this is testdriver2"; }
        public bool Test()
        {
            if (i == 0)
            {
                i++;
                return !tc1.result();//get result from test code and return opposite result
            }
            return !tc2.result();
        }
//----------------Test sub----------------------
#if (TEST_TD2)
        static void Main(string[] args)
        {
            td2 td = new td2();
            bool a = td.Test();
            bool b = td.Test();
        }
#endif
    }
}
