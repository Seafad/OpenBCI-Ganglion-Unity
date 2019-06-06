using GanglionUnity.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Components
{
    public class EEGChart : MonoBehaviour
    {
        [SerializeField]
        private bool filtered;
        [Range(.05f, .5f)]
        private float timeStep;
        [SerializeField]
        private VoltageScale voltageScale;
        [SerializeField]
        private RectTransform channelRT;
        [SerializeField]
        private float paddingRight;
        [SerializeField]
        private Toggle filterToggle;
        [SerializeField]
        private Dropdown voltageDropdown;

        private float width, leftPos;
        private int valuesCount;
        private float stepX;

        private GanglionManager ganglion;
        private LineRenderer[] channelLR;
        private int maxValue;
        private int MCV_SCALE = 1000000;

        // private EEGChartChunk[] Chunks;
        // private int currChunkIndex = 0;
        private float timeLeft;
        private int currWaveIndex;
        private OrderedChunkBuffer<float>[] displayBuffers = new OrderedChunkBuffer<float>[4];
        private int bufferChunkSize = 64, bufferChunks = 8;
        private float[] filteredSignal;
        private int sampleRate = 200;

        private void Awake()
        {
            for (int i = 0; i < 4; i++)
                displayBuffers[i] = new OrderedChunkBuffer<float>(bufferChunkSize, bufferChunks);
            timeLeft = timeStep;
        }

        private IEnumerator Start()
        {
            ganglion = GanglionManager.Instance;
            ganglion.OnEEGReceived.AddListener(OnEEGReceived);
            ganglion.OnChannelTurnedOff.AddListener(OnChannelOff);
            channelLR = GetComponentsInChildren<LineRenderer>();
            filterToggle.isOn = filtered;
            maxValue = VoltageScale2Voltage(voltageScale);
            voltageDropdown.value = (int)voltageScale;
            
            filterToggle.onValueChanged.AddListener(OnFilterToggleClick);
            voltageDropdown.onValueChanged.AddListener(OnVoltageDropdownChanged);
            yield return new WaitForEndOfFrame();
            InitDisplay(bufferChunkSize * bufferChunks);
        }

        private void OnChannelOff(int channelIndex)
        {
            displayBuffers[channelIndex].Clear();
            InitDisplayForChannel(channelIndex);
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
        }

        private void OnFilterToggleClick(bool isOn)
        {
            filtered = isOn;
        }

        private void OnVoltageDropdownChanged(int newVoltageScale)
        {
            voltageScale = (VoltageScale)newVoltageScale;
            maxValue = VoltageScale2Voltage(voltageScale);
        }


        public void OnEEGReceived(List<EEGSample> eegs)
        {
            for (int i = 0; i < eegs.Count; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (ganglion.IsChannelActive(j))
                    {
                        displayBuffers[j].Write((float)eegs[i].channelData[j]);
                        if (displayBuffers[j].IsFull)
                        {
                            if (filtered)
                            {
                                filteredSignal = displayBuffers[j].GetLastChunkValues();
                                DataProcessing.FilterFreqsAbove50Hz(ref filteredSignal, sampleRate, 6);
                                displayBuffers[j].SetChunkValues(filteredSignal, bufferChunks - 1);
                            }
                            DisplayValues(displayBuffers[j].GetAllValues(), j);
                        }
                    }
                }
        }

        private void InitDisplay(int valuesCount)
        {
            this.valuesCount = valuesCount;
            width = channelRT.rect.width;
            leftPos = channelRT.anchoredPosition.x;
            stepX = (width - paddingRight) / (valuesCount - 1);

            for (int i = 0; i < 4; i++)
                InitDisplayForChannel(i);
        }

        private void InitDisplayForChannel(int channelIndex)
        {
            channelLR[channelIndex].positionCount = valuesCount;
            Vector3 curr = new Vector3();
            for (int i = 0; i < valuesCount; i++)
            {
                curr.x = leftPos + i * stepX;
                curr.y = -channelRT.rect.height / 2;
                channelLR[channelIndex].SetPosition(i, curr);
            }
        }

        private void DisplayValues(float[] values, int channelIndex)
        {
            Vector3 curr = new Vector3(0, 0);
            for (int i = 0; i < values.Length; i++)
            {
                curr.x = leftPos + i * stepX;
                curr.y = Mathf.Clamp(values[i] * MCV_SCALE * (channelRT.rect.height / ((int)maxValue * 2)) - channelRT.rect.height / 2, -channelRT.rect.height, 0);
                channelLR[channelIndex].SetPosition(i, curr);
            }
        }
        private int VoltageScale2Voltage(VoltageScale vs)
        {
            switch (vs)
            {
                case VoltageScale.mv50:
                    return 50;
                case VoltageScale.mv100:
                    return 100;
                case VoltageScale.mv250:
                    return 250;
                case VoltageScale.mv500:
                    return 500;
                default:
                    return 50;
            }
        }

        public enum VoltageScale { mv50, mv100, mv250, mv500 };
    }
}
