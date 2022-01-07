using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This class is useful for serializing it and write the result on a log file
/// </summary>
[Serializable]
public class TetrisGeneration
{
    public float[] bestWeights;
    public float bestScore;

    public float scoreMean;
    public int genIndex;

    public int pieces;
    public int lines;
    public int level;
}
