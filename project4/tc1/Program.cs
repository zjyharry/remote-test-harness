//Author: Jiayi Zou
//this is the test code 1 (tc1)
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
    public class tc1
    {
        public void annunciator(string msg)
        {
            Console.Write("\n  Production Code: {0}", msg);
        }
        public bool result()
        {
            return true;
        }
#if (TEST_TC1)
        static void Main(string[] args)
        {
            tc1 ctt = new tc1();
            ctt.annunciator("this is code-to-test 1\r\n");
        }
#endif
    }
}
