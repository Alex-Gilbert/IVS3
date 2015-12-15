using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using Particle = UnityEngine.ParticleSystem.Particle;

public class ParticlePlot : MonoBehaviour
{
    private const float _radius = .4f;

    private ParticleSystem _particleSystem;
    private Particle[] _particles;
    private float _curSize;

    private Quaternion _baseRotation;
    private Vector3 _inV;
    private Quaternion[] _rotations;

    [SerializeField] private TextAsset _jsonData;

    private DataPlot _dataPlot;

    private Transform _camT;

    float ParticleSize = .3f;

    [SerializeField] private Slider _sizeSlider;
    [SerializeField] private InputField _xPos, _yPos, _zPos;
    [SerializeField] private Text _infoText;

    public float RotationSpeed = .05f;



    public void Start()
    {
        _dataPlot = LoadJson.Instance.Load(_jsonData.text);
        _particles = new Particle[_dataPlot.Particles.Length];
        _particleSystem = GetComponent<ParticleSystem>();
        _camT = Camera.main.transform;
        _baseRotation = Quaternion.identity;
        _rotations = new[] {Quaternion.identity, Quaternion.identity};

        _inV = Vector3.zero;

        _sizeSlider.value = ParticleSize;

        //Create Points
        for (int index = 0; index < _dataPlot.Particles.Length; index++)
        {
            var p = _dataPlot.Particles[index];
            
            var normal = new Vector4(p.Position.x, p.Position.y, p.Position.z, p.Position.w);
            normal.Normalize();

            _particles[index] = new Particle()
            {
                position = normal.StereographicProjection(),
                color = p.Color,
                size = p.Size
            };
        }

        _particleSystem.SetParticles(_particles, _particles.Length);

        SelectFile.Instance.FileSelected += CreatePoints;

        _xPos.text = "0";
        _yPos.text = "0";
        _zPos.text = "0";
    }

    public void CreatePoints(string filePath)
    {
        _dataPlot = LoadJson.Instance.LoadFromFile(filePath);
        _particles = new Particle[_dataPlot.Particles.Length];

        for (int index = 0; index < _dataPlot.Particles.Length; index++)
        {
            var p = _dataPlot.Particles[index];

            var normal = new Vector4(p.Position.x, p.Position.y, p.Position.z, p.Position.w);
            normal.Normalize();

            _particles[index] = new Particle()
            {
                position = normal.StereographicProjection(),
                color = p.Color,
                size = p.Size
            };
        }

        _particleSystem.SetParticles(_particles, _particles.Length);
    }

    public void Update()
    {
        ParticleSize = _sizeSlider.value;

        int xpos = (int)_camT.position.x;
        int ypos = (int)_camT.position.y;
        int zpos = (int)_camT.position.z;
        int.TryParse(_xPos.text, out xpos);
        int.TryParse(_yPos.text, out ypos);
        int.TryParse(_zPos.text, out zpos);
        _camT.position = new Vector3(xpos, ypos, zpos);

        if (MouseControls.rotate)
        {
            var inX = Input.GetAxis("Horizontal");
            var inY = Input.GetAxis("Vertical");

            _inV = _camT.right * inX + _camT.forward * inY;

            var invn = _inV.normalized;

            var inq = new Quaternion(invn.x, invn.y, invn.z, 0);

            _baseRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Inverse(inq), Time.deltaTime * _inV.magnitude * .25f);

            _rotations[0] = _baseRotation * _rotations[0];
            _rotations[1] = _rotations[1] * _baseRotation;

            for (int index = 0; index < _dataPlot.Particles.Length; index++)
            {
                var p = _dataPlot.Particles[index];

                var rot = _rotations[0] * p.Position * _rotations[1];
                
                _particles[index].position = rot.StereographicProjection();
                _particles[index].size = p.Size * ParticleSize;
                _particles[index].color = p.Color;
            }
        }
        else
        {
            var mouseP = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, Camera.main.nearClipPlane));//Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var mouseVec = mouseP - _camT.position;
            mouseVec.Normalize();

            float minDistance = 10000f;
            int curPoint = -1;
            for (int index = 0; index < _particles.Length; index++)
            {
                var p = _particles[index];

                _particles[index].color = _dataPlot.Particles[index].Color;

                var newVec = p.position - _camT.position;

                var dot = Vector3.Dot(mouseVec, newVec);
                var magSquared = newVec.sqrMagnitude;
                var r = _radius * p.size;
                var radSquared = r * r;

                if (dot * dot >= ((magSquared * magSquared) / (magSquared + radSquared)) && dot > 0)
                {
                    if (curPoint == -1)
                    {
                        minDistance = Vector3.Distance(_camT.position, p.position);
                        curPoint = index;
                    }
                    else
                    {
                        float newDistance = Vector3.Distance(_camT.position, p.position);
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
                _particles[curPoint].color = Color.white;
                _infoText.text = Info(curPoint);
            }
        }

        _particleSystem.SetParticles(_particles, _particles.Length);
    }

    private string Info(int index)
    {
        StringBuilder sb = new StringBuilder();
        var particle = _dataPlot.Particles[index];

        for (int i = 0; i < _dataPlot.Properties.Length; ++i)
        {
            var prop = _dataPlot.Properties[i];
            

            sb.Append(prop.Name + ": ");

            if(prop.Type < 3)
                sb.Append(particle.Props[i].ToString() + "\n");
            else
            {
                sb.Append(_dataPlot.Enums[prop.Type - 3].values[TakeOnlyNumbers(particle.Props[i].ToString())] + "\n");
            }
        }
        return sb.ToString();
    }

    private int TakeOnlyNumbers(string s)
    {
        StringBuilder sb= new StringBuilder();

        foreach (char c in s.ToCharArray())
        {
            if ((int) c >= 48 && (int) c <= 57)
                sb.Append(c);
        }

        return int.Parse(sb.ToString());
    }
}
