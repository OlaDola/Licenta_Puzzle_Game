using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    private const string SAVE_EXTENSION = "txt";
    private const string IMAGE_EXTENSION = "png";

    private static readonly string SAVE_FOLDER = Application.dataPath + "/Levels/";
    private static bool isInit = false;

    public static string GetFolderPath()
    {
        return SAVE_FOLDER;
    }
    public static void Init()
    {
        if (!isInit)
        {
            isInit = true;
            //Test If save folder exists
            {
                if (!Directory.Exists(SAVE_FOLDER))
                {
                    Directory.CreateDirectory(SAVE_FOLDER);
                }
            }
        }
    }

    public static void SaveJson(string fileName, string saveString, bool overwrite)
    {
        Init();
        string saveFileName = fileName;
        if(!overwrite)
        {
            while( File.Exists(SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION))
            {
                saveFileName = fileName;
            }
            // Is unique
        }
        if (!Directory.Exists(SAVE_FOLDER+saveFileName))
        {
            Directory.CreateDirectory(SAVE_FOLDER+saveFileName);
        }
        File.WriteAllText(SAVE_FOLDER +saveFileName + "/" + saveFileName + "."+ SAVE_EXTENSION, saveString);
    }

    public static void SaveImage(string fileName, byte[] image, bool overwrite)
    {
        Init();
        string saveFileName = fileName;
        if (!overwrite)
        {
            while (File.Exists(SAVE_FOLDER + saveFileName + "." + IMAGE_EXTENSION))
            {
                saveFileName = fileName;
            }
            // Is unique
        }
        File.WriteAllBytes(SAVE_FOLDER + saveFileName + "/" + saveFileName + "." + IMAGE_EXTENSION, image);
    }

    public static string Load(string fileName)
    {
        Init();
        if(File.Exists(SAVE_FOLDER + fileName + "/" + fileName + "." + SAVE_EXTENSION))
        {
            string saveString = File.ReadAllText(SAVE_FOLDER + fileName + "/" + fileName + "." + SAVE_EXTENSION);
            return saveString;
        }
        else
        {
            return null;
        }
    }

    public static Sprite LoadImage(string filename)
    {
        Init();
        if(File.Exists(SAVE_FOLDER + filename + "/" + filename + "." + IMAGE_EXTENSION))
        {
            byte[] image = File.ReadAllBytes(SAVE_FOLDER + filename + "/" + filename + "." + IMAGE_EXTENSION);
            var tex = new Texture2D(1, 1);
            tex.LoadImage(image);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width/2, tex.height/2));
            return sprite;
        }
        else
        {
            return null;
        }
    }

    public static void Delete(string levelname)
    {
        Init();
        if(Directory.Exists(SAVE_FOLDER + levelname))
        {
            Directory.Delete(SAVE_FOLDER + levelname, true);
            File.Delete(SAVE_FOLDER + levelname + ".meta");
        }
    }

    public static string LoadMostRecentFile()
    {
        Init();
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        //get save files

        FileInfo[] saveFiles = directoryInfo.GetFiles("*." + SAVE_EXTENSION);
        FileInfo mostRecentFile = null;
        foreach(FileInfo saveFile in saveFiles)
        {
            if(mostRecentFile == null)
            {
                mostRecentFile = saveFile;
            }
            else
            {
                if(saveFile.LastWriteTime > mostRecentFile.LastWriteTime)
                {
                    mostRecentFile = saveFile;
                }
            }
        }

        if(mostRecentFile != null)
        {
            string saveString = File.ReadAllText(mostRecentFile.FullName);
            return saveString;
        }
        else
        {
            return null;
        }
    }
}
