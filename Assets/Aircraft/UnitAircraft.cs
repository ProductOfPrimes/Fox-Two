using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitAircraft : MonoBehaviour
{
    static int HEIGHT_PER_ALTITUDE = 1;
    public const int ALTITUDE_MAX = 3;

    public Team team = Team.NEUTRAL;

    /// <summary>
    /// name (should be unique!)
    /// </summary>
    public string callsign = "";
    [Range(0, 5)]
    public int heading = Heading.N;
    [Range(0, 5)]
    public int altitude = 1;
    [Range(0, 18)]
    public int speed = 10;
    [Range(0, 10)]
    public int hitPoints = 1;

    public int hitPointsMax = 1;
    public int speedMax = 6;
    public int altitudeBase = 2;
    public int acceleration = 1;

    public HexPosition hexPos = new HexPosition(0, 0);

    public delegate void UnitKillEvent(UnitAircraft killed, string killer);
    public event UnitKillEvent deathSubscribers;

    [SerializeField]
    ParticleSystem gunsParticles;

    public UnitAircraft Clone()
    {
        var clone = (UnitAircraft)this.MemberwiseClone();
        clone.gunsParticles = null;
        clone.deathSubscribers = null;
        clone.callsign += "(clone)";

        return clone;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (callsign == "")
        {
            callsign = gameObject.name; // TODO: make callsigns unique identifiers!
        }

        Respawn(hexPos, heading);
    }

    public void Die(string damageSource = "Unknown")
    {
        hexPos.remove(GameBoard.KEY_UNIT_AIR);
        altitude = 0;
        speed = 0;

        if(deathSubscribers != null)
        {
            deathSubscribers.Invoke(this, damageSource);
        }
    }

    public void TakeDamage(int damage, string damageSource = "Unknown")
    {
        if(hitPoints > 0)
        {
            hitPoints -= damage;
            if (hitPoints < 1)
            {
                Die(damageSource);
            }
        }

    }
    public void Respawn(HexPosition a_position, int a_heading = Heading.N)
    {
        hitPoints = hitPointsMax;
        hexPos = a_position;
        heading = a_heading;
        altitude = altitudeBase;
        speed = speedMax;
        hexPos.add(GameBoard.KEY_UNIT_AIR, this);
    }

    public void AttackAnimation()
    {
        gunsParticles.Play();
    }

    public void Attack(UnitAircraft other)
    {
        other.TakeDamage(3, callsign);
    }

    //public bool CanPerformManeuver(ref Maneuver m)
    //{
    //    int newSpeed = speed + m.deltaSpeed;
    //    int newAltitude = altitude + m.deltaAltitude;
    //
    //    return newSpeed >= 0 && newAltitude > 0;
    //}
    //
    //public List<Maneuver> GetAvailableManeuvers()
    //{
    //    List<Maneuver> availableManeuvers = new List<Maneuver>();
    //
    //  //availableManeuvers.Clear();
    //    availableManeuvers.Add(Maneuver.Straight);
    //    availableManeuvers.Add(Maneuver.Accelerate);
    //
    //    if(CanPerformManeuver(ref Maneuver.Climb))
    //    {
    //        availableManeuvers.Add(Maneuver.Climb);
    //    }
    //
    //    if (CanPerformManeuver(ref Maneuver.Left60))
    //    {
    //        availableManeuvers.Add(Maneuver.Left60);
    //        availableManeuvers.Add(Maneuver.Right60);
    //    }
    //
    //    if (CanPerformManeuver(ref Maneuver.Left120))
    //    {
    //        availableManeuvers.Add(Maneuver.Left120);
    //        availableManeuvers.Add(Maneuver.Right120);
    //    }
    //
    //    if (CanPerformManeuver(ref Maneuver.UTurn))
    //    {
    //        availableManeuvers.Add(Maneuver.UTurn);
    //    }
    //
    //    return availableManeuvers;
    //}

    IEnumerator anim = null;
    public void TriggerAnimation()
    {
        if (anim != null)
        {
            StopCoroutine(anim);
        }
        anim = AnimateStats();
        StartCoroutine(anim);
    }

    IEnumerator AnimateStats()
    {
        Quaternion targetHeading = Quaternion.Euler(0, 60 * heading, 0);
        float headingError = Quaternion.Angle(targetHeading, transform.rotation);

        Vector3 targetPosition = hexPos.getPosition();
        targetPosition.y = HEIGHT_PER_ALTITUDE * altitude;

        Vector3 posError = targetPosition - transform.position;

        float timerMax = 1.0f;
        float timer = timerMax;

        // rotate/climb
        while (timer > 0)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetHeading, headingError / timerMax);
            transform.position += posError * 0.2f;

            posError = targetPosition - transform.position;

            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
    }

    public void UpdatePositionTo(HexPosition pos)
    {
        hexPos.remove(GameBoard.KEY_UNIT_AIR);
        hexPos = pos;
        hexPos.add(GameBoard.KEY_UNIT_AIR, this);
    }

    //public void MoveAircraft()
    //{
    //    // move according to heading and altitude
    //    int tilesToMove = ManeuverManager.SpeedToMovement(speed);
    //
    //    HexPosition newPosition;
    //    switch (heading)
    //    {
    //        case (Heading.N):
    //            {
    //                newPosition = hexPos.goN(tilesToMove);
    //                break;
    //            }
    //        case (Heading.NE):
    //            {
    //                newPosition = hexPos.goNE(tilesToMove);
    //                break;
    //            }
    //        case (Heading.SE):
    //            {
    //                newPosition = hexPos.goSE(tilesToMove);
    //                break;
    //            }
    //        case (Heading.S):
    //            {
    //                newPosition = hexPos.goS(tilesToMove);
    //                break;
    //            }
    //        case (Heading.SW):
    //            {
    //                newPosition = hexPos.goSW(tilesToMove);
    //                break;
    //            }
    //        case (Heading.NW):
    //            {
    //                newPosition = hexPos.goNW(tilesToMove);
    //                break;
    //            }
    //        default:
    //            {
    //                newPosition = new HexPosition();
    //                break;
    //            }
    //    }
    //    UpdatePositionTo(newPosition);
    //}
    //public void Left60()
    //{
    //    heading = (heading + 5) % 6;
    //}
    //public void Left120()
    //{
    //    heading = (heading + 4) % 6;
    //}
    //public void Right60()
    //{
    //    heading = (heading + 1) % 6;
    //}
    //public void Right120()
    //{
    //    heading = (heading + 1) % 6;
    //}
    //public void Climb()
    //{
    //    altitude += 1;
    //    speed -= 6;
    //}
    //public void Dive()
    //{
    //    altitude -= 1;
    //    speed += 6;
    //}
    //public void Idle()
    //{
    //    // nothing
    //}

    public void PerformManeuver(Maneuver m)
    {
        speed += m.deltaSpeed;
        altitude += m.deltaAltitude;
        heading = (heading + m.headingChange + 6) % 6;
        speed += acceleration * m.accelerationMultiplier;
        TruncateStats();
    }

    public void Accelerate()
    {
        speed += acceleration;
    }
    public bool StallCheck()
    {
        if (speed < 1)
        {
            Stall();
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Stall()
    {
        //Debug.Log(callsign + " stalled!");
        altitude -= 2;
        speed = 1;
    }
    public bool CrashCheck()
    {
        if (altitude < 1)
        {
            Crash();
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Crash()
    {
        TakeDamage(hitPoints, "Crashing into the ground");
    }

    public HexPosition LookAhead(int numTiles)
    {
        HexPosition newPosition;
        switch (heading)
        {
            case (Heading.N):
                {
                    newPosition = hexPos.goN(numTiles);
                    break;
                }
            case (Heading.NE):
                {
                    newPosition = hexPos.goNE(numTiles);
                    break;
                }
            case (Heading.SE):
                {
                    newPosition = hexPos.goSE(numTiles);
                    break;
                }
            case (Heading.S):
                {
                    newPosition = hexPos.goS(numTiles);
                    break;
                }
            case (Heading.SW):
                {
                    newPosition = hexPos.goSW(numTiles);
                    break;
                }
            case (Heading.NW):
                {
                    newPosition = hexPos.goNW(numTiles);
                    break;
                }
            default:
                {
                    newPosition = new HexPosition();
                    break;
                }
        }
        return newPosition;
    }


    /// <summary>
    /// Calculate tiles to move based on speed thresholds
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    public static int SpeedToMovement(int speed)
    {

        if (speed >= 18)
        {
            return 4;
        }
        else if (speed >= 12)
        {
            return 3;
        }
        else if (speed >= 6)
        {
            return 2;
        }
        else if (speed >= 1)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void TruncateStats()
    {
        speed = (speed > speedMax) ? speedMax : (speed < 0) ? 0 : speed;
        altitude = (altitude > ALTITUDE_MAX) ? ALTITUDE_MAX : (altitude < 0) ? 0 : altitude;
    }


    /// <summary>
    /// Preview position if the plane was to move
    /// </summary>
    /// <returns></returns>
    public HexPosition ProjectAhead()
    {
        int tilesToMove = SpeedToMovement(speed);
        return LookAhead(tilesToMove);
    }

    /// <summary>
    /// move the plane ahead
    /// </summary>
    public void Move()
    {
        HexPosition newPos = ProjectAhead();

        // crash?
        if (newPos.containsKey(GameBoard.KEY_UNIT_AIR))
        {
            UnitAircraft other = (UnitAircraft)newPos.getValue(GameBoard.KEY_UNIT_AIR);
            if (other != this)
            {
                other.Die("Mid-air collision with " + callsign);
                UpdatePositionTo(newPos);
                Die("Mid-air collision with " + other.callsign);
            }
        } else
        {
            UpdatePositionTo(newPos);
        }
    }

}