using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Text dot1;
    public Text dot2;
    public Text dot3;

    private void Start()
    {
        AnimateDots();
    }

    private void AnimateDots()
    {
        dot1.color = new Color(dot1.color.r, dot1.color.g, dot1.color.b, 0);
        dot2.color = new Color(dot2.color.r, dot2.color.g, dot2.color.b, 0);
        dot3.color = new Color(dot3.color.r, dot3.color.g, dot3.color.b, 0);

        DOTween.Sequence()
            .Append(dot1.DOFade(1, 0.5f))
            .Append(dot1.DOFade(0, 0.5f))
            .Append(dot2.DOFade(1, 0.5f))
            .Append(dot2.DOFade(0, 0.5f))
            .Append(dot3.DOFade(1, 0.5f))
            .Append(dot3.DOFade(0, 0.5f))
            .SetLoops(-1);
    }
}
