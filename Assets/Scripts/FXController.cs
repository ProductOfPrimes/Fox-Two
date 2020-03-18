using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    [SerializeField]
    GameObject deathExplosionPrefab;

    [SerializeField]
    Material dead;

    public void OnUnitDeath(UnitAircraft unit, string killer)
    {
        Debug.Log(unit.callsign + " killed by: " + killer);
        Destroy(Instantiate(deathExplosionPrefab, unit.hexPos.getPosition(), Quaternion.identity), deathExplosionPrefab.GetComponent<ParticleSystem>().main.duration + 0.1f);
        //unit.GetComponent<Renderer>().material = dead;
    }

    //// Start is called before the first frame update
    //void Start()
    //{
    //    
    //}
    //
    //// Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
