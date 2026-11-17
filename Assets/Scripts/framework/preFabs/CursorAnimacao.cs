using System.Collections;
using UnityEngine;

public class CursorAnimacao : MonoBehaviour
{
    
    public float intervalo = 5f; // tempo em segundos
    public GameObject cursor;

    void Start()
    {
        
    }
void OnEnable()
    {
        StartCoroutine(TrocarEstado());
    }

    IEnumerator TrocarEstado()
    {
        while (true)
        {
            // inverte o estado atual (ativo/inativo)
            Debug.Log("ativou/desativou");
            cursor.SetActive(!cursor.activeSelf);

            // espera o intervalo definido
            yield return new WaitForSeconds(intervalo);
        }
    }
}
