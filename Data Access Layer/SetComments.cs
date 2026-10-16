using AriaPrint.Repository;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AriaPrint.Data_Access_Layer
{
    public class SetComments
    {
        List<Comment> CommentsList = new();
        ClassSQL db = new();
        public async Task<List<Comment>> GetCommentsAsync()
        {
            var Result = await db.SelectAsync<Comment>(@"
            SELECT TOP (1000) [FullName]
            ,[PhoneNumber]
            ,[Text]
            ,[NewsId]
            ,[IsDelete]
            ,[DeleteTime]
            ,[IP]
            ,[Admin]
            FROM [AriaPrintDB].[dbo].[CommentsTbl]");

            foreach (var item in Result)
            {
                CommentsList.Add(item);
            }

            return CommentsList;
        }
        public async Task<int> AddCommentAsync(Comment comment, string? ip)
        {
            var insertData = new
            {
                comment.FullName,
                comment.PhoneNumber,
                comment.Text,
                comment.NewsId,
                IsDelete = false,
                IP = ip,
                Admin = (string?)null
            };

            return await db.InsertDynamicAsync("CommentsTbl", insertData);
        }
    }
}
