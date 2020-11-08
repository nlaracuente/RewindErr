using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class RewindTimer : MonoBehaviour
{
    [SerializeField]
    GameObject timerGO;

    [SerializeField]
    Image timerImage;

    Player player;

    IEnumerator routine;
    public bool IsRewinding { get { return routine != null; } }

    private void Start()
    {
        timerGO = timerGO != null ? timerGO : transform.GetChild(0)?.gameObject;
        if (timerGO == null)
            Debug.LogError($"{name} is missing the timer game object");
        else
            timerGO.SetActive(false);

        timerImage = timerImage != null ? timerImage : GetComponent<Image>();
        if (timerImage == null)
            Debug.LogError($"{name} is missing the timer image");

        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        timerGO.SetActive(true);
        timerImage.fillAmount = player.CurrentPower;
    }

    public void TriggerTimer(float duration, bool fillUp = true)
    {
        if(routine == null)
        {
            routine = TimerRoutine(duration, fillUp);
            StartCoroutine(routine);
        }
    }

    IEnumerator TimerRoutine(float duration, bool fillUp = true)
    {
        var target = fillUp ? 1f : 0f;        
        timerImage.fillAmount = fillUp ? 0f : 1f;
        timerGO.SetActive(true);

        while (!Mathf.Approximately(timerImage.fillAmount, target))
        {
            yield return new WaitForEndOfFrame();

            if(fillUp)
                timerImage.fillAmount += 1.0f / duration * Time.deltaTime;
            else
                timerImage.fillAmount -= 1.0f / duration * Time.deltaTime;
        }

        // Wait one more frame to better see timer is done
        yield return new WaitForEndOfFrame();
        timerGO.SetActive(false);

        routine = null;
    }
}
