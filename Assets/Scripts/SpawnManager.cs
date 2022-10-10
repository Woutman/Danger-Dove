using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabList = new List<GameObject>();
    private List<GameObject> buffer = new List<GameObject>();
    private List<Vector3> startPositions = new List<Vector3>();
    private int numObjToBuffer = 5;
    private int numIniObj = 25;
    private float xBound = 5.5f;

    private float nextPeriod = 0.0f;
    private float period = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        // Fill buffer with objects to spawn
        foreach (GameObject obj in prefabList)
        {
            for (int i = 0; i < numObjToBuffer; i++)
            {
                GameObject objToAdd = Instantiate(obj);
                objToAdd.SetActive(false);
                buffer.Add(objToAdd);
            }
        }

        for (int i = 0; i < numIniObj; i++)
        {
            startPositions.Add(new Vector3(Random.Range(-xBound, xBound), -4, -15 + i * 215 / numIniObj));
        }

        foreach (Vector3 startPos in startPositions)
        {
            GameObject objectToSpawn = buffer[Random.Range(0, buffer.Count)];
            if (!objectToSpawn.activeSelf)
            {
                objectToSpawn.transform.position = startPos;
                Vector3 objRotation = objectToSpawn.transform.rotation.eulerAngles;
                objectToSpawn.transform.rotation = Quaternion.Euler(objRotation.x, Random.Range(0, 360), objRotation.z);
                objectToSpawn.SetActive(true);
            }        
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("Game Manager").GetComponent<GameManager>().gameIsRunning)
        {
            // Spawn an object every period if object is not active yet
            if (Time.time > nextPeriod)
            {
                nextPeriod += period;
                Vector3 spawnPos = new Vector3(Random.Range(-xBound, xBound), -4, 200);
                GameObject objectToSpawn = buffer[Random.Range(0, buffer.Count)];
                if (!objectToSpawn.activeSelf)
                {
                    objectToSpawn.transform.position = spawnPos;
                    Vector3 objRotation = objectToSpawn.transform.rotation.eulerAngles;
                    objectToSpawn.transform.rotation = Quaternion.Euler(objRotation.x, Random.Range(0, 360), objRotation.z);
                    objectToSpawn.SetActive(true);
                }
            }
        }            
    }
}
