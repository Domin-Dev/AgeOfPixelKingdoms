using System.Collections.Generic;
using UnityEngine;

public class PathFinding 
{
    int[] checks;

    public PathFinding()
    {
        checks = new int[GameManager.Instance.numberOfProvinces];
    }
    public List<int> FindPath(int startIndex, int targetIndex)
    {
        List<int> path = new List<int>();
        checks[targetIndex] = 1;
        CheckNeighbors(targetIndex);
        int currentProvince = startIndex;
        while (currentProvince != targetIndex)
        {
            List<int> neighbors = GetProvince(currentProvince).neighbors;
            int minValue = int.MaxValue;
            int selected = currentProvince;
            foreach (int i in neighbors)
            {
                if (checks[i] < minValue)
                {
                    minValue = checks[i];
                    selected = i;
                }
            }
            path.Add(selected);
            currentProvince = selected;
        }
        foreach (int i in path)
        {
            Debug.Log(i);
        }
        return null;
    }
    public void CheckNeighbors(int index)
    {
        List<int> neighbors = GetProvince(index).neighbors;
        int value = checks[index] + 1;
        int neighborIndex;
        for (int i = 0; i < neighbors.Count; i++)
        {
            neighborIndex = neighbors[i];
            if (checks[neighborIndex] == 0 || checks[neighborIndex] > value)
            {
                checks[neighborIndex] = value;
                CheckNeighbors(neighborIndex);
            }
        }
    }
    private ProvinceStats GetProvince(int index)
    {
        return GameManager.Instance.provinces[index];
    }
}