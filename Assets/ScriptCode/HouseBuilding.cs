using UnityEngine;

public class HouseBuilding : MonoBehaviour
{
    public int workersProvided = 2; // How many workers this house gives

    void Start()
    {
        // As soon as the building spawns in the world, add the population
        ResourceManager.Instance.AddPopulation(workersProvided);
    }
}