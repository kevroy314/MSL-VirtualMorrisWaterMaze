﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class InputValidator : MonoBehaviour {

    public InputField subIDInputField;
    public Dropdown trialDropdown;
    public string config_filepath = "configuration.ini";
    public bool relativePath = true;
    public GameObject tcpServer;

    void Start()
    {
        if (PlayerPrefs.HasKey("subid"))
            subIDInputField.text = PlayerPrefs.GetString("subid").Trim();
        if (PlayerPrefs.HasKey("trial"))
            trialDropdown.value = PlayerPrefs.GetInt("trial");
        PlayerPrefs.SetInt("iteration", 0);
        Configuration config = LoadConfiguration();
        trialDropdown.ClearOptions();
        trialDropdown.AddOptions(new List<string>(config.TrialStrings));

        if (config.EnableTCP && tcpServer != null)
            tcpServer.SetActive(true);
    }

    public Configuration LoadConfiguration()
    {
        string path = config_filepath;
        if (relativePath)
            path = Application.dataPath + "/" + config_filepath;
        path = path.Replace('/', '\\');

        Configuration config = new Configuration(path);

        string configString = Configuration.Serialize(config);

        PlayerPrefs.SetString("configuration", configString);

        return config;
    }

	public void Begin()
    {
        string subID = subIDInputField.text.Trim();
        int trial = trialDropdown.value;

        Setup(subID, trial);
        CameraFade.SetScreenOverlayColor(Color.black);
        SceneManager.LoadScene("MainRoom");
    }

    private void Setup(string subID, int trial)
    {
        PlayerPrefs.SetString("subid", subID);
        PlayerPrefs.SetInt("trial", trial);
    }

}
