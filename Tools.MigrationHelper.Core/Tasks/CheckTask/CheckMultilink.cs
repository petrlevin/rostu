using System.Linq;
using System.Text;
using BaseApp.Reference;
using Sbor;
using Sbor.Reference;

namespace Tools.MigrationHelper.Core.Tasks.CheckTask
{
    public class CheckMultilink
    {

        public CheckMultilink(string connectionString)
        {
            _context = new DataContext(connectionString);
        }

        public void Check()
        {
            var error = false;
            var sBuilder = new StringBuilder();

            foreach (var activity in _context.Activity)
            {
                if(error) break;

                var mapList = activity.Activity_Contingent.ToList();

                var queryList =
                    _context.Database.SqlQuery<Contingent>(
                        "SELECT * FROM ref.Contingent WHERE id IN (SELECT idContingent FROM ml.Activity_Contingent WHERE idActivity = " +
                        activity.Id + ")").ToList();

                if (mapList.Count != queryList.Count)
                    error = true;
            }

            if (error) sBuilder.Append("Ошибка мультилинка со стороны Activity");

            error = false;
            foreach (var contingent in _context.Contingent)
            {
                if (error) break;

                var mapList = contingent.Activity_Contingent.ToList();

                var queryList =
                    _context.Database.SqlQuery<Activity>(
                        "SELECT * FROM ref.Activity WHERE id IN (SELECT idActivity FROM ml.Activity_Contingent WHERE idContingent = " + contingent.Id + ")").ToList();

                if (mapList.Count != queryList.Count)
                    error = true;
            }

            if (error) sBuilder.Append("Ошибка мультилинка со стороны Contingent");

            error = false;
            foreach (var user in _context.User)
            {
                if (error) break;

                var mapList = user.Roles.ToList();

                var queryList =
                    _context.Database.SqlQuery<Role>(
                        "SELECT * FROM ref.[Role] WHERE id IN (SELECT idRole FROM ml.UserRole WHERE idUser = " + user.Id + ")").ToList();

                if (mapList.Count != queryList.Count)
                    error = true;
            }

            if (error) sBuilder.Append("Ошибка мультилинка со стороны User");

            error = false;
            foreach (var role in _context.Role)
            {
                if (error) break;

                var mapList = role.Users.ToList();

                var queryList =
                    _context.Database.SqlQuery<User>(
                        "SELECT * FROM ref.[User] WHERE id IN (SELECT idUser FROM ml.UserRole WHERE idRole = " + role.Id + ")").ToList();

                if (mapList.Count != queryList.Count)
                    error = true;
            }

            if (error) sBuilder.Append("Ошибка мультилинка со стороны Role");

            error = false;
            foreach (var cc in _context.CategoryContingent)
            {
                if (error) break;

                var mapList = cc.Contingent_CategoryContingent.ToList();

                var queryList =
                    _context.Database.SqlQuery<Contingent>(
                        "SELECT * FROM ref.[Contingent] WHERE id IN (SELECT idContingent FROM ml.Contingent_CategoryContingent WHERE idCategoryContingent = " + cc.Id + ")").ToList();

                if (mapList.Count != queryList.Count)
                    error = true;
            }

            if (error) sBuilder.Append("Ошибка мультилинка со стороны Contingent2");

            error = false;
            foreach (var contingent in _context.Contingent)
            {
                if (error) break;

                var mapList = contingent.Contingent_CategoryContingent.ToList();

                var queryList =
                    _context.Database.SqlQuery<CategoryContingent>(
                        "SELECT * FROM ref.[CategoryContingent] WHERE id IN (SELECT idCategoryContingent FROM ml.Contingent_CategoryContingent WHERE idContingent = " + contingent.Id + ")").ToList();

                if (mapList.Count != queryList.Count)
                    error = true;
            }

            if (error) sBuilder.Append("Ошибка мультилинка со стороны CategoryContingent");

            var mess = sBuilder.ToString();
        }


        private readonly DataContext _context;
    }
}
