using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EEGChartChunk : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private RectTransform rectTransform;

    private int valCount, currValueIndex;
    private float width, leftPos;
    private float stepX = 2;
    private float timeLeft;
    public int Scale { get; set; }

    private float[] waveBuffer = new float[128];
    private int currWaveIndex;
    private int scale;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rectTransform = GetComponent<RectTransform>();
        width = rectTransform.rect.width;
        leftPos = rectTransform.position.x;
        valCount = (int)(width / stepX) - 1;
        currValueIndex = 0;
        lineRenderer.positionCount = valCount;
        Vector3 curr = new Vector3();
        for (int i = 0; i < valCount; i++)
        {
            curr.x = leftPos + i * stepX;
            curr.y = rectTransform.position.y;
            lineRenderer.SetPosition(i, curr);
        }
    }

    public int DisplayValues(float[] values, int fromIndex)
    {
        Vector3 curr = new Vector3(0, 0);
        for (int i = fromIndex; i < values.Length; i++)
        {
            curr.x = leftPos + currValueIndex * stepX;
            curr.y = values[i] * scale;
            lineRenderer.SetPosition(currValueIndex, curr);
            currValueIndex++;
            if (currValueIndex > valCount - 1)
            {
                currValueIndex = 0;
                return i;
            }
        }
        return -1;
    }
}
