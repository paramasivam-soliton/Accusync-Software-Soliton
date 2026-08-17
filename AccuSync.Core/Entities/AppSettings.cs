namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Single-row table of app-wide configuration values. There is exactly one row —
    /// no exposed identifier, since a genuine singleton has nothing to identify;
    /// <c>AppSettingsConfiguration</c> backs the table's required primary key with a
    /// shadow property EF manages on its own, and <c>AppSettingsRepository</c> reads/seeds
    /// the row without ever referring to it by key.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Minutes an account stays locked after hitting the failed-attempt threshold
        /// before it auto-unlocks. Admin-configurable; an Admin can also unlock a
        /// specific account immediately via <c>UnlockUserAsync</c>, overriding this
        /// duration for that one account.
        /// </summary>
        public int LockoutDurationMinutes { get; set; }

        public AppSettings()
        {
            LockoutDurationMinutes = 15;
        }
    }
}
