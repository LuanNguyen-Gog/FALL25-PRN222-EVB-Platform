using Repositories.Basic;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class UserRepository : GenericRepo<User>
    {
        public UserRepository(DBContext.EVBatteryTradingContext context) : base(context)
        {
        }
        public IQueryable<User> GetFiltered(User user)
        {
            var query = _context.Users.AsQueryable();
            if (user.UserId > 0)// Filter by UserId if it's greater than 0
                //AsQueryable() thêm vào để có công thức query động, và khi chưa biết cần điều kiện nào để query
                //Điều kiện nào cần thì thêm vào sau, không query lên danh sách ngay
                query = query.Where(u => u.UserId == user.UserId);
            if (!string.IsNullOrEmpty(user.Name)) // Filter by Name if it's not null or empty
                query = query.Where(u => u.Name.Contains(user.Name));
            if (!string.IsNullOrEmpty(user.Email)) // Filter by Email if it's not null or empty
                query = query.Where(u => u.Email.Contains(user.Email));
            if (!string.IsNullOrEmpty(user.Phone)) // Filter by Phone if it's not null or empty
                query = query.Where(u => u.Phone.Contains(user.Phone));
            if (!string.IsNullOrEmpty(user.Role)) // Filter by Role if it's not null or empty
                query = query.Where(u => u.Role == user.Role);
            if (!string.IsNullOrEmpty(user.Status)) // Filter by Status if it's not null or empty
                query = query.Where(u => u.Status == user.Status);
            return query.OrderBy(u => u.UserId);
            //cần phải .ToList() ở service để thực thi câu query
        }
    }
}
