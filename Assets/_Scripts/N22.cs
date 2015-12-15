using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.Remoting;
using Particle = UnityEngine.ParticleSystem.Particle;

public class N22 : MonoBehaviour
{
    public TextAsset dataSet;
    private Quaternion[] data;
    private string[] words;
    private int[] types;
    private ParticleSystem ps;

    public float Radius;

    Quaternion baseRotation;

    Quaternion[] rotations;

    [Range(.005f, 1f)]
    public float ParticleSize = .03f;
    private float currentSize;

    Particle[] points;

    Vector3 inv;

    float rotationSpeed = .05f;

    private Transform camT;

    Quaternion bottom;

    public Text wordText;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        CreatePoints();

        inv = new Vector3(0,0,0);

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
        words = new string[lines.Length - 2];//lines.Length - 1];
        types = new int[lines.Length - 2];//lines.Length - 1];

        for (int i = 1; i < lines.Length - 1; ++i)
        {
            var line = lines[i].Split(' ');
            
            var normal = new Vector4(float.Parse(line[3]), float.Parse(line[4]), float.Parse(line[5]), float.Parse(line[6]));

            normal.Normalize();
            

            data[i - 1] = new Quaternion(normal.x, normal.y, normal.z, normal.w);
            words[i - 1] = line[0];
            int type = int.Parse(line[1]);
            types[i - 1] = type;
            float length = ParticleSize * Mathf.Pow((1/float.Parse(line[2])), 1.2f);
            var particle = new Particle()
            {
                position = StereographicProjection(normal),
                size = length,//Mathf.Abs(ParticleSize * float.Parse(line[2])),
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
        if (v4.w >= 1)
        {
            return Vector3.one * 100;
        }
        return new Vector3(v4.x / (v4.w - 1), v4.y / (v4.w - 1), v4.z / (v4.w - 1));
    }

    void Update()
    {
        if (MouseControls.rotate)
        {
            var inX = Input.GetAxis("Horizontal");
            var inY = Input.GetAxis("Vertical");

            inv = -camT.right*inX + -camT.forward*inY;

            var invn = inv.normalized;

            var inq = new Quaternion(invn.x, invn.y, invn.z, 0);
            var referentialshift = Quaternion.Inverse(inq)*bottom;

            baseRotation = Quaternion.Lerp(Quaternion.identity, referentialshift, Time.deltaTime*inv.magnitude*.25f);

            rotations[0] = baseRotation*rotations[0];
            rotations[1] = rotations[1]*baseRotation;

            for (var i = 0; i < data.Length; i++)
            {
                /*normalizedZ4[i] = rotation * normalizedZ4[i] * rotation;

            var p = new Vector3(normalizedZ4[i].x / (1 - normalizedZ4[i].w), normalizedZ4[i].y / (1 - normalizedZ4[i].w), normalizedZ4[i].z / (1 - normalizedZ4[i].w));*/

                var rot = rotations[0]*data[i]*rotations[1];
                var p = new Vector3(rot.x/(1 - rot.w), rot.y/(1 - rot.w), rot.z/(1 - rot.w));
                points[i].position = p;
                //points[i].size = ParticleSize;
            }
        }
        else
        {
            var mouseP = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, Camera.main.nearClipPlane));//Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var mouseVec = mouseP - camT.position;
            mouseVec.Normalize();
            
            float minDistance = 10000f;
            int curPoint = -1;
            for (int index = 0; index < points.Length; index++)
            {
                var p = points[index];
                points[index].color = types[index] == 0
                    ? Color.red
                    : types[index] == 1
                        ? Color.green
                        : types[index] == 2
                            ? Color.blue
                            : types[index] == 3
                                ? Color.yellow
                                : Color.magenta;

                var newVec = p.position - camT.position;

                var dot = Vector3.Dot(mouseVec, newVec);
                var magSquared = newVec.sqrMagnitude;
                var r = Radius*p.size;
                var radSquared = r*r;

                if (dot*dot >= ((magSquared*magSquared)/(magSquared + radSquared)) && dot > 0)
                {
                    if (curPoint == -1)
                    {
                        minDistance = Vector3.Distance(camT.position, p.position);
                        curPoint = index;
                    }
                    else
                    {
                        float newDistance = Vector3.Distance(camT.position, p.position);
                        if (newDistance < minDistance)
                        {
                            minDistance = newDistance;
                            curPoint = index;
                        }
                    }
                }
            }

            if (curPoint != -1)
            {
                points[curPoint].color = Color.white;
                wordText.text = words[curPoint];
            }
        }

        ps.SetParticles(points, points.Length);

    }
}
