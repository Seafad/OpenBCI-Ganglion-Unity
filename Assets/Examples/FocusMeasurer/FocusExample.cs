using GanglionUnity.Components;
using GanglionUnity.Experimental;
using GanglionUnity.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FocusExample : MonoBehaviour
{
    public int FFTSize = 256;
    public GanglionController controller;
    public GameObject connectionPanelGO;
    public EEGChart freqChart;
    private List<float> m_samples;
    private int sampleRate = 200;
    private float bucketWidth;

    private void Awake()
    {
        bucketWidth = (float)sampleRate / FFTSize;
        m_samples = new List<float>();
        //controller.OnEEGReceived.AddListener(OnEEG);
    }

    private void OnEEG(EEGSample[] samples)
    {
        m_samples.AddRange(samples.Select(x => (float)x.channelData[0]).ToList());
        /*
        if(m_samples.Count > FFTSize)
        {
            var freqs = GetSpectrum(m_samples.GetRange(0, FFTSize).ToArray());
            m_samples.RemoveRange(0, FFTSize);
            //freqChart.SetData(freqs);
            float alpha = 0;
            for(int i = 9; i <= 15; i++)
            {
                alpha += freqs[i] * freqs[i];
            }
            float beta = 0;
            for (int i = 30; i <= 37; i++)
            {
                beta += freqs[i] * freqs[i];
            }
            Debug.Log("Focus: " + (alpha / beta) + "% (Alpha: " + alpha + " Beta: " + beta + ")");
            
        }*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            connectionPanelGO.SetActive(!connectionPanelGO.activeSelf);
        }
    }
   
}
