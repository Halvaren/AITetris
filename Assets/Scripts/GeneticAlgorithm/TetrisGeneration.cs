using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GeneticAlgorithm
{
    /// <summary>
    /// This class is useful for serializing it and write the result on a log file
    /// </summary>
    [Serializable]
    public class TetrisGeneration
    {
        public float[] bestWeights;
        public float bestScore;
        public int generation;

        public int pieces;
        public int lines;
        public int level;
    }
}
