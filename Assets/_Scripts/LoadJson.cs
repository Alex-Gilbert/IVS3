using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class DataPlot
{
    public string Name { get; set; }
    public ParticleEnum[] Enums { get; set; }
    public Property[] Properties { get; set; }
    public FinalParticle[] Particles { get; set; }

    public DataPlot()
    {
        
    }

    public DataPlot(DataIntermediatePlot dip)
    {
        Name = dip.Name;

        Enums = new ParticleEnum[dip.Enums.Length];
        dip.Enums.CopyTo(Enums, 0);

        Properties = new Property[dip.Properties.Length];
        dip.Properties.CopyTo(Properties, 0);

        Particles = new FinalParticle[dip.Particles.Length];
        for (int index = 0; index < dip.Particles.Length; index++)
        {
            var p = dip.Particles[index];
            Particles[index] = new FinalParticle(p);
        }
    }
}

public class DataIntermediatePlot
{
    public string Name { get; set; }
    public ParticleEnum[] Enums { get; set; }
    public Property[] Properties { get; set; }
    public JsonParticle[] Particles { get; set; }
}

public class Property
{
    public string Name { get; set; }
    public int Type { get; set; }
}

public class ParticleEnum
{
    public string[] values { get; set; }
}

public class JsonParticle
{
    public float[] Position { get; set; }
    public float[] Color { get; set; }
    public float Size { get; set; }
    public object[] Props { get; set; }
}

public class FinalParticle
{
    public Quaternion Position { get; set; }
    public Color Color { get; set; }
    public float Size { get; set; }
    public object[] Props { get; set; }

    public FinalParticle(JsonParticle raw)
    {
        Position = new Quaternion(raw.Position[0], raw.Position[1], raw.Position[2], raw.Position[3]);
        Position = Position.NormailzeQuaternion();
        Color = new Color(raw.Color[0], raw.Color[1], raw.Color[2]);
        Size = raw.Size;
        Props = new object[raw.Props.Length];
        raw.Props.CopyTo(Props, 0);
    }

}

public class LoadJson
{
    private LoadJson(){}

    private static LoadJson _instance;

    public static LoadJson Instance
    {
        get { return _instance ?? (_instance = new LoadJson()); }
    }

    public DataPlot Load(string path)
    {
        DataPlot dataPlot = new DataPlot();

        try
        {
            var inter = JsonConvert.DeserializeObject<DataIntermediatePlot>(path);
            dataPlot = new DataPlot(inter);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return dataPlot;
    }

    public DataPlot LoadFromFile(string path)
    {
        DataPlot dataPlot = new DataPlot();
        using (var reader = new StreamReader(path))
        {
            dataPlot = Load(reader.ReadToEnd());
        }

        return dataPlot;
    }
}
