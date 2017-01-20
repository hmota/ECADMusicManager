using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using SincronizacaoMusical.Domain.SincronizacaoSharepoint;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatoriosSharepoint
    {
        public void GerarECAD(int sonorizacaoID, string titulo, string autor, string classificacao, DateTime data,
                              string isrc, string interprete, TimeSpan minutagem, string programa, string tipoDeExibicao, string tipoDeMusica)
        {
            //verifica se existe para excluir antes de incluir
            var dataContext = GetClientSharepoint();
            ECADItem item = dataContext.ECAD.Where(s => s.SonorizacaoID == sonorizacaoID).FirstOrDefault();

            if (item != null)
            {
                dataContext.DeleteObject(item);
                dataContext.SaveChanges();
            }
            //inclui novo item
            var ecad = new ECADItem
                           {
                               SonorizacaoID = sonorizacaoID,
                               Título = titulo,
                               Autor = autor,
                               Classificação = classificacao,
                               Data = data,
                               ISRC = isrc,
                               Interprete = interprete,
                               Minutagem = minutagem.TotalSeconds,
                               Programa = programa,
                               TipoDeExibição = tipoDeExibicao,
                               TipoDeMusica = tipoDeMusica
                           };

            dataContext.AddToECAD(ecad);
            dataContext.SaveChanges();
        }


        public SincronizacaoMusicalDataContext GetClientSharepoint()
        {
            var webClient = new WebClient
            {
                UseDefaultCredentials = true,
                Credentials =
                    new NetworkCredential("hmota",
                                          ConfigurationManager.AppSettings["pk"].ToString(CultureInfo.InvariantCulture),
                                          "REDERECORD")
            };
            var dataContext =
                new SincronizacaoMusicalDataContext(new Uri("http://xisto02/musical/_vti_bin/listdata.svc"))
                {
                    Credentials = webClient.Credentials
                };

            return dataContext;
        }
    }
}
