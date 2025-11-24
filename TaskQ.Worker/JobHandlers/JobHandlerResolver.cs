using System.Reflection;
using TaskQ.Core.Attributes;

namespace TaskQ.Worker.JobHandlers
{
    public interface IJobHandlerResolver
    {
        IJobHandler Resolve(string jobType);
    }
    public class JobHandlerResolver : IJobHandlerResolver
    {
        private readonly Dictionary<string, IJobHandler> _handlers;

        public JobHandlerResolver(IEnumerable<IJobHandler> handlers)
        {
            _handlers = new Dictionary<string, IJobHandler>(StringComparer.OrdinalIgnoreCase);

            foreach (var handler in handlers)
            {
                var type = handler.GetType();
                var attr = type.GetCustomAttribute<BackgroundJobAttribute>();

                if (attr == null)
                    throw new InvalidOperationException(
                        $"El handler {type.Name} no tiene el atributo [BackgroundJob].");

                if (_handlers.ContainsKey(attr.Name))
                    throw new InvalidOperationException(
                        $"Ya existe un handler registrado para el job '{attr.Name}'.");

                _handlers[attr.Name] = handler;
            }
        }

        public IJobHandler Resolve(string jobType)
        {
            if (!_handlers.TryGetValue(jobType, out var handler))
                throw new InvalidOperationException($"No existe handler registrado para el tipo de job '{jobType}'.");

            return handler;
        }
    }
}
