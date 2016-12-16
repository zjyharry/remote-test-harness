//Author: Jiayi Zou
//this is the Interface we use in project 2
//will be expanded in project 4
//all rights reserved
//version 1.0
//Jiayi Zou 10/7/16
//version 2.0 11/22/16
//modify:
//add support to project 4
//public interface:
//ItestInterface: a interface which will can help test harness run the test
//IstringCommunicator: a interface required by the string service
//IfileService: a interface required by the file service
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;
namespace TestHarness
{
    public interface ITestInterface
    {
        bool Test();
        string getLog();
    }
    [ServiceContract]
    public interface IStringCommunicator
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(string msg);
        // used only locally so not exposed as service method
        string GetMessage();
    }
    [ServiceContract(Namespace = "TestHarness")]
    public interface IFileService
    {
        [OperationContract]
        bool OpenFileForWrite(string name);

        [OperationContract]
        bool WriteFileBlock(byte[] block);

        [OperationContract]
        bool CloseFile();
    }
}
