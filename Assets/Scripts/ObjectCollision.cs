using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Author@ Alan Hart
/// </summary>
public class ObjectCollision : MonoBehaviour
{
    //staic float used for determining when an object is within distance to the player
    private static float HITDISTANCE = 0.6f;
    //transparent material that will be assigned to objects that are blocking view
    public Material seeThroughMat;
    //what the objectCollision hit previously
    public GameObject previousHit;
    //if the player is blocked to the right or downwards
    public bool isTouchingDown, isTouchingRight;

    //list that stores the objects that should be made transparent. One list if for the downwards direction and the other to the right
    //all objects have their own material as well. When it changes the gameObjects to the transparent material it needs to store the old one
    public List<GameObject> obscureListDown, obscureListRight;
    public List<Material> obscureMatListDown, obscureMatListRight;

    void Start()
    {
        previousHit = gameObject;
        isTouchingDown = false;
        isTouchingRight = false;
    }

    public void CollisionDetection()
    {
        //raycasting from the player downwards and from the player to the right
        RaycastHit hit;
        Ray rayBack = new Ray(transform.position, Vector3.back);
        Ray rayRight = new Ray(transform.position, Vector3.right);

        //if the raycast hit something in the downwards direction
        if (Physics.Raycast(rayBack, out hit))
        {
            //if something it hit is within the HITDISTANCE and the hit object is not the player or tagged as "floor"
            if (hit.distance < HITDISTANCE && hit.collider.name != "ThirdPersonController" && hit.collider.tag != "floor")
            {
                //it calls a new RayCast function and passes the hit object
                isTouchingDown = true;
                RayCastFromHitObjectDown(hit.collider.gameObject);
                AddToList(hit.collider.gameObject, "Down");
            }
            else
            {
                //if the previous perameters were not met it resets the gameobjects that have transparency
                isTouchingDown = false;
                ClearList();
            }
        }
        else
        {
            //if the ray doesn't do anything it needs to reset the gameObjects that have been made transparent
            isTouchingDown = false;
            ClearList();
        }
        //this does the same thing as the previous raycast except this raycast happens in a different direction, instead of downwards it goes to the right
        if (Physics.Raycast(rayRight, out hit))
        {
            if (hit.distance < HITDISTANCE && hit.collider.name != "ThirdPersonController" && hit.collider.tag != "floor")
            {
                isTouchingRight = true;
                RayCastFromHitObjectRight(hit.collider.gameObject);
                //AddToList ensures the hit object goes to the correct list
                AddToList(hit.collider.gameObject, "Right");
            }
            else
            {
                isTouchingRight = false;
                ClearList();
            }
        }
        else
        {
            isTouchingRight = false;
            ClearList();
        }
    }

    //the gameobjects and their materials are added in the same order to the lists. When we then reset the material to its origional,
    //all of the indexes will correspond with one another
    private void AddToList(GameObject addThis, string direction)
    {
        //when this function is called a direction is passed. Based on the direction the gameObejct and its material get placed in a list.
        if (direction == "Down")
        {
            if (!CheckContents(addThis))
            {
                obscureListDown.Add(addThis);
                obscureMatListDown.Add(addThis.GetComponent<MeshRenderer>().material);
                addThis.GetComponent<MeshRenderer>().material = seeThroughMat;
            }
        }

        if (direction == "Right")
        {
            if (!CheckContents(addThis))
            {
                obscureListRight.Add(addThis);
                obscureMatListRight.Add(addThis.GetComponent<MeshRenderer>().material);
                addThis.GetComponent<MeshRenderer>().material = seeThroughMat;
            }
        }
    }

    //bool for checking if a list already contains this object already or not
    private bool CheckContents(GameObject checkThis)
    {
        return (obscureListDown.Contains(checkThis) || obscureListRight.Contains(checkThis));
    }

    //called when an object has been hit. This raycasts in the directions perpendicular to the origional raycast. The raycast is from the down direction,
    //so then this will raycast from that hit object to the left and right
    //what this does is when the player is next to an object that is blocking view, the objects next to that object also become transparent
    private void RayCastFromHitObjectDown(GameObject castFromThis)
    {
        //where the ray should come from
        Vector3 castPoint = new Vector3(castFromThis.transform.position.x, 0.0f, castFromThis.transform.position.z);
        //raycastHit information
        RaycastHit hit;
        Ray rayLeft = new Ray(castPoint, Vector3.left);     //ray to the left from castpoint
        Ray rayRight = new Ray(castPoint, Vector3.right);   //ray to the right form castpoint

        //if the ray to the left hit something
        if (Physics.Raycast(rayLeft, out hit))
        {
            //if the hit object within the HITDISTANCE (if there is an object next to it)
            if (hit.distance < HITDISTANCE)
            {
                //add this hit object to the list and make transparent
                AddToList(hit.collider.gameObject, "Down");
            } 
        }

        //same as the former but this casts the ray in the opposite direction
        if (Physics.Raycast(rayRight, out hit))
        {
            if (hit.distance < HITDISTANCE)
            {
                AddToList(hit.collider.gameObject, "Down");
            }
        }
    }

    //this function is pretty much the same as RayCastFromHitObjectDown except that this uses forward and back for its raycasting instead of left and right
    private void RayCastFromHitObjectRight(GameObject castFromThis)
    {
        Vector3 castPoint = new Vector3(castFromThis.transform.position.x, 0.0f, castFromThis.transform.position.z);
        RaycastHit hit;
        Ray rayUp = new Ray(castPoint, Vector3.forward);
        Ray rayDown = new Ray(castPoint, Vector3.back);

        if (Physics.Raycast(rayUp, out hit))
        {
            if (hit.distance < HITDISTANCE)
            {
                AddToList(hit.collider.gameObject, "Right");
            }
        }

        if (Physics.Raycast(rayDown, out hit))
        {
            if (hit.distance < HITDISTANCE)
            {
                AddToList(hit.collider.gameObject, "Right");
            }
        }
    }

    //when the player is not standing next to an object the materials are returned to origional. It does this for both directions
    public void ClearList()
    {
        //because the gameobjects and materials are added in the same order we can easily reassign them with a for loop
        if (obscureListDown.Count > 0 && !isTouchingDown)
        {
            for (int i = 0; i < obscureListDown.Count; i++)
            {
                obscureListDown[i].GetComponent<Renderer>().material = obscureMatListDown[i];
            }
            obscureListDown.Clear();
            obscureMatListDown.Clear();
        }
        else if (obscureListRight.Count > 0 && !isTouchingRight)
        {
            for (int i = 0; i < obscureListRight.Count; i++)
            {
                obscureListRight[i].GetComponent<Renderer>().material = obscureMatListRight[i];
            }
            obscureListRight.Clear();
            obscureMatListRight.Clear();
        }
    }
    
    //function used to force clear all the obscure lists for room transistion
    public void ForceClearList()
    {
        isTouchingRight = false;
        isTouchingDown = false;
        previousHit = gameObject;
        obscureListDown.Clear();
        obscureMatListDown.Clear();
        obscureListRight.Clear();
        obscureMatListRight.Clear();
    }
}
