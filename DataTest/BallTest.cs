using Data;

namespace DataTest
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(0.0, 0.0));
            IVector currentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); currentPosition = position; numberOfCallBackCalled++; };
            newInstance.Move(new Vector(0.0, 0.0));
            Assert.AreEqual(initialPosition.x, currentPosition.x);
            Assert.AreEqual(initialPosition.y, currentPosition.y);

        }
    }
}