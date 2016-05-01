using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawningPool : MonoBehaviour {

    public Transform container;
    public GameObject unitPrefab;
    public int initialAmount = 0;
    public bool populateOnAwake = true;
    public bool isDynamic = true;

    private List<GameObject> units;

    public List<GameObject> Units
    {
        get
        {
            return units;
        }
    }

    void Awake()
    {
        units = new List<GameObject>();

        if(populateOnAwake)
        {
            for (int i = 0; i < initialAmount; i++)
            {
                SpawnUnit(false);
            }
        }
    }

    private GameObject SpawnUnit(bool isActive)
    {
        GameObject go = Instantiate(unitPrefab) as GameObject;
        go.SetActive(isActive);
        units.Add(go);

        if (container)
        {
            go.transform.SetParent(container);
        }

        go.transform.localPosition = new Vector3(0, 0, 0);

        return go;
    }

    public GameObject GetUnit()
    {
        int len = units.Count;
        for (int i = 0; i < len; i++)
        {
            if(!units[i].activeSelf)
            {
                return units[i];
            }
        }

        if(isDynamic)
        {
            return SpawnUnit(true);
        }

        throw new System.Exception("Spawn more overlords!");
    }

    public void ReturnUnit(GameObject unit)
    {
        if(!units.Contains(unit))
        {
            throw new System.Exception("This hatchling is not part of the swarm");
        }

        unit.SetActive(false);
        unit.transform.SetParent(container);
        unit.transform.localScale = new Vector3(1, 1, 1);
    }

}
