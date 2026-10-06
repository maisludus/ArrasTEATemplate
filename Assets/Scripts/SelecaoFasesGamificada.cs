using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ludus.SDK.Framework;

/// <summary>
/// Gamificação da tela de seleção de fases (cena EscolherComImg).
/// Configura os 8 botões de fase (Button + direcionamento pelo NOME da cena),
/// desbloqueio sequencial, e popula estrelas/emblema em elementos pré-montados
/// na cena. NÃO instancia UI em runtime (evita quebra de layout).
/// </summary>
public class SelecaoFasesGamificada : MonoBehaviour
{
    [System.Serializable]
    public class FaseInfo
    {
        public string nomeBotao;    // nome do GameObject do botão na cena
        public string nomeCena;     // nome EXATO da cena a carregar (direcionamento)
        public int levelNumber;     // índice 1..N no GameManager.levels[]
    }

    [Header("As 8 fases (ordem = ordem dos botões no Panel)")]
    public FaseInfo[] fases = new FaseInfo[]
    {
        new FaseInfo(){nomeBotao="Frutas",            nomeCena="Frutas",       levelNumber=1},
        new FaseInfo(){nomeBotao="EncontrarIgual",    nomeCena="UmDif1",       levelNumber=2},
        new FaseInfo(){nomeBotao="EncontrarDiferente",nomeCena="DifGalo",      levelNumber=3},
        new FaseInfo(){nomeBotao="PintarFruta",       nomeCena="PintarFrutas", levelNumber=4},
        new FaseInfo(){nomeBotao="Formas",            nomeCena="Formas",       levelNumber=5},
        new FaseInfo(){nomeBotao="Cores",             nomeCena="Cores",        levelNumber=6},
        new FaseInfo(){nomeBotao="Brinquedos",        nomeCena="Brinquedos",   levelNumber=7},
        new FaseInfo(){nomeBotao="Tamanhos",          nomeCena="TamanhoV",     levelNumber=8},
    };

    [Header("Display de Emblema (auto-localizado por nome no Canvas)")]
    // Se o painel "EmblemaPanel" existir na cena, o runtime populará automaticamente
    // os filhos "EmblemIcon" e "EmblemText". Sem precisar atribuir no Inspector.
    private const string EMBLEM_PANEL = "EmblemaPanel";
    private const string EMBLEM_ICON_CHILD = "EmblemIcon";
    private const string EMBLEM_TEXT_CHILD = "EmblemText";

    // Nome do container de estrelas DENTRO de cada botão (opcional, pré-montado)
    private const string STARS_CONTAINER = "Stars";
    // Nome do cadeado (imagem) dentro de cada botão (opcional, pré-montado)
    private const string LOCK_CONTAINER = "Lock";
    // Nome do filho que carrega o componente Button real (estrutura original do ArrasTEA)
    private const string BUTTON_CHILD = "Fase button pequeno";

    private Sprite estrelaDourada;
    private Sprite estrelaCinza;
    private Sprite[] emblemasColoridos;

    void Awake()
    {
        var estrelas = Resources.LoadAll<Sprite>("Game/UI/estrelas_spritesheet");
        if (estrelas != null && estrelas.Length >= 2)
        {
            // Ordem do spritesheet: [0]=cinza, [1]=dourada.
            // (era invertido, o que fazia fase sem progresso mostrar dourada)
            estrelaCinza   = estrelas[0];
            estrelaDourada = estrelas[1];
        }
        emblemasColoridos = Resources.LoadAll<Sprite>("Game/Emblemas/emblemas_spritesheet");
    }

    void Start()
    {
        // Garante que a regra MESTRA de bloqueio (useLevelLocking) tenha prioridade:
        // se desligada, todas as fases liberam, independente de save/estado manual.
        if (GameManager.instance != null) GameManager.instance.AplicarRegraDeBloqueio();
        ConfigurarBotoes();
        AtualizarDisplayEmblema();
    }

    void ConfigurarBotoes()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("SelecaoFasesGamificada: GameManager não encontrado na cena.");
            return;
        }

        for (int i = 0; i < fases.Length; i++)
        {
            FaseInfo fi = fases[i];
            GameObject botaoGO = GameObject.Find(fi.nomeBotao);
            if (botaoGO == null)
            {
                Debug.LogWarning($"SelecaoFasesGamificada: botão '{fi.nomeBotao}' não encontrado.");
                continue;
            }

            // O botão clicável real no ArrasTEA é um filho chamado "Fase button pequeno"
            // (o container só tem Transform/Outline). Fallback para o próprio container
            // caso a estrutura mude. Não altera o visual.
            Transform btnFilho = botaoGO.transform.Find(BUTTON_CHILD);
            GameObject btnGO = (btnFilho != null) ? btnFilho.gameObject : botaoGO;
            Button btn = btnGO.GetComponent<Button>();
            if (btn == null) btn = btnGO.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int nivel = fi.levelNumber;
            string cena = fi.nomeCena;
            btn.onClick.AddListener(delegate { LigarFase(nivel, cena); });

            // Estado da fase lido do GameManager
            bool unlocked = (GameManager.instance.levels != null && i < GameManager.instance.levels.Length)
                            ? GameManager.instance.levels[i].isUnlocked
                            : (i == 0);
            int starsEarned = (GameManager.instance.levels != null && i < GameManager.instance.levels.Length)
                              ? GameManager.instance.levels[i].starsEarned : 0;

            // Interactable reflete o desbloqueio. NÃO mudamos a opacidade dos botões
            // (esmaecer confunde): o bloqueio é indicado pelo cadeado quando presente.
            btn.interactable = unlocked;

            // Estrelas: preenche apenas se houver container "Stars" pré-montado no botão.
            // Não cria em runtime (não quebra o layout).
            Transform starsContainer = botaoGO.transform.Find(STARS_CONTAINER);
            if (starsContainer != null && estrelaDourada != null && estrelaCinza != null)
            {
                for (int s = 0; s < starsContainer.childCount && s < 3; s++)
                {
                    Image star = starsContainer.GetChild(s).GetComponent<Image>();
                    if (star != null)
                        star.sprite = (s < starsEarned) ? estrelaDourada : estrelaCinza;
                }
            }

            // Cadeado: mostra/oculta conforme o desbloqueio (se houver GO "Lock")
            Transform lockGO = botaoGO.transform.Find(LOCK_CONTAINER);
            if (lockGO != null) lockGO.gameObject.SetActive(!unlocked);
        }
    }

    void LigarFase(int levelNumber, string nomeCena)
    {
        if (GameManager.instance != null)
            GameManager.instance.currentLevelToLoad = levelNumber;

        // Reseta a configuração da fase anterior (mesmo efeito de Genericos.CarregarCenaFase)
        Controle.configuracao = null;

        UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCena);
    }

    void AtualizarDisplayEmblema()
    {
        if (EmblemManager.instance == null || EmblemManager.instance.playerEmblem == null) return;

        var emblem = EmblemManager.instance.playerEmblem;

        // Localiza o painel de emblema no Canvas (se existir) por nome
        Transform panel = transform.Find(EMBLEM_PANEL);
        if (panel != null)
        {
            Transform iconT = panel.Find(EMBLEM_ICON_CHILD);
            if (iconT != null)
            {
                Image icon = iconT.GetComponent<Image>();
                if (icon != null && emblemasColoridos != null && emblemasColoridos.Length > 0)
                {
                    int idx = Mathf.Clamp(emblem.currentLevel - 1, 0, emblemasColoridos.Length - 1);
                    icon.sprite = emblemasColoridos[idx];
                }
            }

            Transform textT = panel.Find(EMBLEM_TEXT_CHILD);
            if (textT != null)
            {
                TextMeshProUGUI t = textT.GetComponent<TextMeshProUGUI>();
                if (t != null)
                    t.text = $"Nível {emblem.currentLevel} — {emblem.currentXP}/{emblem.xpToNextLevel} XP";
            }

            // Barra de XP (opcional): se houver um Slider chamado "EmblemXpBar" no painel, preenche.
            Transform barT = panel.Find("EmblemXpBar");
            if (barT != null)
            {
                Slider bar = barT.GetComponent<Slider>();
                if (bar != null)
                {
                    bar.maxValue = Mathf.Max(1, emblem.xpToNextLevel);
                    bar.value = emblem.currentXP;
                }
            }
        }
    }
}
