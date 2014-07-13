using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Platform.PrimaryEntities.Factoring.FactoryElements;
using Platform.PrimaryEntities.Factoring.Strategies;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.PrimaryEntities.Factoring
{
    public  class Factory<TData> : IFactory 

    {


        public TObject ById<TObject>(int id) where TObject : Metadata
        {
            IFactoryElement<TObject> factoryElement = GetFactory<TObject>();
            if (factoryElement == null)
                throw new InvalidOperationException(
                    String.Format(
                        "Объект  не может быть получен по идентификатору. Передан неверный параметр типа объекта. Значение идентификатора: {0} . Запрашиваемый тип объекта: '{1}' ",
                        id, typeof(TObject)));
            return factoryElement.CreateById(id);
        }


        public TObject ByName<TObject>(string name) where TObject : Metadata
        {
            IFactoryElement<TObject> factoryElement = GetFactory<TObject>();
            if (factoryElement == null)
                throw new InvalidOperationException(String.Format("Объект  не может быть получен по имени. Передан неверный параметр типа объекта. Имя объекта: {0} . Запрашиваемый тип объекта: '{1}' ", name, typeof(TObject)));
            return factoryElement.CreateByName(name);

        }

        public IList<TObject> ChildsById<TObject, TParent>(int parentId) where TObject : Metadata
        {
            IBaseFactoryElement<IList<TObject>> factoryElement = GetChildFactory<TObject, TParent>();
            if (factoryElement == null)
                throw new InvalidOperationException(String.Format("Список дочерних объектов не может быть получен по идентификатору родителя. Передан неверный параметр типа объекта. Значение идентификатора родителя: {0}  . Запрашиваемый тип объекта: '{1}' ", parentId, typeof(TObject)));
            return factoryElement.CreateById(parentId);

        }

        public TObject Create<TObject>() where TObject : Metadata
        {
            IFactoryElement<TObject> factoryElement = GetFactory<TObject>();
            if (factoryElement == null)
                throw new InvalidOperationException(String.Format("Объект  не может создан. Передан неверный параметр типа объекта.  Запрашиваемый тип объекта: '{0}' ", typeof(TObject)));
            return factoryElement.CreateObject();
        }


        public virtual IFactoryStrategy<TData> FactoryStrategy { get; set; }




        protected virtual IBaseFactoryElement<IList<TObject>> GetChildFactory<TObject, TParent>() where TObject : Metadata
        {
            if (IsParent<TObject, TParent>())
                return DoGetFactoryOfChild<TObject, TParent>();
            return null;
        }

        protected virtual IBaseFactoryElement<IList<TObject>> DoGetFactoryOfChild<TObject, TParent>() where TObject : Metadata
        {
            return new OfChilds<TObject, TParent, TData>(FactoryStrategy);
        }

        protected virtual IFactoryElement<TObject> GetFactory<TObject>() where TObject : Metadata
        {
            return new OfSingle<TObject, TData>(FactoryStrategy);
        }

        protected virtual bool IsParent<TChild, TParent>()
        {
            if ((typeof(Entity).IsAssignableFrom(typeof(TParent))) && (typeof(EntityField).IsAssignableFrom(typeof(TChild))))
                return true;
            if ((typeof(Form).IsAssignableFrom(typeof(TParent))) && (typeof(FormElement).IsAssignableFrom(typeof(TChild))))
                return true;

            return false;
        }



    }



    public class Factory : Factory<DataRow>
    {
        public Factory(SqlConnection dbConnection)
        {
            FactoryStrategy = new Default(dbConnection);
        }

        public Factory()
        {
            
        }

    }

}
