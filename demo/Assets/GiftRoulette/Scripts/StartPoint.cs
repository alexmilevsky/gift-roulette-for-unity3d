using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StartPoint : MonoBehaviour {
    
    public static event System.Action ItemExit;

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (ItemExit != null) ItemExit();
    }
}
