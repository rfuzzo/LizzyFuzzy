using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FuzzoBot.Utility
{
    public static class Ensure
    {
        [return: NotNull]
        public static T Null<T>(
            this T obj,
            Action? action = default
        )
        {
            action?.Invoke();
            return obj ?? throw new Exception();
        }

        /// <summary>
        ///     Ensures an object is not null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [return: NotNull]
        public static T NotNull<T>(
            [NotNull] this T? obj,
            string? message = default,
            [CallerArgumentExpression("obj")] string? parameterName = default)
            where T : class
        {
            return obj ?? throw new ArgumentNullException(parameterName, message);
        }

        [return: NotNull]
        public static T NotNull<T>(
            [NotNull] this T? obj,
            Action action,
            string? message = default,
            [CallerArgumentExpression("obj")] string? parameterName = default
        )
            where T : class
        {
            if (obj != null) return obj;

            action();
            throw new ArgumentNullException(parameterName, message);
        }

        /// <summary>
        ///     Ensures an string is not null or empty
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullOrEmptyException"></exception>
        public static string NotNullOrEmpty(
            [NotNull] this string? obj,
            string? message = default,
            [CallerArgumentExpression("obj")] string? parameterName = default)
        {
            return string.IsNullOrEmpty(obj) ? throw new ArgumentNullException(parameterName, message) : obj;
        }

        public static bool IsNotNullOrEmpty(string str = "", Action? action = default)
        {
            switch (string.IsNullOrEmpty(str))
            {
                case true:
                    action?.Invoke();
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNotNull<T>(T? obj, Action? action = default)
        {
            switch (obj is null)
            {
                case false:
                    return true;
                default:
                    action?.Invoke();
                    return false;
            }
        }
    }
}