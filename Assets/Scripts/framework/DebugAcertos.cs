using Ludus.SDK.Framework;
using UnityEngine;

public class DebugAcertos : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()

    {
           //Aqui tem que ir pro GameManager

        if (Controle.configuracao != null) { 
            Debug.Log("Acertos:" + Controle.configuracao.acertos.ToString() + " Erros:" + Controle.configuracao.erros.ToString());
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
