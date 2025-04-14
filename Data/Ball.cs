
namespace Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

    private Vector Position;
    
    private const double Diameter = 20;


        private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(Vector delta)
    {
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;

            double minX = 0;
            double minY = 0;
            double maxX = 400;
            double maxY = 420; 

            double effectiveMaxX = maxX - Diameter*1.5 + 2;
            double effectiveMaxY = maxY - Diameter*1.5 + 2;

            if (newX < minX)
            {
                newX = minX;
            }
            else if (newX > effectiveMaxX)
            {
                newX = effectiveMaxX;
            }

            if (newY < minY)
            {
                newY = minY;
            }
            else if (newY > effectiveMaxY)
            {
                newY = effectiveMaxY;
            }

            Position = new Vector(newX, newY);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}