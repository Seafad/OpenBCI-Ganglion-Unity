using GanglionUnity.Internal;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Components
{
    /// <summary>
    /// Measures focus of the user based on intensity of alpha and beta waves
    /// </summary>
    public class FocusMeasurer : MonoBehaviour
    {
        [SerializeField]
        private int recordTimeSec;
        [SerializeField]
        private RectTransform FocusedPointPrefab, UnfocusedPointPrefab, CurrPoint;
        [SerializeField]
        private RectTransform DataPointContainer;
        private float timeLeft;
        public Button FocusedRecordButton, UnfocusedRecordButton, ClearButton;
        private float alpha, beta;
        private float hzPerIndex;
        private OrderedChunkBuffer<float> waveBuffer;
        private int alphaStart, alphaEnd, betaStart, betaEnd;
        private bool isRecording;
        private bool isRecordingFocused;
        private List<Vector2> focusedPoints = new List<Vector2>(), unfocusedPoints = new List<Vector2>();
        private float scale;
        private Vector2 focusedCenter, unfocusedCenter, currPoint;

        private void Start()
        {
            SpectrumComputer.Instance.OnNewSpectrumData.AddListener(OnNewSpectrumData);
            hzPerIndex = SpectrumComputer.Instance.HzPerIndex;
            alphaStart = (int)(8 / hzPerIndex);
            alphaEnd = (int)(13 / hzPerIndex);
            betaStart = (int)(14 / hzPerIndex);
            betaEnd = (int)(30 / hzPerIndex);
            FocusedRecordButton.onClick.AddListener(RecordFocused);
            UnfocusedRecordButton.onClick.AddListener(RecordUnfocused);
            ClearButton.onClick.AddListener(ClearData);
            timeLeft = recordTimeSec;
            scale = DataPointContainer.rect.width / 100;
            currPoint = new Vector2();
        }

        private void Update()
        {
            if (isRecording)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0)
                {
                    timeLeft = recordTimeSec;
                    isRecording = false;
                }
            }
        }

        private void OnNewSpectrumData(float[] spectrum)
        {
            alpha = 0;
            for (int i = alphaStart; i <= alphaEnd; i++)
            {
                alpha += spectrum[i];
            }
            beta = 0;
            for (int i = betaStart; i <= betaEnd; i++)
            {
                beta += spectrum[i];
            }

            if (isRecording)
            {
                if (isRecordingFocused)
                {
                    focusedPoints.Add(new Vector2(alpha, beta));
                    var tr = Instantiate(FocusedPointPrefab, DataPointContainer);
                    tr.gameObject.SetActive(true);
                    tr.anchoredPosition = new Vector2(alpha * scale, beta * scale);
                    focusedCenter = new Vector2();
                    foreach (var p in focusedPoints)
                    {
                        focusedCenter.x += p.x;
                        focusedCenter.y += p.y;
                    }
                    focusedCenter.x /= focusedPoints.Capacity;
                    focusedCenter.y /= focusedPoints.Capacity;
                }
                else
                {
                    unfocusedPoints.Add(new Vector2(alpha, beta));
                    var tr = Instantiate(UnfocusedPointPrefab, DataPointContainer);
                    tr.gameObject.SetActive(true);
                    tr.anchoredPosition = new Vector2(alpha * scale, beta * scale);
                    unfocusedCenter = new Vector2();
                    foreach (var p in unfocusedPoints)
                    {
                        unfocusedCenter.x += p.x;
                        unfocusedCenter.y += p.y;
                    }
                    unfocusedCenter.x /= unfocusedPoints.Capacity;
                    unfocusedCenter.y /= unfocusedPoints.Capacity;
                }
            }
            else
            {
                currPoint.x = alpha;
                currPoint.y = beta;
                CurrPoint.anchoredPosition = new Vector2(alpha * scale, beta * scale);
                var unfocDist = Vector2.Distance(currPoint, unfocusedCenter);
                var focDist = Vector2.Distance(currPoint, focusedCenter);
                Debug.Log("Focus: " + (focDist / unfocDist) + " (UnfocDist: " + unfocDist + "FocDist: " + focDist + ")");
            }
        }

        private void RecordFocused()
        {
            if (isRecording) return;
            isRecording = true;
            isRecordingFocused = true;
        }

        private void RecordUnfocused()
        {
            if (isRecording) return;
            isRecording = true;
            isRecordingFocused = false;
        }

        private void ClearData()
        {
            if (isRecording)
                isRecording = false;
            focusedPoints.Clear();
            unfocusedPoints.Clear();
            focusedCenter = new Vector2();
            unfocusedCenter = new Vector2();
            foreach (Transform child in DataPointContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }


    }
}