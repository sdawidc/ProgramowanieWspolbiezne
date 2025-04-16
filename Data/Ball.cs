
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
    public IVector Position { get; set; }
    #endregion IBall

    #region private


        private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(IVector delta)
    {
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;

            Position = new Vector(newX, newY);

            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}