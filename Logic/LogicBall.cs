
namespace Logic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball)
        {
            ball.NewPositionNotification += RaisePositionChangeEvent;
            ballData = ball;
        }

        private readonly Data.IBall ballData;

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;
        public double Radius => ballData.Radius;


        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private
    }
}