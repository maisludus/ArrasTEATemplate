using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton_Manager : MonoBehaviour
{
    [Tooltip("Nome exato da cena que este botão deve carregar.")]
    public string fase = "FaseDeJogo";
    
    [Tooltip("Número da fase que será enviado ao GameManager.")]
    public int levelNumber = 1;

    public void SelectLevel() 
    { 
        if (GameManager.instance != null) 
        { 
            GameManager.instance.currentLevelToLoad = levelNumber; 
        } 

        // Reseta a configuração da fase anterior do framework (mesmo efeito do
        // Genericos.CarregarCenaFase) para a nova fase abrir limpa.
        Ludus.SDK.Framework.Controle.configuracao = null;
        
        SceneManager.LoadScene(fase); 
    }
}