namespace NhatSoft.Common.Constants;

public static class AppConstants
{
    // Gom nhóm các hằng số về Role
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "Client";
    }

    // Sau này bạn có thể tái sử dụng file này cho các hằng số khác
    public static class CacheKeys
    {
        public const string ProjectsList = "projects_list";
        public const string PostsList = "posts_list";
    }
}
