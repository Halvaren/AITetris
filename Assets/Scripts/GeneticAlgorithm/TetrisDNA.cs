namespace Assets.Scripts.GeneticAlgorithm
{
    /// <summary>
    /// It represents an individual from a population of the genetic algorithm. Its genes are the weights and its fitness, the score of the game played
    /// </summary>
    public class TetrisDNA
    {
        private float[] weightGenes;
        private float score = 0;

        //This data is also stored to log it later in a file
        private int pieces = 0;
        private int lines = 0;
        private int level = 0;

        private TetrisBoardController tbController;
        public TetrisBoardController TBController
        {
            get
            {
                if (tbController == null) tbController = TetrisBoardController.Instance;
                return tbController;
            }
        }

        public TetrisDNA(int size)
        {
            weightGenes = new float[size];
        }

        /// <summary>
        /// It is called when an initial population is created
        /// </summary>
        public void SetRandomWeights()
        {
            for (int i = 0; i < weightGenes.Length; i++)
            {
                weightGenes[i] = UnityEngine.Random.value;
            }
        }

        /// <summary>
        /// Before playing a game, the current weights must be set in the board controller
        /// </summary>
        public void PlayGame()
        {
            TBController.SetWeights(weightGenes);

            TBController.PlayBot();
        }

        #region Getters & Setters

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

        #endregion 

        /// <summary>
        /// Giving another individual, they generate a new one, giving to them their genes, choosing them randomly
        /// </summary>
        /// <param name="otherDNA"></param>
        /// <returns></returns>
        public TetrisDNA Crossover(TetrisDNA otherDNA)
        {
            TetrisDNA child = new TetrisDNA(weightGenes.Length);

            for(int i = 0; i < weightGenes.Length; i++)
            {
                child.weightGenes[i] = UnityEngine.Random.value < 0.5f ? weightGenes[i] : otherDNA.weightGenes[i];
            }

            return child;
        }

        /// <summary>
        /// Giving a probability, it could mutate its genes by a random amount
        /// </summary>
        /// <param name="mutationRate"></param>
        public void Mutate(float mutationRate)
        {
            for(int i = 0; i < weightGenes.Length; i++)
            {
                if(UnityEngine.Random.value < mutationRate)
                {
                    weightGenes[i] += UnityEngine.Random.value * 0.1f - 0.05f;
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
