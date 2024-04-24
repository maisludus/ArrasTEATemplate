using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // 1



namespace Ludus.SDK.Framework
{

    public class TrocarCenaImagem : MonoBehaviour, IPointerClickHandler
    {

        public string cena;

        public void OnPointerClick(PointerEventData eventData)
        {
            SceneManager.LoadScene(cena);
        }

    }

}