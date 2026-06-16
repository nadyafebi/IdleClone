using UnityEngine;

public interface IPointerBlocker
{
    bool ContainsScreenPoint(Vector2 screenPos);
}
