namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Application-level role, derived from <see cref="User.ProfileId"/> — no schema
    /// change, no EF-level conversion. Screener is the enum's default (0) value so
    /// an unset/uninitialized role is least-privilege.
    /// </summary>
    public enum UserRole
    {
        Screener = 0,
        Admin = 1
    }
}
