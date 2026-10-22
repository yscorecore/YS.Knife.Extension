using System.ComponentModel.DataAnnotations;
using YS.Knife;

namespace YS.Knife.Entity
{
    /// <summary>
    /// 具有状态字段的实体接口，状态类型必须为枚举。
    /// </summary>
    public interface IStatusEntity<TStatus>
          where TStatus : struct, Enum
    {
        TStatus Status { get; set; }
    }
    /// <summary>
    /// 带状态流转记录的实体接口，状态变化时把流转记录追加到 Flows。
    /// </summary>
    public interface IStatusFlowEntity<TStatus, TFlow> : IStatusEntity<TStatus>
        where TFlow : IStatusChangeFlowEntity<TStatus>
        where TStatus : struct, Enum
    {
        List<TFlow> Flows { get; set; }
    }
    /// <summary>
    /// 状态流转记录接口，From 为 null 表示初始状态。
    /// </summary>
    public interface IStatusChangeFlowEntity<T> : ISortableEntity
       where T : struct, Enum
    {
        public T? From { get; set; }
        public T To { get; set; }
        public DateTimeOffset OperationTime { get; set; }
        [StringLength(64)]
        public string Operator { get; set; }
    }
    public static class EnumStatusExtensions
    {
        public static void AppendInitStatusFlow<TStatus, TFlow>(this IStatusFlowEntity<TStatus, TFlow> obj, string operatorName, Action<TFlow> action = null)
            where TStatus : struct, Enum
            where TFlow : IStatusChangeFlowEntity<TStatus>, new()

        {
            obj.Flows = obj.Flows ?? new List<TFlow>();
            var flow = new TFlow
            {
                From = null,
                To = obj.Status,
                OperationTime = DateTimeOffset.Now,
                Operator = operatorName,
                Order = obj.Flows.Count + 1
            };
            action?.Invoke(flow);
            obj.Flows.Add(flow);
        }
        public static void AppendFlow<TStatus, TFlow>(this IStatusFlowEntity<TStatus, TFlow> obj, string operatorName, Action<TFlow> action = null)
          where TStatus : struct, Enum
          where TFlow : IStatusChangeFlowEntity<TStatus>, new()

        {
            obj.Flows = obj.Flows ?? new List<TFlow>();
            var flow = new TFlow
            {
                From = obj.Status,
                To = obj.Status,
                OperationTime = DateTimeOffset.Now,
                Operator = operatorName,
                Order = obj.Flows.Count + 1
            };
            action?.Invoke(flow);
            obj.Flows.Add(flow);
        }
        public static void UpdateStatusAndAppendFlow<TStatus, TFlow>(this IStatusFlowEntity<TStatus, TFlow> obj, TStatus newValue, string operatorName, Action<TFlow> action = null)
            where TStatus : struct, Enum
            where TFlow : IStatusChangeFlowEntity<TStatus>, new()
        {
            var oldValue = obj.Status;
            obj.CheckTransition(newValue);
            obj.Status = newValue;
            obj.Flows = obj.Flows ?? new List<TFlow>();
            var flow = new TFlow
            {
                From = oldValue,
                To = newValue,
                OperationTime = DateTimeOffset.Now,
                Operator = operatorName,
                Order = obj.Flows.Count + 1
            };
            action?.Invoke(flow);
            obj.Flows.Add(flow);
        }
        public static void CheckTransition<TStatus>(this IStatusEntity<TStatus> obj, TStatus newValue)
             where TStatus : struct, Enum
        {
            if (!FromStatusAttribute.IsValidTransition(obj.Status, newValue))
            {
                throw new InvalidOperationException("Invalid status transition from " + obj.Status.ToString() + " to " + newValue.ToString());
            }
        }

    }
}
