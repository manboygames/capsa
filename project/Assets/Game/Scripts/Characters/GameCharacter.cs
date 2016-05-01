using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameCharacter : Character
{

    public TextMesh textName;

    private CanvasScaler canvasScaler;
    protected override void Awake()
    {
        base.Awake();
        canvasScaler = GameObject.Find("CanvasGame").GetComponent<CanvasScaler>();
        animator.speed = 0.75f;
    }
    public override void SetCharacter(Character newCharacter)
    {
        base.SetCharacter(newCharacter);
        spriteRenderer.transform.localPosition /= canvasScaler.referencePixelsPerUnit;
        Vector3 pos = spriteRenderer.transform.localPosition;
        spriteRenderer.transform.localPosition = new Vector3(pos.x, pos.y - 0.5f, pos.z);
    }
}
