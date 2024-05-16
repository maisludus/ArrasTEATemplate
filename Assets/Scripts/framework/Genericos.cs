using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ludus.SDK.Framework
{
    public class Genericos : MonoBehaviour
    {
        //Simplesmente carrega a próxima cena
        //utilzar em transições simples de botão
        public void CarregarCena(string nomeCena)
        {
            SceneManager.LoadScene(nomeCena);
        }


        //Utilizar quando for chamar uma cena que use os Scripts do
        //Template. Ele limpa a configuração anterior da fase
        public void CarregarCenaFase(string nomeCena)
        {
            Controle.configuracao = null;
            this.CarregarCena(nomeCena);
        }

        public void FecharApp()
        {
            Application.Quit();
        }
    }


}

