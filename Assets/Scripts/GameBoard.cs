using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public static string KEY_UNIT_AIR = "Unit_Air";

    [SerializeField]
    GameObject[] terrainTypes;

    [SerializeField]
    public Vector2Int size;

    public List<HexPosition> traversiblePositions;

    public void ResetPositions()
    {
        foreach(var pos in traversiblePositions)
        {
            pos.remove(KEY_UNIT_AIR);
        }
    }

    void Generate()
    {
        traversiblePositions = new List<HexPosition>();
         
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int terrainIdx = Random.Range(0, terrainTypes.Length - 1);
                int heightOffset = Random.Range(0, 3);

                //Vector3 pos = new Vector3(x, 0, y);
                HexPosition newTile = new HexPosition(x,y);

                Vector3 newPos = newTile.getPosition();
                newPos.y = heightOffset * 0.08f;

                GameObject terrainInstance = Instantiate(terrainTypes[terrainIdx], newPos , Quaternion.Euler(0, 30, 0), this.transform);
                newTile.add("Terrain", terrainInstance);
                traversiblePositions.Add(newTile);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public bool isTraversibleAir(HexPosition p)
    {
        if(traversiblePositions.Contains(p))
        {
            return true;
            //if(p.containsKey(KEY_UNIT_AIR))
            //{
            //    return false;
            //} else
            //{
            //    return true;
            //}
        } else
        {
            return false;
        }
    }

    public HexPosition RandomPosition()
    {
        int idx = Random.Range(0, traversiblePositions.Count);
        return traversiblePositions[idx];
    }
}
