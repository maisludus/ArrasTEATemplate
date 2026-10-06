using UnityEngine;

[System.Serializable]
public class LevelData
{
    public bool isUnlocked;
    public int starsEarned;
    public string sceneName;
}

public class GameManager : MonoBehaviour
{
    // Padrão Singleton para garantir acesso global e único.
    public static GameManager instance;

    [Header("Regras do Jogo (Configuráveis no Inspector)")]
    [Tooltip("Se marcado, o jogo exigirá o login. Se desmarcado, mostrará um botão 'Iniciar' direto para o modo anónimo.")]
    public bool requiresLogin = false; // Por padrão, o login é opcional (desligado).

    [Tooltip("Se marcado, os níveis precisam ser desbloqueados em sequência (apenas para novos jogos).")]
    public bool useLevelLocking = true;
    
    [Header("Estado Atual do Jogo")]
    [Tooltip("Identifica o jogador atual. É definido pelo AuthenticationManager.")]
    public string currentUserID;

    // Guarda o número do nível que o jogador selecionou na tela de seleção.
    public int currentLevelToLoad;
    public string currentScene;

    // Armazena os resultados da última fase concluída para a tela de conclusão.
    [Header("Resultados da Última Fase")]
    public int lastPhaseScore;
    public int lastPhaseErrors;
    public int lastPhaseXpGained;

    // "Diário de bordo" com os dados de progressão de todos os níveis.
    [Header("Dados de Progressão dos Níveis")]
    public LevelData[] levels;

    private bool initialized = false;

    // O método Awake é chamado antes de qualquer método Start.
    void Awake()
    {
        // 1. Lógica do Singleton Anti-Clonagem
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        // 2. A DECAPITAÇÃO DO PREFAB
        // Liberta o GameManager do Canvas/Pasta pai e o joga na raiz da cena
        transform.SetParent(null);

        // 3. Imortalidade Absoluta
        DontDestroyOnLoad(this.gameObject);

        // --- LÓGICA DE DECISÃO INTELIGENTE ---
        // Verifica se o jogador já jogou antes (procurando a "marca" deixada pelo SaveLoadManager).
        if (PlayerPrefs.HasKey("HasPlayedBefore"))
        {
            // Se já jogou, NÃO inicializamos. Apenas garantimos que o array 'levels'
            // tenha o tamanho correto para o LoadGame() que será chamado após o login.
            Debug.Log("GameManager: Progresso existente detetado. À espera de login para carregar.");
            if (levels == null || levels.Length == 0)
            {
                int totalLevels = 8; // Deve corresponder ao número total de níveis
                levels = new LevelData[totalLevels];
                for (int i = 0; i < levels.Length; i++) { levels[i] = new LevelData(); }
            }
            // Garante os nomes das cenas do ArrasTEA (o LoadGame vai sobrescrever unlocked/stars)
            AplicarNomesCenas();
        }
        else
        {
            // Se for a primeira vez, INICIALIZAMOS o jogo com as regras do Inspector.
            Debug.Log("GameManager: Nenhum progresso salvo. A inicializar novo jogo...");
            InitializeLevelData();
        }
        // ------------------------------------
    }

    /// <summary>
    /// Nomes EXATOS das cenas de fase do ArrasTEA, na ordem do nível (1..8).
    /// Usado para casar o índice do GameManager com a cena que terminou
    /// (o framework Configuracao.EndPhase procura levels[i].sceneName).
    /// </summary>
    public static readonly string[] FaseScenes = new string[]
    {
        "Frutas",        // 1 - Frutas
        "UmDif1",        // 2 - EncontrarIgual
        "DifGalo",       // 3 - EncontrarDiferente
        "PintarFrutas",  // 4 - PintarFruta
        "Formas",        // 5 - Formas
        "Cores",         // 6 - Cores
        "Brinquedos",    // 7 - Brinquedos
        "TamanhoV"       // 8 - Tamanhos
    };

    /// <summary>
    /// Garante que cada nível conheça a cena do ArrasTEA correspondente.
    /// Necessário para o framework (Configuracao.EndPhase) gravar estrelas/XP
    /// no índice certo — sem isso, currentLevelIndex sempre cai em 0.
    /// </summary>
    public void AplicarNomesCenas()
    {
        if (levels == null) return;
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] == null) levels[i] = new LevelData();
            if (i < FaseScenes.Length)
                levels[i].sceneName = FaseScenes[i];
        }
    }

    /// <summary>
    /// Regra MESTRA de bloqueio: a tranca (useLevelLocking) tem prioridade sobre
    /// qualquer estado individual dos níveis. Se desligada, TODAS as fases liberam.
    /// Deve ser chamado sempre que a seleção de fases for exibida, para que o
    /// estado visual reflita a regra atual (e não um save antigo/estado manual).
    /// </summary>
    public void AplicarRegraDeBloqueio()
    {
        if (levels == null || levels.Length == 0) InitializeLevelData();
        AplicarNomesCenas();
        if (!useLevelLocking)
        {
            // TRANCAR DESLIGADA: todas as fases liberadas (estrelas preservadas)
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null) levels[i] = new LevelData();
                levels[i].isUnlocked = true;
            }
        }
        // Se useLevelLocking == true, respeita o estado carregado/inicializado
        // (fase 1 aberta por padrão; demais desbloqueiam ao ganhar 2+ estrelas).
    }

    /// <summary>
    /// Configura os dados padrão para todos os níveis na primeira vez que o jogo é executado.
    /// Respeita a regra 'useLevelLocking' definida no Inspector.
    /// </summary>
    public void InitializeLevelData()
    {

        if (levels == null || levels.Length == 0) 
        { 
            int totalLevels = 8;
            levels = new LevelData[totalLevels]; 
        }

        AplicarNomesCenas();

        for (int i = 0; i < levels.Length; i++)
        {
            if(levels[i] == null) levels[i] = new LevelData();

            if (useLevelLocking)
            {
                // Modo com bloqueio: só o nível 1 começa desbloqueado.
                levels[i].isUnlocked = (i == 0);
            }
            else
            {
                // Modo sem bloqueio: todos os níveis começam desbloqueados.
                levels[i].isUnlocked = true;
            }

            levels[i].starsEarned = 0;
        }
        
        initialized = true; 
    }
    public void StartAnonymousSession()
    {
        Debug.Log("GameManager: Iniciando sessão Anônima. Limpando dados anteriores...");
        currentUserID = "Anonimo"; // Define um ID claro para evitar sobrescrever saves de usuários logados
        
        // Forçamos o reset das fases para as regras do Inspector
        InitializeLevelData(); 
    }

    /// <summary>
    /// Limpa completamente os dados em memória (útil para botões de "Logout").
    /// </summary>
    public void ClearSessionData()
    {
        Debug.Log("GameManager: Limpando sessão atual.");
        currentUserID = "";
        initialized = false;
        
        // Zera o array de níveis
        if (levels != null)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i] = new LevelData();
            }
        }
    }
}