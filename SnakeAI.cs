using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SnakeAI.GameStuff;
using SnakeAI.AI;

namespace SnakeAI
{
    public class SnakeAI : Game
    {
        public static readonly float MUTATION_RATE = 0.05f;

        public static readonly bool HUMAN_PLAYING = false;
        public static readonly Point SCREEN_SIZE = new Point(1600, 720);

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public static Random globalRandom;

        public static Texture2D pixel;
        public static Texture2D circle;
        public static SpriteFont font;

        public static Snake displaySnake;
        private Population _currentPop;

        public static int HIGH_SCORE = 2;

        public SnakeAI()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREEN_SIZE.X;
            graphics.PreferredBackBufferHeight = SCREEN_SIZE.Y;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            globalRandom = new Random();

            if (HUMAN_PLAYING)
            {
                displaySnake = new Snake(true);
            }
            else
            {
                _currentPop = new Population(4000, 1919);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //create a single pixel texture
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            circle = Content.Load<Texture2D>("circle");
            font = Content.Load<SpriteFont>("font");

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            displaySnake?.Update();
            if (_currentPop != null)
            {
                _currentPop.Update();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _currentPop?.Draw(spriteBatch);
            if (_currentPop == null || !_currentPop.Running)
            {
                displaySnake?.Draw(spriteBatch, 444, 54);
                displaySnake?.DrawBrain(spriteBatch, 24, 48, 8f, 50f, 5f);
                //displaySnake?.DrawBrain(spriteBatch, 24, 248, 8f, 60f, 2f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
