using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public static DebugText instance;
    public Text text;

    public static void SetText(string text)
    {
        instance.text.text = text;
    }

    private void Start()
    {
        instance = this;
    }
}
