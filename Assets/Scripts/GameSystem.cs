using UnityEngine.InputSystem;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one instance!");
            return;
        }

        Instance = this;

        //var player1 = PlayerInput.Instantiate(Resources.Load("LichKing") as GameObject, controlScheme: "KeyboardArrows", devices: new[] {Keyboard.current});
        //var player2 = PlayerInput.Instantiate(Resources.Load("LichKing") as GameObject, controlScheme: "KeyboardWASD", devices: new[] {Keyboard.current});
    }

    void Update()
    {
        // global game update logic goes here
    }

    void OnGui()
    {
        // common GUI code goes here
    }
}
