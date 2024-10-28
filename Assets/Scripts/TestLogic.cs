using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestLogic : MonoBehaviour
{
    [SerializeField] private InputActionReference startTestLogicActionRef;
    [SerializeField] private GameObject whiteScreen;
    [SerializeField] private ArduinoManager arduinoManager;
    [SerializeField] private int EEGPin;
    [SerializeField] private int StimuliPin;


    // Start is called before the first frame update
    void Start()
    {
        whiteScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TEST());
        }
    }

    private void Awake()
    {
        startTestLogicActionRef.action.performed += OnInputStartTestLogic;
    }

    private void OnDestroy()
    {
        startTestLogicActionRef.action.performed -= OnInputStartTestLogic;
    }

    private void OnInputStartTestLogic(InputAction.CallbackContext context)
    {
        StartCoroutine(TestLogicCoroutine());
    }

    private IEnumerator TestLogicCoroutine()
    {
        while (true)
        {
            arduinoManager.SetPinHigh(EEGPin);
            arduinoManager.SetPinHighAndChoseWhenLow(StimuliPin, 0.1f);
            /*
            yield return new WaitForSeconds(0.05f);
            whiteScreen.SetActive(true);
            yield return new WaitForSeconds(0.250f);
            whiteScreen.SetActive(false);
            */
            float randomTime = Random.Range(3f, 4f);
            yield return new WaitForSeconds(randomTime);
        }
        
    }

    private IEnumerator TEST()
    {
        while (true)
        {
            arduinoManager.SetPinHigh(EEGPin);
            arduinoManager.SetPinHighAndChoseWhenLow(StimuliPin, 1f);
            /*
            yield return new WaitForSeconds(0.05f);
            whiteScreen.SetActive(true);
            yield return new WaitForSeconds(0.250f);
            whiteScreen.SetActive(false);
            */
            float randomTime = 2f;
            yield return new WaitForSeconds(randomTime);
        }
    }
}
