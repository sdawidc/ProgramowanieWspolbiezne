using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTest
{
    [TestClass]
    public class PositionUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Random random = new Random();
            double initialX = random.NextDouble();
            double initialY = random.NextDouble();
            IPosition position = new Position(initialX, initialY);
            Assert.AreEqual<double>(initialX, position.x);
            Assert.AreEqual<double>(initialY, position.y);
        }
    }
}