using GanglionUnity.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace GanglionUnity.Components
{
    public class EEGChart : MonoBehaviour
    {
        public bool filtered;
        [Range(0.01f, 1)]
        public float updateRate;
        [Range(.05f, .5f)]
        public float timeStep;
        [SerializeField, Range(1, 100000)]
        private int scale;

        private RectTransform rectTransform;
        private EEGChartChunk[] Chunks;
        private int currChunkIndex = 0;

        private int valCount;
        private float width, leftPos;
        private float stepX = 2;
        private float timeLeft;

        private float[] waveBuffer = new float[128];
        private int currWaveIndex;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Chunks = GetComponentsInChildren<EEGChartChunk>();
            foreach (var chunk in Chunks)
            {
                chunk.Scale = scale;
            }
            timeLeft = timeStep;
            width = rectTransform.rect.width;
            leftPos = rectTransform.position.x;
            valCount = (int)(width / stepX) - 1;
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
        }

        public void OnEEGReceived(List<EEGSample> eegs)
        {
            float[] newValues = new float[eegs.Count];

            for (int i = 0; i < eegs.Count; i++)
            {
                if (filtered)
                {
                    waveBuffer[currWaveIndex++] = (float)eegs[i].channelData[0];
                    if (currWaveIndex == waveBuffer.Length - 1)
                    {
                        currWaveIndex = 0;
                    }
                }
                else
                    newValues[i] = (float)eegs[i].channelData[0];
            }
            if (timeLeft <= 0)
            {
                if (filtered)
                {
                    int lastIndex = 0;
                    while (lastIndex != -1)
                    {
                        lastIndex = Chunks[currChunkIndex++].DisplayValues(waveBuffer, lastIndex);
                        currChunkIndex %= Chunks.Length;
                    }
                }
                else
                {
                    // DisplayValues(newValues);
                }
                timeLeft = timeStep;
            }
        }
    }
}
