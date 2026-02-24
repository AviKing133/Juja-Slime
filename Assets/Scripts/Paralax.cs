using UnityEditor.SpeedTree.Importer;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    Transform cam;
    Vector3 camStartPos;
    float distance;

    GameObject[] backgrounds;
    Material[] materials;
    float[] backSpeed;

    float farthestBackground;

    [Range(0.01f, 0.05f)]
    public float paralaxSpeed;


    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;
        int backgroundCount = transform.childCount;
        materials = new Material[backgroundCount];
        backSpeed = new float[backgroundCount];
        backgrounds = new GameObject[backgroundCount];

        for (int i = 0; i < backgroundCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            materials[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        backSpeedCalculator(backgroundCount);
    }

    public void backSpeedCalculator(int backgroundCount)
    {
        for(int i = 0; i < backgroundCount; i++)
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBackground)
            {
                farthestBackground = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backgroundCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBackground;
        }
    }
    void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x - 1, transform.position.y, 8.57f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * paralaxSpeed;
            materials[i].mainTextureOffset = new Vector2(distance * speed, 0);
        }
    }
}
