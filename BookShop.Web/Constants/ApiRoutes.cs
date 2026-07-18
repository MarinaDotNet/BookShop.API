namespace BookShop.Web.Constants;

/// <summary>
/// Contains API route constants used for communication with the BookShop API.
/// </summary>
public static class ApiRoutes
{
    /// <summary>
    /// Base API path.
    /// </summary>
    public const string Api = "/api";

    /// <summary>
    /// API version 3 routes.
    /// </summary>
    public static class V3
    {   
        /// <summary>
        /// Base path for version 3 endpoints.
        /// </summary>
        public const string Base = $"{Api}/v3";

        /// <summary>
        /// Book-related endpoints.
        /// </summary>
        public static class Books
        {
            /// <summary>
            /// Books controller route.
            /// </summary>
            public const string Controller = "/Books";

            /// <summary>
            /// Endpoint for retrieving the cheapest books.
            /// </summary>
            public const string Cheapest = $"{Base}{Controller}/cheapest";

            /// <summary>
            /// Endpoint for retrieving the most expensive books.
            /// </summary>
            public const string Expensive = $"{Base}{Controller}/expensive";

        }
    }
}