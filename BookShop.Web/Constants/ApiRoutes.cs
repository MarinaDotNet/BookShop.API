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
    /// API version 1 routes.
    /// </summary>
    public static class V1
    {
        /// <summary>
        /// Base path for version 1 endpoints.
        /// </summary>
        public const string Base = $"{Api}/v1";

        public static class Auth
        {
            /// <summary>
            /// Base path for version 1 endpoints.
            /// </summary>
            public const string Controller = "/Auth";

            /// <summary>
            /// Login endpoint.
            /// </summary>
            public const string LogIn = $"{Base}{Controller}/login";

            /// <summary>
            /// API endpoint for logging out the current user.
            /// </summary>
            public const string LogOut = $"{Base}{Controller}/logout";

            /// <summary>
            /// API endpoint for registering a new user account.
            /// </summary>
            public const string Register = $"{Base}{Controller}/register";
        }
    }

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