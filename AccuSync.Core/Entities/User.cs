using System;

namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Represents a user account. Field types and conventions (Unix timestamps,
    /// int-as-boolean) match the device database schema.
    /// </summary>
    // TODO: FirstLogin, Status use int where bool would be clearer. Consider
    //       bool properties with int-backed fields if the DB schema can't change.
    public class User
    {
        public string Guid { get; set; }
        public string AccountName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Status { get; set; }
        public string ProfileId { get; set; }
        public string PasswordHash { get; set; }
        public int FirstLogin { get; set; }                 // 0 = false, 1 = true
        public int FailedLoginAttemptCount { get; set; }
        public int FailedResetAttemptCount { get; set; }
        public long FirstFailedLoginTime { get; set; }      // Unix timestamp (seconds)
        public long FirstResetLoginTime { get; set; }       // Unix timestamp (seconds)
        public long CreationDate { get; set; }              // Unix timestamp (seconds)
        public long ModificationDate { get; set; }          // Unix timestamp (seconds)
        public long PasswordModificationDate { get; set; }  // Unix timestamp (seconds)

        // TODO: Storing previous passwords as a single string is fragile and
        //       unclear (delimiter? hashed?). Document the expected format or
        //       replace with a structured type.
        public string LastThreePasswords { get; set; }

        public User()
        {
            Guid = System.Guid.NewGuid().ToString();
            AccountName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Status = 0;
            ProfileId = string.Empty;
            PasswordHash = string.Empty;
            FirstLogin = 1; // New users haven't logged in yet
            FailedLoginAttemptCount = 0;
            FailedResetAttemptCount = 0;
            FirstFailedLoginTime = 0L;
            FirstResetLoginTime = 0L;
            CreationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ModificationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PasswordModificationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastThreePasswords = string.Empty;
        }
    }
}