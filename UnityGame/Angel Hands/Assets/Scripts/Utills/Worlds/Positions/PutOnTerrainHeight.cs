using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutOnTerrainHeight : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 position =transform.position;
        int x = (int)position.x;
        int y = (int)position.z;
        transform.position = GetTerrainPos(x, y) + transform.lossyScale;
    }

    static Vector3 GetTerrainPos(float x, float y)
    {
        //Create object to store raycast data
        RaycastHit hit;

        //Create origin for raycast that is above the terrain. I chose 100.
        Vector3 origin = new Vector3(x, 1000, y);

        //Send the raycast.
        Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity);

        Debug.Log("Terrain location found at " + hit.point);

        return hit.point;
    }
}
