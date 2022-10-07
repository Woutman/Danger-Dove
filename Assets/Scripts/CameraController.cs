using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player.GetComponent<PlayerController>().isAlive)
        {
            Vector3 newPos = new Vector3(player.transform.position.x, 0.8f, -12);
            transform.position = newPos;
        }       
    }
}
