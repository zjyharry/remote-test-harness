//Author: Jiayi Zou
//this is the executive program just for TA's to grade, will be removed in
//project 4
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestHarness;
using SWTools;
using System.Diagnostics;

namespace executive
{
    class Program
    {
        static void Main(string[] args)
        {
            Process.Start("Repository.exe");
            Process.Start("TestHarness.exe");
            Process.Start("Client.exe");
            Process.Start("Client.exe");
        }
    }
}
