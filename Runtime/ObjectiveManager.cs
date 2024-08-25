using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ObjectiveSystem
{
    [AddComponentMenu("Objective System/Objective Manager")]
    public class ObjectiveManager : NetworkBehaviour
    {
        public static ObjectiveManager Instance;

        public UnityEvent onAllObjectivesCompleted;

        [HideInInspector] public List<Objective> mainObjectives;
        [HideInInspector] public List<Objective> sideObjectives;

        private NetworkList<ObjectiveStatus> _networkedObjectives;

        #region Unity

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _networkedObjectives = new NetworkList<ObjectiveStatus>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                InitializeObjectives();
            }

            _networkedObjectives.OnListChanged += OnObjectivesChanged;
        }

        public override void OnNetworkDespawn()
        {
            _networkedObjectives.OnListChanged -= OnObjectivesChanged;
        }

        #endregion

        #region Functions

        private void OnObjectivesChanged(NetworkListEvent<ObjectiveStatus> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<ObjectiveStatus>.EventType.Value)
            {
                ObjectiveUI.Instance.UpdateObjectiveStatus(changeEvent.Value.Id.ToString(),
                    changeEvent.Value.IsCompleted);
            }
        }

        public void InitializeObjectives()
        {
            foreach (var obj in mainObjectives)
            {
                _networkedObjectives.Add(new ObjectiveStatus(obj.id, false));
            }

            foreach (var obj in sideObjectives)
            {
                _networkedObjectives.Add(new ObjectiveStatus(obj.id, false));
            }
        }

        private void CheckAllObjectivesCompleted()
        {
            foreach (var status in _networkedObjectives)
            {
                if (!status.IsCompleted)
                {
                    return;
                }
            }

            onAllObjectivesCompleted.Invoke();
            Debug.Log("All objectives completed!");
        }

        #endregion

        #region RPCs

        [ServerRpc(RequireOwnership = false)]
        public void CompleteObjectiveServerRpc(FixedString128Bytes objectiveId)
        {
            for (int i = 0; i < _networkedObjectives.Count; i++)
            {
                if (_networkedObjectives[i].Id.Equals(objectiveId))
                {
                    var objectiveStatus = _networkedObjectives[i];
                    objectiveStatus.IsCompleted = true;
                    _networkedObjectives[i] = objectiveStatus; // Update the list entry
                    ObjectiveUI.Instance.UpdateObjectiveStatusClientRpc(objectiveStatus.Id.ToString(),
                        true);
                    CheckAllObjectivesCompleted();
                    break;
                }
            }
        }

        #endregion
    }
}