// In-class example Feb 20, 2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceObject : MonoBehaviour
{

    // public so they can be set via the editor GUI

    public ColorMap colorMap;
    public float maxBounceHeight;
    public float minBounceHeight;
    public float incPerFrame;

    // runtime-only member variables (not public)

    // +1 for up, -1 for down
    float direction = 1;

    Material material;


    // Set good default values for variables here
    private void Reset()
    {
        maxBounceHeight = 10;
        minBounceHeight = -10;
        incPerFrame = 0.1f;
    }


    // Update is called once per frame
    void Update()
    {
        // Move the object's y position up and down
        float x = gameObject.transform.localPosition.x;
        float y = gameObject.transform.localPosition.y + direction * incPerFrame;
        float z = gameObject.transform.localPosition.z;
        if (y > maxBounceHeight) {
            direction = -1;
        } else if (y < minBounceHeight) {
            direction = 1;
        }
        gameObject.transform.localPosition = new Vector3(x, y, z);


        // If a color map has been assigned in the editor and this script is attached to an object
        // that has a MeshRenderer, then get the material used by that renderer and change the
        // color of the material based on the current height.
        if (colorMap != null) {
            if (material == null) {
                MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
                if (mr != null) {
                    material = mr.material;
                }
            }
            float data01 = (y - minBounceHeight) / (maxBounceHeight - minBounceHeight);
            Color newColor = colorMap.LookupColor(data01);
            material.color = newColor;
        }
    }
}
