using UnityEngine;

namespace ObjectiveSystem
{
    public enum ObjectiveType
    {
        Main,
        Side
    }

    [CreateAssetMenu(fileName = "New Objective", menuName = "Objective System/Objective")]
    public class Objective : ScriptableObject
    {
        public string id;
        public ObjectiveType type;
        public string description;
    }
}