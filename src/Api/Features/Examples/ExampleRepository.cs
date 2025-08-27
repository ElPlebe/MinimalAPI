using System.Collections.Concurrent;

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
	private readonly ConcurrentDictionary<Guid, Example> _store = new();

	public Task<Example?> GetAsync(Guid id) =>
		Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

	public Task<IReadOnlyList<Example>> ListAsync() =>
		Task.FromResult((IReadOnlyList<Example>)_store.Values.OrderBy(e => e.Title).ToList());

	public Task<Example> AddAsync(string title)
	{
		if (TitleExists(title))
			throw new InvalidOperationException($"An example with the title '{title}' already exists.");

		var ex = new Example(Guid.NewGuid(), title.Trim(), Done: false);
		_store[ex.Id] = ex;
		return Task.FromResult(ex);
	}

	public Task<bool> UpdateAsync(Guid id, string title, bool done)
	{
		if (!_store.ContainsKey(id)) return Task.FromResult(false);

		var normalized = title?.Trim() ?? string.Empty;
		if (TitleExists(normalized, excludeId: id))
			throw new InvalidOperationException($"An example with the title '{normalized}' already exists.");

		_store[id] = new Example(id, normalized, done);
		return Task.FromResult(true);
	}

	public Task<bool> DeleteAsync(Guid id)
	{
		var removed = _store.TryRemove(id, out _);
		return Task.FromResult(removed);
	}

	private bool TitleExists(string title, Guid? excludeId = null)
	{
		return _store.Values.Any(e =>
			(!excludeId.HasValue || e.Id != excludeId.Value) &&
			string.Equals(e.Title, title, StringComparison.OrdinalIgnoreCase));
	}
}
