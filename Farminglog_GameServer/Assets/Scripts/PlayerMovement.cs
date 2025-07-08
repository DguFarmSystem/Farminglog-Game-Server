// Fusion
using Fusion;

// Unity
using UnityEngine;

public struct PlayerInputData : INetworkInput
{
    public float horizontal;
    public float vertical;
}

[DisallowMultipleComponent]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Networked] private Vector3 Velocity { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out PlayerInputData input))
        {
            // ������ ��ġ ����� ��
            if (Object.HasStateAuthority)
            {
                Vector2 moveDir = new Vector2(input.horizontal, input.vertical).normalized;
                Velocity = moveDir * moveSpeed;
                transform.position += Velocity * Runner.DeltaTime;
            }
        }
    }
}
