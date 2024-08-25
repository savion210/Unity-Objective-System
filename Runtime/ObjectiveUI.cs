using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace ObjectiveSystem
{
    [AddComponentMenu("Frauds/ObjectiveSystem/ObjectiveUI")]
    public class ObjectiveUI : NetworkBehaviour
    {
        public static ObjectiveUI Instance;

        [SerializeField] private GameObject objectiveUIPrefab;
        [SerializeField] private Transform objectiveUIParent;

        private Dictionary<string, TMP_Text> _objectiveTexts = new Dictionary<string, TMP_Text>();

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
        }

        #endregion

        #region Functions

        public void InitializeObjectiveUI(List<Objective> objectives)
        {
            foreach (var objective in objectives)
            {
                var uiElement = Instantiate(objectiveUIPrefab, objectiveUIParent);
                var textComponent = uiElement.GetComponent<TMP_Text>();
                textComponent.text = objective.description;
                _objectiveTexts[objective.id] = textComponent;

                if (IsServer)
                {
                    NetworkObject networkObject = uiElement.GetComponent<NetworkObject>();
                    networkObject.Spawn();
                    SetParentAndInitialize(uiElement, objective.id, objective.description);
                }
            }
        }

        public void UpdateObjectiveStatus(string objectiveId, bool isCompleted)
        {
            if (_objectiveTexts.TryGetValue(objectiveId, out var text))
            {
                text.color = isCompleted ? Color.green : Color.red;
            }
        }

        private void SetParentAndInitialize(GameObject uiElement, string objectiveId, string description)
        {
            var networkObject = uiElement.GetComponent<NetworkObject>();
            SetParentServerRpc(networkObject.NetworkObjectId,
                objectiveUIParent.GetComponent<NetworkObject>().NetworkObjectId, objectiveId, description);
        }

        #endregion

        #region RPCs

        [ClientRpc]
        public void UpdateObjectiveStatusClientRpc(string objectiveId, bool isCompleted)
        {
            UpdateObjectiveStatus(objectiveId, isCompleted);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetParentServerRpc(ulong childNetworkObjectId, ulong parentNetworkObjectId, string objectiveId,
            string description)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(childNetworkObjectId,
                    out NetworkObject childNetworkObject) &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetworkObjectId,
                    out NetworkObject parentNetworkObject))
            {
                childNetworkObject.transform.SetParent(parentNetworkObject.transform);
                SetParentAndInitializeClientRpc(childNetworkObjectId, parentNetworkObjectId, objectiveId, description);
            }
        }

        [ClientRpc]
        private void SetParentAndInitializeClientRpc(ulong childNetworkObjectId, ulong parentNetworkObjectId,
            string objectiveId, string description)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(childNetworkObjectId,
                    out NetworkObject childNetworkObject) &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetworkObjectId,
                    out NetworkObject parentNetworkObject))
            {
                childNetworkObject.transform.SetParent(parentNetworkObject.transform);
                var textComponent = childNetworkObject.GetComponent<TMP_Text>();
                textComponent.text = description;
                _objectiveTexts[objectiveId] = textComponent;
            }
        }

        #endregion
    }
}