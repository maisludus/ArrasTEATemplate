
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ludus.SDK.Framework
{
    public class FaseLayoutFixoUmItem : FaseLayout
    {

        private AudioSource audioSourceSombraAuxiliar;
        private Image imgSombra;
        private Image imgAuxiliar;
       

        void Start()
        {
            if (Controle.configuracao == null)
            {
                Controle.configuracao = new ConfiguracaoFaseUmItem();
            }

            this.CarregarConfiguracao();


            #region PegarElementosDosPaineis
            this.CarregarObjetosPainel();
            this.CarregarSombrasPainel();
            #endregion

            #region CarregaAssetsPastas        
            this.CarregaAssetsPastas();
            #endregion


            #region Popular        
            List<int> indiceSelecionado = this.PopularObjetos();

            this.PopularSombras(indiceSelecionado);
            #endregion

        }
        protected override void PopularSombras(List<int> indiceSelecionado)
        {

            imgSombra.sprite = spritesSombra[indiceSelecionado[0]];
            if (usarDimensoesImagens)
            {
                imgSombra.rectTransform.sizeDelta = new Vector2(spritesSombra[indiceSelecionado[0]].rect.width, spritesSombra[indiceSelecionado[0]].rect.height);
            }
            try
            {
                if (conteudoauxiliar)
                {
                    imgAuxiliar.sprite = spritesAuxiliar[indiceSelecionado[0]];
                    if (usarDimensoesImagens)
                    {
                        imgAuxiliar.rectTransform.sizeDelta = new Vector2(spritesAuxiliar[0].rect.width, spritesAuxiliar[0].rect.height);
                    }
                    //se tem audio auxiliar, relaciona
                    if (audiosAuxiliar != null && audioSourceSombraAuxiliar != null)
                    {
                        if (audiosAuxiliar.Count > 0)
                        {
                            audioSourceSombraAuxiliar.clip = audiosAuxiliar[indiceSelecionado[0]];
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] erro ao carregar o conteúdo adicional da sobra");
                Debug.LogException(ex);

            }

        }


        // Update is called once per frame
        void Update()
        {

        }



        protected override void CarregarSombrasPainel()
        {

            //verifica se tem conteúdo auxiliar pra sombra, se sim busca as duas imagens
            if (this.conteudoauxiliar)
            {
                List<Image> imagens = sombra.GetComponentsInChildren<Image>().ToList<Image>();
                imagens = this.RetirarImagem(sombra, imagens);

                imgSombra = imagens[0];
                imgAuxiliar = imagens[1];

                try
                {
                    audioSourceSombraAuxiliar = sombra.GetComponentInChildren<AudioSource>();
                }
                catch (System.Exception)
                {

                    Debug.LogWarning("[+LUDUS] áudio Source não encontrado no prefab sombra");
                    audioSourceSombraAuxiliar = null;
                }
            }
            else
            {
                List<Image> imagens = sombra.GetComponentsInChildren<Image>().ToList<Image>();
                imagens = this.RetirarImagem(sombra, imagens);
                imgSombra = imagens[0];

            }

        }

    }
}