using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    HexPosition mouse = null;
    [SerializeField]
    GameObject marker = null;

    [SerializeField]
    GameObject cursor;

    // Start is called before the first frame update
    void Start()
    {
        HexPosition.setColor("Cursor", Color.yellow, 1);
        HexPosition.setColor("Friendly", Color.blue, 2);
        HexPosition.setColor("Enemy", Color.red, 3);
        HexPosition.Marker = marker;
        //for(int u = 0; u < board.size.x; u++)
        //{
        //    for (int v = 0; v < board.size.y; u++)
        //    {
        //        HexPosition.sele
        //        //board.positions[u,v].add
        //    }
        //}
    }

    private HexPosition getMouseHex()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length == 0)
        {
            return null;
        }
        else
        {
            float minDist = float.PositiveInfinity;
            int min = 0;
            for (int i = 0; i < hits.Length; ++i)
            {
                if (hits[i].distance < minDist)
                {
                    minDist = hits[i].distance;
                    min = i;
                }
            }
            cursor.transform.position = hits[min].point;
            return (new HexPosition(hits[min].point));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.mousePresent)
        {
            mouse = null;
        }
        else
        {
            HexPosition newMouse = getMouseHex();
            if (newMouse == null)
            {
                HexPosition.clearSelection("Cursor");
            }
            else
            {
                if (newMouse != mouse)
                {
                    if (mouse != null)
                    {
                        mouse.unselect("Cursor");
                    }
                   
                    mouse = newMouse;
                    mouse.select("Cursor");
                }
            }
        }
    }
}
