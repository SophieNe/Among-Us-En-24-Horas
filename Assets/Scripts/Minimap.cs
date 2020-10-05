using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class Minimap : MonoBehaviour
{
    public RectTransform playerInMap, map2dEnd;
    public Transform player, map3dParent, map3dEnd;
    public RectTransform[] taskIconsPool;
    public Dictionary<Task, RectTransform> taskIcons = new Dictionary<Task, RectTransform>();

    private Vector3 normalized, mapped;

    private void Update()
    {
        if (player != null)
        {
            playerInMap.localPosition = FindPositionInMinimap(player.transform);
        }
    }
    private Vector3 FindPositionInMinimap(Transform trans)
    {
        normalized = Divide(map3dParent.InverseTransformPoint(trans.position), map3dEnd.position - map3dParent.position);
        mapped = Multiply(normalized, map2dEnd.localPosition);
        mapped.z = 0;
        return mapped;
    }
    public void AddTaskToMinimap(Task task)
    {
        Debug.Log(task.gameObject.name);
        RectTransform taskIcon = GetNextEmptyTask();
        taskIcon.gameObject.SetActive(true);
        taskIcon.localPosition = FindPositionInMinimap(task.transform);
        taskIcons.Add(task, taskIcon);
    }
    public void RemoveTaskFromMinimap(Task task)
    {
        RectTransform taskIcon = taskIcons[task];
        taskIcon.gameObject.SetActive(false);
        taskIcons.Remove(task);
    }
    private RectTransform GetNextEmptyTask()
    {
        foreach (RectTransform task in taskIconsPool)
        {
            if (!task.gameObject.activeInHierarchy)
            {
                return task;
            }
        }
        return null;
    }
    private Vector3 Divide(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    private Vector3 Multiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}