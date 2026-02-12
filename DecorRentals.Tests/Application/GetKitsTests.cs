using DecorRental.Application.UseCases.GetKits;
using DecorRental.Domain.Entities;
using DecorRental.Tests.Application.Fakes;
using Xunit;

namespace DecorRental.Tests.Application;

public class GetKitsTests
{
    [Fact]
    public async Task Should_return_paginated_items_and_total_count()
    {
        var repository = new FakeKitRepository();
        await repository.AddAsync(new Kit("Kit C"));
        await repository.AddAsync(new Kit("Kit A"));
        await repository.AddAsync(new Kit("Kit B"));

        var handler = new GetKitsHandler(repository);

        var result = await handler.HandleAsync(new GetKitsQuery(2, 1));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Single(result.Items);
        Assert.Equal("Kit B", result.Items[0].Name);
    }
}
