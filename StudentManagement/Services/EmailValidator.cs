using System.Net.Mail;
using System.Net;
using DnsClient;
using Microsoft.Extensions.Caching.Memory;

namespace StudentManagement.Helpers
{
    public static class EmailValidator
    {
        // In-memory cache to avoid repeated DNS lookups for the same domains
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 1000 // Limit cache to 1000 entries
        });

        private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6), // Cache for 6 hours
            SlidingExpiration = TimeSpan.FromHours(1), 
            Size = 1
        };

      
        public static bool IsValidEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var mail = new MailAddress(email);
                // Additional check: ensure the email matches the original string
                return mail.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> HasMxRecordsAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                    return false;

                var host = email.Split('@')[1].ToLowerInvariant();

                // Check cache first
                string cacheKey = $"mx_{host}";
                if (_cache.TryGetValue(cacheKey, out bool cachedResult))
                {
                    return cachedResult;
                }

                // DNS lookup with timeout configuration
                var lookupOptions = new LookupClientOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    UseCache = true,
                    Retries = 2
                };
                var lookup = new LookupClient(lookupOptions);

                var result = await lookup.QueryAsync(host, QueryType.MX);
                bool hasMxRecords = result.Answers.MxRecords().Any();

                // Cache the result
                _cache.Set(cacheKey, hasMxRecords, _cacheOptions);

                return hasMxRecords;
            }
            catch (DnsResponseException)
            {
                // DNS lookup failed - domain doesn't exist
                return false;
            }
            catch (Exception)
            {
                // For other errors (network issues, timeout), return false
                return false;
            }
        }

        public static async Task<bool> IsRealEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine($"[EmailValidator] Email is null or whitespace");
                return false;
            }

            // Quick format check first
            if (!IsValidEmailFormat(email))
            {
                Console.WriteLine($"[EmailValidator] Invalid format: {email}");
                return false;
            }

            // Check cache for full validation result
            string fullCacheKey = $"email_valid_{email.ToLowerInvariant()}";
            if (_cache.TryGetValue(fullCacheKey, out bool cachedFullResult))
            {
                Console.WriteLine($"[EmailValidator] Cache hit for {email}: {cachedFullResult}");
                return cachedFullResult;
            }

            Console.WriteLine($"[EmailValidator] Performing MX lookup for {email}...");
            bool isValid = await HasMxRecordsAsync(email);
            Console.WriteLine($"[EmailValidator] MX lookup result for {email}: {isValid}");

            // Cache the full validation result
            _cache.Set(fullCacheKey, isValid, _cacheOptions);

            return isValid;
        }
        public static void ClearCache()
        {
            _cache.Compact(1.0); // Remove 100% of cache entries
        }

        public static int GetCacheCount()
        {
            return _cache.Count;
        }
    }
}