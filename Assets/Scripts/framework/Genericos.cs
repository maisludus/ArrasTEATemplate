using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ludus.SDK.Framework
{
    public class Genericos : MonoBehaviour
    {

        public void CarregarCena(string nomeCena)
        {
            SceneManager.LoadScene(nomeCena);
        }


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

