using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsuarioServicio.Dominio.Entidades;
using UsuarioServicio.Dominio.Interfaces;
using UsuarioServicio.Infraestructura.Persistencia;

namespace UsuarioServicio.Infraestructura.Services
{
    public class MovimientoUsuarioService : IMovimientoUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public MovimientoUsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarMovimientoAsync(Guid usuarioId, string accion, string? detalles = null, CancellationToken cancellationToken = default)
        {
            var movimiento = new MovimientoUsuario
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Detalles = detalles
            };

            _context.MovimientosUsuario.Add(movimiento);
            await _context.SaveChangesAsync(cancellationToken);
        }

        

    }


}