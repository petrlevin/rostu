using System;
using Platform.Common;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring
{
    /// <summary>
    /// Фабрика для создания базовых объектов
    /// не использует кэш 
    /// не использует DJ
    /// </summary>
    public static class Objects
    {
        /// <summary>
        /// Получает объект из базы данных по его идентификатору 
        /// </summary>
        /// <typeparam name="TObject">тип объекта</typeparam>
        /// <param name="id">идентификатор</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static public TObject ById<TObject>(int id) where TObject : Metadata
        {
            
            return GetFactory().ById<TObject>(id);
            
        }

        static public TObject ByName<TObject>(string name) where TObject : Metadata
        {

            return GetFactory().ByName<TObject>(name);

        }

        static public TObject Create<TObject>() where TObject : Metadata
        {

            return GetFactory().Create<TObject>();

        }






        static internal IFactory GetFactory()
        {
            return IoC.Resolve<IFactory>("MetadataObjectsFactory");
            //return new Platform.PrimaryEntities.Factory(IoC.Resolve<SqlConnection>("DbConnection"));
        }

    }
}
