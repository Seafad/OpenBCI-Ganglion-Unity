using GanglionUnity.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Internal
{
    /// <summary>
    /// Draws the EEG frequency intensity spectrum
    /// </summary>
    public class SpectrumChart : MonoBehaviour
    {
        [SerializeField, Range(0.4f, 2)]
        private float timeStep;
        [SerializeField]
        private RectTransform FreqLinePrefab;
        [SerializeField]
        private RectTransform displayRT;

        private LineRenderer spectrumLR;
        private List<RectTransform> freqLines = new List<RectTransform>();
        private float width, leftPos, stepX, hzPerIndex;
        private int valuesCount, lineIntervalHz, freqLineDeltaIndex;
        private float maxValue = 100;
        private float timeUntilUpdate;
        private float[] spectrum;

        private void Awake()
        {
            spectrumLR = GetComponentInChildren<LineRenderer>();
        }

        private void Start()
        {
            spectrum = new float[SpectrumComputer.Instance.SpectrumSize];
            SpectrumComputer.Instance.OnNewSpectrumData.AddListener(OnNewSpectrumData);
            InitDisplay(SpectrumComputer.Instance.SpectrumSize, SpectrumComputer.Instance.HzPerIndex, 10);
            timeUntilUpdate = timeStep;
        }

        private void Update()
        {
            timeUntilUpdate -= Time.deltaTime;
            if (timeUntilUpdate <= 0)
            {
                timeUntilUpdate = timeStep;
                DisplayValues(spectrum);
            }
        }

        public void OnNewSpectrumData(float[] spectrum)
        {
            this.spectrum = spectrum;
        }

        public void InitDisplay(int valuesCount, float hzPerIndex, int lineIntervalHz)
        {
            this.valuesCount = valuesCount;
            this.hzPerIndex = hzPerIndex;
            this.lineIntervalHz = lineIntervalHz;
            leftPos = displayRT.rect.x;
            stepX = displayRT.rect.width / (valuesCount - 1);
            spectrumLR.positionCount = valuesCount;

            Vector3 curr = new Vector3();
            for (int i = 0; i < valuesCount; i++)
            {
                curr.x = leftPos + i * stepX;
                curr.y = 0;
                spectrumLR.SetPosition(i, curr);
            }

            freqLineDeltaIndex = (int)(lineIntervalHz / hzPerIndex) + 1;
            int n = lineIntervalHz;

            for (int i = freqLineDeltaIndex; i < valuesCount; i += freqLineDeltaIndex)
            {
                var freqLine = Instantiate(FreqLinePrefab, displayRT.transform);
                freqLine.gameObject.SetActive(true);
                freqLine.anchoredPosition = new Vector2(leftPos + i * stepX, 0);
                var freqText = freqLine.GetComponentInChildren<Text>();
                freqText.text = n.ToString();
                freqLines.Add(freqLine);
                n += lineIntervalHz;
            }
        }

        private void DisplayValues(float[] values)
        {
            Vector3 curr = new Vector3(0, 0);
            for (int i = 0; i < values.Length; i++)
            {
                curr.x = leftPos + i * stepX;
                curr.y = Math.Min(displayRT.rect.height, values[i] * (displayRT.rect.height / maxValue));
                spectrumLR.SetPosition(i, curr);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (displayRT == null) return;
            width = displayRT.rect.width;
            leftPos = displayRT.rect.x;
            stepX = width / (valuesCount - 1);

            for (int i = 0; i < valuesCount; i++)
            {
                Vector3 curr = spectrumLR.GetPosition(i);
                curr.x = leftPos + i * stepX;
                curr.y = 0;
                spectrumLR.SetPosition(i, curr);
            }
            int index = freqLineDeltaIndex;
            foreach (var line in freqLines)
            {
                line.anchoredPosition = new Vector2(leftPos + index * stepX, 1);
                index += freqLineDeltaIndex;
            }
        }
    }
}
