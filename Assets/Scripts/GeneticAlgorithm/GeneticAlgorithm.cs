using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GeneticAlgorithm
{
    public class GeneticAlgorithm
    {
        public List<TetrisDNA> population;
        public int generation;

        private float scoreSum;

        public float mutationRate;

        private TetrisDNA best;

        private int nextGame;

        public BotVersion botVersion;

        public GeneticAlgorithm(int populationSize, int dnaSize, float mutationRate, BotVersion botVersion)
        {
            nextGame = 0;
            scoreSum = 0;
            generation = 1;

            TetrisBoardController.Instance.UpdateUIGeneration(generation);

            this.mutationRate = mutationRate;
            this.botVersion = botVersion;

            population = new List<TetrisDNA>();
            for(int i = 0; i < populationSize; i++)
            {
                TetrisDNA tetrisDNA = new TetrisDNA(dnaSize);
                tetrisDNA.SetRandomWeights();

                population.Add(tetrisDNA);
            }

            if(population.Count > 0) best = population[0];
        }

        public GeneticAlgorithm(List<TetrisDNA> population, int generation, float mutationRate, BotVersion botVersion)
        {
            nextGame = 0;
            this.generation = generation;
            scoreSum = 0;

            TetrisBoardController.Instance.UpdateUIGeneration(generation);

            this.mutationRate = mutationRate;
            this.population = population;
            this.botVersion = botVersion;
        }

        public void NewGeneration()
        {
            if(population.Count <= 0)
            {
                return;
            }

            SaveCurrentGeneration();

            List<TetrisDNA> newPopulation = new List<TetrisDNA>();

            for(int i = 0; i < population.Count; i++)
            {
                TetrisDNA parent1 = ChooseParent();
                TetrisDNA parent2 = ChooseParent();

                TetrisDNA child = parent1.Crossover(parent2);

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

        public void PlayNextGame()
        {
            population[nextGame].PlayGame();
        }

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

            if(nextGame >= population.Count)
            {
                NewGeneration();
            }
        }

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

        private void SaveCurrentGeneration()
        {
            TetrisGeneration currentGeneration = new TetrisGeneration();
            currentGeneration.generation = generation;
            currentGeneration.bestScore = best.GetScore();
            currentGeneration.bestWeights = best.GetWeights();

            currentGeneration.pieces = best.GetPieces();
            currentGeneration.level = best.GetLevel();
            currentGeneration.lines = best.GetLines();

            LogWriter.Instance.WriteGeneration(botVersion, currentGeneration);
        }
    }
}
