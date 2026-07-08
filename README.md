📚 Kütüphane Yönetim Sistemi (Library Management System)
Bu proje, .NET Core MVC mimarisi kullanılarak geliştirilmiş, modern ve dinamik bir Kütüphane Yönetim Sistemi web uygulamasıdır. Proje kapsamında kitapların, üyelerin, ödünç alma işlemlerinin ve kategorilerin dijital ortamda kolayca yönetilmesi amaçlanmıştır.

🛠️ Kullanılan Teknolojiler ve Mimari
Backend: C# / .NET Core MVC (Model-View-Controller)
Frontend: HTML5, CSS3, JavaScript, Bootstrap
Veritabanı: PostgreSQL / Supabase
Paket Yönetimi: NuGet
📂 Proje Yapısı (Directory Structure)
Proje, kurumsal yazılım standartlarına uygun olarak katmanlı ve düzenli bir MVC mimarisine sahiptir:

📁 Controllers: Gelen istekleri (HTTP Requests) karşılayan ve iş mantığını (Business Logic) yöneten kontrolcüler.
📁 Models: Veritabanı tablolarını, veri transfer nesnelerini (DTO) ve validation kurallarını barındıran sınıflar.
📁 Views: Kullanıcı arayüzünü oluşturan, dinamik .cshtml (Razor View) sayfaları.
📁 wwwroot: CSS, JavaScript, resimler ve üçüncü parti kütüphanelerin (Bootstrap vb.) tutulduğu statik klasör.
📄 Program.cs: Uygulamanın başlangıç noktası, bağımlılıkların (Dependency Injection) ve middleware hatlarının tanımlandığı ana dosya.
📄 appsettings.json: Veritabanı bağlantı adresleri (Connection String) ve uygulamaya ait konfigürasyon ayarları.
🚀 Kurulum ve Çalıştırma (Setup)
Projeyi kendi yerel bilgisayarınızda (local) çalıştırmak için aşağıdaki adımları takip edebilirsiniz:

1. Gereksinimler
.NET SDK (Sürümünüze uygun olan)
Visual Studio 2022 veya VS Code
Aktif bir PostgreSQL / Supabase veritabanı bağlantısı
2. Projeyi Klonlayın
git clone [https://github.com/KibrisFathi/library.git](https://github.com/KibrisFathi/library.git)
cd library