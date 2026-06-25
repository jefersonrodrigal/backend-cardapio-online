using Application.Common.Interfaces;
using Application.Features.Categories.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands;

public record UpdateCategoryCommand(int Id, string Name, int SortOrder) : IRequest<CategoryDto>;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCategoryHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand cmd, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Categoria {cmd.Id} nao encontrada.");

        category.Name = cmd.Name.Trim();
        category.SortOrder = cmd.SortOrder;

        await db.SaveChangesAsync(ct);

        return CategoryDto.FromCategory(category);
    }
}
