using System;

namespace ShopSavvy.DataApi
{
    /// <summary>
    /// Base exception for ShopSavvy API errors
    /// </summary>
    public class ShopSavvyApiException : Exception
    {
        public ShopSavvyApiException(string message) : base(message) { }
        public ShopSavvyApiException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when authentication fails
    /// </summary>
    public class ShopSavvyAuthenticationException : ShopSavvyApiException
    {
        public ShopSavvyAuthenticationException(string message) : base(message) { }
        public ShopSavvyAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a resource is not found
    /// </summary>
    public class ShopSavvyNotFoundException : ShopSavvyApiException
    {
        public ShopSavvyNotFoundException(string message) : base(message) { }
        public ShopSavvyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when request validation fails
    /// </summary>
    public class ShopSavvyValidationException : ShopSavvyApiException
    {
        public ShopSavvyValidationException(string message) : base(message) { }
        public ShopSavvyValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when rate limit is exceeded
    /// </summary>
    public class ShopSavvyRateLimitException : ShopSavvyApiException
    {
        public ShopSavvyRateLimitException(string message) : base(message) { }
        public ShopSavvyRateLimitException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a network error occurs
    /// </summary>
    public class ShopSavvyNetworkException : ShopSavvyApiException
    {
        public ShopSavvyNetworkException(string message) : base(message) { }
        public ShopSavvyNetworkException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a request times out
    /// </summary>
    public class ShopSavvyTimeoutException : ShopSavvyApiException
    {
        public ShopSavvyTimeoutException(string message) : base(message) { }
        public ShopSavvyTimeoutException(string message, Exception innerException) : base(message, innerException) { }
    }
}