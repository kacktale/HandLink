using UnityEngine;
using UnityEngine.UIElements;

public class Judge : MonoBehaviour
{
    private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        anim.SetTrigger("Judge");
        Invoke("DisableThis",1.2f);
    }

    void DisableThis() => gameObject.SetActive(false);
}
