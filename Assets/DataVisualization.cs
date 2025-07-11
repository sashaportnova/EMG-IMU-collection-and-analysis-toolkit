using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XUGL;

public class DataVisualization : MonoBehaviour
{
    public Button LoadEMGFileButton, ReturnToMainMenuButton,
        LoadChannelLabelsButton, StartButton, PauseButton, StopButton;
    public TMP_Dropdown TrialsList;
    public TMP_Text FileLoadingStatus, currentScale;
    public TMP_InputField EMGScaleInput;
    public static float scaleEMG;
    private string selectedFile, data_path;

    //Channel Labels
    private List<float> channelNumbers = new List<float>();
    private List<string> channelLabels = new List<string>();
    public TMP_Text ch1, ch2, ch3, ch4, ch5, ch6, ch7, ch8,
        ch9, ch10, ch11, ch12, ch13, ch14, ch15, ch16;

    //Data array
    public static float[,] trial_array;
    public static float[] EMG_means;
    public static int sample = 0;

    //flags
    public static bool dataVisualizationFlag, dataVisualizationPauseFlag,
        dataVisualizationStopFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        LoadEMGFileButton.onClick.AddListener(LoadEMGFile);
        ReturnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        LoadChannelLabelsButton.onClick.AddListener(LoadChannelLabels);
        StartButton.onClick.AddListener(StartEMG);
        StopButton.onClick.AddListener(StopEMG);
        PauseButton.onClick.AddListener(PauseEMG);
        EMGScaleInput.onValueChanged.AddListener(EMGScaleChanged);
        TrialsList.onValueChanged.AddListener(TrialSelected);

        //populate the file list
        data_path = "Assets/Data/";
        PopulateDropdownWithFiles(data_path);

        //select the first entry if nothing is clicked
        selectedFile = TrialsList.options[0].text;

        StartButton.interactable = false;
        StopButton.interactable = false;
        PauseButton.interactable = false;
        
        scaleEMG = 3000000f;
        currentScale.text = "(current scale: " + scaleEMG + ")";
    }
     // BUTTON FUNCTIONS
     void LoadChannelLabels()
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
     }
     void StartEMG()
     {
        dataVisualizationFlag = true;
        dataVisualizationStopFlag = false;
        dataVisualizationPauseFlag = false;
        StartButton.interactable = false;
        StopButton.interactable = true;
        PauseButton.interactable = true;
     }

    void StopEMG()
    {
        dataVisualizationPauseFlag = false;
        dataVisualizationStopFlag = true;
        StartButton.interactable = true;
        StopButton.interactable = false;
        PauseButton.interactable = false;

        sample = 0;
    }

    void PauseEMG()
    {
        dataVisualizationPauseFlag = true;
        dataVisualizationStopFlag = false;
        StartButton.interactable = true;
        StopButton.interactable = true;
        PauseButton.interactable = false;
    }
     void LoadEMGFile()
     {
        string trialPath = data_path + selectedFile + ".txt";
        Debug.Log(trialPath);
        trial_array = ReadFile(trialPath);
        trial_array = RemoveZeroRows(trial_array);

        if (trial_array == null)
        {
            Debug.Log("Trial data were not loaded! Something is wrong.");
            FileLoadingStatus.text = "NOT LOADED";
            FileLoadingStatus.color = Color.red;
        }
        else
        {
            Debug.Log("Trial data were loaded!");
            StartButton.interactable = true;
            FileLoadingStatus.text = "LOADED";
            FileLoadingStatus.color = Color.green;

            //Calc the means of each columns
            EMG_means = EMG_meanCalc(trial_array);
            Debug.Log(string.Join("\t", EMG_means));
        }
     }
    void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    void EMGScaleChanged(string s)
    {
        scaleEMG = float.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
        currentScale.text = "(current scale: " + scaleEMG + ")";
    }
    void TrialSelected(int selectedIndex)
    {
        selectedFile = TrialsList.options[selectedIndex].text;
        Debug.Log("The selected file is: " + selectedFile);
    }

    // OTHER FUNCTIONS //
    private float[,] ReadFile(string pathFile)
    {
        // Read all lines from the file
        string[] lines = File.ReadAllLines(pathFile);

        // Early exit if no lines
        if (lines.Length == 0)
            return new float[0, 0];

        // Determine number of rows and columns
        int rowCount = lines.Length;
        string[] firstLineTokens = lines[0].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        int colCount = firstLineTokens.Length;

        float[,] data = new float[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            string[] tokens = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // Optional: throw if row length inconsistent
            if (tokens.Length != colCount)
            {
                throw new Exception($"Inconsistent column count at line {i + 1}. Expected {colCount}, got {tokens.Length}.");
            }

            for (int j = 0; j < colCount; j++)
            {
                if (float.TryParse(tokens[j], out float parsedValue))
                {
                    data[i, j] = parsedValue;
                }
                else
                {
                    throw new Exception($"Failed to parse float at row {i}, col {j}: '{tokens[j]}'");
                }
            }
        }

        return data;
    }
    void PopulateDropdownWithFiles(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogError("Directory not found: " + path);
            return;
        }

        // Only get .txt files
        string[] files = Directory.GetFiles(path, "*.txt");
        List<string> fileNames = new List<string>();

        foreach (string file in files)
        {
            string nameOnly = Path.GetFileNameWithoutExtension(file);

            // Skip files that end with "_IMU"
            if (nameOnly.EndsWith("_IMU"))
                continue;

            fileNames.Add(nameOnly);
        }

        TrialsList.ClearOptions();
        TrialsList.AddOptions(fileNames);

        Debug.Log("Dropdown populated with " + fileNames.Count + " .txt files.");
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
    private float[] EMG_meanCalc(float[,] EMGarray)
    {
        int numRows = EMGarray.GetLength(0);
        int numCols = EMGarray.GetLength(1);
        int colsToProcess = Math.Min(16, numCols);  // Limit to first 16 columns

        float[] columnMeans = new float[colsToProcess];

        for (int col = 0; col < colsToProcess; col++)
        {
            float sum = 0f;
            for (int row = 0; row < numRows; row++)
            {
                sum += EMGarray[row, col];
            }
            columnMeans[col] = sum / numRows;
        }

        return columnMeans;
    }

    private float[,] RemoveZeroRows(float[,] data)
    {
        int rowCount = data.GetLength(0);
        int colCount = data.GetLength(1);

        // Step 1: Identify non-zero rows
        List<int> nonZeroRows = new List<int>();
        for (int i = 0; i < rowCount; i++)
        {
            bool isZeroRow = true;
            for (int j = 0; j < colCount; j++)
            {
                if (data[i, j] != 0f)
                {
                    isZeroRow = false;
                    break;
                }
            }
            if (!isZeroRow)
            {
                nonZeroRows.Add(i);
            }
        }

        // Step 2: Create new array with non-zero rows only
        int newRowCount = nonZeroRows.Count;
        float[,] trimmed = new float[newRowCount, colCount];

        for (int newRow = 0; newRow < newRowCount; newRow++)
        {
            int originalRow = nonZeroRows[newRow];
            for (int j = 0; j < colCount; j++)
            {
                trimmed[newRow, j] = data[originalRow, j];
            }
        }

        return trimmed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (dataVisualizationFlag)
        {
            if (dataVisualizationStopFlag)
            {

            }
            else
            {
                if (dataVisualizationPauseFlag == false)
                {
                    sample = sample + 1;
                }
                else
                {
                    // do not update sample
                }
            }
            
        }

    }
}
