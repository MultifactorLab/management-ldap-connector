namespace Api.Framework.EndpointDiscovery;

/// <summary>
/// Создает экземпляры ендпоинтов.
/// </summary>
internal static class EndpointActivator
{
    /// <summary>
    /// Создает экземпляр указанного типа <paramref name="type"/>, который должен реализовывать <see cref="IEndpoint"/>.
    /// </summary>
    /// <param name="type">Тип, для которого требуется создать экземпляр.</param>
    /// <returns>экземпляр <see cref="IEndpoint"/>. Конструктор типа не должен содержать параметров.</returns>
    /// <exception cref="InvalidOperationException">Бросает исключение, если не получилось активировать тип или привести его к типу <see cref="IEndpoint"/>.</exception>
    public static IEndpoint Activate(Type type)
    {
        var name = type.FullName;
        object? instance;
        try
        {
            instance = Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to activate endpoint {name}", ex);
        }

        if (instance is IEndpoint endpoint)
        {
            return endpoint;
        }
        
        throw new InvalidOperationException($"Unable to instantiate endpoint {name}");
    }
}