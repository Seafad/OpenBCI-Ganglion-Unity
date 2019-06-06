using GanglionUnity.Internal;
using System;
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
        private float alphaMax = 150, betaMax = 100, gammaMax = 20;
        [SerializeField]
        private Button FocusedRecordButton, UnfocusedRecordButton, ClearButton;
        [SerializeField]
        private RectTransform focusFilledImg, alphaFilledImg, betaFilledImg, gammaFilledImg,
            alphaFocused, alphaUnfocused, betaFocused, betaUnfocused, gammaFocused, gammaUnfocused;

        private float timeLeft;
        private float alpha, beta, gamma;
        private float hzPerIndex;
        private OrderedChunkBuffer<float> waveBuffer;
        private int alphaStart, alphaEnd, betaStart, betaEnd, gammaStart, gammaEnd;
        private bool isRecording;
        private bool isRecordingFocused;
        private List<Vector3> focusedPoints = new List<Vector3>(), unfocusedPoints = new List<Vector3>();
        private float displayBarHeight;
        private Vector3 focusedCenter, unfocusedCenter, currPoint;

        private void Start()
        {
            SpectrumComputer.Instance.OnNewSpectrumData.AddListener(OnNewSpectrumData);
            hzPerIndex = SpectrumComputer.Instance.HzPerIndex;
            alphaStart = (int)(8 / hzPerIndex);
            alphaEnd = (int)(13 / hzPerIndex);
            betaStart = (int)(14 / hzPerIndex);
            betaEnd = (int)(30 / hzPerIndex);
            gammaStart = (int)(31 / hzPerIndex);
            gammaEnd = (int)(45 / hzPerIndex);
            displayBarHeight = focusFilledImg.rect.height;
            FocusedRecordButton.onClick.AddListener(RecordFocused);
            UnfocusedRecordButton.onClick.AddListener(RecordUnfocused);
            ClearButton.onClick.AddListener(ClearData);
            timeLeft = recordTimeSec;
            currPoint = new Vector3();
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
                    FocusedRecordButton.interactable = true;
                    UnfocusedRecordButton.interactable = true;
                }
            }
        }

        private void OnNewSpectrumData(float[] spectrum, int channelIndex)
        {
            if (channelIndex != 0) return;
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
            gamma = 0;
            for (int i = gammaStart; i <= gammaEnd; i++)
            {
                gamma += spectrum[i];
            }

            if (isRecording)
            {
                if (isRecordingFocused)
                {
                    var dataPoint = new Vector3(alpha, beta, gamma);
                    focusedPoints.Add(dataPoint);
                    focusedCenter = new Vector3();
                    foreach (var p in focusedPoints)
                    {
                        focusedCenter.x += p.x;
                        focusedCenter.y += p.y;
                        focusedCenter.z += p.z;
                    }
                    focusedCenter.x /= focusedPoints.Count;
                    focusedCenter.y /= focusedPoints.Count;
                    focusedCenter.z /= focusedPoints.Count;
                    alphaFocused.anchoredPosition = new Vector2(alphaFocused.anchoredPosition.x, Math.Min(displayBarHeight, focusedCenter.x * displayBarHeight / alphaMax));
                    betaFocused.anchoredPosition = new Vector2(betaFocused.anchoredPosition.x, Math.Min(displayBarHeight, focusedCenter.y * displayBarHeight / betaMax));
                    gammaFocused.anchoredPosition = new Vector2(gammaFocused.anchoredPosition.x, Math.Min(displayBarHeight, focusedCenter.z * displayBarHeight / gammaMax));
                }
                else
                {
                    var dataPoint = new Vector3(alpha, beta, gamma);
                    unfocusedPoints.Add(dataPoint);
                    foreach (var p in unfocusedPoints)
                    {
                        unfocusedCenter.x += p.x;
                        unfocusedCenter.y += p.y;
                        unfocusedCenter.z += p.z;
                    }
                    unfocusedCenter.x /= unfocusedPoints.Count;
                    unfocusedCenter.y /= unfocusedPoints.Count;
                    unfocusedCenter.z /= unfocusedPoints.Count;
                    alphaUnfocused.anchoredPosition = new Vector2(alphaUnfocused.anchoredPosition.x, Math.Min(displayBarHeight, unfocusedCenter.x * displayBarHeight / alphaMax));
                    betaUnfocused.anchoredPosition = new Vector2(betaUnfocused.anchoredPosition.x, Math.Min(displayBarHeight, unfocusedCenter.y * displayBarHeight / betaMax));
                    gammaUnfocused.anchoredPosition = new Vector2(gammaUnfocused.anchoredPosition.x, Math.Min(displayBarHeight, unfocusedCenter.z * displayBarHeight / gammaMax));
                }
            }

            alphaFilledImg.sizeDelta = new Vector2(alphaFilledImg.sizeDelta.x, Math.Min(displayBarHeight, alpha * displayBarHeight / alphaMax));
            betaFilledImg.sizeDelta = new Vector2(betaFilledImg.sizeDelta.x, Math.Min(displayBarHeight, beta * displayBarHeight / betaMax));
            gammaFilledImg.sizeDelta = new Vector2(gammaFilledImg.sizeDelta.x, Math.Min(displayBarHeight, gamma * displayBarHeight / gammaMax));
            currPoint.x = alpha;
            currPoint.y = beta;
            currPoint.z = gamma;
            if (focusedCenter != Vector3.zero && unfocusedCenter != Vector3.zero)
            {
                float focus = 0;
                if(currPoint.x > focusedCenter.x && currPoint.x < focusedCenter.x + Math.Abs(unfocusedCenter.x - focusedCenter.x))
                    focus += 0.33f;
                if (currPoint.y < unfocusedCenter.y && currPoint.y > focusedCenter.y - Math.Abs(unfocusedCenter.y - focusedCenter.y))
                    focus += 0.33f;
                if (currPoint.z < unfocusedCenter.z && currPoint.z > focusedCenter.z - Math.Abs(unfocusedCenter.z - focusedCenter.z))
                    focus += 0.33f;
                focusFilledImg.sizeDelta = new Vector2(focusFilledImg.sizeDelta.x, focus * displayBarHeight);
            }
        }

        private void RecordFocused()
        {
            if (isRecording) return;
            isRecording = true;
            isRecordingFocused = true;
            FocusedRecordButton.interactable = false;
            UnfocusedRecordButton.interactable = false;
        }

        private void RecordUnfocused()
        {
            if (isRecording) return;
            isRecording = true;
            isRecordingFocused = false;
            FocusedRecordButton.interactable = false;
            UnfocusedRecordButton.interactable = false;
        }

        private void ClearData()
        {
            if (isRecording)
                isRecording = false;
            focusedPoints.Clear();
            unfocusedPoints.Clear();
            focusedCenter = new Vector2();
            unfocusedCenter = new Vector2();
            FocusedRecordButton.interactable = true;
            UnfocusedRecordButton.interactable = true;
            alphaUnfocused.anchoredPosition = new Vector2(alphaUnfocused.anchoredPosition.x, 0);
            betaUnfocused.anchoredPosition = new Vector2(betaUnfocused.anchoredPosition.x, 0);
            gammaUnfocused.anchoredPosition = new Vector2(gammaUnfocused.anchoredPosition.x, 0);
            alphaFocused.anchoredPosition = new Vector2(alphaUnfocused.anchoredPosition.x, 0);
            betaFocused.anchoredPosition = new Vector2(betaUnfocused.anchoredPosition.x, 0);
            gammaFocused.anchoredPosition = new Vector2(gammaUnfocused.anchoredPosition.x, 0);
        }
    }
}