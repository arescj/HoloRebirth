using System;

namespace Ion.Storage
{
    public interface IDataObject
    {
        void INSERT(DatabaseClient dbClient);
        void DELETE(DatabaseClient dbClient);
        void UPDATE(DatabaseClient dbClient);
    }
}
