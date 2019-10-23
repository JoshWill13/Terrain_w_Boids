using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {
    public float speed = 100;
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
    //Arrow keys for movement
    //r and f for movement accros y axis
    //a snd d for panning sideways
    //w and s for vertical rotation
	void Update () {
        if (Input.GetKey(KeyCode.RightArrow)/*|| Input.GetKey(KeyCode.D)*/)
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0)/*, Space.World*/);
        }
        if (Input.GetKey(KeyCode.LeftArrow)/* || Input.GetKey(KeyCode.A)*/)
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0)/*, Space.World*/);
        }
        if (Input.GetKey(KeyCode.DownArrow)/* || Input.GetKey(KeyCode.S)*/)
        {
            transform.Translate(new Vector3(0, 0, -speed * Time.deltaTime)/*, Space.World*/);
        }
        if (Input.GetKey(KeyCode.UpArrow)/* || Input.GetKey(KeyCode.W)*/)
        {
            transform.Translate(new Vector3(0, 0, speed * Time.deltaTime)/*, Space.World*/);
        }
        if (Input.GetKey(KeyCode.F))
        {
            transform.Translate(new Vector3(0, -speed/2 * Time.deltaTime, 0), Space.World);
        }
        if (Input.GetKey(KeyCode.R))
        {
            transform.Translate(new Vector3(0, speed/2 * Time.deltaTime, 0), Space.World);
        }
        if (Input.GetKey(KeyCode.A)) {
            transform.Rotate(0, -speed * Time.deltaTime, 0, Space.World);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, speed * Time.deltaTime, 0, Space.World);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(-speed/2 * Time.deltaTime, 0, 0/*, Space.World*/);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(speed/2 * Time.deltaTime, 0, 0/*, Space.World*/);
        }
    }
}
