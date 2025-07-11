using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataCollection : MonoBehaviour
{
    public Button ConnectDelsys, DisconnectDelsys, StartData, StopData, LoadChannelLabels,
        ReturnToMainMenuButton;
    public TMP_InputField TrialLabelInput, EMGScaleInput;
    public TMP_Text ConnectionStatus, ChannelLabelStatus;
    public static float scaleEMG;

    //Delsys
    private EMG_Delsys EMGobject_Delsys;
    private IMU_Delsys IMUobject_Delsys;
    private Thread IMUThread = null;
    private Thread EMGThread = null;
    private Delsys_TCPIP Delsys_TCPIP = null;
    public static bool connectToDelsys;

    //Saving stuff
    private string pathEMG, trialName, pathIMU;
    private StreamWriter swrawEMG, swrawIMU;
    public static bool dataCollectionFlag = false;

    //Channel Labels
    private List<float> channelNumbers = new List<float>();
    private List<string> channelLabels = new List<string>();
    public TMP_Text ch1, ch2, ch3, ch4, ch5, ch6, ch7, ch8,
        ch9, ch10, ch11, ch12, ch13, ch14, ch15, ch16;

    public static bool EMGviz = true;

    public TMP_Text currentScale;
    void Start()
    {
        ConnectDelsys.onClick.AddListener(ConnectDelsysFunction);
        DisconnectDelsys.onClick.AddListener(DisconnectDelsysFunction);
        StartData.onClick.AddListener(StartDataCollection);
        StopData.onClick.AddListener(StopDataCollection);
        StopData.onClick.AddListener(StopDataCollection);
        LoadChannelLabels.onClick.AddListener(LoadChannelLabelsFunction);
        TrialLabelInput.onValueChanged.AddListener(TrialLabelChanged);
        EMGScaleInput.onValueChanged.AddListener(EMGScaleChanged);
        ReturnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);

        DisconnectDelsys.interactable = false;
        StartData.interactable = false;
        StopData.interactable = false;
        TrialLabelInput.interactable = false;

        scaleEMG = 1000000f;
        currentScale.text = "(current scale: " + scaleEMG + ")";
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    private void LoadChannelLabelsFunction()
    {
        string path = "Assets/ChannelLabels.txt";
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        channelNumbers.Clear();
        channelLabels.Clear();

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            // Always try to parse the number
            if (float.TryParse(parts[0], out float number))
            {
                channelNumbers.Add(number);

                // Add label if it exists, otherwise add empty string
                if (parts.Length > 1)
                    channelLabels.Add(parts[1]);
                else
                    channelLabels.Add("");  // empty label
            }
            else
            {
                Debug.LogWarning("Failed to parse line: " + line);
            }
        }

        Debug.Log("Loaded " + channelLabels.Count + " labels.");
        UpdateLabels();

        ChannelLabelStatus.text = "LOADED";
        ChannelLabelStatus.color = Color.green;
    }

    private void UpdateLabels()
    {
        TMP_Text[] channels = new TMP_Text[]
        {
        ch1, ch2, ch3, ch4, ch5, ch6, ch7, ch8,
        ch9, ch10, ch11, ch12, ch13, ch14, ch15, ch16
        };

        for (int i = 0; i < channels.Length; i++)
        {
            if (channels[i] != null)
            {
                string label = (i < channelLabels.Count && !string.IsNullOrWhiteSpace(channelLabels[i]))
                    ? channelLabels[i]
                    : $"CH{i + 1}"; // fallback label

                channels[i].text = label;
            }
            else
            {
                Debug.LogWarning($"Channel ch{i + 1} is not assigned.");
            }
        }
    }

    private void ConnectDelsysFunction()
    {
        Delsys_TCPIP = new Delsys_TCPIP();
        Delsys_TCPIP.Main();

        if (Delsys_TCPIP.connected)
        {
            ConnectionStatus.text = "CONNECTED";
            ConnectionStatus.color = Color.green;
            DisconnectDelsys.interactable = true;
            ConnectDelsys.interactable = false;
            connectToDelsys = true;
            TrialLabelInput.interactable = true;
        }
        else
        {
            ConnectionStatus.text = "DISCONNECTED";
            ConnectionStatus.color = Color.red;
            DisconnectDelsys.interactable = false;
            ConnectDelsys.interactable = true;
            connectToDelsys = false;
            TrialLabelInput.interactable = false;
        }
    }
    private void DisconnectDelsysFunction()
    {
        StartCoroutine(checkIfDelsysDone());
        Delsys_TCPIP.CloseConnection();
    }
    IEnumerator checkIfDelsysDone()
    {
        while (dataCollectionFlag)
            yield return null;
        StopDataCollection();
    }
    private void StartDataCollection()
    {
        DateTime localDate = DateTime.Now;
        pathEMG = "Assets/Data/" + trialName + ".txt";
        swrawEMG = File.CreateText(pathEMG);

        pathIMU = "Assets/Data/" + trialName + "_IMU.txt";
        swrawIMU = File.CreateText(pathIMU);

        IMUobject_Delsys = new IMU_Delsys();
        IMUThread = new Thread(IMUobject_Delsys.Main);
        IMUThread.Start();

        EMGobject_Delsys = new EMG_Delsys();
        EMGThread = new Thread(EMGobject_Delsys.Main);
        EMGThread.Start();
        UnityEngine.Debug.Log("main thread: Starting Delsys thread...");
        Delsys_TCPIP.SendCommand("START");
        dataCollectionFlag = true;

        StopData.interactable = true;
        StartData.interactable = false;
    }
    private void StopDataCollection()
    {
        dataCollectionFlag = false;
        Delsys_TCPIP.SendCommand("STOP");
        EMGThread.Abort();

        while (EMG_Delsys.EMG_queue.TryDequeue(out string s_raw))
        {
            swrawEMG.WriteLine(s_raw);
            // UnityEngine.Debug.Log("I'm writing EMG Data");
        }
        swrawEMG.Close();

        while (IMU_Delsys.IMU_queue.TryDequeue(out string imu_raw))
        {
            swrawIMU.WriteLine(imu_raw);
        }
        swrawIMU.Close();

        trialName = null;
        StartData.interactable = true;
        StopData.interactable = false;
    }
    void TrialLabelChanged(string s)
    {
        trialName = s;
        checkTrialLabelExists();
    }
    void EMGScaleChanged(string s)
    {
        scaleEMG = float.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
        currentScale.text = "(current scale: " + scaleEMG + ")";
    }
    private void checkTrialLabelExists()
    {
        if (trialName == null)
        {
            StartData.interactable = false;
        }
        else
        {
            StartData.interactable = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dataCollectionFlag)
        {
            while (EMG_Delsys.EMG_queue.TryDequeue(out string s_raw))
            {
                swrawEMG.WriteLine(s_raw);
            }

            while (IMU_Delsys.IMU_queue.TryDequeue(out string imu_raw))
            {
                swrawIMU.WriteLine(imu_raw);
            }
        }
    }
    private void OnApplicationQuit()
    {
        StartCoroutine(checkIfDelsysDone());
        Delsys_TCPIP.CloseConnection();
        UnityEngine.Debug.Log("Application ending");
    }
}
