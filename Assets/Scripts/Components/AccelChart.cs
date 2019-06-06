using System;
using UnityEngine;
using UnityEngine.UI;

namespace GanglionUnity.Components
{
    public class AccelChart : MonoBehaviour
    {
        [SerializeField]
        private Transform accelPointTransform;
        [SerializeField]
        private RectTransform accelPointContainer;
        [SerializeField]
        private Text textX, textY, textZ;

        private GanglionManager ganglion;
        private Vector3 initialScale = new Vector3(2, 2, 2);
        private Vector3 prevPos, currPos;
        private float startTime;
        
        private float scaleX, scaleY;
        private float maxValue = 2;

        void Start()
        {
            ganglion = GanglionManager.Instance;
            ganglion.OnAccelDataReceived.AddListener(OnAccelDataReceived);
            scaleX = accelPointContainer.rect.width / 2;
            scaleY = accelPointContainer.rect.height / 2;
            prevPos = currPos = accelPointTransform.position;
        }

        private void OnAccelDataReceived(Vector3 acceleration)
        {
            prevPos = currPos;
            startTime = Time.time;
            currPos.x = accelPointContainer.transform.position.x + acceleration.x * scaleX / maxValue;
            currPos.y = accelPointContainer.transform.position.y + acceleration.y * scaleY / maxValue;

            accelPointTransform.localScale = initialScale + Vector3.one * acceleration.z;
           
            textX.text = acceleration.x.ToString();
            textY.text = acceleration.y.ToString();
            textZ.text = acceleration.z.ToString();
        }

        private void Update()
        {
            accelPointTransform.position = Vector3.Lerp(prevPos, currPos, (Time.time - startTime) * 10);
        }
    }
}
