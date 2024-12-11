using DIL.Interfaces;
using System;
using System.Collections.Generic;

namespace DIL.Middlewares
{
    /// <summary>
    /// The core middleware processor that manages the execution of middleware classes.
    /// </summary>
    public class MiddlewareCore
    {
        private readonly List<IMiddleware> _middlewares = new();

        /// <summary>
        /// Adds a middleware to the processing pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        public void AddMiddleware(IMiddleware middleware)
        {
            _middlewares.Add(middleware);
        }

        /// <summary>
        /// Processes the input through the middleware pipeline.
        /// </summary>
        /// <param name="input">The input to process.</param>
        /// <returns>The processed output.</returns>
        public string Process(string input)
        {
            string result = input;

            foreach (var middleware in _middlewares)
            {
                result = middleware.Process(result);
            }

            return result;
        }
    }
}
