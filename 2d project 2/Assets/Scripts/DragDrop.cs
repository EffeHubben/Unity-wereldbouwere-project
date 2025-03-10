using UnityEngine;

public class DragDrop : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    public GameObject prefabToInstantiate; // De prefab die je wilt instantieren

    void OnMouseDown()
    {
        // Als je op een sprite in het zijpaneel klikt
        if (prefabToInstantiate != null)
        {
            // Instantieer de prefab en maak het de huidige sprite
            GameObject newSprite = Instantiate(prefabToInstantiate, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
            newSprite.transform.position = new Vector3(newSprite.transform.position.x, newSprite.transform.position.y, 0); // Z-as op 0 houden
            newSprite.tag = "Instantiated"; // Geef de sprite de tag "Instantiated"
            GetComponent<SpriteRenderer>().sprite = newSprite.GetComponent<SpriteRenderer>().sprite;
            prefabToInstantiate = null; // Reset de prefabToInstantiate

            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        else
        {
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            newPosition.z = 0; // Z-as op 0 houden
            transform.position = newPosition;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (prefabToInstantiate == null)
        {
            GameObject newSprite = Instantiate(prefabToInstantiate, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
            newSprite.tag = "Instantiated"; // Geef de sprite de tag "Instantiated"
            GetComponent<SpriteRenderer>().sprite = newSprite.GetComponent<SpriteRenderer>().sprite;
            prefabToInstantiate = null; // Reset de prefabToInstantiate
        }
    }
}