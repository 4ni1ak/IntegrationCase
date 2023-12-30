using Integration.Service;
using Microsoft.Extensions.Caching.Memory;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new ItemIntegrationService(memoryCache);

        #region Default
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(50);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(50);

        Console.WriteLine("Everything recorded:");


        service.GetAllItems().ForEach(Console.WriteLine);

        #endregion

        #region ForParalel



        ThreadPool.QueueUserWorkItem(_ => SaveItemWithDelay(service, "p"));
        ThreadPool.QueueUserWorkItem(_ => SaveItemWithDelay(service, "r"));
        ThreadPool.QueueUserWorkItem(_ => SaveItemWithDelay(service, "l"));

        Thread.Sleep(50);

        ThreadPool.QueueUserWorkItem(_ => SaveItemWithDelay(service, "p"));
        ThreadPool.QueueUserWorkItem(_ => SaveItemWithDelay(service, "r"));
        ThreadPool.QueueUserWorkItem(_ => SaveItemWithDelay(service, "l"));

        Thread.Sleep(50);

        Console.WriteLine("Everything recorded:");


        service.GetAllItems()
            .Where(x => x.Id >= 4)
            .ToList()
            .ForEach(Console.WriteLine);
        #endregion


        Console.ReadLine();
    }
    private static void SaveItemWithDelay(ItemIntegrationService service, string itemContent)
    {
        Thread.Sleep(1000);
        service.SaveItem(itemContent);
    }

}