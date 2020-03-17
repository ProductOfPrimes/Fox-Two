using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Command : ScriptableObject
{
    // public UnitAircraft aircraft = null;
    //public void SetUnit(ref UnitAircraft craft)
    //{
    //    aircraft = craft;
    //}

    //public virtual void Execute(ref UnitAircraft aircraft)
    //{
    //}
}

[CreateAssetMenu(fileName = "Maneuver", menuName = "ScriptableObjects/Maneuver", order = 1)]
public class Maneuver : Command
{

    public Maneuver(ID a_id, short deltaS, short deltaA, short headingC, short accelerationM)
    {
        id = a_id;
        deltaSpeed = deltaS;
        deltaAltitude = deltaA;
        headingChange = headingC;
        accelerationMultiplier = accelerationM;
    }

    public ID id { get; }
    public short deltaSpeed { get; }
    public short deltaAltitude { get; }
    public short headingChange { get; }
    public short accelerationMultiplier { get; }


    public enum ID : short
    {
        NONE = -1,
        STRAIGHT = 0,
        ACCELERATE = 1,
        CLIMB = 2,
        DIVE = 3,
        LEFT60 = 4,
        LEFT120 = 5,
        RIGHT60 = 6,
        RIGHT120 = 7,
        U_TURN = 8,
        Count
    }

    public static Maneuver Straight = new Maneuver(ID.STRAIGHT, 0, 0, 0, 0);
    public static Maneuver Accelerate = new Maneuver(ID.ACCELERATE, 0, 0, 0, 1);
    public static Maneuver Climb = new Maneuver(ID.CLIMB, -6, +1, 0, 0);
    public static Maneuver Dive = new Maneuver(ID.DIVE, +6, -1, 0, 0);
    public static Maneuver Left60 = new Maneuver(ID.LEFT60, -1, 0, -1, 0);
    public static Maneuver Left120 = new Maneuver(ID.LEFT120, -3, 0, -2, 0);
    public static Maneuver Right60 = new Maneuver(ID.RIGHT60, -1, 0, +1, 0);
    public static Maneuver Right120 = new Maneuver(ID.RIGHT120, -3, 0, +2, 0);
    public static Maneuver UTurn = new Maneuver(ID.U_TURN, -6, 0, +3, 0);

    /// <summary>
    /// Static array of basic maneuvers indexed by ID
    /// </summary>
    public static Maneuver[] BasicManeuvers = new Maneuver[(short)ID.Count]
        {
            Straight,
            Accelerate,
            Climb,
            Dive,
            Left60,
            Left120,
            Right60,
            Right120,
            UTurn
        };

    public static Maneuver ToManeuver(ID maneuverID)
    {
        return BasicManeuvers[(int)maneuverID];
    }
}

//public void Execute(ref UnitAircraft craft)
//{
//    craft.speed += deltaSpeed;
//    craft.altitude += deltaAltitude;
//    craft.heading = (craft.heading + headingChange + 6) % 6;
//    craft.speed += craft.acceleration * accelerationMultiplier;
//}
//override public void Execute(ref UnitAircraft aircraft)
//{
//    aircraft.speed += deltaSpeed;
//    aircraft.altitude += deltaAltitude;
//    aircraft.heading = (aircraft.heading + headingChange + 6) % 6;
//    aircraft.speed += aircraft.acceleration * accelerationMultiplier;
//}

//class BasicManeuvers
//{
//    public static void StallCheck(ref UnitAircraft craft)
//    {
//        if(craft.speed < 1)
//        {
//            //Stall.Execute(craft);
//        }
//    }

//    public static void Left60(ref UnitAircraft craft)
//    {
//    }
//    public static void Left120(ref UnitAircraft craft)
//    {
//    }
//    public static void Right60(ref UnitAircraft craft)
//    {
//    }
//    public static void Right120(ref UnitAircraft craft)
//    {
//    }
//    public static void Climb(ref UnitAircraft craft)
//    {
//    }
//    public static void Dive(ref UnitAircraft craft)
//    {
//    }
//    public static void Idle(ref UnitAircraft craft)
//    {
//    }
//    public static void Accelerate(ref UnitAircraft craft)
//    {
//    }
//    public static void Stall(ref UnitAircraft craft)
//    {
//    }
//}
