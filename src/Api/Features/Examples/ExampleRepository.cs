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

	// Comparador para títulos (case-insensitive)
	private static readonly StringComparer TitleComparer = StringComparer.OrdinalIgnoreCase;

	private bool TitleExists(string title, Guid? exceptId = null)
	{
		foreach (var kv in _store)
		{
			if (exceptId.HasValue && kv.Key == exceptId.Value) continue;
			if (TitleComparer.Equals(kv.Value.Title, title)) return true;
		}

		return false;
	}

	public Task<Example?> GetAsync(Guid id) =>
		Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

	public Task<IReadOnlyList<Example>> ListAsync() =>
		Task.FromResult((IReadOnlyList<Example>)_store.Values.ToList());

	public Task<Example> AddAsync(string title)
	{
		if (TitleExists(title))
			throw new InvalidOperationException($"An example with the title '{title}' already exists.");

		var ex = new Example(Guid.NewGuid(), title, false);
		_store[ex.Id] = ex;

		return Task.FromResult(ex);
	}

	public Task<bool> UpdateAsync(Guid id, string title, bool done)
	{
		if (!_store.ContainsKey(id))
			return Task.FromResult(false);

		// Evitar colisión de título con otros registros
		if (TitleExists(title, id))
			throw new InvalidOperationException($"An example with the title '{title}' already exists.");

		_store[id] = new Example(id, title, done);

		return Task.FromResult(true);
	}

	public Task<bool> DeleteAsync(Guid id) =>
		Task.FromResult(_store.Remove(id));
}
//Change only for create a PR....
