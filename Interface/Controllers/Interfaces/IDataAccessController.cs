using System.Collections.Generic;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Controllers.Interfaces
{
    public interface IDataAccessController<TEntity>
    {
        Task<ActionResult<IEnumerable<TEntity>>> GetAll();
        Task<ActionResult<TEntity>> GetById(string id);
        Task<ApiResponse> Update(TEntity model);
        Task<ApiResponse> Patch(TEntity model);
        Task<ApiResponse> Create(TEntity model);
        Task<ApiResponse> Delete(string id);
    }
}