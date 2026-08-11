namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Application-level role, derived from <see cref="User.ProfileId"/> — no schema
    /// change, no EF-level conversion. Screener is the enum's default (0) value so
    /// an unset/uninitialized role is least-privilege.
    /// </summary>
    public enum UserRole
    {
        /// <summary>Least-privileged role; sees only screening features.</summary>
        Screener = 0,

        /// <summary>Full access, including user and system administration.</summary>
        Admin = 1
    }
}
