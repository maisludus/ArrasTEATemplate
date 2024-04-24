using JetBrains.Annotations;


namespace Ludus.SDK.Framework 
{ 
    public  class ConfiguracaoFaseUmItem:Configuracao
    {
    
 
   

        protected override void CarregarPaineis()
        {
            for (int i = 0; i < niveis[nivelAtual].quantidadeElementos; i++)
            {
                AdicionarPrefabObjeto(painelObjeto);
           
            }
            
            AdicionarPrefabSombra(painelSombra);

        }


    }

}



