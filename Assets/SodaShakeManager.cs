using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SodaShakeManager : MonoBehaviour
{
    public SodaController sodaController; // Assign in inspector

    private enum GameState { Waiting, PlayerTurn, AITurn, Result }
    private GameState state = GameState.Waiting;

    private bool gameActive = false;
    private bool playerExploded = false;
    private bool aiExploded = false;
    private float aiTurnDelay = 1.5f;
    private float aiTurnTimer = 0f;
    private int turnCount = 0;

    void Start()
    {
        state = GameState.Waiting;
        gameActive = false;
        playerExploded = false;
        aiExploded = false;
        aiTurnTimer = 0f;
        turnCount = 0;
        // Optionally, auto-start
        StartGame();
    }

    void Update()
    {
        if (!gameActive) return;

        switch (state)
        {
            case GameState.PlayerTurn:
                // Wait for player to shake (handled by SodaController)
                if (sodaController == null) break;
                if (IsSodaExploded())
                {
                    playerExploded = true;
                    state = GameState.Result;
                }
                // For demo: press space to end turn
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    state = GameState.AITurn;
                    aiTurnTimer = 0f;
                }
                break;
            case GameState.AITurn:
                aiTurnTimer += Time.deltaTime;
                // Visualize AI turn: e.g., change soda color or log message
                if (sodaController != null)
                {
                    // Example: change color to yellow during AI turn
                    var rend = sodaController.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material.color = Color.yellow;
                }
                if (aiTurnTimer < aiTurnDelay)
                {
                    Debug.Log("AI is thinking... (visual cue active)");
                }
                if (aiTurnTimer > aiTurnDelay)
                {
                    // AI decides to shake or not
                    if (!IsSodaExploded())
                    {
                        // Restore color before shaking
                        if (sodaController != null)
                        {
                            var rend = sodaController.GetComponent<Renderer>();
                            if (rend != null)
                                rend.material.color = Color.white;
                        }
                        sodaController.SimulateShake();
                        if (IsSodaExploded())
                        {
                            aiExploded = true;
                            state = GameState.Result;
                        }
                        else
                        {
                            state = GameState.PlayerTurn;
                        }
                    }
                }
                break;
            case GameState.Result:
                gameActive = false;
                if (playerExploded)
                    Debug.Log("Player exploded! AI wins.");
                else if (aiExploded)
                    Debug.Log("AI exploded! Player wins.");
                else
                    Debug.Log("Game ended unexpectedly.");
                break;
        }
    }

    public void StartGame()
    {
        state = GameState.PlayerTurn;
        gameActive = true;
        playerExploded = false;
        aiExploded = false;
        aiTurnTimer = 0f;
        turnCount = 0;
        Debug.Log("Soda Shake Game Started! Player goes first.");
    }

    private bool IsSodaExploded()
    {
        // Check if the soda has exploded (use SodaController's state)
        return sodaController != null && sodaController.GetType().GetField("exploded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(sodaController) as bool? == true;
    }
}
