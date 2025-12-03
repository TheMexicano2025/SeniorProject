using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFloat : MonoBehaviour
{

    [Header ("Floating Settings")]
    [Tooltip("How high the item goes up and down")]
    public float floatAmp = 0.3f;

    [Tooltip("How fast the item will go up and down")]
    public float floatFreq = 1f;

    [Header("Rotation Settings")]
    [Tooltip ("How fast the item spins")]
    public float rotationSpeed = 90f;

    private Vector3 startPosition;
    private float offSet;

    void Start()
    {
        startPosition = transform.position;

        offSet = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        upAndDown();
        rightAround();
    }
    
    private void upAndDown()
    {
        float newY = startPosition.y + Mathf.Sin((Time.time + offSet) * floatFreq) * floatAmp;
        
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void rightAround()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
