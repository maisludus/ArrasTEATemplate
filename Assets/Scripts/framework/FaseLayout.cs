using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace Ludus.SDK.Framework
{

    public abstract class FaseLayout : MonoBehaviour
    {

        protected GameObject objeto;
        protected GameObject sombra;
        public int objetoLargura = 100, objetoAltura = 100, sombraLargura = 100, sombraAltura = 100;
        protected GameObject painel;
        protected List<Image> imgsObjeto;
        protected List<Image> imgsObjetoPareado;
        protected List<AudioSource> audiosSourceObjetos;
        public int sombraAuxiliarLargura = 100, sombraAuxiliarAltura = 100;
        protected List<Sprite> spritesObjeto;
        protected List<Sprite> spritesObjetoPareado;
        protected List<Sprite> spritesSombra;
        protected List<AudioClip> audiosObjeto;
        protected List<Sprite> spritesAuxiliar;
        protected List<AudioClip> audiosAuxiliar;
        public Button botaoTrocaCena;
        public string pasta;
        public string cenaFinal;
        public List<Nivel> niveis;
        public bool conteudoauxiliar;
        public bool substituirObjetoAoParear;
        public string sombraauxiliar = "sombraComAuxiliar";

        public bool monitorarCena;
        void Start()
        {



        }
        protected virtual void CarregarConfiguracao()
        {
            #region CarregarConfiguracao
            //Carrega os painéis
            try
            {
                painel = GameObject.Find(this.gameObject.name.ToString());
                objeto = GameObject.Find("PainelObjeto");
                sombra = GameObject.Find("PainelSombra");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] Erro ao instanciar as variáveis, verifique se os painéis estão dispostos de acordo com o preFab");
                Debug.LogException(ex);
                return;
            }

            //Se habilitado, adiciona o monitoramento da cena atráves do script Monitor
            if (this.monitorarCena)
            {
                this.gameObject.AddComponent<Monitor>();
            }

            //associa os valores passados visualmente no painél pra classe estática de Controle
            try
            {

                Controle.configuracao.botaoTroca = this.botaoTrocaCena;

                if (this.niveis == null || this.cenaFinal == null)
                {
                    Debug.LogError("[+LUDUS] Erro ao carregar a configuração da fase. Verifique se os níveis e a cena final foram corretamente atribuídas no inspector do painel ");
                    return;
                }

                Controle.configuracao.niveis = this.niveis;
                Controle.configuracao.cenaFinal = this.cenaFinal;
                Controle.configuracao.conteudoauxiliar = this.conteudoauxiliar;
                Controle.configuracao.sombraauxiliar = this.sombraauxiliar; 
                Controle.configuracao.substituirObjetoAoParear = this.substituirObjetoAoParear;

                //Larguras e Alturas
                Controle.configuracao.objetoAltura = this.objetoAltura;
                Controle.configuracao.objetoLargura = this.objetoLargura;
                Controle.configuracao.sombraAltura = this.sombraAltura; 
                Controle.configuracao.sombraLargura = this.sombraLargura;   
                Controle.configuracao.sombraAuxiliarAltura = this.sombraAuxiliarAltura;
                Controle.configuracao.sombraAuxiliarLargura = this.sombraAuxiliarLargura;


                //carrega as configurações da fase
                Controle.configuracao.CarregarConfiguracao(painel);


            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] Erro ao carregar a fase, verifique as configurações.");
                Debug.LogException(ex);
                return;
            }
            #endregion
        }

        protected virtual void CarregaAssetsPastas()
        {
            //verifica se a variável pasta foi definida
            if (string.IsNullOrEmpty(pasta))
            {

                Debug.LogError("[+LUDUS] Variável pasta não defindida, verifique as variáveis do script e adicione a pasta corresponde aos sprites. Dica: procure na pasta Resources/fases e busque O NOME DA FASE DESEJADA");
                return;
            }

            try
            {
                //Sprite[] Carrega Sprites do objeto e da sombra
                spritesObjeto = Resources.LoadAll<Sprite>("fases/" + pasta + "/objeto").ToList<Sprite>();
                spritesSombra = Resources.LoadAll<Sprite>("fases/" + pasta + "/sombra").ToList<Sprite>();
            }
            catch (System.Exception ex)
            {

                Debug.LogError("[+LUDUS] Sprites não encontrados. Verifique se a pasta fases/" + pasta + "/objeto e fases/" + pasta + "/sombra existem dentro da pasta Resources, e se os os sprites estão corretamente divididos (multiple)");
                Debug.LogException(ex);
                return;
            }

            if (Controle.configuracao.substituirObjetoAoParear)
            {
                try
                {
                    spritesObjetoPareado = Resources.LoadAll<Sprite>("fases/" + pasta + "/objetoPareado").ToList<Sprite>();
                }
                catch (System.Exception ex)
                {

                    Debug.LogError("[+LUDUS] Sprites não encontrados. Verifique se o arquivo objetoPareado encontra-se na pasta fases/" + pasta);
                    Debug.LogException(ex);
                }
            }


            //carregar audios do objeto
            try
            {
                audiosObjeto = Resources.LoadAll<AudioClip>("fases/" + pasta + "/objetoSons").ToList<AudioClip>();
            }
            catch (System.Exception)
            {

                audiosObjeto = null;
            }

            //se tem conteúdo auxiliar busca as imagens desse conteúdo e também, se for o caso, os sons desse conteúdo        
            if (this.conteudoauxiliar)
            {
                try
                {
                    spritesAuxiliar = Resources.LoadAll<Sprite>("fases/" + pasta + "/auxiliar").ToList<Sprite>();
                }
                catch (System.Exception ex)
                {

                    Debug.LogError("[+LUDUS] Sprites não encontrados. Verifique se a pasta fases/" + pasta + "/auxiliar  está com os sprites corretamente colocados");
                    Debug.LogException(ex);
                }
                //busca os sons auxiliares das sombras - caso não tenha atribui null a variável audiosAuxiliar
                try
                {
                    audiosAuxiliar = Resources.LoadAll<AudioClip>("fases/" + pasta + "/auxiliarSons").ToList<AudioClip>();
                }
                catch (System.Exception)
                {

                    Debug.LogWarning("[+LUDUS] Áudios auxiliares não encontrados. Caso estejam na pasta verifique se a pasta fases/" + pasta + "/auxiliarsons existe dentro da pasta Resources");
                    audiosAuxiliar = null;
                }

            }

            if (spritesObjeto.IsUnityNull())
            {

                Debug.LogError("[+LUDUS] spritesObjeto não definido");
                return;
            }
            if (spritesSombra.IsUnityNull())
            {

                Debug.LogError("[+LUDUS] spritesSombra não definido");
                return;
            }



        }

        protected virtual void CarregarObjetosPainel()
        {
            try
            {
                //carrega o(s) gameObjetos Image do painel OBJETO
                //são duas imagens, caso a idéia seja trocar o elemento ao parear.

                if (this.substituirObjetoAoParear)
                {
                    //se tem q manter a informação da imagem que vai ser colada ao parear, busca os inativos do prefab pra poder trocar depois
                    List<Image> imgs = objeto.GetComponentsInChildren<Image>(true).ToList<Image>();
                    imgs = this.RetirarImagem(objeto, imgs);   
                    imgsObjeto = imgs.Where(s => s.gameObject.activeInHierarchy).ToList<Image>();
                    imgsObjetoPareado = imgs.Where(s => !s.gameObject.activeInHierarchy).ToList<Image>();

                }
                else
                {
                    imgsObjeto = objeto.GetComponentsInChildren<Image>().ToList<Image>();
                    imgsObjeto = this.RetirarImagem(objeto, imgsObjeto);

                }
                //lista de elementos de áudio do painel OBJETO
                audiosSourceObjetos = objeto.GetComponentsInChildren<AudioSource>().ToList<AudioSource>();


            }
            catch (System.Exception ex)
            {

                Debug.LogError("[+LUDUS] Erro ao buscar as imagens dentro do painel objeto', verifique se as imagens estão corretamente dispostas na hierarquia de elemetos");
                Debug.LogException(ex);
                return;
            }


        }


        protected abstract void CarregarSombrasPainel();

        protected abstract void PopularSombras(List<int> indiceSelecionado);
        protected virtual List<int> PopularObjetos()
        {
            //buscar os sprites e sombras, abrindo de forma embaralhada seu conteúdo na Cena
            List<int> indiceSelecionado = new List<int>();


            for (int i = 0; imgsObjeto.Count > i; i++)
            {

                int selecionado = this.IndiceNovo(indiceSelecionado);
                imgsObjeto[i].sprite = spritesObjeto[selecionado];
                if (substituirObjetoAoParear)
                {
                    imgsObjetoPareado[i].sprite = spritesObjetoPareado[selecionado];
                }
                if (audiosObjeto != null)
                {
                    try
                    {
                        audiosSourceObjetos[i].GetComponent<AudioSource>().clip = audiosObjeto[selecionado];
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("[+LUDUS] erro ao carregar o áudio correspondente ao sprite. Verifique os arquivos na pasta(quantidade e nome, sugerece que os audios sejam dispostos com o _número correto. Por exemplo audio_0, audio_1,...)");
                        Debug.LogException(ex);
                    }
                }

            }

            

            return this.EmbaralharLista(indiceSelecionado);

        }


        //método para retirar da lista de filhos do Painel o compontente que
        //pertence ao próprio objeto
        protected virtual List<Image> RetirarImagem(GameObject painel, List<Image> imgs)
        {
            if (painel.GetComponent<Image>() != null)
            {
                imgs.Remove(painel.GetComponent<Image>());
            }

            return imgs;
        
        }

        protected virtual int IndiceNovo(List<int> indiceSelecionado)
        {
            int novoindice;
            novoindice = Random.Range(0, spritesObjeto.Count);

            int tentativas = 1;
            //verifica se o índice já apareceu naquele nível e se as tentativas de achar o indice novo acabaram
            //a variavel tentativas é um controle para que o jogo não tranque caso não tenha sprites disponíveis para exibir aquela fase, 
            //nesse caso vai aceitar imagem repetida
            while (Controle.configuracao.VerificaOcorrenciaIndiceNoNivel(novoindice))
            {
                novoindice = Random.Range(0, spritesObjeto.Count);
                tentativas++;
                //se o número de itens disponíveis para a cena for igual a quantidade de objetos já exibidos Zera os exibidos
                if (Controle.configuracao.objetosJaExibidos.Count == spritesObjeto.Count)
                {
                    Controle.configuracao.ZerarExibidos();
                }
            }

            indiceSelecionado.Add(novoindice);
            return novoindice;

        }

        protected virtual List<T>  EmbaralharLista<T>(List<T> lista)
        {
            var rnd = new System.Random();

            var query =
                from i in lista
                let r = rnd.Next()
                orderby r
                select i;

           return query.ToList();


        }


    }

}
