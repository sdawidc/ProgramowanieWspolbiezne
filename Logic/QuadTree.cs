using Data;
using System;
using System.Collections.Generic;

namespace Logic
{
    public class Quadtree
    {
        private const int MaxObjects = 10; //liczba obiektow zanim nastapi przedzial
        private const int MaxLevels = 5; //maksymalna glebokosc drzewa (rekurencji)

        private int level; //glebokosc rekurencjyna
        private List<TempBall> objects; //kulki na tym poziomie
        private double x, y, width, height; // wymiary wezla
        private Quadtree[] nodes; // 4 poddrzewa

        public Quadtree(int level, double x, double y, double width, double height)
        {
            this.level = level;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            objects = new List<TempBall>();
            nodes = new Quadtree[4];
        }

        // Tworzy 4 poddrzewa (dzielenie na ćwiartki)
        private void Split()
        {
            double subWidth = width / 2;
            double subHeight = height / 2;
            double xOffset = x;
            double yOffset = y;

            nodes[0] = new Quadtree(level + 1, xOffset + subWidth, yOffset, subWidth, subHeight);
            nodes[1] = new Quadtree(level + 1, xOffset, yOffset, subWidth, subHeight);
            nodes[2] = new Quadtree(level + 1, xOffset, yOffset + subHeight, subWidth, subHeight);
            nodes[3] = new Quadtree(level + 1, xOffset + subWidth, yOffset + subHeight, subWidth, subHeight);
        }


        // Zwraca indeks ćwiartki, do której należy obiekt (lub -1, jeśli się nie mieści w jednej ćwiartce)
        private int GetIndex(TempBall ball)
        {
            int index = -1;
            double verticalMidpoint = x + width / 2;
            double horizontalMidpoint = y + height / 2;

            bool topQuadrant = (ball.Position.y < horizontalMidpoint && ball.Position.y + ball.Radius < horizontalMidpoint);
            bool bottomQuadrant = (ball.Position.y > horizontalMidpoint);

            if (ball.Position.x < verticalMidpoint && ball.Position.x + ball.Radius < verticalMidpoint)
            {
                if (topQuadrant)
                    index = 1; // Gora lewo
                else if (bottomQuadrant)
                    index = 2; // doł lewo
            }
            else if (ball.Position.x > verticalMidpoint)
            {
                if (topQuadrant)
                    index = 0; // gora prawo
                else if (bottomQuadrant)
                    index = 3; // dol prawo
            }

            return index;
        }


        // Wstawia kulkę do drzewa
        public void Insert(TempBall ball)
        {

            // Jesli są juz poddrzewa, spróbuj wstawić do któregoś z nich
            if (nodes[0] != null)
            {
                int index = GetIndex(ball);
                if (index != -1)
                {
                    nodes[index].Insert(ball);
                    return;
                }
            }

            objects.Add(ball);

            if (objects.Count > MaxObjects && level < MaxLevels)
            {
                if (nodes[0] == null)
                    Split();

                int i = 0;
                while (i < objects.Count)
                {
                    int index = GetIndex(objects[i]);
                    if (index != -1)
                    {
                        var obj = objects[i];
                        objects.RemoveAt(i); // usuwamy z bieżącego poziomu
                        nodes[index].Insert(obj); // wstawiamy niżej
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        // pobiera wszystkie kulki w pobliżu danej kulki (z tej samej ćwiartki + lokalnej listy)
        private List<TempBall> Retrieve(List<TempBall> returnObjects, TempBall ball)
        {
            int index = GetIndex(ball);
            if (index != -1 && nodes[0] != null)
            {
                nodes[index].Retrieve(returnObjects, ball);
            }

            // Dodaj lokalne kulki (które się tu nie dały przypisać do jednej ćwiartki)
            returnObjects.AddRange(objects);
            return returnObjects;
        }


        // public api - pobiera wszystkie kulki w pobliżu (do sprawdzenia kolizji)
        public List<TempBall> GetNearbyBalls(TempBall ball)
        {
            List<TempBall> nearbyBalls = new List<TempBall>();
            return Retrieve(nearbyBalls, ball);
        }
    }
    /// <summary>
    /// Tymczasowa klasa reprezentująca kulkę tylko do logiki kolizji
    /// </summary>
    public class TempBall
    {
        public TempBall(IVector position, IVector velocity, double radius, double weight)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
            Weight = weight;
        }

        public IVector Position { get; set; }
        public IVector Velocity { get; set; }
        public double Radius { get; }
        public double Weight { get; }
    }
}
