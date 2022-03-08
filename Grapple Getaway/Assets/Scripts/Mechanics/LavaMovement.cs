using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(1, -10f, 0);
        Vector3 newPosition = transform.position; // We store the current position
        newPosition.x = 1; // We set a axis, in this case the y axis
        transform.position = newPosition; // We pass it back
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, .25f * Time.deltaTime,0);
    }
}
