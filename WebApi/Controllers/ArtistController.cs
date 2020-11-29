using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {

        public ArtistController(IApplicationDbContext dbContext, IApplicationReadDbConnection readDbConnection, IApplicationWriteDbConnection writeDbConnection)
        {
            _dbContext = dbContext;
            _readDbConnection = readDbConnection;
            _writeDbConnection = writeDbConnection;
        }

        public IApplicationDbContext _dbContext { get; }
        public IApplicationReadDbConnection _readDbConnection { get; }
        public IApplicationWriteDbConnection _writeDbConnection { get; }


        public async Task<IActionResult> AddNew(ArtistDto entityDto)
        {
            _dbContext.Connection.Open();
            using (var transaction = _dbContext.Connection.BeginTransaction())
            {
                try
                {
                    _dbContext.Database.UseTransaction(transaction as DbTransaction);
                    bool GenresExists = await _dbContext.Artists.AnyAsync(a => a.Name == entityDto.Name);
                    if (GenresExists) return BadRequest(new { msg = "Artist Already Exists" });


                    var addGenresQuery = $"INSERT INTO artists(Name) VALUES('{entityDto.Name}');SELECT last_insert_rowid()";
                    var dataId = await _writeDbConnection.QuerySingleAsync<int>(addGenresQuery, transaction: transaction);

                    if (dataId == 0) return BadRequest(new { msg = "Artist could not be inserted" });

                    await _dbContext.SaveChangesAsync(default);
                    transaction.Commit();
                    return Ok(new { id = dataId });
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { msg = "unexpected error occurred." + exp.Message });
                }
                finally
                {
                    _dbContext.Connection.Close();
                }
            }
        }

    }
}
