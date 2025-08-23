namespace Api.Features.Examples;

public interface IExampleRepository
{
	Task<Example?> GetAsync(Guid id);
	Task<IReadOnlyList<Example>> ListAsync();
	Task<Example> AddAsync(string title);
	Task<bool> UpdateAsync(Guid id, string title, bool done);
	Task<bool> DeleteAsync(Guid id);
}

public sealed class InMemoryExampleRepository : IExampleRepository
{
	private readonly Dictionary<Guid, Example> _store = new();

	public Task<Example?> GetAsync(Guid id) =>
		Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

	public Task<IReadOnlyList<Example>> ListAsync() =>
		Task.FromResult((IReadOnlyList<Example>)_store.Values.ToList());

	public Task<Example> AddAsync(string title)
	{
		var ex = new Example(Guid.NewGuid(), title, false);
		_store[ex.Id] = ex;
		return Task.FromResult(ex);
	}

	public Task<bool> UpdateAsync(Guid id, string title, bool done)
	{
		if (!_store.ContainsKey(id)) return Task.FromResult(false);
		_store[id] = new Example(id, title, done);
		return Task.FromResult(true);
	}

	public Task<bool> DeleteAsync(Guid id) =>
		Task.FromResult(_store.Remove(id));
}
