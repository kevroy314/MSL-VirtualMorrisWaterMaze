﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class TrialConfigurationLoader : MonoBehaviour {

    private Configuration config;
    private int trial;
    private string subID;
    private int iteration;

    public GameObject[] orderedLandmarks;
    public GameObject player;
    public GameObject platform;
    public GameObject platformTrigger;
    public GameObject elevator;
    public GameObject flags;
    public GameObject hills;
    public TrialTimer timer;
    public GameObject logger;

    // Use this for initialization
    void Start () {
        if (PlayerPrefs.HasKey("configuration"))
            config = Configuration.Deserialize(PlayerPrefs.GetString("configuration"));
        else
             config = new Configuration();

        if (PlayerPrefs.HasKey("subid"))
            subID = PlayerPrefs.GetString("subid");
        else
            subID = "None";

        if (PlayerPrefs.HasKey("trial"))
            trial = PlayerPrefs.GetInt("trial");
        else
            trial = 0;

        if (PlayerPrefs.HasKey("iteration"))
            iteration = PlayerPrefs.GetInt("iteration");
        else
            iteration = 0;
        

        Initialize(config);
    }
	
	private void Initialize(Configuration c)
    {
        int numberOfExecutions = c.NumberOfExecutions[trial];
        //Debug.Log(iteration);
        //Debug.Log(numberOfExecutions);
        if (iteration >= numberOfExecutions)
        {
            PlayerPrefs.SetInt("iteration", 0);
            CameraFade.SetScreenOverlayColor(Color.black);
            SceneManager.LoadScene("Menu");
            return;
        }

        logger.SetActive(true);

        platform.transform.position = new Vector3(c.PlatformPositions[trial][iteration].x, platform.transform.position.y, c.PlatformPositions[trial][iteration].y);
        player.transform.position = new Vector3(c.PlayerStartPositions[trial][iteration].x, player.transform.position.y, c.PlayerStartPositions[trial][iteration].y);
        platform.transform.rotation = c.PlayerStartOrientations[trial][iteration];

        float timelimit = c.TrialTimeLimits[trial];

        for (int i = 0; i < orderedLandmarks.Length; i++)
            orderedLandmarks[i].SetActive(c.LandmarkVisibilities[trial]);

        if (c.PlatformVisibilities[trial])
            elevator.transform.position = new Vector3(elevator.transform.position.x, -1.2f, elevator.transform.position.z);

        flags.SetActive(c.FlagVisibilities[trial]);
        platformTrigger.SetActive(c.PlatformTriggerEnabled[trial]);
        hills.SetActive(c.HillVisibilities[trial]);

        timer.trialTime = c.TrialTimeLimits[trial];

        player.GetComponentInChildren<AudioListener>().enabled = c.SoundEffectsEnabled[trial];

        player.GetComponent<AugmentedController>().SetWalkSpeed(c.MovementSpeeds[trial]);
}

    public Configuration getActiveConfiguration()
    {
        return config;
    }

    public int getActiveTrialNumber()
    {
        return trial;
    }

    public string getActiveSubID()
    {
        return subID;
    }

    public void NextIteration()
    {
        iteration++;
        PlayerPrefs.SetInt("iteration", iteration);
    }
}
