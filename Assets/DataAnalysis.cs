using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XCharts;
using XCharts.Runtime;

public class DataAnalysis : MonoBehaviour
{
    public Button LoadMVCButton, LoadTrialButton, AnalyzeDataButton,
        RerturnToMainMenuButton;
    public TMP_Dropdown TrialsList, AnalysisList;
    public TMP_Text MVCstatus;
    private string selectedFile, selectedAnalysis;

    //Data Arrays
    private float[,] MVC_array, trial_array;
    private string MVCpath, data_path;

    //Filtering
    private float fs = 2148f;
    private float[,] filteredEMG;
    private float[,] filteredMVC;
    private float[,] normalEMG;

    //Plotting
    public List<LineChart> EMGCharts = new List<LineChart>(16);
    public List<BarChart> EMGBar = new List<BarChart>(16);
    private string channelTitle;
    private List<Color> lineColorList = new List<Color>(16);

    //Labels
    private List<float> channelNumbers = new List<float>();
    private List<string> channelLabels = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        LoadMVCButton.onClick.AddListener(LoadMVC);
        LoadTrialButton.onClick.AddListener(LoadTrial);
        AnalyzeDataButton.onClick.AddListener(AnalyzeData);
        TrialsList.onValueChanged.AddListener(TrialSelected);
        AnalysisList.onValueChanged.AddListener(AnalysisSelected);
        RerturnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);

        //populate the file list
        data_path = "Assets/Data/";
        MVCpath = data_path + "MVC.txt";

        PopulateDropdownWithFiles(data_path);

        //disable all the buttons
        LoadMVCButton.interactable = false;
        LoadTrialButton.interactable = false;
        AnalyzeDataButton.interactable = false;
        TrialsList.interactable = false;
        AnalysisList.interactable = false;

        checkMVC();

        //select the first entry if nothing is clicked
        selectedFile = TrialsList.options[0].text;
        selectedAnalysis = AnalysisList.options[0].text;

        //Clear all default data
        for (int i = 0; i < 16; i++)
        {
            EMGCharts[i].ClearData();
            EMGBar[i].ClearData();
            EMGBar[i].gameObject.SetActive(false);
        }
            
        LoadChannelLabelsFunction();

        //color
        lineColorList = new List<Color> {
            // Pair 1–2: Soft Blue
            new Color(0.38f, 0.69f, 0.89f),  // EMG1
            new Color(0.38f, 0.69f, 0.89f),  // EMG2

            // Pair 3–4: Soft Orange
            new Color(0.99f, 0.75f, 0.44f),  // EMG3
            new Color(0.99f, 0.75f, 0.44f),  // EMG4

            // Pair 5–6: Soft Green
            new Color(0.65f, 0.86f, 0.58f),  // EMG5
            new Color(0.65f, 0.86f, 0.58f),  // EMG6

            // Pair 7–8: Soft Red
            new Color(0.94f, 0.50f, 0.50f),  // EMG7
            new Color(0.94f, 0.50f, 0.50f),  // EMG8

            // Pair 9–10: Lavender
            new Color(0.75f, 0.61f, 0.85f),  // EMG9
            new Color(0.75f, 0.61f, 0.85f),  // EMG10

            // Pair 11–12: Teal
            new Color(0.38f, 0.82f, 0.78f),  // EMG11
            new Color(0.38f, 0.82f, 0.78f),  // EMG12

            // Pair 13–14: Gold
            new Color(0.98f, 0.82f, 0.37f),  // EMG13
            new Color(0.98f, 0.82f, 0.37f),  // EMG14

            // Pair 15–16: Slate Gray
            new Color(0.52f, 0.60f, 0.69f),  // EMG15
            new Color(0.52f, 0.60f, 0.69f),  // EMG16
        };
        
    }
    void AnalysisSelected(int selectedIndex)
    {
        selectedAnalysis = AnalysisList.options[selectedIndex].text;
    }
    void TrialSelected(int selectedIndex)
    {
        selectedFile = TrialsList.options[selectedIndex].text;
        Debug.Log("The selected file is: " + selectedFile);
        AnalysisList.interactable = false;
        AnalyzeDataButton.interactable = false;
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

    // BUTTON FUNCTIONS
    void LoadMVC()
    {
        MVC_array = ReadFile(MVCpath);

        if (MVC_array == null)
        {
            Debug.Log("MVC data could not be loaded! Something is wrong!");
            AnalyzeDataButton.interactable = false;
            AnalysisList.interactable = false;
        }
        else
        {
            Debug.Log("MVC data has been loaded!");
            LoadTrialButton.interactable = true;
            TrialsList.interactable = true;
        }
    }
    void LoadTrial()
    {
        string trialPath = data_path + selectedFile + ".txt";
        Debug.Log(trialPath);
        trial_array = ReadFile(trialPath);

        if (trial_array == null)
        {
            Debug.Log("Trial data were not loaded! Something is wrong.");
        }
        else
        {
            Debug.Log("Trial data were loaded!");
            AnalyzeDataButton.interactable = true;
            AnalysisList.interactable = true;
        }

    }
    void AnalyzeData()
    {
        //remove the 0 entries in the beginning
        trial_array = RemoveZeroRows(trial_array);
        MVC_array = RemoveZeroRows(MVC_array);

        //First, filter it, regardless
        filteredEMG = FilterEMG(trial_array, fs);
        filteredMVC = FilterEMG(MVC_array, fs);

        //extract MVCs
        float[] MVCs = new float[filteredMVC.GetLength(1)];
        for (int ch = 0; ch < filteredMVC.GetLength(1); ch++)
        {
            float max = float.MinValue;

            for (int t = 0; t < filteredMVC.GetLength(0); t++)
            {
                if (filteredMVC[t, ch] > max)
                    max = filteredMVC[t, ch];
            }

            MVCs[ch] = max;
        }

        //Normalize the data
        normalEMG = new float[filteredEMG.GetLength(0), filteredEMG.GetLength(1)];
        for (int ch = 0; ch < filteredEMG.GetLength(1); ch++)
        {
            for (int t = 0; t < filteredEMG.GetLength(0); t++)
            {
                normalEMG[t, ch] = filteredEMG[t, ch] / (MVCs[ch] * 0.95f);
            }
        }

        if (normalEMG == null)
        {
            Debug.Log("Couldn't filter the EMG data!");
        }
        else
        {
            Debug.Log("You have sucessfully filtered the EMG data!");
        }

        //If Analysis option is 0, plot the filtered EMG
        if (selectedAnalysis == "Filtered EMG")
        {
            //First, disable the line graph, and enable the bar graphs
            for (int i = 0; i < 16; i++)
            {
                EMGCharts[i].gameObject.SetActive(true);
                EMGBar[i].gameObject.SetActive(false);
                EMGCharts[i].ClearData();
            }

            for (int ch = 0; ch < normalEMG.GetLength(1); ch++)
            {
                plotFilteredEMG(ch, EMGCharts[ch], lineColorList[ch], lineColorList[ch]);
            }

        }
        else if (selectedAnalysis == "Histogram")
        {
            //First, disable the line graph, and enable the bar graphs
            for (int i = 0; i < 16; i++)
            {
                EMGCharts[i].gameObject.SetActive(false);
                EMGBar[i].gameObject.SetActive(true);
                EMGBar[i].ClearData();
            }

            for (int ch = 0; ch < normalEMG.GetLength(1); ch++)
            {
                float[] data = new float[normalEMG.GetLength(0)];
                for (int i = 0; i < normalEMG.GetLength(0); i++)
                {
                    data[i] = normalEMG[i, ch];
                }

                //Compute the histogram on each column
                ComputeHistogram(data, 10, out List<float> binCenters, out List<int> binCounts);

                //Plot it
                var title = EMGBar[ch].EnsureChartComponent<Title>();
                title.text = "Histogram: " + channelLabels[ch];

                Serie serie = EMGBar[ch].GetSerie(0);
                serie.itemStyle.color = lineColorList[ch];

                for (int i = 0; i < binCenters.Count; i++)
                {
                    string label = binCenters[i].ToString("F2");  // e.g., "0.12"
                    EMGBar[ch].AddXAxisData(label);
                    EMGBar[ch].AddData(0, binCounts[i]);
                }

                EMGBar[ch].RefreshChart();

            }
        }

        else if (selectedAnalysis == "Peak Magnitude")
        {
            //First, disable the line graph, and enable the bar graphs
            for (int i = 0; i < 16; i++)
            {
                EMGCharts[i].gameObject.SetActive(false);
                EMGBar[i].gameObject.SetActive(true);
                EMGBar[i].ClearData();
            }

            //Compute the peak magnitude (on normalized EMG)         
            for (int ch = 0; ch < filteredEMG.GetLength(1); ch++)
            {
                float[] data = new float[filteredEMG.GetLength(0)];

                for (int i = 0; i < filteredEMG.GetLength(0); i++)
                {
                    data[i] = normalEMG[i, ch];
                }
                float peakMagnitude = data.Max();
                Debug.Log("Peak EMG: " + peakMagnitude.ToString());

                //Plot it
                var title = EMGBar[ch].EnsureChartComponent<Title>();
                title.text = "Peak EMG: " + channelLabels[ch];

                Serie serie = EMGBar[ch].GetSerie(0);
                serie.itemStyle.color = lineColorList[ch];

                EMGBar[ch].AddXAxisData("1");
                EMGBar[ch].AddData(0, peakMagnitude);
                
                EMGBar[ch].RefreshChart();

            }
        }
        else if (selectedAnalysis == "Average Magnitude")
        {
            //First, disable the line graph, and enable the bar graphs
            for (int i = 0; i < 16; i++)
            {
                EMGCharts[i].gameObject.SetActive(false);
                EMGBar[i].gameObject.SetActive(true);
                EMGBar[i].ClearData();
            }

            //Compute the peak magnitude (on normalized EMG)         
            for (int ch = 0; ch < normalEMG.GetLength(1); ch++)
            {
                float[] data = new float[normalEMG.GetLength(0)];

                for (int i = 0; i < normalEMG.GetLength(0); i++)
                {
                    data[i] = normalEMG[i, ch];
                }
                float avgMagnitude = data.Average();

                //Plot it
                var title = EMGBar[ch].EnsureChartComponent<Title>();
                title.text = "Avg EMG: " + channelLabels[ch];

                Serie serie = EMGBar[ch].GetSerie(0);
                serie.itemStyle.color = lineColorList[ch];

                var yAxis = EMGBar[ch].EnsureChartComponent<YAxis>();
                yAxis.axisLabel.formatter = "{value:F2}";


                EMGBar[ch].AddXAxisData("1");
                EMGBar[ch].AddData(0, avgMagnitude);

                EMGBar[ch].RefreshChart();

            }
        }
    }
    void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    // DATA FILTERING
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
    public static float[,] FilterEMG(float[,] data, float fs)
    {
        int numSamples = data.GetLength(0);
        int numChannels = data.GetLength(1);

        float[,] filtered = new float[numSamples, numChannels];

        // Create filters
        var hp = BiquadFilter.DesignHighpass(fs, 40f, 4);
        var lp = BiquadFilter.DesignLowpass(fs, 10f, 4);

        for (int ch = 0; ch < numChannels; ch++)
        {
            // Reset filters for each channel
            hp.Reset();
            lp.Reset();

            // 1. High-pass filter
            float[] temp = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
                temp[i] = hp.Apply(data[i, ch]);

            // 2. Rectification
            for (int i = 0; i < numSamples; i++)
                temp[i] = Mathf.Abs(temp[i]);

            // 3. Low-pass filter
            for (int i = 0; i < numSamples; i++)
                filtered[i, ch] = lp.Apply(temp[i]);
        }

        return filtered;
    }
    void ComputeHistogram(float[] data, int binCount, out List<float> binCenters, out List<int> binCounts)
    {
        binCenters = new List<float>();
        binCounts = new List<int>();

        if (data == null || data.Length == 0 || binCount <= 0)
            return;

        float min = Mathf.Min(data);
        float max = Mathf.Max(data);
        float binSize = (max - min) / binCount;

        int[] counts = new int[binCount];

        // Count data in each bin
        foreach (float value in data)
        {
            int binIndex = Mathf.Clamp((int)((value - min) / binSize), 0, binCount - 1);
            counts[binIndex]++;
        }

        // Fill output lists
        for (int i = 0; i < binCount; i++)
        {
            float center = min + binSize * (i + 0.5f);
            binCenters.Add(center);
            binCounts.Add(counts[i]);
        }
    }

    // PLOTTING
    private void plotFilteredEMG(int channel, LineChart EMGGraph, Color lineColor, Color areaColor)
    {
        //Clear the old data first 
        EMGGraph.ClearData();

        channelTitle = channelLabels[channel];
        var title = EMGGraph.EnsureChartComponent<Title>();
        title.text = "EMG_filt: " + channelTitle;
        var xAxis = EMGGraph.EnsureChartComponent<XAxis>();
        xAxis.splitNumber = 5;
        xAxis.boundaryGap = true;
        xAxis.type = Axis.AxisType.Value;

        var yAxis = EMGGraph.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;

        for (int i = 0; i < normalEMG.GetLength(0); i++)
        {
            EMGGraph.AddXAxisData(i.ToString());
            EMGGraph.AddData(0, normalEMG[i, channel]);
        }

        //Color
        Serie serie = EMGGraph.GetSerie(0);
        serie.lineStyle.color = lineColor;
        serie.areaStyle.color = areaColor;
        EMGGraph.RefreshChart();
    }

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

    // OTHER FUNCTIONS
    void checkMVC()
    {
        Debug.Log(MVCpath);
        if (File.Exists(MVCpath))
        {
            MVCstatus.text = "AVAILABLE";
            MVCstatus.color = Color.green;

            LoadMVCButton.interactable = true;
        }
        else
        {
            MVCstatus.text = "NOT AVAILABLE";
            MVCstatus.color = Color.red;

            LoadMVCButton.interactable = false;
        }
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
    }

    private void SaveFloatArrayToTxt(float[,] data, string filePath)
    {
        int rowCount = data.GetLength(0);
        int colCount = data.GetLength(1);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < rowCount; i++)
            {
                string[] row = new string[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    row[j] = data[i, j].ToString("F12"); // 6 decimal places
                }

                string line = string.Join("\t", row); // Tab-separated
                writer.WriteLine(line);
            }
        }

        Debug.Log($"Saved filtered EMG data to: {filePath}");
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }
}
