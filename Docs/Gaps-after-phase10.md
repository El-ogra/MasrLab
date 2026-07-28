📋 قائمة الفجوات الكاملة — после تحليل Domain Layer (المرحلتان الأولى والثانية)
🔴 P0 — فجوات حرجة (Critical) — تمنع اكتمال Domain
#	الفجوة	المصدر	الشرح الفني	الأثر	الاعتماديات
G-01	PatientHistory غائب كلياً	Spec + PDF (ص 70-78) + Repo	المواصفات تذكر 16 عموداً، والـ PDF يظهر الإدراج التلقائي في التقرير وزر Patient History، لكن لا Entity ولا Interface ولا Read Model في الكود	Module 6 (التقارير) غير قابل للتنفيذ؛ لا يمكن عرض سجل المريض التاريخي	يحتاج SQL View + Read Model record
G-02	Gender.Both مفقود	Spec + Repo	ReferenceValue يحتاج قيمة "كلا الجنسين" للنطاقات المرجعية (مثل Hemoglobin)، لكن Gender enum يحوي Male, Female فقط	تحاليل ذات نطاقات مشتركة بين الجنسين لا يمكن تمثيلها	يحتاج ReferenceValueGender enum منفصل
G-03	Sample.CollectionStatus من نوع string	Repo + PDF (ص 208-210)	SampleStatus enum جاهز (Collected, NotCollected) لكنه غير مستخدم في Sample، والحقل من نوع string يسمح بقيم غير معرّفة	تناقض داخلي في الكود؛ عدم وجود Type Safety	ترحيل PatientVisit.SampleStatus إلى Sample
G-04	OutsourcedSample.SettlementStatus من نوع string	Repo + PDF (ص 140-148)	لا يوجد SettlementStatus enum، الحقل string يمنع أي منطق State Machine لتصفية الحسابات الخارجية	تعذر تتبع دورة حياة التصفية (Pending → Partial → Settled)	يحتاج SettlementStatus enum جديد
G-05	PatientVisit يفتقد DoctorId و ReferralEntityId	Spec + PDF	المريض قد يأتي في زيارة بطبيب مختلف عن طبيبه المعتاد، لكن PatientVisit لا يحوي هذين الحقلين	لا يمكن تسجيل زيارة بطبيب بديل أو جهة إحالة مختلفة	يحتاج إضافة FK nullable + Business Invariant
G-13	Medical History sub-fields (~12 حقلاً) مفقودة كلياً	PDF (ص 10-14)	شاشة إضافة مريض تحوي خانات: علاج سكر، ضغط، كبد، مفاصل، سيولة، ريف، فشل كلوي، ذئبة حمراء، إلخ — لا أثر لها في Domain	فقدان بيانات سريرية أساسية؛ التاريخ المرضي لا يمكن تخزينه أو عرضه	يحتاج كيان PatientMedicalHistory أو Value Object
🟠 P1 — فجوات مهمة (Important) — تؤثر على سلامة التصميم
#	الفجوة	المصدر	الشرح الفني	الأثر	الاعتماديات
G-06	Patient.DoctorId و Patient.ReferralEntityId غير nullable	Repo	int (NOT NULL) يمنع تسجيل مرضى نقدي (Cash) بدون طبيب أو جهة إحالة	خرق سيناريو شائع في المعامل — مرضى يأتون مباشرة دون جهة	تغيير النوع إلى int?
G-07	تكرار بيانات بين OutsourcedSample و VisitTest	Repo + Spec	VisitTest يحوي ExternalLabId, CostPrice, Notes وهي نفسها في OutsourcedSample	ازدواج بيانات؛ تحديث في مكان لا ينعكس على الآخر	حذف الحقول من VisitTest، ترك IsOutsourced flag فقط
G-08	TestGroup.TestIds (string) + TestGroupItem معاً	Repo	TestGroup يحوي حقل TestIds من نوع string (قائمة IDs مفصولة بفاصل) بجانب جدول TestGroupItem	مصدران للحقيقة — خطر تعارض وتكرار	حذف TestIds، الاعتماد على TestGroupItem
G-09	CashTransaction.EntityId بلا FK لـ Account	Repo + Spec	CashTransaction يحوي EntityId عام دون FK صريح لـ Account	R-22 مكسورة؛ لا يمكن تتبع التدفق النقدي لكل حساب	إضافة AccountId FK أو توثيق EntityId
G-11	Account.NetProfit مخزّن كـ decimal	Repo + PDF (ص 174-189)	NetProfit يجب أن يكون محسوباً (TotalIncome - TotalDiscount - OutsourcedCost - CashOutflows + CashDeposits) وليس حقل مخزّن	خطأ بيانات عند عدم تحديث الحقل بعد كل معاملة	Domain Service للحساب أو Snapshot مؤقت
G-14	Extra Services غير مدعومة في Receipt	PDF (ص 17-21)	يمكن إضافة خدمة غير تحليلية بإدخال المبلغ مسبوقاً بـ + في شاشة الحساب؛ لا يوجد حقل ExtraServices في Receipt	لا يمكن إضافة تكاليف إضافية (سحب عينة من المنزل/تغليف/شحن)	إضافة ExtraServiceItems collection لـ Receipt
G-15	VisitTest.Price snapshotted behaviour غير موثّق	PDF	الـ PDF يؤكد أن السعر يُثبّت وقت الزيارة ولا يتغير تلقائياً عند تعديل قائمة الأسعار — هذا Business Invariant غير مذكور	قد يُنفّذ سلوك خاطئ (قراءة ديناميكية) مما يغير فواتير الزيارات السابقة	توثيق كـ Business Invariant + ضمان snapshot
G-17	🆕 OutsourcedSample.ReceivedAt مفقود	domain2.md القسم 10.5	State Machine للعينة الخارجية (8.2) تحتاج ReceivedAt لتتبع استلام النتيجة من المعمل الخارجي	لا يمكن تتبع وقت استلام النتيجة	إضافة DateTime? ReceivedAt إلى OutsourcedSample
G-21	🆕 Patient.NationalId (الرقم القومي) مفقود	PDF (ص 10-14)	شاشة إضافة مريض تحتوي حقل "الرقم القومي" لكنه غير موجود في كيان Patient	عدم القدرة على توثيق هوية المريض الرسمية	إضافة NationalId كـ string مع Validation
🟡 P2 — فجوات تحسينية (Enhancement) — تحسن الجودة والنوعية
#	الفجوة	المصدر	الشرح الفني	الأثر	الاعتماديات
G-10	Permission.ScreenId / OperationId من نوع int	Repo + Spec	أرقام حرّة بدون enum تسمح بقيم غير معرّفة — Spec يعدد 13 صلاحية محددة	ضعف Type Safety؛ صلاحية خاطئة قد تُمنح بخطأ	إنشاء ScreenType + PermissionOperation enum
G-12	لا Value Objects في الكود	Repo	جميع القيم بدائية (Primitive Obsession): Age, Phone, DateRange, NormalRange — كلها string أو حقول منفصلة	تكرار Validation في كل مكان؛ لا إعادة استخدام	إنشاء Age, EgyptianPhone, DateRange, NormalRange VOs
G-16	Card Settings غير مذكورة	PDF (ص 190)	الـ PDF يذكر "إعدادات الكارنيه" (بطاقة المريض) كشاشة إعدادات منفصلة — غير موجودة في Spec	إعدادات طباعة الكارنيه (البطاقة التعريفية للمريض) غير مدعومة	إضافة SystemSetting keys أو entity للطباعة
G-18	🆕 Sample.CollectedByUserId مفقود	domain2.md القسم 10.8	وقت سحب العينة يحتاج توثيق المستخدم الذي قام بالسحب للتدقيق	فقدان مسؤولية السحب (من سحب العينة؟)	إضافة CollectedByUserId (nullable)
G-19	🆕 CommentTemplate entity مفقود	domain2.md القسم 10.9	PDF يذكر قوالب كومنتات جاهزة (Templates) يمكن إضافتها لتحليل معين، لكن لا Entity لها	يجب إعادة كتابة الكومنتات الشائعة يدوياً في كل مرة	إنشاء CommentTemplate entity (Id, TestId, Text)
G-20	🆕 ContactPerson / ContactPhone في ReferralEntity مفقودان	domain2.md القسم 10.10	الـ PDF يذكر وجود اسم مسؤول + تليفون للتواصل مع جهة الإحالة	صعوبة التواصل مع جهات الإحالة	إضافة ContactPerson و ContactPhone
G-22	🆕 ChangeDue / RefundToPatient في Receipt مفقود	domain2.md القسم 10.2	شاشة الحساب تظهر "الباقي للمريض" عند دفع مبلغ أكبر من المطلوب (حالة رد باقي)	لا يمكن تسجيل عملية رد باقي للمريض	إضافة ChangeDue أو آلية Refund
G-23	🆕 ExternalLab entity غير مؤكد الوجود	domain2.md القسم 10.5	OutsourcedSample.ExternalLabId يحتاج كيان ExternalLab مرجعي (اسم المعمل الخارجي، عنوانه، بيانات التواصل)	ExternalLabId كـ int بدون Entity مرجعي يضعف التكامل	إنشاء ExternalLab entity أو تأكيد وجوده
📊 إجمالي الفجوات: 23 فجوة
المستوى	العدد	القائمة
🔴 P0	6	G-01, G-02, G-03, G-04, G-05, G-13
🟠 P1	9	G-06, G-07, G-08, G-09, G-11, G-14, G-15, G-17 🆕, G-21 🆕
🟡 P2	8	G-10, G-12, G-16, G-18 🆕, G-19 🆕, G-20 🆕, G-22 🆕, G-23 🆕
⚠️ تضاربات/تناقضات تحتاج حل قبل التنفيذ
C-01: ReferralEntity.PriceListId — NOT NULL vs Nullable
المصدر	الموقف	النص
domain1.md (D-03)	✅ nullable (int?)	"ReferralEntity.PriceListId يجب أن يكون nullable — الجهة قد تستخدم القائمة الافتراضية"
domain2.md (INV-16)	❌ NOT NULL	"ReferralEntity.PriceListId يجب أن يكون موجوداً وفعّالاً"
التحليل:

الـ PDF يذكر أن الجهات تُربط بقوائم أسعار خاصة، لكن ليس كل جهة ملزمة بقائمة
طبيب فردي يحول مريضاً واحداً في الشهر ← يستخدم القائمة الافتراضية (Cash) ← PriceListId = NULL
شركة تأمين كبيرة ← لها قائمة أسعار خاصة ← PriceListId = NOT NULL
القرار: nullable هو الصحيح. يجب تعديل INV-16 إلى: "إذا كان ReferralEntity.PriceListId != null، يجب أن تكون القائمة موجودة وفعّالة".

التأثير على P0: لا يؤثر — P0 لا يعتمد على PriceListId.

C-02: E-07 SampleUncollectedReverted — غير مؤكد المصدر
المصدر	الموقف	النص
domain2.md (القسم 6)	✅ مضاف كحدث	"يُبث عند إلغاء سحب العينة — عودة الحالة من Collected إلى NotCollected"
PDF (ص 208-210)	❌ غير مذكور	يذكر حالتين فقط (مسحوبة/غير مسحوبة) وطريقة التغيير (نقرة واحدة)، لا يذكر زر تراجع أو undo
التحليل:

الوكيل الثاني افترض وجود الإلغاء من المنطق التجاري العام
لكن PDF لم يذكره، والـ Spec لم يذكره
عملياً، قد تحتاجه الإدارة، لكن ليس ضمن المصادر المعتمدة
القرار: تسجيله كـ "مقترح" وليس "مؤكد". إذا احتيج لاحقاً، يُضاف. لا يُنفّذ في الإصدار الحالي.

التأثير على P0: لا يؤثر — P0 لا يتعلق بـ SampleUncollectedReverted.

✅ ملخص التأثير على P0
P0 Item	يعتمد على C-01؟	يعتمد على C-02؟
G-01 PatientHistory	لا	لا
G-02 Gender.Both	لا	لا
G-03 CollectionStatus → SampleStatus	لا	لا
G-04 SettlementStatus enum	لا	لا
G-05 PatientVisit FK (DoctorId, ReferralEntityId)	لا	لا
G-13 MedicalHistory fields	لا	لا
جميع P0 لا تعتمد على أي تضارب → يمكن البدء بها فوراً.

