using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author@ Alan Hart
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class Door : MonoBehaviour {

    //static float used to refrain the player from teleporting to a new room too quickly
    private static float TIMEWAIT = 3.0f;

    //sizes of the door, its parent wall and the floors' width
    private int xSize, ySize, wallSize, gridX;

    //the wall mesh component of the parent
    private Wall wall;

    //gameObject's mesh component
    private Mesh mesh;

    //vertecies vector3 array
    public Vector3[] doorVerts;

    //gameObject references
    public GameObject floor, CBS;

    //material that will be assigned to the door
    public Material doorMaterial;

    //the scale of what the given material has to be
    private Vector2 materialScale;

    //UV coordinated for the generated mesh
    private Vector2[] uv;

    //PlayerMovement component from the main camera
    private PlayerMovement playerInteraction;

    //float used to refrain the player from teleporting to a new room too quickly
    private float justTraveled;

    void Awake()
    {
        //instantiating all of my needed variables
        justTraveled = 0;
        gameObject.isStatic = true;
        gameObject.transform.localPosition = Vector3.zero;
        GetComponent<BoxCollider>().isTrigger = true;
        wall = transform.parent.GetComponent<Wall>();
        floor = transform.parent.transform.parent.gameObject;
        CBS = Camera.main.GetComponent<PlayerMovement>().CBS;
        playerInteraction = Camera.main.GetComponent<PlayerMovement>();
        
        //sets the wallsize based off of what the given tag was
        if (tag == "down" || tag == "back")
        {
            wallSize = wall.xSize;
        }
        else
        {
            wallSize = wall.zSize;
        }

        //door sizes
        xSize = 1;
        ySize = 3;
        gridX = wall.xSize;

        Generate();
    }

    //this generates the door
    private void Generate()
    {
        //create a list that holds all the needed vector3 coordinates for the vertecies
        doorVerts = new Vector3[4];
        //need another list of vector 2 coordinates that is the same length as the vertecies' vector 3 list
        uv = new Vector2[doorVerts.Length];

        //creates a new mesh component insuring a new mesh object and asigns it to mesh
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        //give our newly generated mesh a name
        mesh.name = "Generated Door";
        
        //this is a nested for loop that goes through the vector3 list of the vertecies and places them accordingly
        for (int i = 0, y = 0; y < 2; y++)
        {
            for (int x = 0; x < doorVerts.Length / 2; x++, i++)
            {
                //this loop bases its vector coordinates based off the tag that was provided on instantiation
                if (tag == "down")
                {
                    //if the tag provided was "down" then we know what wall this door belongs to. So then we can allign this door with the wall.
                    //the following loops are for the other directions.
                    //becuase the door is very small we want to put it in the middle of our walls. We do this simply by deviding the wallsize in half and then adding X * the xSize of the door.
                    //on one of the axies I've given them a small offset of 0.01f to prevent texture glitching
                    doorVerts[i] = new Vector3(wallSize / 2 + (x * xSize), y * ySize, -0.01f); 
                }
                else if (tag == "front")
                {
                    doorVerts[i] = new Vector3(gridX + 0.01f, y * ySize, wallSize / 2 + (x * xSize));
                }
                else if (tag == "left")
                {
                    doorVerts[i] = new Vector3(0.01f, y * ySize, wallSize / 2 + (x * xSize));
                }
                else if (tag == "back")
                {
                    doorVerts[i] = new Vector3(wallSize / 2 + (x * xSize), y * ySize, wall.zSize - 0.01f);
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

        //create a new vector3 to set as the collider box size
        Vector3 colliderSize = new Vector3((float)xSize / 2.0f, ySize, 0.5f);
        SetBoxCollider(colliderSize);

        //sets the mesh component's vertecies to the new vector coordinates from the doorVerts list
        mesh.vertices = doorVerts;
        //set the uv and uv2 to the new vector2 coordinates of the UV list
        mesh.uv = uv;
        mesh.uv2 = uv;

        //creates an integer array equal to the ammount of connections needed to generate this mesh
        int[] trianglesDoor = new int[6];

        //hardcoded because it was only 2 triangles that needed to be generated
        trianglesDoor[0] = 0;
        trianglesDoor[1] = 2;
        trianglesDoor[2] = 1;
        trianglesDoor[3] = trianglesDoor[1];
        trianglesDoor[4] = 3;
        trianglesDoor[5] = trianglesDoor[2];

        mesh.triangles = trianglesDoor;

        //fixes lighting
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    //sets the box collider for the generated mesh based off the tag provided
    private void SetBoxCollider(Vector3 colliderSize)
    {
        //gameObject's box collider component
        BoxCollider collider = GetComponent<BoxCollider>();
        //set the size of the box collider component to the passed collider size.
        collider.size = colliderSize;
        if (tag == "front")
        {
            //this collider is used as a trigger to move from room to room so its used as a collider but rather as a trigger,
            //therefore it needs to be offset so the player can walk into it. Based off the tag this gameObject has it places the center of the collider.
            //it always gets its center from the vector coordinate of the first vertex in the doorVerts list.
            collider.center = new Vector3(doorVerts[0].x + (float)xSize / 2 - 1.01f, doorVerts[0].y + (float)ySize / 2, doorVerts[0].z + ((float)xSize / 2));
        }
        else if (tag == "back")
        {
            collider.center = new Vector3(doorVerts[0].x + (float)xSize / 2, doorVerts[0].y + (float)ySize / 2, doorVerts[0].z + ((float)xSize / 2) - 0.99f);
        }
        else if (tag == "down")
        {
            collider.center = new Vector3(doorVerts[0].x + (float)xSize / 2, doorVerts[0].y + (float)ySize / 2, doorVerts[0].z + ((float)xSize / 2) + 0.01f);
        }
        else if (tag == "left")
        {
            collider.center = new Vector3(doorVerts[0].x + (float)xSize / 2 - 0.01f, doorVerts[0].y + (float)ySize / 2, doorVerts[0].z + ((float)xSize / 2));
        }
    }

    //if the player touches a trigger attached to the door
    private void OnTriggerStay()
    {
        //it will first check to see if the player hasn't traveles recently or if they are not walking or if they are in combat or not
        if (justTraveled > TIMEWAIT && !playerInteraction.traveling && !CBS.activeInHierarchy)
        {
            //holds on to the this room so it can access components from it
            GameObject trackPrevious = playerInteraction.activeFloor;

            //calls the ForceClearList function from the ObjectCollision on the player. This is used to refrain the game from getting null
            //refference exceptions due to lists not being populated
            playerInteraction.player.GetComponent<ObjectCollision>().ForceClearList();

            //ClearRoomList clears the tile path that is drawn when the player clicks in the world
            playerInteraction.ClearRoomList();

            //instantiates a floor from the availible floors
            Instantiate(GetFloor(), Vector3.zero, Quaternion.identity);

            //list of the newly generated doors
            List<GameObject> newDoorList = playerInteraction.activeFloor.GetComponent<Grid>().doors;

            //if the player touches the door on the front they are teleported to a door that is opposite to them. This happens in both directions.
            if (tag == "front")
            {
                for (int i = 0; i < newDoorList.Count; i++)
                {
                    //it will search through the new door list for a door with the correct tag on it
                    if (newDoorList[i].tag == "left")
                    {
                        //when it finds this door it will get create a new vector3 coordinate based on the where the newly found door's vertecies are places. 
                        //It adds or subtracts 0.5f to place the player within the level.
                        GameObject teleportDoor = newDoorList[i];
                        Vector3 teleportToDoor = teleportDoor.GetComponent<Door>().doorVerts[0] + new Vector3(0.5f, 0.0f, 0.5f);
                        
                        //ResetPosition is called to stop the pathfinder from doing strange things
                        playerInteraction.ResetPosition(teleportToDoor);

                        //destroys the old room
                        Destroy(trackPrevious);

                        //destroys all enemies of there are any
                        foreach (GameObject enemy in playerInteraction.enemies)
                        {
                            Destroy(enemy);
                        }
                        //empties the list of enemies
                        playerInteraction.enemies.TrimExcess();
                        
                        //breaks out of the for loop to continue
                        break;
                    }
                }
            }
            else if (tag == "back")
            {
                for (int i = 0; i < newDoorList.Count; i++)
                {
                    if (newDoorList[i].tag == "down")
                    {
                        GameObject teleportDoor = newDoorList[i];
                        Vector3 teleportToDoor = teleportDoor.GetComponent<Door>().doorVerts[0] + new Vector3(0.5f, 0.0f, 0.5f);
                        playerInteraction.ResetPosition(teleportToDoor);
                        Destroy(trackPrevious);
                        foreach (GameObject enemy in playerInteraction.enemies)
                        {
                            Destroy(enemy);
                        }
                        playerInteraction.enemies.TrimExcess();

                        break;
                    }
                }
            }
            else if (tag == "down")
            {
                for (int i = 0; i < newDoorList.Count; i++)
                {
                    if (newDoorList[i].tag == "back")
                    {
                        GameObject teleportDoor = newDoorList[i];
                        Vector3 teleportToDoor = teleportDoor.GetComponent<Door>().doorVerts[0] + new Vector3(0.5f, 0.0f, -0.5f);
                        playerInteraction.ResetPosition(teleportToDoor);
                        Destroy(trackPrevious);
                        foreach (GameObject enemy in playerInteraction.enemies)
                        {
                            Destroy(enemy);
                        }
                        playerInteraction.enemies.TrimExcess();

                        break;
                    }
                }
            }
            else if (tag == "left")
            {
                for (int i = 0; i < newDoorList.Count; i++)
                {
                    if (newDoorList[i].tag == "front")
                    {
                        GameObject teleportDoor = newDoorList[i];
                        Vector3 teleportToDoor = teleportDoor.GetComponent<Door>().doorVerts[0] + new Vector3(-0.5f, 0.0f, 0.5f);
                        playerInteraction.ResetPosition(teleportToDoor);
                        Destroy(trackPrevious);
                        foreach (GameObject enemy in playerInteraction.enemies)
                        {
                            Destroy(enemy);
                        }
                        playerInteraction.enemies.TrimExcess();

                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        //timer for restraining travel
        justTraveled += Time.deltaTime;
    }

    //function that picks a random floor from the floor list and returns it
    private GameObject GetFloor()
    {
        GameObject pickedFloor = playerInteraction.floors[Random.Range(0, playerInteraction.floors.Count - 1)];

        return pickedFloor;
    }
    
}
