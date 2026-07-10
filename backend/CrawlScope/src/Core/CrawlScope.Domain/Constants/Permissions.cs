namespace CrawlScope.Domain.Constants
{
    public static class Permissions
    {
        public static class CrawlJobs
        {
            public const string View = "Permissions.CrawlJobs.View";
            public const string Create = "Permissions.CrawlJobs.Create";
            public const string Start = "Permissions.CrawlJobs.Start";
            public const string Export = "Permissions.CrawlJobs.Export";
        }

        public static class CrawledPages
        {
            public const string View = "Permissions.CrawledPages.View";
        }

        public static class Schedules
        {
            public const string View = "Permissions.Schedules.View";
            public const string Create = "Permissions.Schedules.Create";
            public const string Manage = "Permissions.Schedules.Manage";
        }

        public static class Admin
        {
            public const string Access = "Permissions.Admin.Access";
        }

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Manage = "Permissions.Users.Manage";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Manage = "Permissions.Roles.Manage";
        }

        public static IEnumerable<string> All()
        {
            yield return CrawlJobs.View;
            yield return CrawlJobs.Create;
            yield return CrawlJobs.Start;
            yield return CrawlJobs.Export;
            yield return CrawledPages.View;
            yield return Schedules.View;
            yield return Schedules.Create;
            yield return Schedules.Manage;
            yield return Admin.Access;
            yield return Users.View;
            yield return Users.Manage;
            yield return Roles.View;
            yield return Roles.Manage;
        }

        public static IEnumerable<string> UserDefaults()
        {
            yield return CrawlJobs.View;
            yield return CrawlJobs.Create;
            yield return CrawlJobs.Start;
            yield return CrawlJobs.Export;
            yield return CrawledPages.View;
        }
    }
}
