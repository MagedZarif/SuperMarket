using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;

public class ItemUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ItemUpdateService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<APPDBContext>();

                var itemIds = await _context.items
                    .Select(i => i.Id)
                    .ToListAsync(stoppingToken);

                foreach (var itemId in itemIds)
                {
                    var item = await _context.items.FindAsync(itemId);
                    if (item != null)
                    {
                        // Update Quantity and Expiration
                        item.Quantity = await _context.Iitems.CountAsync(i => i.ItemId == itemId, stoppingToken);
                        item.IsExpired = await _context.Iitems.AnyAsync(i => i.ItemId == itemId && i.ExpiredDate < DateTime.UtcNow, stoppingToken);
                    }
                }

                await _context.SaveChangesAsync(stoppingToken);
            }

            // Wait 24 hours before running again
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
