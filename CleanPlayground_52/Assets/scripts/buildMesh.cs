using UnityEngine;
using System.Collections;

public class buildMesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;

        // vertices
        Vector3[] vertices = new Vector3[]
        {
            //new Vector3(1,0,-1), // top left
            //new Vector3(2,0,-1), // top right
            //new Vector3(1,0,-3), // bottom left
            //new Vector3(2,0,-3), // bottom right
            new Vector3(1,0,-1),
            new Vector3(2,0,-1),
            new Vector3(2,0,-3),
            new Vector3(1,0,-3),
            new Vector3(0.5f,0,-2),
            // AVERAGE
            new Vector3(1.3f,0,-2)

        };
        print(vertices.Length);
        // triangles
        //int[vertices.Length-1] triangles = new int[vertices.Length-1]
        //{
        // first triangle
        //        0,1,5,
        //        1,2,5,
        //        2,3,5,
        //       3,4,5,
        //       4,0,5
        // second triangle
        //};
        print(vertices.Length);
        // triangles
        int nr_triangles = vertices.Length - 1;
        int[] triangles = new int[nr_triangles*3];
        Vector2[] uvs = new Vector2[nr_triangles];
        for (int i = 0; i < nr_triangles; i++)
        {
            print("ronde " + i);
            triangles[i * 3] = i;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = nr_triangles;
            print(triangles[i * 3]);
            uvs[i] = new Vector2(1, 1);
        }
        print(triangles);


        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.Optimize();
        mesh.RecalculateNormals();
	}
	
    //
	// Update is called once per frame
	void Update () {
	
	}
}
