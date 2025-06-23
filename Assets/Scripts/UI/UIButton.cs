using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour, IPointerEnterHandler
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISound(AudioManager.Instance.clickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISound(AudioManager.Instance.hoverSound);
        }
    }
}