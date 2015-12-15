using System;
using UnityEngine;
using System.Collections;

public class SelectFile : MonoBehaviour
{
    #region Singleton
    private static SelectFile _instance;

    public static SelectFile Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (null == _instance)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);

        Init();
    }
    #endregion

    public bool _menuIsShowing = false;

    [SerializeField]
    private Texture2D file, folder, back, drive;
    [SerializeField]
    private GUISkin skin;

    private FileBrowser fb = new FileBrowser();

    public event Action<string> FileSelected;

    public void Init()
    {
        fb.guiSkin = skin;
        fb.fileTexture = file;
        fb.directoryTexture = folder;
        fb.backTexture = back;
        fb.driveTexture = drive;
        fb.showSearch = true;
        fb.searchRecursively = true;
    }

    void OnGUI()
    {
        if (!_menuIsShowing) return;

        if (fb.draw())
        {
            if (fb.outputFile != null)
            {
                OnFileSelected(fb.outputFile.ToString());
            }

            _menuIsShowing = false;
        }
    }

    public void OnFileSelected(string path)
    {
        Debug.Log(path + " was selected!");
        if (FileSelected != null)
            FileSelected(path);
    }

    public void Select()
    {
        _menuIsShowing = true;
    }
}
