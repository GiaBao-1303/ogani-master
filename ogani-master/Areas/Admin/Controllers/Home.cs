﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogani_master.Models;



namespace ogani_master.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class Home(OganiMaterContext _context) : Controller
    {
        private readonly OganiMaterContext context = _context;

        private DatabaseSizeInfo GetDatabaseSize()
        {
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT name FROM sys.tables";
            context.Database.OpenConnection();

            List<DatabaseSizeViewModel> tableSizes = new List<DatabaseSizeViewModel>();
            double totalSize = 0;

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = reader["name"].ToString();
                        var spaceCommand = context.Database.GetDbConnection().CreateCommand();
                        spaceCommand.CommandText = $"EXEC sp_spaceused '{tableName}'";

                        using (var spaceReader = spaceCommand.ExecuteReader())
                        {
                            if (spaceReader.Read())
                            {
                                var reservedSize = spaceReader["reserved"].ToString();

                                string capacity = "0";
                                if (!string.IsNullOrEmpty(reservedSize))
                                {
                                    var sizeParts = reservedSize.Split(' ');
                                    if (sizeParts.Length == 2 && double.TryParse(sizeParts[0], out double sizeValue))
                                    {
                                        string unit = sizeParts[1].ToUpper();
                                        switch (unit)
                                        {
                                            case "KB":
                                                capacity = (sizeValue / 1024 / 1024).ToString();
                                                break;
                                            case "MB":
                                                capacity = (sizeValue / 1024).ToString();
                                                break;
                                            case "GB":
                                                capacity = sizeValue.ToString();
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }

                                double capacityDouble = double.Parse(capacity);

                                totalSize += capacityDouble;

                                tableSizes.Add(new DatabaseSizeViewModel
                                {
                                    TableName = tableName,
                                    Capacity = capacityDouble.ToString("F2")
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                context.Database.CloseConnection();
            }

            DatabaseSizeInfo databaseSizeInfo = new DatabaseSizeInfo
            {
                Capacity = totalSize.ToString("F2"),
                tableSizes = tableSizes
            };

            return databaseSizeInfo;
        }

        private List<UserBehaviorSummary> GetUserBehavior()
        {
            DateTime now = DateTime.UtcNow;
            DateTime past24Hours = now.AddHours(-24);

          
            var data = _context.UserBehaviorLogs
                .Where(log => log.Timestamp >= past24Hours && log.Timestamp <= now)
                .GroupBy(log => log.Timestamp.Hour)
                .Select(group => new UserBehaviorSummary
                {
                    Hour = group.Key,         
                    Count = group.Count()     
                })
                .OrderBy(g => g.Hour)       
                .ToList();

            return data;
        }

        public IActionResult Index()
        {
            DatabaseSizeInfo databaseSizeInfo = this.GetDatabaseSize();
            List<UserBehaviorSummary> userBehaviorSummaries = GetUserBehavior();

            ViewBag.userBehaviorSummaries = userBehaviorSummaries;
            ViewBag.databaseSizeInfo = databaseSizeInfo;

            return View(databaseSizeInfo);
        }
    }
}