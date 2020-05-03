using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demonstration_NoclipController : MonoBehaviour {

    public float movSpeed = 20;
    public float lookSpeed = 90;

	void Update () {

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            int scene = Mathf.CeilToInt(Input.GetTouch(0).position.x * 3 / Screen.width);
            if (scene < 1)
                scene = 1;
            else if (scene > 3)
                scene = 3;
            UnityEngine.SceneManagement.SceneManager.LoadScene("example" + scene);
        }

        if (Input.GetButtonDown("Fire3")) {
            int scene = Mathf.CeilToInt(Input.mousePosition.x * 3 / Screen.width);
            if (scene < 1)
                scene = 1;
            else if (scene > 3)
                scene = 3;
            UnityEngine.SceneManagement.SceneManager.LoadScene("example" + scene);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            UnityEngine.SceneManagement.SceneManager.LoadScene("example1");
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UnityEngine.SceneManagement.SceneManager.LoadScene("example2");
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UnityEngine.SceneManagement.SceneManager.LoadScene("example3");

        transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * lookSpeed * Time.deltaTime, Space.World);
        transform.Rotate(transform.right, Input.GetAxisRaw("Mouse Y") * -lookSpeed * Time.deltaTime, Space.World);

        transform.Translate(Vector3.right * Input.GetAxisRaw("Horizontal") * Time.deltaTime * movSpeed);
        transform.Translate(Vector3.forward * Input.GetAxisRaw("Vertical") * Time.deltaTime * movSpeed);

        transform.Translate(Vector3.up * 
            (
                (Input.GetButton("Fire1") ? movSpeed : 0) -
                (Input.GetButton("Fire2") ? movSpeed : 0)
            ) * Time.deltaTime, Space.World);
    }
}
