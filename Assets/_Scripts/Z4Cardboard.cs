using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Particle = UnityEngine.ParticleSystem.Particle;

public class Z4Cardboard : MonoBehaviour
{
    private bool rotateAround;

    private Quaternion[] normalizedZ4;

    private ParticleSystem ps;

    private Quaternion rotation;

    [Range(0, 8)]
    public int resolution = 2;
    private int currentResolution;

    [Range(.005f, .03f)]
    public float ParticleSize = .01f;
    private float currentSize;

    Particle[] points;

    Vector3 inv;

    public float rotationSpeed = .05f;

    private Transform camT;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        CreatePoints();

        rotateAround = false;

        GameObject.FindGameObjectWithTag("Cardboard").GetComponent<Cardboard>().OnTrigger += ToggleRotateAround;

        

        //rot = Matrix4x4.identity;
        inv = Vector3.zero;

        camT = Camera.main.transform;
        rotation = Quaternion.identity;
    }

    private void CreatePoints()
    {
        resolution = Mathf.Clamp(resolution, 0, 8);
        currentResolution = resolution;

        int newRes = 2 * resolution + 1;

        int amountOfParticles = (int)Mathf.Pow(newRes, 4) - 1;

        points = new Particle[amountOfParticles];
        normalizedZ4 = new Quaternion[amountOfParticles];

        Vector4 latticePoint = Vector4.zero;

        int halfRes = newRes / 2;
        float resoRecip = 1f / halfRes;

        for (int x = 0; x < newRes; ++x)
            for (int y = 0; y < newRes; ++y)
                for (int z = 0; z < newRes; ++z)
                    for (int w = 0; w < newRes; ++w)
                        if (x != 0 || y != 0 || z != 0 || w != 0)
                        {
                            int index = (x * newRes * newRes * newRes + y * newRes * newRes + z * newRes + w) - 1;
                            latticePoint.Set(x - halfRes, y - halfRes, z - halfRes, w - halfRes);

                            var norm = latticePoint.normalized;

                            normalizedZ4[index] = new Quaternion(norm.x, norm.y, norm.z, norm.w);

                            var p = new Vector3(norm.x / (1 - norm.w), norm.y / (1 - norm.w), norm.z / (1 - norm.w));

                            points[index].position = p;
                            points[index].color = new Color(Mathf.Abs(x) * resoRecip, Mathf.Abs(y) * resoRecip, Mathf.Abs(z) * resoRecip);
                            points[index].size = ParticleSize * norm.w;
                        }
    }

    void Update()
    {
        //if (resolution != currentResolution)
        //    CreatePoints();



        if (rotateAround)
        {
            inv = camT.up * 1;

            Quaternion fromTo = Quaternion.FromToRotation(camT.forward, inv);
            Quaternion referentialShift = Quaternion.Lerp(Quaternion.identity, fromTo, rotationSpeed * Time.deltaTime);

            //Debug.DrawLine(camT.position, camT.position + inv);
            //Matrix4x4 rotBy = QuatToMat(referentialShift);

            rotation *= referentialShift;
        }

        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    bool inverse = !pslider.gameObject.active;

        //    pslider.gameObject.SetActive(inverse);
        //    slider.gameObject.SetActive(inverse);
        //}

        int newRes = 2 * resolution + 1;

        for (int x = 0; x < newRes; ++x)
            for (int y = 0; y < newRes; ++y)
                for (int z = 0; z < newRes; ++z)
                    for (int w = 0; w < newRes; ++w)
                        if (x != 0 || y != 0 || z != 0 || w != 0)
                        {
                            int index = (x * newRes * newRes * newRes + y * newRes * newRes + z * newRes + w) - 1;
                            //var rotate = rot * normalizedZ4[index];
                            
                            var rotate = rotation * normalizedZ4[index];

                            var p = new Vector3(rotate.x / (1 - rotate.w), rotate.y / (1 - rotate.w), rotate.z / (1 - rotate.w));

                            points[index].position = p;
                            points[index].size = ParticleSize;
                        }
        ps.SetParticles(points, points.Length);
    }

    public void ToggleRotateAround()
    {
        rotateAround = !rotateAround;
    }
}
