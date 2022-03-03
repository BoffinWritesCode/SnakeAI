using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using SnakeAI.GameStuff;

namespace SnakeAI.AI
{
    public class Population
    {
        private bool _autoProgress;

        private Snake[] _snakes;
        private int _count;

        private bool _running;
        private bool _breedingKids;
        private int _generation;

        private long _avgFitness;
        private long _bestFitness;
        private long _totalFitness;
        private long _top10PercentFitness;

        public int SnakesLeft { get; private set; }
        public float Progress { get => (_count - SnakesLeft) / (float)_count; }
        public Snake BestSnake { get; private set; }
        public bool Running { get => _running; }

        List<Tuple<long, long, int, int>> generationScores;

        public Population(int count, int seed)
        {
            _count = count;
            _snakes = new Snake[_count];

            generationScores = new List<Tuple<long, long, int, int>>();

            for (int i = 0; i < _count; i++)
            {
                _snakes[i] = new Snake(false, seed);
                //_snakes[i] = new Snake(false);
                _snakes[i].perMove = 1;
            }
        }

        public void RunGeneration()
        {
            Run();
            //Task.Run(Run);
        }

        public void Update()
        {
            if (InputManager.KeyJustPressed(Keys.Space))
            {
                _autoProgress = !_autoProgress;
            }

            bool doAuto = (InputManager.KeyJustPressed(Keys.G) || (_autoProgress && SnakeAI.displaySnake != null && SnakeAI.displaySnake.lifeTime > 12 && generationScores.Count > 2 && generationScores.Last().Item1 == generationScores[generationScores.Count - 2].Item1));

            if (!_running && SnakeAI.displaySnake != null && !SnakeAI.displaySnake.Alive)
            {
                RunGeneration();
            }
            else if (!_running && doAuto)
            {
                if (SnakeAI.displaySnake != null)
                    SnakeAI.displaySnake.perMove = 99999999;

                RunGeneration();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_running)
            {
                if (_generation == 0)
                {
                    spriteBatch.DrawString(SnakeAI.font, "not running...", new Vector2(4), Color.White);
                    spriteBatch.DrawString(SnakeAI.font, "press [g] to run generation 1.", new Vector2(4, 24), Color.White);
                }
                else
                {
                    int index = 0;
                    for (int i = generationScores.Count - 1; i >= 0; i--)
                    {
                        string text = $"GEN: {i+1} | BEST: {generationScores[i].Item1} | AVG.: {generationScores[i].Item2} | SCORE: {generationScores[i].Item3} | HIGHEST: {generationScores[i].Item4}";
                        Vector2 size = SnakeAI.font.MeasureString(text);
                        int y = SnakeAI.SCREEN_SIZE.Y - 24 - 20 * index;
                        if (y < -40) break;
                        spriteBatch.DrawString(SnakeAI.font, text, new Vector2(SnakeAI.SCREEN_SIZE.X - 4 - size.X, y), Color.White);
                        index++;
                    }
                    spriteBatch.DrawString(SnakeAI.font, $"press [g] to run generation {_generation+1}", new Vector2(4), Color.White);
                    spriteBatch.DrawString(SnakeAI.font, $"press [space] to {(_autoProgress ? "stop auto progressing" : "auto progress")}", new Vector2(4, 24), Color.White);
                }
            }
            else
            {
                if (_breedingKids)
                {
                    spriteBatch.DrawStringAroundCenter("Breeding new population...", SnakeAI.font, new Vector2(SnakeAI.SCREEN_SIZE.X / 2f, SnakeAI.SCREEN_SIZE.Y / 2f), Color.White);
                }
                else
                {
                    for (int i = 0; i < _count; i++)
                    {
                        //_snakes[i].Draw(spriteBatch, 444, 54, i == 0);
                    }
                    spriteBatch.DrawStringAroundCenter($"Progress: {_count - SnakesLeft}/{_count} ({Progress * 100f}%)", SnakeAI.font, new Vector2(SnakeAI.SCREEN_SIZE.X / 2f, SnakeAI.SCREEN_SIZE.Y / 2f), Color.White);
                }
            }
        }

        private void Run()
        {
            _running = true;

            if (_generation > 0)
            {
                BreedKids();
            }

            SnakesLeft = _count;
            while (SnakesLeft > 0)
            {
                for (int i = 0; i < _count; i++)
                {
                    bool aliveBefore = _snakes[i].Alive;
                    if (aliveBefore && !_snakes[i].Update())
                    {
                        SnakesLeft--;
                    }
                }
            }

            List<Snake> orderedDesc = _snakes.OrderByDescending(snake => snake.CalculateFitness()).ToList();
            Snake highScore = _snakes.OrderByDescending(snake => snake.Score).First();
            BestSnake = orderedDesc[0];
            if (BestSnake.Score > SnakeAI.HIGH_SCORE)
            {
                SnakeAI.HIGH_SCORE = BestSnake.Score;
            }

            _totalFitness = 0; 
            for (int i = 0; i < _count; i++)
            {
                long amt = _snakes[i].CalculateFitness();
                if (i < _count * 0.1)
                {
                    _top10PercentFitness += amt;
                }
                _totalFitness += amt;
            }
            
            _bestFitness = BestSnake.CalculateFitness();
            _avgFitness = (long)(_totalFitness / (float)_count);

            generationScores.Add(new Tuple<long, long, int, int>(_bestFitness, _avgFitness, BestSnake.Score, highScore.Score));
            _generation++;

            SnakeAI.displaySnake = BestSnake.Clone();
            SnakeAI.displaySnake.perMove = 3;

            _running = false;
        }

        private void BreedKids()
        {
            _breedingKids = true;

            List<Snake> newSnakes = new List<Snake>();
            List<Snake> orderedDesc = _snakes.OrderByDescending(snake => snake.CalculateFitness()).ToList();

            //add the best snake
            Snake clone = orderedDesc[0].Clone();
            clone.perMove = 1;
            newSnakes.Add(clone);

            for (int i = 1; i < _count; i++)
            {
                Snake child = GetParent(orderedDesc).CreateChild(GetParent(orderedDesc));
                child.Mutate();
                child.perMove = 1;
                newSnakes.Add(child);
            }

            _snakes = newSnakes.ToArray();

            _breedingKids = false;
        }

        private Snake GetParent(List<Snake> orderedList)
        {
            //float rand = SnakeAI.globalRandom.NextFloat(_totalFitness);
            float rand = SnakeAI.globalRandom.NextFloat(_top10PercentFitness);
            float sum = 0f;
            for (int i = 0; i < _count; i++)
            {
                sum += orderedList[i].CalculateFitness();
                if (sum > rand)
                {
                    return orderedList[i];
                }
            }
            int pick = SnakeAI.globalRandom.Next((int)(_count * 0.1));
            return orderedList[pick];
        }
    }
}
