using UnityEngine;
using System.Collections;

public class PullCheck : MonoBehaviour {
    //this script is for both moving normal objects and also throwables
    RaycastHit hit;
    bool hasObject = false;
    bool hasThrow = false;
    bool colFlag = true;
    float pingPongVal = 0.0f;
    // Use this for initialization
    void OnGUI() {
        if (hasThrow)
            GUI.Button(new Rect(10, 10, 150, 100), pingPongVal.ToString());
    }
void Start() {

    }
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.X) && this.gameObject.GetComponent<FixedJoint>() != null) {
            hasObject = false;
            Destroy(this.gameObject.GetComponent<FixedJoint>());
        }
        if (colFlag) {
            this.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        if (hasThrow) {
            pingPongVal = Mathf.PingPong(Time.time, 10);
            if (Input.GetKeyDown(KeyCode.Space)){
                this.transform.GetChild(1).GetComponent<Rigidbody>().AddForce(transform.forward * pingPongVal * 100);
                print("RELEASE");
                this.transform.GetChild(1).transform.parent = null;
                hasThrow = !hasThrow;
            }
        }
    }
    void FixedUpdate() {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, out hit, 1)) {
            if (hit.collider.gameObject.tag == "Object" && !hasObject) {
                colFlag = false;
                this.gameObject.GetComponent<Renderer>().material.color = Color.black;
                if (Input.GetKeyDown(KeyCode.Space)) {
                    hasObject = true;
                    hit.collider.gameObject.GetComponent<Rigidbody>().freezeRotation = true;
                    this.gameObject.AddComponent<FixedJoint>().connectedBody = hit.collider.gameObject.GetComponent<Rigidbody>();
                    this.gameObject.GetComponent<FixedJoint>().enableCollision = true;
                }
                print("pull me up");
            }
        }
    }

    void OnCollisionEnter(Collision other) {//smaller ones
        if (other.transform.tag == "Throwable" && !hasObject) {
            hasThrow = true;
            print("touched throwable");
            other.transform.parent = this.transform;
            other.transform.gameObject.GetComponent<Rigidbody>().freezeRotation = true; 
        }
    }
}
