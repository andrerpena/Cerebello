using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using System.Reflection;

namespace CerebelloWebRole.Code.Mvc
{
    public static class EntityObjectExtensions
    {
        [Flags]
        public enum CollectionUpdateStrategy
        {
            Create = 1,
            Update = 2
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityCollection">Coleção do objeto persistente atual à qual se deseja mesclar os objetos que vieram do ViewModel.</param>
        /// <param name="viewModelCollection">Coleção de objetos que vieram do ViewModel.</param>
        /// <param name="compare">Função comparadora. Exemplo: (telefoneModel, telefoneViewModel) => telefoneModel.Id == telefoneViewModel.Id.</param>
        /// <param name="update"> </param>
        /// <param name="delete"> </param>
        /// <param name="updateStrategy"> </param>
        [Obsolete("This logic is to complicated and error prone to be encapsulated. Just do it manually")]
        public static void Update<TModel, TViewModel>(
            this EntityCollection<TModel> entityCollection,
            IEnumerable<TViewModel> viewModelCollection,
            Func<TViewModel, TModel, bool> compare,
            Action<TViewModel, TModel> update,
            Action<TModel> delete,
            CollectionUpdateStrategy updateStrategy = CollectionUpdateStrategy.Create | CollectionUpdateStrategy.Update)
            where TModel : EntityObject
            where TViewModel : class
        {
            var harakiriQuerue = new Queue<TModel>();

            // varre todos os elementos atuais do modelo verificando quais alteraram e quais foram deletados
            foreach (TModel modelObject in entityCollection)
            {
                // verifico se existe view-model corresponde com este objeto
                var matchingViewModel = viewModelCollection.FirstOrDefault(viewModelObject => compare(viewModelObject, modelObject));
                if (matchingViewModel != null)
                {
                    if ((updateStrategy & CollectionUpdateStrategy.Update) == CollectionUpdateStrategy.Update)
                    {
                        var templateModelObject = ShallowCloneEntityObject(modelObject);
                        update(matchingViewModel, templateModelObject);

                        // comparo cada propriedade de correspondingViewModel para saber se eu devo setar esta propriedade em modelObject
                        foreach (var property in templateModelObject.GetType().GetProperties())
                        {
                            var scalarPropertiAttributes = property.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).Cast<EdmScalarPropertyAttribute>();
                            if (scalarPropertiAttributes.Any() && !scalarPropertiAttributes.ElementAt(0).EntityKeyProperty)
                            {
                                var oldValue = property.GetValue(modelObject, null);
                                var newValue = property.GetValue(templateModelObject, null);

                                if (!Object.Equals(oldValue, newValue))
                                    property.SetValue(modelObject, newValue, null);
                            }
                        }
                    }
                }
                else
                    harakiriQuerue.Enqueue(modelObject);
            }

            // deleto os elementos do modelo que não possuem correspondência com o view-model
            while (harakiriQuerue.Any())
                delete(harakiriQuerue.Dequeue());

            if ((updateStrategy & CollectionUpdateStrategy.Create) == CollectionUpdateStrategy.Create)
                foreach (var notMatchingViewModel in viewModelCollection.Where(vm => !entityCollection.Any(m => compare(vm, m))))
                {
                    var newModelObject = Activator.CreateInstance<TModel>();
                    update(notMatchingViewModel, newModelObject);
                    entityCollection.Add(newModelObject);
                }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TViewModel"> </typeparam>
        /// <param name="entityCollection">Coleção do objeto persistente atual à qual se deseja mesclar os objetos que vieram do ViewModel</param>
        /// <param name="viewModelCollection">Coleção de objetos que vieram do ViewModel</param>
        /// <param name="compare">Função comparadora. Exemplo: (telefoneModel, telefoneViewModel) => telefoneModel.Id == telefoneViewModel.Id</param>
        /// <param name="getModel"> </param>
        /// <param name="onDeleteAction"> </param>
        [Obsolete("This logic is to complicated and error prone to be encapsulated. Just do it by hand")]
        public static void UpdateManyToMany<TModel, TViewModel>(
            this EntityCollection<TModel> entityCollection,
            IEnumerable<TViewModel> viewModelCollection,
            Func<TViewModel, TModel, bool> compare,
            Func<TViewModel, TModel> getModel,
            Action<TModel> onDeleteAction)
            where TModel : EntityObject
            where TViewModel : class
        {
            var harakiriQuerue = new Queue<TModel>();

            // varre todos os elementos atuais do modelo verificando quais alteraram e quais foram deletados
            foreach (TModel modelObject in entityCollection)
            {
                // verifico se existe view-model corresponde com este objeto
                var matchingViewModel = viewModelCollection.FirstOrDefault(viewModelObject => compare(viewModelObject, modelObject));
                if (matchingViewModel == null)
                    harakiriQuerue.Enqueue(modelObject);
            }

            // deleto os elementos do modelo que não possuem correspondência com o view-model
            while (harakiriQuerue.Any())
                onDeleteAction(harakiriQuerue.Dequeue());

            foreach (var notMatchingViewModel in viewModelCollection.Where(vm => !entityCollection.Any(m => compare(vm, m))))
                entityCollection.Add(getModel(notMatchingViewModel));
        }

        public static TModel ShallowCloneEntityObject<TModel>(TModel source) where TModel : EntityObject
        {
            TModel clone = Activator.CreateInstance<TModel>();

            foreach (var property in source.GetType().GetProperties())
            {
                var scalarPropertiAttributes = property.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).Cast<EdmScalarPropertyAttribute>();
                if (scalarPropertiAttributes.Any() && !scalarPropertiAttributes.ElementAt(0).EntityKeyProperty)
                {
                    property.SetValue(clone, property.GetValue(source, null), null);
                }
            }

            return clone;
        }
    }
}