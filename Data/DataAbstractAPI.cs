
using System.Numerics;

namespace Data
{
  public abstract class DataAbstractAPI : IDisposable
  {
    #region Layer Factory

    public static DataAbstractAPI GetDataLayer()
    {
      return modelInstance.Value;
    }

    #endregion Layer Factory

    #region public API

    public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

    public abstract int GetBallsListSize();

        public abstract IVector GetBallPosition(int index);
        public abstract IVector GetBallVelocity(int index);
        public abstract void SetBallVelocity(int index, IVector newVelocity);

        public abstract double GetBallRadius(int index);
        public abstract double GetBallWeight(int index);

        public abstract void MoveBall(int ballNumber,IVector vector);

        public abstract void LogToFile(string fileName, string log);

        public abstract int getBallIndex(IBall ball);


    #endregion public API

    #region IDisposable

    public abstract void Dispose();

    #endregion IDisposable

    #region private

    private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

    #endregion private
  }

  public interface IVector
  {
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    double x { get; init; }

    /// <summary>
    /// The y component of the vector.
    /// </summary>
    double y { get; init; }
  }

  public interface IBall
  {
    event EventHandler<IVector> NewPositionNotification;

    IVector Velocity { get; set; }
    IVector Position { get; set; }

    double Radius { get; set; }

    double Weight { get; }
    }
}