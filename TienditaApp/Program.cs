using TienditaApp.Data;
using TienditaApp.Repositories;
using Microsoft.Data.Sqlite;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// =========================
// DATABASE (SQLite + Dapper)
// =========================
builder.Services.AddSingleton<DapperContext>();

builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<VentaRepository>();
builder.Services.AddScoped<ClienteRepository>();

// =========================
// RAZOR PAGES
// =========================
builder.Services.AddRazorPages();

// =========================
// SESSION
// =========================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.WebHost.UseUrls("http://0.0.0.0:8080");

var app = builder.Build();

// =========================
// ERROR HANDLING
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// =========================
// MIDDLEWARE
// =========================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// =========================
// RAZOR PAGES MAP
// =========================
app.MapRazorPages();

// =========================
// INIT DATABASE (CLAVE PARA RENDER)
// =========================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DapperContext>();

    using var connection = context.CreateConnection();
    connection.Open();

    connection.Execute(@"
        CREATE TABLE IF NOT EXISTS Usuarios (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Usuario TEXT NOT NULL UNIQUE,
            Password TEXT NOT NULL
        );
        
            INSERT INTO Usuarios (Nombre, Usuario, Password)
            SELECT 'Fercho', 'Fercho', '1234'
            WHERE NOT EXISTS (
                SELECT 1 FROM Usuarios WHERE Usuario = 'Fercho'
            );
        

        CREATE TABLE IF NOT EXISTS Clientes (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Telefono TEXT
        );

        CREATE TABLE IF NOT EXISTS Productos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Precio REAL NOT NULL,
            Stock INTEGER NOT NULL DEFAULT 0
        );

       
        CREATE TABLE IF NOT EXISTS Ventas (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        ProductoId INTEGER,
        ClienteId INTEGER,
        Cantidad INTEGER,
        Total REAL,
        EsFiado INTEGER,
        Pagado REAL,
        Fecha TEXT
        );

        CREATE TABLE IF NOT EXISTS VentaDetalle (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            VentaId INTEGER NOT NULL,
            ProductoId INTEGER NOT NULL,
            Cantidad INTEGER NOT NULL,
            Precio REAL NOT NULL,
            Subtotal REAL NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Pagos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ClienteId INTEGER NOT NULL,
            Fecha TEXT NOT NULL,
            Monto REAL NOT NULL
        );
    ");
}

// =========================
// RUN APP
// =========================
app.Run();