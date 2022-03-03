using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace SnakeAI
{
    public static class BoffinMath
    {
        /// <summary>
        /// Returns the floor of the specified parameter as an integer.
        /// </summary>
        public static int FloorToInt(double x)
        {
            return (int)Math.Floor(x);
        }

        /// <summary>
        /// Maps the specified value from it's original minimum and maximum to a new specifed minimum and maximum
        /// </summary>
        /// <param name="baseMin">The original minimum.</param>
        /// <param name="baseMax">The original maximum.</param>
        /// <param name="newMin">The new minimum.</param>
        /// <param name="newMax">The new maximum.</param>
        /// <param name="value">The value to re-map.</param>
        /// <returns>The re-mapped value.</returns>
        public static float Map(float baseMin, float baseMax, float newMin, float newMax, float value)
        {
            return newMin + (value - baseMin) * (newMax - newMin) / (baseMax - baseMin);
        }

        /// <summary>
        /// Returns the biggest rectangle in the ratio provided that fits the width and height.
        /// </summary>
        /// <param name="maxWidth">The maximum width</param>
        /// <param name="maxHeight">The maximum height</param>
        /// <param name="ratio">The y/x ratio you require</param>
        public static Rectangle MaxRectFromRatio(int maxWidth, int maxHeight, float ratio = 0.5625f)
        {
            int x = 0;
            int y = 0;
            int width = maxWidth;
            int height = maxHeight;
            float value = height / (float)width;
            
            if (value < ratio)
            {
                //if it's less than that, the y value is too small, so we need to use the y value for the height and get the right value for the width.
                width = (int)Math.Floor((height / 9f) * 16f);
                x = (maxWidth - width) / 2;
            }
            else
            {
                //if it's greater, the y value is too great, so we need to use the width and get the right value of the height
                height = (int)Math.Floor((width / 16f) * 9f);
                y = (maxHeight - height) / 2;
            }

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Returns the shortest perpendicular distance from a point to a specified line.
        /// </summary>
        /// <param name="point">The point to check the perpendicular distance to.</param>
        /// <param name="lineStart">The start point of the line.</param>
        /// <param name="lineEnd">The end point of the line.</param>
        /// <returns></returns>
        public static float DistanceFromLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, bool isLineInfinite = false)
        {
            return (ClosestPointOnLineToPoint(point, lineStart, lineEnd, isLineInfinite) - point).Length();
        }

        /// <summary>
        /// Returns the closest point on a line to a point.
        /// </summary>
        public static Vector2 ClosestPointOnLineToPoint(Vector2 point, Vector2 lineStart, Vector2 lineEnd, bool isLineInfinite = false)
        {
            Vector2 delta = Vector2.Normalize(lineEnd - lineStart);
            Vector2 pointToStart = point - lineStart;
            float dot = Vector2.Dot(pointToStart, delta);
            Vector2 returnPoint = lineStart + delta * dot;

            if (!isLineInfinite)
            {
                //clamp the return point so that it cannot be a point that doesn't exist on the line defined by start and end points.
                returnPoint.X = MathHelper.Clamp(returnPoint.X, Math.Min(lineStart.X, lineEnd.X), Math.Max(lineStart.X, lineEnd.X));
                returnPoint.Y = MathHelper.Clamp(returnPoint.Y, Math.Min(lineStart.Y, lineEnd.Y), Math.Max(lineStart.Y, lineEnd.Y));
            }

            return returnPoint;
        }
    }
}
