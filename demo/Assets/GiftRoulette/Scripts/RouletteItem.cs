using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteItem : MonoBehaviour {

    private Transform _cachedTransform;
    public Transform cachedTransform
    {
        get
        {
            if (this._cachedTransform == null) this._cachedTransform = this.transform;
            return this._cachedTransform;
        }

        set
        {
            _cachedTransform = value;
        }
    }

    private void Start()
    {
        Roulette.RecycleItemsEvent += RecycleThis;
    }

    private void OnDestroy()
    {
        Roulette.RecycleItemsEvent -= RecycleThis;
    }

    private void RecycleThis()
    {
        this.gameObject.Recycle();
    }

    private void Update()
    {
        cachedTransform.Translate(Vector3.left * Roulette.speed * 100 * Time.deltaTime);
    }

}
