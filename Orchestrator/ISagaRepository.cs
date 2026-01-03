namespace Orchestrator;

public interface ISagaRepository
{
    Task<Guid> Create();
}