
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManeuverManager : MonoBehaviour
{
    //public static class BasicManeuverID
    //{
    //    public const int STRAIGHT = 0;
    //    public const int ACCELERATE = 1;
    //    public const int CLIMB = 2;
    //    public const int DIVE = 3;
    //    public const int LEFT60 = 4;
    //    public const int LEFT120 = 5;
    //    public const int RIGHT60 = 6;
    //    public const int RIGHT120 = 7;
    //    public const int U_TURN = 8;
    //    public const int NUM_MANEUVERS = 9;
    //}
    //
    //[SerializeField]
    //Queue<Maneuver> maneuverQueue = new Queue<Maneuver>();
    //
    //[SerializeField]
    //Maneuver[] basicManeuvers;
    //
    //[SerializeField]
    //UnitAircraft testCraft;

    //public void Queue(Maneuver m)
    //{
    //    if (m.aircraft = null) Debug.LogError("Missing unit in Maneuver!");
    //    maneuverQueue.Enqueue(m);
    //}
    //
    //private void QueueBasicManeuver(int idx, ref UnitAircraft craft)
    //{
    //    Maneuver man = Instantiate(basicManeuvers[idx]);
    //    man.SetUnit(ref craft);
    //    maneuverQueue.Enqueue(man);
    //    Debug.Log("Queued: " + man.GetType().Name);
    //}
    //
    //public void ExecuteOneManeuver()
    //{
    //    Maneuver m = maneuverQueue.Dequeue();
    //    Debug.Log("Executing: " + m.GetType().FullName);
    //    m.Execute();
    //    StallCheck(ref m.aircraft);
    //    CrashCheck(ref m.aircraft);
    //    m.aircraft.Move();
    //}

    //public static void ExecuteManeuver(ref Maneuver m)
    //{
    //    Debug.Log("Executing: " + m.GetType().FullName + "on " + m.aircraft.callsign);
    //    m.Execute();
    //    StallCheck(ref m.aircraft);
    //    CrashCheck(ref m.aircraft);
    //    m.aircraft.Move();
    //}

    //public void ManeuverPhase()
    //{
    //    while (maneuverQueue.Count != 0)
    //        ExecuteOneManeuver();
    //
    //    MoveAllCraft();
    //}

    //public void MoveAllCraft()
    //{
    //    MoveAircraft(ref testCraft);
    //    testCraft.TriggerAnimation();
    //}

    //public static bool StallCheck(ref UnitAircraft craft)
    //{
    //    return (craft.speed < 1);
    //}
    //public static void Stall(ref UnitAircraft craft)
    //{
    //    craft.altitude -= 2;
    //    craft.speed = 1;
    //}
    //public static bool CrashCheck(ref UnitAircraft craft)
    //{
    //    return(craft.altitude < 1);
    //}
    //public static void Crash(ref UnitAircraft craft)
    //{
    //    craft.TakeDamage(craft.hitPoints, "Crash");
    //}

  

   

    //public static void MoveAircraft(ref UnitAircraft craft)
    //{
    //    // move according to heading and altitude
    //    int tilesToMove = SpeedToMovement(craft.speed);
    //
    //    HexPosition newPosition;
    //    switch (craft.heading)
    //    {
    //        case (Heading.N):
    //            {
    //                newPosition = craft.hexPos.goN(tilesToMove);
    //                break;
    //            }
    //        case (Heading.NE):
    //            {
    //                newPosition = craft.hexPos.goNE(tilesToMove);
    //                break;
    //            }
    //        case (Heading.SE):
    //            {
    //                newPosition = craft.hexPos.goSE(tilesToMove);
    //                break;
    //            }
    //        case (Heading.S):
    //            {
    //                newPosition = craft.hexPos.goS(tilesToMove);
    //                break;
    //            }
    //        case (Heading.SW):
    //            {
    //                newPosition = craft.hexPos.goSW(tilesToMove);
    //                break;
    //            }
    //        case (Heading.NW):
    //            {
    //                newPosition = craft.hexPos.goNW(tilesToMove);
    //                break;
    //            }
    //        default:
    //            {
    //                newPosition = new HexPosition();
    //                break;
    //            }
    //    }
    //
    //    UpdateAircraftPositionTo(ref craft, newPosition);
    //}

    //public static Maneuver RandomManeuver()
    //{
    //    int rand = Random.Range(0, (int)Maneuver.ID.Count);
    //    switch(rand)
    //    {
    //        case ((int)Maneuver.ID.STRAIGHT):
    //            {
    //                return Maneuver.Straight;
    //            }
    //        case (BasicManeuverID.ACCELERATE):
    //            {
    //                return Maneuver.Accelerate;
    //            }
    //        case (BasicManeuverID.CLIMB):
    //            {
    //                return Maneuver.Climb;
    //            }
    //        case (BasicManeuverID.DIVE):
    //            {
    //                return Maneuver.Dive;
    //            }
    //        case (BasicManeuverID.LEFT60):
    //            {
    //                return Maneuver.Left60;
    //            }
    //        case (BasicManeuverID.LEFT120):
    //            {
    //                return Maneuver.Left120;
    //            }
    //        case (BasicManeuverID.RIGHT60):
    //            {
    //                return Maneuver.Right60;
    //            }
    //        case (BasicManeuverID.RIGHT120):
    //            {
    //                return Maneuver.Right120;
    //            }
    //        case (BasicManeuverID.U_TURN):
    //            {
    //                return Maneuver.UTurn;
    //            }
    //        default:
    //            {
    //                return Maneuver.Straight;
    //            }
    //    }
    //}

    // Update is called once per frame
   /* void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            QueueBasicManeuver(BasicManeuverID.STRAIGHT, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            QueueBasicManeuver(BasicManeuverID.ACCELERATE, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            QueueBasicManeuver(BasicManeuverID.CLIMB, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            QueueBasicManeuver(BasicManeuverID.DIVE, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            QueueBasicManeuver(BasicManeuverID.LEFT60, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            QueueBasicManeuver(BasicManeuverID.RIGHT60, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            QueueBasicManeuver(BasicManeuverID.LEFT120, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            QueueBasicManeuver(BasicManeuverID.RIGHT120, ref testCraft);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            QueueBasicManeuver(BasicManeuverID.U_TURN, ref testCraft);
        }

        if (Input.anyKeyDown)
        {
            ExecuteOneManeuver();
            MoveAircraft(ref testCraft);
            testCraft.TriggerAnimation();
        }
    }*/


}
