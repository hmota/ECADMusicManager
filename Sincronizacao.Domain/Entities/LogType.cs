
namespace SincronizacaoMusical.Domain.Entities
{
    public enum LogType
    {
        Alerta = 0,
        Informacao = 1,
        Erro = 2,
        Ok=200,
        SemConteudo=204,
        FalhaRequisicao=400,
        NaoAutorizado=401,
        NaoEncontrado=404,
        SessaoExpirada=408,
        ErroInterno=500,
        ServicoIndisponivel=503
    }
}
