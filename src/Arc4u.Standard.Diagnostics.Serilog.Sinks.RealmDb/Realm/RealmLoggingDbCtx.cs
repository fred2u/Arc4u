using AutoMapper;
using Microsoft.Extensions.Logging;
using Realms;

namespace Arc4u.Diagnostics.Serilog.Sinks.RealmDb;

public class RealmLoggingDbCtx : ILogStore
{
    public RealmLoggingDbCtx(RealmConfiguration config)
    {
        _realm = Realm.GetInstance(config);
    }

    public void RemoveAll()
    {
        _realm.Write(_realm.RemoveAll<LogDBMessage>);
    }

    public List<LogMessage> GetLogs(string criteria, int skip, int take)
    {
        var hasCriteria = !string.IsNullOrWhiteSpace(criteria);
        var searchText = hasCriteria ? criteria.ToLowerInvariant() : "";

        var queryable = _realm.All<LogDBMessage>().OrderByDescending(msg => msg.Timestamp);

        var enumerator = queryable.GetEnumerator();

        var i = 0;
        while (i < skip && enumerator.MoveNext())
        {
            i++;
        }

        var result = new List<LogDBMessage>(take);
        i = 0;

        while (i < take && enumerator.MoveNext())
        {
            if (hasCriteria && enumerator.Current.Message.ToLowerInvariant().Contains(searchText))
            {
                result.Add(enumerator.Current);
            }

            if (!hasCriteria)
            {
                result.Add(enumerator.Current);
            }

            i++;
        }

        return Mapper.Map<List<LogMessage>>(result);
    }

    private readonly Realm _realm;
    public Realm Realm { get { return _realm; } }

    private static readonly IMapper _mapper = CreateMapping();
    public static IMapper Mapper { get { return _mapper; } }

    public static IMapper CreateMapping()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // cfg.AddCollectionMappers();
            cfg.CreateMap<LogDBMessage, LogMessage>()
               .ForMember(dest => dest.MessageCategory, opts => opts.MapFrom(src => ((MessageCategory)src.MessageCategory).ToString()))
               .ForMember(dest => dest.MessageType, opts => opts.MapFrom(src => ((LogLevel)src.MessageType).ToString()));
        });

        return config.CreateMapper();

    }
}
