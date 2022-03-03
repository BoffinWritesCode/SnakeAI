using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using SnakeAI.AI;

namespace SnakeAI.GameStuff
{
    public class Snake
    {
        public const int BOARD_SIZE = 25;
        public const int TILE_SIZE = 18;
        public const int WITH_SPACING = 20;

        public int perMove = 4;

        private bool _player;
        private bool _alive;
        private NeuralNet _brain;

        private Point _velocity;
        private Point _head;
        private List<Point> _body;

        private int _seed;
        private Random _random;
        private Point _food;
        private int _count;

        private int _lifeLeft;
        public int lifeTime;
        private List<Point> _tilesVisited;
        private int _timesVisitingOldTiles;

        public bool Alive { get => _alive; }
        public int Score { get; private set; }

        private object lockMe = new object();

        public Snake(bool playerControlled, int? seed = null)
        {
            _head = new Point(12, 12);
            _velocity = new Point(0, -1);

            _body = new List<Point>();
            _body.Add(new Point(12, 13));
            _body.Add(new Point(12, 14));

            //_seed = seed.HasValue ? seed.Value : Environment.TickCount;
            _seed = seed.HasValue ? seed.Value : SnakeAI.globalRandom.Next();
            _random = new Random(_seed);
            _tilesVisited = new List<Point>();

            SpawnNewFood();

            _player = playerControlled;
            _alive = true;

            Score = 2;

            if (!_player)
            {
                _lifeLeft = 150;
                //_brain = new NeuralNet(24, 16, 4, 2);
                _brain = new NeuralNet(18, 50, 4, 5);
            }
        }

        public Snake(Snake snake) : this(snake._player, snake._seed) { }
        
        public long CalculateFitness()
        {
            /*
            if (Score < 10)
            {
                return Math.Max((long)((lifeTime * lifeTime * Math.Pow(2, Score)) - _timesVisitingOldTiles * 2000), 0);
            }
            return (long)(lifeTime * lifeTime * Math.Pow(2, 10)) * (Score - 9);*/
            int effectiveLifetime = Math.Min(lifeTime, 600);
            if (Score < 10)
            {
                return Math.Max((long)((effectiveLifetime * effectiveLifetime * Math.Pow(2, Score)) /*- _timesVisitingOldTiles * 2000*/), 0);
            }
            return (long)(effectiveLifetime * effectiveLifetime * Math.Pow(2, 10)) * (Score - 9);
        }

        public bool Update()
        {
            if (!_alive) return false;

            Control();

            _count++;

            if (_count >= perMove)
            {
                lifeTime++;
                _lifeLeft--;
                if (!_player && _lifeLeft <= 0)
                {
                    return _alive = false;
                }

                _count = 0;
                return Move();
            }

            return _alive;
        }

        private void SpawnNewFood()
        {
            Point _newFood;
            do
            {
                _newFood = new Point(_random.Next(BOARD_SIZE), _random.Next(BOARD_SIZE));
            }
            while (BodyCollides(_newFood));

            _food = _newFood;
        }

        private bool BodyCollides(Point point)
        {
            lock (lockMe)
            {
                foreach (Point p in _body)
                {
                    if (p.Equals(point))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Snake CreateChild(Snake snake)
        {
            Snake child = new Snake(_player, _seed);
            //Snake child = new Snake(_player);
            child._brain = _brain.CrossWith(snake._brain);
            return child;
        }

        private void Control()
        {
            Point beforeChange = _velocity;

            if (_player)
            {
                if (InputManager.KeyDown(Keys.W))
                {
                    _velocity.X = 0;
                    _velocity.Y = -1;
                }
                if (InputManager.KeyDown(Keys.A))
                {
                    _velocity.X = -1;
                    _velocity.Y = 0;
                }
                if (InputManager.KeyDown(Keys.S))
                {
                    _velocity.X = 0;
                    _velocity.Y = 1;
                }
                if (InputManager.KeyDown(Keys.D))
                {
                    _velocity.X = 1;
                    _velocity.Y = 0;
                }
            }
            else
            {
                //float[] brainOut = _brain.Output(AILook());
                float[] brainOut = _brain.Output(AILook2());
                int highestIndex = 0;
                float highest = -100;
                for (int x = 0; x < brainOut.Length; x++)
                {
                    if (brainOut[x] > highest)
                    {
                        highestIndex = x;
                        highest = brainOut[x];
                    }
                }

                switch(highestIndex)
                {
                    case 0:
                        _velocity.X = 0;
                        _velocity.Y = -1;
                        break;
                    case 1:
                        _velocity.X = -1;
                        _velocity.Y = 0;
                        break;
                    case 2:
                        _velocity.X = 0;
                        _velocity.Y = 1;
                        break;
                    case 3:
                        _velocity.X = 1;
                        _velocity.Y = 0;
                        break;
                }
            }

            if (BodyCollides(_head + _velocity))
            {
                _velocity = beforeChange;
            }
        }

        private float[] AILook()
        {
            float[] array = new float[24];

            LookInDir(1, 0, 0, ref array);
            LookInDir(0, 1, 3, ref array);
            LookInDir(-1, 0, 6, ref array);
            LookInDir(0, -1, 9, ref array);

            LookInDir(1, 1, 12, ref array);
            LookInDir(1, -1, 15, ref array);
            LookInDir(-1, 1, 18, ref array);
            LookInDir(-1, -1, 21, ref array);

            return array;
        }

        private float[] AILook2()
        {
            float[] array = new float[18];

            LookInDir2(1, 0, 0, ref array);
            LookInDir2(0, 1, 2, ref array);
            LookInDir2(-1, 0, 4, ref array);
            LookInDir2(0, -1, 6, ref array);

            LookInDir2(1, 1, 8, ref array);
            LookInDir2(1, -1, 10, ref array);
            LookInDir2(-1, 1, 12, ref array);
            LookInDir2(-1, -1, 14, ref array);

            int deltaX = Math.Abs(_head.X - _food.X);
            int deltaY = Math.Abs(_head.Y - _food.Y);

            array[16] = 1f / (deltaX + 1);
            array[17] = 1f / (deltaY + 1);

            return array;
        }

        private void LookInDir(int delX, int delY, int startIndex, ref float[] array)
        {
            Point vel = new Point(delX, delY);
            Point test = _head + vel;
            float distance = 1f;
            while(!OutOfBounds(test))
            {
                if (array[startIndex] <= 0 && _food.Equals(test))
                {
                    array[startIndex] = 1f / distance;
                }
                if (array[startIndex + 1] <= 0 && BodyCollides(test))
                {
                    //array[startIndex + 1] = 1f / Vector2.Distance(_head.ToVector2(), test.ToVector2()); //0 - 1
                    array[startIndex + 1] = 1f / distance;
                }
                test += vel;
                distance++;
            }
            test -= vel;
            array[startIndex + 2] = 1f / distance;
        }

        private void LookInDir2(int delX, int delY, int startIndex, ref float[] array)
        {
            Point vel = new Point(delX, delY);
            Point test = _head + vel;
            float distance = 1f;
            while (!OutOfBounds(test))
            {
                if (array[startIndex] <= 0 && BodyCollides(test))
                {
                    array[startIndex] = 1f / distance;
                }
                test += vel;
                distance++;
            }
            test -= vel;
            array[startIndex + 1] = 1f / distance;
        }

        private bool Move()
        {
            Point newHead = _head + _velocity;
            if (OutOfBounds(newHead))
            {
                //die
                return _alive = false;
            }

            bool extend = false;
            if (newHead.Equals(_food))
            {
                Score++;
                if (!_player)
                {
                    _lifeLeft += 100;
                    if (_lifeLeft > 500) _lifeLeft = 500;
                }
                SpawnNewFood();
                extend = true;
            }

            ShiftBody(newHead, extend);

            if (BodyCollides(newHead))
            {
                //kill
                return _alive = false;
            }

            if (_tilesVisited.Contains(newHead))
            {
                _timesVisitingOldTiles++;
            }

            _tilesVisited.Insert(0, _head);
            if (_tilesVisited.Count > Score + 7)
            {
                _tilesVisited.RemoveAt(_tilesVisited.Count - 1);
            }

            return true;
        }

        public void Mutate()
        {
            _brain.Mutate();
        }

        public Snake Clone()
        {
            Snake snake = new Snake(_player, _seed);
            snake._brain = _brain.Clone();
            return snake;
        }

        private bool OutOfBounds(Point point)
        {
            return point.X < 0 || point.Y < 0 || point.X >= BOARD_SIZE || point.Y >= BOARD_SIZE;
        }

        private void ShiftBody(Point newHead, bool addTail)
        {
            lock (lockMe)
            {
                //remove the last tail part
                if (!addTail)
                {
                    _body.RemoveAt(_body.Count - 1);
                }

                //insert the old head as a new body part
                _body.Insert(0, _head);

                //set the new head
                _head = newHead;
            }
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, bool drawWalls = true)
        {
            lock (lockMe)
            {
                if (drawWalls)
                {
                    spriteBatch.Draw(SnakeAI.pixel, new Rectangle(-4 + x, -4 + y, BOARD_SIZE * WITH_SPACING + 4, 4), Color.Gray);
                    spriteBatch.Draw(SnakeAI.pixel, new Rectangle(-4 + x, BOARD_SIZE * WITH_SPACING + y, BOARD_SIZE * WITH_SPACING + 4, 4), Color.Gray);

                    spriteBatch.Draw(SnakeAI.pixel, new Rectangle(-4 + x, -4 + y, 4, BOARD_SIZE * WITH_SPACING + 4), Color.Gray);
                    spriteBatch.Draw(SnakeAI.pixel, new Rectangle(BOARD_SIZE * WITH_SPACING + x, -4 + y, 4, BOARD_SIZE * WITH_SPACING + 8), Color.Gray);

                    spriteBatch.DrawString(SnakeAI.font, $"SCORE: {Score} | Lifetime: {lifeTime} | Life left: {_lifeLeft} | Old Tiles: {_timesVisitingOldTiles}\nHIGH SCORE: {SnakeAI.HIGH_SCORE}", new Vector2(x, y + BOARD_SIZE * WITH_SPACING + 8), Color.White);
                }

                spriteBatch.Draw(SnakeAI.pixel, new Rectangle(_food.X * WITH_SPACING + x, _food.Y * WITH_SPACING + y, TILE_SIZE, TILE_SIZE), Color.Red);
                spriteBatch.Draw(SnakeAI.pixel, new Rectangle(_head.X * WITH_SPACING + x, _head.Y * WITH_SPACING + y, TILE_SIZE, TILE_SIZE), Color.White);

                foreach (Point p in _body)
                {
                    spriteBatch.Draw(SnakeAI.pixel, new Rectangle(p.X * WITH_SPACING + x, p.Y * WITH_SPACING + y, TILE_SIZE, TILE_SIZE), Color.White);
                }

                foreach (Point p in _tilesVisited)
                {
                    spriteBatch.DrawRectangleDebug(new Rectangle(p.X * WITH_SPACING + x, p.Y * WITH_SPACING + y, TILE_SIZE, TILE_SIZE), Color.Green, 2f, SnakeAI.pixel);
                }
            }
        }

        public void DrawBrain(SpriteBatch spriteBatch, int x, int y, float diameter, float seperation, float spacing, bool doInputs = true)
        {
            if (_player) return;

            if (doInputs)
            {
                //_brain.Show(spriteBatch, x, y, diameter, seperation, spacing, AILook());
                _brain.Show(spriteBatch, x, y, diameter, seperation, spacing, AILook2());
            }
            else
            {
                _brain.Show(spriteBatch, x, y, diameter, seperation, spacing);
            }
        }
    }
}
