
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Ludus.SDK.Framework.Configuracao;



namespace Ludus.SDK.Framework
{

    public abstract class FaseLayoutDivide : MonoBehaviour
     {

        [Header("Configurações da Fase")]
        public FormasdeInteracao formaDeInteracao;
        public Button botaoTrocaCena;
        //ou é a relação de fases ou é a pasta que vale
        public RelacaoFases fasesDisponiveis; 
        private string pasta; //troquei por RelacaoFases fasesDispoviveis -- mas deixei para casos de erro e apps antigos
        public string cenaFinal;
        public List<Nivel> niveis;
        [Header("Configurações - Auxiliar")]
        public bool conteudoauxiliar;
        public bool substituirObjetoAoParear;
        public Auxiliares sombraauxiliar = Auxiliares.Direita;

        [Header("Dimensões das Imagens")]
        [Tooltip("Usar largura e altura padrão")]
        public bool usarDimensoesImagens = false;
        public int objetoLargura = 100, objetoAltura = 100, sombraLargura = 100, sombraAltura = 100;
        public int sombraAuxiliarLargura = 100, sombraAuxiliarAltura = 100;
        [Header("Apenas para Interação por Clique")]
        public bool trocarImagemSombraAoClicar = true;
        [Header("Monitoramento")]
        public bool monitorarCena;
        [Header("Divisão do Sprite")]
        public bool dividirSprite=false;
        public int itensPorParte;
      

        protected GameObject objeto;
        protected GameObject sombra;
        protected GameObject painel;
        protected List<Image> imgsObjeto;
        protected List<Image> imgsObjetoPareado;
        protected List<AudioSource> audiosSourceObjetos;

        protected List<Sprite> spritesObjeto;
        protected List<Sprite> spritesObjetoPareado;
        protected List<Sprite> spritesSombra;
        protected List<AudioClip> audiosObjeto;
        protected List<Sprite> spritesAuxiliar;
        protected List<AudioClip> audiosAuxiliar;



        //Legenda
        protected TextMeshProUGUI txtLegenda;
        string[] textos;
        protected string[] textosAuxiliar;

        protected bool temLegendaObjeto, temLegendaAuxiliar;

        private int controleDivisao;
      


        public void Start()
        {
            //Se for setado o cursor, busca ele seguindo o nome padrão do cursor
            try
            {
                Texture2D cursorTexture = Resources.Load<Texture2D>("cursor/padrao");

                if (cursorTexture != null)
                {
                    Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
                    Controle.configuracao.usacursor = true;
                }
                else
                {
                    Debug.LogWarning("[+LUDUS] Cursor não encontrado em 'Resources/cursor/padrao' .\nCertifique-se de que existe um arquivo chamado padrao na pasta cursor(esse é o nome do arquivo do cursor que irá substituir o cursor padrão).");
                    Controle.configuracao.usacursor = false;
                }
            }
            catch
            {
                Debug.LogWarning("[+LUDUS] Cursor não encontrado em 'Resources/cursor/padrao' .\nCertifique-se de que existe um arquivo chamado padrao na pasta cursor(esse é o nome do arquivo do cursor que irá substituir o cursor padrão).");
                Controle.configuracao.usacursor = false;
            }
            //verifica se está usando a forma de interação dinâmica para a fase. 
            //se sim, troca a forma de interação padrão
            if (PlayerPrefs.HasKey("formainteracao"))
            {
                int escolha = PlayerPrefs.GetInt("formainteracao");
                this.formaDeInteracao = (FormasdeInteracao)escolha;
            }
            else 
            {
                this.formaDeInteracao = FormasdeInteracao.DragAndDrop;
            }


        }
        protected virtual void CarregarConfiguracao()
        {
            #region CarregarConfiguracao
            //Carrega os pain�is
            try
            {
                painel = GameObject.Find(this.gameObject.name.ToString());
                objeto = GameObject.Find("PainelObjeto");
                sombra = GameObject.Find("PainelSombra");
                //busca o Text de legenda (opcional ter na tela)
                try
                {
                    GameObject goTexto = GameObject.Find("Legenda");
                    if (goTexto != null)
                    {
                        txtLegenda = goTexto.GetComponent<TextMeshProUGUI>();

                    }
                }
                catch (System.Exception)
                {

                    throw;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] Erro ao instanciar as vari�veis, verifique se os pain�is est�o dispostos de acordo com o preFab");
                Debug.LogException(ex);
                return;
            }

            //Se habilitado, adiciona o monitoramento da cena atr�ves do script Monitor
            if (this.monitorarCena)
            {
                this.gameObject.AddComponent<Monitor>();
            }
            //se tiver divisão de array, inicializa a variável que controla o bloco que será sorteado
            if (this.dividirSprite)
            {
                this.controleDivisao = 0;
            }

            //associa os valores passados visualmente no pain�l pra classe est�tica de Controle
            try
            {

                Controle.configuracao.botaoTroca = this.botaoTrocaCena;

                if (this.niveis == null || this.cenaFinal == null)
                {
                    Debug.LogError("[+LUDUS] Erro ao carregar a configura��o da fase. Verifique se os n�veis e a cena final foram corretamente atribu�das no inspector do painel ");
                    return;
                }
                //aqui é necessário uma verificação de divisão
                //o número de elementos em cena não pode ser MAIOR que maior que a divisão do array
                if (this.dividirSprite)
                {
                    foreach (Nivel item in this.niveis)
                    {
                        //se a quantidade de elementos for maior que a divisão do sprite, ajusta
                        //ao máximo possível e coloca um aviso no console
                        if (item.quantidadeElementos > this.itensPorParte) 
                        {
                            item.quantidadeElementos = this.itensPorParte;
                            Debug.LogWarning("[+LUDUS] Quantidade de elementos em Cena não pode ser maior que a divisão do sprite. Qualtidade atualizada automaticamente para o máximo permitido");
                        }
                    }
                }
                Controle.configuracao.formaDeInteracao = this.formaDeInteracao;
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
                Controle.configuracao.txtLegenda = this.txtLegenda;
                //Clique
                Controle.configuracao.trocarImagemSombraAoClicar = this.trocarImagemSombraAoClicar;


                //carrega as configura��es da fase
                Controle.configuracao.CarregarConfiguracao(painel);


            }
            catch (System.Exception ex)
            {
                Debug.LogError("[+LUDUS] Erro ao carregar a fase, verifique as configura��es.");
                Debug.LogException(ex);
                return;
            }
            #endregion
        }

        protected virtual void CarregaAssetsPastas()
        {
            
            pasta = fasesDisponiveis.ToString();

            try
            {

                spritesObjeto = this.CarregarAssets<Sprite>("objeto");
                spritesSombra = this.CarregarAssets<Sprite>("sombra");


            }
            catch (System.Exception ex)
            {

                Debug.LogError("[+LUDUS] Sprites n�o encontrados. Verifique se a pasta fases/" + pasta + "/objeto e fases/" + pasta + "/sombra existem dentro da pasta Resources, e se os os sprites est�o corretamente divididos (multiple)");
                Debug.LogException(ex);
                return;
            }
            #region legendaObjeto
            //busca o conte�do da legenda do objeto, caso exista
            textos = this.CarregarAssets("texto", out this.temLegendaObjeto); 
            #endregion

            if (Controle.configuracao.substituirObjetoAoParear)
            {
                spritesObjetoPareado = this.CarregarAssets<Sprite>("objetoPareado");
            }


            //carregar audios do objeto
            try
            {
                audiosObjeto = this.CarregarAssets<AudioClip>("objetoSons");
                audiosObjeto = this.ordenarAudio(audiosObjeto);
            }
            catch (System.Exception)
            {

                audiosObjeto = new List<AudioClip>();
            }

            //se tem conte�do auxiliar busca as imagens desse conte�do e tamb�m, se for o caso, os sons desse conte�do        
            if (this.conteudoauxiliar)
            {
                spritesAuxiliar = this.CarregarAssets<Sprite>("auxiliar");

                //busca os sons auxiliares das sombras - caso n�o tenha atribui null a vari�vel audiosAuxiliar
                try
                {
                    audiosAuxiliar = this.CarregarAssets<AudioClip>("auxiliarSons");
                    audiosAuxiliar = this.ordenarAudio(audiosAuxiliar);
                }
                catch (System.Exception)
                {

                    Debug.LogWarning("[+LUDUS] �udios auxiliares n�o encontrados. Caso estejam na pasta verifique se a pasta fases/" + pasta + "/auxiliarsons existe dentro da pasta Resources");
                    audiosAuxiliar = null;
                }
                #region legendaauxiliar
                this.textosAuxiliar = this.CarregarAssets("textoAuxiliar", out this.temLegendaAuxiliar);
                #endregion

            }

            if (spritesObjeto.IsUnityNull())
            {

                Debug.LogError("[+LUDUS] spritesObjeto n�o definido");
                return;
            }
            if (spritesSombra.IsUnityNull())
            {

                Debug.LogError("[+LUDUS] spritesSombra n�o definido");
                return;
            }

            //atualiza o booleano de legenda, para facilitar posteriormente a verifica��o
            Controle.configuracao.temLegendaAuxiliar = this.temLegendaAuxiliar;
            Controle.configuracao.temLegendaObjeto = this.temLegendaObjeto;



        }

        protected virtual void CarregarObjetosPainel()
        {
            try
            {
                //carrega o(s) gameObjetos Image do painel OBJETO
                //s�o duas imagens, caso a id�ia seja trocar o elemento ao parear.

                if (this.substituirObjetoAoParear)
                {
                    //se tem q manter a informa��o da imagem que vai ser colada ao parear, busca os inativos do prefab pra poder trocar depois
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
                //lista de elementos de �udio do painel OBJETO
                audiosSourceObjetos = objeto.GetComponentsInChildren<AudioSource>().ToList<AudioSource>();


            }
            catch (System.Exception ex)
            {

                Debug.LogError("[+LUDUS] Erro ao buscar as imagens dentro do painel objeto', verifique se as imagens est�o corretamente dispostas na hierarquia de elemetos");
                Debug.LogException(ex);
                return;
            }


        }


        protected abstract void CarregarSombrasPainel();

        protected abstract void PopularSombras(List<int> indiceSelecionado);
        protected virtual List<int> PopularObjetos()
        {
            //buscar os sprites e sombras, abrindo de forma embaralhada seu conte�do na Cena
            List<int> indiceSelecionado = new List<int>();


            for (int i = 0; imgsObjeto.Count > i; i++)
            {

                int selecionado = this.IndiceNovo(indiceSelecionado);
                imgsObjeto[i].sprite = spritesObjeto[selecionado];
                if (usarDimensoesImagens)
                {
                    imgsObjeto[i].rectTransform.sizeDelta = new Vector2(spritesObjeto[selecionado].rect.width, spritesObjeto[selecionado].rect.height);
                }
                //se tem legenda, habilita
                if (this.temLegendaObjeto)
                {
                    try
                    {
                        imgsObjeto[i].GetComponent<BaseInteracao>().legenda = this.textos[selecionado];

                    }
                    catch (System.Exception)
                    {
                        Debug.LogError("[+LUDUS] Erro ao atribuir a legenda no �ndice: " + i.ToString()
                            + " . Verifice se o arquivo de texto est� corretamente formatado e com o n�mero de palavras condizente com as imagens correspondentes(se tem 10 imagens s�o necess�rios 10 textos).");
                    }
                }
                if (substituirObjetoAoParear)
                {
                    imgsObjetoPareado[i].sprite = spritesObjetoPareado[selecionado];
                }
                if (audiosObjeto != null)
                {
                    if (audiosObjeto.Count > 0)
                    {
                        try
                        {
                            audiosSourceObjetos[i].GetComponent<AudioSource>().clip = audiosObjeto[selecionado];
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError("[+LUDUS] erro ao carregar o �udio correspondente ao sprite. Verifique os arquivos na pasta(quantidade e nome, sugerece que os audios sejam dispostos com o _n�mero correto. Por exemplo audio_0, audio_1,...)");
                            Debug.LogException(ex);
                        }
                    }
                }

            }
            



            return this.EmbaralharLista(indiceSelecionado);

        }


        //m�todo para retirar da lista de filhos do Painel o compontente que
        //pertence ao pr�prio objeto
        protected virtual List<Image> RetirarImagem(GameObject painel, List<Image> imgs)
        {
            if (painel.GetComponent<Image>() != null)
            {
                imgs.Remove(painel.GetComponent<Image>());
            }

            return imgs;

        }
        private int GerarIndice(int tamanhoArray)
        {
            int gerado;
            if(this.dividirSprite)
            {
                if(tamanhoArray%this.itensPorParte==0)
                {
                    int inicioRange;
                    //vamos ver se é sequencial ou aleatório o sorteio do bloco que será exibido
                    int numeroBlocos;
                    //número de blocos é a divisao do tamanho do array por quantas partes quero dividir
                    numeroBlocos = tamanhoArray / this.itensPorParte; 
                    
                    //Usa como referencia a repeticao atual, porém, se 
                    //tiver mais fases que bloco tem que controlar isso
                    //nessa lógica estou verificando se o início do range passou o tamanho do array,
                    //se passou tenho q fazer ele voltar ao ciclo 
                    this.controleDivisao = Controle.configuracao.repeticaoAtual;
                    while (this.controleDivisao >= numeroBlocos)
                    {
                        this.controleDivisao -= numeroBlocos;
                    }

                    inicioRange = this.itensPorParte *this.controleDivisao;
                    gerado = Random.Range(inicioRange, inicioRange+this.itensPorParte);

                }
                else
                {
                    Debug.LogWarning("[+LUDUS] Não foi possível dividir o sprite. Número de elementos do sprite não pode ser dividido pelo valor sugerido na variável itensporpartes");
                    gerado = Random.Range(0, tamanhoArray);
                }
               
            }
            else
            {
                gerado = Random.Range(0, tamanhoArray);
            }
            
           return gerado;
        }
        protected virtual int IndiceNovo(List<int> indiceSelecionado)
        {
            int novoindice;
           
            novoindice = this.GerarIndice(spritesObjeto.Count);
            
            int tentativas = 1;
            //verifica se o �ndice j� apareceu naquele n�vel e se as tentativas de achar o indice novo acabaram
            //a variavel tentativas � um controle para que o jogo n�o tranque caso n�o tenha sprites dispon�veis para exibir aquela fase, 
            //nesse caso vai aceitar imagem repetida
            while (Controle.configuracao.VerificaOcorrenciaIndiceNoNivel(novoindice))
            {
                novoindice = this.GerarIndice(spritesObjeto.Count);
                tentativas++;
                //se o n�mero de itens dispon�veis para a cena for igual a quantidade de objetos j� exibidos Zera os exibidos
                if (Controle.configuracao.objetosJaExibidos.Count == spritesObjeto.Count)
                {
                    Controle.configuracao.ZerarExibidos();
                }
            }
            
            indiceSelecionado.Add(novoindice);
            return novoindice;

        }

        protected virtual List<T> EmbaralharLista<T>(List<T> lista)
        {
            var rnd = new System.Random();

            var query =
                from i in lista
                let r = rnd.Next()
                orderby r
                select i;

            return query.ToList();


        }

        private List<AudioClip> ordenarAudio(List<AudioClip> audiosObjeto)
        {
            AudioClip aux;
            string vetori, vetorj;


            for (int i = 0; i < audiosObjeto.Count; i++)
            {
                for (int j = 0; j < audiosObjeto.Count; j++)
                {
                    vetori = audiosObjeto[i].name.ToString().Split('_')[1];
                    vetorj = audiosObjeto[j].name.ToString().Split('_')[1];


                    if (System.Int32.Parse(vetori) < System.Int32.Parse(vetorj))
                    {
                        //aqui acontece a troca, do maior cara  vaia para a direita e o menor para a esquerda
                        aux = audiosObjeto[i];
                        audiosObjeto[i] = audiosObjeto[j];
                        audiosObjeto[j] = aux;
                    }
                }
            }

            return audiosObjeto;


        }


        //Carrega os Assets cua lista � lida diretamente da
        //pasta, como Sprite e AudioClip
        public List<T> CarregarAssets<T>(string asset) where T : Object
        {
            try
            {
                string caminho = $"fases/{pasta}/{asset}";
                return Resources.LoadAll<T>(caminho).ToList();
            }
            catch (System.Exception ex)
            {

                Debug.LogError($"[+LUDUS] Erro ao carregar o asset na pasta {pasta}/{asset}.");
                Debug.LogError(ex.StackTrace);
                return null;
            }
            
        }

        public string[] CarregarAssets(string asset, out bool temLegenda) 
        {
            string[] conteudo;
            try
            {
                string caminho = $"fases/{pasta}/{asset}";
                TextAsset texto = Resources.Load<TextAsset>(caminho);
                if (texto != null)
                {
                    temLegenda = true;
                    conteudo = texto.text.Split(";");
                }
                else
                {
                    temLegenda = false;
                    conteudo = null;
                    Debug.LogWarning("[+LUDUS] Nenhuma Legenda para o Objeto");
                }
            }
            catch (System.Exception ex)
            {

                Debug.LogError($"[+LUDUS] Erro ao carregar o asset na pasta {pasta}/{asset}.");
                Debug.LogError(ex.StackTrace);
                conteudo = null;
                temLegenda = false;
            }

            return conteudo;
        }

    }

   

}
