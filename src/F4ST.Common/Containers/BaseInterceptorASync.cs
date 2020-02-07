using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using F4ST.Common.Tools;

namespace F4ST.Common.Containers
{
    public abstract class BaseInterceptorASync : IInterceptor
    {
        protected abstract bool BeforeRunMethod(IInvocation invocation);

        protected abstract void AfterRunMethod(IInvocation invocation);

        protected abstract void OnException(IInvocation invocation, Exception ex);

        public void Intercept(IInvocation invocation)
        {
            try
            {
                var cont = BeforeRunMethod(invocation);

                if (!cont)
                {
                    return;
                }

                if (IsAsyncMethod(invocation.Method))
                {
                    InterceptAsync(invocation);
                }
                else
                {
                    InterceptSync(invocation);
                }

            }
            catch (Exception ex)
            {
                OnException(invocation, ex);
                throw;
            }
            finally
            {
            }
        }

        private void InterceptAsync(IInvocation invocation)
        {
            //Calling the actual method, but execution has not been finished yet
            invocation.Proceed();

            var returnType = invocation.ReturnValue.GetType();
            if (returnType != typeof(Task) &&
                !(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) &&
                !(returnType.DeclaringType==typeof(AsyncTaskMethodBuilder<>)))
            {
                LogExecutionTime(invocation);
                return;
            }

            var task = (Task)invocation.ReturnValue;
            AsyncHelpers.RunSync(() => task);


            if (returnType == typeof(Task))
            {
                LogExecutionTime(invocation);
                return;
            }

            var result = task.GetType().GetProperty("Result")?.GetValue(task, null);
            invocation.ReturnValue = result;

            if (invocation.Method.ReturnType == typeof(Task))
            {
                invocation.ReturnValue = GetTaskResult();
                LogExecutionTime(invocation);
                return;
            }
            var m = GetType().GetMethod("GetGenericResult");
            var g = m?.MakeGenericMethod(invocation.Method.ReturnType.GenericTypeArguments[0]);
            invocation.ReturnValue = g?.Invoke(this, new[] { invocation.ReturnValue });
            LogExecutionTime(invocation);


            //Wait task execution and modify return value
            /*if (invocation.Method.ReturnType == typeof(Task))
            {
                invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally(
                    (Task)invocation.ReturnValue,
                    null,
                    ex => { LogExecutionTime(invocation, ex); });
            }
            else //Task<TResult>
            {
                invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                    invocation.Method.ReturnType.GenericTypeArguments[0],
                    invocation.ReturnValue,
                    null,
                    ex => { LogExecutionTime(invocation, ex); });
            }*/
        }

        public async Task<TT> GetGenericResult<TT>(TT result)
        {
            return result;
        }

        public async Task GetTaskResult()
        {

        }

        private void InterceptSync(IInvocation invocation)
        {
            //Executing the actual method
            invocation.Proceed();

            AfterRunMethod(invocation);
        }

        private static bool IsAsyncMethod(MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            );
        }

        private void LogExecutionTime(IInvocation invocation, Exception exception = null)
        {
            if (exception != null)
            {
                OnException(invocation, exception);
            }

            AfterRunMethod(invocation);
        }
    }
}