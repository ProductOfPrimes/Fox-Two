using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GamePhase
{
    public const int GAME_OVER = -1;

    // Strategy cards placed
    // Plane order decided
    public const int STRATEGY = 0;

    // Choose Maneuvers for each plane
    public const int COMMAND = 1;

    // Maneuvers, movement, and attacking played out
    public const int MANEUVER = 2;

    // End phase before the cycle continues
    public const int END = 3;

    public const int NUM_PHASES = 4;
}

public class GamePhaseController : MonoBehaviour
{
    [SerializeField]
    bool randomStart = false;

    [SerializeField]
    Commander[] commanders;

    [SerializeField]
    GameBoard board;

    [SerializeField]
    public List<UnitAircraft> allUnits;

    /// <summary>
    /// serialized just for debug
    /// </summary>
    public CommandList allCommands;

    float maneuverInterval;

    public int phase = -1;
    private int lastPhase = -2;

    //[Header("Events to trigger at the start of each Phase")]
    public delegate void GamePhaseEvent();
    public event GamePhaseEvent onGameStart;
    public event GamePhaseEvent onStrategy;
    public event GamePhaseEvent onCommand;
    public event GamePhaseEvent onPrompt;
    public event GamePhaseEvent onManeuver;
    public event GamePhaseEvent onEnd;
    public event GamePhaseEvent onGameOver;

    [SerializeField]
    FXController fXController;

    static HexPosition[] startPositions = new HexPosition[4]
    {
        new HexPosition(0,0),
        new HexPosition(5,5),
        new HexPosition(0,1),
        new HexPosition(5,4)
    };

    static int[] startHeadings = new int[4]
  {
       Heading.NE,
       Heading.SW,
       Heading.NE,
       Heading.SW
  };

    private void Awake()
    {
        onGameStart += OnStartPhase;
        onStrategy += StrategyPhase;
        onCommand += CommandPhase;

        onManeuver += ManeuverPhase;
        onEnd += EndPhase;
        onGameOver += OnGameOver;
    }

    void Start()
    {
        if (allUnits == null)
        {
            allUnits = new List<UnitAircraft>(FindObjectsOfType<UnitAircraft>());
        }

        allCommands = new CommandList();

        // set units
        foreach (var unit in allUnits)
        {
            allCommands.Add(unit, null);

            // watch for unit death
            if (fXController != null)
            {
                unit.deathSubscribers += fXController.OnUnitDeath;
                unit.deathSubscribers += OnUnitDeath;
            }
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
           StartGame();
        }

        if (phase != lastPhase)
        {
            lastPhase = phase;
            TriggerPhaseEvents();
        } else
        {
            switch (phase)
            {

                case (GamePhase.COMMAND):
                    {
                        // determine if it is time to move on
                        if (IsAllPlanesCommanded())
                        {
                            AdvancePhase();
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }


    public void OnStartPhase()
    {

    }

    public void StartGame()
    {
        if(randomStart)
        {
            for (int i = 0; i < allUnits.Count; i++)
            {
                int heading = Random.Range(0, Heading.NUM_DIRECTIONS);
                HexPosition pos;

                do
                {
                    // find an empty spot
                    pos = board.RandomPosition();
                } while (pos.containsKey(GameBoard.KEY_UNIT_AIR));


                allUnits[i].Respawn(pos, heading);
                allUnits[i].TriggerAnimation();
            }
        } else
        {
            SpawnDefaultPositions();
        }

        onGameStart.Invoke();

        phase = GamePhase.STRATEGY;
        TriggerPhaseEvents();

    }

    public void StartGameFixed()
    {
        onGameStart.Invoke();

        phase = GamePhase.STRATEGY;
        TriggerPhaseEvents();

    }

    public void SpawnDefaultPositions()
    {
        for (int i = 0; i < allUnits.Count; i++)
        {
            allUnits[i].Respawn(startPositions[i], startHeadings[i]);
            allUnits[i].TriggerAnimation();
        }
    }


    public bool SubmitCommands(CommandList commands)
    {
        if (phase != GamePhase.COMMAND)
        {
            Debug.LogError("Commands sent on wrong phase!");
            return false;
        }
        else
        {
            // add commands
            foreach (var command in commands)
            {
                if (allCommands.ContainsKey(command.Key))
                {
                    allCommands[command.Key] = command.Value;
                }
                else
                {
                    allCommands.Add(command.Key, command.Value);
                }
            }

            return true;
        }
    }

    /// <summary>
    /// clear all commands from commanders
    /// </summary>
    private void FlushCommands()
    {
        allCommands.Clear();

        foreach (var unit in allUnits)
        {
            allCommands[unit] = null;
        }

        foreach (var commander in commanders)
        {
            commander.ResetCommands();
        }
    }

    bool IsAllPlanesCommanded()
    {
        foreach (var unit in allUnits)
        {
            // if no planned manuever for this plane...
            if ((allCommands[unit] == null) && unit.hitPoints > 0)
            {
                return false;
            }
        }
        return true;
    }

    private void TriggerPhaseEvents()
    {
        switch (phase)
        {
            case (GamePhase.STRATEGY):
                {
                    onStrategy.Invoke();
                    break;
                }
            case (GamePhase.COMMAND):
                {
                    onCommand.Invoke();

                    if (onPrompt != null)
                        onPrompt.Invoke();
                    break;
                }
            case (GamePhase.MANEUVER):
                {
                    onManeuver.Invoke();
                    break;
                }
            case (GamePhase.END):
                {
                    onEnd.Invoke();
                    break;
                }
            case (GamePhase.GAME_OVER):
                {
                    onGameOver.Invoke();
                    break;
                }
        }
    }

    void StrategyPhase()
    {
        SortPlanesBySpeed();

        AdvancePhase();
    }

    void CommandPhase()
    {
        FlushCommands();
        // Tell commanders to plan...
        foreach (var commander in commanders)
        {
            for (int i = 0; i < commander.unitsCommanded.Count; i++)
            {
                var unit = commander.unitsCommanded[i];
                // if for some reason a commander cannot make any moves on a plane...
                if (GetAvailableManeuvers(unit).Count == 0)
                {
                    //kill it!
                    unit.Die("No options left -- unit self-destructed!");
                    i--;
                }
            }
        }
        // if all planes have commands, then move on!
    }

    IEnumerator PerformManeuvers(float delayPerAction)
    {
        for (int i = 0; i < allUnits.Count; i++)
        {
            // do each plane's maneuver
            var unit = allUnits[i];
            var m = allCommands[unit];

            if (unit.hitPoints > 0)
            {
                Debug.Log("Executing: " + m.id.ToString() + " on " + unit.callsign);
                //m.Execute(ref unit);
                unit.PerformManeuver(m);
                unit.Move();
                unit.StallCheck();
                unit.CrashCheck();

                //ManeuverManager.CrashCheck(ref unit);
                //m.aircraft.Move();


                unit.TriggerAnimation();

                // if plane is in range of another plane... attack!
                bool attacked = false;
                for (int range = 1; range <= 2; range++)
                {
                    HexPosition attackPoint = unit.LookAhead(range);
                    if (attackPoint.containsKey(GameBoard.KEY_UNIT_AIR))
                    {
                        UnitAircraft target = (UnitAircraft)attackPoint.getValue(GameBoard.KEY_UNIT_AIR);
                        Debug.Log(unit.callsign + " fires on " + target.callsign);
                        attacked = true;
                        unit.Attack(target);
                        target.TriggerAnimation();
                    }
                }

                if (attacked)
                {
                    unit.AttackAnimation();
                }
            }
            yield return new WaitForSeconds(delayPerAction);
        }

        if (phase == GamePhase.MANEUVER)
        {
            AdvancePhase();
        }
    }

    void ManeuverPhase()
    {
        // TODO: Execute Maneuvers in turn
        StartCoroutine(PerformManeuvers(maneuverInterval));
    }

    void OnGameOver()
    {
        StopCoroutine("PerformManeuvers");
        board.ResetPositions();
        //FlushCommands();
    }

    void OnUnitDeath(UnitAircraft unit, string killer)
    {
    }

    void EndPhase()
    {

        bool gameOver = false;
        for(int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];

            if(unit.hitPoints < 1)
            {
                if (unit.team == Team.BLUFOR)
                {
                    //OPFOR wins
                    Debug.Log("OPFOR WINS");
                }
                else if (unit.team == Team.OPFOR)
                {
                    //BLUFOR wins
                    Debug.Log("BLUFOR WINS");
                }
                SetPhase(GamePhase.GAME_OVER);
                gameOver = true;
            }
        }
        if(!gameOver)
        {
            AdvancePhase();
        }
    }

    void SortPlanesBySpeed()
    {
        // Sort actions by plane speed!
        allUnits.Sort(
            delegate (UnitAircraft plane1, UnitAircraft plane2)
            {
                return plane2.speed - plane1.speed;
            }
            );

        // Randomly swap unit order if speeds are the same
        for (int i = 0; i < allUnits.Count - 1; i++)
        {
            if (allUnits[i].speed == allUnits[i + 1].speed)
            {
                // toss a coin
                if (Random.Range(0, 2) == 1)
                {
                    var temp = allUnits[i];
                    allUnits[i] = allUnits[i + 1];
                    allUnits[i + 1] = temp;
                }
            }
        }

    }

    public void AdvancePhase()
    {

        phase = (++phase) % GamePhase.NUM_PHASES;
        //Debug.Log("Entered game phase: " + phase);
    }

    public void SetPhase(int newPhase)
    {
        phase = newPhase;
        //Debug.Log("Set game phase: " + phase);
    }



    public bool CanPerformManeuver(UnitAircraft unit, ref Maneuver man)
    {
        UnitAircraft copy = unit.Clone();

        // try the maneuver ahead of time on a copy
        copy.PerformManeuver(man);
        copy.StallCheck();
        copy.CrashCheck();

        bool canDo = board.isTraversibleAir(copy.ProjectAhead());
        return canDo; // unit would move outside range!
    }

    public List<Maneuver> GetAvailableManeuvers(UnitAircraft unit)
    {
        List<Maneuver> availableManeuvers = new List<Maneuver>();

        if (CanPerformManeuver(unit, ref Maneuver.Straight))
        {
            availableManeuvers.Add(Maneuver.Straight);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Accelerate))
        {
            availableManeuvers.Add(Maneuver.Accelerate);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Climb))
        {
            availableManeuvers.Add(Maneuver.Climb);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Dive))
        {
            availableManeuvers.Add(Maneuver.Dive);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Left60))
        {
            availableManeuvers.Add(Maneuver.Left60);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Right60))
        {
            availableManeuvers.Add(Maneuver.Right60);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Left120))
        {
            availableManeuvers.Add(Maneuver.Left120);
        }

        if (CanPerformManeuver(unit, ref Maneuver.Right120))
        {
            availableManeuvers.Add(Maneuver.Right120);
        }

        if (CanPerformManeuver(unit, ref Maneuver.UTurn))
        {
            availableManeuvers.Add(Maneuver.UTurn);
        }

        return availableManeuvers;
    }

    public void OnRestartGameButton_Clicked()
    {
      StartGame();
    }

}
