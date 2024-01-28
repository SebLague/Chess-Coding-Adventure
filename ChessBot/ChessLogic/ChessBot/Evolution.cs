using Accord.Genetic;
using Accord.Math.Random;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
        public static int[] layers = {64, 1000, 1};
        public static Random rng = new Random();

        public EvolutionarySystem(int populationSize, int maxGenerations)
        {
            this.populationSize = populationSize -1;
            this.maxGenerations = maxGenerations;
            currentPopulation = new Population(populationSize);
            currentPopulation.Initialize();
        }

        public ChessBot[] GetContenders()
        {
            if (count + 1 >= populationSize)
            {
                count = 0;
                return null;
            }
            else
            {
                ChessBot[] returnValue =
                    { currentPopulation.Bots[count], currentPopulation.Bots[count + 1] };

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
                object obj = currentPopulation.SelectBest(2)[0].Genes;
                SaveToFile("C:/Chess/ChessBot/ChessLogic/Json/Parent1.json", obj);

                return true;
            }

            // if it is not the last generation, repopulate from the 2 best individuals of last gen
            // Select the best individuals
            ChessBot bestLookingGuy = currentPopulation.SelectBest(1)[0];

            // Create new population for next generation
            Population lastPopulation = currentPopulation;
            currentPopulation = new Population(populationSize);

            // Perform crossover and mutation to create new individuals
            for (int i = 0; i < populationSize; i++)
            {
                var child = CrossOver(bestLookingGuy, lastPopulation.Bots[i]);
                currentPopulation.Bots.Add(child);
            }

            lastPopulation.Dispose();

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
        public static ChessBot CrossOver(ChessBot parent1, ChessBot parent2)
        {
            // Create new child
            ChessBot bot = new ChessBot();

            // Choose a random point in the parent chromosomes
            int crossOverPoint = rng.Next(parent1.Genes.genome.Length);

            // Copy the bits of the first parent to the child
            Array.Copy(parent1.Genes.genome, bot.Genes.genome, crossOverPoint);

            // Copy the bits of the second parent to the child
            Array.Copy(parent2.Genes.genome, crossOverPoint, bot.Genes.genome, crossOverPoint, parent2.Genes.genome.Length - crossOverPoint);

            bot.Fitness = 0;

            bot.Genes.Mutate();

            return bot;
        }
    }
    public class Population : IDisposable
    {
        private bool Initialized { get; set; } = false;
        private List<ChessBot> bots;

        public Population(int size)
        {
            bots = new List<ChessBot>(size);
        }

        public void Initialize()
        {
            if (Initialized) return;

            // try loading from file
            try
            {

                ChessBot loadedParent1 =
                    LoadFromFile<ChessBot>("C:/Chess/ChessBot/ChessLogic/Json/Parent1.json");

                bots.Add(loadedParent1);

                for (int i = 0; i < bots.Capacity - 2; i++)
                {
                    ChessBot child = EvolutionarySystem.CrossOver(loadedParent1, new ChessBot());

                    bots.Add(child);
                }

            }
            // else create new randomly generated bots
            catch (Exception)
            {
                for (int i = 0; i < bots.Capacity; i++)
                {
                    ChessBot chessBot = new ChessBot();
                    bots.Add(chessBot);
                }
            }

            Initialized = true;
        }

        public static T LoadFromFile<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        public List<ChessBot> Bots
        {
            get { return bots; }
        }

        public List<ChessBot> SelectBest(int count)
        {
            // Sort the bots in descending order of fitness
            var sortedIndividuals = bots.OrderByDescending(individual => individual.Fitness).ToList();

            // Return the top count bots
            return sortedIndividuals.Take(count).ToList();
        }

        public void Dispose()
        {
            foreach (var bot in bots)
            {
                bot.Dispose();
            }
            bots = null;
        }
    }


}
