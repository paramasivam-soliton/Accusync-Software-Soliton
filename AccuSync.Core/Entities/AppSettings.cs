namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Single-row table of app-wide configuration values. There is exactly one row,
    /// identified by <see cref="Id"/> — see <c>AppSettingsRepository</c> for how it's
    /// seeded/read.
    /// </summary>
    public class AppSettings
    {
        public string Id { get; set; }

        /// <summary>
        /// Minutes an account stays locked after hitting the failed-attempt threshold
        /// before it auto-unlocks. Admin-configurable; an Admin can also unlock a
        /// specific account immediately via <c>UnlockUserAsync</c>, overriding this
        /// duration for that one account.
        /// </summary>
        public int LockoutDurationMinutes { get; set; }

        public AppSettings()
        {
            Id = "Default";
            LockoutDurationMinutes = 15;
        }
    }
}
