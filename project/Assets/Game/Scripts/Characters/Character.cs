using CodeBureau;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour {

    public enum Emotion
    {
        [StringValue("Idle")]
        Normal,
        [StringValue("Win")]
        Happy,
        [StringValue("Lose")]
        Sad
    }

    public int characterID;
    public SpriteRenderer spriteRenderer;

    protected Animator animator;


    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public virtual void SetCharacter(Character newCharacter)
    {
        characterID = newCharacter.characterID;
        animator.runtimeAnimatorController = newCharacter.GetComponent<Animator>().runtimeAnimatorController;
        spriteRenderer.sprite = newCharacter.GetComponentInChildren<SpriteRenderer>().sprite;

        spriteRenderer.transform.localPosition = newCharacter.spriteRenderer.transform.localPosition / transform.localScale.x;
    }

    public void SetLooping(bool looping)
    {
        animator.SetBool("Looping", looping);
    }

    public void SetEmotion(Emotion emotion)
    {
        animator.SetTrigger(StringEnum.GetStringValue(emotion));
    }
}
