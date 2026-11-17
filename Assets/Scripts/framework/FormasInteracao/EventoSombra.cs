using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Ludus.SDK.Framework
{
    public class EventoSombra : MonoBehaviour, IDropHandler
    {
        public GameObject continueButton;
        public GameObject animationButton;
        private AudioSource audioSource;
        //legenda
        public string legenda;

        private void Start()
        {
            // continueButton.SetActive(false);
            //animationButton.SetActive(false);
            if (Controle.configuracao.conteudoauxiliar)
            {
                audioSource = gameObject.GetComponentInChildren<AudioSource>();
                Button btn = gameObject.GetComponentInChildren<Button>();

                btn.onClick.AddListener(EventoCliqueAuxiliar);
            }
            AjustarCollider();

        }

         void AjustarCollider()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 tamanho = rectTransform.rect.size;
        // Define o tamanho do collider com base nas variáveis largura/altura
        boxCollider.size = tamanho = rectTransform.rect.size;
        boxCollider.offset = rectTransform.rect.center;
    }

        public void EventoCliqueAuxiliar()
        {
            audioSource.Play();
            if (Controle.configuracao.temLegendaAuxiliar)
            {
                try
                {
                    if (this.legenda != null && Controle.configuracao.txtLegenda != null)
                    {
                        Controle.configuracao.txtLegenda.text = this.legenda;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[+LUDUS - EventoCliqueAuxiliar] Erro ao carregar a legenda do conteúdo auxiliar");
                    Debug.LogException(ex);
                }
            }
        }


        public void OnDrop(PointerEventData eventData)
        {
            //Se eu não estiver arrastando
            if (eventData.pointerDrag != null)
            {
                //Compara o game tag do objeto que eu to arrastando com o objeto parado
                //aqui tem q testar o final da imagem e e da sombra pra ver se é igual
                Image imgObjeto, imgSombra;
                try
                {
                    imgObjeto = eventData.pointerDrag.gameObject.GetComponent<Image>();
                    imgSombra = gameObject.GetComponent<Image>();
                    


                }
                catch (System.Exception ex)
                {

                    Debug.LogError("[+LUDUS] Não possível acessar as imagens do objeto pareado e/ou de sua sombra");
                    Debug.LogException(ex);
                    return;
                }
                Debug.Log(imgObjeto.sprite.ToString().Split('_')[1]);

                if (imgObjeto.sprite.ToString().Split('_')[1].Equals((imgSombra.sprite.ToString().Split('_')[1])))
                {
                    if (Controle.configuracao.substituirObjetoAoParear)
                    {
                        List<Image> imgs = eventData.pointerDrag.gameObject.GetComponentsInChildren<Image>(true).ToList<Image>();
                        imgSombra.sprite = imgs[1].sprite;
                    }
                    else
                    {
                        imgSombra.sprite = imgObjeto.sprite;
                    }


                    eventData.pointerDrag.transform.localScale = new Vector3(0f, 0f, 0f);
                    DragAndDrop.colouCerto = true;
                    Controle.configuracao.AtualizarAcerto();
                    Controle.configuracao.TocarSom('A');   //caso tenha na pasta de sons, tocar som de acerto


                }
            }
        }
    }

}
