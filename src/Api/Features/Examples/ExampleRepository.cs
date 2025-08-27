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

	private static readonly StringComparer TitleComparer = StringComparer.OrdinalIgnoreCase;

	private bool TitleExists(string title, Guid? exceptId = null) =>
	_store.Any(kv =>
		(!exceptId.HasValue || kv.Key != exceptId.Value) &&
		TitleComparer.Equals(kv.Value.Title, title));

	public Task<Example?> GetAsync(Guid id) =>
		Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

	public Task<IReadOnlyList<Example>> ListAsync() =>
		Task.FromResult((IReadOnlyList<Example>)_store.Values.ToList());

	public Task<Example> AddAsync(string title)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
		title = title.Trim();

		if (TitleExists(title))
			throw new InvalidOperationException($"An example with the title '{title}' already exists.");

		var ex = new Example(Guid.NewGuid(), title, false);
		_store[ex.Id] = ex;

		return Task.FromResult(ex);
	}

	public Task<bool> UpdateAsync(Guid id, string title, bool done)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
		title = title.Trim();

		if (!_store.ContainsKey(id))
			return Task.FromResult(false);

		if (TitleExists(title, id))
			throw new InvalidOperationException($"An example with the title '{title}' already exists.");

		_store[id] = new Example(id, title, done);

		return Task.FromResult(true);
	}

	public Task<bool> DeleteAsync(Guid id)
	{
		var removed = _store.TryRemove(id, out _);
		return Task.FromResult(removed);
	}
}
