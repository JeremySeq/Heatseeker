using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;
    public string Username = "Anonymous";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}