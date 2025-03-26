using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTest
{
    [TestClass]
    public class BusinessLogicAbstractAPIUnitTest
    {
        [TestMethod]
        public void BusinessLogicConstructorTestMethod()
        {
            LogicAbstractAPI instance1 = LogicAbstractAPI.GetBusinessLogicLayer();
            LogicAbstractAPI instance2 = LogicAbstractAPI.GetBusinessLogicLayer();
            Assert.AreSame(instance1, instance2);
            instance1.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => instance2.Dispose());
        }

        [TestMethod]
        public void GetDimensionsTestMethod()
        {
            Assert.AreEqual<Dimensions>(new(10.0, 10.0, 10.0), LogicAbstractAPI.GetDimensions);
        }
    }
}