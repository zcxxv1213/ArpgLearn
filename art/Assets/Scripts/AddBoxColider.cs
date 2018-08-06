using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBoxColider : MonoBehaviour {
    private void Awake()
    {
        var v = this.gameObject.AddComponent<BoxCollider2D>();
        var rectTransform = this.gameObject.GetComponent<RectTransform>();
        v.size = new Vector2(rectTransform.sizeDelta.x,rectTransform.sizeDelta.y);

    }
}
