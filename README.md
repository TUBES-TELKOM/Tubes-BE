# Tubes_POS_API

API backend untuk proyek POS (Point of Sale) Food & Beverage berbasis ASP.NET Web API. Repository ini tampaknya disiapkan sebagai tugas besar / project kelompok, dengan pembagian modul yang jelas untuk pengembangan fitur POS secara bertahap.

## Ringkasan Project

Project ini berfokus pada pembuatan backend REST API untuk sistem kasir / POS, dengan cakupan utama:

- manajemen menu
- transaksi penjualan
- proses pembayaran
- riwayat transaksi dan laporan harian
- infrastruktur API seperti Swagger, error handling, dan standard response

Saat ini project masih berupa skeleton awal ASP.NET Web API.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Swagger / OpenAPI
- Swashbuckle

## Struktur Project

```text
Tubes_POS_API/
├── Tubes_POS_API.slnx
├── README.md
└── Tubes_POS_API/
    ├── Controllers/
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Tubes_POS_API.csproj
```

## Fitur yang Direncanakan

### 1. Menu Module

- CRUD menu
- validasi input menu

### 2. Transaction Module

- create transaksi
- tambah / hapus item
- hitung total pembayaran

### 3. Payment Module

- payment flow
- hitung kembalian
- validasi pembayaran

### 4. History & Report Module

- riwayat transaksi
- detail transaksi
- laporan harian

### 5. Infrastructure & Config

- Swagger setup
- global error handling
- sistem konfigurasi
- standard API response

## Cara Menjalankan

### Prasyarat

- .NET SDK 10

### Jalankan Project

```bash
dotnet restore
dotnet run --project Tubes_POS_API/Tubes_POS_API.csproj
```

### Akses Swagger

Jika dijalankan di mode development, Swagger tersedia di URL default aplikasi, biasanya:

```text
https://localhost:<port>/swagger
```

## Pembagian Modul Tim

- Menu Module
- Transaction Module
- Payment Module
- History & Report Module
- Infrastructure & Config
