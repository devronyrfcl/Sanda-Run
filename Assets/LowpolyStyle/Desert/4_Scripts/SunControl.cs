using UnityEngine;
using System.Collections;
//using UnityStandardAssets.Utility;

public class SunControl : MonoBehaviour {

    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.F1)) {
	    //    GetComponent<AutoMoveAndRotate>().enabled = !GetComponent<AutoMoveAndRotate>().enabled;
            transform.eulerAngles=Vector3.zero;
	    }

        //X
        if (Input.GetKey(KeyCode.F2))
        {
            transform.Rotate(Time.deltaTime*5,0,0,Space.Self);
        }

        if (Input.GetKey(KeyCode.F3))
        {
            transform.Rotate(-Time.deltaTime * 5, 0, 0, Space.Self);
        }
    

        //Y
        if (Input.GetKey(KeyCode.F4))
        {
            transform.Rotate(0,Time.deltaTime * 5, 0, Space.Self);
        }

        if (Input.GetKey(KeyCode.F5))
        {
            transform.Rotate(0,-Time.deltaTime * 5, 0, Space.Self);
        }

        //Z

        if (Input.GetKey(KeyCode.F6))
        {
            transform.Rotate(0,0, Time.deltaTime * 5, Space.Self);
        }

        if (Input.GetKey(KeyCode.F7))
        {
            transform.Rotate(0,0, -Time.deltaTime * 5, Space.Self);
        }

    }
}
