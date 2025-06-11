
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation() { }

        private Logger logger;

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();

            logger = new Logger("Logs");

            double VELOCITYFACTOR = 4.0f;

            BallsList.Clear();

            logger.Log("creation.txt", $"--- NEW SIMULATION START --- {DateTime.Now:yyyy-MM-dd HH:mm:ss}");


            for (int i = 0; i < numberOfBalls; i++)
            {

                Vector startingPosition = new Vector(random.Next(100, 300), random.Next(100, 300));
                Vector startingVelocity = new Vector((random.NextDouble()) * VELOCITYFACTOR, (random.NextDouble()) * VELOCITYFACTOR);
                double radius = random.NextDouble()*10+10;
                Ball newBall = new Ball(startingPosition, startingVelocity, radius);


                string logMessage = $"Ball {i + 1} created at position x: {startingPosition.x:F2}, y: {startingPosition.y:F2} " +
                        $"with velocity vx: {startingVelocity.x:F2}, vy: {startingVelocity.y:F2}, radius: {radius:F2}";
                logger.Log("creation.txt", logMessage);



                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);

            }
        
        }

        public override int GetBallsListSize()
        {
            return BallsList.Count;
        }

        public override void MoveBall(int ballNumber, IVector vector)
        {
            BallsList.ElementAt(ballNumber).Move(vector);
        }

        public override IVector GetBallPosition(int index)
        {
            return BallsList.ElementAt(index).Position; 
        }

        public override double GetBallRadius(int index)
        {
            return BallsList.ElementAt(index).Radius;
        }

        public override IVector GetBallVelocity(int index)
        {
            return BallsList.ElementAt(index).Velocity;
        }

        public override void SetBallVelocity(int index, IVector newVelocity)
        {
            BallsList.ElementAt(index).Velocity = newVelocity;
        }

        public override double GetBallWeight(int index)
        {
            return BallsList.ElementAt(index).Weight;
        }

        public override void LogToFile(string fileName, string log)
        {
            logger.Log(fileName, log);
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));

            if (disposing)
            {
                BallsList.Clear();
            }
            Disposed = true;
        }

        public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
    private bool Disposed = false;

        private List<Ball> BallsList = new List<Ball>();
        private void Move(Ball ball, Vector vec)
        {
            ball.Move(vec);
        }
    

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}