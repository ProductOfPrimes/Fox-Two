using UnityEngine;
using System.Collections.Generic;
public enum Team
{
    BLUFOR,
    OPFOR,
    NEUTRAL,
    UNTARGETABLE
}


/// <summary>
/// This is a typedef for Dictionary<UnitAircraft, Maneuver>
/// </summary>
public class CommandList : Dictionary<UnitAircraft, Maneuver>
{
}

/// <summary>
/// Player script
/// Issues commands to ManeuverManager and Units
/// </summary>
public class Commander : MonoBehaviour
{
    [SerializeField]
    public Team team = Team.NEUTRAL;

    [SerializeField]
    GamePhaseController gameController;

    [SerializeField]
    public List<UnitAircraft> unitsOwned = new List<UnitAircraft>();
    public List<UnitAircraft> unitsCommanded = new List<UnitAircraft>();

    [SerializeField]
    public CommandList commands = new CommandList();//Dictionary<UnitAircraft, Maneuver>();

    [SerializeField]
    public UnitAircraft selectedUnit;

    [SerializeField]
    MouseSelection cursor;

    // has this commander received a request to deliver commands?
    [SerializeField]
    bool promptedForCommands = false;

    // Start is called before the first frame update-
    void Start()
    {
        gameController.onGameStart += OnGameStart;
        gameController.onCommand += PromptForCommands;
    }

    void OnGameStart()
    {

        unitsCommanded.Clear();
        foreach (var unit in unitsOwned)
        {
            unitsCommanded.Add(unit);
        }

        // set units
        foreach (var unit in unitsCommanded)
        {
            if(!commands.ContainsKey(unit))
            {
                commands.Add(unit, null);
                unit.team = team;
                unit.deathSubscribers += OnUnitDeath;
            }
        }

        ResetCommands();
    }

    void OnUnitDeath(UnitAircraft unit, string killer)
    {
        unitsCommanded.Remove(unit);
    }

    // Update is called once per frame
    void Update()
    {
        if (promptedForCommands)
        {
            if (cursor != null)
            {
                // TODO
                // unit selection logic should go here. meh
            }

            if (selectedUnit != null)
            {

                Maneuver myManeuver = null;

                if (Input.GetKeyDown(KeyCode.Keypad5))
                {
                    myManeuver = Maneuver.Straight;
                }

                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    myManeuver = Maneuver.Accelerate;
                }

                if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    myManeuver = Maneuver.Climb;
                }

                if (Input.GetKeyDown(KeyCode.Keypad8))
                {
                    myManeuver = Maneuver.Dive;
                }

                if (Input.GetKeyDown(KeyCode.Keypad4))
                {
                    myManeuver = Maneuver.Left60;
                }

                if (Input.GetKeyDown(KeyCode.Keypad6))
                {
                    myManeuver = Maneuver.Right60;
                }

                if (Input.GetKeyDown(KeyCode.Keypad7))
                {
                    myManeuver = Maneuver.Left120;
                }

                if (Input.GetKeyDown(KeyCode.Keypad9))
                {
                    myManeuver = Maneuver.Right120;
                }

                if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad3))
                {
                    myManeuver = Maneuver.UTurn;
                }

                if (myManeuver != null)
                {
                    if (gameController.CanPerformManeuver(selectedUnit, ref myManeuver))
                    {
                        commands[selectedUnit] = myManeuver;
                        SubmitCommands();
                    } else
                    {
                        Debug.Log("Unable to perform requested maneuver.");
                    }
                }
            }

            //if (Input.GetKeyDown(KeyCode.KeypadEnter))
            //{
            //    SubmitCommands();
            //    Debug.Log("Commands submitted!");
            //}
        }
    }

    // public void QueueRandomCommands()
    // {
    //     for(int i = 0; i < unitsCommanded.Count; i++)
    //     {
    //         commands[unitsCommanded[i]] = ManeuverManager.RandomManeuver();
    //     }
    // }

    public void PromptForCommands()
    {
        promptedForCommands = true;
    }

    public bool IsPromptedForCommands()
    {
        return promptedForCommands;
    }

    public void ResetCommands()
    {
        foreach (var unit in unitsCommanded)
        {
            commands[unit] = null;
        }
    }

    public void SubmitCommands()
    {
        if (gameController.SubmitCommands(commands))
        {
            promptedForCommands = false;
        }
    }
}
