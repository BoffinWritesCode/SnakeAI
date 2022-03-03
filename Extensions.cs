using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnakeAI
{
    public static class Extensions
    {
        //Fields required for some functions
        private static Regex removeNewLines = new Regex("/n+");

        //Categorised in regions via the class that has been extended, not return value.

        #region string
        public static string WrapText(this SpriteFont font, string text, float maxLineWidth)
        {
            if (string.IsNullOrEmpty(text)) return "";

            //replace new lines with spaces. This should create a normal sentence out of the text.
            text = removeNewLines.Replace(text, " ");

            string[] words = text.Split(' ');
            string newText = "";
            float currentWidth = 0f;
            float spaceWidth = font.MeasureString(" ").X;

            foreach(string word in words)
            {
                Vector2 wordSize = font.MeasureString(word);

                if (currentWidth + wordSize.X < maxLineWidth)
                {
                    newText += word + " ";
                    currentWidth += wordSize.X + spaceWidth;
                    continue;
                }

                newText += Environment.NewLine + word + " ";
                currentWidth = wordSize.X + spaceWidth;
            }

            return newText;
        }
        #endregion

        #region Random
        public static float NextFloat(this Random random, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("min cannot be greater than max.");
            }

            return min + (float)random.NextDouble() * (max - min);
        }
        public static float NextFloat(this Random random, float max)
        {
            return (float)(random.NextDouble() * max);
        }
        //Fisher-Yates shuffle
        public static void Shuffle<T>(this Random random, ref T[] input)
        {
            for (int i = input.Length - 1; i > 0; i--)
            {
                int index = random.Next(i + 1);

                T value = input[index];
                input[index] = input[i];
                input[i] = value;
            }
        }
        //Box Muller Gaussian (Normal Dist.)
        public static float NextGaussian(this Random random)
        {
            float x1;
            float w;
            do
            {
                x1 = random.NextFloat(2) - 1;
                float x2 = random.NextFloat(2) - 1;
                w = x1 * x1 + x2 * x2;
            } while (w >= 1f);
            w = (float)Math.Sqrt((-2f * Math.Log(w)) / w);
            return x1 * w;
        }
        #endregion

        #region Rectangle
        public static Vector2 Center(this Rectangle rect)
        {
            return new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
        }
        #endregion

        #region Vector2
        public static float VectorToRotation(this Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }
        public static Vector2 RotateMeAround(this Vector2 point, Vector2 around, float radians)
        {
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            Vector2 between = point - around;

            return new Vector2(around.X + between.X * cos - between.Y * sin, around.Y + between.X * sin + between.Y * cos);
        }
        public static float VectorToAngle(this Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }
        public static Vector2 GetClockwise90(this Vector2 vector)
        {
            return new Vector2(vector.Y, -vector.X);
        }
        public static Vector2 GetAntiClockwise90(this Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }
        #endregion

        #region float
        public static Vector2 AngleToVector(this float radians)
        {
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }
        #endregion

        #region Color
        public static Color Multiply(this Color c, Vector3 rgb)
        {
            return new Color((int)(c.R * rgb.X), (int)(c.G * rgb.Y), (int)(c.B * rgb.Z), c.A);
        }
        #endregion

        #region T[,]
        public static void ForEveryYInEachX<T>(this T[,] array, Action<T, int, int> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action", "Action cannot be null.");
            }
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    action.Invoke(array[x, y], x, y);
                }
            }
        }
        public static void ForEveryXInEachY<T>(this T[,] array, Action<T, int, int> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action", "Action cannot be null.");
            }
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    action.Invoke(array[x, y], x, y);
                }
            }
        }
        #endregion

        #region SpriteBatch
        public static void DrawStringAroundCenter(this SpriteBatch spriteBatch, string text, SpriteFont font, Vector2 center, Color color)
        {
            Vector2 size = font.MeasureString(text);
            spriteBatch.DrawString(font, text, center - size / 2f, color);
        }

        public static void DrawLineDebug(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color colour, float lineWidth, Texture2D pixelTexture)
        {
            Vector2 between = end - start;
            float length = between.Length();
            float rotation = (float)Math.Atan2(between.Y, between.X);
            spriteBatch.Draw(pixelTexture, start, null, colour, rotation, new Vector2(0f, 0.5f), new Vector2(length, lineWidth), SpriteEffects.None, 0f);
        }

        public static void DrawRectangleDebug(this SpriteBatch spriteBatch, Rectangle rectangle, Color colour, float lineWidth, Texture2D pixelTexture)
        {
            Vector2 topLeft = rectangle.Location.ToVector2();
            Vector2 topRight = new Vector2(topLeft.X + rectangle.Width, topLeft.Y);
            Vector2 bottomLeft = new Vector2(topLeft.X, topLeft.Y + rectangle.Height);
            Vector2 bottomRight = topLeft + rectangle.Size.ToVector2();

            DrawLineDebug(spriteBatch, topLeft, topRight, colour, lineWidth, pixelTexture);
            DrawLineDebug(spriteBatch, topLeft, bottomLeft, colour, lineWidth, pixelTexture);
            DrawLineDebug(spriteBatch, bottomRight, topRight, colour, lineWidth, pixelTexture);
            DrawLineDebug(spriteBatch, bottomRight, bottomLeft, colour, lineWidth, pixelTexture);
        }
        #endregion
    }
}
