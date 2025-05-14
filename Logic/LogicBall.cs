
using Data;

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
        public IVector Position
        {
            get => ballData.Position;
            set => ballData.Position = new Vector(value.x, value.y);
        }

        public IVector Velocity
        {
            get => ballData.Velocity;
            set => ballData.Velocity = new Vector(value.x, value.y);
        }

        public double Radius
        {
            get => ballData.Radius;
            set => ballData.Radius = value;
        }

        public double Weight
        {
            get => ballData.Weight;
        }


        #endregion IBallc

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private
    }
}