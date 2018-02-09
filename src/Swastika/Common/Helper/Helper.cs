﻿using Swastika.Domain.Core.ViewModels;
using System.Collections.Generic;

namespace Swastika.UI.Base
{
    /// <summary>
    /// Api Helper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiHelper<T>
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="data">The data.</param>
        /// <param name="responseKey">The response key.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public static ApiResult<T> GetResult(int status, T data, string responseKey
            , List<string> errors)
        {
            ApiResult<T> result = new ApiResult<T>()
            {
                Status = status,
                ResponseKey = responseKey,
                Data = data,
                Errors = errors,
            };

            return result;
        }
    }
}