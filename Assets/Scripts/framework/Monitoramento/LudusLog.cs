using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


// Basicamente, utilizando como conceito primordial as tabelas de banco de dados, o nosso LOG Geral será um grande tabela, com vários logs.
// A classe LudusLogCol seria uma Coluna; logController seria a tabela.
// Então, cada célula do log, teria um id, título, descrição e o tempo de execução em que foi executado.
// Depois, esses dados serão adicionados no LogController e assim serão submetidos.
// Por enquanto, eles estão em formato de string, mas pretendemos colocá-los em JSON, e assim exportar para um banco de dados.


namespace Ludus.SDK.Framework
{
    //Classe estágica para o LudusLog poder ser populado em difeentes objetos

    [System.Serializable]
    public static class JsonLog
    {
        public static LudusLog log = new LudusLog();
    }




    #region LUDUSLOG_COL 
    // Criando a classe LudusLogCol, que seriam as colunas da nossa tabela LudusLog.
    // Elas possuem alguns atributos.
    /*
        1   - Id: é o id da tabela a ser gerado, normalmente são representados por um número inteiro em formato de string.
        2   - Title: Título da célula, pode ser qualquer coisa, mas normalmente para um boa prática, recomendamos colocar um título que seja 
            correspondente ao uso.
        3   - Data: São as informações sobre o log. Você pode covertê-los a um JSON string, e colocar ali. Nesta versão, por enquanto, só há esta opção.
        4   - Date: Pega a description e hora do momento e registra, é feito de foram automática, você nem precisa se preocupar  com isso.
        5   - TimeElapsed: é um atributo que diz respeito ao tempo de execução do script.
     */
    [System.Serializable]
    public class LudusLogCol
    {
        public string id;
        public string title;
        public string description;
        public string date;
        public LudusLogCol(string title = "", string description = "")
        {
            try
            {
                if (title == "")
                {
                    throw new UnityException("[LudusLogCol-module-cell-err]: O título deve ser obrigatório. ");
                }

                if (description == "")
                {
                    description = "Não informada";
                }

                this.id = "To set.";
                this.title = title;
                this.description = description;
                this.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            }
            catch (UnityException err)
            {
                throw err;
            }
        }
    }
    #endregion LUDUSLOG_COL

    #region LUDUS_LOG
    /*
        O LudusLog é feito para abrigar todas as colunas em apenas uma tabela; abrigar todos os objetos em um objeto-pai, para termos os dados de forma centralizada.
        Sobre os atributos:
        Lista LogCells - é a lista das células de log.
        Counter - serve para contar a quantidade de colunas, sem precisar usar reports.Count toda hora.
        Title - Título do log pai.
        Data - Feito para abrigar os dados, representado de em string.
     */
    [System.Serializable]
    public class LudusLog
    {
        public string app; // Título do log.
        public string version; // Título do log.
        public string scene;
        public string build;
        public string datehourstart;
        public string datehourend;
        public List<LudusLogCol> reports = new List<LudusLogCol>(); // Cria uma lista de colunas;
        private int counter = 0; // Contador de colunas.

        //  OBS: Esse contador é uma alternativa mais eficiente, pois ele irá sempre ser adicionado, em vez de contar os elementos da lista.
        // Também, este contador deve ser imutável, ele será utilizado com id. Por execução, ele deve ser uníco. Então, caso ocorra algum erro, ele será adicionado automaticamente
        // e o índice onde o erro ocorreu será anulado.

        
        public void addCol(LudusLogCol newLog) // Adicionar uma coluna...
        {
            try
            {
                if (newLog == null)
                {
                    throw new UnityException("[+LUDUS-createlog-err]: A célula de log está vazia.");
                }
                counter++;
                newLog.id = counter.ToString();
                reports.Add(newLog);

            }
            catch (UnityException err)
            {
                throw err;
            }
        }
        public void removeColById(string id) // Remover uma coluna pelo seu id.
        {
            try
            {
                LudusLogCol logToRemove = reports.Find(log => log.id == id);

                if (logToRemove != null)
                {
                    reports.Remove(logToRemove);
                }
                else
                {
                    throw new UnityException("[+LUDUS-createlog-err]: célula não encontrada.");
                }
            }
            catch (UnityException err)
            {
                throw err;
            }
        }
        public void export() // Exportar o log.
        {
            try
            {
                string jsonToExport = "";
               
                //guarda a hora do final da interação
                this.datehourend = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                jsonToExport = JsonUtility.ToJson(this);

                string rootFolder = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                if (rootFolder == null)
                {
                    throw new UnityException("[+LUDUS-createlog-exportJSON-err]: Diretório não encontrado.");
                }
               

                string directoryPath = Path.Combine(rootFolder, "Assets", "Resources/Log");
                string filename = this.app + "-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "-" + this.genRandomNumAsString(3) + ".json";
                string filePath = Path.Combine(directoryPath, filename);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(jsonToExport);
                }

                Debug.Log("Arquivo exportado com sucesso: " + filePath);
            }
            catch (Exception err)
            {
                Debug.LogError("Erro ao exportar o arquivo: " + err.Message);
            }
        }
        public void reset() // Resetar os dados do log, mas manter o seu título e id padrão,
        {
            this.reports.Clear();
        }
        
        public string genRandomNumAsString(uint bytes = 0)
        {

            try
            {
                if (bytes == 0)
                {
                    throw new UnityException("[genRndNumAsStr(): Defina um valor válido à string.");
                }

                string buffer = "";
                string allowedCode = "1234567890";
                System.Random rand = new System.Random();
                uint i = 0;
                while (i < bytes)
                {
                    buffer += allowedCode[rand.Next(0, allowedCode.Length)];
                    i++;
                }

                return buffer;
            }
            catch (UnityException err)
            {
                throw err;
            }
        }
    }
    #endregion LUDUS_LOG
};
