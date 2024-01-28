using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Accord.Neuro;
using Microsoft.ML;
using Microsoft.ML.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;

namespace ChessLogic
{
    [Serializable]
    public class ChessBot : IDisposable
    {
        private int[] layers;
        private float[][] neurons;
        private float[][] biases;
        private float[][][] weights;
        private Genes genes;

        public Genes Genes
        {
            get => genes; set => genes = value;
        }
        public int[] Layers { get => layers; set => layers = value; }
        public float[][] Neurons { get => neurons; set => neurons = value; }
        public float[][] Biases { get => biases; set => biases = value; }
        public float[][][] Weights { get => weights; set => weights = value; }
        public double Fitness { get; set; }


        public ChessBot()
        {
            this.genes = new Genes();
            genes.Reset();

            int numberOfLayers = (int)genes.NextBetween0And1() * 10;
            this.layers = new int[EvolutionarySystem.layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = EvolutionarySystem.layers[i];
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
        public float[] FeedForward(float[] inputs)
        {
            // Set the input layer
            Parallel.For(0, inputs.Length, i => neurons[0][i] = inputs[i]);

            // Process the remaining layers in parallel
            for (int i = 1; i < layers.Length; i++)
            {
                int layer = i - 1;
                Parallel.For(0, neurons[i].Length, j =>
                {
                    float value = 0f;
                    for (int k = 0; k < neurons[i - 1].Length; k++)
                    {
                        value += Weights[i - 1][j][k] * neurons[i - 1][k];
                    }
                    neurons[i][j] = Activate(value + biases[i][j]);
                });
            }

            return neurons[neurons.Length - 1];
        }
        public float Activate(float value)
        {
            if ((1 + (float)Math.Exp(-value)) == 0) return 0;
            return 1 / (1 + (float)Math.Exp(-value));
        }

        private void InitBiases()
        {
            List<float[]> biasList = new List<float[]>();
            genes.Reset();
            foreach (var t in layers)
            {
                float[] bias = new float[t];
                for (int j = 0; j < t; j++)
                {
                    bias[j] = (float)genes.NextBetween0And1() * 0.2f - 0.1f; // Multiply by 2f - 1f instead of .5f - .25f
                }
                biasList.Add(bias);
            }
            biases = biasList.ToArray();
        }

        private void InitWeights()
        {
            List<float[][]> weightsList = new List<float[][]>();
            genes.Reset();
            for (int i = 0; i < layers.Length - 1; i++)
            {
                float[][] weights = new float[layers[i + 1]][];
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    weights[j] = new float[layers[i]];
                    for (int k = 0; k < layers[i]; k++)
                    {
                        weights[j][k] = (float)genes.NextBetween0And1() * 0.2f - 0.1f; // Multiply by 2f - 1f instead of .5f - .25f
                    }
                }
                weightsList.Add(weights);
            }
            Weights = weightsList.ToArray();
        }

        public void Dispose()
        {
            Genes = null;
        }
    }
}


