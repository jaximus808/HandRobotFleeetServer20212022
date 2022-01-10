using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRendererOne : MonoBehaviour
{

    public float scale;

    public GameObject LandMark; 
    public Transform[] landmarkPos = new Transform[21];
    public Vector3 Offset = new Vector3(0f, 0f, 0f); //test for now

    private int[] previousAngle = new int[4] { 0, 0, 0, 0 };
    private int[] currentDifference = new int[4] { 0, 0, 0, 0 };
    private bool updated = false; 
    private Vector3[] buffer;
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 21; i++)
        {
            landmarkPos[i] = Instantiate(LandMark, transform.position,Quaternion.identity).transform;
            landmarkPos[i].gameObject.name = $"LandMark: {i}";
            //landmarkPos[i].position = 50f;
        }
    }

    // Update is called once per frame
    public void UpdateHands(Vector3[] newPos)
    {
        updated = true; 
        
        buffer = newPos;
        
    }

    public int[] CalculateRotationJoints()
    {
        for(int i = 0; i < 4; i++)
        {
            float hyp = Vector3.Distance(landmarkPos[5+i*4].position, landmarkPos[6 + i * 4].position);
            float adj = Vector3.Distance(new Vector3(landmarkPos[5 + i * 4].position.x, 0, landmarkPos[5 + i * 4].position.z), new Vector3(landmarkPos[6 + i * 4].position.x, 0, landmarkPos[6 + i * 4].position.z));
            float ang = Mathf.Acos(adj/hyp);
            
            currentDifference[i] = Mathf.Abs(previousAngle[i] - (int)ang); 
            previousAngle[i] = (int)ang;

        }
        return currentDifference; 
    }

    private void FixedUpdate()
    {
        if(!updated) return;
        updated = false; 
        for(int i = 0; i < 21; i++)
        {
            //Debug.Log("balls");
            //Debug.Log(buffer[i]);
            //Debug.Log(transform.position + scale * new Vector3(buffer[i].x, buffer[i].y * -1, buffer[i].z));
            landmarkPos[i].position = transform.position+scale* new Vector3(buffer[i].x, buffer[i].y*-1, buffer[i].z);
            //Debug.Log(landmarkPos[i].position);
        }
    }
}
