using UnityEngine;

public class MouseClickTest : MonoBehaviour
{

    private void OnMouseDown()
    {
        Debug.Log("鼠标点击了对象: " + gameObject.name);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
