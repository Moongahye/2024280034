using UnityEngine;

public class book : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
using UnityEngine;

public class Book : MonoBehaviour
{
    public string bookTitle = "책";

    private void OnMouseDown()
    {
        Debug.Log(bookTitle + "을 클릭했습니다!");
    }
}
