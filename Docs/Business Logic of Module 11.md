# Business Logic of Module 11 — Price Lists & Custom Test Packages

**Source:** RLS_Learn.pdf — "Real Lab System Guide Book", RealLab Co. (212 pp.)
**Scope of this extraction:** pp. 105–111 (Contract Price Lists) and pp. 118–123 (Custom Groups) — the confirmed primary range. No other pages were read, except where an explicit cross-reference inside the primary range demands it (see §8 — none were found).
**Extraction method:** Full text layer extraction plus page-image OCR/visual reading of every screenshot on every page of the primary range.
**Conventions:** Every rule, workflow step, field, and label carries its exact source page `[p. N]`. Arabic labels are given with English translations. Confidence ratings: HIGH / MEDIUM / LOW with justification.

---

## 1. Module Overview (as stated by the manual)

- **Contract Price Lists purpose (stated on-screen):** The window header states: *"Through this window, more than one price can be assigned to a single test across different price lists, and they are used with referral entities, for ease of use in entering test and patient data."* `[p. 105, p. 107, p. 109]`
  - **Business rule R-PL-01 (HIGH — stated verbatim in the window header on multiple pages):** A single test may carry **multiple prices simultaneously**, one per price list; the lists are the pricing mechanism used when dealing with **referral entities (جهات الإحالة)**.
- **Custom Groups purpose (stated on-screen):** *"Through this window, a group of tests can be customized into a single group with a specific price, for ease of use when entering test and patient data."* `[p. 121]`; *"To add a custom group containing a number of tests, such as a check-up group or a group of tests specific to a certain entity or a specific doctor."* `[p. 118]`
  - **Business rule R-CG-01 (HIGH — stated in section intro):** A Custom Group is a **named bundle of tests with its own per-test prices**, intended for scenarios like check-up packages, or bundles specific to an **entity** or a **specific doctor**, and it **can be attached to a patient**.

---

## 2. Function 1 — Contract Price List: Create, Add Tests, Edit Test Price, Remove Test, Edit List (pp. 105–110)

### 2.1 Navigation / entry point
1. Click the **System** icon (النظام) in the top toolbar. `[p. 105, p. 106]`
2. Choose **قوائم أسعار التعاقدات (Contract Price Lists)** from the System menu grid. `[p. 105, p. 106]`

### 2.2 Create a new price list `[p. 105, p. 107]`
Workflow in the manual's exact order:
1. Click **إضافة (Add)** (the Add button in the price-list control group, next to the "Price List Name" dropdown). `[p. 105, p. 107]`
2. A modal dialog titled **"اضافة قائمة اسعار" (Add Price List)** appears, prompting **"ادخل اسم قائمة الاسعار الجديدة" (Enter the name of the new price list)**. `[p. 107]`
3. Type the list name (example shown: **"Real Lab"**) and click **OK** (the dialog also has **Cancel**). `[p. 105, p. 107]`
4. The new list appears in the program's price lists (observable in the **اسم قائمة الأسعار (Price List Name)** dropdown; example dropdown contents: `lowpricelist`, `price menu1`, `price menu2`, `Real Lab`). `[p. 105, p. 108]`

**Business rules extracted:**
- **R-PL-02 (HIGH — dialog wording + observed behavior):** A price list is created with a **name only**; no other attributes are captured at creation.
- **R-PL-03 (MEDIUM — counter behavior observed on screen):** A newly created list contains **0 tests**; the on-screen counter **"عدد تحاليل القائمة المسجلة" (Number of registered tests in the list)** shows `0`. `[p. 107]`
- **R-PL-04 (MEDIUM — inferred from dropdown contents):** Multiple price lists coexist and are selectable from one dropdown; the manual shows at least four coexisting lists. `[p. 108]`

### 2.3 Add tests with prices to a list `[p. 105, p. 108, p. 109, p. 110]`
Workflow in the manual's exact order:
1. **Select the price list** from the Price List Name dropdown. `[p. 105, p. 108]`
2. Click **إضافة (Add)** (the Add button in the test-entry control group, under Test Name / Price). `[p. 105, p. 108]`
3. Choose the test from the **Test Name** dropdown (example contents shown: `17 OH Progesterone`, `5' Nucleotidase`, `A/G Ratio`, `ABO`, `ACD-FAST STAIN (ZN Film)`, `ACTH (10 PM)`, `ACTH (9 AM)`, `Adrenaline`). `[p. 105, p. 109]`
4. Enter the test's price in the **Price** field (field default shown as `0`). `[p. 105, p. 109]`
5. Confirm: the running instruction caption states *"Select the test to be added to the price list, specify its price, then press **حفظ (Save)**"*; page 105's text says press **موافق (OK/Agree)**. `[p. 105, p. 109]` *(terminology discrepancy — see Open Question OQ-1)*
6. The test appears in the price list grid; the counter **"Number of registered tests in the list"** increments (observed: `0` → `1` after saving CBC @ 50; later `3` with CBC 50, Stool 10, Urine 10). `[p. 105, p. 109, p. 110, p. 111]`

**Business rules extracted:**
- **R-PL-05 (HIGH — workflow + screen fields):** Adding a test to a list requires exactly two data points: **the test (from the test catalog)** and **a price**; prices are entered **manually per list** — the screen shows the Price field defaulting to `0`, not auto-populated from any default test price. `[p. 107, p. 109]`
- **R-PL-06 (MEDIUM — observed example):** The same test can carry different prices in different lists (implied by the module's stated purpose, R-PL-01); the worked example uses CBC = 50 L.E. in the "Real Lab" list. `[p. 109, p. 110]`
- **R-PL-07 (MEDIUM — screen behavior):** Tests already added to a list are displayed in a grid with columns **Price menu name | Test name | Price**. `[p. 107, p. 110]`

### 2.4 Edit a test's price within a list `[p. 105]`
Workflow:
1. Click (select) the test in the list.
2. Click **تعديل (Modify/Edit)**.
3. Enter the new price.
4. Click **حفظ (Save)**.

When editing an existing list, the manual restates this: *"To modify test prices in the list, we select the test from the **left side of the window**, then click **تعديل (Modify)**, change the test price, then click **حفظ (Save)**."* `[p. 105]`

**Business rules extracted:**
- **R-PL-08 (HIGH — explicit text):** A test's price inside a contract list is **editable at any time** via select → Modify → Save; the manual states **no** restriction, approval step, or audit prompt for this action. `[p. 105]`
- **R-PL-09 (LOW — silence of the manual):** The manual does **not** state any linkage between the price stored in a contract list and the test's default/catalog price. There is **no rule** on pp. 105–111 describing propagation of a default-price change into existing lists. (See Open Question OQ-2.)

### 2.5 Remove a test from a list `[p. 105]`
Workflow:
1. Select (click) the test in the list.
2. Click **حذف (Delete)**.

**Business rules extracted:**
- **R-PL-10 (HIGH — explicit text):** Removing a test from a list is a two-click action (select + Delete). The manual describes **no confirmation dialog** for removing a single test from a price list (contrast with list deletion and group deletion, which do confirm — R-PL-13, R-CG-05). `[p. 105]`

### 2.6 Edit an existing price list `[p. 105]`
Workflow:
1. Select the price list to be modified.
2. **To rename:** click **تعديل (Modify)** at the bottom of the price list area, edit the name, then click **حفظ (Save)**.
3. **To change test prices:** select the test from the left side of the window → **تعديل (Modify)** → change price → **حفظ (Save)**.
4. **To remove a test:** select the test → **حذف (Delete)**.

**Business rules extracted:**
- **R-PL-11 (HIGH — explicit text):** A list's **name is editable** after creation via Modify → Save. `[p. 105]`
- **R-PL-12 (HIGH — explicit text):** List membership and per-test prices remain **fully editable after creation**; no locking or versioning behavior is described. `[p. 105]`

### 2.7 Delete an entire price list `[p. 105]`
Workflow:
1. Select the price list to delete.
2. Click **حذف (Delete)**.
3. Click **موافق (OK/Agree)**.

**Business rules extracted:**
- **R-PL-13 (HIGH — explicit text):** Deleting a whole price list requires an **explicit second confirmation click (موافق)**. `[p. 105]`
- **R-PL-14 (LOW — silence of the manual):** The manual does **not** state any blocking rule preventing deletion of a list that is in use (e.g., attached to a referral entity), nor any cascade behavior for its test prices. (See Open Question OQ-3.)

### 2.8 Screen fields, buttons, and labels — Contract Price List window (completeness inventory)
Window title: **"نافذة قوائم اسعار التحاليل للجهات" (Test Price Lists Window for Entities)**. `[p. 107, p. 109, p. 110, p. 111]`

| Element | Label (AR / EN) | Page |
|---|---|---|
| Header description | "من خلال هذه النافذة يمكن اعطاء اكثر من سعر للتحليل الواحد… والتعامل بها مع جهات الاحالة" / multi-price per test across lists, used with referral entities | 105, 107, 109 |
| Counter | عدد تحاليل القائمة المسجلة / Number of registered tests in the list | 107, 109, 110, 111 |
| Dropdown | اسم قائمة الأسعار / Price List Name | 107–111 |
| Grid columns | Price menu name, Test name, Price | 107, 110 |
| Search box | Test name Search | 107 |
| List-level buttons | إضافة (Add), تعديل (Modify), حذف (Delete), طباعة القائمة (Print List) | 107, 109–111 |
| Test-level fields | Test Name: (dropdown), Price: (numeric, default 0) | 107, 109 |
| Test-level buttons | إضافة (Add), حفظ (Save), تعديل (Modify), حذف (Delete), تراجع (Undo/Cancel) | 107, 109 |
| Modal dialog | اضافة قائمة اسعار (Add Price List) — field "ادخل اسم قائمة الاسعار الجديدة" with OK / Cancel | 107 |
| Navigation button | القائمة الرئيسية (Main Menu) | 107 |
| System-menu entry | قوائم أسعار التعاقدات (Contract Price Lists); also a separate entry طباعة قائمة اسعار التحاليل (Print Test Price List) exists in the System menu grid | 106 |

---

## 3. Function 2 — Print a Price List (pp. 105, 106, 111)

### 3.1 Workflow `[p. 105, p. 111]`
1. Select the price list to be printed (example: "Real Lab").
2. Click **طباعة القائمة (Print List)**.

A dedicated System-menu entry **"طباعة قائمة اسعار التحاليل" (Print Test Price List)** also exists. `[p. 106]`

### 3.2 Printed output structure (from the report sample) `[p. 111]`
- Window/report title: **"قائمة أسعار التحاليل" (Tests Price List)**.
- Report header metadata: **"Printed In [day, date]"** (example: Tuesday, 15-Nov-2016) and **"Current Page 1 from 1"**.
- Report title banner: **"أسعار التحاليل في قائمة الأسعار [List Name]"** (example: "Test prices in the Real Lab price list").
- Body: tests **grouped under their clinical categories** (examples shown: BLOOD PICTURE, STOOL EXAMINATION, URINE EXAMINATION).
- Columns: **Price** (rendered with currency suffix **"L.E."**, e.g., 50 L.E., 10 L.E.), **Result date** (example value "1 Days"), **Collection notes** (examples: "Stool", "Urine", or empty).
- Worked example rows: CBC | 50 L.E. (BLOOD PICTURE); Stool | 10 L.E. (STOOL EXAMINATION); Urine | 10 L.E. (URINE EXAMINATION).

**Business rules extracted:**
- **R-PR-01 (HIGH — observed report structure):** The printed price list **groups tests by clinical category** and shows, per test, its **list price (in L.E.)**, **result turnaround ("Result date")**, and **collection notes** — i.e., the printout pulls non-price test-catalog attributes into the commercial document. `[p. 111]`
- **R-PR-02 (HIGH — observed report):** Prices print in **Egyptian Pounds (L.E.)** with the currency suffix rendered per row. `[p. 111]`
- **R-PR-03 (MEDIUM — observed report):** The printout is **per single selected list**; the list name is embedded in the report title. `[p. 105, p. 111]`

**Confidence for §3:** HIGH — the full printed sample is visible on p. 111 and matches the workflow text on p. 105.

---

## 4. Function 3 — Custom Group: Create a Named Bundle of Tests with Its Own Prices (pp. 118–121)

### 4.1 Navigation / entry point `[p. 118]`
1. Click the **System** icon.
2. Click **Custom Groups** in the System menu grid.

### 4.2 Create the group `[p. 119]`
Workflow in the manual's exact order:
1. Click **إضافة مجموعة (Add Group)**.
2. Type the group name in the **Group Name:** field (example: **"RealLab"**).
3. Click **حفظ (Save)**.

Observed UI state rules:
- Before clicking Add Group, **حفظ (Save) is disabled (greyed out)**; after clicking Add Group the button set changes to **تراجع (Undo/Cancel)** + **حفظ (Save)** — i.e., creation is an explicit two-phase action with a cancel option. `[p. 119]`
- The counter **"عدد المجموعات المسجلة" (Number of registered groups)** increments on save (observed: `5` → `6`; pre-existing example groups: CHECKUP 1 … CHECKUP 5). `[p. 119, p. 120]`

**Business rules extracted:**
- **R-CG-02 (HIGH — screen states):** Group creation is initiated by Add Group and finalized by Save, with an **Undo/Cancel escape path**; the name field is the only attribute captured at creation. `[p. 119]`
- **R-CG-03 (MEDIUM — observed counters):** A newly created group starts **empty** (No. of tests in group = 0; Total group Price = 0). `[p. 119]`

### 4.3 Add tests with prices to the group `[p. 120, p. 121]`
Workflow in the manual's exact order:
1. **Select the group** from the Group Name list. `[p. 120]`
2. Click **اضافة تحليل (Add Test)**. `[p. 120]`
3. Select the test from the **Test Name** dropdown and enter its **Price** (Price field defaults to `0`; example: CBC @ 50). `[p. 120, p. 121]`
4. Click **حفظ (Save)**. `[p. 120, p. 121]`
5. Repeat; the group grid populates with columns **Test ID | Test Name | Price**. Worked example for group "RealLab": `56 | CBC | 50`; `1 | F.Bl.G. | 10`; `4 | Urine | 10`. `[p. 121]`

**Business rules extracted:**
- **R-CG-04 (HIGH — explicit text + observed calculation):** Each test inside a Custom Group carries **its own price, entered manually**; the system automatically computes and displays **"No. Of tests in group"** (example: 3) and **"Total group Price"** as the **sum of member test prices** (50 + 10 + 10 = 70). `[p. 120, p. 121]`
- **R-CG-05-price-independence (MEDIUM — structural inference from both windows):** Custom Group prices are entered independently per group; **nothing on pp. 118–123 references or links these prices to any Contract Price List**. A custom group's prices therefore can, on the evidence of these pages, diverge freely from any contract list. (See Open Question OQ-4.)
- **Test identity in a group:** the group grid carries the test's **Test ID** (internal numeric identifier, e.g., 56, 1, 4) alongside name and price. `[p. 121]`

### 4.4 Screen fields, buttons, and labels — Custom Groups window (completeness inventory)
Window title: **"نافذة مجموعات التحاليل الخاصة بالمعمل" (Lab Custom Test Groups Window)**. `[p. 119, p. 121, p. 123]`

| Element | Label (AR / EN) | Page |
|---|---|---|
| Header description | "من خلال هذه النافذة يمكن تخصيص مجموعة من التحاليل في صورة مجموعة بسعر محدد…" / customize a set of tests into one group with a specific price | 121, 123 |
| Counter | عدد المجموعات المسجلة / Number of registered groups | 119–121, 123 |
| Field + list | Group Name: (with list box of groups) | 119, 121, 123 |
| Group-level buttons | إضافة مجموعة (Add Group), حفظ (Save), تعديل (Edit), حذف مجموعة (Delete Group), طباعة القائمة (Print List), تراجع (Undo — appears during add) | 119, 121, 123 |
| Grid columns | Test ID, Test Name, Price | 119, 121 |
| Grid footers | No. Of tests in group; Total group Price | 119, 121, 123 |
| Test-level fields | Test Name: (dropdown), Price: (numeric, default 0) | 119–121 |
| Test-level buttons | اضافة تحليل (Add Test), حفظ (Save), تعديل (Edit), حذف تحليل (Delete Test) | 119–121, 123 |
| Navigation button | القائمة الرئيسية (Main Menu) | 119, 121 |

Note: the Custom Groups window also exposes **تعديل (Edit)** and **حذف تحليل (Delete Test)** at test level and **تعديل (Edit)** at group level, and a **طباعة القائمة (Print List)** button `[p. 119, p. 121, p. 123]` — the manual's **text** in this section only narrates add-group, add-test, attach-to-patient, and delete-group flows; the edit/print capabilities are present on screen but **not narrated**. (See Open Question OQ-5.)

---

## 5. Function 4 — Attach a Custom Group to a Specific Patient (p. 122)

### 5.1 Workflow `[p. 122]`
The action takes place on the **Patient Registration / Transaction Entry screen**:
1. In the left group-selection panel, set the selector to **CG** (Custom Group) — the panel shows tabs/options `start | TM | TG | CG` with **CG** selected, and a "custom group menu" list. `[p. 122]`
2. Click the desired group in the list (example groups shown: CHECKUP 1–5, **RealLab** — the example clicks **RealLab**). `[p. 122]`
3. The screen provides an **"Add all tests in group"** button (alongside **Delete** and **All**) that transfers the group's tests into the **"Patient's tests"** list (counter shown at 0 before the action). `[p. 122]`

### 5.2 Pricing interaction at the patient screen `[p. 122]`
The patient screen's transaction panel shows the billing fields that consume the prices:
- **اجمالي التحاليل** (Total of Tests), **الخصم** (Discount %), **قيمة الخصم** (Discount Value), **التكلفة بعد الخصم** (Cost After Discount), **المدفوع سابقا** (Previously Paid), **الباقي للمعمل** (Balance Due to Lab), **الباقي للمريض** (Balance Due to Patient), **المدفوع** (Paid Amount). `[p. 122]`
All values shown are `0.0` in the screenshot (pre-attachment state). `[p. 122]`

**Business rules extracted:**
- **R-AT-01 (HIGH — explicit screen + button labels):** Attaching a group to a patient is done from the **patient transaction screen** by selecting the **CG** source and the group; the system supports adding **all tests of the group at once** ("Add all tests in group") and **removing** (Delete / All). `[p. 122]`
- **R-AT-02 (MEDIUM — layout of the patient screen):** The group's prices feed the patient-level **billing computation** visible on the same screen (Total of Tests → Discount → Cost After Discount → Paid / Balances). The manual shows the fields but the screenshot is at the pre-attachment (0.0) state, so the exact computation chain is **structurally implied, not narrated** on this page. (See Open Question OQ-6.)
- **R-AT-03 (MEDIUM — screen context):** The patient screen also captures **Account Type (نوع الحساب)** — example value **Individual** — and **Referral Entity (جهة الإحالة)**, i.e., the same screen holds both the patient-billing context and the referral-entity context that Contract Price Lists are stated to serve (R-PL-01). No rule on p. 122 states how CG pricing and contract-list pricing interact. `[p. 122]`
- Additional patient-screen context visible on this page (recorded for completeness, not part of this module's logic): admission/receiving dates, patient code/name/mobile/ID/address/age, treating physician, additional info, medical-history checkboxes (fasting hours, anticoagulants, antivirals, pregnancy, recent X-ray/ultrasound, lupus, renal failure, hypertension, arthritis, blood transfusion), notes; action buttons Send SMS, Print Result Password, NATIGH.COM, By Fax, Send E-mail; bottom actions Add/Edit/Save/Undo, Receipt, Barcode, Worksheet, Card, Patients Window, Test Results, Main Menu; specimen indicators Taken outside lab / Urine / Stool / Blood / Semen / CSF. `[p. 122]`

**Confidence for §5:** HIGH for the workflow and field inventory (fully visible on p. 122); MEDIUM for the billing-consumption rule R-AT-02 (fields shown, computation not narrated).

---

## 6. Function 5 — Delete a Custom Group (p. 123)

### 6.1 Workflow `[p. 123]`
1. Select the group in the Group Name list (example: **RealLab** selected, showing its 3 tests and totals).
2. Click **حذف مجموعة (Delete Group)**.
3. A modal **"تنبيه" (Alert/Warning)** dialog appears: **"هل تريد حذف هذه المجموعة" (Do you want to delete this group?)** with **OK** and **Cancel**.
4. Click **OK** to confirm (Cancel aborts).

**Business rules extracted:**
- **R-CG-05 (HIGH — explicit dialog):** Deleting a Custom Group is a **confirmed, two-step destructive action**; the system explicitly warns and requires OK, with a Cancel escape. `[p. 123]`
- **R-CG-06 (LOW — silence of the manual):** The manual states **no blocking rule** for deleting a group that has already been attached to patients, and no cascade behavior for historical patient transactions. (See Open Question OQ-7.)

---

## 7. Cross-Module Interactions Explicitly Stated on the Primary Pages

Only what pp. 105–111 and 118–123 themselves state:

1. **Contract price lists ↔ referral entities:** the window header states the lists are **"used with referral entities (جهات الإحالة)"** to give one test multiple prices. `[p. 105, p. 107, p. 109]` — This is the only stated consumer of contract-list pricing.
2. **Custom groups ↔ patient billing:** the Custom Group is created specifically so it **"can be added to the patient"** `[p. 118]`, and the attachment happens on the patient transaction screen whose billing panel (Total of Tests, Discount, Cost After Discount, Paid, Balances) is shown. `[p. 122]` — This is the only stated consumer of custom-group pricing.
3. **Price list printout ↔ test catalog attributes:** the printed list pulls **Result date** and **Collection notes** from test data and **groups rows by clinical category**. `[p. 111]`
4. No page in the primary range states any interaction **between** Contract Price Lists and Custom Groups (e.g., inheritance, precedence, or conflict rules). (See Open Question OQ-4.)

---

## 8. Cross-Reference Audit (Controlled Exception)

- **Result: NONE.** No sentence, note, or screen field on pp. 105–111 or pp. 118–123 explicitly points to another page or section of the manual for pricing rules, precedence, or behavior. The only cross-screen navigations described are to the **System menu** `[p. 106, p. 118]` and to the **patient transaction screen** `[p. 122]`, both shown in full on the cited pages.
- Therefore no content was extracted from outside pp. 105–111 and 118–123, and no "OUTSIDE PRIMARY RANGE" material exists in this deliverable.

---

## 9. Open Questions (ambiguities — no pause, recorded formally)

**OQ-1 — Add-test confirmation label discrepancy (Contract Price List).**
- *Question:* When adding a test to a contract list, does the user confirm with **موافق (OK)** or **حفظ (Save)**?
- *Evidence:* p. 105 text says "then we click **( موافق )**"; p. 109's on-screen caption says "…then press **حفظ** (Save)", and the visible button set contains both حفظ and تراجع. `[p. 105, p. 109]`
- *Options:* (a) The workflow is Add → select test/price → Save (p. 109 reflects a newer/accurate UI; p. 105 text is loose wording). (b) Two distinct confirmations exist in sequence (OK on a dialog, then Save). (c) The button was renamed between manual revisions.
- *Trade-offs:* (a) simplest, matches screenshots; (b) matches both texts literally but no dialog screenshot supports it; (c) unverifiable.
- *Recommended default:* **(a)** — treat Save as the confirm action; treat "موافق" on p. 105 as generic wording.
- *Confidence:* MEDIUM.

**OQ-2 — Effect of a change in a test's default/catalog price on existing contract-list prices.**
- *Question:* If a test's default price changes after the test was added to a contract list, does the list price update, or is it a frozen snapshot?
- *Evidence:* **None.** Pages 105–111 never mention a default/catalog price; list prices are always entered manually (Price field defaults to 0). `[p. 107, p. 109]`
- *Options:* (a) Snapshot — list price is independent and frozen at entry. (b) Linked — list price defaults from and tracks the catalog price. (c) Hybrid — seeded from catalog at add time, editable thereafter.
- *Trade-offs:* (a) consistent with the manual-entry-only evidence but can drift from catalog pricing; (b) keeps prices synchronized but contradicts the observed manual entry with a 0 default; (c) common LIS behavior but unsupported by any text here.
- *Recommended default:* **(a)** snapshot semantics — it is the only behavior consistent with the screens shown.
- *Confidence:* LOW (no evidence exists; recorded per instructions).

**OQ-3 — Deletion constraint for a price list that is in use.**
- *Question:* Can a contract price list be deleted while attached to referral entities or referenced by historical patient invoices?
- *Evidence:* p. 105 shows only select → Delete → موافق (confirm); no blocking or warning about usage. `[p. 105]`
- *Options:* (a) Unrestricted deletion after confirmation. (b) Blocked when referenced. (c) Allowed with cascade/orphaning of references.
- *Trade-offs:* (a) matches the literal text but risks dangling references; (b) safest commercially but unsupported; (c) middle ground, unsupported.
- *Recommended default:* **(a)** as documented behavior; flag (b) as a required business decision.
- *Confidence:* LOW (manual silent).

**OQ-4 — Interaction between Custom Group prices and Contract Price Lists.**
- *Question:* May/must a custom group's prices diverge from any contract list, and is there any precedence when both apply to a patient?
- *Evidence:* None — pp. 118–123 never mention contract lists; pp. 105–111 never mention custom groups. Each window captures prices manually and independently. `[p. 109, p. 120, p. 121]`
- *Options:* (a) Fully independent price domains. (b) Custom group overrides contract list when both exist. (c) Contract list constrains/validates group prices.
- *Trade-offs:* (a) matches evidence, simplest; (b)/(c) would be commercially meaningful but are pure invention relative to the source.
- *Recommended default:* **(a)** — independent domains; each group's prices stand alone.
- *Confidence:* MEDIUM (structural independence is visible; absence of precedence rules is absence of evidence).

**OQ-5 — Un-narrated capabilities on the Custom Groups screen (Edit group, Edit/Delete test, Print List).**
- *Question:* What are the exact rules for editing a group name, editing/removing a test inside a group, and printing a group list?
- *Evidence:* Buttons **تعديل (Edit)**, **حذف تحليل (Delete Test)**, **طباعة القائمة (Print List)** are visible on the window `[p. 119, p. 121, p. 123]`, but the section's text narrates none of these flows.
- *Options:* (a) They behave symmetrically to the Contract Price List equivalents (select → Modify → Save; select → Delete). (b) They have distinct rules not documented here. (c) They are vestigial/disabled in this module.
- *Trade-offs:* (a) is consistent with the manual's own price-list section and with button parity, but is an extrapolation; (b)/(c) have no evidence either way.
- *Recommended default:* **(a)** — adopt price-list symmetry as the working assumption; mark for verification against the running system.
- *Confidence:* LOW (buttons exist; behavior not narrated).

**OQ-6 — Exact pricing computation when a group is attached to a patient.**
- *Question:* When "Add all tests in group" is clicked, are the patient's line prices taken from the group's stored prices, and how do Discount/Previously Paid/Balances derive?
- *Evidence:* p. 122 shows the CG selection, the group list, "Add all tests in group", the "Patient's tests" box, and the full billing panel — but all values are 0.0 and no text narrates the computation. `[p. 122]`; group totals are computed in the group window (R-CG-04). `[p. 121]`
- *Options:* (a) Group member prices become the patient's test prices; Total of Tests = Σ member prices (matching the group's Total group Price). (b) Prices are re-derived at attach time from the patient's account-type price list. (c) Mixed/manual.
- *Trade-offs:* (a) aligns with the module's stated purpose ("a group with a specific price… for ease of entering test and patient data" `[p. 121]`); (b) would contradict R-CG-01's per-group pricing intent; (c) unsupported.
- *Recommended default:* **(a)**.
- *Confidence:* MEDIUM.

**OQ-7 — Deleting a Custom Group that was already attached to patients.**
- *Question:* Does deleting a group affect historical patient transactions that consumed it?
- *Evidence:* p. 123 shows only the confirmation dialog "Do you want to delete this group?" — no usage warning. `[p. 123]`
- *Options:* (a) Delete affects only the group definition; historical patient data untouched. (b) Deletion blocked if group was used. (c) Cascade removal from patient records.
- *Trade-offs:* (a) preserves financial history, matches minimal-dialog evidence; (b) safest but unsupported; (c) would be destructive to billing history and is unlikely.
- *Recommended default:* **(a)**.
- *Confidence:* LOW (manual silent).

---

## 10. Consolidated Rule Index

| ID | Rule (abbreviated) | Pages | Confidence |
|---|---|---|---|
| R-PL-01 | One test may have multiple prices across lists; lists serve referral entities | 105, 107, 109 | HIGH |
| R-PL-02 | A list is created with a name only | 105, 107 | HIGH |
| R-PL-03 | New list starts with 0 tests (counter shown) | 107 | MEDIUM |
| R-PL-04 | Multiple lists coexist in one dropdown | 108 | MEDIUM |
| R-PL-05 | Adding a test to a list = pick test + enter price manually (default 0) | 105, 107–109 | HIGH |
| R-PL-06 | Same test may carry different prices in different lists | 105, 109, 110 | MEDIUM |
| R-PL-07 | List grid shows Price menu name / Test name / Price | 107, 110 | MEDIUM |
| R-PL-08 | Test price inside a list is freely editable (select → Modify → Save) | 105 | HIGH |
| R-PL-09 | No stated linkage between list price and test default price | 105–111 | LOW |
| R-PL-10 | Remove test from list: select → Delete, no confirmation described | 105 | HIGH |
| R-PL-11 | List name editable (Modify → Save) | 105 | HIGH |
| R-PL-12 | Membership and prices editable after creation; no locking described | 105 | HIGH |
| R-PL-13 | Deleting a whole list requires confirmation (حذف → موافق) | 105 | HIGH |
| R-PL-14 | No stated blocking rule for deleting an in-use list | 105 | LOW |
| R-PR-01 | Printed list groups tests by clinical category; shows price, result date, collection notes | 111 | HIGH |
| R-PR-02 | Prices print in L.E. | 111 | HIGH |
| R-PR-03 | Printout is per selected list; list name in report title | 105, 111 | MEDIUM |
| R-CG-01 | Custom Group = named bundle of tests with own prices; usable for check-ups / entity / doctor; attachable to patient | 118, 121 | HIGH |
| R-CG-02 | Group creation: Add Group → name → Save, with Undo escape; Save disabled until Add Group | 119 | HIGH |
| R-CG-03 | New group starts empty (0 tests / 0 total) | 119 | MEDIUM |
| R-CG-04 | Per-test manual prices; system auto-computes test count and Total group Price (50+10+10=70) | 120, 121 | HIGH |
| R-CG-05-price-independence | No linkage between group prices and contract lists on these pages | 118–123 | MEDIUM |
| R-AT-01 | Attach via patient screen: select CG → pick group → "Add all tests in group" (Delete/All available) | 122 | HIGH |
| R-AT-02 | Group attachment feeds patient billing panel (Total/Discount/Cost/Paid/Balances) | 122 | MEDIUM |
| R-AT-03 | Patient screen holds Account Type and Referral Entity alongside CG selection | 122 | MEDIUM |
| R-CG-05 | Group deletion requires confirmation dialog ("Do you want to delete this group?" OK/Cancel) | 123 | HIGH |
| R-CG-06 | No stated blocking rule for deleting an in-use group | 123 | LOW |

---

*End of extraction. All content derives exclusively from RLS_Learn.pdf pp. 105–111 and 118–123; no external knowledge or implementation material was used.*
