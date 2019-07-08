using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EndPoint : MonoBehaviour {

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.Recycle();
    }
}
