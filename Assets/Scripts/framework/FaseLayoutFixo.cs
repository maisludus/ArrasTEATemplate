
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Ludus.SDK.Framework
{ 
    public class FaseLayoutFixo : FaseLayout
    {

        private List<AudioSource> audiosSourceSombraAuxiliar;
        private List<Image> imgsSombra;
        private List<Image> imgsSombraAuxiliar;

        void Start()
        {
            //Carrega o tipo de tela que será associado a configuração da fase
            if (Controle.configuracao == null)
            {
                Controle.configuracao = new ConfiguracaoFase();
            }

            this.CarregarConfiguracao();

            #region PegarElementosDosPaineis
            this.CarregarObjetosPainel();
            this.CarregarSombrasPainel();
            #endregion


            #region CarregaAssetsPastas        
            this.CarregaAssetsPastas();
            #endregion


            List<int> indiceSelecionado =  this.PopularObjetos();
            this.PopularSombras(indiceSelecionado);
       
        }
        protected override void PopularSombras(List<int> indiceSelecionado)
        {
            try
            {
                for (int i = 0; imgsSombra.Count > i; i++)
                {
                    imgsSombra[i].sprite = spritesSombra[indiceSelecionado[i]];
                    if (usarDimensoesImagens)
                    {
                        imgsSombra[i].rectTransform.sizeDelta = new Vector2(spritesSombra[indiceSelecionado[i]].rect.width, spritesSombra[indiceSelecionado[i]].rect.height);
                    }
                    if (this.conteudoauxiliar)
                    {
                        imgsSombraAuxiliar[i].sprite = spritesAuxiliar[indiceSelecionado[i]];
                        if (usarDimensoesImagens)
                        {
                            imgsSombraAuxiliar[i].rectTransform.sizeDelta = new Vector2(spritesAuxiliar[indiceSelecionado[i]].rect.width, spritesAuxiliar[indiceSelecionado[i]].rect.height);
                        }
                        //se tem audio auxiliar, relaciona
                        if (audiosAuxiliar != null && audiosSourceSombraAuxiliar != null)
                        {
                            if (audiosAuxiliar.Count > 0)
                            {
                                audiosSourceSombraAuxiliar[i].clip = audiosAuxiliar[indiceSelecionado[0]];
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] erro ao carregar o conteúdo  da sobra");
                Debug.LogException(ex);
            }

        }



        protected override void CarregarSombrasPainel()
        {
            if (this.conteudoauxiliar)
            {
                imgsSombra = new List<Image>();
                imgsSombraAuxiliar = new List<Image>();
                List<Image> imagens = sombra.GetComponentsInChildren<Image>().ToList<Image>();
                imagens = this.RetirarImagem(sombra, imagens);

                for (int i = 0; i < imagens.Count; i = i + 2)
                {
                    imgsSombra.Add(imagens[i]);
                    imgsSombraAuxiliar.Add(imagens[i + 1]);
                }

                try
                {
                    audiosSourceSombraAuxiliar = sombra.GetComponentsInChildren<AudioSource>().ToList<AudioSource>();
                }
                catch (System.Exception)
                {

                    Debug.LogWarning("[+LUDUS] áudio SOurce não encontrado no prefab sombra");
                    audiosSourceSombraAuxiliar = null;
                }
            }
            else
            {
                imgsSombra = sombra.GetComponentsInChildren<Image>().ToList<Image>();
                imgsSombra = this.RetirarImagem(sombra, imgsSombra);
            }

        }





            // Update is called once per frame
        void Update()
        {
        
        }
   
    }
}
