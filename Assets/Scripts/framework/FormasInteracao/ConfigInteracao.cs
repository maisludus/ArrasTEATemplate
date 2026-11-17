using System;
using UnityEngine;
using UnityEngine.UI;
using static Ludus.SDK.Framework.Configuracao;

public class ConfigInteracao : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button ArrastaESolta, PegaESolta, CliqueSimples;
    private Color cororiginal;
    void Start()
    {
        //pega a cor original dos botões
        cororiginal = ArrastaESolta.GetComponent<Image>().color;
        if (PlayerPrefs.HasKey("formainteracao"))
        {
            int escolha = PlayerPrefs.GetInt("formainteracao");
            switch (escolha)
            {
                case 0:
                    ArrastaESolta.GetComponent<Image>().color = Color.green;
                    break;
                case 1:
                    CliqueSimples.GetComponent<Image>().color = Color.green;
                    break;
                case 2:
                    PegaESolta.GetComponent<Image>().color = Color.green;
                    break;
                default:
                    break;
            }
        }
        //parei aqui
        ArrastaESolta.onClick.AddListener(() => SelecionarFormaInteracao(0));
        CliqueSimples.onClick.AddListener(() => SelecionarFormaInteracao(1));
        PegaESolta.onClick.AddListener(() => SelecionarFormaInteracao(2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SelecionarFormaInteracao(int forma)
    {
        PlayerPrefs.SetInt("formainteracao", forma);
        Debug.Log(forma);
         switch (forma)
        {
            case 0:
                ArrastaESolta.GetComponent<Image>().color = Color.green;
                CliqueSimples.GetComponent<Image>().color = cororiginal;
                PegaESolta.GetComponent<Image>().color = cororiginal;
                break;
            case 1:
                ArrastaESolta.GetComponent<Image>().color = cororiginal;
                CliqueSimples.GetComponent<Image>().color = Color.green;
                PegaESolta.GetComponent<Image>().color = cororiginal;
                break;
            case 2:
                ArrastaESolta.GetComponent<Image>().color = cororiginal;
                CliqueSimples.GetComponent<Image>().color = cororiginal;
                PegaESolta.GetComponent<Image>().color = Color.green;
                break;
            default:
                break;
        }
    }
}
