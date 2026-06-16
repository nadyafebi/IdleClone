using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialog", menuName = "IdleClone/Dialog Data")]
public class DialogData : ScriptableObject
{
    #region Serialized Fields

    [SerializeField]
    public string SpeakerName;

    [SerializeField]
    public List<string> Lines = new();

    #endregion
}
