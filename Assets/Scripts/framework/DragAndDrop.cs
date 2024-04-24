using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludus.SDK.Framework
{
    public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {

        private RectTransform rt;
        private CanvasGroup grupo;
        private Vector2 posicaoOriginal;
        private new AudioSource audio;
        private Canvas canvas;

        public static bool colouCerto;

        public void Awake()
        {
            rt = GetComponent<RectTransform>();
            grupo = GetComponent<CanvasGroup>();
            audio = this.GetComponentInChildren<AudioSource>();


            canvas = Controle.configuracao.painelGeral.GetComponent<Canvas>();
            colouCerto = false;
        }
        public void OnBeginDrag(PointerEventData eventData)
        {

            //posiçãoOriginal recebe a posicação onde o rt começa no canvas
            posicaoOriginal = rt.anchoredPosition;

            grupo.alpha = 0.3f;
            grupo.blocksRaycasts = false;
            colouCerto = false;
            if (audio.clip != null)
                audio.Play();
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Set da posição da imagem para a posição que o mouse está indo
            rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            grupo.alpha = 1f;
            grupo.blocksRaycasts = true;
            //Se o colou certo for diferente de true, o objeto arrastado volta para a posição inicial
            if (colouCerto == false)
            {
                Controle.configuracao.TocarSom('E');   //caso tenha na pasta de sons, tocar som de erro
                rt.anchoredPosition = posicaoOriginal;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //Debug.Log("Pointer");
        }
    }
}
