using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Particle = UnityEngine.ParticleSystem.Particle;

public class Z3toR2 : MonoBehaviour
{
    private Vector3[] normalizedZ3;

    private ParticleSystem ps;

    Quaternion rot;

    [Range(2, 30)]
    public int resolution = 11;
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

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        CreatePoints();

        rot = Quaternion.identity;
        inv = Vector3.zero;

        camT = Camera.main.transform;
    }

    private void CreatePoints()
    {
        resolution = Mathf.Clamp(resolution, 2, 30);
        currentResolution = resolution;

        int newRes = 2 * resolution + 1;

        int amountOfParticles = (int)Mathf.Pow(newRes, 3) - 1;

        points = new Particle[amountOfParticles * 2];
        normalizedZ3 = new Vector3[amountOfParticles];

        Vector3 latticePoint = Vector3.zero;

        int halfRes = newRes / 2;
        float resoRecip = 1f / halfRes;

        for(int x = 0; x < newRes; ++x)
            for(int y = 0; y < newRes; ++y)
                for(int z = 0; z < newRes; ++z)
                {
                    if(x != 0 || y != 0 || z!= 0)
                    {
                        int index = (x * newRes * newRes + y * newRes + z) - 1;
                        //latticePoint.Set(x - halfRes, y - halfRes, z - halfRes);

                        latticePoint.Set(x / (float)newRes, y / (float)newRes, z / (float)newRes);
                        latticePoint += Vector3.one * -.5f;

                        var norm = latticePoint.normalized;
                        normalizedZ3[index] = norm;

                        var p = new Vector3(norm.x / (1 - norm.y), 0f, norm.z / (1 - norm.y));

                        bool clear = false;

                        if (Mathf.Min(x,y,z) == 0 || Mathf.Max(x,y,z) == newRes - 1)
                            clear = true;
                        

                        points[index].position = p;
                        points[index].color = !clear ? Color.clear : new Color(Mathf.Abs(x) * resoRecip, Mathf.Abs(y) * resoRecip, Mathf.Abs(z) * resoRecip);
                        points[index].size = ParticleSize;
                        points[index + amountOfParticles].position = new Vector3(norm.x, norm.y + .5f, norm.z);
                        points[index + amountOfParticles].color = !clear ? Color.clear : new Color(Mathf.Abs(x) * resoRecip, Mathf.Abs(y) * resoRecip, Mathf.Abs(z) * resoRecip);
                        points[index + amountOfParticles].size = ParticleSize;
                    }
                }
    }

    void Update()
    {
        if (resolution != currentResolution)
            CreatePoints();

        float inX = Input.GetAxis("Horizontal");
        float inY = Input.GetAxis("Vertical");

        inv.Set(inX, 0, inY);

        Quaternion rotInv = Quaternion.Inverse(rot);

        inv = rotInv * inv;
        
        var toVec = rotInv *    new Vector3(0, -1, 0);
        

        Quaternion fromTo = Quaternion.FromToRotation(inv.normalized, toVec);
        Quaternion rotBy = Quaternion.Lerp(Quaternion.identity, fromTo, inv.magnitude * rotationSpeed);

        rot *= rotBy;

        int newRes = 2 * resolution + 1;

        if (Input.GetKeyDown(KeyCode.I))
        {
            bool inverse = !pslider.gameObject.activeSelf;

            pslider.gameObject.SetActive(inverse);
            slider.gameObject.SetActive(inverse);
        }


        if(Input.GetKeyDown(KeyCode.Z))
        {
          camT.position += camT.forward * .25f;
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
          camT.position -= camT.forward * .25f;
        }

        for (int x = 0; x < newRes; ++x)
            for (int y = 0; y < newRes; ++y)
                for (int z = 0; z < newRes; ++z)
                {
                    if (x != 0 || y != 0 || z != 0)
                    {
                        int index = (x * newRes * newRes + y * newRes + z) - 1;
                        var rotate = rot * normalizedZ3[index];

                        var p = new Vector3(rotate.x / (1 - rotate.y), 0f, rotate.z / (1 - rotate.y));

                        points[index].position = p;
                        points[index].size = ParticleSize;

                        points[index + normalizedZ3.Length].position = new Vector3(rotate.x, rotate.y + 1f, rotate.z);
                        points[index + normalizedZ3.Length].size = ParticleSize;
                    }
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
}
