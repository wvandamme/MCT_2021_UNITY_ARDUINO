
using System;
using Arduino;
using UnityEngine;

public class ArduinoController : MonoBehaviour
{

    public float speed = 20.0f;
    
    private Driver driver = new Driver();
    private float faktor;
    
    void OnEnable()
    {
        faktor = 0.0f;
        driver.Start();
        driver.SetCommandListener(delegate(Driver.Command command)
        {
            switch (command)
            {
                case Driver.Command.GoLeft:
                    faktor = -1.0f;
                    break;
                case Driver.Command.GoRight:
                    faktor = 1.0f;
                    break;
                case Driver.Command.Stop:
                    faktor = 0.0f;
                    break;
            }
        });
    }

    void OnDisable()
    {
        driver.RemoveCommandListener();
        driver.Stop();
    }

    private void Update()
    {
        driver.Tick();
        transform.Rotate(0.0f, speed * faktor * Time.deltaTime, 0.0f);
        driver.SendAngle(transform.localRotation.eulerAngles.y);
    }
    
}
