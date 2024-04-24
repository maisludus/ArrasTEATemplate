
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// Script feito por Ely Torres Neto para projeto +LUDUS.
// Caso você queira ver o meu github, fique à vontade: https://github.com/netoe1

// Sobre as variáveis constantes com o prefixo PREFIX:
//      Elas serão usadas como textos padronizados para os logs, isso facilita na hora de tratar erros e dar display nas
//      mensagens, pois é um label padrão, então, se mudarmos uma vez, atualiza no código inteiro.
// Depreciado
namespace Ludus.SDK.Framework
{
    interface IMonitorMouseData     // Essa interface é responsável por obter os dados do mouse.
    {
        Vector3 GetMousePosition(); //  Método Get() que retorna a posição do mouse em coordenadas.
        /* void IsPointerNotMoving();*/  //  Verifica se o ponteiro está se mexendo. Função Depreciada.
    }
    //interface IMonitorTime          //  Essa interface é responsável por gerenciar os contadores de tempo que serão utilizados.
    //{
    //    // OBSERVAÇÃO IMPORTANTE: Essas primeiras funções, modificam apenas o TimeSpan e StopWatch do próprio Monitor, o qual já é instanciado por padrão.
    //    void StartMonitoringTime(); //  Inicia o contador de tempo.
    //    TimeSpan GetCurrentTime();  // Obtém data e hora padrão. Similar ao Date.Now() do Javascript.
    //    void EndMonitoringTime();   // Termina o tempo de monitoramento.
    //}
    //Interface depreciada.

    public class Monitor :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerMoveHandler,
        IMonitorMouseData
    {
        //// Prefixos que serão utilizados de string para os logs.
        //const string PREFIX_LUDUS_WARN = "[+LUDUS-WARNING]:";
        //const string PREFIX_LUDUS_SUCCESS = "[+LUDUS-SUCCESS]:";
        //const string PREFIX_LUDUS_ERROR = "[+LUDUS-ERROR]:";
        // Depreciado.

        ////  IMonitorTime
        //private Stopwatch timeCounter = new Stopwatch(); // This object allow to count time, to use in logs.
        // Depreciado.


        void Start()
        {
            Scene scene = SceneManager.GetActiveScene();

            JsonLog.log.app = Application.productName;
            JsonLog.log.version = Application.version;
            JsonLog.log.scene = scene.name;
            JsonLog.log.build = EditorUserBuildSettings.activeBuildTarget.ToString();
            JsonLog.log.datehourstart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            JsonLog.log.addCol(new LudusLogCol("[+LUDUS-monitor-start]", "O script iniciou em'" + this.gameObject.name + "', id: " + this.gameObject.GetInstanceID()));
            //  UnityEngine.Debug.Log("[+LUDUS-monitor-start]: O script iniciou em '" + this.gameObject.name + "',id:" + this.gameObject.GetInstanceID()); // Log mostra para o usuário que o script está em ação!
            //Canvas objeto = FindObjectOfType<Canvas>();

            Transform parentTransform = this.transform;

            // Loop through all child
            //
            // elements of the parent
            AddMonitoramento(parentTransform);

            //DontDestroyOnLoad(this);
            Debug.Log("Cena carregada: ");
        }

        private void AddMonitoramento(Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {

                child.gameObject.AddComponent<MonitorClique>();
                // Se o compontente não é botão adiciona o monitoramento dos filhos
                // isso ocorre pra evitar conflito do OnClick do botão com o monitoramento do texto ilustrativo que vem .. ou de uma imagem
                if (child.gameObject.GetComponent<Button>() == null)
                {
                    AddMonitoramento(child.transform);
                }
            }

        }

        public Vector3 GetMousePosition() // Retorna a posição do Vector3, através do Input.mousePosition.
        {
            return Input.mousePosition;
        }
        public void OnPointerClick(PointerEventData eventData)
        {

            string side = "undefined"; // String auxiliar utilizada para facilitar o JsonLog.log.

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                side = "esquerdo";          // Dá o set identificando qual lado do mouse foi clicado.
            }

            else if (eventData.button == PointerEventData.InputButton.Middle)
            {

                side = "do meio";
            }

            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                side = "direito";
            }

            JsonLog.log.addCol(new LudusLogCol("[+LUDUS-mouse-click]:", "Botão " + side + " ." + eventData.pointerClick.gameObject.name));
            //  UnityEngine.Debug.Log("[+LUDUS-mouse-click]: O botão " + side + "."); // Mostra o log ao usuário.
        }
        public void OnPointerMove(PointerEventData eventData)
        {
            try
            {
                Vector3 pos = GetMousePosition();
                JsonLog.log.addCol(new LudusLogCol("[+LUDUS-mouse-move]:", "POS:(" + pos.x + ";" + pos.y + ";" + pos.z + ")"));
                //  UnityEngine.Debug.Log("[+LUDUS-mouse-move]: POS:(" + pos.x + "," + pos.y + "," + pos.z + ")");
            }
            catch (UnityException err)
            {
                throw err;
            }
        }

        void OnDestroy()
        {
            JsonLog.log.export();
            JsonLog.log.reset();
            
        }

    }

}