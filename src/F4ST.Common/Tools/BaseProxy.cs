using System;
using System.Reflection;
using System.Threading.Tasks;

namespace F4St.Common.Tools
{
    public abstract class BaseProxy<T, TP> : DispatchProxy
        where T : class
        where TP : BaseProxy<T, TP>
    {
        private T _decorated;
        protected object Result;

        protected static T CreateProxy(T decorated)
        {
            var proxy = Create<T, TP>();
            (proxy as BaseProxy<T, TP>)?.SetParameters(decorated);
            return proxy;
        }

        private void SetParameters(T decorated)
        {
            _decorated = decorated;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                var cont = AsyncHelpers.RunSync(() => BeforeRunMethod(targetMethod, args));
                if (!cont)
                    return Result;

                if (!IsAsyncMethod(targetMethod))
                {
                    InterceptSync(targetMethod, args);
                }
                else
                {
                    InterceptAsync(targetMethod, args);
                }

                return Result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                var raise = AsyncHelpers.RunSync(() => OnException(targetMethod, args, ex.InnerException ?? ex));

                if (raise)
                {
                    throw ex.InnerException ?? ex;
                }
            }

            return Result;
        }

        private void InterceptAsync(MethodInfo targetMethod, object[] args)
        {
            //Calling the actual method, but execution has not been finished yet
            Result = targetMethod.Invoke(_decorated, args);

            if (targetMethod.ReturnType == typeof(Task))
            {
                return;
            }

            var task = (Task) Result;

            AsyncHelpers.RunSync(() => task);
            Result = task.GetType().GetProperty("Result")?.GetValue(task, null);
            AsyncHelpers.RunSync(() => AfterRunMethod(targetMethod, args));
        }

        private void InterceptSync(MethodInfo targetMethod, object[] args)
        {
            //Executing the actual method
            Result = targetMethod.Invoke(_decorated, args);

            AsyncHelpers.RunSync(() => AfterRunMethod(targetMethod, args));
        }

        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            );
        }


        /// <summary>
        /// این متود قبل از اجرای متود اصلی اجرا میشود
        /// </summary>
        /// <param name="targetMethod">متود اجرا شونده</param>
        /// <param name="args">پارامترها</param>
        /// <returns>در صورتی که مقدار True بازگردد ادامه اجرای متود جلو میرود</returns>
        protected virtual async Task<bool> BeforeRunMethod(MethodInfo targetMethod, object[] args)
        {
            return true;
        }

        /// <summary>
        /// این متود بعد از اجرای متود اصلی اجرا میشود
        /// </summary>
        /// <param name="targetMethod">متود اجرا شونده</param>
        /// <param name="args">پارامترها</param>
        protected virtual async Task AfterRunMethod(MethodInfo targetMethod, object[] args)
        {
        }

        /// <summary>
        /// در صورت بروز خطا این متود اجرا میشود
        /// </summary>
        /// <param name="targetMethod">متود اجرا شونده</param>
        /// <param name="args">پارامترها</param>
        /// <param name="exception">خطا</param>
        /// <returns>در صورتی که True بازگردانده شود خطا به مرحله قبلی ارسال میشود</returns>
        protected virtual async Task<bool> OnException(MethodInfo targetMethod, object[] args, Exception exception)
        {
            return true;
        }
    }
}