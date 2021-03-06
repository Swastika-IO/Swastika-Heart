// Licensed to the Siocore Foundation under one or more agreements.
// The Siocore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sio.Common.Helper;
using Sio.Domain.Core.Models;
using Sio.Domain.Core.ViewModels;
using Sio.Domain.Data.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sio.Domain.Data.ViewModels
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    public abstract class ViewModelBase<TDbContext, TModel, TView> : IViewModel
        where TDbContext : DbContext
        where TModel : class
        where TView : ViewModelBase<TDbContext, TModel, TView> // instance of inherited
    {
        #region Properties

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        private bool isValid = true;

        /// <summary>
        /// The mapper
        /// </summary>
        private IMapper _mapper;

        /// <summary>
        /// The model
        /// </summary>
        private TModel _model;

        /// <summary>
        /// The model mapper
        /// </summary>
        private IMapper _modelMapper;

        [JsonIgnore]
        public static readonly DefaultRepository<TDbContext, TModel, TView> Repository;

        [JsonIgnore]
        public static readonly DefaultModelRepository<TDbContext, TModel> ModelRepository;

        static ViewModelBase()
        {
            Repository = DefaultRepository<TDbContext, TModel, TView>.Instance;
            ModelRepository = DefaultModelRepository<TDbContext, TModel>.Instance;
        }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        [JsonIgnore]
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is clone.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is clone; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("isClone")]
        public bool IsClone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is lazy load.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is lazy load; otherwise, <c>false</c>.
        /// </value>
        [JsonIgnore]
        public bool IsLazyLoad { get; set; } = true;

        /// <summary>
        /// Gets or sets the list supported culture.
        /// </summary>
        /// <value>
        /// The list supported culture.
        /// </value>
        [JsonProperty("cultures")]
        public List<SupportedCulture> Cultures { get; set; }

        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>
        /// The mapper.
        /// </value>
        [JsonIgnore]
        public IMapper Mapper {
            get { return _mapper ?? (_mapper = this.CreateMapper()); }
            set => _mapper = value;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [JsonIgnore]
        public TModel Model {
            get {
                if (_model == null)
                {
                    Type classType = typeof(TModel);
                    ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
                    _model = (TModel)classConstructor.Invoke(new object[] { });
                }
                return _model;
            }
            set => _model = value;
        }

        /// <summary>
        /// Gets or sets the model mapper.
        /// </summary>
        /// <value>
        /// The model mapper.
        /// </value>
        [JsonIgnore]
        public IMapper ModelMapper {
            get { return _modelMapper ?? (_modelMapper = this.CreateModelMapper()); }
            set => _modelMapper = value;
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        [JsonProperty("priority")]
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the specificulture.
        /// </summary>
        /// <value>
        /// The specificulture.
        /// </value>
        [JsonProperty("specificulture")]
        public string Specificulture { get; set; }

        /// <summary>
        /// Creates the mapper.
        /// </summary>
        /// <returns></returns>
        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TModel, TView>().ReverseMap());
            var mapper = new Mapper(config);
            return mapper;
        }

        /// <summary>
        /// Creates the model mapper.
        /// </summary>
        /// <returns></returns>
        private IMapper CreateModelMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TModel, TModel>().ReverseMap());
            var mapper = new Mapper(config);
            return mapper;
        }

        [JsonIgnore]
        public List<string> Errors { get; set; } = new List<string>();

        [JsonIgnore]
        [JsonProperty("isValid")]
        public bool IsValid { get => isValid; set => isValid = value; }

        #endregion Properties

        #region Common

        /// <summary>
        /// Initializes the model.
        /// </summary>
        /// <returns></returns>
        public virtual TModel InitModel()
        {
            Type classType = typeof(TModel);
            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            TModel context = (TModel)classConstructor.Invoke(new object[] { });

            return context;
        }

        /// <summary>
        /// Parses the model.
        /// </summary>
        /// <returns></returns>
        public virtual TModel ParseModel(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            //AutoMapper.Mapper.Map<TView, TModel>((TView)this, Model);
            this.Model = InitModel();
            Mapper.Map<TView, TModel>((TView)this, Model);
            return this.Model;
        }

        /// <summary>
        /// Validates the specified context.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        public virtual void Validate(TDbContext _context, IDbContextTransaction _transaction)
        {
            var validateContext = new System.ComponentModel.DataAnnotations.ValidationContext(this, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            IsValid = Validator.TryValidateObject(this, validateContext, results);
            if (!IsValid)
            {
                Errors.AddRange(results.Select(e => e.ErrorMessage));
            }
        }

        #endregion Common

        #region Async

        /// <summary>
        /// Expands the view.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        public virtual Task<bool> ExpandViewAsync(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            var taskSource = new TaskCompletionSource<bool>();
            taskSource.SetResult(true);
            return taskSource.Task;
        }

        /// <summary>
        /// Initializes the view.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="isLazyLoad">if set to <c>true</c> [is lazy load].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public static async Task<TView> InitViewAsync(TModel model = null, bool isLazyLoad = true, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            Type classType = typeof(TView);

            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            if (model == null && classConstructor != null)
            {
                var view = (TView)classConstructor.Invoke(new object[] { });
                await view.ParseViewAsync(true, _context, _transaction);
                return view;
            }
            else
            {
                classConstructor = classType.GetConstructor(new Type[] { typeof(TModel), typeof(bool), typeof(TDbContext), typeof(IDbContextTransaction) });
                if (classConstructor != null)
                {
                    var view = (TView)classConstructor.Invoke(new object[] { model, isLazyLoad, _context, _transaction });
                    await view.ParseViewAsync(isLazyLoad, _context, _transaction);
                    return view;
                }
                else
                {
                    classConstructor = classType.GetConstructor(new Type[] { typeof(TModel), typeof(TDbContext), typeof(IDbContextTransaction) });
                    var view = (TView)classConstructor.Invoke(new object[] { model, _context, _transaction });
                    await view.ParseViewAsync(isLazyLoad, _context, _transaction);
                    return view;
                }
            }
        }

        /// <summary>
        /// Parses the view.
        /// </summary>
        /// <param name="isExpand">if set to <c>true</c> [is expand].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<TView> ParseViewAsync(bool isExpand = true, TDbContext _context = null, IDbContextTransaction _transaction = null
                                                    )
        {
            Mapper.Map<TModel, TView>(Model, (TView)this);
            if (isExpand)
            {
                UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
                try
                {
                    var expandResult = await ExpandViewAsync(context, transaction);
                    if (expandResult)
                    {
                        return this as TView;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex) 
                {
                    Repository.LogErrorMessage(ex);
                    if (isRoot)
                    {
                        //if current transaction is root transaction
                        transaction.Rollback();
                    }
                }
                finally
                {
                    if (isRoot)
                    {
                        //if current Context is Root
                        context.Dispose();
                    }
                }
            }
            return (TView)this;
        }

        /// <summary>
        /// Clones the asynchronous.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<List<TView>>> CloneAsync(TModel model, List<SupportedCulture> cloneCultures
            , TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<List<TView>> result = new RepositoryResponse<List<TView>>()
            {
                IsSucceed = true,
                Data = new List<TView>()
            };

            try
            {
                if (cloneCultures != null)
                {
                    foreach (var culture in cloneCultures)
                    {
                        string desSpecificulture = culture.Specificulture;

                        TView view = InitView();
                        view.Model = model;
                        view.ParseView(isExpand: false, _context: context, _transaction: transaction);
                        view.Specificulture = desSpecificulture;

                        bool isExist = Repository.CheckIsExists(view.ParseModel(context, transaction), _context: context, _transaction: transaction);

                        if (isExist)
                        {
                            result.IsSucceed = true;
                            result.Data.Add(view);
                        }
                        else
                        {
                            var cloneResult = await view.SaveModelAsync(false, context, transaction).ConfigureAwait(false);
                            if (cloneResult.IsSucceed)
                            {
                                var cloneSubResult = await CloneSubModelsAsync(cloneResult.Data, cloneCultures, context, transaction).ConfigureAwait(false);
                                if (!cloneSubResult.IsSucceed)
                                {
                                    cloneResult.Errors.AddRange(cloneSubResult.Errors);
                                    cloneResult.Exception = cloneSubResult.Exception;
                                }

                                result.IsSucceed = result.IsSucceed && cloneResult.IsSucceed && cloneSubResult.IsSucceed;
                                //result.Data.Add(cloneResult.Data);
                            }
                            else
                            {
                                result.IsSucceed = result.IsSucceed && cloneResult.IsSucceed;
                                result.Errors.AddRange(cloneResult.Errors);
                                result.Exception = cloneResult.Exception;
                            }
                        }
                        UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    }
                    return result;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex) 
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<List<TView>>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Clones the sub models asynchronous.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<bool>> CloneSubModelsAsync(TView parent, List<SupportedCulture> cloneCultures, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            var taskSource = new TaskCompletionSource<RepositoryResponse<bool>>();
            taskSource.SetResult(new RepositoryResponse<bool>() { IsSucceed = true, Data = true });
            return await taskSource.Task;
        }

        /// <summary>
        /// Removes the model asynchronous.
        /// </summary>
        /// <param name="isRemoveRelatedModels">if set to <c>true</c> [is remove related models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TModel>> RemoveModelAsync(bool isRemoveRelatedModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);

            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                ParseModel(_context, _transaction);
                if (isRemoveRelatedModels)
                {
                    var removeRelatedResult = await RemoveRelatedModelsAsync((TView)this, context, transaction).ConfigureAwait(false);
                    if (removeRelatedResult.IsSucceed)
                    {
                        result = await Repository.RemoveModelAsync(Model, context, transaction).ConfigureAwait(false);
                    }
                    else
                    {
                        result.IsSucceed = result.IsSucceed && removeRelatedResult.IsSucceed;
                        result.Errors.AddRange(removeRelatedResult.Errors);
                        result.Exception = removeRelatedResult.Exception;
                    }
                }
                else
                {
                    result = await Repository.RemoveModelAsync(Model, context, transaction).ConfigureAwait(false);
                }
                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                return result;
            }
            catch (Exception ex) 
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Removes the related models asynchronous.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<bool>> RemoveRelatedModelsAsync(TView view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            var taskSource = new TaskCompletionSource<RepositoryResponse<bool>>();
            taskSource.SetResult(new RepositoryResponse<bool>() { IsSucceed = true });
            return await taskSource.Task;
        }

        /// <summary>
        /// Saves the model asynchronous.
        /// </summary>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<TView>> SaveModelAsync(bool isSaveSubModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            Validate(context, transaction);
            if (IsValid)
            {
                try
                {
                    ParseModel(context, transaction);
                    result = await Repository.SaveModelAsync((TView)this, _context: context, _transaction: transaction).ConfigureAwait(false);

                    // Save sub Models
                    if (result.IsSucceed && isSaveSubModels)
                    {
                        var saveResult = await SaveSubModelsAsync(Model, context, transaction).ConfigureAwait(false);
                        if (!saveResult.IsSucceed)
                        {
                            result.Errors.AddRange(saveResult.Errors);
                            result.Exception = saveResult.Exception;
                        }
                        result.IsSucceed = result.IsSucceed && saveResult.IsSucceed;
                    }

                    // Clone Models
                    if (result.IsSucceed && IsClone)// && isRoot)
                    {
                        var cloneCultures = Cultures.Where(c => c.Specificulture != Specificulture && c.IsSupported).ToList();
                        var cloneResult = await CloneAsync(Model, cloneCultures, _context: context, _transaction: transaction).ConfigureAwait(false);
                        if (!cloneResult.IsSucceed)
                        {
                            result.Errors.AddRange(cloneResult.Errors);
                            result.Exception = cloneResult.Exception;
                        }
                        result.IsSucceed = result.IsSucceed && cloneResult.IsSucceed;
                    }

                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    return result;
                }
                catch (Exception ex) 
                {
                    return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
                }
                finally
                {
                    if (isRoot)
                    {
                        //if current Context is Root
                        context.Dispose();
                    }
                }
            }
            else
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
                return new RepositoryResponse<TView>()
                {
                    IsSucceed = false,
                    Data = null,
                    Errors = Errors
                };
            }
        }

        /// <summary>
        /// Saves the sub models asynchronous.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual async Task<RepositoryResponse<bool>> SaveSubModelsAsync(TModel parent, TDbContext _context, IDbContextTransaction _transaction)
        {
            var taskSource = new TaskCompletionSource<RepositoryResponse<bool>>();
            taskSource.SetResult(new RepositoryResponse<bool>() { IsSucceed = true });
            return await taskSource.Task;
        }

        #endregion Async

        #region Sync

        /// <summary>
        /// Initializes the view.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="isLazyLoad">if set to <c>true</c> [is lazy load].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual TView InitView(TModel model = null, bool isLazyLoad = true, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            Type classType = typeof(TView);

            ConstructorInfo classConstructor = classType.GetConstructor(new Type[] { });
            if (model == null && classConstructor != null)
            {
                return (TView)classConstructor.Invoke(new object[] { });
            }
            else
            {
                classConstructor = classType.GetConstructor(new Type[] { typeof(TModel), typeof(bool), typeof(TDbContext), typeof(IDbContextTransaction) });
                if (classConstructor != null)
                {
                    return (TView)classConstructor.Invoke(new object[] { model, isLazyLoad, _context, _transaction });
                }
                else
                {
                    classConstructor = classType.GetConstructor(new Type[] { typeof(TModel), typeof(TDbContext), typeof(IDbContextTransaction) });
                    return (TView)classConstructor.Invoke(new object[] { model, _context, _transaction });
                }
            }
        }

        /// <summary>
        /// Parses the view.
        /// </summary>
        /// <param name="isExpand">if set to <c>true</c> [is expand].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual TView ParseView(bool isExpand = true, TDbContext _context = null, IDbContextTransaction _transaction = null
                                                    )
        {
            //AutoMapper.Mapper.Map<TModel, TView>(Model, (TView)this);
            Mapper.Map<TModel, TView>(Model, (TView)this);
            if (isExpand)
            {
                UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
                try
                {
                    ExpandView(context, transaction);
                }
                catch (Exception ex) 
                {
                    Repository.LogErrorMessage(ex);
                    if (isRoot)
                    {
                        //if current transaction is root transaction
                        transaction.Rollback();
                    }
                }
                finally
                {
                    if (isRoot)
                    {
                        //if current Context is Root
                        context.Dispose();
                    }
                }
            }
            return (TView)this;
        }

        /// <summary>
        /// Expands the view.
        /// </summary>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        public virtual void ExpandView(TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
        }

        /// <summary>
        /// Clones the specified clone cultures.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<List<TView>> Clone(TModel model, List<SupportedCulture> cloneCultures, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<List<TView>> result = new RepositoryResponse<List<TView>>()
            {
                IsSucceed = true,
                Data = new List<TView>()
            };

            try
            {
                if (cloneCultures != null)
                {
                    foreach (var culture in cloneCultures)
                    {
                        string desSpecificulture = culture.Specificulture;

                        TView view = InitView();
                        view.Model = model;
                        view.ParseView(isExpand: false, _context: context, _transaction: transaction);
                        view.Specificulture = desSpecificulture;

                        bool isExist = Repository.CheckIsExists(view.ParseModel(context, transaction), _context: context, _transaction: transaction);

                        if (isExist)
                        {
                            result.IsSucceed = true;
                            result.Data.Add(view);
                        }
                        else
                        {
                            var cloneResult = view.SaveModel(false, context, transaction);
                            if (cloneResult.IsSucceed)
                            {
                                var cloneSubResult = CloneSubModels(cloneResult.Data, cloneCultures, context, transaction);
                                if (!cloneSubResult.IsSucceed)
                                {
                                    cloneResult.Errors.AddRange(cloneSubResult.Errors);
                                    cloneResult.Exception = cloneSubResult.Exception;
                                }

                                result.IsSucceed = result.IsSucceed && cloneResult.IsSucceed && cloneSubResult.IsSucceed;
                                //result.Data.Add(cloneResult.Data);
                            }
                            else
                            {
                                result.IsSucceed = result.IsSucceed && cloneResult.IsSucceed;
                                result.Errors.AddRange(cloneResult.Errors);
                                result.Exception = cloneResult.Exception;
                            }
                        }
                        UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    }
                    return result;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex) 
            {
                result.IsSucceed = false;
                result.Exception = ex;
                return result;
            }
            finally
            {
                if (isRoot)
                {
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Clones the sub models.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="cloneCultures">The clone cultures.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<bool> CloneSubModels(TView parent, List<SupportedCulture> cloneCultures, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            return new RepositoryResponse<bool>() { IsSucceed = true };
        }

        /// <summary>
        /// Removes the model.
        /// </summary>
        /// <param name="isRemoveRelatedModels">if set to <c>true</c> [is remove related models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TModel> RemoveModel(bool isRemoveRelatedModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TModel> result = new RepositoryResponse<TModel>() { IsSucceed = true };
            try
            {
                ParseModel(_context, _transaction);
                if (isRemoveRelatedModels)
                {
                    var removeRelatedResult = RemoveRelatedModels((TView)this, context, transaction);
                    if (removeRelatedResult.IsSucceed)
                    {
                        result = Repository.RemoveModel(Model, context, transaction);
                    }
                    else
                    {
                        result.IsSucceed = result.IsSucceed && removeRelatedResult.IsSucceed;
                        result.Errors.AddRange(removeRelatedResult.Errors);
                        result.Exception = removeRelatedResult.Exception;
                    }
                }
                else
                {
                    result = Repository.RemoveModel(Model, context, transaction);
                }

                UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                return result;
            }
            catch (Exception ex) 
            {
                return UnitOfWorkHelper<TDbContext>.HandleException<TModel>(ex, isRoot, transaction);
            }
            finally
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
            }
        }

        /// <summary>
        /// Removes the related models.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<bool> RemoveRelatedModels(TView view, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            return new RepositoryResponse<bool>() { IsSucceed = true };
        }

        /// <summary>
        /// Saves the model.
        /// </summary>
        /// <param name="isSaveSubModels">if set to <c>true</c> [is save sub models].</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<TView> SaveModel(bool isSaveSubModels = false, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            UnitOfWorkHelper<TDbContext>.InitTransaction(_context, _transaction, out TDbContext context, out IDbContextTransaction transaction, out bool isRoot);
            RepositoryResponse<TView> result = new RepositoryResponse<TView>() { IsSucceed = true };
            Validate(context, transaction);
            if (IsValid)
            {
                try
                {
                    ParseModel(context, transaction);
                    result = Repository.SaveModel((TView)this, _context: context, _transaction: transaction);

                    // Save sub Models
                    if (result.IsSucceed && isSaveSubModels)
                    {
                        var saveResult = SaveSubModels(Model, context, transaction);
                        if (!saveResult.IsSucceed)
                        {
                            result.Errors.AddRange(saveResult.Errors);
                            result.Exception = saveResult.Exception;
                        }
                        result.IsSucceed = result.IsSucceed && saveResult.IsSucceed;
                    }

                    // Clone Models
                    if (result.IsSucceed && IsClone && isRoot)
                    {
                        var cloneCultures = Cultures.Where(c => c.Specificulture != Specificulture && c.IsSupported).ToList();
                        var cloneResult = Clone(Model, cloneCultures, _context: context, _transaction: transaction);
                        if (!cloneResult.IsSucceed)
                        {
                            result.Errors.AddRange(cloneResult.Errors);
                            result.Exception = cloneResult.Exception;
                        }
                        result.IsSucceed = result.IsSucceed && cloneResult.IsSucceed;
                    }

                    UnitOfWorkHelper<TDbContext>.HandleTransaction(result.IsSucceed, isRoot, transaction);
                    return result;
                }
                catch (Exception ex) 
                {
                    return UnitOfWorkHelper<TDbContext>.HandleException<TView>(ex, isRoot, transaction);
                }
                finally
                {
                    if (isRoot)
                    {
                        //if current Context is Root
                        context.Dispose();
                    }
                }
            }
            else
            {
                if (isRoot)
                {
                    //if current Context is Root
                    context.Dispose();
                }
                return new RepositoryResponse<TView>()
                {
                    IsSucceed = false,
                    Data = null,
                    Errors = Errors
                };
            }
        }

        /// <summary>
        /// Saves the sub models.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        /// <returns></returns>
        public virtual RepositoryResponse<bool> SaveSubModels(TModel parent, TDbContext _context, IDbContextTransaction _transaction)
        {
            return new RepositoryResponse<bool>() { IsSucceed = true };
        }

        #endregion Sync

        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{TDbContext, TModel, TView}"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="_context">The context.</param>
        /// <param name="_transaction">The transaction.</param>
        protected ViewModelBase(TModel model, TDbContext _context = null, IDbContextTransaction _transaction = null)
        {
            this.Model = model;
            ParseView(_context: _context, _transaction: _transaction);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase{TDbContext, TModel, TView}"/> class.
        /// </summary>
        protected ViewModelBase()
        {
            this.Model = InitModel();
            ParseView(isExpand: false);
        }

        #endregion Contructor
    }
}
