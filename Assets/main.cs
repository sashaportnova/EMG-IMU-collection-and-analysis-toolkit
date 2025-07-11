using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class main : MonoBehaviour
{
    public Button DataCollection;
    public Button DataVisualizaiton;
    public Button DataAnalysis;
    // Start is called before the first frame update
    void Start()
    {
        DataCollection.onClick.AddListener(DataCollectionStart);
        DataVisualizaiton.onClick.AddListener(DataVisualizationStart);
        DataAnalysis.onClick.AddListener(DataAnalysisStart);
    }
    private void DataCollectionStart()
    {
        SceneManager.LoadScene(1);
    }

    private void DataVisualizationStart()
    {
        SceneManager.LoadScene(2);
    }

    private void DataAnalysisStart()
    {
        SceneManager.LoadScene(3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
