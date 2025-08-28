using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Features.Examples;
using Xunit;

namespace Api.Tests;

public class ExampleRepositoryTests
{
	[Fact]
	public async Task ListAsync_EmptyStore_ReturnsEmptyList()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();

		// Act
		var list = await repo.ListAsync();

		// Assert
		Assert.NotNull(list);
		Assert.Empty(list);
	}

	[Fact]
	public async Task AddAsync_WithValidTitle_ReturnsCreatedEntity()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var title = "Sample Title";

		// Act
		var created = await repo.AddAsync(title);

		// Assert
		Assert.NotEqual(Guid.Empty, created.Id);
		Assert.Equal(title, created.Title);
		Assert.False(created.Done);
	}

	[Fact]
	public async Task GetAsync_WithValidId_ReturnsEntity()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var created = await repo.AddAsync("A");

		// Act
		var found = await repo.GetAsync(created.Id);

		// Assert
		Assert.NotNull(found);
		Assert.Equal(created.Id, found!.Id);
		Assert.Equal("A", found.Title);
		Assert.False(found.Done);
	}

	[Fact]
	public async Task GetAsync_WithUnknownId_ReturnsNull()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var unknown = Guid.Parse("11dfd289-2544-47da-a263-cc88d34e6808");

		// Act
		var found = await repo.GetAsync(unknown);

		// Assert
		Assert.Null(found);
	}

	[Fact]
	public async Task UpdateAsync_WithExistingId_UpdatesTitleAndDone()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var created = await repo.AddAsync("Original");
		var newTitle = "Updated";
		var newDone = true;

		// Act
		var ok = await repo.UpdateAsync(created.Id, newTitle, newDone);
		var after = await repo.GetAsync(created.Id);

		// Assert
		Assert.True(ok);
		Assert.NotNull(after);
		Assert.Equal(newTitle, after!.Title);
		Assert.True(after.Done);
	}

	[Fact]
	public async Task UpdateAsync_WithUnknownId_ReturnsFalse()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var unknown = Guid.NewGuid();

		// Act
		var ok = await repo.UpdateAsync(unknown, "DoesNotMatter", done: true);

		// Assert
		Assert.False(ok);
	}

	[Fact]
	public async Task UpdateAsync_ToExistingTitle_ThrowsInvalidOperationException()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var a = await repo.AddAsync("TitleA");
		var b = await repo.AddAsync("TitleB");

		// Act + Assert
		// Cambiar el título de B a "TitleA" debe chocar con la validación de duplicado
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await repo.UpdateAsync(b.Id, "TitleA", b.Done);
		});
	}

	[Fact]
	public async Task UpdateAsync_DuplicateTitle_IsCaseInsensitive()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var a = await repo.AddAsync("TitleA");
		var b = await repo.AddAsync("Another");

		// Act + Assert
		// Si la comparación de título es case-insensitive, "titlea" debe considerarse duplicado de "TitleA"
		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await repo.UpdateAsync(b.Id, "titlea", b.Done);
		});
	}

	[Fact]
	public async Task DeleteAsync_WithExistingId_RemovesAndReturnsTrue()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();
		var created = await repo.AddAsync("ToDelete");

		// Act
		var removed = await repo.DeleteAsync(created.Id);
		var after = await repo.GetAsync(created.Id);

		// Assert
		Assert.True(removed);
		Assert.Null(after);
	}

	[Fact]
	public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
	{
		// Arrange
		var repo = new InMemoryExampleRepository();

		// Act
		var removed = await repo.DeleteAsync(Guid.NewGuid());

		// Assert
		Assert.False(removed);
	}
}
