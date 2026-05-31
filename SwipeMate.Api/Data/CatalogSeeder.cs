using Microsoft.EntityFrameworkCore;

namespace SwipeMate.Api.Data;

public static class CatalogSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var seedItems = DemoCatalog.CreateCatalogItems();
        var seededExternalIds = seedItems
            .Select(x => x.ExternalId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in seedItems)
        {
            var existing = await db.CatalogItems
                .FirstOrDefaultAsync(x => x.ExternalId == item.ExternalId);

            if (existing is null)
            {
                db.CatalogItems.Add(item);
                continue;
            }

            existing.Category = item.Category;
            existing.Title = item.Title;
            existing.ImageUrl = item.ImageUrl;
            existing.MetaJson = item.MetaJson;
        }

        var existingItems = await db.CatalogItems.ToListAsync();
        foreach (var retiredItem in existingItems.Where(x => !seededExternalIds.Contains(x.ExternalId)))
        {
            retiredItem.IsActive = false;
        }

        await db.SaveChangesAsync();
    }
}

