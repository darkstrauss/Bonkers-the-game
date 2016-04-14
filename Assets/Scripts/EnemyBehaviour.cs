using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author@ Alan Hart
/// </summary>
public class EnemyBehaviour : MonoBehaviour
{
    //static floats used to represent distances
    public static float GOTOPLAYERDISTANCE = 3.5f;
    public static float STARTCOMBARDISTANCE = 1f;

    //static integer used to represend FLOOR tiles in the map
    private static int FLOOR = 1;

    //reference to the combat system gameObejct
    private GameObject CBS;

    //how fast the enemy can move
    private float moveSpeed = 3.0f;

    //reference to the player gameObject
    private GameObject player;

    //playerMovement component on the main camera
    private PlayerMovement playerMovement;

    //grid component from the active floor
    private Grid grid;

    //mapPositions of the player and enemy
    private MapPosition playerPosition, enemyPosition;

    public AStarNode currentNode, goal;
    //lists used by A* to find a path
    List<AStarNode> closed = new List<AStarNode>();
    List<AStarNode> open = new List<AStarNode>();
    //path that A* returns
    List<MapPosition> path;

    //grid sizes
    private int xSize, zSize;
    public int[,] map;
    public AStarNode[,] nodeMap;

    //if the player is selected or not
    public bool selectedPlayer = false;
    //state machine booleans
    private bool isIdle, pathComplete, isWaiting;
    bool process;

    public int pathCount;

    public void Start()
    {
        playerMovement = Camera.main.GetComponent<PlayerMovement>();
        CBS = Camera.main.GetComponent<PlayerMovement>().CBS;
        player = playerMovement.player;
        grid = playerMovement.GetFloor();
        xSize = grid.xSize;
        zSize = grid.zSize;
        nodeMap = grid.nodeMap;
        map = grid.map;

        StateMachine("idle");
    }

    private IEnumerator Move()
    {
        //when this coroutine gets called it starts to process
        process = true;

        //this work very similarly to the player move coroutine
        while (path != null && path.Count > 0 && process)
        {
            //creates new vector3 for every position in the path
            Vector3 movePosition = new Vector3((float)path[path.Count - 1].xPos + 0.5f, 0.0f, (float)path[path.Count - 1].yPos + 0.5f);
            //rotate the enemy to where he will be moving
            Quaternion targetRotation = Quaternion.LookRotation(movePosition - gameObject.transform.position);
            gameObject.transform.rotation = targetRotation;

            //while the gameObject is not at the destination it moves the gameobject
            while (!gameObject.transform.position.Equals(movePosition))
            {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, movePosition, moveSpeed * Time.deltaTime);
                //small wait to the enemy doesnt move instantly
                yield return new WaitForSeconds(0.01f);
            }

            if (path.Count > 0)
            {
                //as the enemy moves through its path it removes the last entry, untill there are no more path nodes to move to
                path.RemoveAt(path.Count - 1);
            }

            if (path.Count <= 1)
            {
                //when the path reaches zero ResetPath is called
                ResetPath();
            }
        }

        process = false;
    }

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

    public void StateMachine(string state)
    {
        isIdle = false;
        selectedPlayer = false;
        isWaiting = false;
        switch (state)
        {
            case "idle":
                path = GetPath();
                StartCoroutine(Move());
                gameObject.GetComponent<Animator>().Play("Walking");
                isIdle = true;
                break;
            case "chase":
                path = GetPathToPlayer();
                StartCoroutine(Move());
                gameObject.GetComponent<Animator>().Play("Walking");
                selectedPlayer = true;
                break;
            case "wait":
                ResetPath();
                ResetPosition(transform.position);
                gameObject.GetComponent<Animator>().Play("Idle");
                isWaiting = true;
                break;
        }
    }

    //this fucntion gets the neighbours of the node it's called on.
    private List<AStarNode> GetNeighbours(MapPosition current)
    {
        List<AStarNode> neighbours = new List<AStarNode>();

        //checks down
        if (current.yPos > 0 && map[current.xPos, current.yPos - 1] == FLOOR && !closed.Contains(nodeMap[current.xPos, current.yPos - 1]))
            neighbours.Add(nodeMap[current.xPos, current.yPos - 1]);
        //checks up
        if (current.yPos < zSize - 1 && map[current.xPos, current.yPos + 1] == FLOOR && !closed.Contains(nodeMap[current.xPos, current.yPos + 1]))
            neighbours.Add(nodeMap[current.xPos, current.yPos + 1]);
        //checks left
        if (current.xPos > 0 && map[current.xPos - 1, current.yPos] == FLOOR && !closed.Contains(nodeMap[current.xPos - 1, current.yPos]))
            neighbours.Add(nodeMap[current.xPos - 1, current.yPos]);
        //checks right
        if (current.xPos < xSize - 1 && map[current.xPos + 1, current.yPos] == FLOOR && !closed.Contains(nodeMap[current.xPos + 1, current.yPos]))
            neighbours.Add(nodeMap[current.xPos + 1, current.yPos]);

        //returns the neighbours list with the added nodes(if any)
        return neighbours;
    }

    //get path returns a path with 2 entries. the node the enemy is currently standing on and a random neighbour
    //this creates a random walking pattern
    private List<MapPosition> GetPath()
    {
        ResetPath();

        currentNode = nodeMap[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z)];
        List<AStarNode> neighbours = GetNeighbours(currentNode.position);
        goal = nodeMap[neighbours[Random.Range(0, neighbours.Count)].position.xPos, neighbours[Random.Range(0, neighbours.Count)].position.yPos];

        path = FindPath(currentNode.position, goal.position);

        if (path != null && path.Count > 1)
        {
            path.RemoveAt(path.Count - 1);
        }

        return path;
    }

    //this will return a path from the enemy's current position to the player
    private List<MapPosition> GetPathToPlayer()
    {
        ResetPath();

        currentNode = nodeMap[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z)];
        goal = nodeMap[Mathf.FloorToInt(player.transform.position.x), Mathf.FloorToInt(player.transform.position.z)];

        return FindPath(currentNode.position, goal.position);
    }

    private void Update()
    {
        if (gameObject.GetComponent<MockEnemy>().Health > 0 && !gameObject.GetComponent<MockEnemy>().isAttacking)
        {
            if (gameObject != null && !process && (gameObject.transform.position - player.transform.position).magnitude <= GOTOPLAYERDISTANCE && !selectedPlayer && !CBS.activeInHierarchy)
            {
                StateMachine("chase");
            }

            if (gameObject != null && !isWaiting && (gameObject.transform.position - player.transform.position).magnitude <= STARTCOMBARDISTANCE || CBS.activeInHierarchy)
            {
                StateMachine("wait");
            }

            if (gameObject != null && !process && (gameObject.transform.position - player.transform.position).magnitude >= GOTOPLAYERDISTANCE && !CBS.activeInHierarchy)
            {
                StateMachine("idle");
            }
        }
    }

    //reset position is called so the player is recentered over the grid
    private void ResetPosition(Vector3 position)
    {
        transform.position = new Vector3(Mathf.Floor(position.x) + 0.5f, 0.0f, Mathf.Floor(position.z) + 0.5f);
    }

    //reset path clears current path and resets the open and closed lists for A*
    private void ResetPath()
    {
        if (path != null && path.Count > 0)
        {
            path.Clear();
        }

        open.Clear();
        closed.Clear();
    }
}
