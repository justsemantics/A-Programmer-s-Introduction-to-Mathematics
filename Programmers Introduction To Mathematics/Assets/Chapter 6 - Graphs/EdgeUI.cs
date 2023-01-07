using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeUI : MonoBehaviour
{

    [SerializeField]
    RectTransform rectTransform;


    public void Init(Edge e)
    {
        Vector2 fromPos = e.from.Position;
        Vector2 toPos = e.to.Position;

        float height = (toPos - fromPos).magnitude;

        float angle = Vector2.Angle(Vector2.up, toPos - fromPos);

        rectTransform.anchoredPosition = fromPos + (toPos - fromPos) / 2;

        rectTransform.sizeDelta.Set(rectTransform.sizeDelta.x, height);

        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
