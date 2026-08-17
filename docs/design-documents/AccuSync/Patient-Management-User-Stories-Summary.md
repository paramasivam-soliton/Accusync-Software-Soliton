# Patient Management — User Stories Summary (US7–US12)

Working notes covering the six Patient Management stories that sit on top of the
Patient Persistence Layer HLD. For each story: what's asked, what's already done
in the codebase, what's left to do in scope, what's out of scope, and the open
doubts to confirm with the lead — each doubt includes a recommended answer and
the alternative, so a decision can be made quickly rather than starting from a
blank question.

A cross-cutting note before the per-story breakdown: across US9–US12, the same
gap repeats — the Patient screen's Save/Delete buttons currently only update
the in-memory ViewModel/collections and never call the repository. Add save,
edit save, single delete, and bulk delete all have this same missing wiring.
Worth treating as one shared piece of work rather than four separate ones.

---

## ASWD-84 — Create the patient tables and repository

**Asked:** Patients and PatientContacts tables via a code-first migration, a
repository with create/read/update/soft-delete/list, a service layer above the
repository that screens talk to, `IsDeleted` defaulting to not-deleted, every
read filtering out soft-deleted records unless explicitly asked not to,
`SocialSecurityNumber` never logged, and unit tests for create/read/update/
soft-delete/hidden-records/baby-only-row.

**Done:**
- Patients + PatientContacts tables and migration in place.
- `IsDeleted` defaults to `false` at the database column level.
- Repository create/read/update/soft-delete/list all implemented.
- `SocialSecurityNumber` is not logged anywhere in the codebase.
- Repository test suite covers create, read, update, soft-delete-exclusion,
  soft-delete-inclusion, search, paging totals, and timestamp interception.

**To do (in scope):**
- Add the six fields the story explicitly flags as missing: `AudiologyReferral`,
  `ReferralDate`, `Physician`, `Audiologist` on Patients; `TimeOfBirth`,
  `Address2` on PatientContacts — plus the migration for them.
- Build the service layer (`IPatientService`) — screens currently call
  `IPatientRepository` directly.
- Fix `GetByIdAsync`, which does not filter `IsDeleted` at all today.
- Add an explicit unit test for a patient with only a baby row (no mother or
  caregiver row).

**Out of scope:**
- UI wiring for add/edit/delete (covered by ASWD-54/55/56).
- `TestSession`/`TestRecord` entities — not mentioned in this story; already
  built ahead of scope under the earlier HLD.

**Doubts for lead:**
1. How thick should "patient service" be?
   - **Recommended:** A thin `IPatientService` wrapping `IPatientRepository`
     with the same method shapes — no business logic exists yet to justify
     more, and it satisfies the AC literally.
2. What types/nullability should the six new fields have?
   - **Recommended:** Mirror sibling columns — `AudiologyReferral`/`Physician`/
     `Audiologist` as `string?` (like `ReferralFrom`/`ReferralTo`),
     `ReferralDate` as `DateTime?`, `TimeOfBirth` as `DateTime?` (paired with
     `DateOfBirth`), `Address2` as `string?` (paired with `Address1`).
   - **Alternative:** Check the AccuLink source XML schema directly for exact
     types before deciding — more precise, slower.
3. Should `GetByIdAsync` filter soft-deleted records? — **Decided:** always
   exclude deleted patients, no override parameter (see cross-story decision
   under ASWD-56 below, which settles this the same way for
   `GetTestsForPatientAsync`).
4. Should `PatientContacts` get its own `IsDeleted` cascade flag?
   - **Recommended:** No — keep the current implicit hide-via-parent design;
     it already satisfies "hides contacts too" with less schema complexity.
   - **Alternative:** Add an explicit `IsDeleted` to `PatientContacts` for
     future direct-contact queries — over-engineering for current needs.
5. How should `SocialSecurityNumber`-never-logged be enforced?
   - **Recommended:** Add a redacted `ToString()` override on `PatientContact`
     as a cheap structural guardrail, rather than relying purely on convention. This will secure when logging uses contact object where we override to string method, in a way ssn is neglected. If we're using any extensive logging mechanisim in future like serilog with json etc, we need to upgrade the never logged mechanism. Since, How we are logging is unclear - paritally out of scope and implementing basing gaurd.
6. Should `ContactType` be constrained?
   - **Recommended:** Shared string constants (`"Patient"`/`"Mother"`/
     `"Caregiver"`) used consistently across mapper/repository — no DB schema
     change.

---

## ASWD-53 — Build the patient presentation layer

**Asked:** A patient DTO in `AccuSync.Presentation`, a converter that turns an
entity into a DTO and back, no database entity ever stored on a ViewModel or
bound to a screen, the patient ViewModel using the shared base classes from
Epic 0, `AccuSync.Presentation` having no reference to WPF, and unit tests
covering conversions both ways including empty and missing values.

**Done:**
- `PatientMapper` converts both ways (`ToViewModel`/`ApplyTo`/`ToListRow`/
  `ToEntity`).
- No raw entity is ever bound to a screen — confirmed by inspection.

**To do (in scope):**
- The list-row DTO physically lives in `AccuSync.Application`, not
  `AccuSync.Presentation`.
- No shared ViewModel base class exists anywhere — `PatientViewModel`,
  `LoginViewModel`, `ChangePasswordViewModel`, `SplashViewModel` each
  independently implement `INotifyPropertyChanged`.
- `AccuSync.Presentation.csproj` sets `UseWPF=true` (for the QR code's
  `BitmapImage` property), which contradicts "no WPF reference."
- Mapper tests only cover risk-factor null cases — nothing for null contacts,
  addresses, phone, or referral fields in either mapping direction.
- `ApplyTo` unconditionally creates a blank Caregiver contact even with no
  caregiver data, while `ToEntity` correctly skips it when blank —
  inconsistent behavior between the two conversion directions.

**Out of scope:**
- Add/Edit/Delete screen wiring (ASWD-54/55/56).
- Actual contact persistence — that's the repository work under ASWD-84/57.

**Doubts for lead:**
1. Should the list-row DTO move into `AccuSync.Presentation`?
   - **Recommended:** Keep the current split — treat `PatientViewModel` as
     satisfying "a DTO in Presentation," leave the list-row DTO in
     `Application`. Moving it ripples into other consumers for no functional
     gain.
   - **Alternative:** Physically relocate it into `Presentation` to match the
     AC literally — cleaner spec compliance, more refactor blast radius.
2. Should the shared Epic-0 ViewModel base class be built now? — **Decided:**
   build it now and retrofit all existing ViewModels — cheaper to do once now
   than to retrofit later as more ViewModels are added.
3. Is the `UseWPF=true` exception acceptable?
   - **Recommended:** Accept it as a documented, pre-existing deviation (the
     `BitmapImage` need is legitimate) rather than re-architecting QR
     generation right now.
   - **Alternative:** Move `BitmapImage` construction out of `Presentation`
     (expose `byte[]`, let WPF construct the image) — correct per the letter
     of the AC, touches a working feature for marginal benefit.
4. Should `ApplyTo`'s always-creates-a-blank-caregiver behavior be fixed?
   - **Recommended:** Yes — this is a bug (inconsistent with `ToEntity`), not
     a design choice; fix it to match `ToEntity`'s conditional logic.
   - **Alternative:** None reasonable — the only real choice is timing (fix
     now vs. bundle into the ASWD-57 persistence work).

---

## ASWD-54 — Add a new patient

**Asked:** Pressing Add opens a blank form covering patient, caregiver,
medical, and consent details; pressing Save writes the record to the database;
the record survives closing and reopening the application; a failed save tells
the user and keeps the entered data on screen; a success message only appears
if the record was actually saved (the story explicitly calls out today's bug:
"the screen says 'Patient saved' and writes nothing"); unit tests for a
successful save, a failed save, and saving with only mandatory fields filled.

**Done:**
- Add opens a genuinely blank form (`StartAddPatient`/`NewPatient`).
- Required-field validation blocks Save when mandatory fields are empty.

**To do (in scope) — this is most of the story:**
- `SavePillButton_Click` never calls `CreateAsync` — it only calls
  `AcceptChanges()`, an in-memory snapshot. This is the exact bug the story
  names, still present today.
- "New patient created successfully" fires unconditionally, not gated on an
  actual save result.
- No try/catch, no failure UI, no "keep entered data on failure" path exists,
  since there is no repository call that could fail yet.
- No ViewModel-level tests exist for save success, save failure, or
  mandatory-fields-only save.

**Out of scope:**
- Edit-mode save (ASWD-55).
- Delete (ASWD-56).

**Doubts for lead:**
1. What is the canonical "mandatory fields" list referenced in the AC?
   - **Recommended:** Treat the fields already enforced by
     `SavePillButton_Click`'s existing check as canonical, and confirm that
     specific list with the lead rather than guessing new ones.
   - **Alternative:** Proactively expand mandatory fields to match what a
     NICU workflow "should" require — risk of scope creep without sign-off.
2. How should Add persist mother/caregiver contacts?
   - **Recommended:** Populate `patient.Contacts` via the mapper before
     calling `CreateAsync` — EF cascade-inserts the whole new graph
     automatically, so no extra repository work is needed for Add
     specifically.

---

## ASWD-55 — View and edit a patient

**Asked:** Selecting a patient in the list opens their record with every saved
value shown; pressing Edit makes the fields editable; changes are saved to the
database and survive a restart; all 12 dropdowns (gender, birth location,
nationality, screening consent, consent state, NICU, tracking consent, mother
language, mother country, caregiver language, caregiver country, gestational
age) save and reload correctly; names are no longer rewritten as you type —
O'Brien, McDonald, Van Dyke, and hyphenated surnames must store exactly as
entered; unit tests for a full round trip of every field including all 12
dropdowns and the four surname formats.

**Done:**
- Selecting a patient loads the full record correctly via `GetByIdAsync` +
  `ToViewModel`.
- The Edit toggle exists and switches the form into an editable state.
- All 12 named dropdowns already exist as bound `PatientViewModel` properties
  — this AC item is fully covered already.

**To do (in scope):**
- Edit-mode Save also never calls `UpdateAsync` — same root issue as ASWD-54,
  so edits don't actually persist despite the UI implying success.
- `UpdateAsync` ignores `Contacts` entirely — needs extending so
  caregiver/mother-tied fields actually save.
- Confirmed live bug matching the story: the existing capitalization helper
  title-cases each word and mangles apostrophe names (its own code comment
  shows `O'BRIEN` becoming `O'brien`) — exactly the bug the AC describes.
- The capitalization handler is also inconsistently wired — attached only to
  Mother's Title/Last-name fields; the patient's own name fields and all
  caregiver name fields aren't wired to it at all.
- No round-trip unit tests exist for the 12 dropdowns or the four surname
  formats.

**Out of scope:**
- Delete (ASWD-56).

**Doubts for lead:**
1. Should the name auto-capitalization be removed or fixed? — **Decided:**
   remove it entirely. Matches the AC's "no longer rewritten" wording exactly
   and guarantees zero mangling for any name pattern, rather than patching
   the algorithm to handle apostrophes/hyphens and still carrying residual
   edge-case risk.
2. Was the Mother-only wiring intentional?
   - **Recommended:** Treat it as an incomplete rollout to finish (or, given
     the decision above, remove) as part of this story — there's no
     plausible reason mother-only wiring would be a deliberate design choice.

---

## ASWD-56 — Delete a patient (soft delete)

**Asked:** Any user with access to the patient screen can delete a patient —
not restricted to administrators; deleting asks for confirmation first,
naming the patient; deleting sets the record to deleted without removing the
row (decision D3); deleted patients disappear from the patient list and from
search results; test results belonging to a deleted patient are also hidden;
the record can still be found in the database for future audit history; unit
tests covering delete by an ordinary user and by an administrator both
succeeding, deleted patients hidden everywhere, and the row still existing.

**Done:**
- The single-delete confirmation dialog exists and correctly names the
  patient.
- Nothing currently restricts delete to administrators — no role gating
  exists at all today.
- `GetPagedAsync` already excludes soft-deleted patients from the default
  list.

**To do (in scope):**
- `DeletePillButton_Click` has an explicit "no database delete, only clears
  the UI" comment — needs to actually call `SoftDeleteAsync`.
- `BulkDelete_Click` is the same — in-memory removal only, no persistence,
  and its confirmation dialog shows only a count, not names.
- `GetTestsForPatientAsync` filters only by `PatientId`, with no check against
  `Patient.IsDeleted` — a deleted patient's test records are still returned
  in full today.
- `GetByIdAsync` still has no soft-delete filter (shared gap with ASWD-84) —
  relevant here because a deleted patient could otherwise still be opened
  directly by ID.
- No tests exist for ordinary-vs-administrator delete, hidden-everywhere, or
  row-still-exists.

**Out of scope:**
- Audit logging itself — explicitly deferred to a later story.
- Hard delete — explicitly rejected per decision D3.

**Doubts for lead:**
1. Should "any user can delete" be an explicit guard, or stay implicit? —
   **Decided:** add an explicit "all authenticated users allowed" guard —
   cheap insurance so a future change doesn't accidentally restrict delete
   without anyone noticing this AC exists, rather than leaving it satisfied
   only by omission.
2. Should `GetTestsForPatientAsync`/`GetByIdAsync` always exclude deleted
   records, or take an override flag? — **Decided:** always exclude, no
   override parameter — matches the AC's "hidden everywhere" wording, and
   nothing today needs to see deleted-patient data via these paths.
3. Should bulk-delete's confirmation name the patients individually?
   - **Recommended:** List the patient names (or first few plus "and N
     more") instead of just a count, to satisfy "naming the patient" for
     every record being deleted.
   - **Alternative:** Keep count-only wording — simpler, but the AC was
     likely written with single-delete in mind.
---

## ASWD-57 — Save the patient's contact details

**Asked:** Mother and caregiver details are saved in PatientContacts and
linked to the patient; all agreed contact fields save and reload correctly; a
patient can be saved with no caregiver details; deleting a patient hides
their contacts too; unit tests covering a patient with both contacts, with
only a mother, and with neither.

**Done:**
- The import path (`ToEntity`, used by AccuLink) already handles "no
  caregiver"/"no mother" correctly via conditional checks.
- Deleting a patient already implicitly hides contacts, since contacts are
  only ever loaded as a patient's children.

**To do (in scope):**
- `ApplyTo` — the screener-facing edit path — always creates a blank
  Caregiver row even with no caregiver data, contradicting "can be saved
  with no caregiver details." Needs the same conditional logic `ToEntity`
  already has.
- No production code calls `ApplyTo` today, and `UpdateAsync` ignores
  `Contacts` — so there is currently no way for a screener's mother/caregiver
  edits to reach the database at all.
- No tests exist for "both contacts / mother-only / neither" on the edit
  path specifically.

**Out of scope:**
- AccuLink import mapping — already handled via `ToEntity`, not the focus of
  this story.
- Test-record data.

**Doubts for lead:**
1. Where should the "skip empty contact" rule live?
   - **Recommended:** In the mapper (`ApplyTo`/`ToEntity`), consistent with
     where `ToEntity` already implements it — the repository should stay a
     dumb persistence layer.
   - **Alternative:** Push it into `UpdateAsync` (e.g. delete a contact row
     if all fields are null) — mixes business rules into persistence code.
2. What is the "agreed contact fields" source of truth?
   - **Recommended:** Default to the full existing `PatientContacts` column
     list already schema-matched under ASWD-84, and let the lead flag
     anything to exclude, rather than guessing a smaller subset.
   - **Alternative:** Proactively narrow to a smaller "commonly used" subset
     based on our own judgment — risks silently dropping a field the lead
     actually wants.
