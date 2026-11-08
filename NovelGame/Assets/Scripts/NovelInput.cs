
using UnityEngine;

public class NovelInput : MonoBehaviour
{
    private NovelManager novelManager;

    private void Awake()
    {
        novelManager = GetComponent<NovelManager>();
    }

    private void Update()
    {
        if (novelManager == null)
            return;

        bool input =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.KeypadEnter);

        if (input)
        {
            novelManager.OnAdvanceInput();
        }
    }
}
