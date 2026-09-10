# AccuSync — Delivery Plan

**Version:** v0.2 (rewritten to be simpler) · **Date:** 2026-08-11 · **Team:** 1 developer

| Where to look | For what |
|---|---|
| **This document** | The plan. Epics, features, user stories, estimates, order, timeline |
| [AccuSync-Analysis-Reference.md](AccuSync-Analysis-Reference.md) | The detail behind the plan: all 196 requirements, what the code is today, status of each requirement |
| **§9 of this document** | Extra features found in the code that are **not** in the requirements. These need a talk with the customer |

---

## 1. The short answer

We looked at every requirement in the SRS and compared it to the code.

| Question | Answer |
|---|---|
| How many requirements are there? | **196** |
| How many are finished? | **6** (all in Login) |
| How many are partly done? | **17** |
| How many have no code at all? | **164** |
| How much work is left? | **About 536 developer-days** |
| How long with 1 developer? | **About 148 weeks.** With the 20% buffer used up: **177 weeks** |
| Finish date | **2029-06-15** with one developer, or **2030-01-04** if the buffer is used |
| Customer target date | **Sep-2027** — achievable only with about **3 developers**, see §7.3.1 |

### Why it is so long

It is simple arithmetic. One developer produces about **195 usable days per year** (see §2.3). 575 days of work divided by 195 is 2.9 years.

The app *looks* nearly finished, but it is not. There are 46,000 lines of screens in `AccuSync.WPF`. Almost all of them are **empty shells**. They hold data in memory and lose it when you close the window. The code itself says so — for example `UsersContentView.xaml.cs` line 49:

```
// TODO: Replace hardcoded users with data from DatabaseService
```

There are **276** notes like that in the code. Also:

- **26 of the 28 database tables do not exist.** Only `Users` and `AppSettings` are real.
- **Three whole areas have zero code**: talking to the AccuScreen Pro device, audit trail, logging.
- **The solution does not build from a fresh copy.** We tested it. It fails.
- **There is no CI, and no installer.**

### What to do about the timeline

One developer cannot finish this in a normal timeframe. There are three real choices:

| Choice | Team | How long |
|---|---|---|
| **A.** Build everything, 1 developer | 1 | ~3.5 to 4 years |
| **B.** Build everything in 12 months | **4–5 developers** | 12 months |
| **C.** Split into Release 1 and Release 2, 1 developer | 1 | R1 in ~2 years (see §7.4) |

---

## 2. Decisions and ground rules

### 2.1 Decisions already made

| # | Decision | Effect on the plan |
|---|---|---|
| D1 | **Two databases** — a patient database and a settings database | Matches the SRS wording (GID-255042, GID-255043). Plan follows this |
| D2 | **Which table goes in which database is not decided yet** | To be discussed separately. See §8 open item O1. It does not block the first two epics |
| D3 | **Patient records use soft delete** | GID-254886 becomes "mark as deleted", not "remove row". Also keeps audit history later |
| D4 | **Build only what the requirement document asks for** | Extra features found in the code are listed in §9 for customer discussion. They are **not** in the 642 days. We read the code and found **74** of them, not the 11 first thought |
| D5 | **Tables and repositories are built inside the epic that needs them** | There is no separate "build all the tables first" epic |
| D6 | ~~Logging and audit trail come late~~ → **Logging splits in two: basics first, retention last** | **Revised 21 Aug 2026, split again 25 Aug 2026.** The logging **basics** sit at position 4, before Patient Management, so every later epic is written with logging available. **Rotation, retention and file protection** move to ASWD-31 late in the plan, next to Audit Trail, because they produce nothing a stakeholder can see and would only delay visible work. Audit Trail stays late |
| D7 | **Epic order follows the screenshot order** (ASWD-77, then ASWD-1 to ASWD-30) | See §7.1. Some dependency warnings in §7.2 |
| D8 | **Nothing is delivered to the customer until every requirement is shipped.** So there is no live database to upgrade | We stop treating database migration as a delivery problem. We can drop and rebuild the database, and regenerate the whole migration set, at any point during development. Saves ~4 days and removes a whole class of risk |
| D9 | **The installer script is created at delivery time** | Epic 30 stays last. We only need to prove a **fresh install** works — no upgrade-from-previous path to build or test |
| D10 | **Encryption covers the database *file* only, not the data inside it** | GID-256510 and GID-256511 are met by encrypting the two `.db` files. No column-level encryption of patient names, dates of birth, and so on |
| D11 | **The shared Add/Edit/Delete and Save/Revert/Undo framework is not a separate upfront epic.** It is built the first time it is needed — inside Patient Management — and every later epic adopts it | The old "Epic G" is dissolved. SRS §5.1's ten requirements are now delivered as a feature of Patient Management plus a small adoption story in each later screen epic |
| D12 | **System Configuration work belongs to the epic it configures, not to its own epic** | Risk Factors config, Comments config and Patient Field config become **features of Patient Management**. User & Profile config goes to ASWD-7/8, Site & Facility config to ASWD-10. ASWD-15, ASWD-16 and ASWD-17 stop being separate epics |
| D13 | **Login's four remaining items move into User Account Management** | Password rules, configurable strength, the 90-day-expiry decision and blocking deactivated-user transfer all sit with user accounts. ASWD-1 Login becomes a closure epic — its six requirements are already built |
| D26 | **All 14 languages ship in this release, and the strings are frozen before they are translated.** Natus’s distribution partners do the translating; we owe them a pack of every string with its screen and a screenshot | Natus (Haidee Kachniewicz), 27 Aug 2026. **Epic 19 goes from 3 features to 6** and is sliced: **19A** switching and the string pack (M6), **19B** the three language phases (M8). Chinese and Japanese support becomes 3.0 days of real work; Turkish adds culture-aware casing. **Phases 2 and 3 leave the cut list** — they are no longer optional |
| D25 | **The patient test report is one workflow, and it lives in Report Generation.** Epic 18 is restructured so every feature is something a person does: generate and print, save to a file, set the layout — with the library choice left as a timeboxed investigation | Lead, 27 Aug 2026. **Epic 2 F10 and PM-38 retire into Epic 18 F2**, which answers **Q5**: GID-254894 and GID-255112 describe the same report. Epic 2 goes from 10 features to 9; Epic 18 from 2 to 4. 28.5 coding days in, 28.0 out |
| D24 | **Import-fed lists are editable combo boxes, a typed value stays on that patient, and Race is one shared list.** A dropdown that is empty until someone runs an import is unusable on a fresh install, so the screener can always type what they know | Natus (Haidee Kachniewicz), 26 Aug 2026, answering all three questions. **Folded into PM-40 on 26 Aug 2026** — both field sets come from AccuLink, one from its field set and one from its data-exchange plugins, so they are one job: seven HiTrack and Australia fields plus Birth Location becoming a combo box. The shared lists are filled by **ASWD-32 F6**, the HiTrack pick-list import — sample files supplied. Neither story blocks the other, which is the point of the combo box. **Amended 27 Aug 2026: `IndigenousStatus` is an enum in code, not a seeded list, so it is a plain dropdown and a user cannot add a sixth value — this narrows what Natus approved and should be told to them** |
| D23 | **The database encryption approach is researched in week 3, not decided at install time.** A measured comparison of SQLCipher, the SQLite Encryption Extension, column-level encryption and OS-level encryption, with key management, backup and migration proven, ending in a written decision that later features design against | Code reviewer, 26 Aug 2026. **Epic 30 F4 added, 5.5 coding days, pulled forward beside the Epic 21 investigation.** Replaces the 1.5-day bullet inside Epic 0 F1, which stopped being visible when Epic 0 became a single hand-set line. **Encryption still ships in Epic 30 per D14** — only the decision moves earlier. If F4 concludes that development should run against the encrypted provider from Epic 2, F3’s regression pass shrinks and the plan is re-derived |
| D22 | **Role-based access control is a feature, and it is built before the epics that need it.** One authorization service, enforced in the UI *and* in the Application layer, with the 33 permissions written down as a matrix that is both the specification and the test oracle | Code reviewer, 26 Aug 2026. **Epic 8 F3 added, 15 coding days**, and F2 drops from 5 to 3.5 because enforcement moves out of it. **F3 is pulled forward to just before Epic 2**, because every epic after it needs gating and would otherwise inherit PM-21’s temporary role check. **This is X8z from §9.11 made real**, not an extra 14 days on top of it |
| D21 | **Release is an epic, not an assumption.** Integration testing, the system test pass against the SRS, requirement traceability, the IEC 62304 records, the risk file, the user manual and the release itself are now estimated work with an owner, rather than an excluded risk | Code reviewer, 26 Aug 2026. **ASWD-33 added** and needs a Jira number. **Scoped to two features the same day by the lead** — release testing and user documentation; the integration suite, traceability, the IEC 62304 records, the risk file and release engineering came out, and **R3 is reopened for them**. **Closes R3**, which carried this as *"not estimated, possibly +30–50%"* — the estimate lands inside that range. **The number assumes Soliton does all of it**; whatever Natus’s quality and regulatory function owns comes off. F1 and F3 are recommended to run continuously from Epic 2 rather than waiting for the end |
| D20 | **Import, Export and S4H become one Data Exchange epic, with one feature per format covering both directions.** A format is finished when it reads and writes, not when half of it is done. S4H joins them because AccuLink’s eSP connector *is* SMaRT4Hearing, and its two directions are a configuration download and a result upload | Lead, 25 Aug 2026. **ASWD-3, ASWD-4 and ASWD-20 retire; ASWD-32 replaces them** and needs a Jira number. 12 features become 10; the epic is delivered in two slices, **32A file formats (M5)** and **32B S4H (M7)**. Reading the AccuLink source removed the 3-day SEDQ discovery and unblocked the HiTrack and OZ file layouts — see [AccuLink-DataExchange-Analysis.md](AccuLink-DataExchange-Analysis.md). **The epic now runs after Epic 21**, because F10 needs the device and site identifiers Epic 21 establishes |
| D19 | **Field-bulk stories are sized from the field count, and are the one exception to the one-day sub-task cap.** Adding many fields is the same small job many times over, so neither the cap nor the reuse discount applies to it. About **2.5 hours a field** — entity, EF configuration and migration 1.0, DTO and converter 0.5, screen control and binding 1.0, save-and-reload test 0.5 — plus 2 hours a dropdown and 4 hours a new screen section | Lead, 25 Aug 2026. The cap had been charging **PM-39’s 19 fields a total of 8 hours**. PM-40 also drops the comparison and the Natus sign-off, which are not development work. **The lead then set the figures directly on 25 Aug 2026: PM-39 46h, PM-40 32h**, about 1.7 and 2.1 hours a field. Where hours are set by hand the story’s day figure no longer drives them — same convention as Epic 0 |
| D18 | **User stories are workflow-shaped, not layer-shaped.** A story covers one workflow end to end — its screen, its service and its repository work together. Setting up an entity, EF Core mapping and a DbContext may still be a story of its own; nothing else may be. Shared shapes are built by the first workflow that needs them and reused by the rest, so the second and third workflow in a feature cost less | Lead, 25 Aug 2026. **PM-02 Build the patient presentation layer** folded into **PM-03 Add a new patient**; **PM-17 Build the validation engine** folded into **PM-18**, now *Validate a patient record and show what is wrong*. Epic 2 goes from 40 stories to 38. No other epic had a layer-shaped story — checked all 26 |
| D17 | **The workbook is the tracker; the plan is the reasoning.** The customer removed the estimate, requirement-count and computed-date columns from the workbook, and consolidated ASWD-77 into one feature | The workbook now has four sheets (README, Milestone, Epics, Features) and carries only target dates. Day estimates stay in this document. ASWD-77 goes from 5 features to 1; the 14 stories underneath it are unchanged |
| D16 | **Eight milestones, using the customer's grouping and names.** Administration also moves ahead of Import and Export | The 11 milestones become 8, grouped as the customer set them out. Help stays in the final milestone. Cost-neutral — nothing changes size, only order and grouping. **The customer's target dates are carried alongside the computed ones; see §7.3.1 — they imply about 3 developers, not 1** |
| D15 | **Test result views are built before Import and Export**, because they give stakeholders something visible sooner | ASWD-5 and ASWD-6 move ahead of the exchange work (then ASWD-3 and ASWD-4, now ASWD-32). Cost-neutral — the two blocks are the same size, so nothing downstream moves. Adds a half-day story to ASWD-5 so the views can be shown on real screening data rather than invented data |
| D14 | **ASWD-29 Database Storage is dissolved. Which database each table goes in is decided while building the feature that adds it. Encrypting the database files, the network check and the Windows 11 check all move into Installation** | Three more epics stop existing as work: ASWD-22, ASWD-23 and ASWD-29. Installation grows from 15.0 to 27.0 days and now owns 10 requirements. Total drops from 640.0 to 636.5 |

**What D11 and D12 mean in practice.** Both push work *into* the epic that needs it rather than doing it up front. That is the same principle as D5 (tables built inside their epic). The trade-off is honest and worth stating:

| Gain | Cost |
|---|---|
| Nothing is built speculatively. The framework is shaped by a real screen before it is applied to eleven more | The framework's first design will be patient-shaped. Applying it to configuration screens may need adjustment — budgeted as a small adoption story per epic |
| Configuration lands next to the thing it configures, so both are tested together | **Patient Management becomes a very large epic — 171.0 days.** See the warning in §6 |
| Four fewer epics to coordinate | ASWD-15, ASWD-16 and ASWD-17 exist in Jira and now have no work of their own. They need closing or relabelling — a Jira housekeeping item |

**Important distinction on D10, so it is not confused later.** There are two different encryption requirements and only one of them changes:

| Requirement | What it asks for | Effect of D10 |
|---|---|---|
| GID-256510, GID-256511 | Encrypt the patient database and the settings database, minimum AES-256 | **File-level only.** One encrypted database file each. This is what D10 settles |
| GID-255018 | Hash all login passwords and **encrypt all usernames stored in the system** | **Unchanged.** This is a separate requirement about specific fields. It is already built and tested (`PasswordHasher.cs`, `EncryptionService.cs`) and it stays as it is |

So the username field stays individually encrypted because GID-255018 demands it — not because of the database-encryption requirement.

### 2.2 Things we had to assume

Nobody gave us these, so we picked a value. Please correct any that are wrong.

| # | Item | We assumed |
|---|---|---|
| A1 | Start date | **Monday 2026-08-17** |
| A2 | Deadline | **None given** |
| A3 | Who reviews PRs | Tech lead, part-time |
| A4 | How long a PR review takes | **1.5 working days** |
| A5 | How long an HLD sign-off takes | **2 working days** |
| A6 | Holidays and leave | **None counted.** This is wrong and needs the real calendar |
| A7 | Unit test target | **80% on new code** in Core, Application, EF, Presentation. Screens excluded |
| A8 | Buffer | **20%**, kept as a separate reserve |
| A9 | Usable days per week | **3.75 of 5** (see §2.3) |
| A10 | One HLD per epic (not per story) | Matches how the team already works |
| A11 | One PR per user story | Matches the current branch naming |

### 2.3 Why we plan on 3.75 days a week, not 5

A developer does not get 5 clean coding days. From the project's own git history (29 July to 10 August 2026, 9 working days, 82 commits):

- About **1.8 days** went on answering review comments. Commits like `fix: Address PR Comments` and `doc: updated spec based on review` show this.
- One commit is `PR 1-4 Changes` — four pull requests were reviewed together. So the developer had to go back into four "finished" stories and reload them mentally.
- Meetings, support and interruptions do not show up in git, but they are real.

So we plan on **3.75 days a week = 75%**. Over a year that is about **195 days**.

### 2.4 What every estimate includes

No estimate is "just coding". Every one covers all nine steps:

| Step | Where it sits | How we size it |
|---|---|---|
| 1. Write the HLD | Once per epic | 0.5 to 4 days |
| 2. HLD review and sign-off | Once per epic | 2 days waiting + up to 1 day answering comments |
| 3. Write the code | Per user story | 0.5 to 3 days |
| 4. Write unit tests | Per user story | **0.4 × coding time** |
| 5. Self-review | Per user story | **0.1 × coding time** |
| 6. Raise the PR | Per user story | Included in self-review |
| 7. Wait for PR review | Per user story | 1.5 days waiting |
| 8. Fix review comments | Per user story | **0.25 × coding time** |
| 9. Merge | Per user story | 0.05 days |

**About the waiting time.** Waiting is not developer effort. While story A sits in review, the developer starts story B. So most waiting does not add to the calendar. It only adds time in three places:

- Week 1, when there is nothing else to work on yet
- At the end of each milestone, when the last PR must merge before we can say the milestone is done
- When an HLD sign-off blocks the next epic and nothing else is ready

That comes to about **26 extra days (5 weeks)** across the whole plan. It is included.

---

## 3. How the work is organised

Three levels, as you asked:

```
EPIC          A section of the requirement document. One of the 31 in the screenshots.
  └─ FEATURE  One useful capability inside that epic.
       └─ USER STORY  One piece of work. 0.5 to 3 days of coding. One PR.
```

**Example — Epic ASWD-2 Patient Management:**

| Level | Item |
|---|---|
| Epic | Patient Management |
| Feature 1 | Patient record CRUD |
| Feature 2 | Risk factors in the patient record |
| Feature 3 | Comments in the patient record |
| Feature 4 | Patient list view |
| ... | |
| Story inside Feature 1 | Create the Patients table and repository |
| Story inside Feature 1 | Add a new patient |
| Story inside Feature 1 | Edit a patient |

**Where do tables come from?** Inside the feature that needs them. The `Patients` table is a user story inside Patient Management. The `Devices` table is a user story inside Device Management. And so on. There is no separate database epic.

**One exception.** The shared plumbing that *all* epics use — the base classes, the two DbContexts, the repository pattern, encryption — is built once in Epic 0. Otherwise every epic would build its own.

---

## 4. Two epics are missing from Jira

The screenshots show ASWD-77 and ASWD-1 to ASWD-30. Comparing that list to the SRS, **two sections have no epic**:

| SRS section | Requirements | Where it now lives |
|---|---|---|
| **§5.1 General** | GID-254873 to GID-254882 (**10 requirements**) — Add/Edit/Delete and Save/Revert/Undo, on the 12 screens the requirements name | **Distributed, per D11.** Built once as Patient Management **F2**, then adopted by a small story in each later screen epic. No new Jira epic needed |
| **§5.31 Firmware Update** | GID-255005 (**1 requirement**) | **Inside ASWD-21** (Device Communication) as F5, since it needs the same USB connection. Still needs confirming |

**And three Jira epics now have no work of their own** (per D12), because their content moved into the epic it configures:

| Jira epic | Where its work went |
|---|---|
| **ASWD-15** Risk Factors Configuration | Patient Management, feature F8 |
| **ASWD-16** Comments Configuration | Patient Management, feature F9 |
| **ASWD-17** Patient Field Configuration | Patient Management, features F5 and F6 |

These three should be closed, or relabelled as pointers to ASWD-2. That is a Jira housekeeping decision, not a planning one — but it needs doing, or the same work will look unplanned in one place and duplicated in another.

**ASWD-1 Login** also changes: its six built requirements stay, and its four remaining items move to ASWD-7 (per D13). It becomes a 1-day closure epic.

---

## 5. Epic summary — in screenshot order

| # | Epic | Reqs | Features | Stories | Days |
|---|---|---|---|---|---|
| 0 | **ASWD-77 Revamp Code Base** | — | 1 | 14 | **9.1** |
| 1 | **ASWD-1 Login** — closure only | 6 *(all done; 2 moved to ASWD-7)* | 1 | 1 | **8.9** |
| 2 | **ASWD-2 Patient Management** ⚠ *very large — see §6* | 44 † | 9 | 37 | **96.1** |
| 3 | **ASWD-32 Data Exchange: import, export and S4H** ⚠ *needs a Jira epic* | 42 | 10 | 35 | **81.9** |
| 5 | **ASWD-5 OAE Test Result** | 2 | 3 | 5 | **13.1** |
| 6 | **ASWD-6 ABR Test Result** | 1 | 1 | 2 | **8.5** |
| 7 | **ASWD-7 User Account Management** | 11 | 2 | 13 | **28.8** |
| 8 | **ASWD-8 Profile Management** | 6 | 3 | 13 | **28.5** |
| 9 | **ASWD-9 Device Management** | 8 | 3 | 8 | **20.8** |
| 10 | **ASWD-10 Site Management** | 5 | 2 | 5 | **9.5** |
| 11 | **ASWD-11 Facility Management** | 5 | 1 | 4 | **7.2** |
| 12 | **ASWD-12 Location Management** | 6 | 1 | 4 | **7.2** |
| 13 | **ASWD-13 ABR Test Protocol Config** | 4 | 1 | 3 | **7** |
| 14 | **ASWD-14 DPOAE Test Protocol Config** | 4 | 1 | 3 | **8.5** |
| ~~15~~ | ~~ASWD-15 Risk Factors Config~~ → **ASWD-2 F8** | — | — | — | *moved* |
| ~~16~~ | ~~ASWD-16 Comments Config~~ → **ASWD-2 F9** | — | — | — | *moved* |
| ~~17~~ | ~~ASWD-17 Patient Field Config~~ → **ASWD-2 F5, F6** | — | — | — | *moved* |
| 18 | **ASWD-18 Report Generation** | 4 † | 4 | 9 | **19.2** |
| 19 | **ASWD-19 Language** | 5 | 6 | 14 | **35.4** |
| 21 | **ASWD-21 Device Communication** *(+ Firmware)* | 12 † | 5 | 13 | **41.1** |
| 24 | **ASWD-24 Alarms & Messages** | 7 † | 1 | 7 | **8.9** |
| 25 | **ASWD-25 Audit Trail** | 8 | 2 | 7 | **14.2** |
| 26 | **ASWD-26 Logging: the basics** ← *position 4* | 5 | 1 | 3 | **3.8** |
| 31 | **ASWD-31 Logging: retention and protection** ⚠ *needs a Jira epic* | 3 | 1 | 2 | **3.2** |
| 27 | **ASWD-27 About** | 1 | 1 | 1 | **2.5** |
| 28 | **ASWD-28 Help** | 2 | 1 | 2 | **5.3** |
| ~~22~~ | ~~ASWD-22 Network~~ → **ASWD-30 F3** | — | — | — | *moved* |
| ~~23~~ | ~~ASWD-23 Operating System~~ → **ASWD-30 F2** | — | — | — | *moved* |
| ~~29~~ | ~~ASWD-29 Database Storage~~ → **ASWD-30 F3** + per-feature | — | — | — | *moved* |
| 30 | **ASWD-30 Installation** | 10 | 4 | 13 | **34.9** |
| 33 | **ASWD-33 Release: release testing and documentation** ⚠ *needs a Jira epic* | — | 2 | 6 | **32.5** |
| | **Total — 25 epics** | **196** | **67** | **224** | **536.1** |

**What changed from the previous version, and why the total barely moved.** D11, D12 and D13 move work between epics; they do not remove much of it.

| | Days |
|---|---|
| Total before D11–D13 | 642.0 |
| D11–D13: Epic G, ASWD-15, ASWD-16, ASWD-17 and Login's items moved into the epics that need them | −2.0 |
| D14: ASWD-22, ASWD-23 and ASWD-29 moved into Installation; encryption moved out of Epic 0 | −3.5 |
| D15: test-data seeding story added to ASWD-5 so the result views can be demonstrated on real data | +1.0 |
| **New total** | **574.9** |

The 2-day saving is real but small: building the shared framework inside a real screen is slightly cheaper than building it speculatively, and folding configuration into its parent epic removes four epic-level HLDs. What it mainly buys is **less speculative work and better-tested configuration**, not a shorter schedule.

† **The Reqs column adds up to 200, not 196.** That is correct, not an error. Four requirements are delivered by two epics, so they are counted twice:

| Requirement | Owned by | Also counted in |
|---|---|---|
| GID-255010 Connection status indicators | Epic 21 (needs the USB connection) | Epic 24 |
| GID-255011 Firmware update warnings | Epic 21 (needs the firmware feature) | Epic 24 |
| GID-255012 Validation error messages | Epic 2 F5 (needs the validation engine) | Epic 24 |
| GID-254965 Report layout settings | Epic 18 (needs the report renderer) | Epic 2 |

Counting each requirement once gives **196**. This is verified by script, not by hand — see §12.

**Epic 2's 44 requirements**, after the D11/D12 merges: 15 from §5.3 Patient Management + 10 from §5.1 General (the shared framework) + 5 from §5.16 Risk Factors + 5 from §5.17 Comments + 8 from §5.18 Patient Field Config + GID-255012. Two of those 44 are also counted elsewhere — GID-254965 in Epic 18, GID-255012 in Epic 24.

**Epic 1's 6**: the six already built. **Epic 7's 11**: 9 from §5.8 User Account Management, plus GID-255020 and GID-255022 moved across from Login. Note that Epic 7's fourth moved item — the 90-day password expiry decision (F7) — **is not a requirement at all.** It is unrequested code, item X4a in §9, so it adds work but no requirement count.

**Confidence in these numbers:**

| Level | Epics | Days | Why |
|---|---|---|---|
| **Good** | 0, 1, 2, 10, 11, 12 | 210.5 | Requirements are clear, screens exist, and Patient Management already has a written story breakdown in `Dev-Docs/` |
| **Fair** | 3, 5, 6, 7, 8, 9, 13, 14, 24, 25, 26, 27 | 186.5 | Ordinary work, few unknowns |
| **Weak** | 4, 18, 19, 28, 30 | 127.0 | Missing information — see §8 |
| **Very weak** | 20, 21 | 113.5 | No device protocol, no S4H contract. **These are guesses until we investigate** |

**38% of the estimate is weak or very weak.** Almost all of that is because information is missing, not because the work is technically hard. Answering the questions in §8 would fix most of it and costs no developer time.

**Confidence is not the same as risk.** Epic 2 is scored Good — its requirements are clear and a story breakdown already exists — but at 171.0 days it is now nearly a quarter of the whole project in one epic. Being confident about each of its 39 stories is not the same as being confident about a 40-week run without a delivery checkpoint. §6 sets out how to slice it.

---

## 6. The epics in detail

Legend: 🔴 = blocked, needs an answer from §8 · ✅ = already done · **P** = builds database tables

### Epic 0 · ASWD-77 — Revamp Code Base — 9.1 days

**Goal:** get the solution building and testable under the new architecture, and build the shared parts every other epic depends on.
**Touches:** whole solution. **State: in progress.**

| Feature | User stories | Days |
|---|---|---|
| **F1 · Fix the clean-copy build, merge the branches and set up CI** **P** | Fix the clean-copy build failure *(0.5)* · Regenerate the migrations so a fresh install works *(1.0)* · Merge all 12 branches into `main` *(1.5)* · Set up CI: build + test + coverage *(2.0)* · Base classes: `ObservableObject`, `ViewModelBase`, `ValidatableViewModelBase`, `AsyncRelayCommand`, `Result<T>` *(2.0)* · Navigation, dialog and file-picker services *(2.0)* · Move the existing windows onto the new navigation *(1.5)* · Generic Host startup and move DI out of `App.xaml.cs` *(1.0)* · Release build settings and version numbers, and remove the dev-mode login bypass from Release *(1.5)* · Write down the design info held only in the two `.sql` files, before they are deleted *(2.0)* · Repository and unit-of-work pattern, shared base *(2.0)* · Two DbContexts wired up: Patient and Settings *(3.0)* · Encryption approach — **now researched properly in [Epic 30 F4](#epic-30), pulled forward to week 3** *(0.0)* · Move the parsing interfaces into `Core` and point `DataParser` at `Core` instead of `Application` *(1.5)* | 23 |
| | **Coding days total** | **23** |

**This epic is now tracked as one feature.** It was previously split into five (build, MVVM foundation, startup and configuration, shared persistence, layering fix). The customer consolidated it into a single item covering the whole revamp, because the work is already under way as one effort rather than five separable pieces. **The work has not changed — only how it is tracked.** The 14 stories above are the same 14 that were in F1 to F5.

**⚠ One number needs confirming — see the note at the end of this section.**

**Why this epic is first.** We ran `dotnet build` on a fresh copy. It fails:

```
error MSB3030: Could not copy the file "...\eng.traineddata" because it was not found.
Build FAILED.
```

The file is in `.gitignore` but the project file requires it. Also, the three migrations run in the wrong order, so a **new** database fails to create. `UserRepository.cs` line 73 runs migrations on every startup, so this breaks any new machine. Nothing else can be trusted until both are fixed.

**On the persistence work.** We build the two DbContexts here, but **not the tables**. Tables belong to their own epics (D5). We only need to know *which context* each table goes into — that is open item **O1** in §8.

**Encryption is no longer in this epic.** D10 (file-level only) and D14 (encryption moves to Installation) took it out. What stays here is the *investigation* that chooses the approach, because that decision constrains how the two DbContexts are registered.

**One thing D8 does *not* remove.** The migration order still has to be fixed. Today the three migrations run in the wrong sequence, so creating a **brand new** database fails. That breaks every new developer machine and every CI run. What D8 removes is the harder problem — safely moving an existing customer's database forward without losing data.

**⚠ This epic is carried at 20 development hours — 4.1 days — set by hand.** On 25 Aug 2026 the lead instructed that Epic 0 appear in the Task Breakdown sheet as **one line**: *Architecture revamp with MVVM + Clean Architecture, Implementation, 20 hours*. Every Epic 0 figure in §5, §7 and the milestones now comes from that single line. **The 14 items and their 23.0 coding days in the table above are unchanged** — they remain the record of what the work actually is, and they no longer feed the estimate. Before this it was 44.0 days hand-set, then briefly 42.1 derived from the 14 items. The earlier reasoning stood on the assumption that consolidating five features into one changed the tracking and not the work. If instead some of that work is **already finished** — the six-project split, EF Core persistence and the DataParser extraction are all visible in the git history — then this epic is much smaller, perhaps 9 to 12 days, and everything after it pulls in by about **nine weeks**. This is the single biggest open number in the plan right now and it needs one answer: *how much of the revamp is done?*

---

### Epic 1 · ASWD-1 — Login — 8.9 days (closure only)

**Goal:** confirm the six built Login requirements really are complete once everything is merged, and record the traceability.
**Requirements:** 8 total. **6 are built.** The other 2 moved to Epic 7 (per D13).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Re-verify login after the branch merge and record traceability** | Re-verify the six requirements after the branch merge, record requirement traceability, confirm the 103 tests still pass | 1 |
| | **Coding days total** | **1** |

**What is already built and working:**

| GID | What | Evidence |
|---|---|---|
| GID-255015 | Login with username and password | `AuthenticationService.cs:41-164`, 16 tests |
| GID-255016 | Logout | `App.Logout()` → `ICurrentUserContext.SignOut()`, 4 tests |
| GID-255017 | Lock after 5 failed attempts, admin-configurable duration | `AuthenticationService.cs:29`, `:77-99` |
| GID-255018 | Passwords hashed (PBKDF2-HMAC-SHA256, 210,000 iterations), usernames encrypted | `PasswordHasher.cs`, `EncryptionService.cs`, 11 tests |
| GID-255019 | Admin and Screener roles | `UserRoleParser`, `UserPermissionsViewModel`, 7 tests |
| GID-255021 | Deactivated users cannot log in | `AuthenticationService.cs:66-73` |

This is good work and it is genuinely finished — 103 tests pass. The only reason this epic exists at all is that **none of it is on `main` yet** (it sits on a branch stack, 81 commits ahead), so it must be re-verified after Epic 0 F1 merges it.

**What moved to Epic 7** (D13): password rules everywhere, configurable password strength, the 90-day-expiry decision, and blocking transfer of deactivated users to a device. All four are user-account concerns and sit better next to the user administration screens.

---

### Epic 26 · ASWD-26 — Logging: the basics — 3.8 days

**Requirements:** GID-255031, GID-255032, GID-255033, GID-255034, GID-255038 (5).
**Position 4, before Patient Management.** **Split from the retention and protection work on 25 Aug 2026** — see below.

| Feature | User stories | Days |
|---|---|---|
| **F1 · Logging that works, and logging in the code already written** | Logging interface in `Core`, file output, four levels (Debug / Info / Warning / Error), the agreed entry format, written to `%ProgramData%\Natus\AccuSync\Logs`, with exception logging that records type and message but never `ex.ToString()` (GID-255031, GID-255032, GID-255033, GID-255034, GID-255038) · Convert the existing `Debug.WriteLine` and `Console.WriteLine` calls · Add logging to the Epic 0 and Epic 1 code already written | 4.0 |
| | **Coding days total** | **4** |

**Why this is split, and why this half comes first.** Logging has to exist before Patient Management starts, or every later epic gets it retrofitted. But **rotation, retention and file protection produce nothing anyone can see** — no screen, no demonstrable behaviour — and holding up the start of visible work for them buys nothing. So this epic is the minimum that lets development proceed properly: a working logger, the agreed format, the patient-data rule, and logging added to what is already built. Retention and protection follow in **Epic 31**, alongside Audit Trail, which needs the same file-handling approach.

**Everything here was settled by Natus on 22 Aug 2026.**

- **Format**: `{date:yyyy-MM-dd HH:mm:ss} [{level}] {class-name}.{method}() {message}`
- **Four levels** including **Debug** — one more than GID-255032 requires
- **Location**: `%ProgramData%\Natus\AccuSync\Logs`, overridable in the config file
- **Patient data**: *"all patient demographics should not be logged"* — the whole set, not a subset
- **English only**, not part of the translated resource set

Story-level detail is in [EPIC 26 Logging/LOGGING_STORIES.md](EPIC%2026%20Logging/LOGGING_STORIES.md).

**What this asks of the other epics.** Every epic after position 4 writes its own log statements as it goes. That is a small cost per epic which is **not separately estimated** — it sits inside their existing figures.

### Epic 2 · ASWD-2 — Patient Management — 96.1 days ⚠ very large

**Goal:** the full patient record, everything that configures it, and the shared data-management framework the rest of the product will reuse.
**Requirements:** 39 — §5.3 Patient Management (15), §5.1 General (10), §5.16 Risk Factors (5), §5.17 Comments (5), §5.18 Patient Field Config (8), less 2 counted in other epics.
**User stories:** all 38, with acceptance criteria ready for the JIRA board, are in **[EPIC 2 Patient Management/PATIENT_MANAGEMENT_STORIES.md](EPIC%202%20Patient%20Management/PATIENT_MANAGEMENT_STORIES.md)**.

#### ⚠ Read this first: this epic is now a fifth of the project

At 171.0 days it runs about **46 weeks**. That is too long to go without a delivery checkpoint, and it is a direct consequence of D11 and D12 — the shared framework and three configuration areas all landed here because this is the first epic that needs them.

**We recommend delivering it in four slices**, each ending in something demonstrable. This changes nothing in Jira — it stays one epic — but it gives four checkpoints instead of one.

| Slice | Features | Coding | Total | Demonstrable at the end |
|---|---|---|---|---|
| **2A · Patient records exist** | F1, F2 | 30.7 | **30.7** | A patient can be created, viewed, edited and soft-deleted, and **survives a restart**. Add/Edit/Delete and Save/Revert/Undo work |
| **2B · The list is usable** | F3, F4, F5 | 26.6 | **26.6** | Patient list with the required columns, search, the test list, mandatory-field validation, and permissions enforced |
| **2C · Configuration** | F6 | 12.9 | **12.9** | An administrator can configure the patient ID format, list sorting, confirmation prompts, custom field labels and which fields are mandatory |
| **2D · Risk factors and comments** | F7, F8, F9 | 25.9 | **25.9** | Risk factors and comments are administrator-managed lists with translations, and can be assigned to a patient. Patient report prints |
| | | **96.1** | **96.1** | |

**Slices 2A and 2B complete milestone M1; slices 2C and 2D are milestone M2** (§7.3), so the epic has four internal checkpoints rather than one 40-week run.

**Correction made while writing the user stories.** The field setup tables and the validation engine were originally in slice 2C, but the mandatory-field check in slice 2B calls the validation engine, and the engine reads the field setup tables. All three are now one feature, **F5**, inside slice 2B. Building 2B first would have meant writing a throwaway validation check and replacing it a slice later. **F14 and F19 moved into 2B, ahead of F7.** Slice sizes changed; the epic total did not. M1 moves out by two weeks; nothing after M2 changes.

The full story-by-story breakdown, with acceptance criteria for the JIRA board, is in **[EPIC 2 Patient Management/PATIENT_MANAGEMENT_STORIES.md](EPIC%202%20Patient%20Management/PATIENT_MANAGEMENT_STORIES.md)** — 39 stories, PM-01 to PM-39, in build order.

**How Epic 2's figure is made up.** It is no longer the old HLD-plus-multiplier calculation. Every figure now comes from the Task Breakdown sheet: each sub-task's **Development Hours (design + implementation + unit test)**, plus testing at 0.1, **W8B8 at 1.2**, review at 0.1 and management at 0.05, divided by 8 hours a day. The sheet is the audit trail — every story broken into sub-tasks, each costed and rolled up. **W8B8 was cut from 1.3 to 1.2 by the lead**, at the same time as the hours were recalibrated.

#### Features

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete a patient record** **P** | Create the `Patients` and `PatientContacts` tables, repository and service · Add a new patient (GID-254883) · View a patient (GID-254891) · Edit a patient (GID-254885) · **Soft-delete** a patient (GID-254886) · **The 19 ALGO device fields** the ALGO 5 / ALGO Pro devices send that AccuSync cannot store, plus the **Physician / Pediatrician** relabel that fixes a live import defect, and Medication becomes multi-line. **Scope confirmed by Natus 18 Aug 2026** — the baby’s own address and phone were dropped · **The AccuLink fields AccuSync is missing** — **22 fields plus Birth Location becoming a combo box**, from two places and both of them AccuLink: its own field set (six confirmed, the rest from the alignment the lead is running with Natus) and its **HiTrack and Australia data-exchange plugins** (seven fields, fully enumerated). **Combo boxes rather than plain dropdowns, typed values saved on the patient only, one shared Race list — all three confirmed by Natus 26 Aug 2026.** The comparison and the Natus sign-off are not development work and are not costed · | 30 |
| **F2 · Shared Add/Edit/Delete and Save/Revert/Undo framework** ← *first build, per D11* | Shared Add/Edit/Delete commands and the delete-confirmation prompt (GID-254873–254877) · Shared change-tracking and undo stack for Save/Revert/Undo (GID-254878–254882) · Warn before closing or navigating away with unsaved changes (GID-255009) | 6.5 |
| **F3 · Show and search the patient list** | Show the patient list with the 6 required columns (GID-254884, GID-254890) · **Build-your-own filter across several fields at once** — Phase 1 per Natus 19 Aug 2026, *needs a new requirement* · Search by patient ID, first name, last name, date of birth, or test date range (GID-254887) | 5 |
| **F4 · Show, delete and reassign test results, with permissions enforced** **P** | Create the `TestSessions` and `TestRecords` tables · Show the test list with the 7 required columns (GID-254897) · Delete one test entry, admin only (GID-254895) · Move a test result to the correct patient (GID-254896) · Enforce "unless locked by permissions" on edit · Limit test delete and test reassignment to administrators. **Patient delete is open to every user**, by customer decision | 11.5 |
| **F5 · Store the field setup, validate input and enforce mandatory fields** **P** ← *was ASWD-17* ← *was ASWD-17* | Create the `FieldSetup` and `SystemSettings` tables · Show which fields are mandatory (GID-254892) · Only allow save when mandatory fields are filled (GID-254893) · Validate input and block saving bad data (GID-254966) · Show which field is wrong and what to fix (GID-255012) | 6.5 |
| **F6 · Configure patient fields, ID format, list sorting and prompts** ← *was ASWD-17* | Choose which patient fields are mandatory and which are shown (GID-254961, GID-254962) · Configure the patient ID format check (GID-254960) · Configure how the patient list is sorted (GID-254963) · Turn on or off: confirm on save, confirm on delete, warn on change (GID-254964) · **Rename patient field labels — all 62 fields**, not only the 4 spare "Available Field" slots (GID-255466 covers the 4; the other 58 carry over from AccuLink, confirmed by Natus 19 Aug 2026) · **Patient QR code, and the "include in QR" choice per field** — Phase 1 per Natus 19 Aug 2026, *needs a new requirement* | 11 |
| **F7 · Set a patient's risk factors and comments** | Set risk factors with Yes / No / Unknown (GID-254888) — **read DF3 warning below** · Assign a predefined comment, or write a patient-specific one (GID-254889) | 4.5 |
| **F8 · Manage and translate the risk factor list** **P** ← *was ASWD-15* | Create the `RiskFactors` table · Create, view, edit and delete risk factors, blocking edit and delete when already used by a patient (GID-254950–254953) · Create `RiskFactorTranslations` · Enter translated name and description per language (GID-254954) — *sized by Q1* | 5.5 |
| **F9 · Manage and translate the comment list** **P** ← *was ASWD-16* | Create `PredefinedComments` · Create, view, edit and delete comments, blocking delete when already used (GID-254955–254958) · Create `PredefinedCommentTranslations` · Enter translated text per language (GID-254959) — *sized by Q1* | 5.5 |
| | **Coding days total** | **85** |


**⚠ F3 and F6 carried more days than they have stories for.** This table used to show **F3 at 11** and **F6 at 17**, but [PATIENT_MANAGEMENT_STORIES.md](EPIC%202%20Patient%20Management/PATIENT_MANAGEMENT_STORIES.md) only accounts for **5** and **11**. The stories doc is what the workbook prices, so the plan has been costing the lower figure all along and this table has now been aligned to it. **About 12 coding days of work described in those two features has no user story yet** — the build-your-own filter in F3 and part of the configuration screen in F6. Either the stories are missing and Epic 2 is ~12 days larger, or this table was double-counting. It needs one pass through F3 and F6 to settle.
#### Why the internal order matters

**F2 before everything else that has buttons.** The shared framework is built here because this is the first screen that needs it. Every later screen epic then adopts it with a small story instead of writing its own. Today six screens each have their own copy — `UsersContentView.xaml.cs:35` has `Stack<UserSnapshot>`, `SitesContentView.xaml.cs:30` has `Stack<SiteSnapshot>` — and that duplication is exactly what F2 stops spreading.

**The button bar already exists.** The code review (§9.3) found the ribbon toolbar framework is the delivery mechanism for GID-254873 to GID-254882 — `RibbonDefinitions.cs` declares 20 layouts covering the screens those requirements name, and it renders and routes correctly. So F2 is about **putting working behaviour behind buttons that already exist**, not building buttons. This was previously mis-filed as unrequested extra scope.

**Inside F5, build in this order: field setup tables → validate-and-show-what-is-wrong → mandatory-field check.** The tables hold which fields are mandatory; the validation service reads them and is built inside the workflow that first shows its results (PM-18); the mandatory-field check calls that service. Get that order wrong and validation is built twice. This is why F5 sits in 2B rather than 2C.

**F5 before F6.** They read the same two tables.

**F8 and F9 before F7.** You cannot assign from a list that does not exist. This is why the old warning W1 in §7.2 has gone: the merge fixed the ordering problem by putting both sides in one epic.

#### ⚠ Read this before starting F7 and F8 — defect DF3

The code review found **two incompatible designs for risk factors sitting side by side** (§9.10):

- The patient screen shows a **hardcoded 16-question form** — fixed clinical questions, Yes/No/Unknown buttons, grouped Perinatal / Postnatal / Other, with a live "12 of 16 answered" counter. 520 lines, and it works.
- The requirements (GID-254950, GID-254951) say risk factors are a **list the administrator creates and edits** — which is F8.

Adding a risk factor in F8 would not make it appear on the patient screen, because that screen does not read a list. **One of the two has to be rebuilt.** Decide before F7 and F8 start.

Our recommendation: keep the configurable list, because that is what the requirements ask for, and **keep the 16 hardcoded questions as the shipped default list**. They are standard newborn-screening clinical content — family history of permanent childhood hearing loss, bacterial meningitis, NICU over 5 days, ECMO and so on — and they are worth seeding rather than discarding.

#### Where it stands today

The screens exist. `PatientsView.xaml.cs` is 1,350 lines. But **nothing is saved anywhere.** There is no `Patients` table, no patient repository, no patient service. The project's own analysis says it plainly:

> *"Headline: nothing in the Patient/Tests area is persisted"* — `PATIENT_MANAGEMENT_STORIES.md` §2.1

The configuration screens are the same. `FieldSetupConfigView.xaml.cs` is 533 lines with `// TODO: HandleSave has no actual persistence`. `RiskFactorsConfigView` and `CommentsConfigView` are UI-only.

**Soft delete (D3).** GID-254886 marks the record deleted rather than removing it. Two things follow: every patient query must filter out deleted records, and the patient list must not show them. Both are in F1.

**Field scope, set by Natus on 22 Aug 2026.** *"Use AccuLink as reference for fields to display and add the additional from ALGO 5 and ALGO Pro."* Natus also confirmed the current schema design does **not** yet match AccuLink. Six missing fields are known; a field-by-field pass against AccuLink is needed before the first migration is written (question A10).

**Also fix here:** defect DF1 (11 of the 12 clinical dropdowns do not round-trip their value) and DF2 (name auto-capitalisation corrupts surnames like O'Brien and McDonald). Both sit inside GID-254883 and are listed in §9.10. About 3 days, not included in the 136 above.

---

### Epic 5 · ASWD-5 — OAE Test Result — 13.1 days

**Goal:** view TEOAE and DPOAE results with waveforms.
**Requirements:** GID-254901, GID-254902 (2).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Investigate the waveform data format and how to draw it** | 2-day timeboxed look at the waveform data format and how to draw it | 2 |
| **F2 · Create the OAE result tables and load real device data into them** **P** | Create `TEOAEResults` and `DPOAEResults` tables and waveform storage · Small utility that pushes a **real** device file through the existing ALGO 5 / AccuLink / ALGO Pro parsers straight into the test tables, so the result views can be demonstrated on genuine screening data before Import is built | 2 |
| **F3 · Show TEOAE and DPOAE results with waveform, comments and device info** | Details, waveform, comments and device info for TEOAE (GID-254901) · Same for DPOAE (GID-254902) | 5.5 |
| | **Coding days total** | **23** |

**Note.** The `Waveform` type described in `AccuSync Architecture.md` §5 does not exist. Nor does any drawing code. F1 exists to find out how hard this is before we commit.

**Why the real-data utility inside F2 exists — please do not drop it.** Moving this epic ahead of Import means the test tables are built (Epic 2 F4) but **nothing populates them yet**. Without it these screens would be demonstrated on invented data, which is exactly the trap the existing codebase fell into — 46,000 lines of screens showing hardcoded numbers. Three device parsers already work today (ALGO 5 XML, AccuLink XML, ALGO Pro JSON), so F5 is half a day of plumbing to get **real** screening results on screen. It is what makes the earlier delivery genuinely worth having.

---

### Epic 6 · ASWD-6 — ABR Test Result — 8.5 days

**Goal:** view ABR results with waveforms.
**Requirements:** GID-254903 (1).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create the ABR result table and show results with EEG noise and impedance** **P** | Create the `ABRResults` table · Details, waveform, comments, device info, plus EEG noise and impedance (GID-254903) | 4 |
| | **Coding days total** | **4** |

Cheaper than Epic 5 because the waveform investigation and drawing code are reused. `TestRecord.cs` already has the ABR-specific fields (`EegNoisePercent`, `ImpedanceWhite`, `ImpedanceRed`), which helps.

---

### Epic 7 · ASWD-7 — User Account Management — 28.8 days

**Goal:** admin screens for user accounts, plus everything to do with passwords.
**Requirements:** GID-254904 to GID-254912 (9), plus 4 moved from Login per D13.

| Feature | User stories | Days |
|---|---|---|
| **F1 · Manage user accounts: create, edit, delete, activate, unlock and set language** ← *moved from Login* | View the user list (GID-254904) · Create a user (GID-254905) · Edit a user (GID-254906) · Delete a user (GID-254908) · Activate and deactivate (GID-254909) · Unlock an account (GID-254907) · Choose the display language for each user (GID-254910) — *needs Epic 19* · **Let a user set their own display language from the Settings screen** — the dropdown is there but the choice is never stored (carried over from AccuLink) — *needs Epic 19* · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing its own `Stack<UserSnapshot>` copy · Prevent transfer of deactivated user accounts to the device (GID-255022) — *needs Epic 21* | 9.5 |
| **F2 · Apply and configure the password and lockout rules everywhere** ← *moved from Login* | Move the password rules into `Core` and apply them to **every** place a password is set, and remove the hardcoded `"1234"` defaults · **Let a signed-in user change their own password from the Settings screen** — make Save actually persist, add a current-password check, and stop revealing the password in a plain text box (2.0) · 🔴 Let an admin choose None / Simple / Complex (GID-254912), **and show what each level actually enforces** — the definitions are hardcoded in a resource file today (carried over from AccuLink) — *needs Q2* · Screen to set the lockout duration (GID-254911) · 🔴 Decide what happens to the 90-day expiry — **Natus confirmed 19 Aug 2026 that it is wanted**, so it is built rather than removed; *needs a new requirement for the period and the day-91 behaviour* · **Prevent reuse of the last three passwords (GID-255020)** — the rule is coded but can never fire until real accounts exist (F1, defect DF7) | 14 |
| | **Coding days total** | **23.5** |

**Good news.** The `Users` table already exists, and `UnlockUserAsync` and `AppSettings.LockoutDurationMinutes` are already built and tested. The user screens are mostly building on top of that.

**F1 is more important than it looks.** The code review found `CreateUserAsync` is called from exactly one place — the code that seeds the two factory accounts. **There is no working way to create a real user account** (defect DF7, §9.10). Three other things depend on fixing this:

- The last-three-passwords rule in GID-255020 can never fire today, because no account ever accumulates a password history (§9.2, finding 4).
- Forced password change at first login only ever applies to the two seeded accounts.
- The two seeded accounts use `"12345"`, which fails GID-255020's own rules.

**Why the four password and lockout items moved here** (D13), now all inside **F2**. All four are about user accounts and passwords, and the configurable password strength sits directly next to the lockout duration — both are admin settings on the same screen. Keeping them in Login meant an epic that was 80% finished blocking on an open question (Q2).

**A conflict to note.** GID-254905 says an admin can create users. GID-254977 (S4H) says the software must **prevent** an admin creating users. See **Q6**.

---

### Epic 8 · ASWD-8 — Profile Management — 28.5 days

**Goal:** profiles with configurable permissions, replacing today's hardcoded roles.
**Requirements:** GID-254913 to GID-254918 (6).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete permission profiles** **P** | Create the `Profiles` table with its permission fields · View, create, edit and delete profiles, and show Name / Description / Permissions (GID-254913 to GID-254917) · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy | 4.5 |
| **F2 · Set permissions per profile and retire the hardcoded roles** | Set permissions per profile on the profile screen (GID-254918) *(2.0)* · Retire the fixed presets in `UserPermissionsViewModel` and read permissions from the database *(1.5)* | 3.5 |
| **F3 · Complete RBAC: one authorization mechanism, enforced everywhere** **P** ← *build this early, see below* | Capture the **33 permission fields** from `Databases/SettingsDatabase.sql` before that file is deleted, and write them down as the permission model *(1.5)* · One authorization service in `Core`: who is signed in, their effective permissions, and a single `Can(permission)` check that everything calls *(2.0)* · Enforce in the UI — every command’s `CanExecute`, and a blocked action hidden rather than merely greyed out *(2.5)* · Enforce again in the Application layer, so going around a screen does not go around the rule *(2.5)* · Re-evaluate a signed-in user’s permissions when their profile changes, without a restart *(1.0)* · Every denied action is logged and audited *(1.0)* · The **permission matrix**: 33 permissions × every gated action, written down as both the specification and the test oracle *(2.0)* · Tests proving each permission gates its action **at both layers**, and that patient delete stays available to every user per the 22 Aug decision *(2.5)* | 15 |
| | **Coding days total** | **9.5** |

**Why retiring the hardcoded roles matters (inside F2).** Today `UserPermissionsViewModel` has three fixed presets in C#: `Admin()`, `Screener()` and `ReadOnly()`. `ReadOnly` is not in the SRS at all. Once profiles are in the database, these presets should become seeded rows.

**Warning.** The permission fields — 33 of them — are recorded **only** in `Databases/SettingsDatabase.sql`, and that file is due to be deleted. Epic 0 F1 captures this first. If that story is skipped, this epic loses its specification.

**⚠ Why RBAC is a feature of its own, and why it has to be built early.** Until 26 Aug 2026 the whole of role-based access control was **one bullet** — *"enforce them across the app (GID-254918)"* — inside a five-day feature. That bullet covers **33 permissions across every screen in the product**. The code review already found the evidence that it is not built: the sidebar navigation shell "works, but **permission-hiding never fires**" (X1a), and `UserPermissionsViewModel` still returns three hardcoded presets.

**It cannot wait for position 12.** Epic 2 Patient Management is built at positions 5–8 and already needs permission checks — PM-21 enforces patient permissions today against "the existing role check", to be swapped when ASWD-8 lands. Every epic built before Epic 8 inherits that same temporary step, and every one of them would have to be revisited. **F3 is therefore pulled forward to just before Epic 2**, the same way Epic 21’s investigation is pulled forward. The rest of Epic 8 stays where it is.

**This is X8z, made real.** §9.11 Option C already proposes keeping the "33-permission profile matrix" at **14 days**, calling it *"a legitimate elaboration of GID-254918, which Epic 8 needs anyway"*. **F3 is that work**, arrived at independently and costed at 15. If Option C is adopted, X8z is this feature — not another 14 days on top.

---

### Epic 9 · ASWD-9 — Device Management — 20.8 days

**Goal:** device list, assignment and settings.
**Requirements:** GID-254919 to GID-254926 (8).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete screening devices** **P** | Create `Devices` and related tables · View the device list with Name / Serial / Last Seen · Add, edit and delete a device (GID-254919 to GID-254922) · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy | 5.5 |
| **F2 · Assign users and facilities to a device** | GID-254923 · GID-254924 — *needs Epic 11* | 3 |
| **F3 · Configure on-device settings and show device system information** | 🔴 Configure the 7 device settings (GID-254925) — *needs Q8* · Last Seen, Last Updated, Hardware Version, Firmware Version (GID-254926) — *needs Epic 21* | 4.5 |
| | **Coding days total** | **13** |

**Do not start the device settings in F3 yet.** GID-254925 is marked `S,U*` with Hazard ID 6.4. The SRS §4 says a `U*` requirement is one that **reduces a safety risk**. Hazard 6.4 lives in DOC-076518, which we do not have. Building a risk control without knowing the risk is the one mistake that cannot be corrected later. See **Q8**.

---

### Epic 10 · ASWD-10 — Site Management — 9.5 days

**Requirements:** GID-254927 to GID-254931 (5).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete sites, and set what syncs to devices** **P** ← *was System Configuration* | Create the `Sites` table · View, add, edit and delete sites with Name / Description / Code · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy · Setting for whether the site and facility list is pushed to devices during synchronisation (GID-255001, GID-255002) — *cannot be finished until Epic 21* | 3.25 |
| **F2 · Shared “can this record be deleted?” check reused by seven screens** | One shared rule set for "can this record be deleted?", reused by Facility, Location, Profile, Device, Protocol, Risk Factor and Comment | 2.75 |
| | **Coding days total** | **6** |

**Why F2 lives here.** Nine requirements allow deletion but never say what happens to records that depend on the deleted one. Delete a Site — what happens to its Facilities? We build the check once, in the first epic that needs it. See **Q9**.

---

### Epic 11 · ASWD-11 — Facility Management — 7.2 days

**Requirements:** GID-254932 to GID-254936 (5).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete facilities** **P** | Create the `Facilities` table · View, add, edit and delete facilities with Name / Description / Code / Site / Location Type · Reuse Epic 10's "in use" check · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy | 3.5 |
| | **Coding days total** | **3.5** |

---

### Epic 12 · ASWD-12 — Location Management — 7.2 days

**Requirements:** GID-254937 to GID-254941, GID-255465 (6).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete locations, and assign one to every facility** **P** | Create the `Locations` table · View, add, edit and delete locations with Name / Description / Code · When adding a location, offer to assign it to every facility (GID-255465) · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy | 4.5 |
| | **Coding days total** | **4.5** |

---

### Epic 13 · ASWD-13 — ABR Test Protocol Configuration — 7 days

**Requirements:** GID-254942 to GID-254945 (4).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete ABR test protocols** **P** | Create the `ABRProtocols` table with its parameter value ranges · View, create, edit and delete ABR protocols · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy | 4.5 |
| | **Coding days total** | **4.5** |

**Warning.** The allowed values for each protocol parameter are recorded only in `Databases/SettingsDatabase.sql`. Epic 0 F1 must capture them before that file is deleted.

---

### Epic 14 · ASWD-14 — DPOAE Test Protocol Configuration — 8.5 days

**Requirements:** GID-254946 to GID-254949 (4).

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create, view, edit and delete DPOAE test protocols** **P** | Create the `DPOAEProtocols` table with its parameter value ranges · View, create, edit and delete DPOAE protocols · Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy | 4.5 |
| | **Coding days total** | **4.5** |

---

### Epic 3 · ASWD-32 — Data Exchange: import, export and S4H — 81.9 days ⚠ *needs a Jira epic*

**Goal:** every exchange format works in both directions, and the S4H service is in sync. **One feature per format, import and export finished together.**
**Requirements:** GID-254898, GID-254899, GID-254900, GID-254971 to GID-254995, GID-256274 to GID-256286, GID-256512 (42).
**Replaces ASWD-3 Import, ASWD-4 Export and ASWD-20 S4H**, merged on 25 Aug 2026 — see D20.

| Feature | User stories | Days |
|---|---|---|
| **F1 · Exchange foundation: the record layer, settings, selection and de-identification** **P** | Port the record-description and stream layer from AccuLink `Component.Core/Core/DataExchange` — column attributes, and the flat-file, binary and XML readers and writers *(3.0)* · Create `ImportConfiguration` and `ExportConfiguration` and save the settings *(1.5)* · Choose the export folder, format, and default format (GID-256274, GID-256286, GID-256512) *(1.5)* · Export new patients / all patients / selected entries / a test date range (GID-256275 to GID-256278) *(2.0)* · Strip demographics and identifiers before export (GID-256279) *(1.0)* · Connect every parser and writer to the patient repository, so an import is stored and an export reads real data *(1.5)* | 10.5 |
| **F2 · AccuSync XML and JSON — import and export** | Agree and write down the AccuSync XML and JSON layout *(1.5)* · AccuSync XML and JSON parsers *(2.0)* · AccuSync XML and JSON writers with the required file names (GID-256280, GID-256281) *(2.0)* | 5.5 |
| **F3 · CSV — import and export** | 🔴 CSV writer (GID-256284) — *needs Q3; still the one format AccuLink cannot answer* *(2.0)* · Export the on-screen patient list to CSV — Phase 1 per Natus 19 Aug 2026, covered by GID-254898, and it already works today *(0.5)* · CSV import matching the writer *(1.5)* | 4.0 |
| **F4 · ALGO 5 XML and ALGO Pro — import and export** | Test harness plus tests for the three existing parsers *(2.0)* · Fix the three format defects found in the samples: ALGO Pro duplicate `RiskFactors` keys, the missing ABR/DPOAE marker, and six fields encoded as text in one format and numbers in the other *(2.0)* · ALGO 5 XML writer with the required file name (GID-256285) *(1.5)* · ALGO Pro writer *(1.5)* | 7.0 |
| **F5 · AccuLink XML — import and export** | AccuLink XML import through the existing parser, with tests *(1.5)* · AccuLink XML writer and its typed document, from AccuLink `XmlPatientDataExporter` and the 35 classes generated from `AccuLinkXiMpLe.xsd` *(3.0)* | 4.5 |
| **F6 · HiTrack — export, and pick-list import** *(fills PM-40’s Nursery and Race lists; Natus supplied sample pick-list files 26 Aug 2026)* | HiTrack writer, including the duplicate `INTHS.txt` (GID-256282) — **the 114-column layout comes from AccuLink `HiTrackTest`, so Q3 no longer blocks it** *(3.0)* · The export rules that go with it: result codes for deceased and discharged patients, risk-factor and comment aggregation, ethnicity and education code maps *(2.0)* · HiTrack pick-list import: hospitals, physicians, audiologists, screeners, nursery types and race types *(2.5)* | 7.5 |
| **F7 · OZ — binary export** | OZ writer (GID-256283) — **the binary record layout comes from AccuLink `Oz7Test`, so Q3 no longer blocks it** *(2.5)* · ⚠ AccuLink has no OZ importer and none is specified. **X1 must be answered before any OZ import is planned** *(0.5)* | 3.0 |
| **F8 · S4H: the connector and the sync engine** | SOAP client for the SEDQ service — **the contract is `northgate.wsdl` with `uploadData` and `downloadSyncData`, so the 3-day discovery is replaced by a read** *(2.0)* · Client plus a screen to set the web service URL (GID-254974) *(1.5)* · The shared sync framework and its trigger rules *(2.0)* · 🔴 Send patient and test data to an external system over a web service (GID-254899) — *needs Q7* *(1.0)* | 6.5 |
| **F9 · S4H: receive users, devices, facilities and risk factors** | Receive the user list and store username and user ID, read-only (GID-254975, GID-254982, GID-254983) *(2.0)* · Auto-deactivate and reactivate, default profile and password, block admin adding users (GID-254977 to GID-254981) *(2.5)* · Receive devices from S4H: store device ID, name and serial number, block local edits to them, and follow the service’s active and inactive status (GID-254984 to GID-254987) *(2.0)* · Receive facilities from S4H and keep them in step, read-only locally (GID-254988 to GID-254991) *(2.0)* · Receive risk factors from S4H and keep them in step, read-only locally (GID-254992 to GID-254995) *(2.0)* | 10.5 |
| **F10 · S4H: send identifiers, results and NHSP wording** | Site ID, device ID, and inpatient/outpatient facility lists (GID-254972, GID-254973) — *needs Epic 21, which is why this epic now runs after it* *(2.5)* · Test results including binary waveform data (GID-254976) *(3.0)* · NHSP wording: Pass → Clear Response, Refer → No Clear Response, Surname, Forename (GID-254971) *(1.5)* | 7.0 |
| | **Coding days total** | **66.0** |

#### The two slices

This epic straddles two of the customer's milestones, so it is delivered in two slices, the same way Epic 2 is.

| Slice | Features | Coding | Total | Milestone | You can demonstrate |
|---|---|---|---|---|---|
| **32A · File formats** | F1–F7 | 42.0 | **50.6** | M5 | Every file format round-trips: AccuSync XML and JSON, CSV, ALGO 5, ALGO Pro, AccuLink XML, HiTrack and OZ |
| **32B · S4H** | F8–F10 | 24.0 | **31.3** | M7 | AccuSync signs on to the SEDQ service, receives users, devices, facilities and risk factors, and sends results back |
| | | **66.0** | **81.9** | | |

#### Why these three epics became one

**The lead's rule: one feature per format, import and export finished together** — not all imports first and all exports later. AccuLink's own design argues the same way. A format there is declared once:

```csharp
[DataExchangeColumn(Identifier = "CDOB", Format = "{0:yyyy}{0:MM}{0:dd}", ImportFormat = "yyyyMMdd", MaxLength = 8)]
```

`Format` and `ImportFormat` sit on the same declaration. Splitting a format across two epics means opening the same class twice.

**S4H is eSP.** AccuLink's eSP connector *is* the SMaRT4Hearing sync — `northgate.esp.sedq.bserv.ProxySEDQ`, with `uploadData` and `downloadSyncData`. Its import is a configuration download and its export is a result upload, which is exactly what ASWD-20 F3, F4 and F5 describe. It is an exchange target like the others, so it belongs here.

#### What reading the AccuLink source changed

Full evidence: **[AccuLink-DataExchange-Analysis.md](AccuLink-DataExchange-Analysis.md)**.

| | Before | After |
|---|---|---|
| HiTrack file contents | 🔴 unknown, blocked on Q3 | **114 declared columns** in `HiTrackTest` |
| OZ file contents | 🔴 unknown, blocked on Q3 | **Binary record** declared in `Oz7Test` |
| SEDQ contract | 🔴 3-day timeboxed discovery, blocked on Q11 | **`northgate.wsdl` + 3 XSDs on disk** |
| CSV contents | 🔴 unknown, blocked on Q3 | 🔴 **still unknown** — AccuLink has no CSV component |

**What ports and what does not.** The record declarations, the stream readers and writers, and the export business rules port. The eSP SOAP client does not — `SoapHttpClientProtocol` is .NET Framework only, so .NET 10 needs a generated WCF client or SOAP over `HttpClient`. Nothing under `*.WindowsForms` ports. **The formats port; the plumbing at both ends does not** — which still removes the biggest risk here, because the risk was never writing the code, it was not knowing what the files contain.

#### ⚠ Three assumptions that were wrong

1. **OZ has no import.** AccuLink exports OZ and never reads it. OZ import would be new work with no reference implementation and no named format owner — **X1**.
2. **HiTrack import is pick lists, not patients** — hospitals, physicians, audiologists, screeners, nursery and race types. Sizing it as a patient round trip would be wrong — **X2**.
3. **eSP import is a configuration download**, not a file import.

#### ⚠ Before anyone copies a file

**X5: are we licensed to port this code, or must we re-implement from the specifications it contains?** Reading AccuLink to learn the formats is safe. Lifting source into a new product is a question for Natus and Soliton legal, and it needs answering before F1 starts, because F1 is the port.

### Epic 18 · ASWD-18 — Report Generation — 19.2 days

**Goal:** a screener can produce, look at, print and file a patient report, and an administrator can set how it looks.
**Requirements:** GID-255112, GID-255113, GID-254965, **GID-254894** (4).
**Absorbs Epic 2 F10 on 27 Aug 2026**, which answers **Q5** — the patient test report is one thing, and it lives here.

| Feature | User stories | Days |
|---|---|---|
| **F1 · Choose the reporting library** | 1.5-day timeboxed investigation — there is no reporting library in the solution today, and the choice constrains everything below *(1.5)* | 1.5 |
| **F2 · Generate, preview and print a patient report** **P** | Pick a patient, choose **selected tests or all tests**, and choose one of the predefined report types (GID-254894) *(3.0)* · Render the report with demographic information and test result details (GID-255112) *(6.0)* · The **ten predefined report templates** — Phase 1 per Natus 19 Aug 2026, *needs a new requirement naming the ten* *(6.0)* · Preview it on screen before committing to paper *(1.5)* · Print, with a progress indicator *(2.0)* | 18.5 |
| **F3 · Save a report to a file** | 🔴 Export the report to a supported file format (GID-255113) — *the format list is still needed* *(3.0)* | 3.0 |
| **F4 · Set the report layout: logo and paper size** | Add a logo from a graphics file and choose the paper format (GID-254965) *(3.0)* · Apply the layout to every report type, and show the change in the preview *(2.0)* | 5.0 |
| | **Coding days total** | **28.0** |

#### Why this epic was restructured

**The two features it had were shaped by the code, not by the user.** *"Choose the reporting library and render the patient report"* put a timeboxed investigation and the rendering engine in one box; *"Set the report layout, then save and print it"* put an administrator's configuration screen and a screener's print action in another. Neither is something a person does.

**Now each feature is one workflow**, per D18: generate and print (F2), save to a file (F3), set the layout (F4). The library choice stays on its own as F1 — it is a timeboxed investigation ending in a decision, the same shape as Epic 21 F1 and Epic 30 F4.

**Q5 is answered by the move.** It asked whether the patient test report was one feature or two, because **GID-254894** sat in Epic 2 and **GID-255112** sat here. They describe the same report. It is one workflow, and it belongs in the epic that owns reporting. **Epic 2 F10 and its story PM-38 are retired**; Epic 2 goes from 10 features to 9.

**On the numbers:** Epic 18 had 25.5 coding days and Epic 2 F10 had 3.0, so 28.5 came in and 28.0 went out. The half-day is the rounding of splitting two features into four — no work was dropped.


---

### Epic 19 · ASWD-19 — Language — 35.4 days

**Goal:** AccuSync runs in all 14 languages the requirements name, with the text reviewed once, translated by Natus's partners, and switched without a restart.
**Requirements:** GID-254967 to GID-254970, GID-256513 (5).
**Scope confirmed by Natus 27 Aug 2026** — see the answers below. **All three phases are in this release.**

| Feature | User stories | Days |
|---|---|---|
| **F1 · Investigate and build the language switching infrastructure** **P** | 2-day timeboxed look at satellite assemblies, per-user language, and **Chinese and Japanese text layout — now in scope, not deferred** *(2.0)* · Language switching, English as default, language picker and confirmation prompt (GID-254967, GID-256513, GID-254970) *(3.0)* | 5.0 |
| **F2 · Merge the duplicated strings and cover every screen** | The same text exists twice — 1,206 entries in `AccuSync.WPF` and 1,213 in `AccuSync.Application`, which de-duplicate to **860 distinct strings**. Merge them, and move English text out of the service classes *(3.0)* · Go through all text, messages, menus, prompts and results (GID-254969) *(2.5)* | 5.5 |
| **F3 · Review and freeze the strings, and produce the translation pack** **P** | **Review and finalise all 860 strings before any translation starts** — Natus’s condition, 27 Aug 2026 *(2.0)* · Build the pack the distribution partners receive: every string with the screen it appears on, **plus a screenshot of each screen** — Natus asked for screenshots *(1.5)* · Capture the screenshots once the screens are final *(1.0)* · Import returned translations and flag anything missing, changed or untranslated *(0.5)* | 5.0 |
| **F4 · Phase 1: French, Italian, German, Spanish** | Resource set, load and smoke-test every screen for each of the four (GID-254968 — the full 14-language list, delivered across F4, F5 and F6) *(2.0)* · Fix the layout truncation real translations expose — German and French run 20–30% longer than English *(1.0)* | 3.0 |
| **F5 · Phase 2: Brazilian Portuguese, Chinese (simplified), Chinese (traditional), Japanese** | Resource set, load and smoke-test for each of the four *(2.0)* · **Chinese and Japanese support**: fonts that carry the full character set, text measurement and line breaking, and no character loss through export, report or print *(3.0)* · Verify the three dropdown value lists and all validation messages render correctly *(1.0)* | 6.0 |
| **F6 · Phase 3: Norwegian, Danish, Finnish, Swedish, Turkish** | Resource set, load and smoke-test for each of the five *(2.5)* · **Turkish casing**: culture-aware upper and lower case and comparison, because Turkish has a dotless *i* and .NET’s default casing corrupts it *(1.5)* · Finnish and Norwegian layout checks — long compound words break narrow columns *(1.0)* | 5.0 |
| | **Coding days total** | **29.5** |

#### The two slices

The language phases cannot start until the last screen exists, so this epic is delivered in two slices.

| Slice | Features | Coding | Total | Milestone | You can demonstrate |
|---|---|---|---|---|---|
| **19A · Switching and the string pack** | F1–F3 | 15.5 | **16.2** | M6 | A user picks a language and the whole application switches. The 860 strings are reviewed, frozen and packed for the partners |
| **19B · The three language phases** | F4–F6 | 14.0 | **19.2** | M8 | All 14 languages load and every screen reads correctly, including Chinese, Japanese and Turkish |
| | | **29.5** | **35.4** | | |

#### What Natus answered, 27 August 2026

| | Question | Answer |
|---|---|---|
| **1** | Which phase ships in this release? | *"GID-254968 is the list of languages to support as of now. We start with Phase 1. **By product launch, all phases have to be completed.**"* |
| **2** | Who translates the words? | *"We have a process. We provide the strings (**screenshots are helpful**) to associated distribution partners of the region and they will provide the translations."* |
| **3** | Does changing language need a restart? | *"It’s preferred if the text can be updated without a restart. **If not, then restart.**"* |
| **note** | — | *"**Strings have to be reviewed/finalized before they are translated.**"* |

#### What those answers changed

**All 14 languages, not 4.** We had planned Phase 1 only and said so as a suggestion. Natus overrode it: Phase 1 is where we *start*, and every phase must be complete at launch. That adds **Phase 2** (Brazilian Portuguese, Chinese simplified, Chinese traditional, Japanese) and **Phase 3** (Norwegian, Danish, Finnish, Swedish, Turkish) as real features.

**Chinese and Japanese stop being a footnote.** They were a line in F1’s investigation. Now they are 3.0 days of their own: fonts that carry the full character set, text measurement and line breaking, and proving no character is lost through export, report or print. **This is the single largest technical consequence of the answer.**

**Turkish is a correctness problem, not a translation problem.** Turkish has a dotless *i*, and .NET’s default `ToUpper` and `ToLower` corrupt it. Every case-insensitive comparison in the product has to be culture-aware, or a Turkish user gets wrong search results and failed logins.

**The string freeze sets the order.** Strings cannot be sent to the partners until they are reviewed and final, and they are not final while screens are still being added. That is why **F4 to F6 sit in slice 19B, late, next to the release work** — sending the pack early means paying the partners to translate the same strings twice.

**Restart is now a fallback, not an open question.** Live switching is preferred; if a screen turns out to be expensive that way, a restart is acceptable. F1 builds live and falls back where it has to.

#### ⚠ What is still not in the estimate

**The translation itself is Natus’s work**, through their regional distribution partners. **860 distinct strings, about 2,413 English words — across 13 new languages that is roughly 31,400 words.** What we owe them is the pack, and F3 costs it.

**The partner round trip is a schedule risk we do not control.** Thirteen partner sets have to come back, be imported and be checked. If one region is slow, that language holds up the release and no amount of development effort fixes it.

### Epic 21 · ASWD-21 — Device Communication (+ Firmware) — 41.1 days

**Goal:** talk to the AccuScreen Pro over USB, both directions, and update its firmware.
**Requirements:** GID-254996 to GID-255004, GID-255005, GID-255010, GID-255011 (12).
**Status: the project that should hold this does not exist.**

| Feature | User stories | Days |
|---|---|---|
| **F1 · Discover the device protocol and create the communication project** | 🔴 3-day timeboxed discovery against a real device — *needs Q10* · New `AccuSync.Adapters.DeviceCommunication` project and its interfaces in `Core` | 5 |
| **F2 · Connect to devices over USB and show connection status** | USB transport, device discovery, connect and disconnect (GID-254996) · Show connected / disconnected / connecting, with visual indicators (GID-254997, GID-255010) | 5 |
| **F3 · Read device information, patient demographics and test results** | Firmware version and device identification (GID-254998) · Patient demographics and test results (GID-255000, GID-255044, GID-255045) | 5 |
| **F4 · Send patients and configuration to the device** | GID-254999 · Site config (GID-255001) · Facility config (GID-255002) · ABR protocols (GID-255003) · DPOAE protocols (GID-255004) | 11 |
| **F5 · Send firmware to the device with warnings and progress** ⚠ *SRS §5.31, no Jira epic* | Send firmware to the device (GID-255005) · Show warnings and progress before and during (GID-255011) | 5 |
| | **Coding days total** | **31** |

**Three things are missing before this can start.**

1. **The protocol specification.** Command format, framing, timeouts, error handling.
2. **A physical device or an emulator.** You cannot develop or test device I/O against nothing.
3. **A decision: USB or serial port?** GID-254996 says **USB**. But `AccuSync Architecture.md` §5 describes a **serial port** (`SerialPortDeviceChannel`, `System.IO.Ports`). These are very different to build. Note `System.IO.Ports 8.0.0` is listed in `Directory.Packages.props` but no project uses it.

See **Q10**. Until F1 runs, **every number in this epic is a guess.**

---

### Epic 24 · ASWD-24 — Alarms, Warnings, Operator Messages — 8.9 days

**Requirements:** GID-255008 to GID-255014 (7). **Five are delivered by other epics.**

| Feature | User stories | Days |
|---|---|---|
| **F1 · Show progress for slow operations and log the user out after inactivity** | Shared progress indicator for slow operations: printing, transfers, firmware (GID-255013) · 🔴 Auto-logout after inactivity, with a warning first (GID-255014) — *needs Q13* · GID-255008 → Epic 1 · GID-255009 → Epic G · GID-255010 → Epic 21 · GID-255011 → Epic 21 · GID-255012 → Epic 17 | 4 |
| | **Coding days total** | **4** |

**Problem with the session timeout in F1.** GID-255014 asks for a warning *before* automatic logout. But **no requirement asks for automatic logout at all.** There is no timeout value, and nothing says whether it is configurable. We cannot build a warning for a feature that is not specified. See **Q13**.

---

### Epic 31 · ASWD-31 — Logging: retention and protection — 3.2 days ⚠ *needs a Jira epic*

**Requirements:** GID-255035, GID-255036, GID-255037 (3).
**Split out of ASWD-26 on 25 Aug 2026** and moved here deliberately.

| Feature | User stories | Days |
|---|---|---|
| **F1 · Roll, retain and protect the log files** | Roll daily and at 5 MB, keep 366 days, delete oldest first, file name `accusync_log_{count}_{date:yyyy-MM-dd_HH-mm-ss}.log` (GID-255035) · Restrict read access to the log folder (GID-255036) · Provide no user function to delete or modify a log file (GID-255037 as reworded) | 2.0 |
| | **Coding days total** | **2** |

**Why it sits here and not at position 4.** None of this is visible. A stakeholder cannot see a file rolling at midnight or a folder permission. Building it early would delay the work that *is* visible without reducing any risk, because the logger it protects already exists from Epic 26. Placing it next to **Epic 25 Audit Trail** is deliberate: the audit trail needs the same rolling, retention and protection approach, so the two are built once and shared rather than twice.

**⚠ This epic needs a Jira number.** `ASWD-31` is a placeholder we are using so the plan and the workbook reconcile. Natus needs to create the epic, or tell us to keep both halves under ASWD-26 with the second half as a separate feature.

**Two things Natus settled on 22 Aug 2026.**

- **GID-255037 reworded** from *"prevent deletion of log files by any user"* — which no application can do — to *"The software shall not provide any user function to delete or modify log files."* ⚠ **Not yet changed in Jama.**
- **Retention**: keep **366 days**, roll **daily and at 5 MB**, delete oldest first.

Story-level detail is in [EPIC 31 Logging Retention/LOGGING_RETENTION_STORIES.md](EPIC%2031%20Logging%20Retention/LOGGING_RETENTION_STORIES.md).

---

### Epic 25 · ASWD-25 — Audit Trail — 14.2 days

**Requirements:** GID-255023 to GID-255030 (8). **Deliberately late (D6).**

| Feature | User stories | Days |
|---|---|---|
| **F1 · Create the audit table and keep records for at least a year** **P** | Audit table and writer with the 5 required fields: user ID, timestamp, description, device serial, status (GID-255027) · Keep audit records for at least one year (GID-255029) | 4.5 |
| **F2 · Record login, configuration, patient, sync and firmware events** | Login, logout, attempts, failures, password changes (GID-255023) · GID-255025 · Create, change, delete, export, import (GID-255024) · Sync attempts and automatic user status changes (GID-255028, GID-255030) · GID-255026 | 5.5 |
| | **Coding days total** | **10** |

**One consequence of doing this late, which you should know.** GID-255024 requires an audit record for every patient record change. Epic 2 finishes about 15 months before Epic 25 starts. So **any patient records created during testing in that gap will have no audit history.** For a medical device that may matter at verification time. It is a consequence of the chosen order, not a mistake — but worth deciding consciously.

**Also:** with two databases (D1), the audit record and the patient change it describes may sit in different databases. That means they cannot be written in one transaction, so an audit write can fail while the change succeeds. Best-effort only. This is a knock-on effect of D1 and it belongs in the Epic 25 HLD.

---

### Epic 27 · ASWD-27 — About — 2.5 days

| Feature | User stories | Days |
|---|---|---|
| **F1 · Show version number, manufacturer and website** | Show version number, manufacturer name and website (GID-255039) | 1 |

---

### Epic 28 · ASWD-28 — Help — 5.3 days

| Feature | User stories | Days |
|---|---|---|
| **F1 · Open the user help and jump to the page for the current screen** | 🔴 Open user help from inside the app (GID-255040) — *needs Q15* · Help icon opens the page for the current screen (GID-255041) | 4.5 |
| | **Coding days total** | **4.5** |

**Not included:** writing the help content. There is no help content in the repository. Building the viewer is a developer task; writing the text is not. See **Q15**.

---

### Epic 30 · ASWD-30 — Installation — 34.9 days

**Goal:** get AccuSync onto a machine, encrypted and running on a supported system, and off it again cleanly.
**Requirements:** GID-255046, GID-255047, plus GID-255006, GID-255007, GID-255042–255045, GID-256510, GID-256511 (**10**) — absorbed from ASWD-22, ASWD-23 and ASWD-29 per D14.

| Feature | User stories | Days |
|---|---|---|
| **F1 · Choose the installer technology and build the guided install** | 1.5-day timeboxed comparison and a working minimal installer · Check prerequisites and system requirements before installing (GID-255046) | 4.5 |
| **F2 · Uninstall cleanly, declare Windows 11 support and test a fresh install** ← *was ASWD-23* | Remove everything, with a choice to keep or delete the database (GID-255047) · Confirm and declare support for Windows 11 Pro and Enterprise, and check the operating system at install time (GID-255007) · Test a fresh install on clean Windows 11 Pro and Enterprise | 4.5 |
| **F3 · Encrypt the databases, check the network and confirm final storage** ← *was ASWD-22* ← *was ASWD-29* | 🔴 Detect no network before calling a web service and show a clear error (GID-255006) — *needs Q12* · Encrypt both database files to AES-256 and manage the key at install time (GID-256510, GID-256511) · Confirm patient records and settings land in the right database, including records received from a device (GID-255042 to GID-255045) | 4.5 |
| **F4 · Database encryption research: the options, the cost and the decision** **P** ← *pulled forward to week 3, see below* | Survey the options and what each one costs to licence: **SQLCipher** through SQLitePCLRaw, the commercial **SQLite Encryption Extension**, **column-level encryption** through EF Core value converters, and **OS-level** BitLocker or EFS *(1.0)* · Spike each viable option against the real schema and a realistic data volume, and **measure** read and write cost rather than guess it *(1.5)* · Key management: where the key lives, how it is protected at rest, how it rotates, and what happens when it is lost — a lost key on an encrypted patient database is unrecoverable data *(1.0)* · Prove backup, restore and EF Core migrations still work on an encrypted file, and decide how support reads a customer database *(1.0)* · Write the decision: the recommendation, the trade-offs, and **the design rules every later feature must follow** — including whether development proceeds against the encrypted provider from Epic 2 onward *(1.0)* | 5.5 |
| | **Coding days total** | **19** |

**Cannot start before Epic 0 F1.** The build must work first.

**Why encryption moved here (D14).** Because nothing is delivered until every requirement is shipped (D8), there is no customer data to protect during development. So the databases can stay unencrypted while the product is being built, and encryption can be switched on and verified as part of getting it onto a machine. Epic 0 still runs the *investigation* that chooses the approach, because that decision constrains how the two DbContexts are registered — but the implementation and the key management belong here.

**One thing to watch.** Switching encryption on at the end means the whole application is developed and tested against unencrypted databases. F3 must therefore include a full regression pass, not just a switch-on: an encrypted SQLite provider changes the connection string, and can change behaviour around file locking and performance. That is why F6 is 2 days rather than a few hours.

**⚠ Why the encryption research is pulled forward to week 3.** Asked for by the code reviewer on 26 Aug 2026: *"we need to do a research about encrypting the database early so that we understand pros and cons of it. This knowledge is crucial and should be considered while designing new features."*

The plan already contained the reason. This section says F3 must include **a full regression pass** because *"an encrypted SQLite provider changes the connection string, and can change behaviour around file locking and performance"* — that is the cost of finding out at position 30 what could have been known in week 3. Until now the only encryption research in the plan was a single 1.5-day bullet inside Epic 0 F1, and since Epic 0 became one hand-set 20-hour line that bullet is neither visible nor separately costed. **It is now Epic 30 F4, sized at 5.5 coding days, and scheduled beside the Epic 21 investigation.**

**What it could save.** If the decision is to develop against the encrypted provider from Epic 2 onward, most of F3’s regression pass stops being necessary — there is nothing to regress if nothing was ever built on the other provider. **That saving is not banked here**, because whether to develop against it is part of what F4 decides. If the answer is yes, F3 gets smaller and the plan should be re-derived.

**What it protects against.** Two answers from F4 change design, not just deployment: column-level encryption through EF Core value converters would change how every entity is mapped and would break querying on encrypted columns; and a per-machine key would change what backup, restore and support access mean for every epic that touches data. Neither is something to discover at position 30.

**D9 keeps the install testing in F2 small.** We only prove a fresh install works. There is no previous version in the field, so there is no upgrade path to build or test.

**Still true regardless:** the uninstall in F2 must delete the encryption key folder as well as the database file. If it removes the database but leaves the keys, or the reverse, a reinstall lands in a broken state.


---

### Epic 33 · ASWD-33 — Release: release testing and documentation — 32.5 days ⚠ *needs a Jira epic*

**Goal:** take a feature-complete build, test it by hand against every requirement, prove it installs on a clean Windows machine, and ship it with the documents a user and a regulator need.
**Requirements:** none. **No requirement in DOC-076814 covers release, testing or documentation** — which is exactly why this work has been invisible until now.
**Added 26 Aug 2026 at the code reviewer's request; scoped to two features by the lead the same day.**

| Feature | User stories | Days |
|---|---|---|
| **F1 · Release testing: manual test against every requirement, and install on a new Windows machine** **P** | Write the **test plan**: one manual test case per requirement, with the expected result and the evidence to capture *(4.0)* · Execute the full manual pass across all **196 requirements** against a named build, and re-test what fails after each fix *(7.0)* · Installer testing: install on a clean machine of each supported Windows version and run the pass there, not only on a developer machine *(1.4)* | 12.4 |
| **F2 · User documentation: manual and release notes** | Write the **user manual as a standalone document** covering every screen and workflow, against the finished product rather than the design *(4.0)* · Release notes and the known-issue list *(2.0)* · Check the user manual against the in-application help from Epic 28 so the two do not disagree — **a review of both, no application changes** *(3.0)* | 9.0 |
| | **Coding days total** | **21.4** |

#### How this epic is costed

**Development Hours are simply coding days × 8** — and since the lead set the hours directly on 27 Aug 2026, **the day figures above follow the hours**, not the other way round. The rest of the plan adds a design and unit-test uplift to every sub-task; here that would double-count, because the work *is* testing and writing. The one-day sub-task cap and the reuse discount are also switched off — executing 196 test cases is the same small job 196 times, the same argument that applies to the field-bulk stories in Epic 2.

**And each task is one sub-task, named after itself.** Everywhere else in the plan a task breaks into service, ViewModel and screen work. **Nothing in this epic changes the application** — the user manual is a separate document, and the help check is a review of two documents. Breaking a writing task into code layers would be nonsense.

#### ⚠ What was cut, and what that means

The epic was proposed with seven features. **The lead scoped it to two on 26 Aug 2026.** These five came out:

| Cut | Was | Where it stands now |
|---|---:|---|
| Integration test suite across epics | 20.0 | **Not in the plan.** Unit tests stay inside every feature's estimate; there is no end-to-end suite |
| Requirement traceability and the verification report | 10.0 | **Not in the plan.** F1 records evidence per requirement, which is not the same as a maintained trace from requirement to design to code to test |
| IEC 62304 lifecycle records | 12.0 | **Not in the plan** — safety classification, SOUP list, configuration management, maintenance plan, unresolved anomalies list |
| Risk management file (ISO 14971) | 10.0 | **Not in the plan** — hazard analysis, risk controls, residual risk |
| Release engineering — signed reproducible build, checklist, go/no-go, design history file | 8.0 | **Partly kept.** Installing and validating on a clean Windows machine moved into F1. The signed reproducible build, the release checklist and the design history file are not in the plan |

**Also removed on 26 Aug 2026, in the same pass:** the separate **instructions for use** task, and the **upgrade path from AccuLink** in the release notes. **Defect triage and regression are no longer a task of their own** — re-testing after a fix is inside the 10 days of testing.

**This is a scope decision, not a discovery that the work is unnecessary.** On a medical device the IEC 62304 records and the ISO 14971 risk file are mandatory, and something has to produce them. The most likely reason they do not belong here is that **Natus's own quality and regulatory function owns them** — which was always the open question under this epic. **Risk R3 is therefore reopened for the part that is no longer covered.**

## 7. Order and timeline

### 7.1 Order of work

Screenshot order (D7), with the investigations pulled forward, test results ahead of Import/Export (D15) and Administration ahead of Import/Export (D16).

| Position | Epic | Days | Running total | Weeks |
|---|---|---|---|---|
| 1 | Epic 0 · Revamp Code Base | 9.1 | 9.1 | 1–3 |
| 2 | **Investigations pulled forward:** Epic 21 F1 · **Epic 30 F4 — database encryption research** | 17.1 | 26.2 | 4–7 |
| 3 | Epic 1 · Login closure | 8.9 | 35.1 | 8–10 |
| 4 | Epic 26 · Logging | 3.8 | 38.9 | 11 |
| 5 | **RBAC pulled forward:** Epic 8 F3 — the authorization mechanism | 13.9 | 52.8 | 12–15 |
| 6 | Epic 2 · Patient Management — **slice 2A** | 30.7 | 83.5 | 16–23 |
| 7 | Epic 2 · Patient Management — **slice 2B** | 26.6 | 110.1 | 24–30 |
| 8 | Epic 2 · Patient Management — **slice 2C** | 12.9 | 123.0 | 31–33 |
| 9 | Epic 2 · Patient Management — **slice 2D** | 25.9 | 148.9 | 34–40 |
| 10 | Epic 5 · OAE Test Result | 13.1 | 162.0 | 41–44 |
| 11 | Epic 6 · ABR Test Result | 8.5 | 170.5 | 45–46 |
| 12 | Epic 7 · User Account Management | 28.8 | 199.3 | 47–54 |
| 13 | Epic 8 · Profile Management *(minus the pulled-forward F3)* | 14.6 | 213.9 | 55–58 |
| 14 | Epic 9 · Device Management | 20.8 | 234.7 | 59–63 |
| 15 | Epic 10 · Site Management | 9.5 | 244.2 | 64–66 |
| 16 | Epic 11 · Facility Management | 7.2 | 251.4 | 67–68 |
| 17 | Epic 12 · Location Management | 7.2 | 258.6 | 69 |
| 18 | Epic 13 · ABR Protocol Config | 7.0 | 265.6 | 70–71 |
| 19 | Epic 14 · DPOAE Protocol Config | 8.5 | 274.1 | 72–74 |
| 20 | Epic 3 · Data Exchange — **slice 32A, the file formats** | 50.6 | 324.7 | 75–87 |
| 21 | Epic 18 · Report Generation | 19.2 | 343.9 | 88–92 |
| 22 | Epic 19 · Language — **slice 19A, switching and the string pack** | 16.2 | 360.1 | 93–97 |
| 23 | Epic 21 · Device Communication + Firmware *(minus F1)* | 33.5 | 393.6 | 98–105 |
| 24 | Epic 3 · Data Exchange — **slice 32B, S4H** | 31.3 | 424.9 | 106–114 |
| 25 | Epic 24 · Alarms & Messages | 8.9 | 433.8 | 115–116 |
| 26 | Epic 31 · Logging: retention and protection | 3.2 | 437.0 | 117 |
| 27 | Epic 25 · Audit Trail | 14.2 | 451.2 | 118–121 |
| 28 | Epic 27 · About | 2.5 | 453.7 | 121 |
| 29 | Epic 28 · **Help** | 5.3 | 459.0 | 122–123 |
| 30 | Epic 30 · Installation *(minus the pulled-forward F4)* | 25.4 | 484.4 | 124–130 |
| 31 | Epic 19 · Language — **slice 19B, the three language phases** | 19.2 | 503.6 | 131–135 |
| 32 | Epic 33 · **Release: release testing and documentation** | 32.5 | **536.1** | 136–143 |

The work finishes in week 143. Adding the **5 weeks of review waiting that cannot be worked around** (§2.4) takes the schedule to **week 148**.

**Help (ASWD-28) sits in the final milestone**, at position 28 of 29 — immediately before Installation. That is deliberate: the help content has to describe finished screens, so writing it earlier means rewriting it.

**Why the investigation still comes forward to weeks 13–14.** Epic 21 (Device) is large and at weak confidence and does not start until late. Running its 3-day investigations early costs 6 days and surfaces a dead end in **month 3** rather than year 3.

### 7.2 Where the order fights the dependencies

| # | Problem | Options |
|---|---|---|
| ~~W1~~ | ~~Patient needs risk factors and comments from later epics~~ | ✅ **Resolved by D12** — both are now Patient Management features F8 and F9 |
| ~~W2~~ | ~~Patient needs the validation engine from a later epic~~ | ✅ **Resolved by D12** — the validation engine is now part of Patient Management F5 |
| ~~W3~~ | ~~Epic 9 (Device) comes before Epic 11 (Facility)~~ | ✅ **Resolved by D16.** Administration epics now run as one block, so Facility (week 82) still lands after Device (week 77). **Do Device F3 last within that block** |
| **W4** | **The “send sites and facilities to the device” setting inside Epic 10 F1 cannot finish until Epic 21** at week 134 | Build the setting now, wire it to the transfer when Epic 21 lands. Same pattern as the device system information in Epic 9 F3 |
| **W5** | **D14 puts database encryption at the very end (week 174).** Everything is built and tested against unencrypted databases | Accepted, because D8 means there is no customer data to protect during development. Epic 30 F3 must include a **full regression pass** — an encrypted SQLite provider changes the connection string and can change file-locking and performance behaviour |
| **W6** | **D15 puts the test result views (week 58) before Import (week 96).** The test tables exist by week 50, but nothing fills them — and D16 widens this gap from 6 weeks to 38 | Epic 5 **F2** solves it: a half-day utility that runs a real device file through the three parsers that already work, straight into the test tables. **This story is now load-bearing, not a nicety** — without it, the result views, the whole Administration block and 38 weeks of demos all run on invented data |

### 7.3 Milestones

Eight milestones, using your grouping and names. Start Monday **2026-08-17**, capacity 3.75 days/week, **no holidays counted (A6)**.

| MS | Name | Epics / slices | Days | Weeks | **Your target** | **1 developer** |
|---|---|---|---|---|---|---|
| **M1** | Solid ground · Logging · Patient records exist · The patient list is usable | Epic 0, investigations, Epic 1, **Epic 26 Logging**, Epic 2 **2A + 2B** | 110.1 | 1–30 | Sep-26 | **2027-03-12** |
| **M2** | Patient configuration · Risk factors and comments | Epic 2 **2C + 2D** | 38.8 | 31–40 | Oct-26 | **2027-05-21** |
| **M3** | Test results | Epics 5, 6 | 21.6 | 41–46 | Dec-26 | **2027-07-02** |
| **M4** | Administration | Epics 7, 8, 9, 10, 11, 12, 13, 14 | 103.6 | 47–74 | Feb-27 | **2028-01-14** |
| **M5** | Data in and out | Epics 3, 4 | 50.6 | 75–87 | Apr-27 | **2028-04-14** |
| **M6** | Reports and language | Epics 18, 19 | 35.4 | 88–97 | Jun-27 | **2028-06-23** |
| **M7** | Integration | Epics 20, 21 | 64.8 | 98–114 | Jul-27 | **2028-10-20** |
| **M8** | Finish and ship | Epics 24, 25, 27, **28 Help**, 30 | 111.2 | 115–148 | Sep-27 | **2029-06-15** |
| | | | **536.1** | | | |

**Exit criteria**

| MS | It is done when |
|---|---|
| **M1** | Builds from a fresh copy · CI green · all branches merged · shared base classes and two DbContexts done · Login's six requirements re-verified · **logging in place, with no patient-identifiable data in any log file** · **we know whether the device and S4H work is possible** · a patient can be created, viewed, edited and soft-deleted and **survives a restart** · Add/Edit/Delete and Save/Revert/Undo work · patient list with its six columns, search, the test list with its seven columns, mandatory-field validation and permissions enforced |
| **M2** | An administrator can set mandatory and active fields, the patient ID format, list sorting, confirmation prompts and validation rules · risk factors and comments are administrator-managed lists with translations, assignable to a patient · patient report prints · **Patient Management complete** |
| **M3** | TEOAE, DPOAE and ABR results viewable with waveforms, **demonstrated on real screening data** loaded through the parsers that already work |
| **M4** | Users with working account creation and password rules · profiles with real permissions · devices, sites, facilities, locations · both protocol types |
| **M5** | All five import formats parse **and save** · all available export formats write to the right folders with the right file names |
| **M6** | Patient reports print and export · the application runs in the agreed languages |
| **M7** | S4H sync works against a test endpoint · device connects over USB both ways · firmware updates |
| **M8** | **Help** available in the application · audit trail in place · **databases encrypted and verified** · installer proven on clean Windows 11 Pro and Enterprise · **all 196 requirements either built and tested, or formally out of scope** |

### 7.3.1 The target dates need about three developers, not one

The milestone **structure and grouping above are yours and are used exactly as given.** The dates are the problem, and it is worth being precise about the size of the gap rather than leaving it implied.

| MS | Your target | 1 developer | Gap | Developers the target implies |
|---|---|---|---|---|
| M1 | Sep-26 | 2027-04-23 | +7 months | **5.5** |
| M2 | Oct-26 | 2027-08-27 | +10 months | 4.7 |
| M3 | Dec-26 | 2027-09-24 | +9 months | 2.9 |
| M4 | Feb-27 | 2028-05-05 | +14 months | 3.2 |
| M5 | Apr-27 | 2028-09-01 | +16 months | 2.9 |
| M6 | Jun-27 | 2028-12-01 | +17 months | 2.6 |
| M7 | Jul-27 | 2029-06-22 | +23 months | 3.0 |
| M8 | Sep-27 | 2029-08-31 | **+23 months** | 2.7 |

**The whole-plan arithmetic.** From a 2026-08-17 start, Sep-2027 is **58 weeks**. The work is **574.9 developer-days**. That needs **9.9 developer-days a week** — which is **2.7 developers** at 3.75 effective days each, or **3.2 with the 20% buffer**. Allowing for coordination and peer review, **4 developers** is the realistic number for these dates.

**M1 is the hardest to hit and the most important to understand.** Its target implies **5.5 developers**, the highest of any milestone — and it is the one milestone where extra people help least, because its content is largely **serial**: the build must be fixed before CI can run, CI before the branch merge is verifiable, and the MVVM foundation before any screen work starts. Adding developers to M1 mostly creates waiting, not throughput. Even with four developers, M1 realistically lands in **Dec-2026 to Jan-2027**, not September.

**Three ways forward, and this is a decision rather than an estimate:**

1. **Staff to the dates** — 4 developers from the start. The dates become achievable from M2 onward; M1 still slips by roughly three months.
2. **Keep one developer and re-date the milestones** — the structure stays exactly as you have it; the dates become the "1 developer" column, finishing Jan-2030.
3. **Keep one developer and the Sep-2027 date, and cut scope** — §7.4 Option D lists the candidates. The largest realistic cut is ~104 days, which brings 574.9 down to ~471 and Jan-2030 to about **Jul-2029**. **That is still 22 months past Sep-2027**, so scope-cutting alone cannot close this gap.

Both date columns are carried in the plan and in the workbook so the gap stays visible rather than being resolved silently in one direction.

### 7.4 If the date is too late

No deadline was given. **If there is one earlier than about Feb 2030, one developer cannot meet it.** Here are the real options.

**Option B — 4 to 5 developers, 12 months.** 574.9 days plus 20% buffer is 690 days. Divided by 195 days per developer-year, that is **3.5 developer-years**. Four is the arithmetic minimum but not realistic — people need to review each other, and Epic 0 must largely finish before others can work in parallel. **Five developers** is the practical number, with the first 3 months at reduced parallelism.

There is a bonus: with 4–5 people, nearly all the review waiting disappears, because there is always other work and peers can review each other.

**Option C — split into two releases, keep 1 developer.**

**Release 1 — "AccuSync without the device": about 332.0 days → 111 weeks → ~2028-09-29** *(was 317.5; Logging moved in on 21 Aug 2026)*

| In Release 1 | Left for Release 2 |
|---|---|
| Epic 0 Revamp · Epic G Framework · Epic 1 Login | **Epic 32 slice 32B (S4H)** |
| Epic 2 Patient Management | Epic 5, 6 Test result views |
| **Epic 32 slice 32A** — the file formats | Epic 13, 14 Protocol config |
| Epic 7 Users · Epic 8 Profiles | Epic 15, 16 Risk factors and comments |
| Epic 9 Devices (list only) · Epic 10, 11, 12 Sites/Facilities/Locations | Epic 18 Reports |
| Epic 17 Field config (core only) | Epic 19 Languages beyond English |
| Epic 22 Network · Epic 23 OS | |
| Epic 27 About · Epic 28 Help | **Epic 21 Device Communication + Firmware** |
| Epic 29 DB Storage · Epic 30 Installation | Epic 24 Alarms · Epic 25 Audit |
| **Epic 26 Logging** ← *moved into Release 1, 21 Aug 2026* | |

**What Release 1 gives you:** an installable, encrypted app for managing patients, users, profiles, sites, facilities, locations and devices, in English, with file import.

**What Release 1 does not give you — and this is important:** **no connection to the AccuScreen Pro at all.** No export. No reports. No S4H. No waveform views. No audit trail. For a product whose job is to sync with a screening device, Release 1 is a useful internal milestone but **not a product you can sell.**

**Release 2 — the remaining ~304.5 days → 106 more weeks → ~2030-08.** Note that Option C does not finish sooner overall. It only moves a usable subset forward by about 17 months. That may still be the right call if early feedback matters, but the end date is unchanged.

**One thing that makes Option C easier than it looks.** Because nothing is delivered until all requirements are done (D8), a Release 1 in Sept 2028 would be an **internal** release — for demos, testing and feedback, not for a hospital. So it needs no upgrade path when Release 2 lands. That removes the usual cost of splitting a product into two releases.

**Option D — cut scope permanently.** Ranked by days saved:

| Cut | Days saved | What you lose |
|---|---|---|
| ~~Language Phases 2 and 3~~ | — | ~~9 languages~~ — **no longer available as a cut. Natus confirmed on 27 Aug 2026 that all phases must be complete at product launch** |
| CSV import and export | ~4 | 1 of 8 formats. **The only one with no spec** — HiTrack and OZ are now settled by the AccuLink source, so cutting them saves less than it looks |
| **Epic 32 slice 32B (S4H) entirely** | ~30 | The whole UK / NHSP market. Cheaper to keep than it was: the SEDQ contract is now known |
| Web service export | ~5 | File export still works. Probably covered by S4H (Q7) |
| Firmware update | ~11 | Firmware updated by a separate service tool |
| Context-sensitive help | ~4 | General help still works |
| Rename custom fields | ~3 | Field labels stay fixed |
| **Most you can realistically cut** | **~104** | Brings the total to ~532 days, **~147 weeks, ~Jun 2029** |

**Even the biggest realistic cut still takes about 2 years 10 months with one developer.** The limit is team size, not scope. Cutting scope helps, but it cannot turn a four-year job into a one-year one.

### 7.5 The buffer

The 20% buffer is **128 days (34 weeks)**, kept as a separate reserve. It is not hidden inside the estimates.

| Reserved for | Days |
|---|---|
| Epics 20 and 21 turning out worse than guessed (113.5 days at ±40%) | 45 |
| The other weak estimates (100 days at ±15%) | 15 |
| Re-running the early investigations, because they will be about 2 years old by the time we build | 8 |
| Holidays and leave (none counted in A6) | 20 |
| Reviewer not available (see R1) | 12 |
| Requirements changing — Rev 01 is the first release of a "living document" | 18 |
| Extra migration work from building tables epic by epic (D5) | 6 |
| Unallocated | 4 |
| **Total** | **61** |

**The migration reserve dropped from 13 days to 6 because of D8.** With no delivered database anywhere, we can throw away the whole migration set and regenerate it from the current model whenever it gets untidy. Previously each epic adding tables risked leaving a migration history we had to preserve forever.

**Rule:** report buffer used at every milestone. If we burn buffer faster than we burn schedule, twice in a row, we re-plan instead of quietly running out.

---

## 8. What we need answered

### 8.1 Open items already agreed to discuss later

| # | Item | Blocks | Needed by |
|---|---|---|---|
| ~~O1~~ | ~~Which table goes in which database?~~ — **answered by Natus 22 Aug 2026.** **Patient database:** all patient information and test data — patient details, risks and comments on a patient, test data and detail. **Settings database:** all configuration — field setup, risk factor list, comment list, protocols, sites, devices, users, profiles | Epic 2 F1 | ✅ closed |

### 8.2 Questions for the customer

Ordered by how much time each one puts at risk.

| # | Question | Blocks | Days at risk |
|---|---|---|---|
| ~~Q1~~ | ~~**How many languages ship in this release?**~~ — **answered 27 Aug 2026: all of them.** *"We start with Phase 1. By product launch, all phases have to be completed."* Translation is done by Natus’s regional distribution partners, and **strings must be reviewed and finalised before they are sent** | Epic 19 | ✅ closed |
| **Q2** | **Are password rules fixed or configurable?** GID-255020 says the rules are fixed. GID-254912 says an admin can set them to **None**. Both cannot be true. Also, None / Simple / Complex are never defined. And the code currently requires a special character, which no requirement asks for — is that wanted? Our suggestion: keep it configurable, drop "None", define Complex as GID-255020's rules and Simple as 8 characters | Epic 7 F2 | 4–8 |
| **Q3** | **What goes inside the CSV export file?** — **HiTrack and OZ are answered.** AccuLink declares both layouts in `HiTrackTest` (114 columns) and `Oz7Test` (binary). Only CSV is still open, and GID-256283/256284 still say *fixed width* for a format called CSV. Original question: The SRS gives exact folders and file names but never the file contents. No field list, no column widths, no reference to a spec. Also, "CSV fixed width" is a contradiction, and what is `state` in the file name? | Epic 4 F3 | 12–20 |
| **Q4** | **Is 90-day password expiry wanted?** It is in the code (`AuthenticationService.cs` lines 136-152) but in no requirement. The code's own comment warns that an expired user has no way to reset their own password — so a feature nobody asked for can lock everyone out on day 91. Related: **letting a user change their own password is now planned as its own story in Epic 7 F2**, because the Settings screen already exists for it. Our suggestion: build that, and remove the 90-day expiry | Epic 7 | 3–6 |
| ~~Q5~~ | ~~**Is the patient test report one feature or two?**~~ — **answered by moving it.** GID-254894 and GID-255112 describe the same report, so Epic 2 F10 was absorbed into **Epic 18 F2** on 27 Aug 2026 | Epic 18 | ✅ closed |
| **Q6** | **How does S4H mode work alongside normal operation?** Five S4H requirements contradict normal ones. GID-254977 says prevent admins adding users; GID-254905 says allow it. Same for editing users, facilities and risk factors, and for UK terminology. Is S4H a setting, a separate build, or always on? If it is a setting, every affected screen needs testing both ways | Epics 7, 8, 15, 16, 20 | 10–20 |
| **Q7** | **Is web service export in scope beyond S4H?** GID-254899 says export "via file or web service" but no contract or target system is defined anywhere except the S4H SEDQ URL. Our suggestion: confirm S4H covers it | Epic 4 last story | 8–14 |
| **Q8** | **We need DOC-076518 (Risk Assessment) and the Usability File.** GID-254925 is a safety-related requirement pointing at Hazard 6.4, which is in a document we do not have. GID-255018 is also safety-related but its Hazard ID says "TBD", and the SRS itself lists the Usability File as "TBD". Neither can be verified without these | Epic 9 F3 | 6–12 + compliance |
| **Q9** | **What happens when you delete something that is in use?** Nine requirements allow deletion but never say what happens to dependent records. Delete a Site with Facilities? A Profile with Users? A Device with test results? Only risk factors and comments have a rule. Patient is now settled (soft delete, D3). Our suggestion: block deletion while in use, and soft-delete for User as well as Patient | Epic 10 F2, and all delete stories | 8–14 |
| **Q10** | **We need the AccuScreen Pro protocol spec and a physical device.** Nothing exists for device communication. We also need to know: **USB or serial?** GID-254996 says USB; the architecture document says serial port. And: where does firmware come from, and how do we check it is genuine? | Epic 21 entirely, plus parts of Epics 1, 9, 20 | 30–50 |
| **Q11** | ~~**We need the SEDQ web service contract**~~ — **the contract is in the AccuLink source**: `northgate.wsdl` plus three XSDs, with `uploadData` and `downloadSyncData`. **A test endpoint is still needed**, and X4 asks whether that contract is still live. Original question: 25 requirements, zero code. We also need the sync data format, what triggers a sync, and what happens when a local edit clashes with an incoming value on a read-only field | Epic 20 entirely | 25–45 |
| **Q12** | **What should happen when there is no network?** GID-255006 states a requirement for network access but no behaviour, so there is nothing to test. Our suggestion: reword it as "detect no network before calling a web service, show an error naming the service, lose no data" | Epic 22 | 2–3 |
| **Q13** | **There is no requirement for automatic logout.** GID-255014 asks for a warning before it, but the logout itself is not specified — no timeout value, no statement about whether it is configurable, nothing about unsaved data at timeout. A parent requirement is needed | Epic 24 F1 | 2–4 |
| **Q14** | **Log files: what happens after one year, and can we reword GID-255037?** Retention is required but there is no purge rule, so logs grow forever. And "prevent deletion by any user" cannot be done against a Windows administrator. Our suggestion: rotate daily, purge after a configurable period of at least a year, and reword to "prevent deletion through the application" | Epic 26 F2 | 3–5 |
| **Q15** | **Who writes the help content?** GID-255040 and GID-255041 need help text, in what format, by when, and in how many languages. Building the viewer is our job; writing the content is not | Epic 28 | 4–8 |
| **Q16** | **Are the extra patient fields wanted?** GID-254883 does not list any fields, but the code's patient record has about 110, including `MotherSSN` and `CaregiverSSN`, plus referral and medical fields. SSN in particular affects de-identified export (GID-256279), what may appear in logs (GID-255034), and encryption. We need an agreed field list | Epic 2 F1 | 3–6 |
| **Q17** | **How do Roles and Profiles relate?** GID-255019 requires "at least two roles: Admin and Screener". Epic 8 defines fully configurable Profiles. The SRS never says how the two connect. The code has already invented a third role, `ReadOnly`, with no requirement. Our suggestion: Admin and Screener become seeded, non-deletable Profiles | Epics 8, 7, 2 | 4–8 |
| **Q18** | **Jira housekeeping** (§4): ASWD-15, ASWD-16 and ASWD-17 now have no work of their own — their content moved into ASWD-2 as features. Close them or relabel them as pointers. Also confirm Firmware Update (SRS §5.31) belongs inside ASWD-21 | Epic 2, Epic 21 F5 | 5–10 |
| **Q19** | **Confirm the named formats and languages are the whole scope.** Three requirements end with "[Additional formats/languages as required]". We cannot estimate an open list. We have estimated only the named items | Epics 3, 4, 19 | open-ended |
| **Q20** | **What is in §9 — the extra features in the code?** See the next section | Epic backlog | 10–45 |

**Four answers are needed in the first 15 weeks:** O1 (week 10, which table goes in which database) · Q10 (week 13, device protocol) · Q11 (week 14, SEDQ contract) · Q16 (week 16, the patient field list, before slice 2A models the Patients table).

**And two more before Patient Management slice 2D starts at week 43:** Q1 (how many languages — it sizes features F11 and F13) and the DF3 risk-factor decision in §6.

---

## 9. Extra features in the code that are NOT in the requirements

**This is the section to take to the customer.**

### 9.0 Natus decision on Phase 1 — 19 Aug 2026

Haidee Kachniewicz answered the Phase 1 question. Recorded verbatim:

> *"1 & 2 will not be included, but do not remove from the project. 3 & 4 for Phase 1. Export
> patient lists to CSV is under GID-254898. Prevent reuse of the last three passwords is under
> GID-255020. Others in 3 & 4 need to be added to the requirements."*

| Group | Decision | What that means for us |
|---|---|---|
| **A — Dashboards and screener workflow** | **Not in Phase 1. Do not remove.** | The code stays in the repository. It must keep compiling, must stay unreachable, and must not block the build. No requirements, no persistence, no tests |
| **B — Facesheet reading (OCR / PDF / Word)** | **Not in Phase 1. Do not remove.** | Same. **The build fix in Epic 0 F1 must therefore be the conditional-include fix, not deletion** |
| **C — Patient list features** | **In Phase 1** | CSV list export, custom list filter, ten report types, patient QR code |
| **D — Login and password** | **In Phase 1** | 90-day password expiry, last-three-passwords check |

**Two are already covered by existing requirements** — no new requirement needed:

- **Export patient list to CSV → GID-254898** (Supported Export Formats, which lists CSV). Now part of **Epic 32 F3**
- **Prevent reuse of the last three passwords → GID-255020**. Now part of **Epic 7 F2**

**Four need new requirements written by Natus before we build them:**

| Item | Now planned in | Blocked until the requirement exists |
|---|---|---|
| Custom / build-your-own list filter | Epic 2 F3 | Which fields are filterable, and whether filters are saved |
| Ten predefined report types | Epic 18 F1 | Which ten, and what each one contains |
| Patient QR code | Epic 2 F6 | Which fields go in the code, and the privacy position on displaying them |
| 90-day password expiry | Epic 7 F2 | The period, and what happens to a user on day 91 |

**Q4 is now answered.** The 90-day expiry **is wanted**. That makes the self-service password change
story in Epic 7 F2 **mandatory rather than optional** — expiry without a working way to change your
own password locks every user out on day 91.

**Option A in §9.11 is no longer available.** "Remove everything unrequested" is ruled out by
*"do not remove from the project."*

**Cost of this decision: +60.0 days** on the basis in use at the time, which took the plan from 662.0 to 722.0. The plan was later re-based onto the workbook's hours model (21 Aug 2026) and now totals **574.9**. The breakdown is in
§9.11.

---

### 9.1 How we found these, and how much to trust them

We read the code directly. Eight reviewers each took one area, wrote down what they found, and then a second reviewer went back to the code and tried to **disprove** each claim. That second pass mattered: it corrected **13 of the 40 claims it checked**, almost always in the same direction — something described as "working" turned out to be a screen with invented data behind it, or a screen nobody can open.

**A caveat you should know.** The review ran out of budget near the end. So:

| Items | Investigated | Independently checked |
|---|---|---|
| X1 to X7 (40 items) | ✅ | ✅ |
| X8 (34 items) | ✅ | ❌ **not checked** |

Treat the X8 findings as a first pass. They are still evidence-based and cite file paths, but nobody went back to challenge them.

**Headline: we expected about 11 extra features. We found 74.** Roughly 20,000 lines was the right order of magnitude, but it is spread across far more distinct capabilities than the earlier review showed.

### 9.2 Five findings that change the picture

These are more important than the individual feature list.

**1. The dashboards cannot be opened at all.**
Both the Admin and the Screener dashboard pages are hidden. In `SidebarNavigation.xaml:50-55` the Dashboard menu item is set to `Visibility="Collapsed"`, and **no code anywhere makes it visible again**. The Screener window is the same: `ScreenerDashboardWindow.xaml.cs:38-39` opens the Patients screen instead, with the dashboard line commented out.

So an Admin logs in and lands on Patients and can never reach the dashboard. This is not a working feature the customer might want to keep — **it is roughly 5,000 lines of unreachable code**, and eight of the dashboard dialogs are unreachable with it.

**2. Facesheet import is switched off too.**
The three facesheet options in the import dropdown are commented out in `ImportFileDialog.xaml:111-115` behind *"TODO: Re-enable when OCR/facesheet import is ready"*. A user of the built application cannot select them. And even if re-enabled, image OCR would fail immediately: the code looks for its language file in a `tessdata` folder next to the program, but the build copies it to `Resources/OCR/tessdata` instead. It has never worked.

**3. Several screens tell the user something happened when nothing did.**
The clearest cases:

| Screen | What it says | What it does |
|---|---|---|
| Not-exported dialog | "Export complete" | Writes no file, changes no record |
| Devices firmware dialog | Update complete | Runs no firmware update |
| Patient Information | "Patient saved" | Writes nothing |
| Import / Export config | "Configurations saved" | Persists nothing |

On a medical device a false confirmation is worse than a missing feature. These need to be either finished or disabled — not left as they are.

**4. One requirement we thought was partly met is actually unreachable.**
GID-255020 says a new password "cannot be the same as the three previously used passwords". That code exists and is correct. But the change-password screen can **only** be opened at first login, when the history is always empty — so the rule can never actually fire. The requirement is not met in practice.

**5. The developer back door can be switched on by an end user.**
Placing a file at `C:\ProgramData\AccuSync\skip_login.flag` makes AccuSync start with **no login at all**, as Admin. There is no `#if DEBUG` guard anywhere, so this code ships in Release builds, and ordinary Windows users can create files in `C:\ProgramData`. See §9.9.

### 9.3 What we listed as "extra" but is actually required

Two items were wrongly on the list. They belong to the plan, not to this discussion.

| Item | Why it is required after all |
|---|---|
| **Ribbon toolbar framework** | It is the delivery mechanism for GID-254873 to GID-254882 — the Add/Edit/Delete and Save/Revert/Undo buttons, on exactly the screens those requirements name. **Moved into Epic G.** Removing it would cost more (15 days) than keeping it |
| **Reusable screen frame** (toolbar + content area) | Same reason. It is the plumbing the ribbon sits in |

### 9.4 Group A — Dashboards and screener workflow (16 items, ~5,000 lines)

**Every item in this group is unreachable today** (finding 1), and every number on every screen is a hardcoded literal.

| # | Feature | What it would give you | State | Make real | Delete |
|---|---|---|---|---|---|
| X1a | Sidebar navigation shell | The app's only navigation. Not optional — without it no screen is reachable | Works, but permission-hiding never fires | 15d | 12d |
| X1c | Admin landing page — 6 stat tiles, 5 quick actions, weekly stats | A supervisor's morning overview: referred and passed this week, screenings today, what needs exporting, which devices need firmware | Unreachable, all numbers hardcoded | 12d | 1d |
| X1d | Referred / Pass / Incomplete patient lists | Click "4 referred this week" and see who they are, with risk-factor badges | Unreachable, 11 invented patients | 5d | 0.5d |
| X1e | Screeners active today | Who is on shift, how busy each is, who has gone quiet | Unreachable, 5 invented screeners | 6d | 0.5d |
| X1f | Not-yet-exported patients, with per-row Export | End-of-day safety net: results that would otherwise be lost | Unreachable; **Export button exports nothing** | 7d | 0.5d |
| X1g | Devices needing firmware | Which devices are out of date, and update from one place | Unreachable; **Update button updates nothing** | 6d | 0.5d |
| X1h | Screener dashboard + monthly activity calendar | A screener's home page: my worklist, what I finished, week-at-a-glance throughput | Unreachable, all hardcoded | 14d | 2d |
| X1i | My assigned patients worklist | The screener's daily to-do list | Unreachable, 5 invented babies | 10d | 1d |
| X1j | Completed screenings today | Confirm my own day's output | Unreachable, 3 invented records | 6d | 1d |
| X1k | Pending screenings with waiting time | "Nothing forgotten" check — who is still waiting and how long | Unreachable; waiting times are fixed strings that never age | 8d | 1d |
| X1l | My not-exported screenings | Screener's end-of-shift checklist | Unreachable; **Export button exports nothing** | 6d | 1d |
| X1m | Individual screener detail card | Per-person drill-down: their day, their patients, pass/refer split | **Dead code — nothing opens it** | 6d | 0.5d |
| X1n | Assign waiting patients to a screener | Triage: see who is waiting, who is urgent, tick several and hand them to a named screener | **Dead code — nothing opens it** | 12d | 0.5d |
| X1o | "What do you want to add?" chooser | Press Add anywhere, then pick the record type | **Dead code**; superseded by the ribbon Add button | 2d | 0.25d |
| X1p | 11 dashboard display models | The shapes behind the cards and badges | Hardcoded only; 4 are near-identical copies | 6d | 1d |
| | **Group total** | | | **~104d** | **~11d** |

**The decision hiding inside this group.** Four of these screens (X1i, X1k, X1m, X1n) assume a concept that **does not exist in the requirements or in the data model**: that a patient is *assigned to a named screener*. There is no such field, no such table, and none of the 196 requirements mentions it.

That is not a small gap. **Patient triage and assignment to staff is a whole capability.** If the customer wants it, it needs requirements, a data model and its own estimate — most of the ~104 days above is that, not the screens. If they do not want it, four of these screens have no purpose.

**Ask the customer this first: is assigning patients to individual screeners part of AccuSync?** The answer decides most of this group.

### 9.5 Group B — Reading patient details from facesheets (5 items, ~3,200 lines)

A "facesheet" is the admission summary a hospital already prints for a newborn. This feature reads the patient's details off it automatically.

| # | Feature | What it would give you | State | Make real | Delete |
|---|---|---|---|---|---|
| X2a | Read from a scanned photo (OCR) | Photograph the paper sheet, get ~10 fields plus 16 risk factors pre-filled | **Has never worked** — looks for its language file in the wrong folder | 20d | 1d |
| X2b | Read from a PDF | Same, from a PDF. The most likely of the three to actually pay off — clean text, no OCR errors | Code runs, but unreachable and saves nothing | 12d | 0.5d |
| X2c | Read from a Word document | Same, from .docx | Code runs, but unreachable and saves nothing | 8d | 0.5d |
| X2d | Facesheet review and correction screen | The safety net: source document on the left, extracted values on the right, correct anything wrong before it enters the record | Form works; **cannot be opened; saves nothing** | 10d | 1d |
| | **Group total** | | | **~50d** | **~3d** |

**The value if it worked:** a screener stops re-typing about fifteen fields per baby, and transcription typos disappear. On every single patient. That is genuine, repeated time saved.

**Three things to weigh against that:**

1. **It is the reason the solution does not build from a clean copy.** The project file requires an OCR language file that `.gitignore` excludes. Deleting this feature permanently fixes that.
2. **The Word patterns look like they were tuned against fake documents.** Several patterns search for literal `**` Markdown bold markers. Real Word files store bold as formatting, never as asterisks. That strongly suggests the samples were Markdown converted to .docx, not real hospital facesheets.
3. **The extraction technique is fragile.** PDF text comes out in drawing order, not reading order, and facesheets are multi-column forms — so a label and its value are often far apart in the text, while every pattern assumes they sit together.

**Our reading:** this is a good idea that was never finished and never tested against real documents. If the customer wants it, budget the 50 days and get real facesheets to test with. If not, deleting it is cheap and fixes the build.

### 9.6 Group C — Patient record fields and behaviour (16 items)

| # | Feature | What it would give you | State | Make real | Delete |
|---|---|---|---|---|---|
| X7a / X8r | **Social Security Number** for mother and caregiver | The identifier US state screening registries and insurers use to match a baby's result to the right family record | On screen, unmasked, **never saved** | 10d | 0.5d |
| X7b | Full postal and contact details for mother and caregiver (32 fields) | How you reach a family to book follow-up when a baby fails a screen. Failing to reach families is the main reason babies are lost to follow-up | On screen, **never saved, never filled from imports** | 8d | 3d |
| X7c | Audiology referral tracking (5 fields) | The paper trail proving a referred baby was actually handed on to an audiologist, and who to chase | On screen, **never saved** | 5d | 1d |
| X7d | Medication, Physician, Audiologist | Some newborn medications are a recognised hearing-loss risk factor, so this has direct clinical relevance. The other two answer "who do I contact about this baby" | On screen, **never saved**; 2 of 3 have no database column | 4d | 0.5d |
| X7e / X8q | International phone numbers, 15 countries | Foreign numbers written in a shape a caller recognises, so follow-up calls do not mis-dial | Formats live; **country choice is not stored anywhere** | 5d | 2d |
| X7f | 12 clinical dropdown lists | Stops "M", "Male", "male" and "boy" landing in the same column — which is what makes registry reporting possible | ⚠ **11 of the 12 are broken** — see §9.10 | 4d | 1d |
| X7g / X8s | Automatic name capitalisation | Tidier printed names | ⚠ **Actively corrupts names** — see §9.10 | 2d | 0.5d |
| X7h | Fields with no screen at all | Middle initial is read from all three device formats then thrown away. Source-tracking fields would let a repeat import recognise a patient it has seen before | Model/schema only, no UI, nothing saved | 6d | 1d |
| X8t | 16-question risk factor form with progress tracking | A structured clinical checklist with live "12 of 16 answered" feedback, so risk factors get skipped less often | ⚠ **Design conflict** — see §9.10 | 10d | 2d |
| X8n | Patient list paging, sorting, multi-select, **bulk delete** | Makes a long ward list usable — page through it, sort by surname, act on a batch | Works in memory only | 8d | 2d |
| X8o | Export the patient list to CSV | Pull the current list into a spreadsheet for a handover sheet or a chase list | ✅ **Genuinely works — writes a real file** | 3d | 0.5d |
| X8p | Build-your-own list filter | Filter on several fields at once instead of one search box | Works in memory only | 6d | 1.5d |
| X8u | Ten predefined report types | Pick exactly which report to print — a detailed ABR report for a referral, a basic summary for parents | Choices only; nothing renders | 15d | 1d |
| X3a | Patient QR code on screen | Point a phone at the screen and read the patient's name, ID and date of birth off it | Works; display only | 4d | 0.5d |
| | **Group total** | | | **~90d** | **~17d** |

**Two of these need a decision beyond keep-or-delete:**

**SSN (X7a / X8r) is the most privacy-sensitive thing we found.** It is displayed in full, unmasked, selectable and copyable. The code's own comment says *"SSNs are sensitive PII. Ensure values are masked in the UI and never written to logs"* — and neither is done. It also touches three requirements: de-identified export must strip it (GID-256279), logs must not contain it (GID-255034), and it must be encrypted at rest (GID-256510). **If SSN is kept, it needs a requirement and a privacy decision, not just a field.**

**The QR code (X3a) carries patient name, ID and date of birth as plain readable text.** Anyone who can photograph the screen from across a ward gets the patient's identity. That is a privacy decision, not a technical one.

### 9.7 Group D — Login and password extras (5 items)

| # | Feature | What it would give you | State | Make real | Delete |
|---|---|---|---|---|---|
| X4a | 90-day password expiry | Standard security hygiene — stale passwords forced out after three months | ⚠ **Works, and locks users out permanently** | 4d | 0.5d |
| X4c | Forced password change at first login | The shared factory password cannot stay in use. This is what makes individual accountability real | Works; only ever fires for the 2 seeded accounts | 2d | 0.5d |
| X4d | Last-three-passwords check | Stops users cycling between two familiar passwords | Code correct, **can never fire** (finding 4) | 2d | 0.5d |
| X4e | Mandatory special character | Marginally stronger passwords | Works — but conflicts with GID-254912 | 2d | 0.5d |
| X5a | A third "ReadOnly" role | Auditors, supervisors or trainees could view screening records without changing or exporting them | Never used; would behave identically to Screener | 10d | 0.5d |
| | **Group total** | | | **~20d** | **~2.5d** |

**Self-service password change was removed from this group on 19 Aug 2026.** The application already has a **Settings** screen with a Change Password section, so this is a real part of the product rather than an unrequested extra. It moves to **Epic 7 F2** as its own user story. What it needs there: the Settings screen currently reports *"Settings saved"* and **writes nothing** (`SettingsContentView.xaml.cs:94` — the developer's own note says *"Saves snapshot only — doesn't persist password change to database"*), it has **no current-password field**, and its eye toggle puts the password into a plain `TextBox`. All three are fixed in that story.

**X4a is the one to decide quickly.** We traced day 91 exactly: the user types the correct password, it verifies, the age check fails, and they are refused with *"contact your administrator."* Today they **cannot** recover on their own, because the only working route to a password change is the first-login path, which has already been used. Once Epic 7 F2 makes the Settings screen work this stops being a lockout, but until then a feature nobody asked for can lock every user out of the product three months after go-live.

**X4c is the best value in this group** — about 25 lines, resting on a screen that already exists, and it is what turns "everyone on the ward knows the Admin password" into real individual accountability.

### 9.8 Group E — Configuration screens beyond the requirements (7 items, ~3,300 lines)

| # | Feature | What it would give you | State | Make real | Delete |
|---|---|---|---|---|---|
| X8z | **33-permission profile matrix** | Build precise roles — a supervisor who views everything and edits patients but cannot delete users | Hardcoded; 936 lines | 14d | 2d |
| X8e | **Per-device** field configuration | Different data-entry rules on the NICU device than the well-baby nursery device | Hardcoded; near-duplicate of the system-level screen | 9d | 1d |
| X8g | Named import profiles with passwords | Pre-defined repeatable import jobs, password-protected | Hardcoded; **plain-text "1234" password** | 8d | 1d |
| X8h | Named export profiles | Pre-defined repeatable export jobs — "weekly HiTrack submission to this folder" | Hardcoded | 7d | 1d |
| X8j | Auto-lock after inactivity | How long an unattended workstation stays open before locking — a real ward privacy control | Setting only; **no timer exists** | 5d | 0.5d |
| X8b | Duplicate patient detection | Prevents duplicate baby records when the same file is imported twice; choose Replace / New / Add tests / Skip per patient | **Dead — the trigger can never occur** | 10d | 1d |
| X8a | Multi-step import review workspace | See what is inside a device file and choose which babies to bring in, before anything is committed | Parsing is real; **import saves nothing** | 12d | 2d |
| | **Group total** | | | **~68d** | **~8.75d** |

**Three items left this group on 19 Aug 2026.** Natus confirmed all three already exist in **AccuLink**, so they are carried-over product behaviour rather than unrequested extras, and AccuSync already has the screens for them. They are now planned work:

| Was | Now planned in | What still has to be built |
|---|---|---|
| **X8k** Definitions of None / Simple / Complex | **Epic 7 F2** | The definitions are currently invented in a resource file — *"Simple = 4 characters"*. **Q2 still has to confirm what each level enforces**, because a 4-character password option on a medical device needs explicit sign-off |
| **X8i** Own display language on the Settings screen | **Epic 7 F1** | The dropdown exists but `GetSelectedLanguage()` has no callers, so the choice is never stored or applied. Needs Epic 19 before the change is visible |
| **X8f** Rename all 62 patient fields | **Epic 2 F6** | The screen exists but the labels are hardcoded. Note the widened scope: GID-255466 asks only for the 4 spare fields, and this covers all 62 including Patient ID and Date of Birth |

**Still unanswered in this group:** **X8j** invents an auto-lock setting for a feature no requirement describes — that is open question **Q13**.

### 9.9 Group F — Developer back doors (2 items) — a security decision

| # | Feature | State | Delete |
|---|---|---|---|
| X5b | **Skip the login screen entirely** | Works. Ships in Release | 1d |
| X5c | Jump straight to a chosen screen | Works. Ships in Release | 1d |

**Exactly how X5b works:** if a file exists at `C:\ProgramData\AccuSync\skip_login.flag`, AccuSync starts with no username or password prompt. The file's contents pick the username and role; if it is empty or unreadable, **the code defaults to Admin**. The only visible sign is `[DEV: skip_login]` in the window title.

**The three facts that matter:**

1. **It is switched on by a file, not by a compile setting.** No environment variable, no registry key, no build symbol.
2. **There is no compile-time guard.** A search for `#if` across the WPF, Application and Presentation projects returns nothing. The `RELEASE` symbol is defined in the project file but never used to exclude this code.
3. **An ordinary Windows user can create that file.** `C:\ProgramData` grants `BUILTIN\Users` permission to create folders by default.

So on a shipped medical device holding patient data, **any user of the machine can bypass authentication entirely and become Admin by creating one text file.**

The genuine development value is real — a developer saves several clicks on every rebuild. But that is worth about 2 days a year, and it does not need to ship. **Epic 0 F1 removes it from Release builds regardless of what the customer decides**, because this is not a scope question.

### 9.10 Defects found while investigating — these need fixing whatever you decide

These are not scope decisions. They are bugs, and they exist inside features the requirements *do* ask for.

| # | Defect | Why it matters | Fix |
|---|---|---|---|
| **DF1** | **11 of the 12 clinical dropdowns are broken.** Gender, birth location, nationality, consent, NICU, tracking consent, and the language and country lists all bind `SelectedValue` without `SelectedValuePath` | The value stored is the list item object, not its text — and a saved value cannot be matched back to a list entry when the record is reopened. **Patient demographic data does not round-trip.** Affects GID-254883 | 2d |
| **DF2** | **Name auto-capitalisation corrupts surnames.** It runs on every keystroke and rewrites O'BRIEN to O'brien, McDONALD to Mcdonald. It also collapses spaces, so "Van Dyke" cannot be typed | Silently corrupting a family's surname in a medical record is worse than not helping at all. Affects GID-254883 | 1d |
| **DF3** | **Two incompatible models of risk factors exist side by side.** The patient screen shows a hardcoded 16-question form; the requirements (GID-254950, GID-254951) say risk factors are administrator-configurable | One of the two must be rebuilt. This affects Epic 2 F2 and Epic 15 — **please read this before starting either** | in scope |
| **DF4** | **Bulk-delete of patient records** with no audit entry and no rules about dependent test results | Deleting many patient records at once, on a medical device, with no audit trail. Soft delete (D3) mitigates but does not remove the concern | 1d |
| **DF5** | **False success messages** in at least four places (§9.2 finding 3), including the **Settings** screen, which says *"Settings saved"* and writes neither the password nor the language | Users are told data was saved or exported when it was not | 1d |
| **DF6** | **Plain-text default passwords.** `"1234"` in the import config screen and in the user screen; `"12345"` for the two seeded accounts | Violates GID-255020. Already covered by Epic 7 F2 | in scope |
| **DF7** | **The seeded Admin/Screener accounts are the only accounts that can exist.** `CreateUserAsync` is called from exactly one place — the seeding code. There is no working way to create a real user | Explains why the first-login and password-history features can never fire in practice | in scope (Epic 7) |
| | **Total outside existing scope** | | **~5d** |

### 9.11 What it costs

| Option | Days | What you get |
|---|---|---|
| ~~A — Remove everything unrequested~~ | ~~47~~ | ❌ **Ruled out by Natus 19 Aug 2026** — *"do not remove from the project"* |
| **B — Keep everything: requirements, persistence and tests for all of it** | **~344** | You keep it all. The customer would need to write roughly 60–80 new requirements. **This more than half again the size of the whole project** |
| **C — Keep the genuinely valuable, remove the rest** *(our suggestion)* | **~95** | See below |

**What Option C keeps:**

| Keep | Days | Why |
|---|---|---|
| Patient contact and referral fields (X7b, X7c, X7d) | 17 | These are how you reach a family for follow-up. Closest to what GID-254883 already asks for |
| Forced password change at first login (X4c) | 2 | Cheapest real security win in the codebase |
| Patient list paging, sorting, filter (X8n, X8p — **without** bulk delete) | 13 | Near-essential for a long ward list |
| CSV list export (X8o) | 3 | Already works and produces a real file |
| Database backup before upgrade (X8w) | 0 | **Already built and working.** Free to keep |
| Config file for database location (X8x) | 2 | Lets IT place patient data per hospital policy |
| Automatic created/modified timestamps (X8y) | 1 | Good practice; feeds audit later |
| 33-permission profile matrix (X8z) | 14 | A legitimate elaboration of GID-254918, which Epic 8 needs anyway |
| 16-question risk factor form (X8t) | 10 | Real clinical content — but resolve DF3 first |
| Defect fixes DF1, DF2, DF4, DF5 | 5 | Not optional |
| International phone formatting (X7e) | 5 | Modest cost, real benefit for follow-up calls |
| Remove: dashboards, facesheet import, QR, dev back doors, ReadOnly role, duplicate config screens | 23 | |
| **Total** | **~95** | |

**What Option C removes, and what that costs you:**

- **The dashboards** — nobody can open them today, so nothing is lost that anyone currently has. But it means giving up the whole supervisor-overview and screener-worklist idea unless the customer wants patient assignment as new scope (§9.4).
- **Facesheet import** — gives up the re-typing saving, and permanently fixes the build.
- **The QR code** — removes a patient-privacy exposure.
- **90-day password expiry** — removes a lockout trap nobody asked for. If the customer's security policy requires expiry, it needs a requirement **and** a self-service reset path first.

### 9.12 The questions to put to the customer

In priority order:

1. **Is assigning patients to individual screeners part of AccuSync?** This single answer decides most of §9.4 — about 40 of the 104 days there.
2. **Were the dashboards asked for somewhere we have not seen** — a UX specification, a demo, or DOC-075955 User Requirements? If yes, they need requirement IDs. If no, they are unreachable code.
3. **Is facesheet import (OCR/PDF/Word) a real need, or was it a trial?** GID-254900's "[Additional formats as required]" is the one clause that could cover it.
4. **Is capturing Social Security Numbers intended?** If yes it needs a requirement, masking on screen, and decisions about de-identified export and logging.
5. **Do you want 90-day password expiry?** It only works once the Settings screen can actually change a password — that is now a story in Epic 7 F2. Without it, expiry locks everyone out on day 91.
6. **Ratify or replace the invented definitions** of Simple and Complex passwords (§9.8). "Simple = 4 characters" is currently written into the product.
7. **Confirm the developer back doors are excluded from Release.** We are doing this regardless; we want it on the record.

---

## 10. Main risks

| # | Risk | Chance | Impact | Days | What we do about it |
|---|---|---|---|---|---|
| **R1** | **One part-time reviewer gates everything.** Every PR and every HLD waits on one person | High | High | 12–40 | Keep PRs under 400 lines so a review takes 30 minutes. Fixed weekly HLD review slot. Name a backup reviewer. Escalate if a PR sits 3 days |
| **R2** | **Only one developer knows the system.** All 82 commits are by one person. Illness or resignation stops the project | Medium | Very high | 20+ | Every epic gets a written, reviewed HLD before coding. Keep `main` always releasable. Never leave work unmerged more than a week |
| **R3** | **Part of the verification and regulatory work is still not in this estimate.** [Epic 33](#epic-33) now covers the **manual test pass against all 196 requirements**, **installing on a clean Windows machine**, and the **user documentation**. It does **not** cover the integration test suite, requirement traceability, the IEC 62304 lifecycle records or the ISO 14971 risk file — those five features were cut on 26 Aug 2026 | High | High | **~50 not estimated** | **Name the owner.** On a medical device these are mandatory, so the likely answer is that Natus’s quality and regulatory function produces them. If it is Soliton, they come back into the plan at roughly the figures listed in §Epic 33 |
| **R4** | **No device or protocol for Epic 21.** 60 days of work becomes impossible | Medium | High | 62–75 | Run the investigation in week 15, not week 144. Escalate on day 1 if the inputs are missing |
| **R5** | **No S4H contract for Epic 20.** 53 days becomes impossible | Medium | High | 53 | Same — investigate in week 16 |
| **R6** | **Requirements will change.** Rev 01 is the first release and the document calls itself "a living document". 196 requirements over 3.5 years will move | High | Medium | 18–40 | 18 buffer days reserved. Re-plan at every milestone. Every change needs its own estimate |
| **R7** | **No holidays are in the plan (A6).** Weeks 19–20 already fall over Christmas | High | Medium | 20+ | 20 buffer days reserved. **Get the real holiday calendar and re-plan.** This is a known gap, not a risk |
| **R8** | **The early investigations go stale.** We investigate in weeks 15–16 but build in weeks 130–159. Firmware and web services will have moved on | High | Medium | 8–20 | 8 buffer days to re-investigate. Write findings as proper documents, not notes |
| **R9** | **Design knowledge is lost when the `.sql` files are deleted.** They are the only record of 18 indexes, 33 permission fields and all protocol parameter ranges | Medium | High | 8–15 | Epic 0 F1 captures this first. The PR that deletes the files must link to the capture document |
| **R10** | **No requirements for performance or capacity.** If we hit a limit late, persistence has to be redesigned | Medium | High | 15–30 | Set provisional targets in the Epic 0 HLD (say 100,000 patients, 500,000 tests) and test against them from Epic 2 onwards |
| **R15** | **Data from an earlier AccuSync may still need importing.** There is old build output from a `net8.0-windows` app in the repo, which suggests a previous version exists. D8 removes the *database upgrade* problem, but not this: if hospitals hold data in an older product, someone has to get it in | Medium | Medium | 10–25 | Ask the customer directly whether any existing installation holds data that must come across. If yes, it is new scope and needs a requirement and an estimate |
| **R11** | **Merging 12 branches at once could break the one working feature.** 81 commits go into `main` with no CI yet | Medium | Medium | 3–8 | Fix the build first, run all 103 tests before and after, and set up CI immediately after |
| **R12** | **The partner translation round trip is outside our control.** Natus’s regional distribution partners supply the words — **13 languages, about 31,400 words** — and none of it can be sent until the strings are reviewed and frozen | Medium | High | 10–30 | Freeze the strings in Epic 19 F3 and send the pack the same week. Track the 13 partner sets individually. **A slow region holds up the release and no development effort fixes it** |
| **R13** | **Building tables epic by epic (D5) costs more than doing it once.** More migrations, and some rework when a later epic needs a table an earlier one made | Medium | Low | 6 | 6 buffer days reserved — reduced from 13 by D8, since with nothing delivered we can discard and regenerate the whole migration set at will. Keep entity design reviews consistent across epics |
| **R14** | **Extra features (§9) stay undecided for years.** 20,000 lines sit in the product unverified | High | Medium | 10–45 | **Get an answer to Q20 by week 25**, even though the work itself comes much later |

---

## 11. How we work

### 11.1 A user story is done when

1. Its epic's HLD is approved **before** coding started
2. The code works and meets the story's acceptance criteria
3. Unit tests are written and passing, hitting 80% on new code in Core, Application, EF and Presentation
4. **All 103+ existing tests still pass.** The count never goes down
5. Self-review is complete (checklist below)
6. The PR is reviewed and approved
7. Review comments are fixed, or answered with a written reason
8. It is merged to `main` with CI green
9. The requirement IDs it delivers are recorded against it
10. Documents are updated if the story changed something they describe
11. No new TODO or BUG comments without a ticket

### 11.2 Self-review checklist

- [ ] `Core` references nothing. `Application`, `EF` and the adapters reference only `Core`. `Presentation` has no WPF reference
- [ ] No database entity is stored on a ViewModel property or bound to a screen. Converters take entities and return DTOs
- [ ] No `MessageBox.Show` and no business logic in screen code-behind. Use `IDialogService`
- [ ] No hardcoded text shown to users — everything goes through resx
- [ ] No patient-identifiable data in any log message
- [ ] No new `.sql` files. The schema comes from the C# model
- [ ] Migrations are additive, and a fresh install was actually tested
- [ ] No passwords, secrets or developer shortcuts
- [ ] Tests cover the failure cases, not just the happy path
- [ ] The PR is under 400 changed lines

### 11.3 Branching and PRs

Keep the existing naming, which works:
`users/<name>/feat/AccuSync-<epic>-<story>-<Description>`

**One change, and it is important.** Today there are 12 branches stacked on each other and `main` is 81 commits behind. From Epic 0 F1 onwards: **every story branches from `main` and merges back to `main`.** No stacking. Stacked branches on a one-person project just hide integration problems, which is exactly the situation now.

- No branch lives longer than **5 working days**. If a story cannot merge in a week, it is too big — split it
- **PRs stay under 400 changed lines** (not counting generated migration files). This is the main defence against R1: a 400-line PR is a 30-minute review, which is what makes a 1.5-day turnaround realistic
- **One story in progress, one in review. Never two in progress**

### 11.4 HLD or a short design note?

| Write a full HLD (once per epic) when the work | Write a short note in the PR when the work |
|---|---|
| Adds or changes a table or a migration | Is a bug fix with an obvious cause |
| Crosses a project boundary | Only touches one file or one screen |
| Adds a new interface to `Core` | Follows a design an approved HLD already covers |
| Picks a third-party library | Is a build or config change |
| Is safety-related (`U*`: GID-255018, GID-254925) | Only adds tests |
| Touches login, encryption, audit or logging | Is a rename or a mechanical tidy-up |
| Settles a question from §8 | |

Use the existing template. `docs/design-documents/AccuSync/User-Authentication-and-Access-Control.md` is a good model — keep its "Alternative implementations" and "Open issues" sections, because that is where the thinking that can be reviewed actually lives.

### 11.5 Re-planning

We re-plan at **every milestone** — 10 times. Each time we produce:

1. Estimated vs actual days for the finished milestone
2. Real velocity against the 3.75 days/week assumption. If two milestones in a row are more than 15% off, **we change the assumption** rather than defend it
3. Buffer used and buffer left
4. A week-by-week breakdown of the next milestone
5. New estimates for anything whose investigation has since finished — Epics 20 and 21 must be re-estimated right after weeks 15–16
6. Status of the §8 questions and what each still costs
7. A check that all 196 requirements are still accounted for

**Re-plan immediately, out of cycle, if:** an answer changes an epic's design · Rev 02 of the SRS arrives · an investigation moves an estimate by more than 25% · review turnaround goes past 3 days for two weeks running · the team size changes.

**Between milestones:** a one-page weekly status — stories merged, story in progress, story in review, days used vs planned, blockers, open questions. On a project this long with one developer, that is the only early warning we have.

---

## 12. Checks

| Check | Result |
|---|---|
| All 196 requirements assigned to an epic | ✅ Verified by script — 196 of 196, none missing, none invented |
| All 25 epics have features and user stories | ✅ 67 features, 224 user stories — counted by script from the feature tables in §6, and reconciled against the §5 summary. **Features were combined from 127 to 61 on 19 Aug 2026** so that no feature is merely story-sized |
| All epics are in the order table (§7.1) | ✅ 29 rows, including the pulled-forward investigations and Patient Management as four slices. Days column sums to 536.1, matching §5 |
| Milestones use the customer's grouping | ✅ 8 milestones, §7.3, with both the target and the computed date shown |
| Help is in the final milestone | ✅ Epic 28, position 28 of 29, inside M8 |
| Each epic's feature days add up to its stated total | ✅ Checked by script — 0 mismatches. Earlier runs of this check found and fixed two real errors: Epic 0 was understated by 8.5 days and Epic 30 by 2.0 |
| Epic 2's 171.0 days is auditable | ✅ Broken down activity by activity in §6, and split into four delivery slices |
| Every estimate covers all nine activities | ✅ §2.4 |
| Buffer kept separate, not hidden | ✅ §7.5 |
| Requirement conflicts flagged, not silently fixed | ✅ 20 questions in §8 |
| Features in the code with no requirement listed | ✅ **74 items** in §9, found by reading the code. 40 were independently re-checked; the other 34 were not (§9.1) |
| Defects found that sit inside required features | ✅ 7 listed in §9.10. ~5 days of fixes outside the current estimate |

**Two things this plan does not cover, stated openly:**

1. **Weeks 15 onward are at epic and milestone level, not week by week.** Detail is added one milestone at a time (§11.5). A 175-row week table would be false precision when 38% of the estimate is still weak.
2. **Verification and regulatory work is not estimated at all** (R3). On a medical device this is the biggest gap in the document — and it is a gap in *scope definition*, not analysis. Nobody has told us who owns it.

---

*Answers to §8 can be written straight into this document. Estimates and dates will be updated on the next pass.*
