using UnityEngine;
using UnityEngine.UI;
namespace GanglionUnity.Internal
{
    public class ImpedanceIndicator : MonoBehaviour
    {
        [SerializeField]
        private Image indicator;
        [SerializeField]
        private Text valueText;

        private static Color32 disabledColor = new Color32(198, 198, 198, 255), excellentColor = new Color32(13, 107, 0, 255), goodColor = new Color32(0, 255, 0, 255),
            okColor = new Color32(209, 255, 79, 255), poorColor = new Color32(255, 164, 0, 255), badColor = new Color32(255, 0, 0, 255);

        private void Awake()
        {
            Disable();
        }

        public void SetValue(float value)
        {
            valueText.text = value.ToString();
            if (value > 0 && value <= 10) //very good signal quality
                indicator.color = excellentColor;
            else if (value > 10 && value <= 50)//good signal quality
                indicator.color = goodColor;
            else if (value > 50 && value <= 100)//acceptable signal quality
                indicator.color = okColor;
            else if (value > 100 && value <= 150) //questionable signal quality
                indicator.color = poorColor;
            else if (value > 150)//bad signal quality
                indicator.color = badColor;
        }

        public void Disable()
        {
            valueText.text = "";
            indicator.color = disabledColor;

        }
    }
}
