using UnityEngine;
using System.Collections;

/// <summary>
/// Author@ Alan Hart
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class Water : MonoBehaviour
{
    private Material waterMaterial;
    private int xSize, zSize;
    private Vector3[] waterVerts;
    private Mesh waterMesh;

	void Start ()
    {
        waterMaterial = Resources.Load<Material>("Materials/water");
        xSize = GetComponentInParent<Grid>().xSize;
        zSize = GetComponentInParent<Grid>().zSize;
        Generate();
	}

    private void Generate()
    {
        waterVerts = new Vector3[4];
        Vector2[] waterUV = new Vector2[waterVerts.Length];

        for (int x = 0, i = 0; x < waterVerts.Length / 2; x++)
        {
            for (int y = 0; y < waterVerts.Length / 2; y++, i++)
            {
                waterVerts[i] = new Vector3(x * xSize, 0, y * zSize);
                waterUV[i] = new Vector2((float)x / xSize, (float)y / zSize);
            }
        }

        GetComponent<MeshFilter>().mesh = waterMesh = new Mesh();
        waterMesh.name = "Generated Floor Mesh";

        waterMesh.vertices = waterVerts;
        waterMesh.uv = waterUV;
        waterMesh.uv2 = waterUV;

        int[] TrianglesFloor = new int[6];
        TrianglesFloor[0] = 1;
        TrianglesFloor[1] = 2;
        TrianglesFloor[2] = 0;
        TrianglesFloor[3] = 1;
        TrianglesFloor[4] = 3;
        TrianglesFloor[5] = 2;
        waterMesh.triangles = TrianglesFloor;

        waterMesh.triangles = TrianglesFloor;

        waterMesh.RecalculateBounds();
        waterMesh.RecalculateNormals();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = waterMaterial;
        meshRenderer.material.mainTextureScale = new Vector2(xSize, zSize);

        Vector3 colliderSize = new Vector3((float)xSize, 0.01f, (float)zSize);

        BoxCollider collider = GetComponent<BoxCollider>();
        collider.size = colliderSize;
        collider.center = new Vector3(xSize / 2, 0.0f, zSize / 2);

        gameObject.transform.Translate(0.0f, -0.01f, 0.0f);

        gameObject.tag = "water";
    }
}
