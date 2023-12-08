using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class Save
{
    public static void SaveSettings(float masterVol, float musicVol, float voiceVol, float sfxVol, float mouseSense)
    {
        string destination = Application.persistentDataPath + "/settings.dat";
        FileStream file;

        if (File.Exists(destination))
            file = File.OpenWrite(destination);
        else
            file = File.Create(destination);

        SettingsData data = new SettingsData(masterVol, musicVol, voiceVol, sfxVol, mouseSense);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public static SettingsData LoadFile()
    {
        string destination = Application.persistentDataPath + "/settings.dat";
        FileStream file;
        SettingsData data;
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.LogError("Save file does not exist yet, creating new file.");
            file = File.Create(destination);
            data = new SettingsData(80f, 80f, 80f, 80f, 15f);
            bf.Serialize(file, data);
        }

        data = (SettingsData)bf.Deserialize(file);
        file.Close();

        if (data != null)
            return data;

        return null;
    }
}


[System.Serializable]
public class SettingsData
{
    public float masterVol;
    public float musicVol;
    public float voiceVol;
    public float sfxVol;
    public float mouseSense;

    public SettingsData(float MasterVol, float MusicVol, float VoiceVol, float SfxVol, float MouseSense)
    {
        masterVol = MasterVol;
        musicVol = MusicVol;
        voiceVol = VoiceVol;
        sfxVol = SfxVol;
        mouseSense = MouseSense;
    }
}