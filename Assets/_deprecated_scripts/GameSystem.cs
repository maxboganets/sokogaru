using UnityEngine.InputSystem;
using UnityEngine;

public class DeprecatedPlayer
{
    private PlayerInput player;
    private int playerIndex;
    private GameObject healthBar;

    public DeprecatedPlayer(GameObject playerPrefab, int playerIndex, string controlScheme, InputDevice controlDevice)
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

    public void AssignHealthBar(GameObject healthBarPrefab)
    {
        this.healthBar = GameObject.Instantiate(
            healthBarPrefab,
            Vector2.zero,
            Quaternion.identity,
            GameObject.Find("GUI").transform
        );
        this.player.GetComponent<PlayerController>().AssignHealthBar(this.healthBar.GetComponent<HealthBar>());
    }

    public GameObject GetHealthBar()
    {
        return this.healthBar;
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

    private Resolution screenResolution;
    private int healthBarOffsetX = 40;
    private int healthBarOffsetY = 40;

    private DeprecatedPlayer player1, player2;

    private void Start()
    {
        // Save current screen resolution
        this.screenResolution = Screen.currentResolution;
        // Load Prefabs from resources
        var player1Prefab = Resources.Load("LichKing") as GameObject;
        var player2Prefab = Resources.Load("ArcaneArcher") as GameObject;
        // Create Players
        this.player1 = new DeprecatedPlayer(player1Prefab, 1, "KeyboardWASD", Keyboard.current);
        this.player2 = new DeprecatedPlayer(player2Prefab, 2, "KeyboardArrows", Keyboard.current);
        // Create Health Bars and attach them to GUI
        var healthBarPrefab = Resources.Load("HealthBar") as GameObject;
        this.player1.AssignHealthBar(healthBarPrefab);
        this.player2.AssignHealthBar(healthBarPrefab);
        // Flip Players if needed
        this.player2.GetPlayerInput().gameObject.GetComponent<SpriteRenderer>().flipX = true;
        // Ignore collisions between players
        Physics2D.IgnoreCollision(this.player1.GetPlayerInput().GetComponent<Collider2D>(), this.player2.GetPlayerInput().GetComponent<Collider2D>());
        // Update Health Bars positions
        this.UpdateGUIPosition();
    }

    private void Update()
    {
        if (this.screenResolution.width != Screen.currentResolution.width)
        {
            this.UpdateGUIPosition();
        }
    }

    private void UpdateGUIPosition()
    {
        RectTransform canvasTransform = GameObject.Find("GUI").GetComponent<RectTransform>();
        var healthBarRealWidth = this.player1.GetHealthBar().GetComponent<RectTransform>().sizeDelta.x
            * this.player1.GetHealthBar().GetComponent<RectTransform>().localScale.x;

        player1.GetHealthBar().GetComponent<Transform>().position = new Vector2(
                healthBarOffsetX + (int)(healthBarRealWidth / 2),
                canvasTransform.rect.height - healthBarOffsetY
            );
        player2.GetHealthBar().GetComponent<Transform>().position = new Vector2(
                canvasTransform.rect.width - (int)(healthBarRealWidth / 2) - healthBarOffsetX,
                canvasTransform.rect.height - healthBarOffsetY
            );
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

    void OnGui()
    {
        // common GUI code goes here
    }
}
