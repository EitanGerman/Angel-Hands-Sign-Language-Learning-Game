
using Assets.Scripts.GameManager;
using Assets.Scripts.Statistics;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XCharts.Runtime;

class statisticsManager : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] LineChart scoreLineChart;
    [SerializeField] LineChart timeToRecognizeLineChart;
    [SerializeField] PieChart skippedPieChart;
    [SerializeField] Button backButton;
    [SerializeField] BarChart CommonWordsChart;

    public void Start()
    {
        WordSignRushStatistics.getGameSessions();
        defineDropDown();
        updateCommonWords();
        updateScoreChart();
        foreach (string word in GameManager.Instance.GetWordsSupportedByModel())
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(word));
        }
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync(0);
        });
        updateWordToSHow();
    }
    private void defineDropDown()
    {
        dropdown.onValueChanged.AddListener(delegate
        {
            updateWordToSHow();

        });
    }
    private void updateWordToSHow()
    {
        string selectedText = dropdown.options[dropdown.value].text;
        WordSignRushStatistics.currentWord = selectedText;
        updatePieChart();
        updateTimeToResponseChart();
    }
    private void updatePieChart()
    {
        // Clear the existing data
        skippedPieChart.ClearData();

        // Add a PieSerie if it doesn't exist
        if (skippedPieChart.series.Count == 0)
        {
            skippedPieChart.AddSerie<Pie>();
        }

        // Add data to the pie chart with custom labels
        skippedPieChart.AddData(0, WordSignRushStatistics.getSkippedStatus(), "Skipped");
        skippedPieChart.AddData(0, WordSignRushStatistics.getNotSkippedStatus(), "Not Skipped");

        // Customize the series (set the radius)
        var serie = skippedPieChart.GetSerie<Pie>();
        serie.radius = new float[] { 30f, 100f };  // Set radius for the pie chart

        // Ensure the itemStyle object exists before setting the color
        serie.data[0].GetOrAddComponent<ItemStyle>().color = Color.red;    // Set color for "Skipped"
        serie.data[1].GetOrAddComponent<ItemStyle>().color = Color.green;  // Set color for "Not Skipped"

        // Refresh the chart to update the visual representation
        skippedPieChart.RefreshChart();
    }





    private void updateTimeToResponseChart()
    {
        // Clear the chart before adding new data
        timeToRecognizeLineChart.ClearData();

        // Add a line series if it doesn't exist
        if (timeToRecognizeLineChart.series.Count == 0)
        {
            timeToRecognizeLineChart.AddSerie<Line>("TimeToRecognize");  // Add a line series
        }
        Debug.Log("DEBUG!!!");
        // Get the scores with dates from the getScores method
        Dictionary<DateTime, float> responses = WordSignRushStatistics.getResponseTimes();//update this method to get response time
        Debug.Log("DEBUG!!!!??????");

        // Loop through the dictionary and add data to the chart
        foreach (var entry in responses)
        {
            DateTime sessionDate = entry.Key;   // The date (x-axis)
            float responseTime = entry.Value;          // The sesponse-time (y-axis)

            // Format the session date for the x-axis (e.g., show date and time)
            string formattedDate = sessionDate.ToString("yyyy-MM-dd HH:mm");
            Debug.Log("Time to recognize: " + responseTime);
            Debug.Log("Formatted date: " + formattedDate);
            // Add the formatted date to the x-axis and the score to the y-axis
            timeToRecognizeLineChart.AddXAxisData(formattedDate);
            timeToRecognizeLineChart.AddData(0, responseTime);  // Add the score as the y-axis value
        }

        // Refresh the chart to display the updated data
        timeToRecognizeLineChart.RefreshChart();
    }

    private void updateScoreChart()
    {
        // Clear the chart before adding new data
        scoreLineChart.ClearData();

        // Add a line series if it doesn't exist
        if (scoreLineChart.series.Count == 0)
        {
            scoreLineChart.AddSerie<Line>("Score");  // Add a line series
        }

        // Get the scores with dates from the getScores method
        Dictionary<DateTime, float> scores = WordSignRushStatistics.getScores();

        // Loop through the dictionary and add data to the chart
        foreach (var entry in scores)
        {
            DateTime sessionDate = entry.Key;   // The date (x-axis)
            float score = entry.Value;          // The score (y-axis)

            // Format the session date for the x-axis (e.g., show date and time)
            string formattedDate = sessionDate.ToString("yyyy-MM-dd HH:mm");

            // Add the formatted date to the x-axis and the score to the y-axis
            scoreLineChart.AddXAxisData(formattedDate);
            scoreLineChart.AddData(0, score);  // Add the score as the y-axis value
        }

        // Refresh the chart to display the updated data
        scoreLineChart.RefreshChart();
    }

    private void updateCommonWords()
    {
        // Clear any existing data in the chart
        CommonWordsChart.ClearData();
        CommonWordsChart.RemoveData();

        // Add a BarSerie if it doesn't exist
        if (CommonWordsChart.series.Count == 0 || !CommonWordsChart.HasSerie<Bar>())
        {
            CommonWordsChart.AddSerie<Bar>("Top 5 Common Words");
        }

        // Get the top 5 most common words and their frequencies
        Dictionary<string, int> top5Words = WordSignRushStatistics.GetTop5MostCommonWords();

        // Clear existing categories on the x-axis
        
        //CommonWordsChart.xAxis0.ClearData();

        // Add the top 5 data points to the bar chart
        foreach (var pair in top5Words)
        {
            string word = pair.Key;  // The word (x-axis label)
            int count = pair.Value;  // The frequency (y-axis value)

            // Add the word to the x-axis categories
            CommonWordsChart.AddXAxisData(word);

            // Add data to the chart. 0 is the series index for the BarSerie.
            CommonWordsChart.AddData(0, count);  // Only the y-axis value (frequency) is needed here
        }

        // Refresh the chart to display the updated data
        CommonWordsChart.RefreshChart();
    }

}