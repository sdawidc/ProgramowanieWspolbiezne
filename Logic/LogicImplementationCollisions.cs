using Data;
using System;
using System.Collections.Concurrent;
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

        private const float ballSpeed=50f;
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

                    var tempBall = new TempBall(i, pos, vel, radius, weight);
                    tempBalls.Add(tempBall);
                    quadtree.Insert(tempBall);
                }

                List<BallUpdate> updates = new();

                var loggedPairs = new ConcurrentDictionary<(int, int), bool>();

                // Kolizje i ruch przetwarzane równolegle
                Parallel.For(0, tempBalls.Count, i =>
                {
                    var ball = tempBalls[i];
                    var nearbyBalls = quadtree.GetNearbyBalls(ball);

                    foreach (var other in nearbyBalls)
                    {
                        if (ball != other && CheckIfBallsCollide(ball, other))
                        {
                            var pair = (Math.Min(ball.Index, other.Index), Math.Max(ball.Index, other.Index)); //krotka z indeksami piłek kolidujących w danej klatce
                            lock (GetLock(ball, other))
                            {
                                HandleBallCollision(ball, other);

                                if (!loggedPairs.ContainsKey(pair))
                                {
                                    loggedPairs[pair] = true;

                                    IVector vec = new Vector(
                                        (ball.Position.x + other.Position.x) / 2,
                                        (ball.Position.y + other.Position.y) / 2);

                                    layerBellow.LogData(vec);
                                }
                            }
                        }
                    }

                    double radius = ball.Radius;
                    double diameter = radius * 2;

                    double newX = ball.Position.x + ball.Velocity.x  * ballSpeed * deltaTime;
                    double newY = ball.Position.y + ball.Velocity.y  * ballSpeed * deltaTime;

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

                    lock (updates)
                    {
                        updates.Add(new BallUpdate
                        {
                            index = i,
                            newVelocity = updatedVelocity,
                            movement = movement
                        });
                    }


                });
                lock (layerBellow)
                {
                    foreach (BallUpdate update in updates)
                    {
                        layerBellow.SetBallVelocity(update.index, update.newVelocity);
                        layerBellow.MoveBall(update.index, update.movement);
                    }
                }

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
            double dist = Math.Sqrt(distSq);

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

            double overlap = (a.Radius + b.Radius) - dist;
            const double offset = 1.03f;

            if (overlap > 0)
            {
                double totalWeight = a.Weight + b.Weight;
                double nx = dx / dist;
                double ny = dy / dist;

                double displacementA = overlap * (b.Weight / totalWeight) * offset;
                double displacementB = overlap * (a.Weight / totalWeight) * offset;

                a.Position = new Vector(
                    a.Position.x + nx * displacementA,
                    a.Position.y + ny * displacementA
                );

                b.Position = new Vector(
                    b.Position.x - nx * displacementB,
                    b.Position.y - ny * displacementB
                );
            }
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
