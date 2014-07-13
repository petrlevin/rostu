using System;

namespace Platform.Environment.Interfaces
{
    public interface IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> :
        IStorageContainer<TApplicationStorage, TSessionStorage, TRequestStorage>
        where TRequestStorage:IRequestStorageBase
    {
        /// <summary>
        /// Действия, выполняемые при старте веб-приложения
        /// </summary>
        /// <param name="appStor">Объект хранилища уровня приложения</param>
        /// <returns>Объект среды (для возможности fluent-синтаксиса)</returns>
        IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> ApplicationStart(TApplicationStorage appStor);
        IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> SessionStart(TSessionStorage sessionStor);
        IEnvironment<TApplicationStorage, TSessionStorage, TRequestStorage> RequestStart(TRequestStorage reqStor);
    }
}
