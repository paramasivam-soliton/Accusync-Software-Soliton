# ASWD-26 Logging: the basics — Features and User Stories

**For the JIRA board.** Epic → Feature → User Story. Every feature has a description and its own acceptance criteria; every story underneath has its own.

| | |
|---|---|
| Epic | **ASWD-26 Logging: the basics** |
| Features | **1** |
| User stories | **3** |
| Estimate | **4.7 developer-days** (4.0 coding days, plus testing, W8B8, review and management) |
| Requirements covered | **5** — GID-255031, GID-255032, GID-255033, GID-255034, GID-255038 |
| Split from | Rotation, retention and protection moved to **ASWD-31** on 25 Aug 2026 |
| Position in the plan | **4 of 29** — after Login, before Patient Management |
| Milestone | **M1** |
| Plan | [AccuSync-Delivery-Roadmap.md](../AccuSync-Delivery-Roadmap.md) |
| All feature criteria | [AccuSync-Feature-Acceptance-Criteria.md](../AccuSync-Feature-Acceptance-Criteria.md) |
| All epic criteria | [AccuSync-Epic-Acceptance-Criteria.md](../AccuSync-Epic-Acceptance-Criteria.md) |

---

## 1. How to read this

Three levels, each answering a different question.

| Level | Question | What you get here |
|---|---|---|
| **Feature** | Does this capability work? | A description, why it matters, and feature acceptance criteria |
| **Story** | Is this piece of work done? | The story, its acceptance criteria, and its requirement IDs |
| **Epic** | Is the whole area finished? | In the [epic criteria file](../AccuSync-Epic-Acceptance-Criteria.md) |

**A feature is done when all its stories are done *and* its own acceptance criteria hold.**

**Build in the order given.** LG-01 comes first because everything else writes through it.

Story and feature figures below are **coding days** — the input to the Task Breakdown sheet of `AccuSync-Roadmap.xlsx`. The **4.7 days** in the header is what comes back out of that sheet once testing, W8B8, review and management are added.

---

## 2. Where this epic stands today

**There is no logging in AccuSync at all.** Searching the solution for `ILogger`, `Serilog` and `NLog` returns **zero matches**. There is no log file, no log format and no way to find out what happened after a problem is reported.

| | |
|---|---|
| Logging framework | **none** |
| Log file | **none** |
| What exists instead | **163 `Debug.WriteLine` and `Console.WriteLine` calls** scattered through the code |
| Why that is not logging | `Debug.WriteLine` output only exists while a debugger is attached. On a hospital workstation it goes nowhere |

So this epic is not "configure a logger". It is introducing the mechanism, then going back over 163 existing call sites.

---

## 3. Why this epic comes early, and why it is only half of Logging

It used to sit at position 26, near the end. That meant every line written in epics 0 to 24 would have had no logging in it, and the last story would have gone back to retrofit the whole product.

**It was moved ahead of Patient Management on 21 August 2026, and split in two on 25 August 2026.** Everything built after it is written with logging already available.

**Why the split.** A logger has to exist before Patient Management starts. But **rolling files, keeping them a year and locking the folder down produce nothing anyone can see** — no screen, no demonstrable behaviour — so holding up visible work for them buys nothing. This epic is the minimum that lets development proceed properly: a working logger, the agreed format, the patient-data rule, and logging added to what is already built. The rest is **[ASWD-31](../EPIC%2031%20Logging%20Retention/LOGGING_RETENTION_STORIES.md)**, late in the plan next to Audit Trail, which needs the same file-handling approach.

**⚠ What that leaves open in the meantime.** Between this epic and ASWD-31, logs are written but never pruned. Harmless on a development or test machine; **it must not reach a customer that way.** ASWD-31 is deferred, not optional.

**What that asks of the other epics.** Every epic after position 4 writes its own log statements as it goes. That is a small cost per epic which is **not separately estimated** — it sits inside their existing figures.

---

## 4. Natus decisions — 22 August 2026

Haidee Kachniewicz answered every open question on both features. Recorded verbatim, because several of these are now the acceptance criteria.

**Feature 1**

| Question | Answer |
|---|---|
| What counts as patient-identifiable information? | *"All patient demographics should not be logged"* |
| What values can "status of the operation" take? | *"Debug, Info, Warning, Error. Success/Failure/Warning can be in the message."* |
| Should log messages be translated? | *"No"* |
| Where should the log file live? | *"%ProgramData%\\Natus\\AccuSync\\Logs"* |

**Answers that now belong to [ASWD-31](../EPIC%2031%20Logging%20Retention/LOGGING_RETENTION_STORIES.md)** — the GID-255037 rewording and the 366-day / daily / 5 MB retention rules — are recorded there rather than repeated here.

**Still relevant to this epic**

| Question | Answer |
|---|---|
| Log entry format | *"{date:yyyy-MM-dd HH:mm:ss} [{level/status}] {class-name}.{method}() {message}"* |
| Logging and audit separate? | Confirmed. *"audit trail should be under %ProgramData%\\Natus\\AccuSync\\auditTrail folder. Preferred format: {date: yyyy-MM-dd HH:mm:ss} {level} {user} {message}, level can be Info"* |

### 4.1 Three things these answers change

**GID-255037 becomes buildable.** The old wording — *"prevent deletion of log files by any user"* — was impossible. The new wording moves the obligation to the software: **AccuSync provides no function to delete or modify a log file.** That is both buildable and testable. Note it now covers **modify** as well as delete.

> **⚠ The requirement itself still has to be changed in Jama.** Haidee said *"please take a note of it."* Until GID-255037 is formally reworded, we are building to wording that does not yet exist in DOC-076814. **This needs the requirement updated before verification.**

**A fourth severity level was added.** GID-255032 requires informational, warning and error. Haidee's answer adds **Debug**. We will build four levels. Debug is beyond the requirement, so it needs to be covered by the reworded or an additional requirement, or recorded as an agreed extra.

**"Status" is satisfied by the level plus the message.** GID-255038 asks for timestamp, description and status on every entry. The agreed format carries the level in `[{level/status}]` and Success/Failure wording inside `{message}`. That is how we will close GID-255038 — recorded here so the traceability is explicit rather than assumed.

---

## 5. Features and user stories

### Feature F1 · Logging that works, and logging in the code already written

*4.0 coding days · 3 stories · milestone M1 · **requirements:** GID-255031, GID-255032, GID-255033, GID-255034, GID-255038*

**What it is.** The logging mechanism itself: an interface the rest of the application writes through, a file it writes to, four severity levels, the agreed entry format, and a hard rule that no patient demographic data ever reaches the file.

**Why it matters.** Everything after position 4 depends on this existing. It is also the only way anyone will diagnose a problem reported from a hospital.

**Feature acceptance criteria**

1. A log file is produced on a normal machine with **no debugger attached**.
2. Every entry follows the agreed format exactly: `{date:yyyy-MM-dd HH:mm:ss} [{level}] {class-name}.{method}() {message}`.
3. **Four severity levels** are produced and can be told apart: **Debug, Info, Warning, Error**.
4. **No patient demographic data appears in any log file.** Enforced by developer discipline plus the exception rule in LG-01 AC9. **Proven in Epic 2 F1**, which is the first point a patient record exists to test with.
5. Log messages are in **English only**, and are not part of the translated resource set.
6. Log files are written to **`%ProgramData%\Natus\AccuSync\Logs`**, with the path overridable in the config file.
7. No `Debug.WriteLine` or `Console.WriteLine` call remains as the only record of anything that matters.
8. Unit tests cover each severity level, the entry format, and the patient-data rule.

**User stories**

| Story | Title | Days |
|---|---|---|
| LG-01 | Create the logging interface and file writer | 1.8 |
| LG-02 | Replace the existing debug-print calls | 1.2 |
| LG-06 | Add logging to the Epic 0 and Epic 1 code | 1.0 |

#### LG-01 · Create the logging interface and file writer

*1.8 coding days · requirements: GID-255031, GID-255032, GID-255033, GID-255034, GID-255038*

**Story.** As a developer, I want a logging interface in `Core` with a file writer behind it, so that every part of the application can record what it did in a way that survives a restart.

**Acceptance criteria**

1. An `ILogger`-style interface lives in `AccuSync.Core`, so `Core` code can log without depending on any outer layer.
2. The writer is registered in dependency injection, and any class can take the interface as a constructor parameter.
3. **Four severity levels** are supported: **Debug, Info, Warning, Error** *(Natus, 22 Aug 2026 — GID-255032 requires only the last three; Debug is an agreed addition)*.
4. Every entry is written in exactly this format: **`{date:yyyy-MM-dd HH:mm:ss} [{level}] {class-name}.{method}() {message}`**. The calling class and method are captured automatically, not typed by the caller.
5. **GID-255038 is met by** the timestamp, the level in `[{level}]`, and the operation outcome stated in `{message}` as Success, Failure or Warning.
6. Messages are written in **English only** and are **not** added to the translated resource files.
7. Log files are written to **`%ProgramData%\Natus\AccuSync\Logs`**, creating the folder if absent, and the path can be overridden in the config file.
8. Writing works on a machine with **no debugger attached** and without administrator rights.
9. **Exception logging records the exception type and message, never `ex.ToString()`.** A full exception dump can carry EF Core SQL parameters — which means patient names, dates of birth and SSNs — into a plain-text file that nobody chose to log there. This is the one part of GID-255034 the logger itself has to enforce; the rest is developer discipline (see the note below).
10. A logging failure never crashes the application — if the file cannot be written, the application continues.
11. Unit tests cover all four levels, the exact format string, the automatic class/method capture, the write-failure path, and that a logged exception does not include its full `ToString()` output.

> **GID-255034 has no story of its own, deliberately.** *"All patient demographics should not be logged"* is a rule developers follow while writing each epic, not a feature to build — and **the test that proves it cannot be written here.** Logging is built at position 4; the patient record does not exist until position 5. So the proof lives in **Epic 2 F1**, where a populated patient record exists to test against, and the rule goes into the coding standard so it applies to every epic from Patient Management onward. What stays in this story is AC9 — the one leak discipline cannot prevent.

#### LG-02 · Replace the existing debug-print calls

*1.2 coding days · requirements: GID-255031, GID-255033*

**Story.** As a developer, I want the existing `Debug.WriteLine` and `Console.WriteLine` calls replaced with real logging, so that the diagnostics already written into the code actually reach a file.

**Acceptance criteria**

1. All 163 `Debug.WriteLine` and `Console.WriteLine` occurrences are reviewed. Each one is either converted to a log call at the right level, or deleted as noise — none is left as-is.
2. Anything reporting a failure becomes **Warning** or **Error**, not Info. Developer-only tracing becomes **Debug**.
3. A search of the solution for `Debug.WriteLine` and `Console.WriteLine` returns matches only inside test projects.
4. Converted messages keep enough context to be useful — which operation, and what failed.
5. **No converted message carries patient demographic data.** Several current calls print field values; those are rewritten, not copied.

> **Do this before Patient Management starts.** The 163 calls are in code that already exists. Every epic after position 4 writes proper log calls from the outset, so this number does not grow.

#### LG-06 · Add logging to the Epic 0 and Epic 1 code

*1.0 coding days · requirements: GID-255031, GID-255033*

**Story.** As a support engineer, I want the code written before this epic to log like everything else, so that there is no blind spot in the two epics that came first.

**Acceptance criteria**

1. **Epic 0 code** — startup, dependency injection, database creation and migration — logs what it did, and logs failures as **Error**.
2. **Epic 1 code** — login, logout, lockout — logs each outcome. A failed login is **Warning**; a lockout is **Error**.
3. **No password, password hash or session token is ever logged**, at any level.
4. Login logging records the username and the outcome, and nothing else about the person.
5. A fresh install can be traced from launch to first successful login using the log file alone.

> **Scope note.** This story covers Epic 0 and Epic 1 only, because they are the only epics built before Logging.
>
> **Audit trail stays separate — confirmed by Natus, 22 Aug 2026.** The audit trail is a different record in a different folder with a different format:
> - Folder: **`%ProgramData%\Natus\AccuSync\auditTrail`**
> - Format: **`{date: yyyy-MM-dd HH:mm:ss} {level} {user} {message}`**, level may be Info
>
> That belongs to **Epic 25**, not here. Logging a login does **not** satisfy GID-255023. The detail is recorded here because it arrived with these answers, and has been carried into Epic 25's criteria.

---

## 6. Feature summary

| Feature | Name | Stories | Days | Requirements |
|---|---|---|---|---|
| **F1** | Logging that works, and logging in the code already written | 3 | 4.0 | GID-255031, GID-255032, GID-255033, GID-255034, GID-255038 |
| | **1 feature** | **3** | **4.0** | **5 requirements** |

## 7. Story summary

| Story | Title | Feature | Days | Requirements |
|---|---|---|---|---|
| LG-01 | Create the logging interface and file writer | F1 | 1.8 | GID-255031, GID-255032, GID-255033, GID-255034, GID-255038 |
| LG-02 | Replace the existing debug-print calls | F1 | 1.2 | GID-255031, GID-255033 |
| LG-06 | Add logging to the Epic 0 and Epic 1 code | F1 | 1.0 | GID-255031, GID-255033 |
| | **3 stories** | **1** | **4.0** | **5 requirements** |

## 8. Things to settle

Every question we raised on 21 August was answered on 22 August. What remains is paperwork, not design. **The GID-255037 rewording and the retention rules moved to [ASWD-31](../EPIC%2031%20Logging%20Retention/LOGGING_RETENTION_STORIES.md)** along with the stories they belong to.

| # | What | Affects | Needed by |
|---|---|---|---|
| ~~Q-loc~~ | ~~Where does the log file live?~~ — **answered.** `%ProgramData%\Natus\AccuSync\Logs` | LG-01 | ✅ closed |
| ~~Q-aud~~ | ~~Do logging and audit stay separate?~~ — **answered.** Yes, separate folder and format, given in §4 | LG-06, Epic 25 | ✅ closed |
| **R2** | **Debug level needs a requirement, or an agreed exception.** GID-255032 lists only informational, warning and error. Natus asked for Debug as a fourth level | LG-01 verification | Before LG-01 is signed off |
| **R3** | **Confirm GID-255038 is met by level + message.** The agreed format carries status in `[{level}]` and in the message text rather than as a separate field. We are treating that as compliant | LG-01 verification | Before LG-01 is signed off |

## 9. What this epic depends on, and what depends on it

**Needs first**

| From | Why |
|---|---|
| Epic 0 · Revamp Code Base | Needs the `Core` project, dependency injection and the config file. The logger interface belongs in `Core`, which Epic 0 creates |

**Everything after position 4 depends on this**

| What | Why |
|---|---|
| Every epic from Patient Management onward | Each writes its own log calls as it goes, which is only possible because this epic lands first |
| **ASWD-31 Logging: retention and protection** | Nothing to roll or protect until this epic writes files |
| Epic 25 · Audit Trail | Shares the rolling, retention and protection approach — which is why it sits next to ASWD-31, not here. Its folder and format are already set, see §4 |
| Epic 30 · Installation | The installer must create the log folder and the config file must carry its path |

**A process point, not a story.** Once this epic lands, "did you log it?" belongs on the definition of done for every later story. Without that, the retrofit this move was meant to avoid comes back one epic at a time.
