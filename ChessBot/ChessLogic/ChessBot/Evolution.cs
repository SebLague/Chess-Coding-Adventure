using Accord.Genetic;
using Accord.Math.Random;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json.Serialization;

namespace ChessLogic
{
    public class EvolutionarySystem
    {
        private Population currentPopulation;
        private int maxGenerations;
        private int currentGeneration;
        private int populationSize;
        private int count = 0;

        public EvolutionarySystem(int populationSize, int maxGenerations)
        {
            this.populationSize = populationSize;
            this.maxGenerations = maxGenerations;
            currentPopulation = new Population(populationSize);
            currentPopulation.Initialize();
            int x = 1;
        }

        public static float[][] Copy2DJaggedArray(float[][] originalArray)
        {
            float[][] newArray = new float[originalArray.Length][];
            for (int i = 0; i < originalArray.Length; i++)
            {
                // Copy the inner arrays to the new array
                newArray[i] = new float[originalArray[i].Length];
                Array.Copy(originalArray[i], newArray[i], originalArray[i].Length);
            }

            return newArray;
        }
        

        public static float[][][] Copy3DJaggedArray(float[][][] originalArray)
        {
            float[][][] newArray = new float[originalArray.Length][][];

            for (int i = 0; i < originalArray.Length; i++)
            {
                newArray[i] = new float[originalArray[i].Length][];

                for (int j = 0; j < originalArray[i].Length; j++)
                {
                    newArray[i][j] = new float[originalArray[i][j].Length];
                    Array.Copy(originalArray[i][j], newArray[i][j], originalArray[i][j].Length);
                }
            }

            return newArray;
        }

        public Individual[] GetContenders()
        {
            if (count + 1 >= populationSize)
            {
                count = 0;
                return null;
            }
            else
            {
                Individual[] returnValue =
                    { currentPopulation.Individuals[count], currentPopulation.Individuals[count + 1] };

                count += 2;
                return returnValue;
            }
        }
        
        public bool EndGeneration()
        {
            // if it is the last generation, save the best bot to file
            count = 0;
            currentGeneration++;
            if (currentGeneration >= maxGenerations)
            {
                currentGeneration = 0;
                object obj = currentPopulation.SelectBest(2)[0];
                SaveToFile("D:/Chess/ChessBot/ChessLogic/Json/Parent1.json", obj);

                obj = currentPopulation.SelectBest(2)[1];
                SaveToFile("D:/Chess/ChessBot/ChessLogic/Json/Parent2.json", obj);

                return true;
            }

            // if it is not the last generation, repopulate from the 2 best individuals of last gen
            // Select the best individuals
            var parents = currentPopulation.SelectBest(2);


            // Create new population for next generation
            currentPopulation = new Population(populationSize);

            // Perform crossover and mutation to create new individuals
            for (int i = 0; i < populationSize; i++)
            {
                var child = CrossOver(parents[0], parents[1]);
                currentPopulation.Individuals.Add(child);
            }

            return false;
        }
        public void SaveToFile(string path, object obj)
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            var json = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(path, json);
        }
        public static Individual CrossOver(Individual parent1, Individual parent2)
        {
            // Create new child
            ChessBot bot = new ChessBot(parent1.ChessBot);
            Individual child = new Individual(bot, parent1.Genes.Count);
            bot.individual = child;

            // Choose a random point in the parent chromosomes
            Random rnd = new Random();
            int crossOverPoint = rnd.Next(parent1.Genes.Count);

            // Copy the bits of the first parent to the child
            for (int i = 0; i < crossOverPoint; i++)
            {
                child.Genes.Add(parent1.Genes[i]);
            }

            // Copy the bits of the second parent to the child
            for (int i = crossOverPoint; i < parent2.Genes.Count; i++)
            {
                child.Genes.Add(parent2.Genes[i]);
            }

            child.Fitness = 0;
            child.Mutate();

            return child;
        }
    }
    [Serializable]
    public class Individual
    {
        public List<float> Genes { get; set; }
        public double Fitness { get; set; }

        public ChessBot ChessBot { get => chessBot; set => chessBot = value ; }  
        private ChessBot chessBot;

        public ChessBot GetChessBot()
        {
            return chessBot;
        }
        public Individual(int[] layers, int geneCount)
        {
            Genes = new List<float>(geneCount);

            Random rand = new Random();
            for (int i = 0; i < geneCount; i++)
            {
                Genes.Add(rand.NextSingle() * 1.5f + 0.5f); // Random number between 0.5 - 2
            }

            this.chessBot = new ChessBot(layers, this);
        }

        public Individual(ChessBot bot, int geneCount)
        {
            Genes = new List<float>(geneCount);
            chessBot = bot;
        }

        public Individual()
        {
        }

        public void Mutate()
        {
            Random rnd = new Random();

            double mutationFactor = rnd.NextDouble();

            for (int i = 0; i < Genes.Count; i++)
            {
                // Alter gene (always happens
                Genes[i] += (float)mutationFactor * 0.4f - 0.2f;
                Genes[i] = Math.Max(0.5f, Math.Min(Genes[i], 2f));

            }

            chessBot.CalculateGeneModification();
        }
    }

    public class Population
    {
        private bool Initialized { get; set; } = false;
        private List<Individual> individuals;

        public Population(int size)
        {
            individuals = new List<Individual>(size);
        }

        public void Initialize()
        {
            if (Initialized) return;

            // try loading from file
            try
            {

                Individual loadedParent1 =
                    LoadFromFile<Individual>("D:/Chess/ChessBot/ChessLogic/Json/Parent1.json");
                Individual loadedParent2 =
                    LoadFromFile<Individual>("D:/Chess/ChessBot/ChessLogic/Json/Parent2.json");

                for (int i = 0; i < individuals.Capacity; i++)
                {
                    Individual child = EvolutionarySystem.CrossOver(loadedParent1, loadedParent2);
                    individuals.Add(child);
                }

            }
            // else create new randomly generated individuals
            catch (Exception _)
            {
                for (int i = 0; i < individuals.Capacity; i++)
                {
                    int[] layers = { 64, 1000, 1 };
                    Individual newDude = new Individual(layers, 24);
                    newDude.GetChessBot().individual = newDude;
                    individuals.Add(newDude);
                }
            }

            Initialized = true;
        }

        public static T LoadFromFile<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        public List<Individual> Individuals
        {
            get { return individuals; }
        }

        public double TotalFitness()
        {
            return individuals.Sum(individual => individual.Fitness);
        }

        public List<Individual> SelectBest(int count)
        {
            // Sort the individuals in descending order of fitness
            var sortedIndividuals = individuals.OrderByDescending(individual => individual.Fitness).ToList();

            // Return the top count individuals
            return sortedIndividuals.Take(count).ToList();
        }
    }


}
