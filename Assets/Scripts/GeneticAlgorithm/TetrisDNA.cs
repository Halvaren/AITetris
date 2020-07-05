using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GeneticAlgorithm
{
    [Serializable]
    public class TetrisDNA
    {
        private float[] weightGenes;
        private float score = 0;

        private int pieces = 0;
        private int lines = 0;
        private int level = 0;

        public TetrisDNA(int size)
        {
            weightGenes = new float[size];
        }

        public void SetRandomWeights()
        {
            for (int i = 0; i < weightGenes.Length; i++)
            {
                weightGenes[i] = UnityEngine.Random.value;
            }
        }

        public void PlayGame()
        {
            TetrisBoardController.Instance.SetWeights(weightGenes);

            TetrisBoardController.Instance.PlayBot();
        }

        public float GetScore()
        {
            return score;
        }

        public int GetPieces()
        {
            return pieces;
        }

        public int GetLevel()
        {
            return level;
        }

        public int GetLines()
        {
            return lines;
        }

        public void SetScore(float score)
        {
            this.score = score;
        }

        public void SetPieces(int pieces)
        {
            this.pieces = pieces;
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        public void SetLines(int lines)
        {
            this.lines = lines;
        }

        public float GetWeight(int index)
        {
            return weightGenes[index];
        }

        public float[] GetWeights()
        {
            return weightGenes;
        }

        public TetrisDNA Crossover(TetrisDNA otherDNA)
        {
            TetrisDNA child = new TetrisDNA(weightGenes.Length);

            for(int i = 0; i < weightGenes.Length; i++)
            {
                child.weightGenes[i] = UnityEngine.Random.value < 0.5f ? weightGenes[i] : otherDNA.weightGenes[i];
            }

            return child;
        }

        public void Mutate(float mutationRate)
        {
            for(int i = 0; i < weightGenes.Length; i++)
            {
                if(UnityEngine.Random.value < mutationRate)
                {
                    weightGenes[i] += UnityEngine.Random.value * 0.1f - 0.05f; //Numeros mágicos!!!!
                }
            }
        }

        public override string ToString()
        {
            string result = "Weights: \n";
            result += "Holes weight: " + weightGenes[0];
            result += "Bumpiness weight: " + weightGenes[1];
            result += "Lines weight: " + weightGenes[2];
            result += "Rows with holes weight: " + weightGenes[3];
            if (weightGenes.Length == 5) result += "Humanized weight: " + weightGenes[4];

            result += "Score: " + score;

            return result;
        }
    }
}
