using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackwards : MonoBehaviour
{
    private float speed = 50.0f;
    private float zBound = -15.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("Game Manager").GetComponent<GameManager>().gameIsRunning)
        {
            transform.Translate(Vector3.back * Time.deltaTime * speed, Space.World);
            if (transform.position.z < zBound)
            {
                gameObject.SetActive(false);
            }
        }            
    }
}
