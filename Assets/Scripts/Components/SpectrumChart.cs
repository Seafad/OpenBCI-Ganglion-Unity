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
        [SerializeField]
        private LineRenderer[] spectrumLR;

        private GanglionManager ganglion;
        private SpectrumComputer spectrumComputer;
        private List<RectTransform> freqLines = new List<RectTransform>();
        private float width, leftPos, stepX, hzPerIndex;
        private int valuesCount, lineIntervalHz, freqLineDeltaIndex;
        private float maxValue = 100;
        private float timeUntilUpdate;
        private float[][] spectrums;
        private bool isStreaming;

        private void Start()
        {
            spectrums = new float[4][];
            for (int i = 0; i < 4; i++)
                spectrums[i] = new float[SpectrumComputer.Instance.SpectrumSize];
            spectrumComputer = SpectrumComputer.Instance;
            spectrumComputer.OnNewSpectrumData.AddListener(OnNewSpectrumData);
            InitDisplay(spectrumComputer.SpectrumSize, spectrumComputer.HzPerIndex, 10);
            timeUntilUpdate = timeStep;
            ganglion = GanglionManager.Instance;
            ganglion.OnStreamStart.AddListener(OnStreamStart);
            ganglion.OnStreamEnd.AddListener(OnStreamEnd);
            ganglion.OnChannelTurnedOff.AddListener(OnChannelTurnedOff);
            ganglion.OnChannelTurnedOn.AddListener(OnChannelTurnedOn);
        }

        private void OnChannelTurnedOn(int channelIndex)
        {
            InitDisplayForChannel(channelIndex);
        }

        private void OnChannelTurnedOff(int channelIndex)
        {
            spectrumLR[channelIndex].positionCount = 0;
        }

        private void OnStreamStart()
        {
            isStreaming = true;
        }

        private void OnStreamEnd()
        {
            isStreaming = false;
            timeUntilUpdate = timeStep;
            InitDisplay(spectrumComputer.SpectrumSize, spectrumComputer.HzPerIndex, 10);
        }

        private void Update()
        {
            if (isStreaming)
            {
                timeUntilUpdate -= Time.deltaTime;
                if (timeUntilUpdate <= 0)
                {
                    timeUntilUpdate = timeStep;
                    for (int i = 0; i < 4; i++)
                        if (ganglion.IsChannelActive(i))
                        {
                            DisplayValues(spectrums[i], i);
                        }
                }
            }
        }

        public void OnNewSpectrumData(float[] spectrum, int channelIndex)
        {
            spectrums[channelIndex] = spectrum;
        }

        public void InitDisplay(int valuesCount, float hzPerIndex, int lineIntervalHz)
        {
            this.valuesCount = valuesCount;
            this.hzPerIndex = hzPerIndex;
            this.lineIntervalHz = lineIntervalHz;
            leftPos = displayRT.anchoredPosition.x;
            stepX = displayRT.rect.width / (valuesCount - 1);

            for (int i = 0; i < 4; i++)
                InitDisplayForChannel(i);

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

        private void InitDisplayForChannel(int channelIndex)
        {
            spectrumLR[channelIndex].positionCount = valuesCount;
            Vector3 curr = new Vector3();
            for (int i = 0; i < valuesCount; i++)
            {
                curr.x = leftPos + i * stepX;
                curr.y = 0;
                spectrumLR[channelIndex].SetPosition(i, curr);
            }
        }

        private void DisplayValues(float[] values, int channelIndex)
        {
            Vector3 curr = new Vector3(0, 0);
            for (int i = 0; i < values.Length; i++)
            {
                curr.x = leftPos + i * stepX;
                curr.y = Math.Min(displayRT.rect.height, values[i] * (displayRT.rect.height / maxValue));
                spectrumLR[channelIndex].SetPosition(i, curr);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (displayRT == null) return;
            width = displayRT.rect.width;
            leftPos = displayRT.rect.x;
            stepX = width / (valuesCount - 1);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < valuesCount; j++)
                {
                    Vector3 curr = spectrumLR[i].GetPosition(j);
                    curr.x = leftPos + j * stepX;
                    curr.y = 0;
                    spectrumLR[i].SetPosition(j, curr);
                }
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
