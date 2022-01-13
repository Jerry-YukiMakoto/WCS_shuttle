using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirle.MPLC;
using Mirle.ASRS.Conveyors.Signal;
using Mirle.MPLC.DataType;
using Mirle.ASRS.WCS.Controller;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.PLCDefinitions;

namespace TestProject1;
[TestClass]
public class UnitTest1
{
    [TestClass]
    public class UnitTest2
    {
        int EmptyINReady = 7;
        int CommandId = 1;
        int CommandId1 = 1;
        int actual = 999;
        int A2LV2 = 2;
        int COUNT = 0;

        [TestMethod]
        public void TestMethod1()
        {
            
            if ((EmptyINReady == 8 && CommandId != 0) || EmptyINReady == 8 && CommandId1 != 0)
            {
                 actual = 1;
            }
            else
            {
                 actual = 0;
            }
            Assert.AreEqual(1, actual,"fail");

        }
        [TestMethod]
        public void TestMethod2()
        {
            if (A2LV2 == 0 && COUNT == '0' && CommandId == 0 && CommandId1 == 0)
            {
                actual = 1;
            }
            else
            {
                actual = 0;
            }
            Assert.AreEqual(1, actual, "fail");
        }
    }
}