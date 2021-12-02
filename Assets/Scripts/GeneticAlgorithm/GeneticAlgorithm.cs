using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GeneticAlgorithm
{
    /// <summary>
    /// Algorithm that uses a population of individuals, with some genes as its main features, and this population will evolve through some generations,
    /// selecting them, they will reproduce, mutate...
    /// </summary>
    public class GeneticAlgorithm
    {
        public List<TetrisDNA> population;
        public int generation;

        private float scoreSum;

        public float mutationRate; //Mutation probability
        
        private TetrisDNA best; //Best individual of the current generation

        private int nextGame; //Next individual that will be tested (it will play)

        public BotVersion botVersion; //BotVersion that is being training

        public GeneticAlgorithm(int populationSize, int dnaSize, float mutationRate, BotVersion botVersion)
        {
            nextGame = 0;
            scoreSum = 0;
            generation = 1;

            TetrisBoardController.Instance.UpdateUIGeneration(generation);

            this.mutationRate = mutationRate;
            this.botVersion = botVersion;

            population = new List<TetrisDNA>(); //A new random population is created
            for(int i = 0; i < populationSize; i++)
            {
                TetrisDNA tetrisDNA = new TetrisDNA(dnaSize);
                tetrisDNA.SetRandomWeights();

                population.Add(tetrisDNA);
            }

            if(population.Count > 0) best = population[0];
        }

        //When all the individuals have been tested, a new generation is created
        public void NewGeneration()
        {
            if(population.Count <= 0)
            {
                return;
            }

            //The current generation data will be saved in a log file
            SaveCurrentGeneration();

            List<TetrisDNA> newPopulation = new List<TetrisDNA>();

            for(int i = 0; i < population.Count; i++)
            {
                //Selection
                TetrisDNA parent1 = ChooseParent();
                TetrisDNA parent2 = ChooseParent();

                //Crossover
                TetrisDNA child = parent1.Crossover(parent2);

                //Mutation
                child.Mutate(mutationRate);

                newPopulation.Add(child);
            }

            population = newPopulation;

            generation++;
            TetrisBoardController.Instance.UpdateUIGeneration(generation);

            nextGame = 0;

            scoreSum = 0;
            if(population.Count > 0) best = population[0];
        }

        //Since the testing requires a real-time game, the individuals are tested individually jeje
        public void PlayNextGame()
        {
            population[nextGame].PlayGame();
        }

        //When a game ends, the TetrisBoardController sends data about this game. The most important one, the score
        public void SetDataToLastGame(float score, int pieces, int lines, int level)
        {
            population[nextGame].SetScore(score);
            population[nextGame].SetPieces(pieces);
            population[nextGame].SetLines(lines);
            population[nextGame].SetLevel(level);

            scoreSum += score;

            if(best.GetScore() < score)
            {
                best = population[nextGame];
            }

            nextGame++;

            //If all the individuals have been tested, a new generation have to be created
            if(nextGame >= population.Count)
            {
                NewGeneration();
            }
        }

        /// <summary>
        /// Chooses a parent randomly but lending more weight to those individuals which have a better score (selection)
        /// </summary>
        /// <returns></returns>
        public TetrisDNA ChooseParent()
        {
            double randomNumber = Random.value * scoreSum;

            for(int i = 0; i < population.Count; i++)
            {
                if(randomNumber < population[i].GetScore())
                {
                    return population[i];
                }

                randomNumber -= population[i].GetScore();
            }

            return null;
        }

        //The data of the current generation is saved in a log file
        private void SaveCurrentGeneration()
        {
            TetrisGeneration currentGeneration = new TetrisGeneration();
            currentGeneration.genIndex = generation;
            currentGeneration.bestScore = best.GetScore();
            currentGeneration.bestWeights = best.GetWeights();

            currentGeneration.scoreMean = scoreSum / population.Count;

            currentGeneration.pieces = best.GetPieces();
            currentGeneration.level = best.GetLevel();
            currentGeneration.lines = best.GetLines();

            LogWriter.Instance.WriteGeneration(botVersion, currentGeneration);
        }
    }
}
