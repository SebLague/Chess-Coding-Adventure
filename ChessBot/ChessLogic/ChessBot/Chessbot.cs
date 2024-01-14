using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChessLogic
{
    public class ChessBot
    {
        private int[] layers;
        private float[][] neurons;
        private float[][] biases;
        private float[][][] weights;

        public ChessBot(int[] layers)
        {
            this.layers = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                this.layers[i] = layers[i];
            }
            InitNeurons();
            InitBiases();
            InitWeights();
        }

        private void InitNeurons()
        {
            List<float[]> neuronsList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                neuronsList.Add(new float[layers[i]]);
            }
            neurons = neuronsList.ToArray();
        }

        private void InitBiases()
        {
            List<float[]> biasList = new List<float[]>();
            Random rand = new Random();
            for (int i = 0; i < layers.Length; i++)
            {
                float[] bias = new float[layers[i]];
                for (int j = 0; j < layers[i]; j++)
                {
                    bias[j] = (float)(rand.NextDouble() * 1.0 - 0.5);
                }
                biasList.Add(bias);
            }
            biases = biasList.ToArray();
        }

        private void InitWeights()
        {
            List<float[][]> weightsList = new List<float[][]>();
            Random rand = new Random();
            for (int i = 0; i < layers.Length - 1; i++)
            {
                float[][] weights = new float[layers[i + 1]][];
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    weights[j] = new float[layers[i]];
                    for (int k = 0; k < layers[i]; k++)
                    {
                        weights[j][k] = (float)(rand.NextDouble() * 1.0 - 0.5);
                    }
                }
                weightsList.Add(weights);
            }
            weights = weightsList.ToArray();
        }

        public float[] FeedForward(float[] inputs)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                neurons[0][i] = inputs[i];
            }
            for (int i = 1; i < layers.Length; i++)
            {
                int layer = i - 1;
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float value = 0f;
                    for (int k = 0; k < neurons[i - 1].Length; k++)
                    {
                        value += weights[i - 1][j][k] * neurons[i - 1][k];
                    }
                    neurons[i][j] = activate(value + biases[i][j]);
                }
            }
            return neurons[neurons.Length - 1];
        }

        public float activate(float value)
        {
            return 1 / (1 + (float)Math.Exp(-value));
        }
    }
}


