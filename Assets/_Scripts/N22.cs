using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Particle = UnityEngine.ParticleSystem.Particle;

public class N22 : MonoBehaviour
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

        for(int i = 1; i < lines.Length - 1; ++i)
        {
            var line = lines[i].Split(' ');
            
            var normal = new Vector4(float.Parse(line[3]), float.Parse(line[4]), float.Parse(line[5]), float.Parse(line[6]));

            normal.Normalize();
            

            data[i - 1] = new Quaternion(normal.x, normal.y, normal.z, normal.w);
            int type = int.Parse(line[1]);

            var particle = new Particle()
            {
                position = StereographicProjection(normal),
                size = ParticleSize,//Mathf.Abs(ParticleSize * float.Parse(line[2])),
                color = type == 0 ? Color.red :
                        type == 1 ? Color.green : 
                        type == 2 ? Color.blue : 
                        type == 3 ? Color.yellow : 
                        Color.magenta
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
