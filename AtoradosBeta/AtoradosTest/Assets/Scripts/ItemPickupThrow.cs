using UnityEngine;
using System.Collections;

public class ItemPickupThrow : MonoBehaviour {
    int items = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision other) {
        if (other.collider.name == "Pickup") {
            Destroy(other.gameObject);
            items++;
            print("got item");
        }
    }
}
