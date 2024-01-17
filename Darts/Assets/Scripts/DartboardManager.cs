using UnityEngine;

public enum DartGameStates {
    Loading,
    Waiting,
    Playing,
    Finished
}

public enum DartSectionTypes {
    Miss,
    Single,
    Double,
    Triple
}

public enum DartGameEvents {
    GameStart,
    StartTurn,
    EndTurn,
    StartThrow,
    EndThrow,
    Miss,
    Hit,
    GameOver,
    Bust
}

public class DartboardManager : MonoBehaviour {
    private DartGameStates GameState = DartGameStates.Loading;

    private int throwsPerTurn = 3;
    private int currentThrow = 1; // current throw for the current turn

    private int playerThrowCount = -1;
    private int playerTurnCount = -1;
    private int playerScore = -1;
    private DartboardThrowInfo lastThrowInfo;
    
    private bool gameOver = false;
    private int startingScore = 501;
    private int scoreAtStartOfTurn; // the current players score at the start of the turn

    public void StartGame() {
        if (GameState is DartGameStates.Loading or DartGameStates.Playing) {
            return;
        }

        currentThrow = 1;

        playerScore = 0;
        playerThrowCount = 0;
        playerTurnCount = 0;

        gameOver = false;
        playerScore = startingScore;
        GameState = DartGameStates.Playing;
        TriggerGameEvent(DartGameEvents.GameStart);

        StartTurn();
    }

    public int GetPlayerScore() {
        return playerScore;
    }
    
    public void EndGame() {
        GameState = DartGameStates.Finished;
        gameOver = true;
        TriggerGameEvent(DartGameEvents.GameOver);
    }
    
    private void StartTurn() {
        currentThrow = 1;
        playerTurnCount += 1;
        scoreAtStartOfTurn = playerScore;
        TriggerGameEvent(DartGameEvents.StartTurn);
        StartThrow();
    }
    
    public void EndTurn() {
        TriggerGameEvent(DartGameEvents.EndTurn);
        StartTurn();
    }
    
    private void StartThrow() {
        TriggerGameEvent(DartGameEvents.StartThrow);
    }
    
    public void EndThrow() {
        currentThrow += 1;
        TriggerGameEvent(DartGameEvents.EndThrow);
        if (currentThrow > throwsPerTurn) {
            EndTurn();
        }
        else {
            StartThrow();
        }
    }

    public void TriggerGameEvent(DartGameEvents gameEvent) {
        if (DartGameEventCallback != null) {
            DartGameEventCallback(gameEvent);
        }
    }

    public DartboardThrowInfo GetLastThrowInfo() {
        return lastThrowInfo;
    }

    public void RegisterHit(DartboardThrowInfo hitInfo) {
        if (GameState == DartGameStates.Playing) {
            lastThrowInfo = hitInfo;
            playerThrowCount += 1;

            Hit(hitInfo);
            TriggerGameEvent(DartGameEvents.Hit);
        }
    }
    
    private void Hit(DartboardThrowInfo hitInfo) {
        var oldScore = playerScore;
        var newScore = oldScore - hitInfo.Score;

        if (newScore >= 0) {
            playerScore = newScore;
        } else {
            playerScore = scoreAtStartOfTurn;
            TriggerGameEvent(DartGameEvents.Bust);
            EndTurn();
            return;
        }

        checkWinningConditions();
        if (!gameOver) {
            EndThrow();
        }
    }

    public void RegisterMiss() {
        if (GameState == DartGameStates.Playing) {
            lastThrowInfo = new DartboardThrowInfo(DartSectionTypes.Miss, 0);
            playerThrowCount += 1;

            EndThrow();
            TriggerGameEvent(DartGameEvents.Miss);
        }
    }
    
    private void checkWinningConditions() {
        if (playerScore == 0) {
            gameOver = true;
        }
        if (gameOver) {
            EndGame();
        }
    }

    public static DartboardManager GetDartboardManager() {
        return FindObjectOfType<DartboardManager>();
    }
    
    
    
    public delegate void DartGameDelegateEvent(DartGameEvents gameEvent);

    private DartGameDelegateEvent DartGameEventCallback;

    public void Subscribe(DartGameDelegateEvent callback) {
        DartGameEventCallback += callback;
    }

    public void Unsubscribe(DartGameDelegateEvent callback) {
        DartGameEventCallback -= callback;
    }

    private void Start() {
        GameState = DartGameStates.Waiting;
    }
}