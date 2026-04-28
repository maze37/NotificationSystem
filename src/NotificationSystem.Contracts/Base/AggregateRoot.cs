namespace NotificationSystem.Contracts.Base;

/// <summary>
/// Базовый класс корня агрегата.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
