using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Logic : MonoBehaviour
{
    [SerializeField] private ArduinoManager arduinoManager;
    [SerializeField] private int StartPin = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpaceBarPressed();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(LogicCoroutine());
        }
    }

    private void Awake()
    {
    }

    private void OnDestroy()
    {
    }

    private void OnInputStartLogic(InputAction.CallbackContext context)
    {
        StartCoroutine(LogicCoroutine());
    }

    private IEnumerator LogicCoroutine()
    {
        arduinoManager.SetPinHigh(StartPin);
        yield return null;

    }

    private void SpaceBarPressed()
    {
        //Record spaceBar pressed by user
    }
}
