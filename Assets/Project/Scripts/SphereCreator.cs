using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;   // Needed for List<T>
using System.Linq;                  // Import LINQ for Max and Min
using TMPro;                        // For TextMeshPro

public class SphereCreator : MonoBehaviour
{
    public GameObject prefab;  // Prefab to instantiate

    public float radius = 1f;                // Default radius of the spheres
    public Color color = Color.red;          // Default color of the spheres
    public int fontSize = 2;                 // Default font size for the labels
    public float minRadius = 0.25f;
    public float maxRadius = 1.5f;
    public Gradient gradient;

    // These variables need to be equal in AxisGenerator
    private float minLuminosity = 300f;
    private float maxLuminosity = 1000f;
    private float minHumidity = 25f;
    private float maxHumidity = 70f;
    private int divisionsZ = 30;
    public float XaxisLength = 5f;      // Length of each axis (meters)
    public float YaxisLength = 5f;
    public float ZaxisLength = 5f;

    public enum TypeOption
    {
        Size,
        Color,
        Label
    }

    public TypeOption typeSelection;

    public float rotation = 0f;

    private string fileName = "room_data.csv";

    public TextAsset csvFile;

    public VRInteractionLoggerCSV logger;

    void Start()
    {
        Debug.Log(csvFile.text);

        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Create a List to store temperatures
        List<float> temperatures = new List<float>();

        // Parse the CSV and collect temperatures
        for (int i = 1; i < lines.Length; i++)  // Skip the header line
        {
            string[] values = lines[i].Split(',');
            if (float.TryParse(values[3], out float t)) // Make sure the temperature is valid
            {
                temperatures.Add(t);
            }
        }

        float minTemperature = temperatures.Min();
        float maxTemperature = temperatures.Max();

        if (typeSelection == TypeOption.Size)
        {
            // Call the function that returns a Tuple
            var parameters = GetLinearParameters(minTemperature, maxTemperature);

            // Access the two floats from the Tuple
            float m = parameters.Item1;
            float b = parameters.Item2;

            // Loop through each line, skipping the first line (header)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                if (float.TryParse(values[0], out float x) && 
                    float.TryParse(values[1], out float y) && 
                    float.TryParse(values[3], out float t)) // Parse X, Y, Z and temperature
                {
                    string z = values[2];
                    Vector3 position = yRotation(convertPos(x, y, z), rotation, "deg");
                    CreateSphere(position, t * m + b, color, "Sphere " + i.ToString());
                }
            }
        }

        else if (typeSelection == TypeOption.Color)
        {
            // Loop through each line and set the color of each sphere based on temperature
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                if (float.TryParse(values[0], out float x) && 
                    float.TryParse(values[1], out float y) && 
                    float.TryParse(values[3], out float t)) // Parse X, Y, Z and temperature
                {
                    string z = values[2];
                    t = Mathf.InverseLerp(minTemperature, maxTemperature, t);
                    Vector3 position = yRotation(convertPos(x, y, z), rotation, "deg");
                    CreateSphere(position, radius, gradient.Evaluate(t), "Sphere " + i.ToString());
                }
            }
        }

        else if (typeSelection == TypeOption.Label)
        {
            // Loop to create labels for each point
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                if (float.TryParse(values[0], out float x) && 
                    float.TryParse(values[1], out float y) && 
                    float.TryParse(values[3], out float t)) // Parse X, Y, Z and temperature
                {
                    string z = values[2];
                    Vector3 position = yRotation(convertPos(x, y, z), rotation, "deg");
                    CreateSphere(position, radius, color, "Sphere " + i.ToString());
                    CreateLabel(t.ToString(), position);
                }
            }
        }
    }

    // Function to create the sphere at a given position and with a given radius
    void CreateSphere(Vector3 position, float radius, Color color, string name)
    {
        var obj = Instantiate(prefab, position, Quaternion.identity);
        obj.name = name;
        obj.transform.localScale = new Vector3(radius, radius, radius);
        obj.transform.SetParent(this.transform);
        obj.GetComponent<Renderer>().material.color = color;
        obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectEntered.AddListener((c) => logger.LogInteraction(obj, "Select"));
    }

    // Function to estimate the linear parameters
    Tuple<float, float> GetLinearParameters(float min, float max)
    {
        float m = (maxRadius - minRadius) / (max - min);
        float b = maxRadius - m * max;

        return new Tuple<float, float>(m, b);
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

    Vector3 convertPos(float x1, float y1, string z1)
    {
        float x2 = XaxisLength / (maxLuminosity - minLuminosity) * (x1 - minLuminosity);
        float y2 = YaxisLength / (maxHumidity - minHumidity) * (y1 - minHumidity);
        float z2 = 0;

        // Extract the last two characters
        string day_s = z1.Substring(z1.Length - 2);

        // Try to convert to an integer
        if (int.TryParse(day_s, out int day))
        {
            z2 = ZaxisLength / divisionsZ * day;
        }
        else
        {
            Debug.Log("Conversion failed. Last two characters are not a valid number.");
        }

        // Create a new Vector3 with the same components
        Vector3 pos = new Vector3(x2, y2, z2);

        // Return the new vector
        return pos;

    }

    void CreateLabel(string label, Vector3 position)
    {
        // Create a GameObject for the label
        GameObject labelObject = new GameObject("Label " + label);
        labelObject.transform.SetParent(this.transform);
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.Euler(0, rotation - 135f, 0);

        // Add a TextMeshPro component to display the label
        TextMeshPro textMeshPro = labelObject.AddComponent<TextMeshPro>();

        // Set the text, font size, and other properties
        textMeshPro.text = label;
        textMeshPro.fontSize = fontSize;  // Adjust the size of the label (higher value = bigger text)
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = Color.white;  // Label color

        labelObject.transform.position += new Vector3(0, radius/2 + 0.1f, 0); // Offset slightly upwards
    }
}
