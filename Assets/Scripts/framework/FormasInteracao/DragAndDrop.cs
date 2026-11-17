using UnityEngine;
using UnityEngine.EventSystems;

namespace Ludus.SDK.Framework
{
    public class DragAndDrop : BaseInteracao, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Vector2 posicaoOriginal;
        private new AudioSource audio;
       
        private void Awake()
        {
            canvas = Controle.configuracao.painelGeral.GetComponent<Canvas>();
            rt = GetComponent<RectTransform>();
            grupo = GetComponent<CanvasGroup>();
            audio = this.GetComponentInChildren<AudioSource>();

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
            //verifica se tem legenda
            if(Controle.configuracao.temLegendaObjeto)
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
                    Debug.LogError("Erro ao carregar o objeto de legenda. Está nulo, inexistente ou mal definido na cena.");
                    Debug.LogError(ex.GetBaseException());
                }
                

            }
            //testa o cursor
            if (Controle.configuracao.usacursor)
            {

                Texture2D cursorTexture = Resources.Load<Texture2D>("cursor/pegando");

                if (cursorTexture != null)
                {
                    Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
                }
            }
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
            //Se o colou certo for diferente de true,
            //o objeto arrastado volta para a posição inicial
            //este colou certo vem do eventoSombra
            if (colouCerto == false)
            {
                // Aqui vou verificar se ele realmente tentou para dar a mensagem de áudio
                //Por tentar eu quero saber se ele largou dentro de uma sombra ou se ele nem conseguiu
                //chegar lá, nesse caso não foi erro e sim problema para colocar no lugar, 
                //ai não vou dar o áudio de erro
                GameObject alvo = eventData.pointerEnter;
                if (alvo != null) {
                    if (alvo.name.Contains("sombra"))
                    {
                        //AQUI SIM configura um erro, então toca o som (se tiver na pasta)
                        Controle.configuracao.AtualizarErro();
                        Controle.configuracao.TocarSom('E');   
                    }
                    
                }

                
                rt.anchoredPosition = posicaoOriginal;
            }

            //limpa o texto da legenda, caso exista
             if(this.legenda!=null && Controle.configuracao.txtLegenda!=null)
            {
                Controle.configuracao.txtLegenda.text = string.Empty;    
            }

            if (Controle.configuracao.usacursor)
            {

                Texture2D cursorTexture = Resources.Load<Texture2D>("cursor/padrao");

                if (cursorTexture != null)
                {
                    Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //Debug.Log("Pointer");
        }
    }
}
