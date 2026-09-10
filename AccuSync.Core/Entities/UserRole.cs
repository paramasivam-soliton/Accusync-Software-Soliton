namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Application-level role, derived from <see cref="User.ProfileId"/> — a foreign key
    /// into the <see cref="Profile"/> table whose id values are defined to match this enum's
    /// underlying values exactly. Screener is the enum's default (0) value so an
    /// unset/uninitialized role is least-privilege.
    /// </summary>
    public enum UserRole
    {
        /// <summary>Least-privileged role; sees only screening features.</summary>
        Screener = 0,

        /// <summary>Full access, including user and system administration.</summary>
        Admin = 1
    }
}
