using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A POD datatype to store only the necessary data from a UnitAircraft to build a functioning reinforcement agent.
public struct AircraftState
{
    public AircraftState(UnitAircraft aircraft)
    {
        u = aircraft.hexPos.U;
        v = aircraft.hexPos.V;
        speed = aircraft.speed;
        heading = aircraft.heading;
        altitude = aircraft.altitude;
        hp = aircraft.hitPoints;
    }

    public AircraftState(int a_u, int a_v, int a_speed, int a_heading, int a_altitude, int a_hp)
    {
        u = a_u;
        v = a_v;
        speed = a_speed;
        heading = a_heading;
        altitude = a_altitude;
        hp = a_hp;
    }

    public int u, v, speed, heading, altitude, hp;
}

[System.Serializable]
class StateSpace
{
    public const int SPEED_MAX = 7;
    public const int HITPOINTS_MAX = 1;
    public const int BOARD_SIZE_U = 6;
    public const int BOARD_SIZE_V = 6;
    public const int DIMENSIONS_PER_AIRCRAFT = 6;
    public const int FIXED_PLANE_STATE_SPACE_SIZE = 0;
    //All states are multiplied to compute state size despite properties being independent because changing one property produces a completely unique state. 
    public const int PLANE_STATE_SPACE_SIZE = FIXED_PLANE_STATE_SPACE_SIZE + BOARD_SIZE_U * BOARD_SIZE_V * (SPEED_MAX + 1) * Heading.NUM_DIRECTIONS * (UnitAircraft.ALTITUDE_MAX + 1) * (HITPOINTS_MAX + 1);

    /// <summary>
    /// Stores mapping of aircraft stats to an index within the state space, for fast access to state index by state
    /// </summary>
    private static Dictionary<AircraftState, int> aircraftToStateSpaceIndex;

    /// <summary>
    /// Stores all possible states for a plane, for fast access of states by index
    /// </summary>
    public List<AircraftState> states;

    public string ToString(AircraftState state)
    {
        return "Position: " + state.u + "," + state.v + "\t  Speed: " + state.speed + "\t  Heading: " + state.heading + "\t  Altitude: " + state.altitude + "\t  HP:" + state.hp;
    }

    public int GetStateIndex(UnitAircraft aircraft)
    { 
        AircraftState state = new AircraftState(aircraft);

        int idx;

        if(aircraftToStateSpaceIndex.TryGetValue(state, out idx))
        {
            return idx;
        } else
        {
            throw new System.Exception("StateSpace couldn't find: " + ToString(state));
        }
    }

    public UnitAircraft GetAircraftAtState(int index)
    {
        UnitAircraft unit = new UnitAircraft();
        AircraftState state = states[index];
        unit.hexPos.U = state.u;
        unit.hexPos.V = state.v;
        unit.speed =    state.speed;
        unit.heading =  state.heading;
        unit.altitude = state.altitude;
        unit.hitPoints =state.hp;

        //int[] state = states[index];
        //aircraft.hexPos.U, aircraft.hexPos.V, aircraft.speed, aircraft.heading, aircraft.altitude, aircraft.hitPoints 
        //unit.hexPos.U =     state[0];
        //unit.hexPos.V =     state[1];
        //unit.speed =        state[2];
        //unit.heading =      state[3];
        //unit.altitude =     state[4];
        //unit.hitPoints =    state[5];

        return unit;
    }

    //Forms the statespace by appending every possible state to the states list.
   public StateSpace(UnitAircraft unit, int boardSize_u, int boardSize_v)
    {
        states = new List<AircraftState>();
        aircraftToStateSpaceIndex = new Dictionary<AircraftState, int>();

        for (int u = 0; u < boardSize_u; u++)
        {
            for (int v = 0; v < boardSize_v; v++)
            {
                for (int speed = 0; speed <= unit.speedMax; speed++)
                {
                    for (int heading = 0; heading < Heading.NUM_DIRECTIONS; heading++)
                    {
                        for (int altitude = 0; altitude <= UnitAircraft.ALTITUDE_MAX; altitude++)
                        {
                            for (int hitPoints = 0; hitPoints <= unit.hitPointsMax; hitPoints++)
                            {
                                // skip states that should not happen
                                if(altitude == 0 && hitPoints > 0)
                                {
                                    continue; // if altitude is 0 you should be dead my dude
                                }
                                if (speed == 0 && hitPoints > 0)
                                {
                                    continue; // if speed is 0 you should be dead my dude
                                }

                                AircraftState aircraftState = new AircraftState(
                                    u,
                                    v,
                                    speed,
                                    heading,
                                    altitude,
                                    hitPoints
                                    ); 

                                // add the index of this plane state to the mapping so we can look it up
                                aircraftToStateSpaceIndex.Add(aircraftState, states.Count);

                                // store this plane state
                                states.Add(aircraftState);
                            }
                        }
                    }
                }
            }
        }
    }

    //Order: u,v,speed,heading,altitude,HP
    public static int[] AircraftToArray(UnitAircraft aircraft)
    {
        return new int[] { aircraft.hexPos.U, aircraft.hexPos.V, aircraft.speed, aircraft.heading, aircraft.altitude, aircraft.hitPoints };
    }

}





//public List<GameState> EnumerateBoardStateSpace(UnitAircraft[] allAircraft)
//{
//    List<GameState> stateSpace = new List<GameState>();
//    List<int[]> aircraftStateSpace = EnumerateAircraftStateSpace();
//
//
//    //int numStates = Mathf.Pow(allAircraft.Length, aircraftStateSpace.Count);
//    //Math.Pow(2, list.Count)
//
//    for (int stateIndex = 0; stateIndex < numStates; stateIndex++)
//    {
//        // try every combination of state for each aircraft
//        GameState currentState = new GameState();
//
//        //currentState[0] = stateSpace[index1]
//        //currentState[1] = stateSpace[index2]
//        //currentState[2] = stateSpace[index3]
//
//        for (int aircraftState = 0; aircraftState < aircraftStateSpace.Count; aircraftState++)
//        {
//            // assuming 2 planes for now...
//            for (int planeIDX_1 = 0; planeIDX_1 < allAircraft.Length; planeIDX_1++)
//            {
//                currentState.Add(allAircraft[planeIDX_1].callsign, aircraftStateSpace[aircraftState]);
//            }
//        }
//
//        for (int aircraftState = 0; aircraftState < aircraftStateSpace.Count; aircraftState++)
//        {
//            // assuming 2 planes for now...
//            for (int planeIDX_1 = 0; planeIDX_1 < allAircraft.Length; planeIDX_1++)
//            {
//                currentState.Add(allAircraft[planeIDX_1].callsign, aircraftStateSpace[aircraftState]);
//            }
//        }
//
//        stateSpace.Add(currentState);
//    }
//
//
//
//    return stateSpace;
//}