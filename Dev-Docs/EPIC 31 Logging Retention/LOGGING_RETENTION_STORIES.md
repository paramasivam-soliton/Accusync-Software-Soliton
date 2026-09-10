# ASWD-31 Logging: retention and protection — Features and User Stories

**For the JIRA board.** Epic → Feature → User Story.

| | |
|---|---|
| Epic | **ASWD-31 Logging: retention and protection** ⚠ *needs a Jira epic — see §2* |
| Features | **1** |
| User stories | **2** |
| Estimate | **3.7 developer-days** (2.0 coding days, plus testing, W8B8, review and management) |
| Requirements covered | **3** — GID-255035, GID-255036, GID-255037 |
| Position in the plan | **late — immediately before Audit Trail** |
| Milestone | **M8** |
| Split from | **ASWD-26 Logging** on 25 August 2026 |
| Plan | [AccuSync-Delivery-Roadmap.md](../AccuSync-Delivery-Roadmap.md) |

---

## 1. Why this is a separate epic

Logging was one epic. On 25 August 2026 it was split in two, because the two halves have completely different urgency.

**The basics have to come first.** A logger has to exist before Patient Management starts, or every epic after it gets logging retrofitted at the end. That work is now **ASWD-26**, at position 4.

**This half has to come last.** Rolling a file at midnight, keeping it 366 days, and locking a folder down produce **nothing anyone can see**. There is no screen, no demonstrable behaviour, nothing to show in a progress review. Building it early would push back the work that *is* visible, and would reduce no risk — the logger it protects already exists from Epic 26.

So it sits next to **Epic 25 Audit Trail**, and that placement is deliberate rather than convenient: the audit trail needs the same rolling, retention and protection approach. Built together, the approach is designed once and shared. Built apart, it is designed twice.

**What is *not* deferred.** Logs are still written, still in the right format, still free of patient data, and still in `%ProgramData%\Natus\AccuSync\Logs` from Epic 26 onward. What is deferred is only what happens to the files over time.

---

## 2. Two things to settle before this starts

### 2.1 This epic needs a Jira number

`ASWD-31` is a placeholder we are using so the plan and the workbook reconcile. **Natus needs to either create the epic, or tell us to keep both halves under ASWD-26 with this as a second feature.** Either is fine — we need to know which before the board is built.

### 2.2 GID-255037 is being built to wording that is not in Jama yet

Natus agreed on 22 August 2026 to reword it:

> ~~*"The software shall prevent deletion of log files by any user."*~~ → **"The software shall not provide any user function to delete or modify log files."**

The old wording was impossible — a local Windows administrator can delete any file. The new wording is buildable and testable, and it covers **modify** as well as delete.

**The requirement itself has not been changed.** Haidee said *"please take a note of it."* Until it is updated, verification would trace to the old, impossible text. **This must be corrected in Jama before LG-05 can be signed off.**

---

## 3. Feature and user stories

### Feature F1 · Roll, retain and protect the log files

*2.0 coding days · 2 stories · milestone M8 · **requirements:** GID-255035, GID-255036, GID-255037*

**What it is.** Everything about the log files over time: rolling them daily and at 5 MB, naming them to a set pattern, keeping them 366 days, deleting the oldest beyond that, restricting who can read them, and making sure nothing in AccuSync can delete or change one.

**Why it matters.** A log that filled the disk and stopped writing, or that anyone can quietly edit, is worse than no log — it gives false assurance during an investigation.

**Feature acceptance criteria**

1. Log files roll **daily**, and also whenever the current file reaches **5 MB**.
2. File names follow **`accusync_log_{count}_{date:yyyy-MM-dd_HH-mm-ss}.log`**.
3. Files are retained **366 days**; anything older is deleted oldest-first, automatically.
4. An ordinary logged-in user cannot read the log folder (GID-255036).
5. **AccuSync provides no user function to delete or modify a log file** — no screen, menu item, button, service method or API (GID-255037 as reworded).
6. The behaviour is proven by test, including a full disk and a 367-day-old file.
7. The same rolling, retention and protection approach is used by **Epic 25 Audit Trail** — designed once, not twice.

**User stories**

| Story | Title | Days |
|---|---|---|
| LG-04 | Roll log files daily and at 5 MB, and keep them 366 days | 1.2 |
| LG-05 | Restrict read access, and provide no way to delete or modify a log | 0.8 |

#### LG-04 · Roll log files daily and at 5 MB, and keep them 366 days

*1.2 coding days · requirements: GID-255035*

**Story.** As a support engineer, I want log files rolled and pruned automatically, so that I can investigate a problem from months ago without the log folder filling the disk.

**Acceptance criteria**

1. A new log file starts **every day**, and also whenever the current file reaches **5 MB** — whichever comes first.
2. File names follow **`accusync_log_{count}_{date:yyyy-MM-dd_HH-mm-ss}.log`**, where `{count}` distinguishes multiple files from the same day.
3. Files are kept **366 days**, which satisfies GID-255035's one-year minimum with a day in hand.
4. A file older than 366 days is **deleted, oldest first**, automatically and without a user action.
5. Pruning works after the machine has been switched off — a workstation off for a month prunes correctly on next start, rather than only pruning while running.
6. If the disk is full or the folder is unwritable, the application keeps running and surfaces the problem rather than failing silently.
7. Tests cover the daily roll, the 5 MB roll, the `{count}` suffix, a 366-day-old file surviving, a 367-day-old file being deleted, and the disk-full path.

> **Every number here is Natus's**, set on 22 August 2026. Retention 366+, roll daily, 5 MB per file, and the file-name pattern. None of it is our choice, which is why the criteria are this specific.

#### LG-05 · Restrict read access, and provide no way to delete or modify a log

*0.8 coding days · requirements: GID-255036, GID-255037 (reworded)*

**Story.** As a data protection officer, I want log files protected from casual reading and from being changed through the application, so that they can be trusted.

**Acceptance criteria**

1. The log folder's permissions stop an ordinary logged-in user reading the files (GID-255036).
2. **AccuSync provides no user function to delete or modify a log file** — no screen, menu item, button, service method or API. Verified by code review as well as by test.
3. A code-level check confirms nothing in the solution opens a log file for writing except the logger's own append path.
4. If a log file is deleted or altered outside the application, the next write records that the sequence is broken, so the gap is visible.
5. Tests confirm an ordinary user cannot read the folder, that no public method exists to remove or rewrite a log, and that a missing file is detected on the next write.

> **⚠ Do not mark GID-255037 verified until Jama is updated** — see §2.2. The requirement in DOC-076814 still says *"prevent deletion by any user"*, which this story does not and cannot do.

---

## 4. Feature summary

| Feature | Name | Stories | Coding days | Requirements |
|---|---|---|---|---|
| **F1** | Roll, retain and protect the log files | 2 | 2.0 | GID-255035, GID-255036, GID-255037 |
| | **1 feature** | **2** | **2.0** | **3 requirements** |

## 5. Story summary

| Story | Title | Feature | Coding days | Requirements |
|---|---|---|---|---|
| LG-04 | Roll log files daily and at 5 MB, and keep them 366 days | F1 | 1.2 | GID-255035 |
| LG-05 | Restrict read access, and provide no way to delete or modify a log | F1 | 0.8 | GID-255036, GID-255037 |
| | **2 stories** | **1** | **2.0** | **3 requirements** |

## 6. Things to settle

| # | What | Blocks | When |
|---|---|---|---|
| **J1** | **This epic needs a Jira number**, or a decision to keep both halves under ASWD-26 | the board | Before the epic is created |
| **R1** | **GID-255037 must be reworded in Jama.** Natus agreed the wording on 22 Aug 2026 and asked us to note it | LG-05 sign-off | Before LG-05 is verified |

## 7. What this depends on, and what depends on it

**Needs first**

| From | Why |
|---|---|
| **ASWD-26 Logging: the basics** | There is nothing to roll or protect until the logger exists and is writing files |
| Epic 30 · Installation | The installer must create the log folder with the right permissions |

**What depends on this**

| What | Why |
|---|---|
| **Epic 25 · Audit Trail** | Reuses the same rolling, retention and protection approach. This is why the two sit together — the approach is designed once |

**A note on sequencing.** Between Epic 26 and this epic, logs are written but never pruned. On a development or test machine that is harmless. **It must not reach a customer that way**, so this epic has to land before release even though it is late in the order — it is deferred, not optional.
