using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private List<SpawnEntry> _spawnEntries = new();

    #endregion

    #region Public Methods

    public bool TryGetSpawnPoint(string fromSceneName, out Vector2 position, out bool facingRight)
    {
        foreach (SpawnEntry entry in _spawnEntries)
        {
            if (entry.FromSceneName == fromSceneName)
            {
                position = entry.Position;
                facingRight = entry.FacingRight;
                return true;
            }
        }
        position = Vector2.zero;
        facingRight = false;
        return false;
    }

    #endregion

    #region Editor Visualisation

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (SpawnEntry entry in _spawnEntries)
            entry.SyncSceneName();
    }
#endif

    #endregion

    [Serializable]
    private class SpawnEntry
    {
#if UNITY_EDITOR
        [SerializeField]
        private UnityEditor.SceneAsset _fromScene;
#endif

        [HideInInspector]
        [SerializeField]
        private string _fromSceneName;

        [SerializeField]
        private Transform _spawnPoint;

        [Tooltip("Direction the player faces when spawning from this scene.")]
        [SerializeField]
        private bool _facingRight;

        public string FromSceneName => _fromSceneName;
        public Vector2 Position =>
            _spawnPoint != null ? (Vector2)_spawnPoint.position : Vector2.zero;
        public bool FacingRight => _facingRight;

#if UNITY_EDITOR
        public void SyncSceneName() => _fromSceneName = _fromScene != null ? _fromScene.name : "";
#endif
    }
}
