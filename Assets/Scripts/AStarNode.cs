/// <summary>
/// Auhtor@ Alan Hart
/// </summary>
public class AStarNode
{
    // the position it represents
    public MapPosition position;
    // the parent node of this position (the step in the path before this one)
    public MapPosition parent = null;
    // the A* numbers
    public float f;
    public float g;

    //extra cost that can be provided
    public int terrainCost;

    //the AStarNode contains a MapPosition, f value and g value.
    public AStarNode(MapPosition _position, float _f = 1.0f, float _g = 0.0f)
    {
        position = _position;
        f = _f;
        g = _g;
    }
}