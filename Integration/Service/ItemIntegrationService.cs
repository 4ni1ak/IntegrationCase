using Integration.Backend;
using Integration.Common;
using Microsoft.Extensions.Caching.Memory;

namespace Integration.Service
{
    // The class within the Integration.Service namespace
    public sealed class ItemIntegrationService
    {
        private readonly IMemoryCache _memoryCache;  // Interface reference for memory caching
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();  // ReaderWriterLockSlim object for read/write locking
        private readonly ItemOperationBackend _itemIntegrationBackend = new ItemOperationBackend();  // Reference for backend operations

        // Constructor configured with IMemoryCache
        public ItemIntegrationService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        // Represents the method to save an item coming from the external world
        public Result SaveItem(string itemContent)
        {
            _lock.EnterWriteLock();
            try
            {
                // Attempt to add the item atomically
                if (_memoryCache.TryGetValue(itemContent, out _))
                {
                    return new Result(false, $"Duplicate item received with content {itemContent}.");
                }

                // Create a new item using ItemOperationBackend
                var item = _itemIntegrationBackend.SaveItem(itemContent);

                // Attempt to add the item atomically
                if (_memoryCache.TryGetValue(itemContent, out _))
                {
                    // Another thread may have added this item.
                    return new Result(false, $"Duplicate item received with content {itemContent}.");
                }

                // Attempt to add the item atomically using the Add method
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Example: 1 minute
                };

                if (_memoryCache.TryGetValue(itemContent, out _))
                {
                    // Another thread may have added this item.
                    return new Result(false, $"Duplicate item received with content {itemContent}.");
                }
                _memoryCache.Set(itemContent, item, cacheEntryOptions);

                return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Represents a method to get all items
        public List<Item> GetAllItems()
        {
            _lock.EnterReadLock();  // Acquire the lock before starting the read operation
            try
            {
                return _itemIntegrationBackend.GetAllItems();
            }
            finally
            {
                _lock.ExitReadLock();  // Release the lock after completing the read operation
            }
        }
    }
}
