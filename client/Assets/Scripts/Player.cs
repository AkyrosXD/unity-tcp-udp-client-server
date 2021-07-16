using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string Name;
    public long ID;
    public bool IsLocal;
    public PlayerMovement Movement;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Movement != null && IsLocal)
        {
            Movement.Move();
        }
    }
}
