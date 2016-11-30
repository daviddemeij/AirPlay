using UnityEngine;
using System.Collections;

public class buildMesh : MonoBehaviour {

    // Use this for initialization
    void Start() {



    }
    public void setVertices(Vector3[] vertices) {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;

        // triangles
        int nr_triangles = vertices.Length-1;
        int[] triangles = new int[nr_triangles*3];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(1, 1);
        }
        for (int i = 0; i < nr_triangles; i++)
        {
            triangles[i * 3] = i;
            if (i == (nr_triangles - 1))
            {
                triangles[i * 3 + 1] = 0;
            }
            else
            {
                triangles[i * 3 + 1] = i + 1;
            }
            
            triangles[i * 3 + 2] = nr_triangles;

        }

        MeshCollider myMC = GetComponent<MeshCollider>();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.Optimize();
        mesh.RecalculateNormals();
        myMC.sharedMesh = mesh;
    }
	

}
