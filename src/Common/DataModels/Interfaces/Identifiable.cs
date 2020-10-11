using System;

namespace Common.DataModels
{
    public interface Identifiable
    {
        Guid Id { get; }
    }
}
