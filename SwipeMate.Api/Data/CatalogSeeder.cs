using Microsoft.EntityFrameworkCore;

namespace SwipeMate.Api.Data;

public static class CatalogSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var seedItems = DemoCatalog.CreateCatalogItems();

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
            existing.IsActive = true;
        }

        await db.SaveChangesAsync();
    }
}
