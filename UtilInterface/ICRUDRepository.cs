namespace UtilInterface
{
    public interface ICRUDRepositorio<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Lista todfa la tabla
        /// </summary>
        /// <returns>resultado tabla</returns>
        Task<List<TEntity>> GetAllAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Valor del PK</param>
        /// <returns>retorna el registro de la tabla por ID</returns>
        Task<TEntity?> GetByIdAsync(object id);
        /// <summary>
        /// Inserta un nuevo registro
        /// </summary>
        /// <param name="entity">Registro a insertar</param>
        /// <returns>Retorna el registro insertado</returns>
        Task<TEntity> CreateAsync(TEntity entity);
        /// <summary>
        /// Actualiza un nuevo registro
        /// </summary>
        /// <param name="entity">Registro a Actualiza</param>
        /// <returns>retorna el registro Actualiza</returns>
        Task<TEntity> UpdateAsync(TEntity entity);
        /// <summary>
        /// elimina un registro
        /// </summary>
        /// <param name="id">Valor del PK</param>
        /// <returns>Cantidad de registros afectados</returns>
        Task DeleteAsync(object id);
        /// <summary>
        /// Elimina una lista de registros
        /// </summary>
        /// <param name="lista">lista de registros a eliminar</param>
        /// <returns>retorna la cantidad de registros a eliminar</returns>
        Task DeleteMultipleItemsAsync(List<TEntity> id);
        /// <summary>
        /// Inserta una lista de registros
        /// </summary>
        /// <param name="lista">lista de registros a instertar</param>
        /// <returns>retorna la cantidad de registros a instertar</returns>
        Task<List<TEntity>> InsertMultipleAsync(List<TEntity> lista);
        /// <summary>
        /// actualiza un lista de registrps
        /// </summary>
        /// <param name="lista">lista de registros a actualiza</param>
        /// <returns>lista de registros actualizados</returns>
        Task<List<TEntity>> UpdateMultipleAsync(List<TEntity> lista);
        /// <summary>
        /// retora una lista de registros por coincidencia
        /// </summary>
        /// <param name="query">valor a buscar</param>
        /// <returns>lista de registros que coinciden</returns>
        Task<List<TEntity>> GetAutoCompleteAsync(string query);
        //GenericFilterResponse<T> GetByFilter(GenericFilterRequest filter);
    }
}
