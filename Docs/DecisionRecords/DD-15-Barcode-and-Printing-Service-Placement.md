# DD-15 — موضع خدمات الباركود والطباعة

- **الحالة:** مقبول
- **التاريخ:** 2026-08-10

## السياق (Context)

- كانت طبقة العرض تحتوي فئة `BarcodeGenerator` فارغة، بينما يحتاج النظام توليد PNG وPDF وطباعة فعلية.

## القرار (Decision)

يبنى المنطق الكامل للباركود والطباعة داخل `BarcodeService` و`PrintService` في Infrastructure، وليس في Presentation.

## النتائج (Consequences)

- حُذفت فئة العرض الفارغة؛ ويبقى ربط التقارير الحالية بالخدمات لجولة لاحقة.

## الروابط (Links)

- `src/MasrLab.Infrastructure/Services/BarcodeService.cs`
- `src/MasrLab.Infrastructure/Services/PrintService.cs`
