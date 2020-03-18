using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Threading;


public class QLearning : MonoBehaviour
{
    [SerializeField]
    GameObject successParticles;

    //"Players". Each commander is capable of controlling multiple jets, however jets are restricted to 1 for simplicity of this prototype.
    [SerializeField]
    Commander[] commandersToControl;
    [SerializeField]
    GamePhaseController gameController;

    [Header("Q-learning properties")]
    /// <summary>
    /// All potential states
    /// </summary>
    [SerializeField]
    StateSpace stateSpace;

    [SerializeField]
    GameBoard board;

    //An array of lists where the array indices denote a state and the lists contain all possible scored actions for each state.
    //Currently this datastructure functions as List<Dictionary<Maneuver, float>>[] due to single jet restrictions.
    [SerializeField]
    List<Tuple<CommandListAbstract, float>>[] policy;

    /// <summary>
    /// Function pointer for how reward should being calculated
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public delegate float RewardFunction(int state);

    [SerializeField]
    RewardFunction rewardFunc;

    [SerializeField]
    float lastReward;

    [SerializeField]
    Vector2Int targetPosition = new Vector2Int(2, 2);
    [SerializeField]
    int targetHeading = Heading.N;

    /// <summary>
    /// Game state index inside state space. also used to index rewards, policy, and state transitions
    /// </summary>
    [SerializeField]
    int gameState;

    /// <summary>
    /// Factor for trusting new information versus trusting old (alpha)
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    float learningRate = 0.2f;

    /// <summary>
    /// Randomness parameter for weighting choices (rho)
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    float randomnessFactor = 0.1f;

    /// <summary>
    /// Discount factor for future states (gamma)
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    float discountFactor = 0.01f;

    /// <summary>
    /// How smart will the AI choose its moves? 1 = best moves. 0 = worst possible moves every time
    /// </summary>
    [SerializeField]
    [Range(0, 1)]
    float difficultyLevel = 1.0f;

    [SerializeField]
    int lastActionIndex = -1;
    [SerializeField]
    int lastState = -1;
    [SerializeField]
    int roundCountThisEpisode = 0;
    //private struct EpisodeHistory
    //{
    //    public List<int> actionSequenceThisEpisode; // documents which index in the policy was used... which means the indices are kind of worthless once the policy changes. Only the most recent one is actually usable.
    //    public List<int> statesThisEpisode; // history of states. could be useful if we want to make a transition probability table.
    //}
    /// <summary>
    /// keep track of state-action pairs tried this episode
    /// </summary>
    //EpisodeHistory currentEpisode;

    [SerializeField]
    bool isTraining = false;

    [SerializeField]
    bool sendCommands = true;

    /// <summary>
    /// How many episodes to train
    /// </summary>
    [SerializeField]
    int numEpisodesToRun = 5;

    /// <summary>
    /// How many episodes have been run since starting
    /// </summary>
    [SerializeField]
    private int episodeCount = 0;

    /// <summary>
    /// Limit to the number of rounds of the game to play within each episode. After this limit we will update the Policy
    /// </summary>
    [SerializeField]
    int maximumRoundsPerEpisode = 10;

    private Thread thread1;

    // Start is called before the first frame update
    void Start()
    {
        rewardFunc = TestRewardFunction;
        InitStateSpace();
        gameController.onGameStart += OnGameStart;
        gameController.onPrompt += OnCommandPrompt;
        gameController.onEnd += OnEndPhase;
        gameController.onGameOver += OnGameEnd;
    }

    //The function of this script is to train the AI.
    private void Update()
    {
        if (policy == null)
        {
            thread1 = new Thread(new ThreadStart(InitializePolicy));
            thread1.Start();
        }

        if (policy != null)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (!isTraining)
                {
                    StartTrainingRun();
                }
                else
                {
                    EndTrainingRun();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) // play
        {
            sendCommands = !sendCommands;
            OnCommandPrompt();
        }
    }

    void StartTrainingRun()
    {
        episodeCount = 0;
        isTraining = true;
        BeginEpisode();
    }

    void BeginEpisode()
    {
        gameController.StartGame();
        roundCountThisEpisode = 0;
    }

    void EndEpisode()
    {
        Debug.Log("End of Episode " + episodeCount);

        episodeCount++;

        if (episodeCount < numEpisodesToRun)
        {
            BeginEpisode();
        }
        else
        {
            EndTrainingRun();
        }
    }

    //Publish the training results!
    void EndTrainingRun()
    {
        Debug.Log("Finished training run");
        isTraining = false;
        var path = "Assets/Resources/policy.csv";
        PolicyIO.Serialize(policy, path);
        //List<Tuple<CommandListAbstract, float>>[] testPolicy = null;
        //PolicyIO.Deserialize(testPolicy, path);
    }

    //Similar to the policy datastructure except this stores all state information such as position and speed rather than scores for maneuvers in each state.
    void InitStateSpace()
    {
        stateSpace = new StateSpace(commandersToControl[0].unitsCommanded[0], board.size.x, board.size.y);
    }

    /// <summary>
    /// Must enumerate every possible action to take from every possible state, where 1 action is a set of commands given to units. FOR NOW IT ONLY COMMANDS ONE UNIT!!!!
    /// </summary>
    /// <param name="c"></param>
    void InitializePolicy()
    {
        Commander c = commandersToControl[0];

        Debug.Log("Creating policy...");

        float initialQValue = 0.8f;
        policy = new List<Tuple<CommandListAbstract, float>>[stateSpace.states.Count];

        //for (int s = 0; s < stateSpace.Length; s++)
        for (int s = 0; s < stateSpace.states.Count; s++)
        {
            List<Tuple<CommandListAbstract, float>> commandLists = new List<Tuple<CommandListAbstract, float>>();

            // available options for each plane
            List<Maneuver>[] availableManeuvers = new List<Maneuver>[c.unitsOwned.Count];

            // enumerate possible maneuvers for each plane controlled
            for (int unitIdx = 0; unitIdx < c.unitsOwned.Count; unitIdx++)
            {
                var aircraft = stateSpace.GetAircraftAtState(s);
                var potentialManeuvers = gameController.GetAvailableManeuvers(aircraft);
                availableManeuvers[unitIdx] = potentialManeuvers;
            }

            // get all possible combinations of maneuvers for ONE plane
            for (int unitIdx = 0; unitIdx < c.unitsOwned.Count; unitIdx++)
            {
                if (c.unitsOwned.Count > 1)
                {
                    Debug.LogError("Whoops, did not implement Policy creation for commanding multiple units yet...");
                }
                var maneuvers = availableManeuvers[unitIdx];
                for (int maneuverIdx = 0; maneuverIdx < maneuvers.Count; maneuverIdx++)
                {
                    CommandListAbstract action = new CommandListAbstract();
                    action.Add(unitIdx, maneuvers[maneuverIdx].id);
                    commandLists.Add(new Tuple<CommandListAbstract, float>(action, initialQValue));

                }
            }
            policy[s] = commandLists;

            //Debug.Log("Policy creation progress: " + s + "/" + stateSpace.states.Count);
        }

        Debug.Log("Policy creation finished!");
    }

    public void OnGameStart()
    {
        // maybe randomize starting state?
    }

    public void OnCommandPrompt() // should happen when it is time to make a decision
    {
        if (sendCommands)
        {
            UnitAircraft myplane = commandersToControl[0].unitsCommanded[0];
            gameState = stateSpace.GetStateIndex(myplane);

            // if exceeded time limit... then just end it
            if (isTraining && roundCountThisEpisode > maximumRoundsPerEpisode)
            {
                EndEpisode();
            }
            else
            {
                // for each agent...
                // Find state
                // Choose action
                // Queue action
                // Tell Game to submit actions
                // Use resulting state-action choice

                for (int i = 0; i < commandersToControl.Length; i++)
                {
                    SelectMove(ref commandersToControl[i], randomnessFactor, difficultyLevel);
                }
            }
        }
    }

    //Update the policy if it was improved.
    public void OnEndPhase()
    {
        // update state...
        UnitAircraft myplane = commandersToControl[0].unitsOwned[0];
        gameState = stateSpace.GetStateIndex(myplane);


        if (rewardFunc.Invoke(gameState) == 1.0f)
        {
            Destroy(Instantiate(successParticles, myplane.hexPos.getPosition(), Quaternion.identity), 0.5f);
            Debug.Log("Reached goal state!");

            if (!isTraining)
            {
                sendCommands = false;
            }
        }



        if (isTraining)
        {
            // improve policy every iteration
            UpdateValueEstimatesInPolicy(lastState, lastActionIndex, gameState, discountFactor, learningRate);
            ImprovePolicyAt(lastState);
        }
    }

    public void OnGameEnd()
    {
        if (isTraining)
        {
            EndEpisode();
        }
    }


    // from the textbook: AI For Games by Ian Millington
    //start state, the action taken, the reinforcement value, and the resultingstate—arecalledtheexperiencetuple,oftenwrittenas< s,a,r,s′ >.
    //Q(s,a) = (1−α)Q(s,a) + α(r + γmax(Q(s′,a′)))
    //s, a are given by current state
    //s', a' are given by the following state
    void UpdateValueEstimatesInPolicy(int lastState, int lastActionIndex, int resultingState, float discountFactor, float learningRate) // to be called after the result of a choice is known
    {
        //Debug.Log("state " + lastActionIndex + "& action " + lastActionIndex + " led to state " + resultingState);


        var pastActionQPair = policy[lastState][lastActionIndex];
        float oldQ = pastActionQPair.Item2;

        // get reward of the initial state
        float reward = rewardFunc.Invoke(resultingState);

        lastReward = reward; // just for visibility

        //reward For Best Action In New State
        float maxQ = policy[resultingState][0].Item2;

        // episodic learning
        //v = reward + (discountFactor * rewardForBestActionInNextState)
        float newValueEstimate = reward + (discountFactor * maxQ);

        float newQValue = Mathf.Lerp(oldQ, newValueEstimate, learningRate);

        var newActionQPair = new Tuple<CommandListAbstract, float>(pastActionQPair.Item1, newQValue);

        policy[lastState][lastActionIndex] = newActionQPair;

        // Debug.Log("state " + lastActionIndex + "& action " + lastActionIndex + "(value: " + newQValue + ") led to state " + resultingState);
        //Debug.Log(newActionQPair.Item1[0].ToString() + " at state " + lastState + "rated: " + newQValue);
    }

    /// <summary>
    /// Sort actions possible in policy at that state by Value estimate, highest to lowest
    /// </summary>
    void ImprovePolicyAt(int state)
    {
        policy[state].Sort(
            delegate (Tuple<CommandListAbstract, float> a1, Tuple<CommandListAbstract, float> a2)
            {
                if (a1.Item2 > a2.Item2) return -1;
                else if (a1.Item2 < a2.Item2) return 1;
                else return 0;
            }
            );
    }

    /// <summary>
    /// Static reward for a given state
    /// </summary>
    /// <param name="stateIndex"></param>
    /// <returns></returns>
    float TestRewardFunction(int state)
    {
        // reward for going to position 2,2
        float maxReward = 1.0f;
        var aircraft = stateSpace.GetAircraftAtState(state);
        HexPosition target = new HexPosition(targetPosition.x, targetPosition.y);
        int distanceToTarget = target.dist(aircraft.hexPos);

        if (aircraft.hitPoints == 0)
        {
            return -1;
        }

        if (distanceToTarget == 0 && aircraft.heading == targetHeading)
        {
            return maxReward;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Chooses moves for commander
    /// </summary>
    /// <param name="commander"></param>
    /// <param name="randomness"></param>
    /// <param name="difficultyLevel"></param> difficulty from 0 to 1, 1 meaning always choose the best move. 0 meaning always choose the worst move
    void SelectMove(ref Commander commander, float randomness, float difficultyLevel)
    {
        if (policy == null)
        {
            Debug.Log("No Policy!");
            return;
        }

        if (policy[gameState].Count == 0)
        {
            Debug.Log("Policy does not have any actions for state: " + gameState);
            return;
        }

        List<Tuple<CommandListAbstract, float>> potentialActions = policy[gameState];
        int chosenActionIndex;

        if (policy[gameState].Count == 0)
        {
            Debug.Log("Policy does not have any actions for state: " + gameState);
        }
        float randomFloat = UnityEngine.Random.Range(0.0f, 1.0f);
        if (randomFloat <= randomness)
        {
            // choose a random available action
            Debug.Log("random action");
            chosenActionIndex = UnityEngine.Random.Range(0, potentialActions.Count);
        }
        else
        {
            //May choose second, third, fourth best action etc. based on "AI difficulty factor"
            chosenActionIndex = Mathf.FloorToInt(potentialActions.Count * (1.0f - difficultyLevel));
        }

        CommandListAbstract actionToTake = potentialActions[chosenActionIndex].Item1;
        commander.commands = CommandListAbstractToConcrete(actionToTake, commander);


        Debug.Log("Commanded " + commander.unitsCommanded[0].callsign + "To do Maneuver: " + actionToTake[0].ToString());

        lastActionIndex = chosenActionIndex;
        lastState = gameState;

        if (isTraining)
        {
            roundCountThisEpisode++;
        }

        commander.SubmitCommands();

    }

    /////////////////////////////
    /////////////////////////////
    /////////////////////////////
    ///Abstract Command Lists
    /////////////////////////////
    /////////////////////////////
    /////////////////////////////

    /// <summary>
    /// This is a typedef for Dictionary<int, Maneuver> where the key is the index of the aircraft in the commanded aircraft list belonging to the commander
    /// </summary>
    public class CommandListAbstract : Dictionary<int, Maneuver.ID>
    {
    }
    // A commander with 2 planes with an abstract command list just says "First plane does this, second plane does that" without any regard for what the plane actually is.
    // The Q-learning algorithm learns in this way with the understanding that what each of those "first" and second "plane" (s) are is stored as the game state


    /// <summary>
    /// Convert command list with specific Aircraft into an abstract command list that just references them by this commander's list of planes
    /// </summary>
    /// <param name="commander"></param>
    /// <param name="commands"></param>
    /// <returns></returns>
    CommandListAbstract CommandListConcreteToAbstract(CommandList commands, Commander commander)
    {
        CommandListAbstract abstractCommands = new CommandListAbstract();
        for (int i = 0; i < commander.unitsCommanded.Count; i++)
        {
            abstractCommands[i] = commands[commander.unitsCommanded[i]].id;
        }
        return abstractCommands;
    }

    /// <summary>
    /// Convert abstract command list that just references them by this commander's list of planes into a command list with specific Aircraft
    /// </summary>
    /// <param name="commander"></param>
    /// <param name="commands"></param>
    /// <returns></returns>
    CommandList CommandListAbstractToConcrete(CommandListAbstract commands, Commander commander)
    {
        CommandList concreteCommands = new CommandList();
        for (int i = 0; i < commands.Count; i++)
        {
            Maneuver maneuver = Maneuver.BasicManeuvers[(short)commands[i]];
            concreteCommands.Add(commander.unitsCommanded[i], maneuver);
        }
        return concreteCommands;
    }

    public void OnTrainingButton_Click()
    {
      if (!isTraining)
      {
          StartTrainingRun();
      }
      else
      {
          EndTrainingRun();
      }
    }

    public void OnPlayButton_Click()
    {
      sendCommands = !sendCommands;
      OnCommandPrompt();
    }


}
