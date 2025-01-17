using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AxisGenerator : MonoBehaviour
{
    public float XaxisLength = 5f;      // Length of each axis (meters)
    public float YaxisLength = 5f;      
    public float ZaxisLength = 5f;
    public int divisionsX = 40;
    public int divisionsY = 9;
    public int divisionsZ = 30;
    public float lineWidth = 0.02f;     // Width of the axis lines (very thin)
    public string texto = "hello";

    private int labelSize = 2;           // Fontsize for labels (default size)
    public float rotation = 0f;         // Rotation (in degrees) of the Y-axis

    public bool grid;

    private float labelOffset = 0.1f;   // Offset for label positioning

    

    private LineRenderer xAxisRenderer;
    private LineRenderer yAxisRenderer;
    private LineRenderer zAxisRenderer;

    private float minLuminosity = 300f;
    private float maxLuminosity = 1000f;
    private float minHumidity = 25f;
    private float maxHumidity = 70f;
    private int Ndays = 30;


    void Start()
    {
        // Create LineRenderers for each axis with customized width
        CreateAxis(ref xAxisRenderer, Color.red, new Vector3(XaxisLength, 0, 0), "Luminosity", "X");
        CreateAxis(ref yAxisRenderer, Color.red, new Vector3(0, YaxisLength, 0), "Humidity", "Y");
        CreateAxis(ref zAxisRenderer, Color.red, new Vector3(0, 0, ZaxisLength), "Day", "Z");

        // Add scale labels for each axis
        CreateScaleLabels(xAxisRenderer, new Vector3(XaxisLength, 0, 0), "X");
        CreateScaleLabels(yAxisRenderer, new Vector3(0, YaxisLength, 0), "Y");
        CreateScaleLabels(zAxisRenderer, new Vector3(0, 0, ZaxisLength), "Z");

        if (grid)
        {
            // Create grids for each plane, aligned with the labels
            CreateGridXY(new Vector3(0, 0, 0), XaxisLength, YaxisLength, divisionsX, divisionsY);
            CreateGridYZ(new Vector3(0, 0, 0), YaxisLength, ZaxisLength, divisionsY, divisionsZ);
            CreateGridXZ(new Vector3(0, 0, 0), XaxisLength, ZaxisLength, divisionsX, divisionsZ);
        }
        CreateTextCanvasAtPosition(XaxisLength, ZaxisLength);
    }

    void CreateAxis(ref LineRenderer axisRenderer, Color axisColor, Vector3 endPosition, string name, string axis)
    {
        // Create a new GameObject for the axis
        GameObject axisObject = new GameObject(name + " Axis");
        axisObject.transform.SetParent(this.transform);

        // Add LineRenderer component
        axisRenderer = axisObject.AddComponent<LineRenderer>();

        // Set the start and end points of the axis (from origin to end position)
        axisRenderer.SetPosition(0, Vector3.zero);
        axisRenderer.SetPosition(1, endPosition);

        // Set the color of the axis
        axisRenderer.startColor = axisColor;
        axisRenderer.endColor = axisColor;

        // Set the width of the axis lines
        axisRenderer.startWidth = lineWidth;
        axisRenderer.endWidth = lineWidth;

        // Set LineRenderer material and other properties for better appearance
        axisRenderer.material = new Material(Shader.Find("Sprites/Default"));
        axisRenderer.useWorldSpace = true;

        CreateLabel(name, endPosition, labelSize, axis, "end");
    }

    void CreateLabel(string axisLabel, Vector3 position, int fontsize, string axis, string com = null)
    {
        // Create a GameObject for the label
        GameObject labelObject = new GameObject(axisLabel + " Label");
        labelObject.transform.SetParent(this.transform);
        labelObject.transform.position = position;

        // Offset rotation for each axis
        if (axis == "X") { labelObject.transform.rotation = Quaternion.Euler(0, rotation + 180f, 0); }
        else if (axis == "Y") { labelObject.transform.rotation = Quaternion.Euler(0, rotation + 225f, 0); }
        else if (axis == "Z") { labelObject.transform.rotation = Quaternion.Euler(0, rotation + 270f, 0); }

        // Add a TextMeshPro component to display the label
        TextMeshPro textMeshPro = labelObject.AddComponent<TextMeshPro>();

        // Set the text, font size, and other properties
        textMeshPro.text = axisLabel;
        textMeshPro.fontSize = fontsize;  // Adjust the size of the label (higher value = bigger text)
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = Color.white;  // Label color

        // Optionally adjust label position if needed (slightly offset)
        if (com == "end")
        {
            labelObject.transform.position += new Vector3(0, labelOffset, 0); // Offset slightly upwards
        }
        else
        {
            labelObject.transform.position += new Vector3(0, labelOffset, 0); // Offset slightly upwards
        }
    }

    void CreateScaleLabels(LineRenderer axisRenderer, Vector3 dir, string axis)
    {
        // Calcular o tamanho do eixo e o espaço disponível para as labels
        int divisions = 0;
        float min = 0, max = 0;
        float axisLength = dir.magnitude;

        if (axis == "X")
        { 
            divisions = divisionsX;
            min = minLuminosity;
            max = maxLuminosity;
        }
        else if (axis == "Y") 
        { 
            divisions = divisionsY;
            min = minHumidity;
            max = maxHumidity;
        }
        else if (axis == "Z") { divisions = divisionsZ; }

        // Ajustar o tamanho da fonte com base no espaço disponível
        float spaceBetweenLabels = axisLength / divisions;
        int adjustedFontSize = AdjustLabelFontSize(spaceBetweenLabels);

        if (axis != "Z")
        {
            // Loop para criar as labels em cada ponto de escala ao longo do eixo
            for (int i = 0; i < divisions; i++)
            {
                // Calcular a posição para cada label
                Vector3 position = dir / divisions * (i + 1);
                float value = (max - min) / divisions * (i + 1) + min;

                // Criar uma label para cada divisão
                CreateLabel(value.ToString(), position, adjustedFontSize, axis);
            }
        }

        else
        {
            for (int i = 0; i < divisions; i++)
            {
                // Calcular a posição para cada label
                Vector3 position = dir / divisions * (i + 1);

                // Criar uma label para cada divisão
                CreateLabel(((i+1)*Ndays / divisions).ToString(), position, adjustedFontSize, axis);
            }
        }
    }

    int AdjustLabelFontSize(float spaceBetweenLabels)
    {
        // Definir tamanhos de fonte
        int baseFontSize = 3;
        int maxFontSize = 10;
        int minFontSize = 1;

        // Calcular o tamanho da fonte com base no espaço disponível
        int adjustedFontSize = Mathf.FloorToInt(baseFontSize * (spaceBetweenLabels / 1f));

        // Limitar o tamanho da fonte para garantir que ele não seja nem muito grande nem muito pequeno
        adjustedFontSize = Mathf.Clamp(adjustedFontSize, minFontSize, maxFontSize);

        return adjustedFontSize;
    }

    void CreateGridXY(Vector3 offset, float lengthX, float lengthY, int divisionsX, int divisionsY)
    {
        for (int i = 0; i <= divisionsX; i++)
        {
            float x = i * (lengthX / divisionsX);
            CreateLine(new Vector3(x, 0, 0), new Vector3(x, lengthY, 0), Color.gray);  
        }
        for (int i = 0; i <= divisionsY; i++)
        {
            float y = i * (lengthY / divisionsY);
            CreateLine(new Vector3(0, y, 0), new Vector3(lengthX, y, 0) , Color.gray);
            
        }

    }

    void CreateGridYZ(Vector3 offset, float lengthY, float lengthZ, int divisionsY, int divisionsZ)
    {
        // Gerar as linhas no eixo Y
        for (int i = 0; i <= divisionsY; i++)
        {
            float y = i * (lengthY / divisionsY);
            CreateLine(new Vector3(0, y, 0), new Vector3(0, y, lengthZ), Color.gray);  // Linha ao longo do eixo Z
        }

        // Gerar as linhas no eixo Z
        for (int i = 0; i <= divisionsZ; i++)
        {
            float z = i * (lengthZ / divisionsZ);
            CreateLine(new Vector3(0, 0, z), new Vector3(0, lengthY, z), Color.gray);  // Linha ao longo do eixo Y
        }
    }

    void CreateGridXZ(Vector3 offset, float lengthX, float lengthZ, int divisionsX, int divisionsZ)
    {
        // Gerar as linhas no eixo X
        for (int i = 0; i <= divisionsX; i++)
        {
            float x = i * (lengthX / divisionsX);
            CreateLine(new Vector3(x, 0, 0), new Vector3(x, 0, lengthZ), Color.gray);  // Linha ao longo do eixo Z
        }

        // Gerar as linhas no eixo Z
        for (int i = 0; i <= divisionsZ; i++)
        {
            float z = i * (lengthZ / divisionsZ);
            CreateLine(new Vector3(0, 0, z), new Vector3(lengthX, 0, z), Color.gray);  // Linha ao longo do eixo X
        }
    }

    void CreateLine(Vector3 start, Vector3 end, Color lineColor)
    {
        GameObject lineObject = new GameObject("Grid Line");
        lineObject.transform.SetParent(this.transform);
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 0.005f;  // Make lines very thin
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useWorldSpace = true;
    }

    Vector3 yRotation(Vector3 v, float angle, string type)
    {
        if (type == "deg") { angle = angle * Mathf.Deg2Rad; }

        Matrix4x4 R = new Matrix4x4();
        R.SetRow(0, new Vector4(Mathf.Cos(angle), 0, Mathf.Sin(angle), 0));
        R.SetRow(1, new Vector4(0, 1, 0, 0));
        R.SetRow(2, new Vector4(-Mathf.Sin(angle), 0, Mathf.Cos(angle), 0));
        R.SetRow(3, new Vector4(0, 0, 0, 1));

        return R.MultiplyPoint3x4(v);
    }

   void CreateTextCanvasAtPosition(float xLength, float zLength)
    {
        // Definir a posição do Canvas
        Vector3 canvasPosition = new Vector3(xLength, 1f, zLength / 2);

        // Criar um GameObject para o Canvas
        GameObject canvasObject = new GameObject("TextCanvas");
        canvasObject.transform.SetParent(this.transform);

        // Adicionar o componente Canvas
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Atribuir a câmera principal ao Canvas
        Camera mainCamera = Camera.main;
        canvas.worldCamera = mainCamera;

        // Adicionar o componente CanvasScaler para escalonamento adequado
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.dynamicPixelsPerUnit = 10f;

        // Adicionar o componente GraphicRaycaster (opcional se você precisar de interação)
        canvasObject.AddComponent<GraphicRaycaster>();

        // Definir o tamanho do Canvas para 3x4 e aplicar a escala de 0.2
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(8f, 2f); // Tamanho do canvas 3x4
        canvasRect.localScale = new Vector3(0.2f, 0.2f, 0.2f); // Escalonar o canvas

        // Definir a posição do Canvas
        canvasRect.position = canvasPosition;

        // Rotacionar o Canvas -45 graus ao longo do eixo Y
       // canvasRect.rotation = Quaternion.Euler(0f, -45f, 0f); // Rotação do Canvas

        // Criar o GameObject para o texto dentro do Canvas
        GameObject textObject = new GameObject("CanvasText");
        textObject.transform.SetParent(canvasObject.transform);

        // Adicionar o componente TextMeshProUGUI
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        RectTransform textRect = text.GetComponent<RectTransform>();

        // Definir o tamanho do texto para corresponder ao tamanho do Canvas
        textRect.sizeDelta = new Vector2(6f, 2f); // O tamanho do texto será igual ao tamanho do Canvas
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Definir a escala do texto para 1 (tamanho normal)
        textRect.localScale = Vector3.one; // Definir a escala do texto para 1

        // Ativar auto redimensionamento e definir o tamanho mínimo
        text.enableAutoSizing = true;
        text.fontSizeMin = 0.3f; // Tamanho mínimo do texto
        text.fontSizeMax = 10f;  // Tamanho máximo do texto (você pode ajustar conforme necessário)

        // Definir as propriedades do texto
        text.text = texto; // O texto a ser exibido
        text.fontSize = 1f;   // O tamanho inicial do texto, o auto-redimensionamento ajustará conforme necessário
        text.alignment = TextAlignmentOptions.Center; // Centralizar o texto
        text.color = Color.white; // Definir a cor do texto

        // Opcionalmente ajustar a aparência do texto
        text.enableWordWrapping = true;  // Permitir a quebra de palavras
        text.overflowMode = TextOverflowModes.Ellipsis; // Lidar com transbordamento de texto, se necessário

        // **Manter a rotação do texto em 0 para que ele não seja afetado pela rotação do Canvas**
        // Isso "desfaz" a rotação aplicada ao Canvas para o texto


        // Garantir que o texto tenha posição z = 0 em relação ao canvas
        textRect.localPosition = new Vector3(textRect.localPosition.x, textRect.localPosition.y, 0f);
        canvasRect.rotation = Quaternion.Euler(0f, 90f, 0f); // Rotação do Canvas
    }




}
