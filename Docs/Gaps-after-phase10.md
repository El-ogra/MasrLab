## تحليل واقعي للفجوات بعد الموجتين — ما هو مُنجَز وما زال مفتوحاً

### ✅ مغلق فعلياً (19/23)

| الفجوة | التقييم | ما تم |
|--------|--------|-------|
| G-02 | ✅ | `ReferenceValueGender.Both` موجود (جولة سابقة) |
| G-03 | ✅ | `Sample.CollectionStatus` → `SampleStatus` enum |
| G-04 | ✅ | `SettlementStatus` enum + استخدامه في `OutsourcedSample` |
| G-05 | ✅ | `PatientVisit.DoctorId?` + `ReferralEntityId?` |
| G-06 | ✅ | `Patient.DoctorId?` + `ReferralEntityId?` |
| G-07 | ✅ | `VisitTest` — حذف `ExternalLabId`, `CostPrice`, `Notes` |
| G-08 | ✅ | `TestGroup` — حذف `TestIds` |
| G-09 | ✅ | `CashTransaction.EntityId` → `AccountId` |
| G-13 | ✅ | 8 حقول bool جديدة + 4 قديمة = 12 (كامل العدد) |
| G-14 | ✅ | `ExtraServiceItem` entity + collection في `Receipt` |
| G-15 | ✅ | `Price` → `internal set` + تعليق Snapshot |
| G-16 | ✅ | `CardSetting` entity (10 حقول) |
| G-17 | ✅ | `OutsourcedSample.ReceivedAt` (`DateTime?`) |
| G-18 | ✅ | `Sample.CollectedByUserId` (`int?`) |
| G-19 | ✅ | `CommentTemplate` entity |
| G-20 | ✅ | `ReferralEntity.ContactPhone` |
| G-21 | ✅ | `Patient.NationalId` **موجود مسبقاً** — وهمي |
| G-22 | ✅ | `Receipt.ChangeDue` + `RefundToPatient` |
| G-23 | ✅ | `ExternalLab` entity |

### 🔴 ما زال مفتوحاً (4 فجوات حقيقية + 3 مخاطر تشغيلية)

| الأولوية | الفجوة/المخاطرة | شرحه الحقيقي |
|---------|----------------|-------------|
| **P0** 🔴 | **G-01 — PatientHistory** | لم نلمسه إطلاقاً. `PatientHistoryEntry.cs` موجود كـ Read Model لكن لا SQL View ولا Repository ولا Service |
| **P2** 🟡 | **G-10 — Permission.ScreenId/OperationId** | `ScreenType.cs` و `PermissionOperation.cs` enums أُنشئا، لكن `Permission.cs:8-9` لا يزال يحوي `int ScreenId`, `int OperationId` — لم يُغيّر النوع |
| **P2** 🟡 | **G-12 — VOs غير موصلة** | `Age.cs`, `DateRange.cs`, `EgyptianPhone.cs` جاهزة لكن `Patient.cs` لا يزال يستخدم `AgeYears/Months/Days` كـ int — VOs مجردة |
| **P2** 🟡 | **G-11 — NetProfit مخزّن** | موثّق بتعليق لكن لا يزال مخزناً. متعمّد حتى AccountingService |
| **Runtime** 🔴 | **Handlers الثلاثة** | `NotImplementedException` — أي عملية تسجيل/تحديث/تسوية ستفشل |
| **Runtime** 🟠 | **ReceiptDto غير متوافق** | لا يحوي `ExtraServiceItems`, `ChangeDue`, `RefundToPatient` ← AutoMapper سيفشل |
| **مفقود** ⚪ | **Domain1-10.md** | ملف التصميم المرجعي لـ G-12 غير موجود |

### الأمر الحقيقي — ماذا ينقص فعلياً؟

```
G-01 (P0)    → لم نبدأ — الأكبر حجماً (SQL View + Read Model + Service)
G-10 (P2)    → Permission.cs يحتاج ScreenId → ScreenType, OperationId → PermissionOperation  
G-12 (P2)    → VOs موجودة لكن Patient لا يزال يستخدم int — تحتاج Value Converter + تغيير entity
ReceiptDto   → يحتاج 3 حقول جديدة + ExtraServiceItemDto
Handlers ×3  → ثبت NotImplementedException
Domain1-10.md→ مفقود كلياً
```
