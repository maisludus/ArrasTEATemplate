using Ludus.SDK.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Clique : BaseInteracao
{
    
    private Vector2 posicaoOriginal;
    private new AudioSource audio;
    private GameObject sombras;
    
    
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        grupo = GetComponent<CanvasGroup>();
        audio = this.GetComponentInChildren<AudioSource>();

        colouCerto = false;
        canvas = Controle.configuracao.painelGeral.GetComponent<Canvas>();
    }

    void Start()
    {
        sombras = GameObject.Find("PainelSombra");
        AjustarCollider();
    }

    void AjustarCollider()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider2D>();
        }
        //ajusta o colisor ao tamanho da imagem que foi utilizada
        Image img = GetComponent<Image>();

                // Pega largura e altura da textura original (em pixels)
        if (img.sprite != null) { 
            float largura = img.sprite.rect.width;
            float altura = img.sprite.rect.height;

            // Converter pixels para Unity Units usando pixelsPerUnit
        
            Vector2 tamanho2 = new Vector2(largura, altura);

            // Ajusta o collider
            box.size = tamanho2;
            box.offset = Vector2.zero;
        }

    }

    void OnMouseDown()
    {
        
        if (audio.clip != null)
            audio.Play();
        //verifica se tem legenda
        if (Controle.configuracao.temLegendaObjeto)
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

            Image imgObjeto, imgSombra;
            try
            {
                imgObjeto = gameObject.GetComponent<Image>();
                List<Image> imagens = sombras.GetComponentsInChildren<Image>().ToList<Image>();
                string numeroSpriteObj = imgObjeto.sprite.ToString().Split('_')[1];
                string numeroSpriteSombra;
                for (int i = 0; i < imagens.Count; i++)
                {
                    string nomeSprite = imagens[i].sprite.ToString();
                    if (nomeSprite.Contains("sombra_"))
                    { 
                        numeroSpriteSombra = nomeSprite.Split('_')[1];
                        imgSombra = imagens[i];
                        //se o sprite clicado tem correspondente na sombra
                        if (numeroSpriteObj.Equals(numeroSpriteSombra))
                        {
                            if (Controle.configuracao.substituirObjetoAoParear)
                            {
                                try
                                {
                                    List<Image> imgs = gameObject.GetComponentsInChildren<Image>(true).ToList<Image>();
                                    imgSombra.sprite = imgs[1].sprite;
                                }
                                catch (System.Exception ex)
                                {

                                    Debug.LogError("[+LUDUS] Não achou a imagem para substituir ao parear");
                                    Debug.LogError(ex.InnerException);
                                }

                            }
                            else
                            {
                                if (Controle.configuracao.trocarImagemSombraAoClicar)
                                {
                                    imgSombra.sprite = imgObjeto.sprite;
                                }
                            }


                            Controle.configuracao.AtualizarAcerto();
                            Controle.configuracao.TocarSom('A');   //caso tenha na pasta de sons, tocar som de acerto
                        }
                        else 
                        {
                            Controle.configuracao.AtualizarErro();
                            StartCoroutine(ExecutarDepoisDoAudio());
                    }
                        break;
                        }
                }

            if (Controle.configuracao.usacursor)
            {

                Texture2D cursorTexture = Resources.Load<Texture2D>("cursor/clique");

                if (cursorTexture != null)
                {
                    Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
                }

                StartCoroutine(voltarCursor());
            }


        }
        catch (System.Exception ex)
        {

            Debug.LogError("[+LUDUS] Não possível acessar as imagens do objeto pareado e/ou de sua sombra");
            Debug.LogException(ex);
            return;
        }


        Debug.Log("Objeto clicado: " + gameObject.name);
        // Aqui você pode fazer o que quiser: pegar, mover, mudar cor etc.
    }

    private IEnumerator voltarCursor()
    {
       

        yield return new WaitForSeconds(1f); // espera 2 segundos

        Texture2D cursorTexture = Resources.Load<Texture2D>("cursor/padrao");

        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }

    private IEnumerator ExecutarDepoisDoAudio()
    {
        // Espera enquanto o áudio estiver tocando
        while (audio.isPlaying)
        {
            yield return null;
        }
        //executa a ação
        Controle.configuracao.TocarSom('E');
    }





    private void Update()
    {
            
    }

 
}
