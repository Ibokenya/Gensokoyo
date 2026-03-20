using UnityEngine;

public class Game1 : MonoBehaviour
{
    void OnEnable()
    {
        Global_GameManager.Instance.state = State.Gaming;
    }
}
