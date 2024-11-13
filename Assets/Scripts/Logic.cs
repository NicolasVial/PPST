using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uduino;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Logic : MonoBehaviour
{
    [SerializeField] private ArduinoManager arduinoManager;
    [SerializeField] TaskLogger taskLoggerOddball;
    [SerializeField] TextMeshProUGUI timerTxt;
    [SerializeField] TaskLogger taskLoggerTrials;

    private bool startTrials = false;
    private bool startNewTrial = true;
    private float startTrialTime = 0.0f;

    // Trial definitions and counts
    private enum TrialType
    {
        VISUOTACTILE,
        VISUAL_ONLY,
        TACTILE_ONLY,
        ODDBALL
    };

    // Structure to hold trial information
    private struct Trial
    {
        public TrialType type; // Type of the trial
        public bool near;      // true for near, false for far
        public int delay;     // delay between LED flash and tactile stimulation
        public int id;

        public Trial(TrialType type, bool near, int delay, int id)
        {
            this.type = type;
            this.near = near;
            this.delay = delay;
            this.id = id;
        }
    };

    // Trial configuration: 900 visuotactile, 100 visual only, 50 tactile only, 100 oddball
    private int visuotactileTrials = 900;
    private int visualOnlyTrials = 100;
    private int tactileOnlyTrials = 50;
    private int oddballTrials = 100;
    //const int totalTrials = visuotactileTrials + visualOnlyTrials + tactileOnlyTrials + oddballTrials;
    private int totalTrials = 1150;
    private int blockNb = 0;
    private float startOddballTime = 0.0f;
    private bool needToPress = false;
    private bool startTimer = false;
    private float timer;

    // Array to store all trial configurations
    private Trial[] block1;
    private Trial[] block2;
    private Trial[] block3;
    private Trial[] block4;
    private Trial[] block5;
    private Trial[] block6;
    private Trial[] block7;
    private Trial[] block8;
    private Trial[] block9;
    private Trial[] block10;
    private Trial[] actualTrials;
    private int currentTrialIndex = 0;

    /**
     * 1 == visuotactile near -500 (1 is LED, 101 is Tactile)
     * 2 == visuotactile near -250 (2 is LED, 102 is Tactile)
     * 3 == visuotactile near -125 (3 is LED, 103 is Tactile)
     * 4 == visuotactile near -62.5 (4 is LED, 104 is Tactile)
     * 5 == visuotactile near 0 (5 only, both tactile and LED at the same time)
     * 6 == visuotactile near 62.5 (6 is LED, 106 is Tactile)
     * 7 == visuotactile near 125 (7 is LED, 107 is Tactile)
     * 8 == visuotactile near 250 (8 is LED, 108 is Tactile)
     * 9 == visuotactile near 500 (9 is LED, 109 is Tactile)
     * 10 == visuotactile far -500 (10 is LED, 110 is Tactile)
     * 11 == visuotactile far -250 (11 is LED, 111 is Tactile)
     * 12 == visuotactile far -125 (12 is LED, 112 is Tactile)
     * 13 == visuotactile far -62.5 (13 is LED, 113 is Tactile)
     * 14 == visuotactile far 0 (14 only, both tactile and LED at the same time)
     * 15 == visuotactile far 62.5 (15 is LED, 115 is Tactile)
     * 16 == visuotactile far 125 (16 is LED, 116 is Tactile)
     * 17 == visuotactile far 250 (17 is LED, 117 is Tactile)
     * 18 == visuotactile far 500 (18 is LED, 118 is Tactile)
     * 19 == visual only near
     * 20 == visual only far
     * 21 == tactile only
     * 22 == oddball
     **/

    // Start is called before the first frame update
    void Start()
    {
        taskLoggerOddball.StartTaskLogging();
        taskLoggerTrials.StartTaskLogging();
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        block1 = new Trial[totalTrials / 10];
        block2 = new Trial[totalTrials / 10];
        block3 = new Trial[totalTrials / 10];
        block4 = new Trial[totalTrials / 10];
        block5 = new Trial[totalTrials / 10];
        block6 = new Trial[totalTrials / 10];
        block7 = new Trial[totalTrials / 10];
        block8 = new Trial[totalTrials / 10];
        block9 = new Trial[totalTrials / 10];
        block10 = new Trial[totalTrials / 10];
        actualTrials = new Trial[totalTrials / 10];
        // Populate trial array with all combinations
        int index1 = 0;
        int index2 = 0;
        int index3 = 0;
        int index4 = 0;
        int index5 = 0;
        int index6 = 0;
        int index7 = 0;
        int index8 = 0;
        int index9 = 0;
        int index10 = 0;
        int[] delays = { -5000, -2500, -1250, -625, 0, 625, 1250, 2500, 5000 };
        // 900 visuotactile trials
        int id = 0;
        for (int d = 0; d < 2; d++)
        {  // near/far
            if(d == 0)
            {
                id = 1;
            }
            else
            {
                id = 10;
            }
            for (int delay = 0; delay < 9; delay++)
            { // 9 delays
                if (d == 0)
                {
                    id = 1+delay;
                }
                else
                {
                    id = 10 + delay;
                }
                for (int rep = 0; rep < 50; rep++)
                {    // 50 repetitions
                    if(rep < 5)
                        block1[index1++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 10)
                        block2[index2++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 15)
                        block3[index3++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 20)
                        block4[index4++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 25)
                        block5[index5++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 30)
                        block6[index6++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 35)
                        block7[index7++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 40)
                        block8[index8++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else if(rep < 45)
                        block9[index9++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                    else
                        block10[index10++] = new Trial(TrialType.VISUOTACTILE, d == 0, delays[delay], id);
                }
            }
        }
        
        // 100 visual only trials
        for (int d = 0; d < 2; d++)
        {   // near/far
            if (d == 0)
            {
                id = 19;
            }
            else
            {
                id = 20;
            }
            for (int rep = 0; rep < 50; rep++)
            {
                if(rep < 5)
                {
                    block1[index1++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 10)
                {
                    block2[index2++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 15)
                {
                    block3[index3++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 20)
                {
                    block4[index4++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 25)
                {
                    block5[index5++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 30)
                {
                    block6[index6++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 35)
                {
                    block7[index7++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 40)
                {
                    block8[index8++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else if(rep < 45)
                {
                    block9[index9++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }else
                {
                    block10[index10++] = new Trial(TrialType.VISUAL_ONLY, d == 0, 0, id);
                }
            }
        }

        // 50 tactile only trials
        id = 21;
        for (int rep = 0; rep < 50; rep++)
        {
            if (rep < 5)
            {
                block1[index1++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 10)
            {
                block2[index2++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 15)
            {
                block3[index3++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 20)
            {
                block4[index4++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 25)
            {
                block5[index5++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 30)
            {
                block6[index6++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 35)
            {
                block7[index7++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 40)
            {
                block8[index8++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else if (rep < 45)
            {
                block9[index9++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
            else
            {
                block10[index10++] = new Trial(TrialType.TACTILE_ONLY, true, 0, id);
            }
        }

        // 100 oddball trials
        id = 22;
        for (int rep = 0; rep < 100; rep++)
        {
            if(rep < 10)
            {
                block1[index1++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 20)
            {
                block2[index2++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 30)
            {
                block3[index3++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 40)
            {
                block4[index4++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 50)
            {
                block5[index5++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 60)
            {
                block6[index6++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 70)
            {
                block7[index7++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 80)
            {
                block8[index8++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else if(rep < 90)
            {
                block9[index9++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }else
            {
                block10[index10++] = new Trial(TrialType.ODDBALL, true, 0, id);
            }
        }
        
        // Shuffle the trial array for random order
        shuffleTrials();

        //StartCoroutine(test());
    }

    // Update is called once per frame
    void Update()
    {
        if(startTimer)
        {
            timer += Time.deltaTime;
            timerTxt.text = "Break timer " + timer.ToString("F2");
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            UduinoManager.Instance.sendCommand("testSetPinsHigh", 122);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpaceBarPressed();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            startTrials = true;
            switch (blockNb)
            {
                case 0:
                    actualTrials = block1;
                    break;
                case 1:
                    actualTrials = block2;
                    break;
                case 2:
                    actualTrials = block3;
                    break;
                case 3:
                    actualTrials = block4;
                    break;
                case 4:
                    actualTrials = block5;
                    break;
                case 5:
                    actualTrials = block6;
                    break;
                case 6:
                    actualTrials = block7;
                    break;
                case 7:
                    actualTrials = block8;
                    break;
                case 8:
                    actualTrials = block9;
                    break;
                case 9: 
                    actualTrials = block10;
                    break;
            }
        }

        if (startTrials)
        {
            startTimer = false;
            timer = 0;
            timerTxt.text = "block " + (blockNb+1) +  " started";
            if (currentTrialIndex < actualTrials.Length)
            {
                if (startNewTrial)
                {
                    // Start new trial
                    startTrialTime = Time.time;
                    startNewTrial = false;
                    Debug.Log("Starting trial " + currentTrialIndex);
                    Debug.Log("Trial type: " + actualTrials[currentTrialIndex].type);
                    Debug.Log("Trial near: " + actualTrials[currentTrialIndex].near);
                    Debug.Log("Trial delay: " + actualTrials[currentTrialIndex].delay);

                    // Execute the current trial
                    SendExecuteTrial(actualTrials[currentTrialIndex]);
                    taskLoggerTrials.WriteToFile("block " + (blockNb + 1) + " trial " + (currentTrialIndex+1) + " type " + actualTrials[currentTrialIndex].type + " near " + actualTrials[currentTrialIndex].near + " delay " + actualTrials[currentTrialIndex].delay);
                    if (actualTrials[currentTrialIndex].type == TrialType.ODDBALL)
                    {
                        if (needToPress)
                        {
                            taskLoggerOddball.WriteToFile("miss");
                        }
                        needToPress = true;
                        startOddballTime = Time.time;
                    }
                }

                // Check if sufficient time has passed since the last trial
                if (Time.time - startTrialTime >= UnityEngine.Random.Range(3.5f, 4.5f))
                {
                    Debug.Log("Trial completed");
                    startNewTrial = true;
                    currentTrialIndex++;
                }

            }
            else
            {
                // All trials completed
                startTrials = false;
                blockNb += 1;
                currentTrialIndex = 0;
                startTimer = true;
                if (needToPress)
                {
                    taskLoggerOddball.WriteToFile("miss");
                }
            }
            

        }
    }

    private IEnumerator test()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Trial trial = new Trial(TrialType.VISUOTACTILE, true, 500, 1);
            SendExecuteTrial(trial);
            yield return new WaitForSeconds(5f);

            trial = new Trial(TrialType.VISUOTACTILE, false, -500, 1);
            SendExecuteTrial(trial);
            yield return new WaitForSeconds(5f);

            trial = new Trial(TrialType.VISUAL_ONLY, false, 0, 1);
            SendExecuteTrial(trial);
            yield return new WaitForSeconds(5f);

            trial = new Trial(TrialType.TACTILE_ONLY, true, 0, 1);
            SendExecuteTrial(trial);
            yield return new WaitForSeconds(5f);

            trial = new Trial(TrialType.VISUOTACTILE, false, 0, 1);
            SendExecuteTrial(trial);
            yield return new WaitForSeconds(5f);
        }
    }

    private void SendExecuteTrial(Trial trial)
    {
        int isNear = trial.near ? 1 : 0;
        
        // Execute the trial based on the trial type
        switch (trial.type)
        {
            case TrialType.VISUOTACTILE:
                UduinoManager.Instance.sendCommand("startTrial", 0, isNear, trial.delay, trial.id);
                break;
            case TrialType.VISUAL_ONLY:
                Debug.Log(trial.near.ToString());
                UduinoManager.Instance.sendCommand("startTrial", 1, isNear, trial.delay, trial.id);
                break;
            case TrialType.TACTILE_ONLY:
                UduinoManager.Instance.sendCommand("startTrial", 2, isNear, trial.delay, trial.id);
                break;
            case TrialType.ODDBALL:
                UduinoManager.Instance.sendCommand("startTrial", 3, isNear, trial.delay, trial.id);
                break;
            default:
                Debug.Log("Invalid trial type");
                break;
        }
    }

    private void SpaceBarPressed()
    {
        //Record spaceBar pressed by user
        float reactionTime = Time.time - startOddballTime;
        taskLoggerOddball.WriteToFile(reactionTime.ToString());
        needToPress = false;
    }

    // Shuffle function
    private void shuffleTrials()
    {
        for(int i = 0; i< block1.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block1.Length);
            Trial temp = block1[rnd];
            block1[rnd] = block1[i];
            block1[i] = temp;
        }
        for (int i = 0; i < block2.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block2.Length);
            Trial temp = block2[rnd];
            block2[rnd] = block2[i];
            block2[i] = temp;
        }
        for (int i = 0; i < block3.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block3.Length);
            Trial temp = block3[rnd];
            block3[rnd] = block3[i];
            block3[i] = temp;
        }
        for (int i = 0; i < block4.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block4.Length);
            Trial temp = block4[rnd];
            block4[rnd] = block4[i];
            block4[i] = temp;
        }
        for (int i = 0; i < block5.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block5.Length);
            Trial temp = block5[rnd];
            block5[rnd] = block5[i];
            block5[i] = temp;
        }
        for (int i = 0; i < block6.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block6.Length);
            Trial temp = block6[rnd];
            block6[rnd] = block6[i];
            block6[i] = temp;
        }
        for (int i = 0; i < block7.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block7.Length);
            Trial temp = block7[rnd];
            block7[rnd] = block7[i];
            block7[i] = temp;
        }
        for (int i = 0; i < block8.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block8.Length);
            Trial temp = block8[rnd];
            block8[rnd] = block8[i];
            block8[i] = temp;
        }
        for (int i = 0; i < block9.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block9.Length);
            Trial temp = block9[rnd];
            block9[rnd] = block9[i];
            block9[i] = temp;
        }
        for (int i = 0; i < block10.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(i, block10.Length);
            Trial temp = block10[rnd];
            block10[rnd] = block10[i];
            block10[i] = temp;
        }
    }
}
