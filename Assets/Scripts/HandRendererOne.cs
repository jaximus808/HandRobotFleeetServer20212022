using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRendererOne : MonoBehaviour
{

    public float scale;

    public GameObject LandMark; 
    public Transform[] landmarkPos = new Transform[21];
    public Vector3 Offset = new Vector3(0f, 0f, 0f); //test for now

    private bool updated = false; 
    private Vector3[] buffer;
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 21; i++)
        {
            landmarkPos[i] = Instantiate(LandMark, transform.position,Quaternion.identity).transform;
            //landmarkPos[i].position = 50f;
        }
    }

    // Update is called once per frame
    public void UpdateHands(Vector3[] newPos)
    {
        updated = true; 
        
        buffer = newPos;
        
    }

    private void FixedUpdate()
    {
        if(!updated) return;
        updated = false; 
        for(int i = 0; i < 21; i++)
        {
            Debug.Log("balls");
            Debug.Log(buffer[i]);
            Debug.Log(transform.position + scale * new Vector3(buffer[i].x, buffer[i].y * -1, buffer[i].z));
            landmarkPos[i].position = transform.position+scale* new Vector3(buffer[i].x, buffer[i].y*-1, buffer[i].z);
            Debug.Log(landmarkPos[i].position);
        }
    }
}
