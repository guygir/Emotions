using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMover : MonoBehaviour
{
    public Camera mainCamera;
    public float addOn = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x,transform.position.y-mainCamera.transform.position.y+ addOn, transform.position.z);
    }
}
