using System.ComponentModel;

namespace CelHost.Models.Enums
{
    public enum NodeStatus
    {
        /// <summary>
        /// The node is active and available for processing requests.
        /// </summary>
        [Description("正常")]
        Active,
        /// <summary>
        /// The node is temporarily unavailable for processing requests.
        /// </summary>
        [Description("不可用")]
        Inactive,
        /// <summary>
        /// The node is in an error state and cannot be used for processing requests.
        /// </summary>
        [Description("错误")]
        Error
    }
}
