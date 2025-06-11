using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnderneathLayerAPI = Data.DataAbstractAPI;

namespace Logic
{
    internal class LogicImplementationCollisions : LogicAbstractAPI
    {
        #region ctor

        private DateTime lastUpdate = DateTime.Now;
        private readonly object timeLock = new();

        private const float ballSpeed=80f;
        public LogicImplementationCollisions() : this(null) { }

        internal LogicImplementationCollisions(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LogicImplementationCollisions));

            movementTimer?.Dispose();
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(LogicImplementationCollisions));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall)));

            movementTimer = new Timer(MoveBalls, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(frameInterval));
        }

        public override void MoveBalls(object? _)
        {
            _ = MoveBallsAsync();
        }

        public async Task MoveBallsAsync()
        {
            if (Disposed)
                return;

            double deltaTime;
            lock (timeLock)
            {
                DateTime now = DateTime.Now;
                deltaTime = (now - lastUpdate).TotalSeconds;
                lastUpdate = now;
            }
            await Task.Run(() =>
            {
                
                var quadtree = new Quadtree(0, 0, 0, 392, 412);
                int count = layerBellow.GetBallsListSize();
                List<TempBall> tempBalls = new(count);

                for (int i = 0; i < count; i++)
                {
                    var pos = layerBellow.GetBallPosition(i);
                    var vel = layerBellow.GetBallVelocity(i);
                    double radius = layerBellow.GetBallRadius(i);
                    double weight = layerBellow.GetBallWeight(i);

                    var tempBall = new TempBall(pos, vel, radius, weight);
                    tempBalls.Add(tempBall);
                    quadtree.Insert(tempBall);
                }

                // Kolizje i ruch przetwarzane równolegle
                Parallel.For(0, tempBalls.Count, i =>
                {
                    var ball = tempBalls[i];
                    var nearbyBalls = quadtree.GetNearbyBalls(ball);

                    foreach (var other in nearbyBalls)
                    {
                        if (ball != other && CheckIfBallsCollide(ball, other) && ball.GetHashCode()<other.GetHashCode())
                        {
                            lock (GetLock(ball, other)) // synchronizacja przy modyfikacji prędkosci
                            {
                                HandleBallCollision(ball, other);
                                layerBellow.LogToFile("collisions.txt","Collision position: x - "+(ball.Position.x+other.Position.x)/2 + " y - "+(ball.Position.y+other.Position.y)/2);
                            }
                        }
                    }

                    double radius = ball.Radius;
                    double diameter = radius * 2;

                    double newX = ball.Position.x + ball.Velocity.x * deltaTime * ballSpeed;
                    double newY = ball.Position.y + ball.Velocity.y * deltaTime * ballSpeed;

                    double newVelX = ball.Velocity.x;
                    double newVelY = ball.Velocity.y;

                    if (newX < 0 || newX + diameter > 392)
                    {
                        newVelX *= -1;
                        newX = Math.Clamp(newX, 0, 392 - diameter);
                    }

                    if (newY < 0 || newY + diameter > 412)
                    {
                        newVelY *= -1;
                        newY = Math.Clamp(newY, 0, 412 - diameter);
                    }

                    var updatedVelocity = new Vector(newVelX, newVelY);
                    var movement = new Vector(newX - ball.Position.x, newY - ball.Position.y);

                    lock (layerBellow)
                    {
                        layerBellow.SetBallVelocity(i, updatedVelocity);
                        layerBellow.MoveBall(i, movement);
                    }
                });
            });
        }

        // wybiera ktora kula bedzie lockowala kolizje
        private static object GetLock(TempBall a, TempBall b)
        {
            return a.GetHashCode() < b.GetHashCode() ? a : b;
        }


        private bool CheckIfBallsCollide(TempBall a, TempBall b)
        {
            double dx = a.Position.x - b.Position.x;
            double dy = a.Position.y - b.Position.y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return dist < (a.Radius + b.Radius);
        }

        private void HandleBallCollision(TempBall a, TempBall b)
        {
            double dx = a.Position.x - b.Position.x;
            double dy = a.Position.y - b.Position.y;
            double distSq = dx * dx + dy * dy;

            if (distSq == 0) return;

            double dvx = a.Velocity.x - b.Velocity.x;
            double dvy = a.Velocity.y - b.Velocity.y;

            double dot = dvx * dx + dvy * dy;
            if (dot >= 0) return;

            double scale = dot / distSq;
            double impulseX = scale * dx;
            double impulseY = scale * dy;

            a.Velocity = new Vector(
                a.Velocity.x - (2 * b.Weight / (a.Weight + b.Weight)) * impulseX,
                a.Velocity.y - (2 * b.Weight / (a.Weight + b.Weight)) * impulseY
            );

            b.Velocity = new Vector(
                b.Velocity.x + (2 * a.Weight / (a.Weight + b.Weight)) * impulseX,
                b.Velocity.y + (2 * a.Weight / (a.Weight + b.Weight)) * impulseY
            );
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private Timer? movementTimer;
        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private const double frameInterval = 16;

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }

    /// <summary>
    /// Tymczasowa klasa reprezentująca kulkę tylko do logiki kolizji
    /// </summary>

}
