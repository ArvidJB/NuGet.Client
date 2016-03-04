using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NuGet.VsExtension.Test
{
    public static class AssertExtensions
    {
        public static async Task<T> RecordExceptionAsync<T>(Func<Task> task) where T : Exception
        {
            try
            {
                await task();
                Assert.Fail($"Expected exception {typeof(T).FullName}");
                return null;
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(T));
                return (T)ex;
            }
        }
    }
}
