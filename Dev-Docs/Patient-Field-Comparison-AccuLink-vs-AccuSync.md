# Patient fields — AccuLink compared with AccuSync

**Purpose:** settle open question **Q16** (which fields belong on the patient record) by comparing the existing product with what has been built so far and with what the requirements ask for.

**Sources**
- **AccuLink** — screenshots of the live product, version 1.5.0.9378 (MS SQL Server CE): Patient Information, Mother Information, Caregiver Information, Referral Information, Medical Information, Risks, Comments
- **AccuSync** — `AccuSync.Core/Entities/PatientData.cs` and `AccuSync.WPF/Resources/Strings.resx`
- **Requirements** — `DOC-076814 Rev 01`, requirement GID-254883

---

## 1. The headline

**AccuLink answers Q16.** The predecessor product already has an agreed patient field list, and AccuSync is close to it — one field short.

| | Count |
|---|---|
| Fields on the AccuLink patient screens | **52** |
| Fields in AccuSync today | **63** (60 user fields + 3 internal import-tracking fields) |
| In AccuLink but missing from AccuSync | **1 field — Time of Birth** — plus see §5 for risk factors and comments |
| Added in AccuSync, not in AccuLink | **7 user fields** + 3 internal |
| In AccuSync but named by no requirement | **9** — and **8 of those 9 exist in AccuLink** |

**This reverses our earlier recommendation.** We previously suggested dropping the Social Security Number fields because no requirement asked for them. AccuLink has them (labelled *Social No.*), for both mother and caregiver. They are not something a developer invented — they are carried over from the product being replaced.

**The real finding is that the requirement document has a gap, not that the code has extra fields.** GID-254883 names four groups and no fields, so it fails to describe a patient record that AccuLink has been capturing for years.

---

## 2. The entire AccuLink field list — 52 fields

### Patient Information (18)

Mandatory fields are marked ▶ in the product and shown with a yellow background.

| # | Field | Type | Notes |
|---|---|---|---|
| 1 | **Patient ID** ▶ | text | mandatory |
| 2 | Hospital ID | text | |
| 3 | Forename | text | AccuSync calls this First name |
| 4 | **Surname** ▶ | text | mandatory. AccuSync calls this Last name |
| 5 | **Date of Birth** ▶ | date | mandatory |
| 6 | **Time of Birth** | time | the small control beside Date of Birth — **confirmed by the customer**. AccuSync does not have this |
| 7 | Birth Location | dropdown | |
| 8 | Nationality | dropdown | |
| 9 | Discharged | dropdown | |
| 10 | Deceased | dropdown | |
| 11 | Gestational Age (weeks) | dropdown | |
| 12 | Gender | dropdown | |
| 13 | Weight | text | |
| 14 | Height | text | |
| 15 | Consent State | dropdown | |
| 16 | NICU | dropdown | |
| 17 | Screening Consent | dropdown | |
| 18 | Tracking Consent | dropdown | |

### Mother Information (14)

| # | Field | Notes |
|---|---|---|
| 19 | Title | |
| 20 | **Social No.** | the Social Security Number field |
| 21 | Mother's ID | |
| 22 | Forename | |
| 23 | Surname | |
| 24 | Date of Birth | |
| 25 | Language | dropdown |
| 26 | Street | one address line only |
| 27 | Zip | |
| 28 | City | |
| 29 | Country | dropdown |
| 30 | Phone | |
| 31 | Mobile Phone | |
| 32 | Fax | |

### Caregiver Information (12)

| # | Field | Notes |
|---|---|---|
| 33 | Title | |
| 34 | **Social No.** | |
| 35 | Forename | |
| 36 | Surname | |
| 37 | Language | dropdown |
| 38 | Street | |
| 39 | Zip | |
| 40 | City | |
| 41 | Country | dropdown |
| 42 | Phone | |
| 43 | Mobile Phone | |
| 44 | Fax | |

Note the caregiver has **no** Date of Birth and **no** ID. The mother has both.

### Referral Information (5)

| # | Field | Type |
|---|---|---|
| 45 | Audiology Referral | dropdown |
| 46 | Referral Date | date |
| 47 | Referral To | text |
| 48 | Referral From | text |
| 49 | Referral Phone | text |

### Medical Information (3)

| # | Field | Type | Notes |
|---|---|---|---|
| 50 | Medication | large free text | |
| 51 | **Pediatrician** | **dropdown** | AccuSync renamed this **Physician** and made it free text |
| 52 | Audiologist | **dropdown** | AccuSync made this free text |

### Plus two things that are not simple fields

- **Risks tab** — 19 named risk factors, each with a Name, a Description, and a *Has Risk* answer of Yes / No / Unknown
- **Comments tab** — comments chosen from a dropdown, each stored with a **Date** and an **Examiner**

---

## 3. In AccuLink but missing from AccuSync

**One field is missing: Time of Birth.** AccuSync covers the other 51.

| # | Item | AccuLink | AccuSync | Comment |
|---|---|---|---|---|
| 1 | **Time of Birth** | Captured beside Date of Birth | **Not present** | **Confirmed missing.** Must be added |
| 2 | Pediatrician / Physician | **Dropdown** — pick from a list | **Free text** | Free text cannot be reported on; the same doctor gets typed five ways |
| 3 | Audiologist | **Dropdown** | **Free text** | Same problem |

**Why Time of Birth matters.** For a newborn, age in hours is not a detail. Screening too soon after birth produces more refers, because fluid and vernix in the ear canal interfere with the test. Date alone cannot tell you whether a baby was 2 hours old or 20 hours old at the time of screening. AccuLink has been capturing it, so any records carried across would lose it.

**Comments lose information.** AccuLink stores a Date and an Examiner with every comment. AccuSync's patient model has a single flat `Comments` text field, so who wrote a comment and when is lost.

---

## 4. Added in AccuSync, not in AccuLink

### User-visible fields (7)

| # | Field | Comment |
|---|---|---|
| 1 | Middle initial | On the patient record |
| 2 | Mother — Address 2 | AccuLink has one line ("Street"); AccuSync has two |
| 3 | Mother — State | AccuLink has no state field |
| 4 | Mother — Email | AccuLink has no email |
| 5 | Caregiver — Address 2 | |
| 6 | Caregiver — State | |
| 7 | Caregiver — Email | |

All seven are reasonable additions for a US product. **None is asked for by a requirement.**

### Internal fields (3)

Source ID, Source created, Source modified. Not on screen. Used to recognise a patient already imported from a device file, so a repeat import does not create a duplicate.

---

## 5. Risk factors — the lists do not match

This is the most significant difference found, and it has clinical consequences.

**AccuLink has 19. AccuSync has 16. They are different lists, not a subset.**

| AccuLink (19) | In AccuSync? |
|---|---|
| Abnormal apgar scores | ❌ **no** |
| Caregiver Concern | ✅ yes |
| Chemotherapy | ⚠️ merged into "Exposure to Head Trauma or Chemotherapy" |
| Craniofacial anomalies | ✅ as "Craniofacial Anomalies - Microtia/Atresia, Clefting" |
| Culture-positive postnatal infections associated with sensorineural hearing loss | ⚠️ as "Perinatal or Postnatal Infection" — **broader wording** |
| Family history of hearing loss | ✅ as "Family History of Permanent Childhood Hearing Loss" — **narrower wording** |
| Head trauma | ⚠️ merged with Chemotherapy |
| Hyperbilirubinemia | ⚠️ as "Hyperbilirubinemia with Exchange Transfusion" — **narrower** |
| In utero infections | ✅ as "In Utero Infections such as CMV & Zika" |
| Low birth weight | ✅ yes |
| Mechanical ventilation | ✅ as "Prolonged Ventilation" |
| Neonatal indicators | ❌ **no** |
| Neonatal intensive care | ❌ **no** |
| Neurodegenerative disorders | ❌ **no** |
| **NICU > 48 Hours** | ⚠️ **AccuSync says NICU >5 Days — a different threshold** |
| Ototitis Media | ❌ **no** |
| Ototoxic medication | ✅ as "Ototoxic Medications" |
| Physical findings | ❌ **no** |
| Syndromes associated with hearing loss | ✅ as "Syndromes & Genetic Disorders of Hearing Loss" |

**In AccuSync but not in AccuLink (4):**
Aminoglycosides for >5 Days · Asphyxia - Hypoxic Ischemic Encephalopathy (HIE) · Bacterial Meningitis · ECMO - Extracorporeal Membrane Oxygenation

### Why this matters

1. **Six AccuLink risk factors have no home in AccuSync.** If existing patient records carry them, that data cannot be represented.
2. **The NICU threshold changed from 48 hours to 5 days.** These are not the same clinical question. A baby in NICU for 3 days answers Yes in AccuLink and No in AccuSync.
3. **Three factors were narrowed** (family history, hyperbilirubinemia) or **broadened** (postnatal infection). The same baby can get a different answer in each product.
4. AccuSync's list looks like a modern JCIH-style set, and AccuLink's like an older one. **That may well be a deliberate clinical update** — but it is not written down anywhere, and no requirement records it.

**This needs a clinical decision, not a technical one.** It also ties directly to the design conflict in the Patient Management story document §4.1: AccuSync hardcodes its 16 into the screen, while the requirements (GID-254950, GID-254951) call for an administrator-managed list.

---

## 6. Fields with no requirement behind them

Nine AccuSync fields are named by no requirement. **Eight of them exist in AccuLink.**

| Field | In AccuLink? | What it means |
|---|---|---|
| Mother — Social No. / SSN | ✅ **yes** | Established practice in the product being replaced |
| Caregiver — Social No. / SSN | ✅ **yes** | Same |
| Audiology Referral | ✅ yes | |
| Referral Date | ✅ yes | |
| Referral To | ✅ yes | |
| Referral From | ✅ yes | |
| Referral Phone | ✅ yes | |
| Physician / Pediatrician | ✅ yes | AccuLink has it as a dropdown |
| Audiologist | ✅ yes | AccuLink has it as a dropdown |
| Middle initial | ❌ no | New in AccuSync |
| Mother/Caregiver Address 2, State, Email (6) | ❌ no | New in AccuSync |

**So this is a requirements gap, not scope creep.** DOC-076814 does not describe a patient record that AccuLink has been capturing for years.

---

## 7. What we recommend

### 7.1 The field list

**Build all 52 AccuLink fields plus the 7 AccuSync additions — 59 user fields — and write requirements to cover the 9 that currently have none.**

Rationale: AccuLink is the product being replaced. A replacement that cannot hold what the old product held is not a replacement. The 7 additions are sensible for a US deployment and cost little.

### 7.2 Social Security Number — revised advice

Our earlier advice was to remove it. **That was based on not knowing AccuLink had it.** Revised:

**Keep it, and protect it properly.** It has a clear precedent, and the likely purpose — matching a baby to a state EHDI registry — is real.

But it still needs the four things that were never done:

1. A requirement stating that SSN is collected and why
2. **Masking on screen** (`•••-••-1234`) — the code's own TODO asks for this and it was not done
3. **A rule keeping it out of every log file** — GID-255034
4. **A decision that de-identified export strips it** — GID-256279. SSN is one of the 18 HIPAA Safe Harbor identifiers

Estimated at **2 developer-days**, which is now recommended work rather than optional.

### 7.3 Three smaller decisions

| # | Decision | Our advice |
|---|---|---|
| 1 | Physician and Audiologist: dropdown or free text? | **Dropdown**, as AccuLink has it. Free text cannot be reported on. Needs a small practitioners list |
| 2 | Comments: keep Date and Examiner? | **Yes.** AccuLink stores both. Losing who commented and when is a real loss in a medical record |
| 3 | Time of Birth | **Confirmed by the customer.** Add it. It is one extra column and it affects how a screening result is read |

### 7.4 Risk factors — needs a clinical answer

| Question | Why it matters |
|---|---|
| Is AccuSync's 16-item list the intended replacement for AccuLink's 19? | If yes, it is a clinical decision that must be recorded as a requirement |
| What happens to the 6 AccuLink factors with no AccuSync equivalent? | Existing records may carry them |
| Is the NICU threshold now 5 days, not 48 hours? | It changes the answer for real babies |
| Were family history, hyperbilirubinemia and postnatal infection deliberately reworded? | Each changes who gets flagged |

---

## 8. Effect on the plan

| Item | Before | After |
|---|---|---|
| Q16 status | Open, blocking PM-01 | **Answerable.** AccuLink gives the list; confirm the 9 gap fields and the 2 remaining smaller decisions |
| SSN | Recommended for removal | **Recommended to keep**, with masking, logging and export rules — 2 days |
| Risk factors | Known design conflict (§4.1) | **Also a content conflict.** Two different clinical lists, needing a clinical decision |
| New work identified | — | **Time of Birth (confirmed missing)**; practitioner pick-lists; Date and Examiner on comments |

**Superseded 18 Aug 2026 — ASWD-2 is now 171.0 days.** The ALGO field work became story PM-39 in F1 (+14.0 days). The paragraph below was written before that. **None of this changed the ASWD-2 estimate of 136 days at the time.** The SSN protection work (2 days) was already priced as an option. Time of Birth, the practitioner pick-lists and the comment metadata are all small. What could move the number is the risk factor decision, and that cannot be sized until the clinical question is answered.

---

## Update — ALGO 5 and ALGO Pro device exports

Two device export samples were supplied after this comparison was written
(`ALGO5_sample.xml`, `ALGOPro_sample.json`). They change three conclusions above.

**1. The risk-factor question is settled.** Both ALGO files carry exactly the **16** risk factors
AccuSync already has, character-for-character, including **NICU >5 Days**. AccuLink's 19-item list
with the **>48 hours** threshold is the legacy list. AccuSync is correct as built; the difference
only matters when importing historic AccuLink data.

**2. SSN is confirmed as a manual field.** Neither ALGO file has a Social Security Number anywhere.
It comes from AccuLink's screen only, so no device import will ever fill it. The recommendation
stands: keep it and protect it.

**3. Time of Birth is confirmed as a manual field too.** Not in either ALGO file. It exists only on
the AccuLink screen and on the `Patient.cs:BirthTime` list model.

**4. `Physician` is really the pediatrician.** Natus confirmed AccuSync's `Physician` field holds
what ALGO calls `CurrentPediatrician`. ALGO also has a *separate* field named `Physician` holding a
different person, and both import parsers currently overwrite the pediatrician with it
(`AlgoProJsonParser.cs:135` then `:150-151`). The screen label needs to match what the field holds.

**And it adds a much larger gap.** The ALGO files carry **43** (ALGO 5) and **54** (ALGO Pro)
patient fields. **28 of them have no home in AccuSync** — most notably the baby's own address,
city, state, postal code, country, telephone and mobile, which today exist only for the mother and
the caregiver. Full analysis, including 28 missing test-result fields and 24 missing device fields:
**[ALGO5-ALGOPro-Field-Analysis.md](ALGO5-ALGOPro-Field-Analysis.md)**.
