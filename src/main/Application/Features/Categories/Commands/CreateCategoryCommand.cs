using Application.Common.Interfaces;
using Application.Features.Categories.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, int SortOrder) : IRequest<CategoryDto>;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateCategoryHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var slug = GenerateSlug(cmd.Name);

        if (await db.Categories.AnyAsync(c => c.Slug == slug, ct))
            throw new InvalidOperationException($"Ja existe uma categoria com o identificador '{slug}'.");

        var category = new Category
        {
            Slug = slug,
            Name = cmd.Name.Trim(),
            SortOrder = cmd.SortOrder,
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return CategoryDto.FromCategory(category);
    }

    internal static string GenerateSlug(string name)
    {
        var normalized = name.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return Regex.Replace(
            sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(),
            @"[^a-z0-9]+", "-").Trim('-');
    }
}
