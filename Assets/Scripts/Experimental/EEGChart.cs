using GanglionUnity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GanglionUnity.Experimental
{
    [RequireComponent(typeof(LineRenderer))]
    public class EEGChart : MonoBehaviour
    {
        [Range(0.01f, 1)]
        public float updateRate;
        [Range(0.1f, 10f)]
        public float step;
        [SerializeField, Range(1, 100000)]
        private float scale;
        private int valCount, currValueIndex;
        private List<double> values;

        private LineRenderer lineRenderer;
        private RectTransform rectTransform;
        private float width, leftPos;



        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            rectTransform = GetComponent<RectTransform>();
            //lineRenderer.hideFlags = HideFlags.HideInInspector;

            width = rectTransform.sizeDelta.x;
            leftPos = rectTransform.position.x;
            
            
            valCount = (int)(width / step);
            values = new List<double>(valCount);
            currValueIndex = 0;
            lineRenderer.positionCount = valCount;
            Vector3 right = new Vector3(leftPos + width, rectTransform.position.y);
            for(int i = 0; i < valCount; i++)
            {
                lineRenderer.SetPosition(i, right);
            }
        }

        /*
        public void AddEEGData(float[] eegData)
        {
            this.data = data;
            lineRenderer.positionCount = data.Length;
            Vector3 curr = new Vector3(0, 0);
            float step = width / data.Length;
            for (int i = 0; i < data.Length; i++)
            {
                curr.x = leftPos + i * step;
                curr.y = data[i] * scale;
                lineRenderer.SetPosition(i, curr);
            }
        }*/

        public void OnEEGReceived(List<EEGSample> eegs)
        {
            Vector3 curr = new Vector3(0, 0);
            foreach (var eeg in eegs)
            {
                //values[currValueIndex] = eeg.channelData[0];
                curr.x = leftPos + currValueIndex * step;
                curr.y = (float)eeg.channelData[0] * scale;
                lineRenderer.SetPosition(currValueIndex, curr);

                currValueIndex++;
                if (currValueIndex > valCount - 1)
                    currValueIndex = 0;
            }

            
            
        }

    }
}
