using UnityEngine;

public class TerritoryButton : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;

    public Color hoverColor = Color.yellow;
    public Color clickColor = Color.red;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color; // Guardamos el color original
    }

    void OnMouseEnter()
    {
        sr.color = hoverColor; // Cambia color cuando el mouse entra
    }

    void OnMouseExit()
    {
        sr.color = originalColor; // Vuelve al color original
    }

    void OnMouseDown()
    {
        sr.color = clickColor; // Cambia de color al hacer clic
        Debug.Log("Clic en " + gameObject.name);
    }
}
