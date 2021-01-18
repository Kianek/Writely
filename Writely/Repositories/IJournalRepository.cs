using System.Collections.Generic;
using System.Threading.Tasks;
using Writely.Models;
using Writely.Services;

namespace Writely.Repositories
{
    public interface IJournalRepository
    {
        Task<Journal> GetById(long id);
        Task<List<Journal>> GetAll(int limit = 0, string orderBy = "date-desc");
        Task<Journal> Save(Journal journal);
        Task<Journal> Update(Journal updatedJournal);
        Task<bool> Delete(long id);
    }
}