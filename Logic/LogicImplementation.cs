

using System.Diagnostics;
using System.Threading;
using UnderneathLayerAPI = Data.DataAbstractAPI;

namespace Logic;

internal class LogicImplementation : LogicAbstractAPI
{
#region ctor

public LogicImplementation() : this(null)
{ }

internal LogicImplementation(UnderneathLayerAPI? underneathLayer)
{
  layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
}

#endregion ctor

#region BusinessLogicAbstractAPI

public override void Dispose()
{
  if (Disposed)
    throw new ObjectDisposedException(nameof(LogicImplementation));
        movementTimer?.Dispose();
        layerBellow.Dispose();
  Disposed = true;
}

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
        if (Disposed)
            throw new ObjectDisposedException(nameof(LogicImplementation));
        if (upperLayerHandler == null)
            throw new ArgumentNullException(nameof(upperLayerHandler));
        layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall)));
        movementTimer = new Timer(MoveBalls, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
    }

    public override void MoveBalls(object? x)
    {

        if (Disposed)
            throw new ObjectDisposedException(nameof(LogicImplementation));

        double minX = 0;
        double minY = 0;
        double maxX = 392;
        double maxY = 412;

        for (int i = 0; i < layerBellow.GetBallsListSize(); i++)
        {
            double radius = layerBellow.GetBallRadius(i);
            double diameter = radius * 2;

            Data.IVector velocity = layerBellow.GetBallVelocity(i);
            Data.IVector currentPos = layerBellow.GetBallPosition(i);

            double newX = currentPos.x + velocity.x;
            double newY = currentPos.y + velocity.y;

            double newVelocityX = velocity.x;
            double newVelocityY = velocity.y;

            // Odbicie od lewej/prawej ściany (uwzględniając lewy górny róg jako anchor)
            if (newX < minX || newX + diameter > maxX)
            {
                newVelocityX *= -1;
                newX = Math.Clamp(newX, minX, maxX - diameter);
            }

            // Odbicie od góry/dolnej ściany
            if (newY < minY || newY + diameter > maxY)
            {
                newVelocityY *= -1;
                newY = Math.Clamp(newY, minY, maxY - diameter);
            }

            layerBellow.SetBallVelocity(i, new Vector(newVelocityX, newVelocityY));
            layerBellow.MoveBall(i, new Vector(newX - currentPos.x, newY - currentPos.y));
        }
    }
    #endregion BusinessLogicAbstractAPI

    #region private

    private Timer? movementTimer;
    private const double frameInterval = 16; // ok. 60 FPS

    private bool Disposed = false;

private readonly UnderneathLayerAPI layerBellow;

#endregion private

#region TestingInfrastructure

[Conditional("DEBUG")]
internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
{
  returnInstanceDisposed(Disposed);
}

#endregion TestingInfrastructure
}