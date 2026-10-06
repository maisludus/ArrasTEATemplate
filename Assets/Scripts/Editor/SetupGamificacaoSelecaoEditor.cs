using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Script de EDIÇÃO (roda no Editor, via menu). Monta uma única vez a estrutura
/// visual de gamificação na cena EscolherComImg: estrelas por fase (pequenas,
/// 3 por botão) e o painel de emblema/XP no topo. Depois pode ser removido.
///
/// Uso: menu "ArrasTEA > Montar Seleção Gamificada" com a cena EscolherComImg aberta.
/// </summary>
public class SetupGamificacaoSelecaoEditor : EditorWindow
{
    private const string StarsContainer = "Stars";
    private const string LockContainer = "Lock";

    [MenuItem("ArrasTEA/Montar Seleção Gamificada")]
    public static void Montar()
    {
        // Normaliza alguns caracteres minúsculos para os nomes exatos dos botões
        string[] botoes = { "Frutas", "EncontrarIgual", "EncontrarDiferente", "PintarFruta",
                            "Formas", "Cores", "Brinquedos", "Tamanhos" };
        string[] cenas = { "Frutas", "UmDif1", "DifGalo", "PintarFrutas",
                           "Formas", "Cores", "Brinquedos", "TamanhoV" };

        var estrelas = Resources.LoadAll<Sprite>("Game/UI/estrelas_spritesheet");
        Sprite dourada = estrelas != null && estrelas.Length > 0 ? estrelas[0] : null;
        Sprite cinza   = estrelas != null && estrelas.Length > 1 ? estrelas[1] : null;
        // Debug por arquivo (o log do console via MCP é pouco confiável)
        string d0 = dourada != null ? dourada.name + " " + dourada.rect.width + "x" + dourada.rect.height : "NULL";
        string d1 = cinza != null ? cinza.name + " " + cinza.rect.width + "x" + cinza.rect.height : "NULL";
        string nEstrelas = estrelas != null ? estrelas.Length.ToString() : "0";
        string deb = string.Concat("LoadAll count=", nEstrelas, "\nd0=", d0, "\nd1=", d1, "\n");
        try {
            System.IO.File.WriteAllText("/tmp/arrastea_debug.txt", deb);
        } catch (System.Exception e) { UnityEngine.Debug.Log("arrastea debug write fail: " + e.Message); }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas não encontrado na cena."); return; }

        int criados = 0;

        for (int i = 0; i < botoes.Length; i++)
        {
            GameObject botao = GameObject.Find(botoes[i]);
            if (botao == null) { Debug.LogWarning("Botão não encontrado: " + botoes[i]); continue; }

            // Container de estrelas (se ainda não existir) - âncora no centro-baixo interno do botão
            Transform stars = botao.transform.Find(StarsContainer);
            if (stars == null)
            {
                // Cria com RectTransform já no construtor (evita conflito com o Transform nativo)
                GameObject starsGO = new GameObject(StarsContainer, typeof(RectTransform));
                starsGO.transform.SetParent(botao.transform, false);
                stars = starsGO.transform;
                var rt = starsGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -22f); // dentro do botão, abaixo do centro
                rt.sizeDelta = new Vector2(78f, 22f);
            }

            // Limpa estrelas antigas (copia a lista para iterar com segurança)
            Transform[] antigos = stars.GetComponentsInChildren<Transform>(true);
            foreach (Transform c in antigos)
            {
                if (c != stars) Object.DestroyImmediate(c.gameObject);
            }

            // Cria 3 imagens de estrela, imunes ao layout (LayoutElement.ignoreLayout) e
            // com tamanho garantido via SetNativeSize para não distorcer.
            for (int s = 0; s < 3; s++)
            {
                GameObject starGO = new GameObject("Star" + (s + 1), typeof(RectTransform));
                starGO.transform.SetParent(stars, false);
                Image img = starGO.AddComponent<Image>();
                img.raycastTarget = false;
                // A primeira normalmente dourada, resto cinza (preview; runtime corrige pela pontuação)
                img.sprite = (s == 0) ? dourada : cinza;
                img.preserveAspect = true;
                img.SetNativeSize(); // força o tamanho real da sprite (pequena)

                // Tamanho pequeno garantido (clamp)
                var srt = starGO.GetComponent<RectTransform>();
                srt.sizeDelta = new Vector2(22f, 22f);
                srt.localScale = Vector3.one;

                // LayoutElement impedir que o layout do pai reorganize/estique as estrelas
                var le = starGO.AddComponent<LayoutElement>();
                le.ignoreLayout = true;

                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.anchorMin = new Vector2(0.5f, 0.5f);
                srt.anchorMax = new Vector2(0.5f, 0.5f);
                srt.anchoredPosition = new Vector2((s - 1) * 26f, 0f);
            }

            criados++;
        }

        // Painel de emblema no topo do Canvas (uma linha: ícone + texto)
        CriarPainelEmblema(canvas, estrelas);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveScene(canvas.gameObject.scene);
        Debug.Log($"ArrasTEA: montou gamificação em {criados} botões e painel de emblema.");
    }

    static void CriarPainelEmblema(Canvas canvas, Sprite[] estrelas)
    {
        // Evita duplicar
        if (canvas.transform.Find("EmblemaPanel") != null) return;

        var emblemas = Resources.LoadAll<Sprite>("Game/Emblemas/emblemas_spritesheet");
        Sprite emblemIcon = emblemas != null && emblemas.Length > 0 ? emblemas[0] : null;

        // Container do painel (canto superior direito do Canvas)
        GameObject panel = new GameObject("EmblemaPanel");
        panel.transform.SetParent(canvas.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(1f, 1f);
        prt.anchorMax = new Vector2(1f, 1f);
        prt.pivot = new Vector2(1f, 1f);
        prt.anchoredPosition = new Vector2(-120f, -60f);
        prt.sizeDelta = new Vector2(220f, 70f);

        // Ícone do emblema
        GameObject icon = new GameObject("EmblemIcon");
        icon.transform.SetParent(panel.transform, false);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.sprite = emblemIcon;
        iconImg.raycastTarget = false;
        var irt = icon.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0f, 0.5f);
        irt.anchorMax = new Vector2(0f, 0.5f);
        irt.pivot = new Vector2(0f, 0.5f);
        irt.anchoredPosition = new Vector2(0f, 0f);
        irt.sizeDelta = new Vector2(60f, 60f);

        // Texto de emblema/XP
        GameObject txt = new GameObject("EmblemText");
        txt.transform.SetParent(panel.transform, false);
        TextMeshProUGUI t = txt.AddComponent<TextMeshProUGUI>();
        t.text = "Nível 1 — 0/100 XP";
        t.fontSize = 20;
        t.color = new Color(0.25f, 0.2f, 0.4f);
        t.alignment = TextAlignmentOptions.Left;
        t.raycastTarget = false;
        var trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.5f);
        trt.anchorMax = new Vector2(0f, 0.5f);
        trt.pivot = new Vector2(0f, 0.5f);
        trt.anchoredPosition = new Vector2(70f, 0f);
        trt.sizeDelta = new Vector2(150f, 30f);
    }

    [MenuItem("ArrasTEA/Instanciar Tela de Seleção (LevelSelect_Manager)")]
    public static void InstanciarTelaSelecao()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LevelSelect_Manager.prefab");
        if (prefab == null) { Debug.LogError("Prefab LevelSelect_Manager não encontrado no caminho estrito."); return; }

        // Remove instâncias anteriores do mesmo prefab na cena
        var existentes = Object.FindObjectsOfType<Transform>();
        foreach (var t in existentes)
        {
            if (t.name == "LevelSelect_Manager")
                Object.DestroyImmediate(t.gameObject);
        }

        GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instancia == null) { Debug.LogError("Falha ao instanciar o prefab LevelSelect_Manager."); return; }

        if (!UnityEngine.EventSystems.EventSystem.current && Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        EditorSceneManager.MarkSceneDirty(instancia.scene);
        EditorSceneManager.SaveScene(instancia.scene);
        Debug.Log("ArrasTEA: instanciei a tela de seleção (LevelSelect_Manager) na cena.\n");
    }

    // 8 tipos do ArrasTEA (nome do cartao, nome da cena a carregar, nome do icone em Assets/Imagens)
    static readonly string[] TIPOS = {
        "Frutas","Frutas","escolherFrutas",
        "EncontrarIgual","UmDif1","escolherIgual",
        "EncontrarDiferente","DifGalo","escolherDiferente",
        "PintarFruta","PintarFrutas","escolherCoresFrutas",
        "Formas","Formas","escolherFormas",
        "Cores","Cores","escolherCores",
        "Brinquedos","Brinquedos","escolherBrinquedos",
        "Tamanhos","TamanhoV","escolherTamanho"
    };

    [MenuItem("ArrasTEA/Configurar Grade de Fases no Prefab")]
    public static void ConfigurarGradeFases()
    {
        string prefabPath = "Assets/Prefabs/LevelSelect_Manager.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null) { Debug.LogError("ArrasTEA: Prefab LevelSelect_Manager não encontrado."); return; }
        try
        {
            var levelGrid = root.transform.Find("Canvas/ContentPanel/LevelGrid");
            if (levelGrid == null) { Debug.LogError("ArrasTEA: LevelGrid não encontrado no prefab."); return; }

            // Modelo base: primeiro cartão existente no grid (LevelButton_Template)
            // Configuramos os cartões existentes/duplicados. Não criamos prefab aninhado.
            System.Collections.Generic.List<GameObject> cartoes = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in levelGrid) if (child != levelGrid) cartoes.Add(child.gameObject);

            int desejados = 8;
            // Garantir 8 cartões: duplica o primeiro modelo até chegar a 8
            GameObject modelo = cartoes.Count > 0 ? cartoes[0] : null;
            while (cartoes.Count < desejados && modelo != null)
            {
                GameObject copia = Object.Instantiate(modelo, levelGrid);
                cartoes.Add(copia);
            }
            // Se sobrou, remove os excedentes (mantém só os 8 primeiros)
            while (cartoes.Count > desejados)
            {
                GameObject exc = cartoes[cartoes.Count - 1];
                Object.DestroyImmediate(exc);
                cartoes.RemoveAt(cartoes.Count - 1);
            }

            // Configura cada um dos até 8 cartões
            for (int i = 0; i < cartoes.Count && i < 8; i++)
            {
                GameObject go = cartoes[i];
                string nome = TIPOS[i*3];
                string cena = TIPOS[i*3+1];
                string icone = TIPOS[i*3+2];

                go.name = nome;
                Image iconImgCriado = null;

                // Texto (TextMeshProUGUI em qualquer nível)
                TMPro.TextMeshProUGUI[] textso = go.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in textso) { t.text = nome; }

                // Ícone do tipo: criação de uma Image dedicada "Icon" (pequena, acima do texto),
                // em vez de sobrescrever a primeira Image do botão (que costuma ser o fundo).
                var iconeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Imagens/" + icone + ".png");
                Transform iconT = go.transform.Find("Icon");
                if (iconT == null)
                {
                    GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGO.transform.SetParent(go.transform, false);
                    var irt = iconGO.GetComponent<RectTransform>();
                    irt.anchorMin = new Vector2(0.5f, 0.5f);
                    irt.anchorMax = new Vector2(0.5f, 0.5f);
                    irt.pivot = new Vector2(0.5f, 0.5f);
                    irt.anchoredPosition = new Vector2(0f, 10f);
                    irt.sizeDelta = new Vector2(80f, 80f);
                    iconImgCriado = iconGO.GetComponent<Image>();
                    iconImgCriado.raycastTarget = false;
                    iconT = iconGO.transform;
                }
                else
                {
                    iconImgCriado = iconT.GetComponent<Image>();
                    if (iconImgCriado == null) iconImgCriado = iconT.gameObject.AddComponent<Image>();
                }
                if (iconImgCriado != null && iconeSprite != null)
                {
                    iconImgCriado.sprite = iconeSprite;
                    iconImgCriado.preserveAspect = true;
                }
                // Garante que o ícone fique por cima do fundo/UnlockedContent (último irmão)
                if (iconT != null) iconT.SetAsLastSibling();

                // Limpa sprites DUPLICADOS: zera o sprite de qualquer outra Image do cartão
                // (fundos/placeholders) que não seja o Icon nem as estrelas nem um LockIcon
                Image[] todas = go.GetComponentsInChildren<Image>(true);
                foreach (var im in todas)
                {
                    bool isIcon = (im.transform == iconT);
                    bool isStar = im.transform.parent != null && im.transform.parent.name.Contains("Stars");
                    bool isLock = im.gameObject.name == "LockIcon" || im.transform.name == "LockIcon";
                    if (!isIcon && !isStar && !isLock)
                    {
                        if (im.sprite != null && im.sprite.name.StartsWith("escolher"))
                            im.sprite = null; // zera o escolher* duplicado de fundos/placeholders antigos
                    }
                }

                // Estrelas: setar a sprite (cinza default) nas 3 estrelas do StarsContainer
                var spawn = EstrelaCinzaSprite();
                if (spawn != null)
                {
                    Transform stars = go.transform.Find("UnlockedContent/StarsContainer");
                    if (stars == null) stars = go.transform.Find("StarsContainer");
                    if (stars != null)
                    {
                        for (int sIdx = 0; sIdx < stars.childCount && sIdx < 3; sIdx++)
                        {
                            var starImg = stars.GetChild(sIdx).GetComponent<Image>();
                            if (starImg != null) starImg.sprite = spawn;
                        }
                    }
                }

                // LevelButton_Manager: fase + levelNumber
                var lbtn = go.GetComponent<LevelButton_Manager>();
                if (lbtn == null) lbtn = go.AddComponent<LevelButton_Manager>();
                lbtn.fase = cena;
                lbtn.levelNumber = i + 1;
            }

            // Remove componentes com script missing (senão o SaveAsPrefabAsset recusa salvar)
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            foreach (var c in cartoes) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(c);

            bool okSave;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out okSave);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ArrasTEA: grade configurada. cartões no grid=" + cartoes.Count + " saveOk=" + okSave);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // Carrega o sub-sprite "estrela_cinza" da spritesheet de estrelas (via fileID conhecido)
    static Sprite EstrelaCinzaSprite()
    {
        // estrelas_spritesheet.png: estrela_cinza internalID = -8520370035612903112
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Game/UI/estrelas_spritesheet.png");
        foreach (var o in sprites)
        {
            var s = o as Sprite;
            if (s != null && s.name.Contains("cinza")) return s;
        }
        return null;
    }

    // Ajusta o layout da grade de fases: 2 linhas x 4 colunas, estrelas no topo,
    // ícone no centro, nome em baixo (na frente) — regra visual pedida pelo Joane.
    [MenuItem("ArrasTEA/Ajustar Layout Grade (2x4 + nome/estrelas)")]
    public static void AjustarLayoutGrade()
    {
        string prefabPath = "Assets/Prefabs/LevelSelect_Manager.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null) { Debug.LogError("ArrasTEA: Prefab não encontrado."); return; }
        try
        {
            var levelGrid = root.transform.Find("Canvas/ContentPanel/LevelGrid");
            if (levelGrid == null) { Debug.LogError("ArrasTEA: LevelGrid não encontrado."); return; }

            // 1) Grade 2 x 4 (FixedColumnCount = 4)
            var grid = levelGrid.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (grid != null)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
                grid.cellSize = new Vector2(300f, 340f);
                grid.spacing = new Vector2(30f, 30f);
            }

            // 2) Para cada cartão, reposiciona: estrelas topo, icone centro, nome baixo
            for (int i = 0; i < 8; i++)
            {
                string nome = i < 8 ? TIPOS[i*3] : "Fase" + (i+1);
                Transform cartao = levelGrid.Find(nome);
                if (cartao == null) continue;

                // Estrelas no topo
                Transform stars = cartao.Find("UnlockedContent/StarsContainer");
                if (stars != null)
                {
                    var rt = stars.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, -20f);
                }

                // Ícone no centro (pode já existir "Icon" ou ser o filho Image do UnlockedContent)
                Transform icone = cartao.Find("Icon");
                if (icone != null)
                {
                    var rt = icone.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.55f);
                    rt.anchorMax = new Vector2(0.5f, 0.55f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, 0f);
                    rt.sizeDelta = new Vector2(100f, 110f);
                    icone.SetAsLastSibling();
                }

                // Nome embaixo, na frente (último irmão de novo após o ícone)
                Transform nomeT = cartao.Find("UnlockedContent/Text (TMP)");
                if (nomeT == null) nomeT = cartao.transform.Find("Text (TMP)");
                if (nomeT != null)
                {
                    var rt = nomeT.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, 16f);
                    nomeT.SetAsLastSibling(); // fica por cima
                }
            }

            bool ok;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out ok);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ArrasTEA: layout grade ajustado (2x4, estrelas topo, nome baixo). saveOk=" + ok);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // Reposiciona no LevelButton_Template.prefab: estrelas no TOPO e nome no RODAPÉ.
    // Como os 8 cartões da grade são instâncias deste template, a mudança propaga a todos.
    [MenuItem("ArrasTEA/Posicionar Estrelas/Nome no Template")]
    public static void PosicionarTemplate()
    {
        string prefabPath = "Assets/Prefabs/LevelButton_Template.prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null) { Debug.LogError("ArrasTEA: LevelButton_Template não encontrado."); return; }
        try
        {
            // Acha por nome robusto (GetComponentsInChildren)
            Transform stars = null, textT = null;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "StarsContainer") stars = t;
                else if (t.name == "Text (TMP)") textT = t;
            }

            if (stars != null)
            {
                var rt = stars.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -18f);
                rt.SetAsLastSibling();
            }
            if (textT != null)
            {
                var rt = textT.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 18f);
                textT.SetAsLastSibling();
            }

            bool ok;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out ok);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ArrasTEA: template reposicionado (estrelas topo, nome rodapé). stars=" + (stars!=null) + " text=" + (textT!=null) + " saveOk=" + ok);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // Reposiciona na cena EscolherComImg (os 8 cartões REAIS que o Joane usa):
    // estrelas no TOPO (visíveis, dentro do cartão), ícone da fase GRANDE centralizado,
    // nome no RODAPÉ; cartão mais alto p/ acomodar. Roda com a cena aberta.
    [MenuItem("ArrasTEA/Ajustar Layout dos Botões na Cena (ícones+estrelas)")]
    public static void AjustarLayoutCena()
    {
        string[] botoes = { "Frutas", "EncontrarIgual", "EncontrarDiferente", "PintarFruta",
                            "Formas", "Cores", "Brinquedos", "Tamanhos" };

        int ok = 0;
        foreach (string nome in botoes)
        {
            GameObject container = GameObject.Find(nome);
            if (container == null) { Debug.LogWarning("Não achei " + nome); continue; }

            // Botão visual (o filho "Fase button pequeno" que tem Button/Image)
            Transform btnT = container.transform.Find("Fase button pequeno");
            RectTransform btnRT = btnT != null ? btnT.GetComponent<RectTransform>() : null;
            if (btnRT == null) continue;

            // 1) Cartão mais alto p/ caber ícone + estrelas + nome
            btnRT.anchorMin = new Vector2(0.5f, 0.5f);
            btnRT.anchorMax = new Vector2(0.5f, 0.5f);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.sizeDelta = new Vector2(200f, 175f);   // cartão razoável
            btnRT.anchoredPosition = new Vector2(0f, 30f); // sobe um pouco p/ estrelas no topo

            // 2) Ícone da fase — a Image FILHA do botão (não o fundo do próprio botão),
            // que carrega o sprite escolher* da fase. Grande e centralizado.
            Transform iconT = btnT.Find("Image");           // filho direto = ícone da fase
            if (iconT == null) iconT = btnT.Find("Icon");
            Image icone = iconT != null ? iconT.GetComponent<Image>() : null;
            TextMeshProUGUI txt = btnRT.GetComponentInChildren<TextMeshProUGUI>(true);
            if (icone != null)
            {
                RectTransform irt = icone.rectTransform;
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(0f, 12f);
                irt.sizeDelta = new Vector2(90f, 90f);
                icone.raycastTarget = false;
            }

            // 3) Nome da fase — rodapé, na frente
            if (txt != null)
            {
                RectTransform trt = txt.rectTransform;
                trt.anchorMin = new Vector2(0.5f, 0f);
                trt.anchorMax = new Vector2(0.5f, 0f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.anchoredPosition = new Vector2(0f, 14f);
                txt.fontSizeMin = 28f;
                txt.enableAutoSizing = true;
                txt.transform.SetAsLastSibling();
            }

            // 4) Estrelas — container para o TOPO do cartão, visíveis
            Transform stars = container.transform.Find("Stars");
            if (stars != null)
            {
                RectTransform srt = stars.GetComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 1f);
                srt.anchorMax = new Vector2(0.5f, 1f);
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.anchoredPosition = new Vector2(0f, -14f);
                srt.sizeDelta = new Vector2(90f, 24f);
                // cada estrela: 22px visível
                for (int s = 0; s < stars.childCount && s < 3; s++)
                {
                    RectTransform srt2 = stars.GetChild(s).GetComponent<RectTransform>();
                    if (srt2 != null) srt2.sizeDelta = new Vector2(22f, 22f);
                }
                stars.SetAsLastSibling();
            }
            ok++;
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("ArrasTEA: layout dos botões ajustado na cena (ok=" + ok + "/8). Valide no fluxo real.");
    }

    // Monta o display de emblema ELABORADO no canvas do ArrasTEA (estilo preservado):
    // ícone do emblema + nível + BARRA DE XP (Slider) + texto. Usa o painel EmblemaPanel
    // já existente (que o SelecaoFasesGamificada popula). Mantém o estilo roxo/branco.
    [MenuItem("ArrasTEA/Montar Display de Emblema Elaborado (ícone+XpBar)")]
    public static void MontarEmblemaElaborado()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("ArrasTEA: Canvas não encontrado."); return; }

        Transform panel = canvas.transform.Find("EmblemaPanel");
        if (panel == null)
        {
            // Cria painel se não existir (canto superior direito)
            GameObject p = new GameObject("EmblemaPanel");
            p.transform.SetParent(canvas.transform, false);
            var prt = p.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(1f, 1f); prt.anchorMax = new Vector2(1f, 1f);
            prt.pivot = new Vector2(1f, 1f); prt.anchoredPosition = new Vector2(-24f, -24f);
            prt.sizeDelta = new Vector2(230f, 96f);
            panel = p.transform;
        }
        // Ajusta o painel p/ acomodar ícone + barra + texto
        RectTransform prt2 = panel.GetComponent<RectTransform>();
        prt2.pivot = new Vector2(1f, 1f);
        prt2.anchorMin = new Vector2(1f, 1f); prt2.anchorMax = new Vector2(1f, 1f);
        prt2.anchoredPosition = new Vector2(-24f, -24f);
        prt2.sizeDelta = new Vector2(250f, 96f);

        // Ícone do emblema
        Transform icon = panel.Find("EmblemIcon");
        if (icon == null)
        {
            GameObject g = new GameObject("EmblemIcon", typeof(Image));
            g.transform.SetParent(panel, false);
            icon = g.transform;
        }
        RectTransform irt = icon.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0f, 0.5f); irt.anchorMax = new Vector2(0f, 0.5f);
        irt.pivot = new Vector2(0f, 0.5f); irt.anchoredPosition = new Vector2(0f, 0f);
        irt.sizeDelta = new Vector2(72f, 72f);
        var emblemas = Resources.LoadAll<Sprite>("Game/Emblemas/emblemas_spritesheet");
        Image iconImg = icon.GetComponent<Image>();
        if (emblemas != null && emblemas.Length > 0) iconImg.sprite = emblemas[0];
        iconImg.preserveAspect = true; iconImg.raycastTarget = false;

        // Texto do nível na frente do ícone
        Transform txtNivel = panel.Find("EmblemNivelText");
        if (txtNivel == null)
        {
            GameObject g = new GameObject("EmblemNivelText", typeof(TextMeshProUGUI));
            g.transform.SetParent(panel, false);
            txtNivel = g.transform;
        }
        RectTransform nrt = txtNivel.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(0f, 1f); nrt.anchorMax = new Vector2(0f, 1f);
        nrt.pivot = new Vector2(0f, 0.5f); nrt.anchoredPosition = new Vector2(80f, -14f);
        nrt.sizeDelta = new Vector2(160f, 24f);
        TextMeshProUGUI nt = txtNivel.GetComponent<TextMeshProUGUI>();
        nt.text = "Nível 1"; nt.fontSize = 22; nt.color = new Color(0.25f, 0.2f, 0.4f);
        nt.alignment = TextAlignmentOptions.Left; nt.raycastTarget = false;

        // BARRA DE XP (Slider) — preenchida por nome "EmblemXpBar" pelo runtime
        Transform bar = panel.Find("EmblemXpBar");
        if (bar == null)
        {
            GameObject g = new GameObject("EmblemXpBar", typeof(Slider));
            g.transform.SetParent(panel, false);
            bar = g.transform;
        }
        RectTransform brt = bar.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0.5f); brt.anchorMax = new Vector2(0f, 0.5f);
        brt.pivot = new Vector2(0f, 0.5f); brt.anchoredPosition = new Vector2(80f, -6f);
        brt.sizeDelta = new Vector2(160f, 14f);
        Slider sl = bar.GetComponent<Slider>();
        sl.interactable = false;
        // Background quadrado (fundo da barra)
        if (bar.GetComponent<Image>() == null) bar.gameObject.AddComponent<Image>();
        bar.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.9f, 1f);

        // Fill (preenchimento roxo, cor tema ArrasTEA)
        Transform fillArea = bar.Find("Fill Area");
        if (fillArea == null)
        {
            GameObject fa = new GameObject("Fill Area"); fa.transform.SetParent(bar, false);
            fillArea = fa.transform;
            RectTransform fart = fa.AddComponent<RectTransform>();
            fart.anchorMin = Vector2.zero; fart.anchorMax = Vector2.one; fart.offsetMin = Vector2.zero; fart.offsetMax = Vector2.zero;
        }
        Transform fill = fillArea.Find("Fill");
        if (fill == null)
        {
            GameObject fg = new GameObject("Fill", typeof(Image)); fg.transform.SetParent(fillArea, false);
            fill = fg.transform;
            RectTransform frt = fg.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            Image fi = fg.GetComponent<Image>();
            fi.color = new Color(0.48f, 0.3f, 0.75f, 1f); // roxo
            fi.raycastTarget = false;
        }
        // Handle invisível
        Transform handleArea = bar.Find("Handle Slide Area");
        if (handleArea == null)
        {
            GameObject ha = new GameObject("Handle Slide Area"); ha.transform.SetParent(bar, false);
            handleArea = ha.transform;
            RectTransform hart = ha.AddComponent<RectTransform>();
            hart.anchorMin = Vector2.zero; hart.anchorMax = Vector2.one; hart.offsetMin = Vector2.zero; hart.offsetMax = Vector2.zero;
            GameObject hg = new GameObject("Handle", typeof(Image)); hg.transform.SetParent(handleArea, false);
            RectTransform hrt = hg.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(20f, 20f); hrt.anchorMin = new Vector2(0f,0.5f); hrt.anchorMax = new Vector2(0f,0.5f);
            Image hi = hg.GetComponent<Image>(); hi.color = new Color(0f,0f,0f,0f);
        }
        sl.fillRect = (RectTransform)fill.transform;
        sl.handleRect = (RectTransform)handleArea.Find("Handle") != null ? (RectTransform)handleArea.Find("Handle") : null;
        sl.targetGraphic = handleArea.Find("Handle") != null ? handleArea.Find("Handle").GetComponent<Image>() : null;
        sl.direction = Slider.Direction.LeftToRight; sl.minValue = 0; sl.maxValue = 100; sl.value = 0;
        if (sl.handleRect != null) sl.handleRect.transform.gameObject.SetActive(false);

        // Texto de XP abaixo da barra
        Transform txtXP = panel.Find("EmblemText");
        if (txtXP == null)
        {
            GameObject g = new GameObject("EmblemText", typeof(TextMeshProUGUI));
            g.transform.SetParent(panel, false);
            txtXP = g.transform;
        }
        RectTransform xrt = txtXP.GetComponent<RectTransform>();
        xrt.anchorMin = new Vector2(0f, 0f); xrt.anchorMax = new Vector2(0f, 0f);
        xrt.pivot = new Vector2(0f, 0.5f); xrt.anchoredPosition = new Vector2(80f, 10f);
        xrt.sizeDelta = new Vector2(160f, 22f);
        TextMeshProUGUI xt = txtXP.GetComponent<TextMeshProUGUI>();
        xt.text = "0/100 XP"; xt.fontSize = 18; xt.color = new Color(0.25f, 0.2f, 0.4f);
        xt.alignment = TextAlignmentOptions.Left; xt.raycastTarget = false;

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveScene(canvas.gameObject.scene);
        Debug.Log("ArrasTEA: display de emblema elaborado montado (ícone + XpBar).");
    }

    // CORREÇÃO DE PROPORÇÃO (importante): o Canvas interno do prefab LevelSelect_Manager veio
    // com m_Camera=0 (screen-space-camera sem câmera => escala por overlay, feito pro My-project).
    // Para os elementos ficarem proporcionais AO FUNDO do ArrasTEA (que usa screen-space-camera
    // com a câmera da cena), vincula o canvas interno do prefab à MESMA câmera da cena.
    [MenuItem("ArrasTEA/Vincular Canvas do LevelSelect à Câmera da Cena (proporção)")]
    public static void VincularCanvasLevelSelect()
    {
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam == null)
        {
            // usa qualquer câmera da cena
            cam = UnityEngine.Object.FindObjectOfType<UnityEngine.Camera>();
        }
        if (cam == null) { Debug.LogError("ArrasTEA: nenhuma câmera na cena."); return; }

        Transform lsm = GameObject.Find("LevelSelect_Manager") != null ? GameObject.Find("LevelSelect_Manager").transform : null;
        if (lsm == null) { Debug.LogError("ArrasTEA: LevelSelect_Manager não instanciado na cena."); return; }

        // acha o Canvas dentro do prefab (filho LevelSelect_Manager/Canvas)
        Transform canvasT = lsm.Find("Canvas");
        UnityEngine.Canvas cv = canvasT != null ? canvasT.GetComponent<UnityEngine.Canvas>() : null;

        // também o Canvas raiz original pode ter sido o que tem camera; pega o do prefab
        if (cv == null)
        {
            UnityEngine.Canvas[] all = lsm.GetComponentsInChildren<UnityEngine.Canvas>(true);
            cv = all.Length > 0 ? all[0] : null;
        }
        if (cv == null) { Debug.LogError("ArrasTEA: Canvas do prefab LevelSelect_Manager não encontrado."); return; }

        cv.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
        cv.worldCamera = cam;
        cv.planeDistance = 100f;

        EditorSceneManager.MarkSceneDirty(lsm.gameObject.scene);
        EditorSceneManager.SaveScene(lsm.gameObject.scene);
        Debug.Log("ArrasTEA: canvas do LevelSelect vinculado à câmera '" + cam.name + "' (proporção com o fundo do ArrasTEA).");
    }

    // NORMALIZAÇÃO DE ESCALA/TAMANHO (reconstrução limpa do layout no canvas ArrasTEA):
    // todo elemento de UI deve ter localScale=1 (o tamanho é feito pelo CanvasScaler 1920x1080,
    // não por scale manual) e sizeDelta coerente. Corrige botões que ficaram com scale 0.07
    // (minúsculos) e estrelas/ícones desalinhados.
    [MenuItem("ArrasTEA/Normalizar Layout dos Botões (scale=1 + tamanho)")]
    public static void NormalizarLayout()
    {
        string[] botoes = { "Frutas", "EncontrarIgual", "EncontrarDiferente", "PintarFruta",
                            "Formas", "Cores", "Brinquedos", "Tamanhos" };
        int ok = 0;
        foreach (string nome in botoes)
        {
            GameObject container = GameObject.Find(nome);
            if (container == null) continue;

            // Container: scale 1
            container.transform.localScale = Vector3.one;

            // Botão visual
            Transform btnT = container.transform.Find("Fase button pequeno");
            if (btnT == null) continue;
            btnT.localScale = Vector3.one;
            RectTransform btnRT = btnT.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0.5f);
            btnRT.anchorMax = new Vector2(0.5f, 0.5f);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = new Vector2(0f, 10f);
            btnRT.sizeDelta = new Vector2(180f, 150f);   // cartão coerente p/ 1920x1080

            // Ícone da fase (filho Image) — centralizado, grande
            Transform iconT = btnT.Find("Image");
            if (iconT == null) iconT = btnT.Find("Icon");
            if (iconT != null)
            {
                iconT.localScale = Vector3.one;
                RectTransform irt = iconT.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(0f, 14f);
                irt.sizeDelta = new Vector2(90f, 90f);
            }

            // Nome — rodapé
            Transform txtT = btnT.Find("Text (TMP)");
            if (txtT != null)
            {
                txtT.localScale = Vector3.one;
                RectTransform trt = txtT.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2(0.5f, 0f);
                trt.anchorMax = new Vector2(0.5f, 0f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.anchoredPosition = new Vector2(0f, 12f);
                trt.sizeDelta = new Vector2(160f, 30f);
                TextMeshProUGUI t = txtT.GetComponent<TextMeshProUGUI>();
                if (t != null) { t.enableAutoSizing = true; t.fontSizeMin = 22f; t.fontSizeMax = 30f; }
            }

            // Estrelas — dentro do cartão, no topo (scale 1, pequenas)
            Transform stars = container.transform.Find("Stars");
            if (stars != null)
            {
                stars.localScale = Vector3.one;
                RectTransform srt = stars.GetComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.5f, 1f);
                srt.anchorMax = new Vector2(0.5f, 1f);
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.anchoredPosition = new Vector2(0f, -12f);
                srt.sizeDelta = new Vector2(78f, 24f);
                for (int s = 0; s < stars.childCount && s < 3; s++)
                {
                    Transform st = stars.GetChild(s);
                    st.localScale = Vector3.one;
                    RectTransform srt2 = st.GetComponent<RectTransform>();
                    if (srt2 != null) srt2.sizeDelta = new Vector2(22f, 22f);
                }
            }
            ok++;
        }

        // EmblemaPanel: scale 1 (se existir)
        var embObj = GameObject.Find("EmblemaPanel");
        if (embObj != null) embObj.transform.localScale = Vector3.one;

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("ArrasTEA: layout normalizado (scale=1 + tamanho) em " + ok + "/8 botões.");
    }

    // PORTA o sistema de emblema COMPLETO do prefab LevelSelect_Manager para o CANVAS do ArrasTEA:
    // os 3 objetos (EmblemDisplay + EmblemInfoPanel + UnlockedEmblemsPanel) com seus filhos, e a
    // lógica (LevelSelectController) reconectada. Funcionamento: clicar no EmblemDisplay abre o
    // EmblemInfoPanel; o ViewUnlockedButton do painel abre o UnlockedEmblemsPanel; Close/Back fecham.
    [MenuItem("ArrasTEA/Portar Sistema Emblema p/ Canvas")]
    public static void PortarSistemaEmblema()
    {
        var lsm = GameObject.Find("LevelSelect_Manager");
        var canvas = GameObject.Find("Canvas"); // canvas raiz do ArrasTEA
        if (lsm == null || canvas == null) { Debug.LogError("ArrasTEA: preciso de LevelSelect_Manager e Canvas na cena."); return; }

        string[] paineis = { "EmblemDisplay", "EmblemInfoPanel", "UnlockedEmblemsPanel" };
        foreach (string p in paineis)
        {
            // acha o painel dentro do canvas interno do prefab (qualquer profundidade)
            Transform src = null;
            foreach (Transform t in lsm.GetComponentsInChildren<Transform>(true))
                if (t.name == p) { src = t; break; }
            if (src == null) { Debug.LogWarning("Portar: painel não achado no LevelSelect: " + p); continue; }
            src.SetParent(canvas.transform, false); // move p/ canvas do ArrasTEA (mantém RectTransform local)
        }

        // LevelSelectController no canvas do ArrasTEA (lógica do emblema)
        var csc = canvas.GetComponent<LevelSelectController>();
        if (csc == null) csc = canvas.AddComponent<LevelSelectController>();

        // Remove o LevelSelectController do LevelSelect_Manager (evita duplicidade que não usaremos)
        var lsc = lsm.GetComponent<LevelSelectController>();
        if (lsc != null) Object.DestroyImmediate(lsc);

        // Conecta os botões ao LevelSelectController do canvas
        ConectaBotao(canvas.transform, "EmblemIcon_Display", csc, "ToggleEmblemInfoPanel");
        ConectaBotao(canvas.transform, "CloseButton", csc, "ToggleEmblemInfoPanel");
        ConectaBotao(canvas.transform, "ViewUnlockedButton", csc, "ShowUnlockedEmblemsPanel");
        ConectaBotao(canvas.transform, "BackToInfoPanelButton", csc, "HideUnlockedEmblemsPanel");

        // Desativa painéis que abrem ao clicar
        Transform info = canvas.transform.Find("EmblemInfoPanel");
        if (info != null) info.gameObject.SetActive(false);
        Transform unp = canvas.transform.Find("UnlockedEmblemsPanel");
        if (unp != null) unp.gameObject.SetActive(false);

        // Remove o display de emblema simples antigo (EmblemaPanel) p/ não conflitar
        var velho = canvas.transform.Find("EmblemaPanel");
        if (velho != null) Object.DestroyImmediate(velho.gameObject);

        // Desativa o LevelSelect_Manager (agora casca vazia, sem os painéis)
        lsm.SetActive(false);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveScene(canvas.gameObject.scene);
        Debug.Log("ArrasTEA: sistema de emblema portado p/ canvas (EmblemDisplay/Info/Unlocked).");
    }

    static void ConectaBotao(Transform raiz, string nome, LevelSelectController alvo, string metodo)
    {
        Transform bt = null;
        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            if (t.name == nome && t.GetComponent<Button>() != null) { bt = t; break; }
        if (bt == null) { Debug.LogWarning("ConectaBotao: achou GO mas sem Button: " + nome); return; }
        Button b = bt.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        // liga ao método público do LevelSelectController
        var metodoPublico = alvo.GetType().GetMethod(metodo, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (metodoPublico != null)
        {
            if (metodo == "ToggleEmblemInfoPanel")
                b.onClick.AddListener(alvo.ToggleEmblemInfoPanel);
            else if (metodo == "ShowUnlockedEmblemsPanel")
                b.onClick.AddListener(alvo.ShowUnlockedEmblemsPanel);
            else if (metodo == "HideUnlockedEmblemsPanel")
                b.onClick.AddListener(alvo.HideUnlockedEmblemsPanel);
        }
    }
}
