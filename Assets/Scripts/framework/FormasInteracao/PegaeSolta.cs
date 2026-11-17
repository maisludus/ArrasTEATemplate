using Ludus.SDK.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PegaeSolta : BaseInteracao, IPointerClickHandler
{
    private RectTransform rectTransform;
   
    private bool isFollowing = false;
    private Vector3 originalPosition;
   
    void Awake()
    {
       
        rectTransform = GetComponent<RectTransform>();
        
        originalPosition = rectTransform.position;
    }
    void Start()
    {

         canvas = GameObject.Find("PainelObjeto").GetComponent<Canvas>();
        //cursorHotspot = new Vector2(CursorSolto.width / 2, CursorSolto.height / 2);
        //Cursor.SetCursor(CursorSolto, cursorHotspot, CursorMode.Auto);

     
    }

    void Update()
    {
        if (isFollowing)
        {
            Vector2 mousePos;
           
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePos
            );
            rectTransform.localPosition = mousePos;
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isFollowing = !isFollowing;

        if (isFollowing == false)
        {
            TryDrop();

        }
        
        if (Controle.configuracao.usacursor)
        {
            Texture2D cursorTexture;
            if (isFollowing)
            {
                cursorTexture = Resources.Load<Texture2D>("cursor/pegando");
            }
            else
            {
                cursorTexture = Resources.Load<Texture2D>("cursor/padrao");
            }

            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    private void TryDrop()
    {

        
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
         colouCerto = false;
        foreach (var hit in raycastResults)
        {

            try
            {
                Image imgObjeto = gameObject.GetComponent<Image>();

                string numeroSpriteObj = imgObjeto.sprite.name.Split('_')[1];
                Debug.Log(numeroSpriteObj);

                Image imgSombra = hit.gameObject.GetComponent<Image>();
                if (imgSombra != null)
                {

                    if (imgSombra.sprite.name.Contains("sombra_"))
                    {
                        string numeroSpriteSombra = imgSombra.sprite.name.Split('_')[1];

                        if (numeroSpriteObj.Equals(numeroSpriteSombra))
                        {
                            
                            if (Controle.configuracao.substituirObjetoAoParear)
                            {
                                try
                                {
                                    List<Image> imgs = gameObject.GetComponentsInChildren<Image>(true).ToList();
                                    imgSombra.sprite = imgs[1].sprite;
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError("[+LUDUS] Erro ao substituir sprite.");
                                    Debug.LogError(ex.InnerException);
                                }
                            }
                            else
                            {
                                imgSombra.sprite = imgObjeto.sprite;
                                gameObject.SetActive(false); // desativa o GameObject atual
                            }

                            Controle.configuracao.AtualizarAcerto();
                            Controle.configuracao.TocarSom('A');
                            colouCerto = true;
                        }
                        else
                        {
                            Controle.configuracao.AtualizarErro();
                            Controle.configuracao.TocarSom('E');
                            colouCerto = false;

                        }

                        break;
                    }
                }

                if (!colouCerto)
                {
                    rectTransform.position = originalPosition;
                    isFollowing = false;
                    gameObject.SetActive(false); // desativa o GameObject
                    gameObject.SetActive(true);  // ativa novamente 
                }
                
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] Erro ao acessar imagens para pareamento.");
                Debug.LogException(ex);
            }


            

            
        }

        // Se não soltou no local correto
      
    }
    

}
