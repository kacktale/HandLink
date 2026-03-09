using UnityEngine;

public class InputAxis : MonoBehaviour 
{
    protected static bool isPc = true;
    public bool gameStarted = false;
    protected Touch touch;
    protected Touch lastTouch;
    protected Vector3 mousePosition;
    protected Vector3 lastmousePos;
    protected Vector2 distanseValue;
    // Update is called once per frame
    public virtual void Update()
    {
        if (!isPc)
        {
            if (Input.touchCount <= 0)
            {
                gameStarted = false;
                distanseValue = Vector2.zero;
                return;
            }
            lastTouch = touch;
            touch = Input.GetTouch(0);
            distanseValue = lastTouch.position - touch.position;
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                gameStarted = false;
                distanseValue = Vector2.zero;
                return;
            }
            lastmousePos = mousePosition;
            gameStarted = true;
            mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            distanseValue = lastmousePos - mousePosition;
        }
    }
}
