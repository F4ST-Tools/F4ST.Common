using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace F4ST.Common.Containers
{
    public class LoggingMethodInterceptor : BaseInterceptorASync
    {
        private Stopwatch _stopwatch;
        protected override bool BeforeRunMethod(IInvocation invocation)
        {
            _stopwatch = Stopwatch.StartNew();
            Debug.WriteLine($"Start method '{invocation.Method.Name}'...");
            /*using (var log = new QLogger())
            {
                log.Trace("",$"Start method '{invocation.Method.Name}'...")
            }*/

            return true;
        }

        protected override void AfterRunMethod(IInvocation invocation)
        {
            //todo: this variable not valid, must not use variable
            _stopwatch.Stop();

            Debug.WriteLine(
                $"Method '{invocation.MethodInvocationTarget.Name}' executed in {_stopwatch.Elapsed.TotalMilliseconds:0.000} milliseconds.");

        }

        protected override void OnException(IInvocation invocation, Exception ex)
        {
            Debug.WriteLine($"Error for '{invocation.Method.Name}' = {ex.Message}");
        }
    }
}