using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotHandler : MonoBehaviour
{
    private static ScreenShotHandler instance;

    [SerializeField] Camera PreviewCamera;

    private bool takeScreenShot;
    private static string LevelName;

    private void Awake()
    {
        instance = this;
        PreviewCamera = gameObject.GetComponent<Camera>();
    }

    private void OnPostRender()
    {
        if (takeScreenShot)
        {
            takeScreenShot = false;

            RenderTexture rt = PreviewCamera.targetTexture;

            Texture2D renderResult = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, rt.width, rt.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] data = renderResult.EncodeToPNG();
            SaveSystem.SaveImage(LevelName, data, true);
            print("Saved:"+LevelName);

            RenderTexture.ReleaseTemporary(rt);
            PreviewCamera.targetTexture = null;
        }
    }

    private void ScreenShotTake()
    {
        PreviewCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
        takeScreenShot = true;
    }

    public static void ScreenShottake_Static(string levelName)
    {
        LevelName=levelName;
        instance.ScreenShotTake();
    }

    public byte[] ScreenShot()
    {
        PreviewCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

        RenderTexture rt = PreviewCamera.targetTexture;

        Texture2D renderResult = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        Rect rect = new Rect(0, 0, rt.width, rt.height);
        renderResult.ReadPixels(rect, 0, 0);

        byte[] byteArray = renderResult.EncodeToPNG();

        return byteArray;
    }

}
