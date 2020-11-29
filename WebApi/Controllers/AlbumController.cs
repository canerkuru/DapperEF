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
    public class AlbumController : ControllerBase
    {
        public AlbumController(IApplicationDbContext dbContext, IApplicationReadDbConnection readDbConnection, IApplicationWriteDbConnection writeDbConnection)
        {
            _dbContext = dbContext;
            _readDbConnection = readDbConnection;
            _writeDbConnection = writeDbConnection;
        }

        public IApplicationDbContext _dbContext { get; }
        public IApplicationReadDbConnection _readDbConnection { get; }
        public IApplicationWriteDbConnection _writeDbConnection { get; }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = $"select * from albums";
            var data = await _readDbConnection.QueryAsync<Album>(query);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(AlbumDto entityDto)
        {
            _dbContext.Connection.Open();
            using (var transaction = _dbContext.Connection.BeginTransaction())
            {
                try
                {
                    _dbContext.Database.UseTransaction(transaction as DbTransaction);
                    bool GenresExists = await _dbContext.Albums.AnyAsync(a => a.Title == entityDto.Title);
                    if (GenresExists) return BadRequest(new { msg = "Album Already Exists" });


                    var addGenresQuery = $"INSERT INTO albums(Title,ArtistId) VALUES('{entityDto.Title}','{entityDto.ArtistId}');SELECT last_insert_rowid()";
                    var genresId = await _writeDbConnection.QuerySingleAsync<int>(addGenresQuery, transaction: transaction);

                    if (genresId == 0) return BadRequest(new { msg = "Album could not be inserted" });

                    await _dbContext.SaveChangesAsync(default);
                    transaction.Commit();
                    return Ok(new { id = genresId });
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



        [HttpPost("withartist")]
        public async Task<IActionResult> AddNewWithArtist(AlbumArtistDto entityDto)
        {
            _dbContext.Connection.Open();
            using (var transaction = _dbContext.Connection.BeginTransaction())
            {
                try
                {
                    _dbContext.Database.UseTransaction(transaction as DbTransaction);
                    bool dataExists = await _dbContext.Albums.AnyAsync(a => a.Title == entityDto.TitleAlbum);
                    if (dataExists) return BadRequest(new { msg = "Album Already Exists" });

                    var artist = await _dbContext.Artists.Where(a => a.Name == entityDto.ArtistName).ToListAsync();
                    int artistId;
                    if (artist.Count == 0)
                    {
                        var addArtistQuery = $"INSERT INTO artists(Name) VALUES('{entityDto.ArtistName}');SELECT last_insert_rowid()";
                        artistId = await _writeDbConnection.QuerySingleAsync<int>(addArtistQuery, transaction: transaction);
                    }
                    else
                    {
                        artistId = artist[0].ArtistId;
                    }


                    var addAlbumQuery = $"INSERT INTO albums(Title,ArtistId) VALUES('{entityDto.TitleAlbum}',{artistId});SELECT last_insert_rowid()";
                    var dataId = await _writeDbConnection.QuerySingleAsync<int>(addAlbumQuery, transaction: transaction);

                    if (dataId == 0) return BadRequest(new { msg = "Album could not be inserted" });

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
