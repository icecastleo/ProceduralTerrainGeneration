using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerCamera : MonoBehaviour {

    float speed = 10.0f;

    Vector3 target;

	// Use this for initialization
	void Start () {
        float radius = (float)GetComponentInParent<TerrainGenerator>().mapWidth / 2.0f;
        target = new Vector3(0.0f, -0.25f * radius, 0.0f);

        transform.position = new Vector3(radius, 0.4f * radius, 0.0f);
        transform.LookAt(target);
    }
	
	void Update () {
        transform.RotateAround(target, Vector3.up, Time.deltaTime * speed);
    }
}
