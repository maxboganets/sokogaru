using UnityEngine.InputSystem;
using UnityEngine;

public class Player
{
    private PlayerInput player;
    private int playerIndex;

    public Player(GameObject playerPrefab, int playerIndex, string controlScheme, InputDevice controlDevice)
    {
        this.playerIndex = playerIndex;

        player = PlayerInput.Instantiate(
            playerPrefab,
            -1,
            controlScheme,
            -1,
            controlDevice
        );

        player.GetComponent<Rigidbody2D>().transform.position = GameObject.Find("Player"+ playerIndex +"Spawn").transform.position;
    }

    public int GetIndex()
    {
        return this.playerIndex;
    }

    public PlayerInput GetPlayerInput()
    {
        return this.player;
    }
}

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    private void Start()
    {
        var player1 = new Player(Resources.Load("LichKing") as GameObject, 1, "KeyboardWASD", Keyboard.current);
        var player2 = new Player(Resources.Load("ArcaneArcher") as GameObject, 2, "KeyboardArrows", Keyboard.current);
        Physics2D.IgnoreCollision(player1.GetPlayerInput().GetComponent<Collider2D>(), player2.GetPlayerInput().GetComponent<Collider2D>());
    }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one instance!");
            return;
        }

        Instance = this;
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
