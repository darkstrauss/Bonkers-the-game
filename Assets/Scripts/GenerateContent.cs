using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Author@ Alan Hart
/// </summary>
public class GenerateContent : MonoBehaviour
{
    //list of availible gameObjects and enemies to spwan from
    public List<GameObject> availibleGameObjects;
    public List<GameObject> availibleEnemies;
    //vector3 list of where the spawned objects are placed
    public List<Vector3> takenSpots;
    public Text text;
    //grid component from the active floor
    private Grid grid;
    //playerMovement component from the main camera
    private PlayerMovement playerMovement;
    private int lastSelection = -1;
    //how meny objects and enemies it should place
    public int amountOfItems, amountOfEnemies;
    //seed
    public int SEED;

    //if the room places a fountain it should not place another
    private bool placedFountain = false;

    void Start()
    {
        playerMovement = Camera.main.GetComponent<PlayerMovement>();
        grid = GetComponent<Grid>();

        if (SEED == 0)
        {
            SEED = Random.seed;
            Random.seed = SEED;
        }
        else
        {
            Random.seed = SEED;
        }

        Generate();
        GenerateEnemies();
    }

    //generates the contents of the room
    private void Generate()
    {
        if (amountOfItems > 0)
        {
            //random selection from 0 to however meny objects are in the availible list
            int selection = Random.Range(0, availibleGameObjects.Count);

            //instantiates a new gameobject based on selection
            GameObject temp = Instantiate(availibleGameObjects[selection], Vector3.zero, Quaternion.identity) as GameObject;
            //set the parent to keep the heirarchy clean
            temp.transform.parent = gameObject.transform;
            //give the spawned object a unique name
            temp.name = "" + amountOfItems;
            temp.isStatic = true;

            //this controls at what height the object should be placed. All of the objects have their pivot on the bottom so that's why this is 0
            float heightGet = 0;

            //sets the position of the instantiated object to be somewhere within the room with the exclusion of the edge
            temp.transform.position = new Vector3(Mathf.Floor(Random.Range(1, grid.xSize - 1)) + 0.5f, heightGet, Mathf.Floor(Random.Range(1, grid.zSize - 1)) + 0.5f);

            //if lastSelection is the same as selection it should place that object next to each other
            if (lastSelection == selection)
            {
                //first it sets the position to be the same as the previously placed object
                //then it adds or subtracts one on either or both of the x axis and z axis
                temp.transform.position = takenSpots[takenSpots.Count - 1];
                temp.transform.position += new Vector3((int)Random.Range(-1, 1), 0.0f, (int)Random.Range(-1, 1));

                //this is a extra security mesure to refrain objects from spawning inside each other
                foreach (Vector3 item in takenSpots)
                {
                    //if the instantiated object has a vector position that has already been taken it adds or subtracts to move it
                    while (temp.transform.position.x == item.x && temp.transform.position.z == item.z)
                    {
                        temp.transform.position += new Vector3((int)Random.Range(-1, 1), 0.0f, (int)Random.Range(-1, 1));
                    }
                }

                //if the object gets placed out of bounds it gets destroyed
                if (temp.transform.position.x > grid.xSize || temp.transform.position.x < 0 || temp.transform.position.z > grid.zSize || temp.transform.position.z < 0)
                {
                    Destroy(temp);
                }
            }
            else if (selection == availibleGameObjects.Count - 1 && placedFountain == false)
            {
                //the fountain is placed in the last position of the availible object list. When this object gets picked it need to be in the middle of the room
                //Because the obejct is larger then one square it needs to get the positions next to it as well because otherwise the player could walk through the fountain
                temp.transform.position = new Vector3(Mathf.Floor(grid.xSize / 2) + 0.5f, heightGet, Mathf.Floor(grid.zSize / 2) + 0.5f);
                List<Vector3> neighbours = GetNeighbours(temp.transform.position);
                //if the list didn't return null it adds the neighbour spots to the takenSpots list
                if (neighbours != null)
                {
                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        takenSpots.Add(neighbours[i]);
                    }
                }
                else if (neighbours == null)
                {
                    //if the list returned null that means that there is an object in the way. It then destroys the fountain
                    Destroy(temp);
                }

                //removes the fountain from the availible list
                availibleGameObjects.RemoveAt(selection);
                placedFountain = true;
            }
            else
            {
                foreach (Vector3 item in takenSpots)
                {
                    while (item.x == temp.transform.position.x && item.z == temp.transform.position.z)
                    {
                        temp.transform.position = new Vector3(Mathf.Floor(Random.Range(1, grid.xSize - 1)) + 0.5f, heightGet, Mathf.Floor(Random.Range(1, grid.zSize - 1)) + 0.5f);
                    }
                }
            }

            //gives all the spawned obejct a random rotation except the fountain
            if (selection != availibleGameObjects.Count - 1)
            {
                Vector3 randomRotation = new Vector3(0, Random.Range(-360, 360), 0);
                temp.transform.Rotate(randomRotation);
            }

            //sets the last selection to the current selection
            lastSelection = selection;
            //count the amountOfItems downwards so the script will eventually stop
            amountOfItems--;

            //if the object was placed successfully it adds its vector coordinates to the takenSpots list
            if (temp != null && temp.transform.position.x < grid.xSize && temp.transform.position.x > 0 && temp.transform.position.z < grid.zSize && temp.transform.position.z > 0)
            {
                takenSpots.Add(temp.transform.position);
            }
            
            //when the script is done populating it needs to assign the takenSpots to be non-walkable tiles
            if (amountOfItems == 0)
            {
                grid.OccupySpots(takenSpots);
            }

            //recursive call so the generation happens instantly
            Generate();
        }
    }

    //picks random enemies from the availible enemy list and places them within the bounds of the map
    private void GenerateEnemies()
    {
        if (amountOfEnemies > 0)
        {
            int selection = (int)Random.Range(0, availibleEnemies.Count);

            Vector3 placement = new Vector3(Mathf.Floor(Random.Range(0, grid.xSize)) + 0.5f, 0.0f, Mathf.Floor(Random.Range(0, grid.zSize)) + 0.5f);
            GameObject enemy = Instantiate(availibleEnemies[selection], placement, Quaternion.identity) as GameObject;
            playerMovement.enemies.Add(enemy);
            
            //count down so the function will eventually stop
            amountOfEnemies--;
            //recursive call so the generation happens instantly
            GenerateEnemies();
        }
    }

    //get the neighbouring positions of the passed position
    private List<Vector3> GetNeighbours(Vector3 currentPosition)
    {
        bool taken = false;
        List<Vector3> neighbours = new List<Vector3>();

        Vector3 up = new Vector3(currentPosition.x, 0.0f, currentPosition.z + 1.0f);        //upwards
        Vector3 down = new Vector3(currentPosition.x, 0.0f, currentPosition.z - 1.0f);      //downwards
        Vector3 right = new Vector3(currentPosition.x + 1.0f, 0.0f, currentPosition.z);     //right
        Vector3 left = new Vector3(currentPosition.x - 1.0f, 0.0f, currentPosition.z);      //left

        //add these coordinates to the neighbours list
        neighbours.Add(up);
        neighbours.Add(down);
        neighbours.Add(right);
        neighbours.Add(left);

        //this is a check to see if the neighbour positions are not already taken. If they are it sets taken to true and breaks
        for (int i = 0; i < takenSpots.Count; i++)
        {
            if (takenSpots[i].x == right.x || takenSpots[i].x == left.x)
            {
                taken = true;
                break;
            }
            else if (takenSpots[i].z == up.z || takenSpots[i].z == down.z)
            {
                taken = true;
                break;
            }
        }

        //if taken is true then the function will return null, else it will return the neighbour list
        if (taken)
        {
            return null;
        }
        else
        {
            return neighbours;
        }
        
    }
}
