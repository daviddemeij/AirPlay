using UnityEngine;
using System.Collections;

public class CreatePlaneMesh : MonoBehaviour {
    public float width = 5f;
    public float height = -5f;

	// Use this for initialization
	void Start () {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        // vertices
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0,0,0),
            new Vector3(width, 0, 0),
            new Vector3(0,0,height),
            new Vector3(width,0,height)
        };
        // triangles
        int[] tri = new int[6];
        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        // Normals (to display)
        Vector3[] normals = new Vector3[4];

        normals[0] = -Vector3.right;
        normals[1] = -Vector3.right;
        normals[2] = -Vector3.right;
        normals[3] = -Vector3.right;

        // UVs
        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        // Assign Arrays
        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.normals = normals;
        mesh.uv = uv;

    }

}
