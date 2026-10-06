
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Ludus.SDK.Framework
{ 
    public abstract class Configuracao
    {
        public List<Nivel> niveis;
        public String cenaFinal;
        public int objetoLargura, objetoAltura, sombraLargura, sombraAltura;
        public bool conteudoauxiliar; //se a sombra que será exibida tem cotnteúdo auxiliar, influencia no prefab que será carregado da sombra
        public string sombraauxiliar;
        public int sombraAuxiliarLargura = 100, sombraAuxiliarAltura = 100;
        public bool substituirObjetoAoParear;

        public Button botaoTroca;
        protected AudioClip somOk;
        protected AudioClip somErro;
        protected AudioSource audioSomJogo;
        protected AudioSource audioMusica;
    
        protected int acertosFase;
        public List<int> objetosJaExibidos; //elementos que já foram exibidos na cena e que n devem reaparecer no mesmo nível

        //controle da fase atualmente apresentada
        protected int nivelAtual;
        protected int repeticaoAtual;

        protected bool inicial = true;

        public GameObject painelObjeto, painelSombra, painelGeral;
        private GameObject preFabObjeto, preFabSombra;

        //gamificação
        public int acertos;
        public int erros;


        public virtual void CarregarConfiguracao(GameObject novoPainelGeral)
        {
            if (inicial)
            {
                if ( niveis.Count.Equals(0))
                {
                    Debug.LogError("[+LUDUS] Não é possível iniciar a fase pois nenhum nível foi definido. Verifique as Configurações no Script do Painel e crie os níveis para a fase");
                    return;

                }
                acertos = 0;
                erros = 0;
                nivelAtual = 0;
                repeticaoAtual = 0;
                inicial = false;
                objetosJaExibidos = new List<int>();
            }

            try
            {
                botaoTroca.onClick.AddListener(VerificaFase);
                botaoTroca.gameObject.SetActive(false);
            }
            catch (Exception ex)
            {

                Debug.LogError("[+LUDUS] Erro ao instanciar o botão. Verifique se o mesmo foi corretamente associado no script do Painel.");
                Debug.LogException(ex);
                return;
            }

            painelGeral = novoPainelGeral;

            //verifica se existe os arquivos na pasta estimulosons, caso exista inicializa as mensagens de sons de erro e acerto quando o usuário cola certo e/ou errado um item
            //gerencia os sons do jogo, primeiro os audios
            List<AudioSource> audiosS = painelGeral.GetComponentsInChildren<AudioSource>().ToList<AudioSource>();
            List<Canvas> paineis = painelGeral.GetComponentsInChildren<Canvas>().ToList<Canvas>();

            foreach (Canvas painel in paineis)
            {
                if (painel.name.Equals("PainelObjeto"))
                {
                    painelObjeto = painel.gameObject;
                }
                else if (painel.name.Equals("PainelSombra"))
                {
                    painelSombra = painel.gameObject;
                }
            }

            try
            {
                audioSomJogo = audiosS[0];
                audioMusica = audiosS[1];
            }
            catch (Exception ex)
            {

                Debug.LogError("[+LUDUS] O Canvas Painel precisa ter componentes de áudio conforme o preFab. O primeiro relacionado aos sons da fase e o segundo a música, quando desejado, mas os componentes precisam estar presentes");
                Debug.LogException(ex);
                return;
            }

            AudioClip audioClip = null;
            try
            {
                audioClip = Resources.Load<AudioClip>("sonsGerais/acerto");
                if (audioClip != null)
                {

                    somOk = audioClip;
                }

                audioClip = Resources.Load<AudioClip>("sonsGerais/erro");
                if (audioClip != null)
                {

                    somErro = audioClip;
                }
            }
            catch (Exception)
            {

                Debug.LogWarning("[+LUDUS] Erro ao buscar os sons padrão de acerto e erro");
            }


            try
            {
                if (painelObjeto == null || painelSombra == null)
                {
                    Debug.LogError("[+LUDUS] Erro ao atribuir os paineis. Verifique se os nomes dos paineis sao PainelObjeto e PainelSombra, e se estes estão dentro do componente Painel");
                    return;
                }

            }
            catch (Exception ex)
            {

                Debug.LogError("[+LUDUS] Erro ao atribuir os paineis. Verifique se os nomes dos paineis sao PainelObjeto e PainelSombra");
                Debug.LogException(ex);
                return;
            }

            //Busca os Prefabs
            this.CriarPrefabs();

            this.CarregarPaineis();

        }
        protected abstract void CarregarPaineis();
        public virtual void CarregarCena()
        {
            //verifica se ja passou todos niveis
            Nivel atual = niveis[nivelAtual];
            repeticaoAtual++;
            //se chegou ao maximo de repetições incrementa o nível atual
            if (repeticaoAtual == atual.repeticoes)
            {

                nivelAtual++;
                objetosJaExibidos.Clear(); //limpa os objetos que já apareceram

                if (nivelAtual < niveis.Count)
                {
                    repeticaoAtual = 0;
                }
                else
                {
                    //caso contrario manda pra cena final
                    //se usa o módulo de gamificação guarda essa informação para atualização posterior de estrelas
                    if (GameManager.instance != null)
                    {
                        this.EndPhase();
                    }
                    
                    SceneManager.LoadScene(cenaFinal);
                    return;
                }

            }
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);

        }
        
        public virtual void AtualizarAcerto()
        {

            botaoTroca.gameObject.SetActive(true);

        }

        public virtual void ZerarExibidos()
        {
            objetosJaExibidos.Clear();
        }

        public virtual bool VerificaOcorrenciaIndiceNoNivel(int indice)
        {
            bool verificaOcorrencia;

            //se terá mais repetições que objetos possíveis, limpa os exibidos para
            //voltar a aparecer aleatoriamente

            verificaOcorrencia = objetosJaExibidos.Contains(indice);
            if (!verificaOcorrencia)
            {
                objetosJaExibidos.Add(indice);
            }

            return verificaOcorrencia;
        }

        protected virtual void AdicionarPrefabObjeto(GameObject panel)
        {
            GameObject novo = GameObject.Instantiate(preFabObjeto) as GameObject;
            novo.transform.SetParent(panel.transform, false);
        }

        protected virtual void AdicionarPrefabSombra(GameObject panel)
        {
            GameObject novo = GameObject.Instantiate(preFabSombra) as GameObject;
            novo.transform.SetParent(panel.transform, false);
        }

        protected virtual void CriarPrefabs() 
        {
            GameObject meuPF = Resources.Load<GameObject>("preFabs/objeto");
            preFabObjeto = GameObject.Instantiate(meuPF) as GameObject;
            RectTransform rt = preFabObjeto.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(objetoLargura, objetoAltura);


            rt = new RectTransform();

            if (this.conteudoauxiliar)
            {
                meuPF = Resources.Load<GameObject>("preFabs/" + this.sombraauxiliar);
                preFabSombra = GameObject.Instantiate(meuPF) as GameObject;
                rt = preFabSombra.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(sombraLargura, sombraAltura);

                preFabSombra.GetComponentsInChildren<RectTransform>().ElementAt(1).sizeDelta 
                    = new Vector2(sombraAuxiliarLargura, sombraAuxiliarAltura);
            }
            else
            {
                meuPF = Resources.Load<GameObject>("preFabs/sombra");
                preFabSombra = GameObject.Instantiate(meuPF) as GameObject;
                rt = preFabSombra.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(sombraLargura, sombraAltura);
            }

           
        }
       

        protected virtual void VerificaFase()
        {
            CarregarCena();
        }

        public virtual void TocarSom(char sompratocar)
        {
            switch (sompratocar)
            {
                case 'A':
                    if (somOk != null)
                    {
                        audioSomJogo.clip = somOk;
                        audioSomJogo.Play();
                        acertos++;
                        Debug.Log(acertos);
                    }
                    break;

                case 'E':
                    if (somErro != null)
                    {
                        audioSomJogo.clip = somErro;
                        audioSomJogo.Play();
                        erros++;
                    }
                    break;



                default:
                    break;

            }
        }

        //Gamificação
        private void EndPhase()
        {
            // 1. Pega o índice do nível atual (Nível 1 = índice 0).
 
            int currentLevelIndex=0;
            for (int i = 0; i < GameManager.instance.levels.Length; i++)
            {
                String atual = GameManager.instance.levels[i].sceneName;
                if (atual.Equals(SceneManager.GetActiveScene().name))
                {
                    currentLevelIndex = i;
                }
            }
            
            // 2. Calcula os resultados da partida.
            int score = 1000 - (erros * 200);
            if (score < 0) score = 0;
            int xpGained = 150 - (erros * 25);
            if (xpGained < 0) xpGained = 0;
            int starsEarned = CalculateStars(erros);

            // 3. ATUALIZA O EMBLEMA NO EMBLEMManager.
            // Isso deve acontecer ANTES de salvar o jogo.
            if (EmblemManager.instance != null)
            {
                EmblemManager.instance.playerEmblem.AddXP(xpGained);
            }

            // 4. Atualiza os dados de progressão no "diário de bordo" do GameManager.
            // Só salva as estrelas se o jogador conseguiu uma pontuação melhor que a anterior.
            if (starsEarned > GameManager.instance.levels[currentLevelIndex].starsEarned)
            {
                GameManager.instance.levels[currentLevelIndex].starsEarned = starsEarned;
            }

            // Desbloqueia o próximo nível apenas se o jogador conseguiu 2 ou mais estrelas.
            if (starsEarned >= 2)
            {
                int nextLevelIndex = currentLevelIndex + 1;
                // Garante que não tentamos desbloquear um nível que não existe.
                if (nextLevelIndex < GameManager.instance.levels.Length)
                {
                    GameManager.instance.levels[nextLevelIndex].isUnlocked = true;
                }
            }

            // 5. Prepara o "pacote de dados" para a tela de conclusão.
            GameManager.instance.lastPhaseScore = score;
            GameManager.instance.lastPhaseErrors = erros;
            GameManager.instance.lastPhaseXpGained = xpGained;

            // 6. SALVA O PROGRESSO NO DISCO. (Agora com o emblema e níveis atualizados).
            SaveLoadManager.SaveGame();

           
        }

        private int CalculateStars(int errorCount)
        {
            if (errorCount == 0) return 3;       // Perfeito (0 erros)
            if (errorCount <= 2) return 2; // Bom (1 ou 2 erros)
            if (errorCount <= 4) return 1; // Razoável (3 ou 4 erros)
            return 0;                      // Falha (5 ou mais erros)
        }

        //Gamificação Fim
    }
}