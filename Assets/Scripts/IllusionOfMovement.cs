using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllusionOfMovement : MonoBehaviour
{
    private float speed = 50.0f;
    private float zSize;
    private float zBound;
    private int bufferSize = 20;

    private List<(int, GameObject)> buffer = new List<(int, GameObject)>();
    private List<Vector3> startPositions = new List<Vector3>();

    public GameObject environmentPrefab;

    // Start is called before the first frame update
    void Start()
    {
        zSize = environmentPrefab.GetComponent<BoxCollider>().size.z - 1.0f;

        for (int i = 0; i < bufferSize; i++)
        {
            GameObject newEnvironment = Instantiate(environmentPrefab);
            newEnvironment.transform.parent = gameObject.transform;
            buffer.Add((i, newEnvironment));
        }

        for (int i = 0; i < bufferSize + 1; i++)
        {
            Vector3 startPos = new Vector3(0, -4, -15 + i * zSize);
            startPositions.Add(startPos);
        }
       
        foreach ((int i, GameObject env) in buffer)
        {
            env.transform.position = startPositions[i + 1];
        }

        zBound = startPositions[0].z;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("Game Manager").GetComponent<GameManager>().gameIsRunning)
        {
            transform.Translate(Vector3.back * Time.deltaTime * speed);

            if (buffer[0].Item2.transform.position.z < zBound - zSize)
            {
                foreach ((int i, GameObject env) in buffer)
                {
                    env.transform.position = startPositions[i + 1];
                }
            }            
        }        
    }
}
