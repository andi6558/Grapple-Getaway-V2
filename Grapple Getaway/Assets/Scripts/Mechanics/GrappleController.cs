using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleController : MonoBehaviour
{
    // Start is called before the first frame update

    private double startTime;
    public double grappleLength = 0.3;

    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > grappleLength)
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log(collision.gameObject.ToString() + "a");
            Destroy(gameObject);
        }
    }

}
