using System.Threading;
using System.Threading.Tasks;

namespace Si.DomainToolkit.Application.CQRS
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
    }

    /// <summary>
    /// 带返回值的命令接口
    /// </summary>
    public interface ICommand<TResult>
    {
    }

    /// <summary>
    /// 命令处理器接口
    /// </summary>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task Handle(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 带返回值的命令处理器接口
    /// </summary>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }
} 