﻿
namespace Logic
{
  public abstract class LogicAbstractAPI : IDisposable
  {
    #region Layer Factory

    public static LogicAbstractAPI GetBusinessLogicLayer()
    {
      return modelInstance.Value;
    }

    #endregion Layer Factory

    #region Layer API

    public static readonly Dimensions GetDimensions = new(10.0, 10.0, 10.0);

    public abstract void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler);

    public abstract void MoveBalls(object? x);
    #region IDisposable

    public abstract void Dispose();

    #endregion IDisposable

    #endregion Layer API

    #region private

    private static Lazy<LogicAbstractAPI> modelInstance = new Lazy<LogicAbstractAPI>(() => new LogicImplementationCollisions());

        #endregion private
    }
  /// <summary>
  /// Immutable type representing table dimensions
  /// </summary>
  /// <param name="BallDimension"></param>
  /// <param name="TableHeight"></param>
  /// <param name="TableWidth"></param>
  /// <remarks>
  /// Must be abstract
  /// </remarks>
  public record Dimensions(double BallDimension, double TableHeight, double TableWidth);

  public interface IPosition
  {
    double x { get; init; }
    double y { get; init; }
  }

  public interface IBall 
  {
    event EventHandler<IPosition> NewPositionNotification;
        public Data.IVector Position { get; set; }
        public Data.IVector Velocity { get; set; }
        public double Radius { get; set; }
        public double Weight { get; }

    }
}