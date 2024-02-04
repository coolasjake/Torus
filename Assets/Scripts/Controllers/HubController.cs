using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HubController : MonoBehaviour
{
    public string missionScene = "Mission";
    public string missionDataFolder = "/Data/Missions";
    public static List<MissionData> possibleMissions = new List<MissionData>();
    public List<MissionData> missionOptions = new List<MissionData>();

    public List<HubCharacter> characters = new List<HubCharacter>();

    public List<Button> missionButtons = new List<Button>();


    // Start is called before the first frame update
    void Start()
    {
        SetupMissions();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupMissions()
    {
        possibleMissions.AddRange(Resources.LoadAll<MissionData>(missionDataFolder));
    }

    public void SelectMission(int index)
    {
        index = Mathf.Clamp(index, 0, possibleMissions.Count - 1);
        BattleController.missionData = possibleMissions[index];
        SceneManager.LoadScene(missionScene);
    }
}

public enum Faction
{
    None,

    Terran,
    Forescouts,
    Astrogenesis,
    Maisonner,

    Ahnlia,
    Ryshvi,
    Zodan,
}