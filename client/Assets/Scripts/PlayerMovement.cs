using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController Controller;

    public void Move()
    {
        if (Controller != null)
        {
            P_PlayerMovement playerMovement = default;
            playerMovement.player_id = LocalPlayerInfo.ID;
            playerMovement.rotation = Controller.transform.rotation;
            playerMovement.dx = Input.GetAxis("Horizontal");
            playerMovement.dy = Input.GetAxis("Vertical");
            if (playerMovement.dx != 0 || playerMovement.dy != 0)
            {
                Client.UDP.SendPacket(E_PACKET.PLAYER_MOVEMENT, playerMovement);
            }

            // client-sided
            //Vector3 motion = (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical")) * 5f;
            //Move(motion);
        }
    }

    public void Move(Vector3 motion)
    {
        Controller.Move(motion);
    }
}
