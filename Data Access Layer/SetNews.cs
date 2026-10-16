using AriaPrint.Models;
using AriaPrint.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AriaPrint.Data_Access_Layer
{
    public class SetNews
    {
        public async Task<List<News>> GetNewsAsync() 
        {
            var NewsList = new List<News>();

            var db = new ClassSQL();
            var Result = await db.SelectAsync<News>(@"
            SELECT TOP (1000) [Id]
            ,[Content]
            ,[IsNew]
            ,[PictureUrl]
            ,[Summary]
            ,[PageUrl]
            ,[Title]
            ,[Category]
            ,[Author]
            ,[ReleaseDate]
            ,[Documents] AS DocumentsJson
            ,[Conditions] AS ConditionsJson
            ,[deadline]
            FROM [AriaPrintDB].[dbo].[NewsTbl]");

            foreach (var item in Result)
            {
                NewsList.Add(item);
            }

            return NewsList;

        }
    }
}
