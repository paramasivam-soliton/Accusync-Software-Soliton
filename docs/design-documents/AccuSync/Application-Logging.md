[[_TOC_]]

# Who

Author: [Chokkalingam Shanmugam](mailto:chokka.shanmugam@solitontech.com)

# 1. Feature work item

1. [ASWD-98 — LG-F1: Logging that works, and logging in the code already written (feature)](https://natus-jira.atlassian.net/browse/ASWD-98)

# 2. Links to reference material

- AccuSync Software Requirements Specification, [DOC-076814](https://mynatus.sharepoint.com/:w:/r/teams/AccuSyncSoftware/Shared%20Documents/General/ForSoliton/Software%20Requirements/DOC-076814%20Rev%2001%20AccuSync%20Software%20Requirements.docx?d=w9fde02a7e5e248f186d18b25f4975f86&csf=1&web=1&e=wupYgv) —
  referenced clauses: GID-255031 (log message generation), GID-255032 (log message types),
  GID-255033 (log message storage), GID-255034 (log data privacy), GID-255038 (log entry fields).

# 3. Implementation and design 

## Problem statement

### Why logging

When something goes wrong in AccuSync — the app fails to start, a sign-in is rejected, a save to
the database fails — there is currently no reliable, shared way to record what the app was doing
right before it happened, which means understanding a problem after the fact means trying to
reproduce it live:

- What the app prints today only shows up in a debugger or an attached console. Once the app is
  closed, or when it's running on a machine nobody is actively debugging, that information is
  simply gone.
- There's no single, consistent place or format for this information — different parts of the app
  handle it differently, or not at all.
- A couple of the app's existing capabilities — starting up (including first-time database setup)
  and signing in — currently produce no log output of their own. If either one fails, there is
  nothing to look at afterward.

### What we introduce

A shared logging capability that any part of the app can use to write a line to a single log file,
in a consistent format — without patient data ending up in that file, and without ever being able
to crash the app itself.

### Where logging gets added

Alongside the logging capability itself, this work also adds real logging to the two areas called
out above that don't have any today: application startup (including first-time database setup),
and the sign-in flow (login, logout, lockout). The scattered print-style debug statements found
elsewhere in the app are also replaced with real logging calls.

## Implementation

A new interface, `ILoggingService`, lives in `AccuSync.Core` — the same layer that already holds
`IAuthenticationService`, `IPasswordHasher`, and similar. It offers four simple methods, one per
severity: Debug, Info, Warning, Error. Error optionally takes the exception that caused it.

A second piece, `FileLoggingService`, does the actual writing and lives in `AccuSync.Application`.
`AccuSync.WPF` wires the two together on startup and adds the new configuration setting for the log
location.

### The four logging levels

Every log line is written at one of four levels, chosen by whoever is writing the call based on
how significant the event is:

| Level | Used for | Example |
|---|---|---|
| Debug | Fine-grained detail only useful while actively investigating something | The exact parameters being sent for a database query |
| Info | Normal, expected events worth knowing happened | The app finished starting up successfully |
| Warning | Something unexpected that the app noticed and recovered from on its own | A device took longer than expected to respond, but the app retried and continued |
| Error | Something failed and needs attention | Saving a patient record failed and had to be aborted |

### What's inside the log file, and where it lives

Every line written to the log includes the date and time, its level (from the table above), which
class and method it came from, and the message itself. When an error is logged together with an
exception, only the exception's type and message are recorded — never its full technical dump —
since that dump can otherwise contain the exact data an operation was trying to save, which for
this app can mean a patient's name, birth date, or other personal details. That guarantee covers
the mechanism itself; it can't stop someone from typing sensitive information into a log message by
hand — avoiding that is up to whoever writes the log call, the same as for any other part of the
app.

The log is a single text file, appended to over time — no daily files, and nothing is
automatically deleted. By default it's written to `%ProgramData%\Natus\AccuSync\Logs`, with the
option to point it somewhere else through the app's configuration if needed.

### Where we use it

Any class in the app can ask for its own `ILoggingService`, the same way it already asks for other
shared capabilities like `IAuthenticationService` or `IPasswordHasher`. That logger already knows
which class it belongs to, and automatically records which method a message came from the moment
it's written — so using it anywhere in the app takes no extra typing, and nothing has to be
repeated across what will eventually be hundreds of log calls.

## How it will be tested

Each of the four logging levels, the exact line format, the automatic class/method labeling, and
the exception-message-only rule all get automated tests. A further test confirms that a write
failure (for example, a missing folder or a full disk) is caught and never allowed to crash the
app.

# 4. Alternative implementations and designs

1. **Asking the caller to type both the class and method name on every single log call.**
   Rejected — it would work, but it puts a repeated, easy-to-get-wrong burden on every one of the
   (eventually hundreds of) log calls across the codebase, for something that can be handled once
   instead.
2. **Working out the class and method automatically at the moment each line is logged**, rather
   than labeling each class's logger up front. Considered and rejected: a large share of the app's
   code runs inside `async` methods, which get restructured behind the scenes by the compiler — so
   a technique that inspects "what's currently running" ends up reporting that restructured name
   instead of the real class and method most of the time.
3. **Using .NET's built-in logging interface (`ILogger`) instead of a custom one.** Considered and
   rejected. It doesn't give the exact log line format or the never-log-the-full-exception rule this
   feature needs, so custom code would be needed either way. What this design does borrow from it is
   the one idea worth keeping — a logger that already knows which class it belongs to.

# 5. Open issues

- **The log file grows forever.** Rotating, trimming, or protecting old log files is explicitly out
  of scope for this feature; a follow-on story should address it before this ships at scale.
