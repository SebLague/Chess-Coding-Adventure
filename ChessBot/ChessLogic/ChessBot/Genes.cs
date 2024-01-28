using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord;
using Accord.Math;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;

namespace ChessLogic
{
    [Serializable]
    public class Genes
    {
        public char[] genome;
        private int count = 0;
        public static int numberOfPossibleCharacters = 11;
        public static int mutationRate = 1;

        public char[] Seed
        {
            get => genome;set => genome = value;
        }

        public Genes()
        {
            GenerateNewGene(500);
            Reset();
        }

        private void GenerateNewGene(int size)
        {
            genome = new char[size];
            Random rng = new Random();

            for (int i = 0; i < size; i++)
            {
                int value = rng.Next(0, 11);
                if (value == 10) genome[i] = 'D';
                else genome[i] = value.ToString()[0];
            }
        }

        public void Reset()
        {
            count = 0;
        }

        public double NextBetween0And1()
        {
            char g = genome[count % genome.Length];
            return Char.IsDigit(g) ? Char.GetNumericValue(g) / 10 : g == 'D' ? 1 : 0;
        }



        public void Mutate()
           {
               Random rng = new Random();
           
               for (int i = 0; i < genome.Length; i++)
               {
                   if (rng.NextDouble() < mutationRate)
                   {
                       // Perform mutation only if a random value is less than mutationRate
                       int value = rng.Next(0, numberOfPossibleCharacters);
                       
                       // Check if the new value is the same as the current one
                       while (genome[i] == ValueChartConvert(value))
                       {
                           value = rng.Next(0, numberOfPossibleCharacters);
                       }
           
                       genome[i] = ValueChartConvert(value);
                   }
               }
           }

        public char ValueChartConvert(int value)
        {
            int returnValue = value;

            if (value == 10) returnValue = 'D';


            return returnValue.ToString()[0];
        }
    }
}
