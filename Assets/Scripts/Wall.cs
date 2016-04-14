using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author@ Alan Hart
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class Wall : MonoBehaviour {

    //grid component from the floor
    public Grid floorGrid;
    //floor gameObject
    public GameObject floorObject;
    //sizes for the wall x and y, and the grid x and z
    public int xSize, ySize, zSize, gridX;

    //vertecies vector3 array
    public Vector3[] wallVerts;

    //list of trees that it generates
    private List<GameObject> trees;

    //mesh component of gameObject
    private Mesh mesh;

    //material that will be assigned to the generated mesh
    private Material wallMat;

    //the scale of what the given material has to be
    private Vector2 materialScale;

    //direction string that will is passed as the wall's name to determine where it has to be drawn
    private string direction;

    void Awake()
    {
        //sets the gameObject to be static
        gameObject.isStatic = true;

        //gets the xSize and zSize from the grid
        xSize = GetComponentInParent<Grid>().xSize;
        zSize = GetComponentInParent<Grid>().zSize;
        gridX = xSize;
        floorObject = transform.parent.gameObject;
        floorGrid = floorObject.GetComponent<Grid>();

        //sets direction to the name of the instantiated object
        direction = gameObject.name;

        //loads the material it needs from the resources folder
        wallMat = Resources.Load<Material>("Materials/wall");

        //list of gameobjects for the generated trees. use this so we can destroy them later
        trees = new List<GameObject>();

        //sets the height of the wall. if the wall is on the far side it's supposed to be high. if it's near it needs to be low so the player can see over it
        if (direction == "front" || direction == "down")
        {
            ySize = 1;
        }
        else
        {
            ySize = 5;
        }

        NewWall();
    }

    //creates the wall
    public void NewWall()
    {
        //initializes a vector3 list with a count of 4. one for each corner of the wall's mesh
        wallVerts = new Vector3[4];
        //need a uv space equal to the same size of the vertex list
        Vector2[] uv = new Vector2[wallVerts.Length];

        //gets and sets the mesh component to a new one
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();

        //gives the newly generated mesh a name
        mesh.name = "Generated Wall Mesh";

        for (int i = 0, y = 0; y < 2; y++)
        {
            //this is a nested for loop that goes through the vector3 list of the vertecies and places them accordingly
            for (int x = 0, z = 0; x < wallVerts.Length / 2; x++, z++, i++)
            {
                //this loop bases its vector coordinates based off the direction that was provided on instantiation
                if (direction == "left")
                {
                    //if the direction was "left" it needs to place the vectors in a different order then if the direction is "down"
                    //since this wall can be drawn with just two triangles we only need to set the vertecies to the far corners. 
                    //that would be the bottom left, top left, bottom right, and top right. It does this for all directions.
                    wallVerts[i] = new Vector3(0, y * ySize, z * zSize);

                    //generate a new vector3 for the collider box of the wall, depending on the direction the variables have to change
                    Vector3 colliderSize = new Vector3(0.01f, ySize, zSize);

                    SetBoxCollider(colliderSize);
                }
                else if (direction == "down")
                {
                    wallVerts[i] = new Vector3(x * xSize, y * ySize);
                    Vector3 colliderSize = new Vector3(xSize, ySize, 0.01f);
                    SetBoxCollider(colliderSize);
                }
                else if (direction == "front")
                {
                    wallVerts[i] = new Vector3(gridX, y * ySize, z * zSize);
                    Vector3 colliderSize = new Vector3(0.01f, ySize, zSize);
                    SetBoxCollider(colliderSize);
                }
                else if (direction == "back")
                {
                    wallVerts[i] = new Vector3(x * xSize, y * ySize, zSize);
                    Vector3 colliderSize = new Vector3(xSize, ySize, 0.01f);
                    SetBoxCollider(colliderSize);
                }
                else
                {
                    Debug.LogError("NO DIRECTION SET/INPROPPER");
                }

                //as the loop is going through vertex list it also sets the UV space of the generated mesh.
                //a uv space goes from 0 to 1 on two axies. As the for loop progresses the coordinates slowly go from zero to one on both axies for every vector coordinate
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
            }
        }

        //sets the mesh component's vertecies to the new vector coordinates from the doorVerts list
        mesh.vertices = wallVerts;
        //set the uv and uv2 to the new vector2 coordinates of the UV list
        mesh.uv = uv;
        mesh.uv2 = uv;

        int[] TrianglesWall = new int[6];

        TrianglesWall[0] = 0;
        TrianglesWall[1] = 2;
        TrianglesWall[2] = 1;
        TrianglesWall[3] = TrianglesWall[1];
        TrianglesWall[4] = 3;
        TrianglesWall[5] = TrianglesWall[2];

        mesh.triangles = TrianglesWall;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = wallMat;

        if (direction == "down" || direction == "back")
        {
            materialScale = new Vector2(xSize, ySize);
        }
        else
        {
            materialScale = new Vector2(zSize, ySize);
        }

        meshRenderer.material.mainTextureScale = materialScale;

        mesh.Optimize();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        ///*
        /// this is used to make the generated walls invisible
        /// later during the development of the game it turned out that the walls were no longer needed.
        /// I left them in because I didn't want to rip apart my work but it could have worked without these walls.
        /// However, because I made this class it was easy to implement the tree placement and the random prison wall.
        ///*
        meshRenderer.enabled = false;

        PlaceWallStructures(direction);
        PlaceTrees(direction);
    }

    //sets the box collider for the generated mesh based off the direction provided
    private void SetBoxCollider(Vector3 colliderSize)
    {
        //gameObject's box collider component
        BoxCollider collider = GetComponent<BoxCollider>();
        //set the size of the box collider component to the passed collider size.
        collider.size = colliderSize;
        if (direction == "front")
        {
            //the center of the collider is based off of what the direction of the wall is
            //if the direction is "front", the center for the x position is on the far end of the floor.
            collider.center = new Vector3((float)xSize, (float)ySize / 2, (float)zSize / 2);
        }
        else if (direction == "back")
        {
            collider.center = new Vector3((float)xSize / 2, (float)ySize / 2, (float)zSize);
        }
        else if (direction == "down")
        {
            collider.center = new Vector3((float)xSize / 2, (float)ySize / 2, 0.0f);
        }
        else if (direction == "left")
        {
            collider.center = new Vector3(0.0f, (float)ySize / 2, (float)zSize / 2);
        }
    }

    //can place a prison wall like structure along the wall. It has a probability so it might not happen
    private void PlaceWallStructures(string direction)
    {
        //it instantiates the wall by loading it from the resources folder
        GameObject wall = Instantiate(Resources.Load("Prefabs/Chocolate Wall"), Vector3.zero, Quaternion.identity) as GameObject;

        //vector3 coordinate where the prison wall will be placed
        Vector3 placementLocation = Vector3.zero;
        
        //sets the parent of the wall to the the current gameObject
        wall.transform.parent = transform;

        if (direction == "front")
        {
            //random choice from 0 to 2
            int choice = Mathf.RoundToInt(Random.Range(0, 3));
            if (choice == 0)
            {
                //if the choice was 0 it will place it to the left of the door
                placementLocation = new Vector3(xSize, 0, Random.Range(1, zSize / 2));
            }
            else if (choice == 1)
            {
                //if the choice was 1 if will place it to the right of the door
                placementLocation = new Vector3(xSize, 0, Random.Range(zSize / 2 + 2, zSize));
            }
            else if (choice == 2)
            {
                //if the choice is 2 it will destroy the door and thus not place one
                Destroy(wall);
            }

            //if the choice was not 2
            if (wall != null)
            {
                wall.transform.position = placementLocation;
            }
        }
        else if (direction == "back")
        {
            int choice = Mathf.RoundToInt(Random.Range(0, 3));
            if (choice == 0)
            {
                placementLocation = new Vector3(Random.Range(1, xSize / 2), 0, zSize);
            }
            else if (choice == 1)
            {
                placementLocation = new Vector3(Random.Range(xSize / 2 + 2, xSize), 0, zSize);
            }
            else if (choice == 2)
            {
                Destroy(wall);
            }

            if (wall != null)
            {
                wall.transform.position = placementLocation;
                //because the wall is at a different angle the door needs to be rotated to match the wall direction
                wall.transform.Rotate(Vector3.up, 90.0f);
            }
        }
        else if (direction == "down")
        {
            int choice = Mathf.RoundToInt(Random.Range(0, 3));
            if (choice == 0)
            {
                placementLocation = new Vector3(Random.Range(1, xSize / 2), 0, 0);
            }
            else if (choice == 1)
            {
                placementLocation = new Vector3(Random.Range(xSize / 2 + 2, xSize), 0, 0);
            }
            else if (choice == 2)
            {
                Destroy(wall);
            }

            if (wall != null)
            {
                wall.transform.position = placementLocation;
                wall.transform.Rotate(Vector3.up, 90.0f);
            }
        }
        else if (direction == "left")
        {
            int choice = Mathf.RoundToInt(Random.Range(0, 3));
            if (choice == 0)
            {
                placementLocation = new Vector3(0, 0, Random.Range(1, zSize / 2));
            }
            else if (choice == 1)
            {
                placementLocation = new Vector3(0, 0, Random.Range(zSize / 2 + 2, zSize));
            }
            else if (choice == 2)
            {
                Destroy(wall);
            }

            if (wall != null)
            {
                wall.transform.position = placementLocation;
            }
        }
        else
        {
            Debug.LogError("NO DIRECTION SET/INPROPPER");
        }
    }

    //places trees along the wall based on the direction string
    private void PlaceTrees(string direction)
    {
        if (direction == "front")
        {
            for (int i = 0; i < zSize; i++)
            {
                //for however long the zSize is it places a new tree every unit.
                GameObject tree = Instantiate(Resources.Load("Prefabs/Tree"), Vector3.zero, Quaternion.identity) as GameObject;
                //add the new tree to a list so we can destroy it later
                trees.Add(tree);
                //set the parent to keep the hierarchy clean
                tree.transform.parent = transform;
                //give the tree a random rotation to give variation
                Vector3 randomRotation = new Vector3(0, Random.Range(-360, 360), 0);
                //this every tree one unit further for every tree it places, and on every second tree it places,
                //it adds one to the x axis to create a zigzag pattern. If a different direction is passed it add acts accordingly
                tree.transform.Translate(new Vector3(xSize + 0.5f + (i % 2), 0, i * 1 + 0.5f));
                tree.transform.Rotate(randomRotation);
            }
        }
        else if (direction == "back")
        {
            for (int i = 0; i < xSize; i++)
            {
                GameObject tree = Instantiate(Resources.Load("Prefabs/Tree"), Vector3.zero, Quaternion.identity) as GameObject;
                trees.Add(tree);
                tree.transform.parent = transform;
                Vector3 randomRotation = new Vector3(0, Random.Range(-360, 360), 0);
                tree.transform.Translate(new Vector3(0.5f + i * 1, 0, zSize + 0.5f + (i % 2)));
                tree.transform.Rotate(randomRotation);
            }
        }
        else if (direction == "down")
        {
            for (int i = 0; i < xSize; i++)
            {
                GameObject tree = Instantiate(Resources.Load("Prefabs/Tree"), Vector3.zero, Quaternion.identity) as GameObject;
                trees.Add(tree);
                tree.transform.parent = transform;
                Vector3 randomRotation = new Vector3(0, Random.Range(-360, 360), 0);
                tree.transform.Translate(new Vector3(0.5f + i * 1, 0, -0.5f - (i % 2)));
                tree.transform.Rotate(randomRotation);
            }
        }
        else if (direction == "left")
        {
            for (int i = 0; i < zSize; i++)
            {
                GameObject tree = Instantiate(Resources.Load("Prefabs/Tree"), Vector3.zero, Quaternion.identity) as GameObject;
                trees.Add(tree);
                tree.transform.parent = transform;
                Vector3 randomRotation = new Vector3(0, Random.Range(-360, 360), 0);
                tree.transform.transform.Translate(new Vector3(-0.5f - (i % 2), 0, i * 1 + 0.5f));
                tree.transform.Rotate(randomRotation);
            }
        }
        else
        {
            Debug.LogError("NO DIRECTION SET/INPROPPER");
        }

        //this sets the trees that are directly next the the doors (in the middle of the wall) to have a transparent texturem
        //it makes the doors easier to see for the player
        trees[trees.Count / 2].GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("Materials/SeeThrough");
        trees[trees.Count / 2 + 1].GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("Materials/SeeThrough");
        trees[trees.Count / 2 - 1].GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("Materials/SeeThrough");
    }
}
