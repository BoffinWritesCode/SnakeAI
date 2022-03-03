using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SnakeAI.AI
{
    public class Matrix
    {
        public float[,] data;

        public int rows;
        public int columns;

        public Matrix(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            data = new float[columns, rows];
        }

        public Matrix(float[] input)
        {
            this.rows = input.Length;
            this.columns = 1;
            data = new float[columns, rows];

            for (int x = 0; x < input.Length; x++)
            {
                data[0, x] = input[x];
            }
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix result = new Matrix(m1.rows, m2.columns);

            if (m1.columns == m2.rows)
            {
                for (int y = 0; y < result.rows; y++)
                {
                    for (int x = 0; x < result.columns; x++)
                    {
                        float sum = 0f;
                        for (int x2 = 0; x2 < m1.columns; x2++)
                        {
                            sum += m1.data[x2, y] * m2.data[x, x2];
                        }
                        result.data[x, y] = sum;
                    }
                }
            }

            return result;
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            Matrix result = new Matrix(m1.rows, m1.columns);

            if (m1.rows == m2.rows && m1.columns == m2.columns)
            {
                for (int x = 0; x < m1.columns; x++)
                {
                    for (int y = 0; y < m1.rows; y++)
                    {
                        result.data[x, y] = m1.data[x, y] + m2.data[x, y];
                    }
                }
            }

            return result;
        }

        public float[] ToArray()
        {
            float[] array = new float[rows];
            for (int i = 0; i < rows; i++)
            {
                array[i] = data[0, i];
            }
            return array;
        }

        public Matrix CrossWith(Matrix other)
        {
            Matrix child = new Matrix(rows, columns);
            int row = SnakeAI.globalRandom.Next(rows);
            int col = SnakeAI.globalRandom.Next(columns);
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < columns; i++)
                {
                    if (j <= row || (i <= col && j == row))
                    {
                        child.data[i, j] = data[i, j];
                    }
                    else
                    {
                        child.data[i, j] = other.data[i, j];
                    }
                }
            }
            return child;
        }

        public void Mutate(float min, float max, float mult)
        {
            //Random random = SnakeAI.globalRandom;
            Random random = new Random();
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (random.NextFloat(1f) < SnakeAI.MUTATION_RATE)
                    {
                        double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
                        double u2 = 1.0 - random.NextDouble();
                        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                        data[i, j] = MathHelper.Clamp(data[i, j] + (float)randStdNormal * mult, min, max);
                    }
                }
            }
        }

        public Matrix Clone()
        {
            Matrix clone = new Matrix(rows, columns);
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    clone.data[i, j] = data[i, j];
                }
            }
            return clone;
        }

        public override string ToString()
        {
            string result = "";
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    result += data[x, y];
                    if (x != columns - 1) result += ", ";
                }
                result += "\n";
            }
            return result;
        }
    }
}
