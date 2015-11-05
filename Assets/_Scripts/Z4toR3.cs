using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Particle = UnityEngine.ParticleSystem.Particle;

public class Z4toR3 : MonoBehaviour
{
    private Quaternion[] normalizedZ4;
    private ParticleSystem ps;

    Quaternion rotation;

    Quaternion[] rotationTest;

    [Range(0, 11)]
    public int resolution = 2;
    private int currentResolution;

    [Range(.005f, .03f)]
    public float ParticleSize = .01f;
    private float currentSize;

    Particle[] points;

    Vector3 inv;

    float rotationSpeed = .05f;

    private Transform camT;

    public Slider slider;
    public Slider pslider;

    Quaternion bottom;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        CreatePoints();
        
        inv = Vector3.zero;

        camT = Camera.main.transform;
        rotation = Quaternion.identity;

        bottom = new Quaternion(0, 0, 0, 1);

        rotationTest = new Quaternion[] { Quaternion.identity, Quaternion.identity };
    }

    private void CreatePoints()
    {
        resolution = Mathf.Clamp(resolution, 0, 11);
        currentResolution = resolution;

        int newRes = 2 * resolution + 1;

        int amountOfParticles = (int)Mathf.Pow(newRes, 4) - 1;

        points = new Particle[amountOfParticles];
        normalizedZ4 = new Quaternion[amountOfParticles];

        Vector4 latticePoint = Vector4.zero;

        int halfRes = newRes / 2;
        float resoRecip = 1f / halfRes;

        /*for (int x = 0; x < newRes; ++x)
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
                        }*/

        for (int x = 0; x < newRes; ++x)
        {
            for (int y = 0; y < newRes; ++y)
            {
                for (int z = 0; z < newRes; ++z)
                {
                    for (int w = 0; w < newRes; ++w)
                    {
                        if (x != 0 || y != 0 || z != 0 || w != 0)
                        {
                            int index = (x * newRes * newRes * newRes + y * newRes * newRes + z * newRes + w) - 1;
                            latticePoint.Set(x / (float)newRes, y / (float)newRes, z / (float)newRes, w / (float)newRes);
                            latticePoint -= Vector4.one * .5f;

                            var norm = latticePoint.normalized;

                            normalizedZ4[index] = new Quaternion(norm.x, norm.y, norm.z, norm.w);

                            int numberOfZero = 0;
                            if (x == 0 || x == newRes - 1)
                                numberOfZero++;
                            if (y == 0 || y == newRes - 1)
                                numberOfZero++;
                            if (z == 0 || z == newRes - 1)
                                numberOfZero++;
                            if (w == 0 || w == newRes - 1)
                                numberOfZero++;


                            var p = new Vector3(norm.x / (1 - norm.w), norm.y / (1 - norm.w), norm.z / (1 - norm.w));

                            points[index].position = p;
                            points[index].color = numberOfZero == 3 ? Color.red : numberOfZero == 2 ? Color.yellow : Color.clear;
                            points[index].size = numberOfZero == 3 ? ParticleSize * 8 : numberOfZero == 2 ? ParticleSize * 4 : ParticleSize * 0;
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        float inX = Input.GetAxis("Horizontal");
        float inY = Input.GetAxis("Vertical");

        inv = camT.right * inX + -camT.forward * inY;
        
        Vector3 invn = inv.normalized;
        
        Quaternion inq = new Quaternion(invn.x, invn.y, invn.z, 0);
        Quaternion referentialshift = Quaternion.Inverse(inq) * bottom;

        rotation = Quaternion.Lerp(Quaternion.identity, referentialshift, Time.deltaTime * inv.magnitude * .25f);

        rotationTest[0] = rotation * rotationTest[0];
        rotationTest[1] = rotationTest[1] * rotation;

        for (int i = 0; i < normalizedZ4.Length; i++)
        {
            /*normalizedZ4[i] = rotation * normalizedZ4[i] * rotation;

            var p = new Vector3(normalizedZ4[i].x / (1 - normalizedZ4[i].w), normalizedZ4[i].y / (1 - normalizedZ4[i].w), normalizedZ4[i].z / (1 - normalizedZ4[i].w));*/

            var rot = rotationTest[0] * normalizedZ4[i] * rotationTest[1];
            var p = new Vector3(rot.x / (1 - rot.w),rot.y / (1 - rot.w), rot.z / (1 - rot.w)); 
            points[i].position = p;
        }

        ps.SetParticles(points, points.Length);
    }

    public void ChangeResolution()
    {
        resolution = (int)slider.value;
    }

    public void ChangeSize()
    {
        ParticleSize = pslider.value;
    }

    private Vector4 multByQ(Vector4 V, Quaternion Q)
    {
        Quaternion vq = new Quaternion(V.x, V.y, V.z, V.w);

        Quaternion mult = Q * vq;

        return new Vector4(mult.x, mult.y, mult.z, mult.w);
    }

    private Matrix4x4 QuatToMat(Quaternion quat)
    {
        Matrix4x4 toReturn = new Matrix4x4();
        float xx2 = 2 * quat.x * quat.x;
        float yy2 = 2 * quat.y * quat.y;
        float zz2 = 2 * quat.z * quat.z;
        float xy = 2 * quat.x * quat.y;
        float xz = 2 * quat.x * quat.z;
        float yz = 2 * quat.y * quat.z;
        float wx = 2 * quat.w * quat.x;
        float wy = 2 * quat.w * quat.y;
        float wz = 2 * quat.w * quat.z;

        toReturn[1, 1] = 1 - yy2 - zz2;
        toReturn[1, 2] = xy - wz;
        toReturn[1, 3] = xz + wy;
        toReturn[1, 0] = 0;

        toReturn[2, 1] = xy + wz;
        toReturn[2, 2] = 1 - xx2 - zz2;
        toReturn[2, 3] = yz + wx;
        toReturn[2, 0] = 0;

        toReturn[3, 1] = xz - wy;
        toReturn[3, 2] = yz + wx;
        toReturn[3, 3] = 1 - xx2 - yy2;
        toReturn[3, 0] = 0;

        toReturn[0, 1] = 0;
        toReturn[0, 2] = 0;
        toReturn[0, 3] = 0;
        toReturn[0, 0] = 1;

        return toReturn;
    }   
}
