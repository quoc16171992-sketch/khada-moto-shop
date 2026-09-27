namespace Demo1.Services;

public interface IOrderCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
