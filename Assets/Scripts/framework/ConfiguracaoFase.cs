using UnityEngine;
namespace Ludus.SDK.Framework
{

    public class ConfiguracaoFase : Configuracao
    {


        public override void CarregarConfiguracao(GameObject novoPainelGeral)
        {
            //se é a primeira vez que chama a cena
            if (inicial)
            {
                acertosFase = 0;
            }
            base.CarregarConfiguracao(novoPainelGeral);


        }

        protected override void CarregarPaineis()
        {
            for (int i = 0; i < niveis[nivelAtual].quantidadeElementos; i++)
            {
                AdicionarPrefabObjeto(painelObjeto);
                AdicionarPrefabSombra(painelSombra);
            }

        }

        public override void CarregarCena()
        {
            acertosFase = 0;


            base.CarregarCena();

        }

        public override void AtualizarAcerto()
        {
            acertosFase++;
            if (acertosFase == niveis[nivelAtual].quantidadeElementos)
            {
                base.AtualizarAcerto();
            }
        }


    }

}



