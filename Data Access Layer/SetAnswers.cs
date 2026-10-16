using AriaPrint.Repository;

namespace AriaPrint.Data_Access_Layer
{
    public class SetAnswers
    {
        ClassSQL db = new();
        public async Task<List<Answer>> GetAnswersAsync(int commentId)
        {
            var Result = await db.SelectAsync<Answer>(@"
            SELECT TOP (1000) [AnswerId]
            ,[Date]
            ,[Text]
            ,[IsDelete]
            ,[DeleteTime]
            ,[IP]
            ,[Admin]
            FROM [AriaPrintDB].[dbo].[AnswersTbl]
            WHERE [AnswerId] = @CommentId AND (IsDelete = 0 OR IsDelete IS NULL)",
            new { CommentId = commentId });

            return Result.ToList();
        }
        public async Task<int> AddAnswerAsync(Answer answer, string? ip)
        {
            var insertData = new
            {
                answer.AnswerId,
                answer.Date,
                answer.Text,
                IP = ip,
                Admin = (string?)null
            };

            return await db.InsertDynamicAsync("AnswersTbl", insertData);
        }
    }
}
