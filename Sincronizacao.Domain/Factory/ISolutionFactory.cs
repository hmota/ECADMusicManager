
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.Domain.Factory
{
    public interface ISolutionFactory
    {
        ILogRepository GetLogRepository();
    }
}
