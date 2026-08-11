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
        /// <summary>The user's unique identifier.</summary>
        public string Guid { get; set; }
        /// <summary>The user's login account name.</summary>
        public string AccountName { get; set; }
        /// <summary>The user's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>The user's last name.</summary>
        public string LastName { get; set; }
        /// <summary>The user's account status.</summary>
        public int Status { get; set; }
        /// <summary>The identifier of the profile assigned to the user.</summary>
        public string ProfileId { get; set; }
        /// <summary>The user's password, as stored by the device database schema.</summary>
        public string ProfilePassword { get; set; }
        /// <summary>Whether this is the user's first login (0 = false, 1 = true).</summary>
        public int FirstLogin { get; set; }                 // 0 = false, 1 = true
        /// <summary>The number of consecutive failed login attempts.</summary>
        public int FailedLoginAttemptCount { get; set; }
        /// <summary>The number of consecutive failed password reset attempts.</summary>
        public int FailedResetAttemptCount { get; set; }
        /// <summary>Unix timestamp (seconds) of the first failed login attempt.</summary>
        public long FirstFailedLoginTime { get; set; }      // Unix timestamp (seconds)
        /// <summary>Unix timestamp (seconds) of the first failed password reset attempt.</summary>
        public long FirstResetLoginTime { get; set; }       // Unix timestamp (seconds)
        /// <summary>Unix timestamp (seconds) when the user account was created.</summary>
        public long CreationDate { get; set; }              // Unix timestamp (seconds)
        /// <summary>Unix timestamp (seconds) when the user account was last modified.</summary>
        public long ModificationDate { get; set; }          // Unix timestamp (seconds)
        /// <summary>Unix timestamp (seconds) when the user's password was last modified.</summary>
        public long PasswordModificationDate { get; set; }  // Unix timestamp (seconds)

        // TODO: Storing previous passwords as a single string is fragile and
        //       unclear (delimiter? hashed?). Document the expected format or
        //       replace with a structured type.
        /// <summary>The user's last three passwords.</summary>
        public string LastThreePasswords { get; set; }

        /// <summary>Creates a new user with a generated <see cref="Guid"/> and default field values.</summary>
        public User()
        {
            Guid = System.Guid.NewGuid().ToString();
            AccountName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Status = 0;
            ProfileId = string.Empty;
            ProfilePassword = string.Empty;
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