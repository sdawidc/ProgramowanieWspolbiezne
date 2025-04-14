
using System;
using System.Diagnostics;
using System.Threading;

namespace Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
    }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
        
            lock (_ballsListLock)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {

                    Vector startingPosition = new Vector(random.Next(100, 300), random.Next(100, 300));
                    Vector startingVelocity = new Vector((random.NextDouble() - 0.5) * 7.0, (random.NextDouble() - 0.5) * 7.0);
                    Ball newBall = new Ball(startingPosition, startingVelocity);
                    upperLayerHandler(startingPosition, newBall);
                    BallsList.Add(newBall);

                }
            }
    }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));

            if (disposing)
            {
                MoveTimer.Dispose();
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

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = new List<Ball>();

        private readonly object _ballsListLock = new object();
        private void Move(object? x)
        {

            double dt = 0.03;
            double maxSpeed = 8.0;
            double accelerationFactor = 0.6;

            lock (_ballsListLock)
            {

                foreach (Ball ball in BallsList)
                {

                    var currentVelocity = ball.Velocity;
                    double deltaVx = (RandomGenerator.NextDouble() - 0.5) * accelerationFactor;
                    double deltaVy = (RandomGenerator.NextDouble() - 0.5) * accelerationFactor;
                    Vector newVelocity = new Vector(currentVelocity.x + deltaVx, currentVelocity.y + deltaVy);

                    double speed = Math.Sqrt(newVelocity.x * newVelocity.x + newVelocity.y * newVelocity.y);
                    if (speed > maxSpeed)
                    {
                        newVelocity = new Vector(newVelocity.x * maxSpeed / speed, newVelocity.y * maxSpeed / speed);
                    }

                    ball.Velocity = newVelocity;

                    Vector deltaPosition = new Vector(newVelocity.x * dt, newVelocity.y * dt);
                    ball.Move(deltaPosition);
                }
            }
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