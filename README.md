# ABP Öğrenci Otomasyonu (abp_obs_project)

Bu rehber; mimariyi, kurulum/sunum adımlarını, roller–izinleri, önemli dosya yollarını ve eklediğimiz tüm özellikleri örnek kodlarla birlikte anlatan sunum‑tarzı bir dokümandır. Verilen tüm dosya yolları projedeki gerçek konumlardır ve IDE’nizde tıklanarak açılabilir.

## İçindekiler
- Genel Bakış
- Mimari ve Katmanlar
- Önemli Dosya ve Klasörler
- Kurulum ve Çalıştırma
- Docker ile Çalıştırma
- Giriş Bilgileri ve Roller
- İzinler (Permissions)
- Veri Tohumlama (Seeding)
- Swagger ve Test
- Blazor UI (Sayfalar ve Menüler)
- Öğrenci Özellikleri
- Öğretmen Özellikleri
- Ders / Not / Devamsızlık
- Önbellekleme (Caching)
- Olaylar ve Arka Plan (Events)
- Sağlık Kontrolleri (HealthChecks)
- Günlükleme (Logging) ve Hata Ayıklama
- Sık Sorunlar ve Çözümler

---

# Login-Register
- Kullanıcı kayıt olduktan sonra admin tarafından yetki verilmeyi bekler. Obs tarzı bir sistem için en mantıklı kayıt aşamasının bu olduğunu düşündüm. 


## Genel Bakış
- Proje, ABP Framework üzerine inşa edilmiş tam yığın (full‑stack) bir “Öğrenci Otomasyon Sistemi”dir.
- Kimlik doğrulama: OpenIddict + ABP Account modülü.
- Arayüz: Blazor Server (Bootstrap 5 + Blazorise bileşenleri).
- Veri erişim: EF Core + Repository Pattern.
- Ekler: Redis önbellek, Swagger, Docker, HealthChecks, Role/Permission yönetimi.

Repo kökünde iki ana bölüm bulunur:
- Backend/Frontend birleşik çözüm: `abp_obs_project/` altında çok katmanlı proje yapısı.
- Yardımcı dosyalar: Docker Compose, ReadMe, vs.

---

## Mimari ve Katmanlar
ABP’nin tipik temiz mimarisi izlenir:
- Domain Shared: `abp_obs_project/src/abp_obs_project.Domain.Shared` – Sabitler (Consts), Localization, Exception tipleri.
- Domain: `abp_obs_project/src/abp_obs_project.Domain` – Varlıklar (Entities), Domain servisleri (Manager), Repository sözleşmeleri.
- Application Contracts: `abp_obs_project/src/abp_obs_project.Application.Contracts` – DTO’lar, AppService arayüzleri, Permission tanımları.
- Application: `abp_obs_project/src/abp_obs_project.Application` – AppService uygulamaları, uygulama mantığı.
- EFCore: `abp_obs_project/src/abp_obs_project.EntityFrameworkCore` – DbContext ve EF Core mapping/repository.
- Blazor (UI): `abp_obs_project/src/abp_obs_project.Blazor` – UI, menüler, sayfalar, Swagger barındırma.
- DbMigrator: `abp_obs_project/src/abp_obs_project.DbMigrator` – Migration/Seeding çalıştırıcı.

---

## Önemli Dosya ve Klasörler
- Çözüm kökü
  - `abp_obs_project/docker-compose.yml`
  - `abp_obs_project/README.md` (bu dosya)
  - `abp_obs_project/Abp_README.md` (ABP’nin oluşturduğu başlangıç notları)

- Domain Shared
  - Localization:
    - `abp_obs_project/src/abp_obs_project.Domain.Shared/Localization/abp_obs_project/en.json`
    - `abp_obs_project/src/abp_obs_project.Domain.Shared/Localization/abp_obs_project/tr.json`
  - Özel Hata Tipi: `abp_obs_project/src/abp_obs_project.Domain.Shared/GlobalExceptions/StudentAutomationException.cs`

- Domain (İş kuralları)
  - Öğrenciler: `abp_obs_project/src/abp_obs_project.Domain/Students/StudentManager.cs`
  - Öğretmenler: `abp_obs_project/src/abp_obs_project.Domain/Teachers/TeacherManager.cs`
  - Dersler: `abp_obs_project/src/abp_obs_project.Domain/Courses/CourseManager.cs`
  - Notlar: `abp_obs_project/src/abp_obs_project.Domain/Grades/GradeManager.cs`
  - Devamsızlık: `abp_obs_project/src/abp_obs_project.Domain/Attendances/AttendanceManager.cs`
  - Seed: `abp_obs_project/src/abp_obs_project.Domain/Data/abp_obs_projectDataSeederContributor.cs`
  - OpenIddict Seed: `abp_obs_project/src/abp_obs_project.Domain/OpenIddict/OpenIddictDataSeedContributor.cs`

- Application Contracts (DTO/Permission/Interfaces)
  - Öğrenciler: `abp_obs_project/src/abp_obs_project.Application.Contracts/Students/*`
  - Öğretmenler: `abp_obs_project/src/abp_obs_project.Application.Contracts/Teachers/*`
  - Dersler: `abp_obs_project/src/abp_obs_project.Application.Contracts/Courses/*`
  - Notlar: `abp_obs_project/src/abp_obs_project.Application.Contracts/Grades/*`
  - Devamsızlık: `abp_obs_project/src/abp_obs_project.Application.Contracts/Attendances/*`
  - İzin tanımları: `abp_obs_project/src/abp_obs_project.Application.Contracts/Permissions/abp_obs_projectPermissionDefinitionProvider.cs`

- Application (AppService’ler)
  - Öğrenci: `abp_obs_project/src/abp_obs_project.Application/Students/StudentAppService.cs`
  - Öğretmen: `abp_obs_project/src/abp_obs_project.Application/Teachers/TeacherAppService.cs`
  - Ders: `abp_obs_project/src/abp_obs_project.Application/Courses/CourseAppService.cs`
  - Not: `abp_obs_project/src/abp_obs_project.Application/Grades/GradeAppService.cs`
  - Devamsızlık: `abp_obs_project/src/abp_obs_project.Application/Attendances/AttendanceAppService.cs`

- Blazor (UI)
  - Ana modül: `abp_obs_project/src/abp_obs_project.Blazor/abp_obs_projectBlazorModule.cs`
  - Menüler:
    - Admin: `abp_obs_project/src/abp_obs_project.Blazor/Menus/Admin/AdminMenuContributor.cs`
    - Teacher: `abp_obs_project/src/abp_obs_project.Blazor/Menus/Teacher/TeacherMenuContributor.cs`
    - Student: `abp_obs_project/src/abp_obs_project.Blazor/Menus/Student/StudentMenuContributor.cs`
  - Rota/Dashboard yönlendirme: `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Index.razor`
  - Student sayfaları: `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Student/*`
  - Teacher sayfaları: `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Teacher/*`
  - Admin sayfaları: `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Admin/*`

---

## Kurulum ve Çalıştırma
Ön Koşullar: .NET SDK (>= 9.0), Node/Yarn (Blazor UI bağımlılıkları için), Redis (opsiyonel), RabbitMQ (opsiyonel – HealthCheck).

1) Bağımlılıkların yüklenmesi (UI için)
   - Klasör: `abp_obs_project/src/abp_obs_project.Blazor`
   - Komut: `yarn install` (veya `npm install`)

2) Veritabanı ve seed
   - İlk migrasyon ve seed için DbMigrator’ı çalıştırın:
     - Klasör: `abp_obs_project/src/abp_obs_project.DbMigrator`
     - Komut: `dotnet run`

3) Uygulama
   - Klasör: `abp_obs_project/src/abp_obs_project.Blazor`
   - Komut: `dotnet run`
   - Uygulama: `https://localhost:44368`

4) Swagger
   - `https://localhost:44368/swagger/index.html`

> Not: Development ortamında çalıştığınızdan emin olun (Developer Exception Page ve dosya sisteminden gömülü kaynak değiştirme için).

### appsettings.secrets.json (İsteğe Bağlı)
Özel ayarlar için `abp_obs_project/src/abp_obs_project.Blazor/appsettings.secrets.json` dosyasını kullanın (repoya eklemeyin). Örnek alanlar: bağlantı stringleri, Redis/RabbitMQ URL’leri.

---

## Docker ile Çalıştırma
- `abp_obs_project/docker-compose.yml` temel servisleri tanımlar (DB, Redis, vb.).
- Komutlar:
  - `docker compose up -d`
  - Servisler ayağa kalktıktan sonra DbMigrator’ı bir kez çalıştırın.

---

## Giriş Bilgileri ve Roller
Seed ile roller ve gerekli izinler tanımlanır. Örnek kullanıcılarınızı README’ye ekleyin (admin/teacher/student). Kimlik doğrulama ABP Account + OpenIddict.

Rol ve İzinler (örnek):
- Admin: Tüm modüllerde `*.ViewAll/Create/Edit/Delete` + grup default izinleri.
- Teacher: Öğretmen ve ilişkili modüller için default ve gerekli izinler (ders/grade/attendance yönetimi).
- Student: Kendi verisini görebileceği default izinler.

---

## İzinler (Permissions)
- Tanımlar: `abp_obs_project/src/abp_obs_project.Application.Contracts/Permissions/abp_obs_projectPermissionDefinitionProvider.cs`
- Örnek isimler:
  - `abp_obs_project.Students`, `abp_obs_project.Students.ViewAll`, `abp_obs_project.Students.Create` …
  - `abp_obs_project.Teachers`, `abp_obs_project.Courses`, `abp_obs_project.Grades`, `abp_obs_project.Attendances`, `abp_obs_project.Enrollments`

> UI görünürlüğü ve menü öğeleri de bu izinlere göre şekillenir.

---

## Veri Tohumlama (Seeding)
- Dosya: `abp_obs_project/src/abp_obs_project.Domain/Data/abp_obs_projectDataSeederContributor.cs`
- Roller ve izin atamaları, ilk kullanıcılar ve OpenIddict istemcileri burada tanımlanır.

- OpenIddict istemci ve scope seeding örneği: `abp_obs_project/src/abp_obs_project.Domain/OpenIddict/OpenIddictDataSeedContributor.cs`
  - Swagger istemcisi otomatik oluşturulur, gerekli izinler ve yönlendirme URL’leri atanır.

---

## Swagger ve Test
- Adres: `https://localhost:44368/swagger/index.html`
- Auth: Swagger UI içinde “Authorize” butonu ile token alın.
- Not: Aynı HTTP method+path ile çakışan action’lar Swagger’da 500’e neden olur. Çözüm örneği: parametresiz `GetMyCoursesAsync()` metodu `[RemoteService(false)]` ile dışa kapatıldı.

---

## Blazor UI (Sayfalar ve Menüler)
- Yönlendirme: `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Index.razor`
  - Admin: `/admin/dashboard`
  - Teacher: `/teacher/dashboard`
  - Student: `/student/dashboard`

- Menüler
  - Admin: `abp_obs_project/src/abp_obs_project.Blazor/Menus/Admin/AdminMenuContributor.cs`
  - Teacher: `abp_obs_project/src/abp_obs_project.Blazor/Menus/Teacher/TeacherMenuContributor.cs`
  - Student: `abp_obs_project/src/abp_obs_project.Blazor/Menus/Student/StudentMenuContributor.cs`

Örnek: Rol bazlı dashboard yönlendirmesi (Index.razor)

```
abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Index.razor
```

```
// Admin: Students.ViewAll + Teachers.ViewAll
// Teacher: Courses/Grades/Attendances ViewAll’dan en az biri
// Aksi halde Student kabul edilir
var hasStudentManagement = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Students.ViewAll);
var hasTeacherManagement = await AuthorizationService.IsGrantedAsync(abp_obs_projectPermissions.Teachers.ViewAll);
if (hasStudentManagement && hasTeacherManagement) NavigationManager.NavigateTo("/admin/dashboard", true);
...
```

Örnek: Student menüsü sadece öğrenci profiline uygun olduğunda gösterilir ve diğer menüler temizlenir:

```
abp_obs_project/src/abp_obs_project.Blazor/Menus/Student/StudentMenuContributor.cs
```

```
// Admin/Teacher izinlerine sahipse öğrenci menüsü oluşturulmaz
if (hasStudentsViewAll || hasTeachersViewAll || hasGradesCreate || hasAttendancesCreate || hasCoursesCreate) return;

// Sadece öğrenci menüsünü göstermek için mevcut öğeleri temizle
context.Menu.Items.Clear();

// Dashboard, My Courses, My Grades, My Attendances, Profile eklenir
```

---

## Öğrenci Özellikleri
- Profil: `StudentProfile` (kendi bilgilerini gör/güncelle – Identity senkron)
  - `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Student/StudentProfile.razor`
  - `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Student/StudentProfile.razor.cs`
- Notlar: `StudentGrades` (özet, tablo, filtre)
- Derslerim: `StudentMyCourses` (arama/filtre/sayfalama)
- Devamsızlıklarım: `StudentMyAttendances` (filtreler + özet)
- Self endpoint’ler: `GetMeAsync`, `GetMyCoursesAsync`, `GetMyGradesAsync`, `GetMyAttendancesAsync`

Örnek: StudentMyCourses sunucu tarafı filtre/sıralama/sayfalama kullanımı

```
abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Student/StudentMyCourses.razor.cs
```

```
var input = new GetMyCoursesInput {
  FilterText = SearchText,
  Status = StatusFilter,
  CreditsMin = MinCredits,
  CreditsMax = MaxCredits,
  Sorting = CurrentSorting,
  SkipCount = (CurrentPage - 1) * PageSize,
  MaxResultCount = PageSize
};
var coursesResult = await CourseAppService.GetMyCoursesAsync(input);
```

Örnek: Öğrencinin kendi profilini güncellemesi (Identity ile senkron)

```
abp_obs_project/src/abp_obs_project.Application/Students/StudentAppService.cs: UpdateMyProfileAsync
```

---

## Öğretmen Özellikleri
- Profilim: `TeacherMyProfile` (güncelleme + Identity senkron)
  - `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Teacher/TeacherMyProfile.razor`
  - `abp_obs_project/src/abp_obs_project.Blazor/Components/Pages/Teacher/TeacherMyProfile.razor.cs`
- Derslerim, Öğrencilerim, Not/Devamsızlık yönetimi sayfaları mevcuttur.

---

## Ders / Not / Devamsızlık
- Kurs (Course) AppService: `abp_obs_project/src/abp_obs_project.Application/Courses/CourseAppService.cs`
  - Öğretmenler için ViewAll yetkisi yoksa kendi derslerine filtrelenir.
  - `GetMyCoursesAsync(GetMyCoursesInput)` ile öğrenciye ait derslerin server-side filtreli listesi döner.
- Notlar (Grade) AppService: `abp_obs_project/src/abp_obs_project.Application/Grades/GradeAppService.cs`
  - `GetMyGradesAsync()` öğrenciye özel listeyi döner (CourseName/StudentName doldurulmuştur).
- Devamsızlık (Attendance) AppService: `abp_obs_project/src/abp_obs_project.Application/Attendances/AttendanceAppService.cs`
  - `GetMyAttendancesAsync()` öğrenciye özel listeyi döner.

Örnek: Öğrencinin dersleri – çakışan Swagger route’u önlemek için dış API’dan gizleme

```
abp_obs_project/src/abp_obs_project.Application/Courses/CourseAppService.cs
```

```
[RemoteService(false)]
public virtual async Task<ListResultDto<CourseDto>> GetMyCoursesAsync() { ... }
```

---

## Önbellekleme (Caching)
- Anahtarlar ve kullanım: `abp_obs_project/src/abp_obs_project.Application/*` içerisinde (Students/Courses listelerinde örnek)
- Redis desteği etkinleştirilebilir (appsettings üzerinden) ve `IObsCacheService` ile kullanılır.

Örnek: Öğrenci listesinde basit isteklerde tüm listeyi cache’leyip sayfalamayı bellekte uygulama

```
abp_obs_project/src/abp_obs_project.Application/Students/StudentAppService.cs: GetListAsync
```

```
var cachedResult = await _cacheService.GetOrSetAsync(
  ObsCacheKeys.Students.List,
  async () => { /* tüm listeyi getir ve map et */ }
);
// ardından Skip/Take ile sayfalama uygula
```

---

## Olaylar ve Arka Plan (Events)
- Örnek: Öğrenci/Not oluşturma sonrası `IDistributedEventBus` ile event yayınlanır.
- Dosya örnekleri: `StudentAppService.cs`, `GradeAppService.cs`.

Örnek: Not oluşturulduğunda dağıtık olay yayını

```
abp_obs_project/src/abp_obs_project.Application/Grades/GradeAppService.cs: CreateAsync
```

```
await _distributedEventBus.PublishAsync(new GradeCreatedEto { GradeId = grade.Id, ... });
```

---

## Sağlık Kontrolleri (HealthChecks)
- Ayar: `abp_obs_project/src/abp_obs_project.Blazor/abp_obs_projectBlazorModule.cs`
- RabbitMQ sağlık kontrolü eklenmiştir; dev ortamında kapatmak isterseniz appsettings ile yönetebilirsiniz.
- Uç nokta: `/health`

---

## Günlükleme (Logging) ve Hata Ayıklama
- ABP’nin Exception Handling ve Dynamic Claims middleware’leri devrede.
- Geliştirme modunda ayrıntılı hata sayfası aktiftir.
- Swagger 500 sorunlarında en yaygın sebep çakışan action’lar veya hatalı şema üretimidir (örnek çözüm uygulandı).

---

## Sık Sorunlar ve Çözümler
- Swagger 500 – Conflicting method/path
  - Sebep: Aynı HTTP method + path’e düşen birden fazla action.
  - Çözüm: Overload/parametresiz metodları `[RemoteService(false)]` ile dış API’dan gizleyin veya alternatif route verin.

- Authorization hataları (AccessDenied / PermissionRequirement)
  - Sebep: Rol/izin ataması eksik veya kullanıcı rolü güncel değil.
  - Çözüm: Role Permission ekranından ilgili izni verin, DbMigrator ile seed’i tekrar çalıştırın ve kullanıcıyla yeniden giriş yapın.

- DbContext disposed (UI’da IQueryable enumerate)
  - Sebep: Repository’den alınan IQueryable UI katmanında enumerate edilirse UoW scope dışına taşabilir.
  - Çözüm: UI’dan AppService çağrıları ile veriyi çektik, sorun giderildi.

- “Teacher/Student” dashboard yönlendirme yanlış rol algısı
  - Sebep: Öğretmen/kullanıcı tespiti default izinlerle yapıldıysa yanlış eşleşebilir.
  - Çözüm: Yönetim (ViewAll) izinlerine göre ayrım yaptık.

- Menü çakışmaları (Admin/Teacher/Student aynı anda görünmesi)
  - Sebep: Menü katkıcılarında koşulların yeterince ayırt edici olmaması.
  - Çözüm: Student menüsünde `context.Menu.Items.Clear()` ile menüyü temizleyip sadece öğrenci öğeleri eklendi; Teacher menüsü Admin koşulunu ayrı kontrol eder.

---

