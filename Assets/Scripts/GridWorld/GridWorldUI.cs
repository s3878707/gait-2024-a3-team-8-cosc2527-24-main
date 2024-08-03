using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridWorldUI : MonoBehaviour
{
    [SerializeField] private Text TopLeftText;
    [SerializeField] private Text TopRightText;
    [SerializeField] private Text BottomLeftText;
    [SerializeField] private Text BottomRightText;

    public void UpdateTopLeftText(string newText)
    {
        TopLeftText.text = newText;
    }

    public void UpdateTopRightText(string newText)
    {
        TopRightText.text = newText;
    }

    public void UpdateBottomLeftText(string newText)
    {
        BottomLeftText.text = newText;
    }

    public void UpdateBottomRightText(string newText)
    {
        BottomRightText.text = newText;
    }
}
