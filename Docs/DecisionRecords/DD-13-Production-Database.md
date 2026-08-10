# DD-13 — قاعدة بيانات الإنتاج

- **الحالة:** مقبول
- **التاريخ:** 2026-08-10

## السياق (Context)

- يحتاج MasrLab قراراً ثابتاً لقاعدة بيانات بيئة الإنتاج.

## القرار (Decision)

قاعدة بيانات الإنتاج هي **SQL Server Express**.

## النتائج (Consequences)

- يستمر إعداد Infrastructure في استخدام موفر EF Core لـ SQL Server.

## الروابط (Links)

- `src/MasrLab.Infrastructure/DependencyInjection.cs`
