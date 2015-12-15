using UnityEngine;
using System.Collections;

using Particle = UnityEngine.ParticleSystem.Particle;

public class S05 : MonoBehaviour
{
    public TextAsset dataSet;
    private Quaternion[] data;
    private ParticleSystem ps;

    Quaternion baseRotation;

    Quaternion[] rotations;

    [Range(.005f, .04f)]
    public float ParticleSize = .03f;
    private float currentSize;

    Particle[] points;

    Vector3 inv;

    float rotationSpeed = .05f;

    private Transform camT;

    Quaternion bottom;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        CreatePoints();

        inv = Vector3.zero;

        Debug.Log("I'm doing somehting");

        camT = Camera.main.transform;
        baseRotation = Quaternion.identity;

        bottom = new Quaternion(0, 0, 0, 1);

        rotations = new Quaternion[] { Quaternion.identity, Quaternion.identity };
    }

    private void CreatePoints()
    {
        var lines = dataSet.text.Split('\n');

        points = new Particle[lines.Length - 2];//lines.Length - 1];
        data = new Quaternion[lines.Length - 2];//lines.Length - 1];

        for (int i = 1; i < lines.Length - 1; ++i)
        {
            var line = lines[i].Split(' ');

            var normal = new Vector4(float.Parse(line[2]), float.Parse(line[3]), float.Parse(line[4]), float.Parse(line[5]));

            normal.Normalize();


            data[i - 1] = new Quaternion(normal.x, normal.y, normal.z, normal.w);
            

            var particle = new Particle()
            {
                position = StereographicProjection(normal),
                size = ParticleSize,//Mathf.Abs(ParticleSize * float.Parse(line[2])),
                color = Color.white
            };

            points[i - 1] = particle;
        }

        ps.SetParticles(points, points.Length);
    }

    Vector3 StereographicProjection(Vector4 v4)
    {
        if (v4.w == 1)
        {
            return Vector3.one * 100;
        }
        return new Vector3(v4.x / (v4.w - 1), v4.y / (v4.w - 1), v4.z / (v4.w - 1));
    }

    void Update()
    {
        float inX = Input.GetAxis("Horizontal");
        float inY = Input.GetAxis("Vertical");

        inv = -camT.right * inX + -camT.forward * inY;

        Vector3 invn = inv.normalized;

        Quaternion inq = new Quaternion(invn.x, invn.y, invn.z, 0);
        Quaternion referentialshift = Quaternion.Inverse(inq) * bottom;

        baseRotation = Quaternion.Lerp(Quaternion.identity, referentialshift, Time.deltaTime * inv.magnitude * .25f);

        rotations[0] = baseRotation * rotations[0];
        rotations[1] = rotations[1] * baseRotation;

        for (int i = 0; i < data.Length; i++)
        {
            /*normalizedZ4[i] = rotation * normalizedZ4[i] * rotation;

            var p = new Vector3(normalizedZ4[i].x / (1 - normalizedZ4[i].w), normalizedZ4[i].y / (1 - normalizedZ4[i].w), normalizedZ4[i].z / (1 - normalizedZ4[i].w));*/

            var rot = rotations[0] * data[i] * rotations[1];
            var p = new Vector3(rot.x / (1 - rot.w), rot.y / (1 - rot.w), rot.z / (1 - rot.w));
            points[i].position = p;
            points[i].size = ParticleSize;
        }

        ps.SetParticles(points, points.Length);
    }
    
}
