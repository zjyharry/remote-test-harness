//Author: Jiayi Zou
//this is the test code 2
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tc
{
    public class tc2
    {
        public void annunciator(string msg)
        {
            Console.Write("\n  Production Code: {0}", msg);
        }
        public bool result()
        {
            int a, c, b;
            a = 1;
            b = 0;
            c = a / b; //must error here
            return false;
        }
#if(TEST_TC2)
        static void Main(string[] args)
        {
            tc2 tcc = new tc2();
            tcc.annunciator("this is code-to-test-2 \r\n");
        }
#endif
    }
}
