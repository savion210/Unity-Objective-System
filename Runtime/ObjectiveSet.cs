using UnityEngine;

namespace ObjectiveSystem
{
    [CreateAssetMenu(fileName = "New Objective Set", menuName = "Objective System/Objective Set")]
    public class ObjectiveSet : ScriptableObject
    {
        public string mapName;
        public GameObject environmentPrefab;
        public Objective[] mainObjectives;
        public Objective[] sideObjectives;
    }
}