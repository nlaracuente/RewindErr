using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HintSystem : MonoBehaviour
{
    [SerializeField]
    Image iconImage;

    [SerializeField]
    Text hintText;

    [SerializeField]
    Sprite noImage;

    [SerializeField]
    Sprite spacebarIcon;

    [SerializeField]
    Sprite mouseIcon;

    [SerializeField]
    float hintRevealDelay = 3f;

    Disc disc;
    Disc Disc 
    {
        get
        {
            if (disc == null)
                disc = FindObjectOfType<Disc>();
            return disc;
        }
    }

    bool DiscIsRecallable { 
        get {
            return Player.instance.IsPoweredOn && 
                   (Disc.State == DiscState.Dispensed ||
                    Disc.State == DiscState.Charged);
        } 
    }

    bool DiscIsThrowable
    {
        get
        {
            return Player.instance.IsPoweredOn && Disc.State == DiscState.Attached;
        }
    }

    void Start()
    {
        HideHint();
        StartCoroutine(ShowRewindHintRoutine());
        StartCoroutine(ShowThrowHintRoutine());
        StartCoroutine(ShowRecallHintRoutine());
    }

    public void ShowRewindHint()
    {
        ShowHint(spacebarIcon, "Rewind");
    }

    public void ShowRecallHint()
    {
        ShowHint(mouseIcon, "Recall");
    }

    public void ShowThrowHint()
    {
        ShowHint(mouseIcon, "Throw");
    }

    void ShowHint(Sprite icon, string text)
    {
        iconImage.sprite = icon;
        hintText.text = text;
        iconImage.enabled = true;
    }

    public void HideHint()
    {
        iconImage.enabled = false;
        hintText.text = "";
    }

    IEnumerator ShowRewindHintRoutine()
    {
        while(GameManager.instance.CurrentLevel < 1)
        {
            while(Player.instance.IsPoweredOn)
                yield return new WaitForEndOfFrame();

            if (GameManager.instance.CurrentLevel > 0)
                break;

            yield return new WaitForSeconds(hintRevealDelay);

            ShowRewindHint();
            while (!Player.instance.IsPoweredOn)
                yield return new WaitForEndOfFrame();

            if (GameManager.instance.CurrentLevel > 0)
                break;

            HideHint();
        }
    }

    IEnumerator ShowThrowHintRoutine()
    {
        while (GameManager.instance.CurrentLevel < 2)
        {
            while (!DiscIsThrowable)
                yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(hintRevealDelay);

            if (GameManager.instance.CurrentLevel > 1)
                break;

            ShowThrowHint();
            while (DiscIsThrowable)
                yield return new WaitForEndOfFrame();

            if (GameManager.instance.CurrentLevel > 1)
                break;

            HideHint();
        }
    }

    IEnumerator ShowRecallHintRoutine()
    {
        while (GameManager.instance.CurrentLevel < 3)
        {
            while (!DiscIsRecallable)
                yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(hintRevealDelay);

            if (GameManager.instance.CurrentLevel > 2)
                break;

            ShowRecallHint();
            while (DiscIsRecallable)
                yield return new WaitForEndOfFrame();

            if (GameManager.instance.CurrentLevel > 2)
                break;

            HideHint();
        }
    }
}
