using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphDrawer : MonoBehaviour
{
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform axisContainer;
    [SerializeField] private RectTransform gridContainer;

    [SerializeField] private RectTransform circleTemplate;
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;

    public Color tetrisBotColor = new Color(1, 1, 1, 1);
    public Color mctsBotColor = new Color(1, 1, 1, 1);
    public Color humanBotColor = new Color(1, 1, 1, 1);
    private Color tetrisBotConnectionColor;
    private Color mctsBotConnectionColor;
    private Color humanBotConnectionColor;

    public int yAxisSeparatorCount = 10;

    public float GraphWidth
    {
        get
        {
            return graphContainer.sizeDelta.x;
        }
    }

    public float GraphHeight
    {
        get
        {
            return graphContainer.sizeDelta.y;
        }
    }

    private static GraphDrawer instance;
    public static GraphDrawer Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;

        tetrisBotConnectionColor = CopyColor(tetrisBotColor);
        tetrisBotConnectionColor.a = 0.5f;

        mctsBotConnectionColor = CopyColor(mctsBotColor);
        mctsBotConnectionColor.a = 0.5f;

        humanBotConnectionColor = CopyColor(humanBotColor);
        humanBotConnectionColor.a = 0.5f;
    }

    private Color CopyColor(Color other)
    {
        return new Color(other.r, other.g, other.b, other.a);
    }

    public void DrawValues(List<float> valueList, BotVersion botVersion, float minValue, float maxValue)
    {
        int maxElements = valueList.Count;
        Color circleColor = tetrisBotColor;
        Color connectionColor = tetrisBotConnectionColor;

        switch (botVersion)
        {
            case BotVersion.MCTSBot:
                circleColor = mctsBotColor;
                connectionColor = mctsBotConnectionColor;
                break;
            case BotVersion.HumanizedBot:
                circleColor = humanBotColor;
                connectionColor = humanBotConnectionColor;
                break;
        }

        float xGap = GraphWidth / maxElements;
        float yMinimum = 10f;
        float yMaximum = GraphHeight - yMinimum;

        if (minValue == maxValue) { maxValue += 0.5f; minValue -= 0.5f; }

        bool firstCircle = true;
        Vector2 previousCirclePosition = Vector2.zero;
        for (int i = 0; i < maxElements; i++)
        {
            if(valueList[i] != -1)
            {
                float xPosition = xGap * (i + 0.5f);
                float yPosition = ((valueList[i] - minValue) * (yMaximum - yMinimum) / (maxValue - minValue)) + yMinimum;

                Vector2 circlePosition = new Vector2(xPosition, yPosition);
                DrawCircle(circlePosition, circleColor);

                if (!firstCircle)
                    DrawConnection(previousCirclePosition, circlePosition, connectionColor);
                else
                    firstCircle = false;

                previousCirclePosition = circlePosition;
            }
        }
    }

    public void DrawGridAndAxis(int maxElements, float minValue, float maxValue)
    {
        float xGap = GraphWidth / maxElements;
        float yMinimum = 10f;
        float yMaximum = GraphHeight - yMinimum;

        for (int i = 0; i < maxElements; i++)
        {
            float xPosition = xGap * (i + 0.5f);

            RectTransform labelX = Instantiate(labelTemplateX, axisContainer);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(labelX.anchoredPosition.x + xPosition - xGap * 0.5f, labelX.anchoredPosition.y);
            labelX.GetComponent<TMP_Text>().text = (i + 1).ToString();

            RectTransform dashY = Instantiate(dashTemplateY, gridContainer);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(dashY.anchoredPosition.x + xPosition - (1.75f * xGap), dashY.anchoredPosition.y);
        }

        float yGap = GraphHeight / yAxisSeparatorCount;
        float yValueGap = (maxValue - minValue) / yAxisSeparatorCount;
        for (int i = 0; i <= yAxisSeparatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY, axisContainer);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(labelY.anchoredPosition.x, labelY.anchoredPosition.y + yGap * i);
            labelY.GetComponent<TMP_Text>().text = (minValue + yValueGap * i).ToString();

            RectTransform dashX = Instantiate(dashTemplateX, gridContainer);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(dashX.anchoredPosition.x, dashX.anchoredPosition.y + yGap * i);
        }
    }

    public void CleanGraph()
    {
        for(int i = graphContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(graphContainer.GetChild(i).gameObject);
        }

        for(int i = gridContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(gridContainer.GetChild(i).gameObject);
        }

        for(int i = axisContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(axisContainer.GetChild(i).gameObject);
        }
    }

    private void DrawCircle(Vector2 anchoredPosition, Color color)
    {
        RectTransform circle = Instantiate(circleTemplate, graphContainer);
        circle.gameObject.SetActive(true);
        circle.GetComponent<Image>().color = color;

        circle.anchoredPosition = anchoredPosition;
        circle.anchorMin = circle.anchorMax = Vector2.zero;
    }

    private void DrawConnection(Vector2 dotPositionA, Vector2 dotPositionB, Color color)
    {
        GameObject connectionGO = new GameObject("dotConnection", typeof(Image));
        connectionGO.transform.SetParent(graphContainer, false);
        connectionGO.GetComponent<Image>().color = color;

        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        RectTransform rectTransform = connectionGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;

        rectTransform.localEulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, dir));
    }
}
