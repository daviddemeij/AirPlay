using UnityEngine;
using System.Collections;

public class texture_animation : MonoBehaviour {
    public int columns = 1;
    public int rows = 1;
    public float framesPerSecond = 10f;

    //the current frame to display
    private int index = 0;
    // Use this for initialization
    void Start () {
        StartCoroutine(updateTiling());

        //set the tile size of the texture (in UV units), based on the rows and columns
        Vector2 size = new Vector2(1f / columns, 1f / rows);
        this.GetComponent<Renderer>().material.SetTextureScale("_MainTex", size);
    }
    public void setColumns(int nrColumns)
    {
        columns = nrColumns;
        Vector2 size = new Vector2(1f / columns, 1f / rows);
        this.GetComponent<Renderer>().material.SetTextureScale("_MainTex", size);
        
        //move to the next index
        index++;
        if (index >= rows * columns)
            index = 0;

        //split into x and y indexes
        Vector2 offset = new Vector2((float)index / columns - (index / columns), //x index
                                      (index / columns) / (float)rows);          //y index

        this.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);

    }
    private IEnumerator updateTiling()
    {
        while (true)
        {
            //move to the next index
            index++;
            if (index >= rows * columns)
                index = 0;

            //split into x and y indexes
            Vector2 offset = new Vector2((float)index / columns - (index / columns), //x index
                                          (index / columns) / (float)rows);          //y index
            this.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);

            yield return new WaitForSeconds(1f / framesPerSecond);
        }

    }
}
