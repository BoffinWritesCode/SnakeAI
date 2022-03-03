using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnakeAI.AI
{
    public class NeuralNet
    {
        private const float WEIGHT_MIN = -1f;
        private const float WEIGHT_MAX = 1f;
        private const float BIAS_MIN = -1f;
        private const float BIAS_MAX = 1f;

        private static readonly Color MIN_NODE = new Color(80, 80, 80);
        private static readonly Color MAX_NODE = Color.White;
        private static readonly Color MIN_WEIGHT = new Color(120, 0, 120);
        private static readonly Color MAX_WEIGHT = Color.LightBlue;

        private Matrix[] _weights;
        private Matrix[] _biases;

        private int _inputs;
        private int _hidden;
        private int _outputs;
        private int _hiddenLayers;

        public NeuralNet(int input, int hidden, int output, int hiddenLayers = 1)
        {
            _inputs = input;
            _hidden = hidden;
            _outputs = output;
            _hiddenLayers = hiddenLayers;

            _weights = new Matrix[hiddenLayers + 2];
            _biases = new Matrix[hiddenLayers + 2];

            //create weight and bias matrices
            _weights[0] = new Matrix(hidden, input);
            _biases[0] = new Matrix(hidden, 1);
            for (int i = 1; i <= hiddenLayers; i++)
            {
                _weights[i] = new Matrix(hidden, hidden);
                _biases[i] = new Matrix(hidden, 1);
            }
            _weights[hiddenLayers + 1] = new Matrix(output, hidden);
            _biases[hiddenLayers + 1] = new Matrix(output, 1);

            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = Randomise(_weights[i], SnakeAI.globalRandom, WEIGHT_MIN, WEIGHT_MAX);
                _biases[i] = Randomise(_biases[i], SnakeAI.globalRandom, BIAS_MIN, BIAS_MAX);
            }
        }

        public float[] Output(float[] input)
        {
            if (input.Length != _weights[0].columns)
            {
                throw new ArgumentException("length of input array is not the same as number of input nodes.");
            }

            Matrix inputMatrix = new Matrix(input);
            Matrix currentLayer = Activate(_weights[0] * inputMatrix + _biases[0]);

            for (int i = 1; i <= _hiddenLayers; i++)
            {
                currentLayer = Activate(_weights[i] * currentLayer + _biases[i]);
            }

            currentLayer = Activate(_weights[_hiddenLayers + 1] * currentLayer + _biases[_hiddenLayers + 1]);

            return currentLayer.ToArray();
        }

        private Matrix Randomise(Matrix input, Random random, float min, float max)
        {
            for (int x = 0; x < input.columns; x++)
            {
                for (int y = 0; y < input.rows; y++)
                {
                    input.data[x, y] = random.NextFloat(min, max);
                }
            }
            return input;
        }

        private Matrix Activate(Matrix input)
        {
            Matrix result = new Matrix(input.rows, input.columns);
            for (int x = 0; x < input.columns; x++)
            {
                for (int y = 0; y < input.rows; y++)
                {
                    result.data[x, y] = Math.Max(0, input.data[x, y]);
                }
            }
            return result;
        }
        
        public NeuralNet CrossWith(NeuralNet other)
        {
            NeuralNet cross = new NeuralNet(_inputs, _hidden, _outputs, _hiddenLayers);
            for (int i = 0; i < _weights.Length; i++)
            {
                cross._weights[i] = _weights[i].CrossWith(other._weights[i]);
                cross._biases[i] = _biases[i].CrossWith(other._biases[i]);
            }
            return cross;
        }

        public void Mutate()
        {
            foreach(Matrix m in _weights)
            {
                m.Mutate(WEIGHT_MIN, WEIGHT_MAX, 0.9f);
            }
            foreach (Matrix m in _biases)
            {
                m.Mutate(BIAS_MIN, BIAS_MAX, 0.9f);
            }
        }

        public NeuralNet Clone()
        {
            NeuralNet result = new NeuralNet(_inputs, _hidden, _outputs, _hiddenLayers);
            for (int i = 0; i < result._weights.Length; i++)
            {
                result._weights[i] = _weights[i].Clone();
                result._biases[i] = _biases[i].Clone();
            }
            return result;
        }

        public void Show(SpriteBatch spriteBatch, int x, int y, float nodeDiameter, float layerSep, float spacing, float[] inputs = null)
        {
            Vector2 offset = new Vector2(x, y);
            //draw input weights
            for (int i = 0; i < _inputs; i++)
            {
                for (int j = 0; j < _hidden; j++)
                {
                    Color color = GetColour(MIN_WEIGHT, MAX_WEIGHT, _weights[0].data[i, j], WEIGHT_MIN, WEIGHT_MAX);
                    spriteBatch.DrawLineDebug(GetNodePosition(0, i, nodeDiameter, layerSep, spacing) + offset, GetNodePosition(1, j, nodeDiameter, layerSep, spacing) + offset, color, 1f, SnakeAI.pixel);
                }
            }

            //draw input nodes
            for (int i = 0; i < _inputs; i++)
            {
                Color color = inputs == null ? MAX_NODE : GetColour(MIN_NODE, MAX_NODE, inputs[i], 0f, 1f);
                spriteBatch.Draw(SnakeAI.circle, GetNodePosition(0, i, nodeDiameter, layerSep, spacing) + offset, null, color, 0f, new Vector2(50), nodeDiameter / 100f, SpriteEffects.None, 0f);
            }

            for (int a = 1; a <= _hiddenLayers; a++)
            {
                //draw hidden weights
                int nextlayer = a == _hiddenLayers ? _outputs : _hidden;
                for (int i = 0; i < _hidden; i++)
                {
                    for (int j = 0; j < nextlayer; j++)
                    {
                        Color color = GetColour(MIN_WEIGHT, MAX_WEIGHT, _weights[a].data[i, j], WEIGHT_MIN, WEIGHT_MAX);
                        spriteBatch.DrawLineDebug(GetNodePosition(a, i, nodeDiameter, layerSep, spacing) + offset, GetNodePosition(a + 1, j, nodeDiameter, layerSep, spacing) + offset, color, 1f, SnakeAI.pixel);
                    }
                }
                //draw hidden nodes
                for (int i = 0; i < _hidden; i++)
                {
                    Color color = GetColour(MIN_NODE, MAX_NODE, _biases[a - 1].data[0, i], BIAS_MIN, BIAS_MAX);
                    spriteBatch.Draw(SnakeAI.circle, GetNodePosition(a, i, nodeDiameter, layerSep, spacing) + offset, null, color, 0f, new Vector2(50), nodeDiameter / 100f, SpriteEffects.None, 0f);
                }
            }

            float[] outputs = null;
            int highest = -1;
            float test = -1;
            if (inputs != null)
            {
                outputs = Output(inputs);
                for (int k = 0; k < outputs.Length; k++)
                {
                    if (outputs[k] > test)
                    {
                        test = outputs[k];
                        highest = k;
                    }
                }
            }

            //draw output nodes
            for (int i = 0; i < _outputs; i++)
            {
                Color color = GetColour(MIN_NODE, MAX_NODE, _biases[_hiddenLayers + 1].data[0, i], BIAS_MIN, BIAS_MAX);
                spriteBatch.Draw(SnakeAI.circle, GetNodePosition(_hiddenLayers + 1, i, nodeDiameter, layerSep, spacing) + offset, null, color, 0f, new Vector2(50), nodeDiameter / 100f, SpriteEffects.None, 0f);

                if (i == highest)
                {
                    string[] meme = new string[] { "W", "A", "S", "D" };
                    spriteBatch.DrawStringAroundCenter(meme[i], SnakeAI.font, GetNodePosition(_hiddenLayers + 1, i, nodeDiameter, layerSep, spacing) + offset + new Vector2(nodeDiameter * 0.5f + spacing + nodeDiameter * 0.25f, 0f), Color.Red);
                }
            }
        }

        private Vector2 GetNodePosition(int layer, int number, float diameter, float layerSep, float spacing)
        {
            if (layer == 0)
            {
                return new Vector2(diameter, diameter + number * (diameter + spacing));
            }

            int inLayer = layer == _hiddenLayers + 1 ? _outputs : _hidden;
            float totalHeight = diameter * _inputs + spacing * (_inputs - 1);
            float myCenterY = totalHeight * 0.5f - spacing * 0.5f - (inLayer * 0.5f) * (diameter + spacing) + (number + 1) * (diameter + spacing);
            float myCenterX = diameter + layerSep * layer;
            return new Vector2(myCenterX, myCenterY);
        }
        
        private Color GetColour(Color c1, Color c2, float value, float min, float max)
        {
            return Color.Lerp(c1, c2, BoffinMath.Map(min, max, 0f, 1f, value));
        }
    }
}
