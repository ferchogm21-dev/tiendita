using Dapper;
using TienditaApp.Data;
using TienditaApp.Models;

namespace TienditaApp.Repositories
{
    public class UsuarioRepository
    {
        private readonly DapperContext _context;

        public UsuarioRepository(DapperContext context)
        {
            _context = context;
        }

        // 🔹 Obtener todos
        public List<Usuario> ObtenerTodos()
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    Id,
                    Nombre,
                    Usuario AS UsuarioNombre,
                    Password,
                    Rol,
                    NombreNegocio,
                    NumeroCuenta,
                    WhatsApp
                FROM Usuarios
                ORDER BY Id DESC
            ";

            return connection.Query<Usuario>(sql).ToList();
        }

        // 🔹 Obtener por ID
        public Usuario? ObtenerPorId(int id)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    Id,
                    Nombre,
                    Usuario AS UsuarioNombre,
                    Password,
                    Rol,
                    NombreNegocio,
                    NumeroCuenta
                FROM Usuarios
                WHERE Id = @Id
            ";

            return connection.QueryFirstOrDefault<Usuario>(
                sql,
                new { Id = id });
        }

        // 🔹 Agregar
        public void Agregar(Usuario usuario)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                INSERT INTO Usuarios
                (
                    Nombre,
                    Usuario,
                    Password,
                    Rol,
                    NombreNegocio,
                    NumeroCuenta,
                    WhatsApp
                )
                VALUES
                (
                    @Nombre,
                    @UsuarioNombre,
                    @Password,
                    @Rol,
                    @NombreNegocio,
                    @NumeroCuenta,
                    @WhatsApp
                )
            ";

            connection.Execute(sql, usuario);
        }

        // 🔹 Actualizar
        public void Actualizar(Usuario usuario)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE Usuarios
                SET
                    Nombre = @Nombre,
                    Usuario = @UsuarioNombre,
                    Password = @Password,
                    Rol = @Rol,
                    NombreNegocio = @NombreNegocio,
                    NumeroCuenta = @NumeroCuenta,
                    WhatsApp = @WhatsApp
                WHERE Id = @Id
            ";

            connection.Execute(sql, usuario);
        }

        // 🔹 Eliminar
        public void Eliminar(int id)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                DELETE FROM Usuarios
                WHERE Id = @Id
            ";

            connection.Execute(sql, new { Id = id });
        }

        // 🔹 Validar si usuario existe
        public bool ExisteUsuario(string usuarioNombre)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(*)
                FROM Usuarios
                WHERE Usuario = @Usuario
            ";

            var total = connection.ExecuteScalar<int>(
                sql,
                new { Usuario = usuarioNombre });

            return total > 0;
        }
    }
}