using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author@ Alan Hart
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    //list of availible floors to spawn
    public List<GameObject> floors;

    //trail of cookies that the player sees when they make a path
    public List<GameObject> floorTiles;

    //list of availible enemies to spawn
    public List<GameObject> enemies;

    //main camera
    private Camera mainCamera;

    //gameObject references
    public GameObject player, activeFloor, pointer, previousHit, floorTile, CBS;

    //animator component of the player gameObject
    private Animator playerAnimator;

    //transform of the player gameObject
    private Transform playerTransform;

    //the grid component of the current active floor
    private Grid activeGrid;

    //target and destination postitions for traveling
    public Vector3 target, destinationPosition;

    //floats used to control the ObjectCollision script on the player
    private float previousCast, pastDistance, previousMove;
    public float remainingDistance, moveSpeed, destinationDistance;

    //small pointer so the player can see what node they selected
    public GameObject instantiatedPointer;

    //transparent material that is assigned to objects that are blocking view
    public Material seeThroughMat;
    private Material tempMat;

    //used to control how often the game checks if an object is blocking view
    private bool raycastObscure, checkForObject;

    //used to control travel between rooms
    public bool traveling, inCombat;

    void Start()
    {
        playerAnimator = player.GetComponent<Animator>();
        Screen.SetResolution(1920, 1080, true);
        mainCamera = Camera.main;
        playerTransform = player.transform;
        destinationPosition = playerTransform.position;
        previousCast = 0;
        raycastObscure = false;
        traveling = false;
        inCombat = false;
        previousHit = gameObject;
        previousMove = 0;
        checkForObject = false;
    }

    //these functions always need to happen
	void Update ()
    {
        Travel();

        FollowPlayer();
    }

    //function called when the player moves to a new room. Some things need to be reset to prevent strange things from happening
    public void CancelMovement()
    {
        ResetPosition(player.transform.position);
        GetFloor().ClearPath();
        destinationDistance = 0;
        if (floorTiles != null && floorTiles.Count > 0)
        {
            for (int i = 0; i < floorTiles.Count; i++)
            {
                Destroy(floorTiles[i]);
            }
            floorTiles.Clear();
        }
    }

    //LateUpdate is at the end of the update. This function controls the camera following the player. Because it's in
    //lateUpdate the camera doesn't move unless the player actually moves. Some component have to reset the positions of the player,
    //this causes the camera to jiggle a little. By putting it in LateUpdate this doesn't happen
    void LateUpdate()
    {
        Vector3 cameraPosition = new Vector3(player.transform.position.x + 4.3f, 9.0f, player.transform.position.z - 7.3f);
        mainCamera.transform.position = cameraPosition;
    }

    //function that controls the player's movent.
    private void Travel()
    {
        //computed distance between where the player wants to go and where he is right now
        destinationDistance = Vector3.Distance(destinationPosition, playerTransform.position);

        //if the destinations is reached
        if (destinationDistance < .01f && !inCombat)
        {
            remainingDistance = 0;
            //ResetPosition is called to center the player gameObject over the grid
            ResetPosition(player.transform.position);
            //sets the playerAnimator to play the "Idle" animation
            playerAnimator.Play("Idle");
            if (activeFloor != null && activeFloor.GetComponent<Grid>().path != null && activeFloor.GetComponent<Grid>().path.Count > 0)
            {
                //clearPath is called to reset the pathfinder
                activeFloor.GetComponent<Grid>().ClearPath();
            }
            //removes the pointer that shows where the player is going to move to
            Destroy(instantiatedPointer);
            if (checkForObject)
            {
                //CollisionDetection controls the transparency material for objects that block the view. When the player
                //stops moving it still needs to do a final check of where it is standing right now
                player.GetComponent<ObjectCollision>().CollisionDetection();
                checkForObject = false;
            }
        }   //if the destination has not been reached yet
        else if (destinationDistance > .01f && !inCombat)
        {
            //plays the "Running" animation on the player
            playerAnimator.Play("Running");
            //allows the ObjectCollision script to be called
            if (!checkForObject)
            {
                checkForObject = true;
            }

            if (remainingDistance == 0)
            {
                remainingDistance = destinationDistance;
            }
        }

        //used to prevent the player from picking a new path too quickly
        previousMove += Time.deltaTime;

        //Moves the Player if the Left Mouse Button was clicked
        if (Input.GetMouseButtonUp(0) && previousMove > 0.3f && !inCombat)
        {
            //ray from the mouse position on screen to the world
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //if the ray hits something
            if (Physics.Raycast(ray, out hit))
            {
                //if the hit object has the tag "floor" and the player is not in combat
                if (hit.collider.tag == "floor" && !CBS.activeInHierarchy)
                {
                    //do the first collisiton detection as the player starts to move
                    player.GetComponent<ObjectCollision>().CollisionDetection();
                    //target vector3
                    Vector3 targetPoint = hit.point;
                    //destination vector3. the game is locked on a grid so the coordinates for floored and 0.5f is added to the x and z to center over a grid square
                    destinationPosition = new Vector3(Mathf.Floor(targetPoint.x) + 0.5f, 0, Mathf.Floor(targetPoint.z) + 0.5f);
                    //destroys the old pointer if there is one
                    if (instantiatedPointer != null)
                    {
                        DestroyObject(instantiatedPointer);
                    }
                    //creates a new pointer and places it ad the destination
                    instantiatedPointer = Instantiate(pointer, destinationPosition, Quaternion.identity) as GameObject;

                    //if the player was already traveling it needs to stop the previous move.
                    if (traveling)
                    {
                        StopAllCoroutines();
                        traveling = false;
                    }

                    //starts the coroutine that controls the movement
                    StartCoroutine(Move());
                }
            }

            previousMove = 0;
        }
    }

    private IEnumerator Move()
    {
        bool process = false;
        traveling = true;
        //path that the player will follow
        List<MapPosition> path = GetFloor().FindPath(destinationPosition);

        //if the player was already traveling we need to remove all the old tiles
        if (floorTiles != null && floorTiles.Count > 0)
        {
            for (int i = 0; i < floorTiles.Count; i++)
            {
                Destroy(floorTiles[i]);
            }
            floorTiles.Clear();
        }

        //creates the cookie path so the player can see what path they will follow
        if (path != null && path.Count > 1)
        {
            for (int i = 0; i < path.Count; i++)
            {
                //creates a new vector3 for every position in the path
                Vector3 position = new Vector3((float)path[i].xPos + 0.5f, 0.01f, (float)path[i].yPos + 0.5f);

                GameObject tile = Instantiate(floorTile, position, Quaternion.identity) as GameObject;
                //add them to a list so they can be destroyed later
                floorTiles.Add(tile);
            }

            //enables the processing of the path
            process = true;
        }

        //small wait time between the player clicking and the player gameObject moving
        yield return new WaitForSeconds(0.2f);

        while (process)
        {
            //create a new vector3 based off the path given to the player
            Vector3 movePosition = new Vector3(path[path.Count - 1].xPos + 0.5f, 0.0f, path[path.Count - 1].yPos + 0.5f);
            //rotate to the position the player is going to move to
            Quaternion targetRotation = Quaternion.LookRotation(movePosition - playerTransform.position);
            playerTransform.rotation = targetRotation;

            //while the player is not at the new movePosition the player moves towards the new position
            while (!playerTransform.position.Equals(movePosition))
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, movePosition, moveSpeed * Time.deltaTime);
                yield return new WaitForSeconds(0.01f);
            }

            //when the player has moved to the next position we can remove this from the path list
            if (floorTiles != null && floorTiles.Count > 0)
            {
                Destroy(floorTiles[floorTiles.Count - 1]);
                floorTiles.RemoveAt(floorTiles.Count - 1);
            }
            
            //if the path is empty stop processing
            if (path.Count == 0)
            {
                process = false;
            }
            else
            {
                //else just remove the last one in the list so the player can continue to the next path position
                path.RemoveAt(path.Count - 1);
            }
        }

        //when it's done processing
        traveling = false;
    }

    //determines when the CollisionDetection shouldl get called. It gets called for every unit the player moves
    private void FollowPlayer()
    {
        previousCast += Time.deltaTime;

        if (destinationDistance < remainingDistance - 0.95f)
        {
            remainingDistance = destinationDistance;
            raycastObscure = true;
        }
        else
        {
            raycastObscure = false;
        }

        if (raycastObscure)
        {
            player.GetComponent<ObjectCollision>().CollisionDetection();
        }
    }

    //when the player moves to a new room the floorTiles need to be removed
    public void ClearRoomList()
    {
        GetFloor().ClearPath();

        if (floorTiles != null && floorTiles.Count > 0)
        {
            for (int i = 0; i < floorTiles.Count; i++)
            {
                Destroy(floorTiles[i]);
            }
            floorTiles.Clear();
        }
    }

    //resets the player position and destination
    public void ResetPosition(Vector3 position)
    {
        destinationPosition = new Vector3(Mathf.Floor(position.x) + 0.5f, 0.0f, Mathf.Floor(position.z) + 0.5f);
        player.transform.position = destinationPosition;
    }

    //returns the active floor's grid component
    public Grid GetFloor()
    {
        return activeFloor.GetComponent<Grid>();
    }
}
