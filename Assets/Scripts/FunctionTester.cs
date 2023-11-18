using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionTester : MonoBehaviour
{
    public UnityEvent function1;
    public UnityEvent function2;
    public UnityEvent function3;
    public UnityEvent function4;
    public UnityEvent function5;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            function1.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            function2.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            function3.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            function4.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha5))
            function5.Invoke();
    }
}
