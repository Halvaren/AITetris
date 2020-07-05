///A piece action is a object that contains a rotation index and a coordinate in X. 
///When a piece executes a piece action, rotates the number of times rotationIndex indicates
///Moves to that X coordinate and then makes a hard drop
public class PieceAction
{
    public int rotationIndex;
    public int xCoord;

    public PieceAction(int rotationIndex, int xCoord)
    {
        this.rotationIndex = rotationIndex;
        this.xCoord = xCoord;
    }
}
