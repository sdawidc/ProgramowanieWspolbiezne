using Data;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic;
namespace LogicTest
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (LogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
            LogicImplementation newInstance = new(dataLayerFixcure);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
            Assert.IsTrue(dataLayerFixcure.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStartFixcure dataLayerFixcure = new();
            using (LogicImplementation newInstance = new(dataLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) => { called++; Assert.IsNotNull(startingPosition); Assert.IsNotNull(ball); });
                Assert.AreEqual<int>(1, called);
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
            }
        }

        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override double GetBallRadius(int index)
            {
                return 5.0;
            }

            public override void LogToFile(string fileName, string log)
            {
                throw new NotImplementedException();
            }

            public override double GetBallWeight(int index)
            {
                return 1.0;
            }

            public override void MoveBall(int ballNumber, IVector vector)
            {
                
            }

            public override IVector GetBallPosition(int index)
            {
                throw new NotImplementedException();
            }

            public override int GetBallsListSize()
            {
                throw new NotImplementedException();
            }

            public override IVector GetBallVelocity(int index)
            {
                throw new NotImplementedException();
            }

            public override void SetBallVelocity(int index, IVector newVelocity)
            {
                throw new NotImplementedException();
            }


            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override IVector GetBallPosition(int index)
            {
                throw new NotImplementedException();
            }
            public override double GetBallRadius(int index)
            {
                return 5.0;
            }

            public override double GetBallWeight(int index)
            {
                return 1.0;
            }
            public override int GetBallsListSize()
            {
                throw new NotImplementedException();
            }

            public override IVector GetBallVelocity(int index)
            {
                throw new NotImplementedException();
            }

            public override void SetBallVelocity(int index, IVector newVelocity)
            {
                throw new NotImplementedException();
            }

            public override void MoveBall(int ballNumber, IVector vector)
            {
                throw new NotImplementedException();
            }

            public override void LogToFile(string fileName, string log)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;

            public override double GetBallRadius(int index)
            {
                return 5.0;
            }

            public override double GetBallWeight(int index)
            {
                return 1.0;
            }

            public override void Dispose()
            { }

            public override void MoveBall(int ballNumber, IVector vector)
            {

            }

            public override IVector GetBallPosition(int index)
            {
                throw new NotImplementedException();
            }

            public override int GetBallsListSize()
            {
                throw new NotImplementedException();
            }

            public override IVector GetBallVelocity(int index)
            {
                throw new NotImplementedException();
            }

            public override void SetBallVelocity(int index, IVector newVelocity)
            {
                throw new NotImplementedException();
            }

            public override void LogToFile(string fileName, string log)
            {
                throw new NotImplementedException();
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
            }

            private record DataVectorFixture : Data.IVector
            {
                public double x { get; init; }
                public double y { get; init; }
            }

            private class DataBallFixture : Data.IBall
            {
                public double Radius { get; set; } = 5.0;
                public double Weight { get; set; } = 1.0;
                public IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
                public IVector Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                public event EventHandler<IVector>? NewPositionNotification = null;
            }
        }

        #endregion testing instrumentation
    }
}