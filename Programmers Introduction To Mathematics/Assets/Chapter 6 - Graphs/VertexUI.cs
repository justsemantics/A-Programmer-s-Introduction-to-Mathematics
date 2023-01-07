using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VertexUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Text;

    [SerializeField]
    RectTransform rectTransform;

    [SerializeField]
    Image image;
    public void Init(Vertex v)
    {
        Text.text = v.Name;

        rectTransform.anchoredPosition = v.Position;

        image.color = v.Color;
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
