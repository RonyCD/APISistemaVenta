using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DAL.Repositorios
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {

        private readonly DbventaContext _dbContext;

        public VentaRepository(DbventaContext dbContext)  :base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Venta> Registrar(Venta modelo)
        {
            Venta ventaGenerada = new Venta();

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    //Restar el stock de cada producto de la venta mediante la tabla VentaDetalle
                    foreach (DetalleVenta dv in modelo.DetalleVenta) {
                        
                        Producto productoEncontrado = _dbContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();
                        productoEncontrado.Stock = productoEncontrado.Stock - dv.Cantidad;
                        _dbContext.Productos.Update(productoEncontrado);
                    }

                    await _dbContext.SaveChangesAsync();

                    //Generar un numero de documento de venta
                    NumeroDocumento correlativo = _dbContext.NumeroDocumentos.First();
                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    
                    //Actualizar la fecha
                    correlativo.FechaRegistro = DateTime.Now;

                    //Actualizar la información
                    _dbContext.NumeroDocumentos.Update(correlativo);
                    await _dbContext.SaveChangesAsync();

                    //Genrar el formato de numero de documento de la venta
                    int cantidadDigitos = 4;
                    string ceros = string.Concat(Enumerable.Repeat("0", cantidadDigitos));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();   //Este seria el formato 00001

                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - cantidadDigitos, cantidadDigitos);


                    //Actualizar el numero de documento de la venta
                    modelo.NumeroDocumento = numeroVenta;
                    await _dbContext.Venta.AddAsync(modelo);
                    await _dbContext.SaveChangesAsync();

                    //llamar a la variable para pasarle el modelo
                    ventaGenerada = modelo;

                    //Finalizar la transaccion sin problema
                    transaction.Commit();


                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                return ventaGenerada;

            }

        }
    }
}
