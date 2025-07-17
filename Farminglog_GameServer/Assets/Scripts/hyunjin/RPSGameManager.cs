using Fusion;
using UnityEngine;
using System.Collections.Generic;

public enum RPSGameState {
    WAITINGFORPLAYERS,
    COUNTDOWN,
    RESULT
}

public class RPSGameManager : NetworkBehaviour
{
    [Networked] public RPSGameState currentState { get; set; }
    [Networked] private TickTimer roundTimer { get; set; }
    [Networked] public float RemainingTimer { get; set;}
    [Networked] public string resultText { get; set; }
    private RPSPlayer[] players;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        List<RPSPlayer> validPlayers = new List<RPSPlayer>();
        foreach(var playerRef in Runner.ActivePlayers) {
            var playerObj = Runner.GetPlayerObject(playerRef);
            if (playerObj != null && playerObj.TryGetComponent(out RPSPlayer player)) {
                validPlayers.Add(player);
            }
        }

        if (validPlayers.Count < 2) {
            currentState = RPSGameState.WAITINGFORPLAYERS;
            return;
        }

        players = validPlayers.ToArray();
        switch (currentState)
        {
            case RPSGameState.WAITINGFORPLAYERS:
                Debug.Log("[RPS] 2명 접속. 라운드 시작");
                StartRound();
                break;
            case RPSGameState.COUNTDOWN:
                players[0].RemainingTime = roundTimer.RemainingTime(Runner) ?? 0f;
                players[1].RemainingTime = roundTimer.RemainingTime(Runner) ?? 0f;
                if (roundTimer.Expired(Runner)) {
                    Debug.Log("[RPS] 5초 경과");
                    JudgeRound();
                }
                break;
            case RPSGameState.RESULT:
                break;
        }
    }

    void StartRound()
    {
        currentState = RPSGameState.COUNTDOWN;
        roundTimer = TickTimer.CreateFromSeconds(Runner, 5f);
        foreach (var p in players)
            p.CurrentChoice = RPSChoice.NONE;

        Debug.Log("[RPS] 5초 안에 선택하세요!");
    }

    void JudgeRound()
    {
        if (players == null || players.Length < 2) {
            Debug.LogWarning("[RPS] JudgeRound() 호출됐지만 플레이어 수 부족!");
            return;
        }

        var p1 = players[0];
        var p2 = players[1];

        var c1 = p1.CurrentChoice;
        var c2 = p2.CurrentChoice;

        int result = Judge(c1, c2);

        resultText = result switch
        {
            0 => "무승부",
            1 => "플레이어 1 승리",
            2 => "플레이어 2 승리",
            _ => "오류"
        };

        // 각 플레이어의 선택과 승패결과 저장
        p1.OpponentChoice = c2;
        p2.OpponentChoice = c1;
        p1.Result = result switch
        {
            0 => RPSResult.DRAW,
            1 => RPSResult.WIN,
            2 => RPSResult.LOSE,
            _ => RPSResult.NONE
        };
        p2.Result = result switch
        {
            0 => RPSResult.DRAW,
            1 => RPSResult.LOSE,
            2 => RPSResult.WIN,
            _ => RPSResult.NONE
        };

        Debug.Log($"[RPS] 결과: P1({c1}) vs P2({c2}) → {resultText}");
        currentState = RPSGameState.RESULT;
    }

    int Judge(RPSChoice a, RPSChoice b)
    {
        if (a == RPSChoice.NONE && b == RPSChoice.NONE) return 0;
        if (a == RPSChoice.NONE) return 2;
        if (b == RPSChoice.NONE) return 1;
        if (a == b) return 0;

        if ((a == RPSChoice.ROCK && b == RPSChoice.SCISSORS) ||
            (a == RPSChoice.PAPER && b == RPSChoice.ROCK) ||
            (a == RPSChoice.SCISSORS && b == RPSChoice.PAPER))
            return 1;

        return 2;
    }
}
