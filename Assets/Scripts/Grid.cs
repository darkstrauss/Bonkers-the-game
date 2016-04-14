using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author@ Alan Hart
/// </summary>
[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer), typeof (BoxCollider))]
public class Grid : MonoBehaviour
{
    //material for the floor mesh
    private Material floorMaterial;
    //size of the what the grid should generate
    public int xSize, zSize;
    //list of vector3 coordinates for the vertecies
    public Vector3[] floorVerts, waterVerts;

    //mesh component of the gameObject
    private Mesh meshFloor;
    //this gameObject
    public GameObject floorObject;
    
    //list of door gameObjects
    public List<GameObject> doors;

    //path that the A* path finder returns
    public List<MapPosition> path;
    
    //the starting node and end node that A* finds a path between
    public AStarNode currentNode, goal;

    //player gameObject
    public GameObject player;

    //static integer values used to destinguish walkable tiles from non-walkable tiles
    public static int WALL = 0;
    public static int FLOOR = 1;

    //two dimentional integer array used as map to represent the playing area
    public int[,] map;

    //two dimentional array of AStarNodes that overlays the map
    public AStarNode[,] nodeMap;

    //open and closed list for the A* to keep track of visited nodes
    List<AStarNode> closed = new List<AStarNode>();
    List<AStarNode> open = new List<AStarNode>();

    private void Awake()
    {
        //sets the gameObject to static
        gameObject.isStatic = true;
        //sets the floorObject to this gameObject
        floorObject = this.gameObject;
        //gets the player witch is stored in PlayerMovement on the main camera
        player = Camera.main.GetComponent<PlayerMovement>().player;
        //sets the active floor of the PlayerMovement
        Camera.main.GetComponent<PlayerMovement>().activeFloor = this.gameObject;

        //generates the visuals.
        Generate();

        //generates the functionality
        GenerateAStarPath();
    }

    private void GenerateAStarPath()
    {
        map = new int[xSize, zSize];

        for (int x = 0, i = 0; x < xSize; x++)
        {
            for (int y = 0; y < zSize; y++, i++)
            {
                map[x, y] = FLOOR;
            }
        }

        GenerateNodeMapFromIntMap();
    }

    private void GenerateNodeMapFromIntMap()
    {
        nodeMap = new AStarNode[xSize, zSize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < zSize; y++)
            {
                nodeMap[x, y] = new AStarNode(new MapPosition(x, y, map[x, y] > 0), 0f, 0f);
            }
        }
    }

    //this is the A* pathfinding algorithm.
    public List<MapPosition> FindPath(MapPosition start, MapPosition goal)
    {
        //unparrents any previously partented path sections
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < zSize; y++)
            {
                nodeMap[x, y].parent = null;
            }
        }

        //determines from what node to start looking.
        AStarNode startNode = nodeMap[start.xPos, start.yPos];
        //g score is 0 because its the starting node.
        startNode.g = 0.0f;

        //the remaining distance between the start and goal.
        startNode.f = startNode.g + MapPosition.EucludianDistance(start, goal);

        //adds the starting node to the open list to start the path finding.
        open.Add(startNode);

        while (open.Count > 0)
        {
            //sets the current node to the first node in the open list.
            AStarNode currentNode = open[0];

            //if the current node is equal to the goal it goes through the parents of the current node untill no more parents are seen.
            //at every parent it adds the current node to the path list and sets the current node to its parent.
            //when there are no more parents it adds the final current node to the path and returns a list of positions.
            if (currentNode.position == goal)
            {
                List<MapPosition> path = new List<MapPosition>();
                while (currentNode.parent != null)
                {
                    path.Add(currentNode.position);
                    currentNode = nodeMap[currentNode.parent.xPos, currentNode.parent.yPos];
                }
                path.Add(start);
                return path;
            }

            //if the current node isn't the goal it removes it from the open list and adds it to the closed list to stop checking that node.
            open.RemoveAt(0);
            closed.Add(currentNode);

            //list of neighbour nodes.
            List<AStarNode> neighbours = GetNeighbours(currentNode.position);

            //it goes through this for each neighbour in out neighbour list.
            foreach (AStarNode neighbourNode in neighbours)
            {
                //if the neighbour is already in closed list skip checking it.
                if (closed.Contains(neighbourNode))
                    continue;

                //sets the g cost
                float g = currentNode.g + neighbourNode.f;

                //checks if the neighbour is in the open list or not.
                bool inOpenList = open.Contains(neighbourNode);

                //If the neighbour isn't in the open list or the computed g score is lower than the neighbour node's g score.
                if (!inOpenList || g < neighbourNode.g)
                {
                    //sets the neighbour node's parent to the current node, and recomputes the f score.
                    neighbourNode.parent = currentNode.position;
                    neighbourNode.g = g;
                    neighbourNode.f = g + MapPosition.EucludianDistance(neighbourNode.position, goal);

                    //if the neighbour isn't in the open list it adds it to the end of the open list.
                    if (!inOpenList)
                    {
                        //index represents the position in the list that the neighbour should be put in.
                        int index = 0;
                        //while loop increases index untill it reaches the end of the list.
                        while (index < open.Count && open[index].f < neighbourNode.f)
                        {
                            index++;
                        }
                        //inserts the neighbour into the end of the open list.
                        open.Insert(index, neighbourNode);
                    }
                }
            }
        }

        //if the algorithm reaches this point it could not find a path.
        return null;
    }

    //this functions gets the neighbours of the node it's called on.
    public List<AStarNode> GetNeighbours(MapPosition current)
    {
        List<AStarNode> neighbours = new List<AStarNode>();

        //checks down
        if (current.yPos > 0 && map[current.xPos, current.yPos-1] == FLOOR && !closed.Contains(nodeMap[current.xPos, current.yPos - 1]))
        {
            neighbours.Add(nodeMap[current.xPos, current.yPos - 1]);
        }
        //checks up
        if (current.yPos < zSize - 1 && map[current.xPos, current.yPos + 1] == FLOOR && !closed.Contains(nodeMap[current.xPos, current.yPos + 1]))
        {
            neighbours.Add(nodeMap[current.xPos, current.yPos + 1]);
        }
        //checks left
        if (current.xPos > 0 && map[current.xPos - 1, current.yPos] == FLOOR && !closed.Contains(nodeMap[current.xPos - 1, current.yPos]))
        {
            neighbours.Add(nodeMap[current.xPos - 1, current.yPos]);
        }
        //checks right
        if (current.xPos < xSize - 1 && map[current.xPos + 1, current.yPos] == FLOOR && !closed.Contains(nodeMap[current.xPos + 1, current.yPos]))
        {
            neighbours.Add(nodeMap[current.xPos + 1, current.yPos]);
        }

        //returns the list of neighbours
        return neighbours;
    }

    private void Generate()
    {
        //generate the walls and doors
        GenerateWallsAndDoors();

        //create a list of vertecies equal to width * depth plus one on each side because of the edge
        floorVerts = new Vector3[(xSize + 1) * (zSize + 1)];
        //uv vector2 list equal to the amount of vertecies we have
        Vector2[] uvFloor = new Vector2[floorVerts.Length];

        //floor material it loads
        floorMaterial = Resources.Load<Material>("Materials/floor");

        //nested for loop to create vector3 coordinates
        for (int i = 0, y = 0; y < zSize + 1; y++)
        {
            for (int x = 0; x < xSize + 1; x++, i++)
            {
                //for every vector3 in floorVerts create a new vector3. As it loops through x and y increment by one.
                //when the x is done looping it increases the y by one and then x starts at zero again. This lays the vertecies in a grid pattern.
                floorVerts[i] = new Vector3(x, 0, y);

                //as the loop is going through vertex list it also sets the UV space of the generated mesh.
                //a uv space goes from 0 to 1 on two axies. As the for loop progresses the coordinates slowly go from zero to one on both axies for every vector coordinate
                uvFloor[i] = new Vector2((float)x / xSize, (float)y / zSize);
            }
        }

        //creates a new mesh component insuring a new mesh object and asigns it to mesh
        GetComponent<MeshFilter>().mesh = meshFloor = new Mesh();
        //give our newly generated mesh a name
        meshFloor.name = "Generated Floor Mesh";

        //sets the mesh component's vertecies to the new vector coordinates from the floorVerts list
        meshFloor.vertices = floorVerts;
        //set the uv and uv2 to the new vector2 coordinates of the UV list
        meshFloor.uv = uvFloor;
        meshFloor.uv2 = uvFloor;

        //creates an integer array equal to the ammount of connections needed to generate this mesh.
        //Say xSize is 10 and zSize is 10. It would be 10*10*6. For every triangle we need three vertecies,
        //but to make a quad we need two triangles. These two triangles share two vertecies but are assigned in,
        //a different order.
        int[] TrianglesFloor = new int[xSize * zSize * 6];

        //for every quad there are 6 entries and 4 vertecies. As the loop progresses it assigns the corners of the triangles to the correct vertex
        //each itteration adds 6 ti. This allows each itteration of the loop to create a new quad untill the zSize and xSize have been met
        for (int ti = 0, vi = 0, y = 0; y < zSize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                TrianglesFloor[ti] = vi;
                TrianglesFloor[ti + 3] = TrianglesFloor[ti + 2] = vi + 1;
                TrianglesFloor[ti + 4] = TrianglesFloor[ti + 1] = vi + xSize + 1;
                TrianglesFloor[ti + 5] = vi + xSize + 2;
                meshFloor.triangles = TrianglesFloor;
            }
        }

        //assigns the triangles
        meshFloor.triangles = TrianglesFloor;

        //create a vector3 for the collider box's size
        Vector3 colliderSize = new Vector3((float)xSize, 0.01f, (float)zSize);

        //assign the colliderSize and recenter the box
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = colliderSize;
        collider.center = new Vector3(xSize / 2, 0.0f, zSize / 2);

        //for lighting
        meshFloor.RecalculateBounds();
        meshFloor.RecalculateNormals();

        //assign material and how often it was to tile
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = floorMaterial;
        meshRenderer.material.mainTextureScale = new Vector2(xSize, zSize);

        gameObject.tag = "floor";
    }

    private void GenerateWallsAndDoors()
    {
        //creates a new gameObject for the wall and give it a usable name
        GameObject front = new GameObject("front");
        //set the parent to keep the heirarchy clean
        front.transform.parent = gameObject.transform;
        //change the layer to "Ignore Raycast" need this to keep the wall from being detected by the player's ObjectCollision
        front.layer = 2;
        //sets the wall's tag
        front.tag = "front";
        //add the wall component to start the wall generation
        front.AddComponent<Wall>();
        //creates a new gameObject for the door and name it door
        GameObject doorFront = new GameObject("door");
        doorFront.transform.parent = front.transform;
        //change the layer to "Ignore Raycast" need this to keep the wall from being detected by the player's ObjectCollision
        doorFront.layer = 2;
        //set the tag to the name of the corresponding wall
        doorFront.tag = front.name;
        //add the wall component to start the door generation
        doorFront.AddComponent<Door>();
        //add the door the the door list for access
        doors.Add(doorFront);

        //repeat this for every direction

        GameObject back = new GameObject("back");
        back.transform.parent = gameObject.transform;
        back.layer = 2;
        back.tag = "back";
        back.AddComponent<Wall>();
        GameObject doorBack = new GameObject("door");
        doorBack.layer = 2;
        doorBack.tag = back.name;
        doorBack.transform.parent = back.transform;
        doorBack.AddComponent<Door>();
        doors.Add(doorBack);

        GameObject down = new GameObject("down");
        down.transform.parent = gameObject.transform;
        down.layer = 2;
        down.tag = "down";
        down.AddComponent<Wall>();
        GameObject doorDown = new GameObject("door");
        doorDown.layer = 2;
        doorDown.tag = down.name;
        doorDown.transform.parent = down.transform;
        doorDown.AddComponent<Door>();
        doors.Add(doorDown);

        GameObject left = new GameObject("left");
        left.transform.parent = gameObject.transform;
        left.layer = 2;
        left.tag = "left";
        left.AddComponent<Wall>();
        GameObject doorLeft = new GameObject("door");
        doorLeft.layer = 2;
        doorLeft.tag = left.name;
        doorLeft.transform.parent = left.transform;
        doorLeft.AddComponent<Door>();
        doors.Add(doorLeft);
    }

    //used to reset the path finder
    public void ClearPath()
    {
        if (path != null)
        {
            path.Clear();
        }
        open.Clear();
        closed.Clear();
    }

    //this function gets called when the player clicks on the floor in the world
    public List<MapPosition> FindPath(Vector3 destination)
    {
        //reset the previous path
        ClearPath(); 

        //sets the current node and the goal node to start the path finding algorithm
        currentNode = nodeMap[Mathf.FloorToInt(player.transform.position.x), Mathf.FloorToInt(player.transform.position.z)];
        goal = nodeMap[Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.z)];

        //finds a path between the currentNode and goal and returns a list of mapPositions
        path = FindPath(currentNode.position, goal.position);

        return path;
    }

    //function is called by the GenerateContent script when it's done populating to occupy spots, refraining the player from walking there
    public void OccupySpots(List<Vector3> vectorList)
    {
        for (int i = 0; i < vectorList.Count; i++)
        {
            //every object that gets placed has a vector3 coordinate. This can be translated to the integer map and spots can be labled as WALL
            map[(int)vectorList[i].x, (int)vectorList[i].z] = WALL;
        }
    }

    //resets the currentNode when the player teleports to a new room
    public void SetCurrentNode(Vector3 destination)
    {
        currentNode = nodeMap[Mathf.FloorToInt(destination.x), Mathf.FloorToInt(destination.z)];
    }

    //forces the nodeMap to null on destruction of this gameObject
    private void OnDestroy()
    {
        nodeMap = null;
    }

    //returns the door list
    public List<GameObject> GetDoors()
    {
        return doors;
    }
}
