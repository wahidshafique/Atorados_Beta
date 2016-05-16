using UnityEngine;
using System.Collections;

public class PullCheck : MonoBehaviour {
    RaycastHit hit;
    bool colFlag = true;
    // Use this for initialization
    void Start() {

    }
    // Update is called once per frame
    void Update() {
        if (colFlag)
            this.gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
    void FixedUpdate() {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        //Debug.DrawRay(transform.position, fwd, Color.green, 1);
        if (Physics.Raycast(transform.position, fwd, out hit, 1)) {
            if (hit.collider.gameObject.tag == "Object") {
                colFlag = false;
                this.gameObject.GetComponent<Renderer>().material.color = Color.black;
                if (Input.GetKeyDown(KeyCode.Space)){
                    this.gameObject.AddComponent<SpringJoint>().connectedBody = hit.collider.gameObject.GetComponent<Rigidbody>();
                } else if (Input.GetKeyDown(KeyCode.X)) {
                    Destroy(this.gameObject.GetComponent<SpringJoint>());
                }
                print("pull me up");
            }
        }
    }
}
