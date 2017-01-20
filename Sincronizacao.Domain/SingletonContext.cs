namespace SincronizacaoMusical.Domain
{
    public sealed class SingletonContext
    {
        
            // Inicialização estática - thread-safe.
            private static readonly SingletonContext instance = new SingletonContext();

            // Instância do ObjectContext.
            private readonly Context context;

            // Construtor privado - Singleton pattern.
            private SingletonContext()
            {
                context = new Context();
                context.Configuration.LazyLoadingEnabled = true;
            }

            // Retorna instância única da classe.
            public static SingletonContext Instance
            {
                get
                {
                    return instance;
                }
            }

            // Retorna ObjectContext
            public Context Context
            {
                get
                {
                    return context;
                }
            }
        
    }
}
