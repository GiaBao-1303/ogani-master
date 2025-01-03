using ogani_master.enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ogani_master.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetType()
                .GetField(enumValue.ToString())
                ?.GetCustomAttribute<DisplayAttribute>();

            return attribute?.Name ?? enumValue.ToString();
        }

        public static OrderStatus GetOrderStatus(int status)
        {
            return (OrderStatus)status;
        }

        public static bool IsFinalStatus(OrderStatus status)
        {
            return status == OrderStatus.Delivered || status == OrderStatus.Canceled || status == OrderStatus.Returned;
        }

        private static readonly Dictionary<OrderStatus, string> StatusColors = new()
        {
            { OrderStatus.Pending, "text-warning" },        
            { OrderStatus.Confirmed, "text-primary" },     
            { OrderStatus.Preparing, "text-info" },       
            { OrderStatus.Shipping, "text-success" },     
            { OrderStatus.Delivered, "text-muted" },        
            { OrderStatus.Canceled, "text-danger" },        
            { OrderStatus.Returned, "text-secondary" }   
        };

        public static T? GetEnumFromDisplayName<T>(string displayName) where T : struct, Enum
        {
            return typeof(T).GetFields()
                            .Where(f => f.IsStatic)
                            .FirstOrDefault(f => f.GetCustomAttributes<DisplayAttribute>()
                                                  .FirstOrDefault()?.Name == displayName)?
                            .GetValue(null) as T?;
        }

        public static string GetCssClass(this OrderStatus status)
        {
            return StatusColors.TryGetValue(status, out var cssClass) ? cssClass : "text-default";
        }
    }
}
